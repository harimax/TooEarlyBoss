using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class TitanBossController : MonoBehaviour, IBossController
{
    enum TitanBossState
    {
        Idle,// 待機中（プレイヤーを見つめるだけ） 
        AttackSprint,//突進
        AttackThrow,//岩を投げる遠距離攻撃
        AttackJump,//ジャンプして地面に衝撃波
        CoolDown,// cooldown状態
        Dead//死亡時（今後追加）
    }
    [Header("Reference")]
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    CancellationTokenSource loopCts;
    [SerializeField] private float playerDistance;
    [SerializeField] private float faceTime = 0.5f;      //  向き直り（ため）時間
    [SerializeField] private float minSprintTime = 1.0f; //  突進の最大継続時間（秒）
    [SerializeField] private float maxSprintTime = 1.5f; //  突進の最大継続時間（秒）
    [SerializeField] private float restTime = 3.5f;      //  休憩時間（秒）
    [SerializeField] private Collider SprintColider; // 突進攻撃の当たり判定
    [SerializeField] private float turnSpeed = 10f;         // 向き合わせスピード
    [SerializeField] private int jumpAttackProbability = 50; // ジャンプ攻撃の選択確率（％）

    [SerializeField] private GameObject jmupAttackEffect; // ジャンプ攻撃エフェクト

    [Header("Reference")]
    [SerializeField] private float shotSpeed;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform ShotPos;
    private Vector3 childPosition;
    public string targetTag = "Target";

    private TitanBossState currentState = TitanBossState.CoolDown;//最初はIdle状態にする
    private bool _isPaused = false;
    private bool isjumpAttack = false;
    private bool _cooling;

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = gameObject.GetComponent<NavMeshAgent>();
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPaused) return;

        if (currentState == TitanBossState.CoolDown)
        {
            Cooldown().Forget();
            //クールダウン処理をする
        }
        //近ければジャンプしてからの遠距離攻撃
        else if (currentState == TitanBossState.Idle && PlayerDistaneceCheck() < playerDistance)
        {
            //ジャンプ攻撃
            if (isjumpAttack == true)
            {
                animator.SetTrigger("AttackJump");
                currentState = TitanBossState.AttackJump;
            }
            //遠距離攻撃の処理をする
            else
            {
                FacePlayerForSeconds(faceTime, turnSpeed).Forget();
                animator.SetTrigger("AttackThrow");
                currentState = TitanBossState.AttackThrow;
            }
        }
        //遠ければ突進攻撃
        else if (currentState == TitanBossState.Idle && PlayerDistaneceCheck() > playerDistance)
        {
            //ジャンプ攻撃
            if (isjumpAttack == true)
            {
                animator.SetTrigger("AttackJump");
                currentState = TitanBossState.AttackJump;
            }
            //遠距離攻撃の処理をする
            else
            {
                loopCts?.Cancel();
                loopCts = new CancellationTokenSource();
                currentState = TitanBossState.AttackSprint;
                Sprint(loopCts.Token).Forget();
            }
        }
    }
    public void PauseBoss()
    {
        _isPaused = true;
    }
    /// <summary>
    /// ボスの動きを再開する
    /// </summary>
    public void ResumeBoss()
    {
        _isPaused = false;
        if (animator != null) animator.enabled = true;
        // タイマー初期化
    }
    private async UniTaskVoid Sprint(CancellationToken ct)
    {
        if (!player) return;

        while (!ct.IsCancellationRequested)
        {
            // --- 突進準備 ---
            //向く（ため）
            await FacePlayerForSeconds(faceTime, turnSpeed, ct);
            if (ct.IsCancellationRequested) return;

            animator.SetTrigger("AttackSprint");

            // --- PreDashMotion が終わり、dashAttack に入るまで待つ ---
            AnimatorStateInfo state;
            do
            {
                await UniTask.Yield();
                state = animator.GetCurrentAnimatorStateInfo(0);
            }
            while (!state.IsName("dashAttack") && !ct.IsCancellationRequested);

            if (ct.IsCancellationRequested) return;

            float SprintTime = UnityEngine.Random.Range(minSprintTime, maxSprintTime);
            float elapsed = 0f;

            SprintColider.enabled = true;
            Debug.Log(SprintTime);
            while (elapsed < SprintTime)
            {
                elapsed += Time.deltaTime;
                await UniTask.Yield();
                Debug.Log("突進中", this);
            }
            Debug.Log("突進終了", this);
            animator.SetTrigger("FinishSprint");
            await UniTask.Delay(TimeSpan.FromSeconds(restTime), cancellationToken: ct);
            break;
        }
        Debug.Log("BattleLoop: ct.Cancelled で終了");
        currentState = TitanBossState.CoolDown;
    }
    public void FacePlayerOnlyYaw(float slerpSpeed)
    {
        if (!player) return;
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        var target = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * slerpSpeed);
    }
    // 一定時間プレイヤーの方向に向き続ける（Yawのみ）
    private async UniTask FacePlayerForSeconds(float duration, float turnSpeed, CancellationToken ct = default)
    {
        float t = 0f;

        while (t < duration && !ct.IsCancellationRequested)
        {
            FacePlayerOnlyYaw(turnSpeed);
            t += Time.deltaTime;
            await UniTask.Yield(); // フレームごとに更新
        }
    }
    //攻撃後のクールダウン処理
    private async UniTask Cooldown()
    {

        if (_cooling) return;
        _cooling = true;
        try
        {
            Debug.Log("クールダウン中...");
            await UniTask.Delay(3000, cancellationToken: this.GetCancellationTokenOnDestroy());
            animator.SetTrigger("FinishBreakTime");
            currentState = TitanBossState.Idle;
        }
        finally
        {
            _cooling = false;
            SelectJumpAttack();
        }
    }
    //プレイヤーとの距離を計算
    private float PlayerDistaneceCheck()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        return distance;
    }
    //攻撃選択ロジック
    private void SelectJumpAttack()
    {
        int roll = UnityEngine.Random.Range(0, 100);
        if (roll < 20)
        {
            isjumpAttack = true;
        }
        else
        {
            isjumpAttack = false;
        }
    }

    public void DeadTrigger()
    {
        currentState = TitanBossState.Dead;
        SprintColider.enabled = false;
        animator.SetTrigger("Dead");
    }
    public void OnSprintStart()
    {
    }
    /// <summary>
    /// Idle状態へ戻す
    /// </summary>
    public void OnAttackEnd()
    {
        currentState = TitanBossState.CoolDown;
        SprintColider.enabled = false;
    }
    public void StartJumpAttackEffect()
    {
        Debug.Log("ジャンプ攻撃エフェクト開始", this);
        jmupAttackEffect.SetActive(true);
        // エフェクト開始処理
    }
    public void StopJumpAttackEffect()
    {
        Debug.Log("ジャンプ攻撃エフェクト停止", this);
        jmupAttackEffect.SetActive(false);
        // エフェクト停止処理
    }
    public void OnRockThrowEvent()
    {
        // 発射位置
        Vector3 origin = ShotPos.transform.position;
        // プレイヤー（ターゲット）の位置
        Vector3 target = Targetpos();
        // 発射地点 → プレイヤー へのベクトル
        Vector3 direction = (target - origin).normalized;
        var shot = Instantiate(rockPrefab.gameObject, ShotPos.transform.position, gameObject.transform.rotation);
        shot.GetComponent<Rigidbody>().linearVelocity = direction * shotSpeed;
    }

    //プレイヤーの位置を取得する------------------------------------------------------
    private Vector3 Targetpos()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform parentObject = player.transform;
        foreach (Transform child in parentObject)
        {
            if (child.CompareTag(targetTag))
            {
                // ターゲットの子オブジェクトの位置を取得
                childPosition = child.position;
                break;
            }
        }
        return childPosition;
    }
}
