using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Game.Boss;

public class DragonBossController : MonoBehaviour, IBossController
{
    private enum DragonState
    {
        Idle,
        AttackCrow,
        Avoid,
        AttackFire,
        AttackSprint,
        CoolDown,
        Dead
    }
    private bool _isPaused = false;
    private Transform player;
    private Animator animator;
    [Header("Behaviour")]
    [SerializeField] private float playerDistance = 25f;
    [SerializeField] private float crowDistance = 5f;
    [SerializeField] private float faceTime = 0.5f;      //  向き直り（ため）時間
    [SerializeField] private float maxSprintTime = 1.0f; //  突進の最大継続時間（秒）
    [SerializeField] private float restTime = 3.5f;      //  休憩時間（秒）
    [SerializeField] private float turnSpeed = 10f;         // 向き合わせスピード
    // ステージ中央の基準位置（空なら復帰処理は行わない）
    [SerializeField] private Transform stageCenter;
    // この距離を超えて中央から離れたら、Jump中に中央へ戻す
    [SerializeField] private float returnToCenterDistance = 25f;
    [SerializeField] private GameObject rangeAttackPrefab;
    [SerializeField] private GameObject closeAttackPrefab;
    [SerializeField] private Collider chaseAttackcollider;
    private int attackChoicePercent;
    private int closeAttackType;
    private bool _cooling;
    private bool isRangeAttack;
    CancellationTokenSource sprintCts;

    DragonState currentState = DragonState.CoolDown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void OnDestroy()
    {
        sprintCts?.Cancel();
        sprintCts?.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPaused || currentState == DragonState.Dead) return;
        //現在の状態でそれぞれの処理に分岐する
        switch (currentState)
        {
            case DragonState.CoolDown:
                HandleCoolDownState();
                break;
            case DragonState.Idle:
                HandleIdleState();
                break;

            // Attack中・回避中・死亡中は基本的にアニメーション/コルーチン側に任せる
            case DragonState.AttackCrow:
            case DragonState.Avoid:
            case DragonState.AttackFire:
            case DragonState.AttackSprint:
            case DragonState.Dead:
            default:
                break;
        }
    }
    /// <summary>
    /// Idle 状態時のフレーム処理
    /// 距離で「近距離ロジック」と「中〜遠距離ロジック」に振り分ける
    /// </summary>
    private void HandleIdleState()
    {
        float distance = PlayerDistanceCheck();

        if (distance < crowDistance)
        {
            HandleCloseRangeBehaviour();
        }
        else
        {
            HandleLongRangeBehaviour();
        }
    }
    /// <summary>
    /// 近距離時の行動ロジック（元の「近距離時の大きな if/else」をここに集約）
    /// </summary>
    private void HandleCloseRangeBehaviour()
    {
        // まずプレイヤー方向に向き直る（ため）
        FacePlayerForSeconds(faceTime, turnSpeed).Forget();

        // 近距離攻撃 or 回避をランダムで決定
        closeAttackType = UnityEngine.Random.Range(0, 2);

        if (closeAttackType == 0)
        {
            // 遠距離攻撃フラグが立っているときは、
            // 近距離でもブレスを選択する分岐
            if (isRangeAttack)
            {
                FacePlayerForSeconds(faceTime, turnSpeed).Forget();
                animator.SetTrigger("IsRangeAttack");
                currentState = DragonState.AttackFire;
            }
            // 通常の近距離攻撃
            else
            {
                FacePlayerForSeconds(faceTime, turnSpeed).Forget();
                animator.SetTrigger("IsCrowAttack");
                currentState = DragonState.AttackCrow;
            }
        }
        else
        {
            // 回避行動
            if (ShouldReturnToCenter())
            {
                animator.SetTrigger("IsAvoidJump");
                currentState = DragonState.Avoid;
                return;
            }
            animator.SetTrigger("IsAvoid");
            currentState = DragonState.Avoid;
        }
    }

    /// <summary>
    /// 中〜遠距離時の行動ロジック（元の「遠距離攻撃」「突進攻撃」をここに集約）
    /// </summary>
    private void HandleLongRangeBehaviour()
    {
        // isRangeAttack が true なら遠距離ブレス、false なら突進
        if (isRangeAttack)
        {
            FacePlayerForSeconds(faceTime, turnSpeed).Forget();
            animator.SetTrigger("IsRangeAttack");
            currentState = DragonState.AttackFire;
        }
        else
        {
            sprintCts?.Cancel();
            sprintCts = new CancellationTokenSource();
            currentState = DragonState.AttackSprint;
            Sprint(sprintCts.Token).Forget();
        }
    }
    //=========================
    // ここから下は元のまま
    // PauseBoss / ResumeBoss / Sprint / Cooldown / SelectAttack など
    //=========================
    public void PauseBoss()
    {
        // Update 系を止める
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
    //プレイヤーとの距離を測る
    private float PlayerDistanceCheck()
    {
        return BossUtilities.DistanceToTarget(transform, player);
    }
    private async UniTaskVoid Sprint(CancellationToken ct)
    {
        if (!player) return;

        while (!ct.IsCancellationRequested)
        {
            //向く（ため）
            FacePlayerForSeconds(faceTime, turnSpeed, ct).Forget();

            if (ct.IsCancellationRequested) break;

            float SprintTime = UnityEngine.Random.Range(0, maxSprintTime);

            // --- 突進準備 ---
            float elapsed = 0f;
            animator.SetTrigger("IsSprint");
            chaseAttackcollider.enabled = true;
            while (elapsed < SprintTime && !ct.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;
                await UniTask.Yield();

            }
            Debug.Log("突進終了", this);
            animator.SetTrigger("FinishSprint");
            await UniTask.Delay(TimeSpan.FromSeconds(restTime), cancellationToken: ct);
            chaseAttackcollider.enabled = false;
            //ダッシュ終了後に攻撃範囲内なら攻撃
            if (PlayerDistanceCheck() < crowDistance)
            {
                FacePlayerForSeconds(faceTime, turnSpeed).Forget();
                animator.SetTrigger("IsCrowAttack");
                currentState = DragonState.AttackCrow;
                break;
            }
            //攻撃範囲外ならIdleへ戻る
            else
            {
                animator.SetTrigger("FinishAttack");
                currentState = DragonState.CoolDown;
                break;
            }
        }
        Debug.Log("BattleLoop: ct.Cancelled で終了");
    }
    /// <summary>
    /// クールダウン状態のフレーム処理
    /// </summary>
    private void HandleCoolDownState()
    {
        // Cooldown 内部で _cooling フラグを見てくれるので連打しても安全
        Cooldown().Forget();
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
            currentState = DragonState.Idle;
        }
        finally
        {
            _cooling = false;
            SelectAttack();
        }
    }

    public void FacePlayerOnlyYaw(float slerpSpeed)
    {
        BossUtilities.FaceTargetYaw(transform, player, slerpSpeed);
    }

    public bool ShouldReturnToCenter()
    {
        // stageCenter が未設定なら復帰判定は無効
        if (!stageCenter) return false;

        // 現在位置と中央の距離が閾値を超えた時だけ true
        return Vector3.Distance(transform.position, stageCenter.position) > returnToCenterDistance;
    }

    // stageCenter が未設定の場合でも null 参照を避けるため現在位置を返す
    public Vector3 StageCenterPosition => stageCenter ? stageCenter.position : transform.position;
    //攻撃選択ロジック
    private void SelectAttack()
    {
        //プレイヤーとの位置が遠ければ遠距離攻撃率が高くなる
        if (PlayerDistanceCheck() > playerDistance)
        {
            attackChoicePercent = 60;
        }
        else
        {
            attackChoicePercent = 40;
        }
        int roll = UnityEngine.Random.Range(0, 100);
        if (roll < attackChoicePercent)
        { isRangeAttack = true; }
        else
        { isRangeAttack = false; }
    }
    // 一定時間プレイヤーの方向に向き続ける（Yawのみ）
    private async UniTask FacePlayerForSeconds(float duration, float turnSpeed, CancellationToken ct = default)
    {
        float t = 0f;

        while (t < duration && !ct.IsCancellationRequested)
        {
            BossUtilities.FaceTargetYaw(transform, player, turnSpeed);
            t += Time.deltaTime;
            await UniTask.Yield(); // フレームごとに更新
        }
    }

    //DragonAttackBehaviourから呼ばれる
    public void OnAttackEnd()
    {
        currentState = DragonState.CoolDown;
    }
    public void OnRangeAttackEvent()
    {
        Debug.Log("遠距離攻撃発動", this);
        rangeAttackPrefab.SetActive(true);
    }
    public void OnRangeAttackEndEvent()
    {
        Debug.Log("遠距離攻撃終了", this);
        rangeAttackPrefab.SetActive(false);
    }
    public void OnCloseAttackEvent()
    {
        Debug.Log("近距離攻撃発動", this);
        closeAttackPrefab.SetActive(true);
    }
    public void OnCloseAttackEndEvent()
    {
        Debug.Log("近距離攻撃終了", this);
        closeAttackPrefab.SetActive(false);
    }
    public void DeadTrigger()
    {
        currentState = DragonState.Dead;
        animator.SetTrigger("Dead");
    }
}
