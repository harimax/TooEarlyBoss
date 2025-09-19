using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.AI;
using Invector;

public class TreeBossController : MonoBehaviour, IBossController
{
    private Animator animator;
    private bool _isPaused = false;
    private Transform player;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform[] firePoints;
    // ===== 攻撃まわり =====
    [Header("Attack Settings")]
    [SerializeField] private float attackInterval = 5f;   // 攻撃間隔（秒）
    [SerializeField] private float projectileSpeed = 12f; // 弾速
    [SerializeField] private float fireAngleLimit = 75f;  // 左右制限角度
    [SerializeField] private float fireDelayBetweenMuzzles = 0.3f; // 発射口ごとの間隔
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints; // 巡回ポイント
    [SerializeField] private float waypointThreshold = 0.5f; // 到達判定距離
    private int currentWaypoint = 0;
    private bool isWalking = false;
    private NavMeshAgent agent;
    // ===== 体力監視（子のモグラ） =====
    private vHealthController moleHealth; // 子に付いている vHealthController
    private float startWalkThreshold;     // 1/3 しきい値
    private bool walkTriggered = false;   // 多重起動防止


    private float timer;
    private bool _isFiring = false;                              // 多重起動ガード
    private CancellationToken _ct;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = this.gameObject.GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        timer = attackInterval;
        _ct = this.GetCancellationTokenOnDestroy();
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true; // 最初は止まっている
        }
        // 子オブジェクトからモグラの vHealthController を取得して監視
        moleHealth = GetComponentInChildren<vHealthController>(includeInactive: true);
        if (moleHealth != null)
        {
            Debug.Log("TreeBossController: 子に vHealthController(モグラ)が見つかりました。");
            // Max の 1/3 をしきい値として計算（Maxは公開プロパティで参照可）
            startWalkThreshold = moleHealth.maxHealth / 3f;
            // 体力変化イベントに登録（現在値→変更のたびに呼ばれる）
            moleHealth.onChangeHealth.AddListener(OnMoleHealthChanged); // 値は現在HP(float)
        }
        else
        {
            Debug.LogWarning("TreeBossController: 子に vHealthController(モグラ)が見つかりませんでした。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            BallShotAsync().Forget();
            timer = attackInterval; // クールダウン後に再セット
        }

        // 巡回処理（HPが1/3以下になったら）
        if (isWalking && agent != null && !agent.pathPending)
        {
            if (agent.remainingDistance <= waypointThreshold)
            {
                GoToNextWaypoint();
            }
        }
    }

    //=== HP監視コールバック ===
    private void OnMoleHealthChanged(float current)
    {
        if (walkTriggered) return; // もう開始済み
        if (current <= startWalkThreshold)
        {
            walkTriggered = true;
            StartWalking();
        }
    }
    //------------------------------------------------------------------------------------------------
    /// <summary>
    /// 弾を発射する
    /// </summary>
    private async UniTask BallShotAsync()
    {
        _isFiring = true;

        Debug.Log("弾発射");
        // すべての発射口から発射を試みる
        foreach (var muzzle in firePoints)
        {
            FireAtPlayer(muzzle);
        }

        // 発射後ちょっと待つ（演出やディレイ用）
        // 最低限 1 フレームだけ待ってガード解除（同フレーム連打防止）
        await UniTask.Yield(PlayerLoopTiming.Update, _ct);
        _isFiring = false;
    }
    /// <summary>
    /// プレイヤーが範囲内なら弾を発射
    /// </summary>
    private void FireAtPlayer(Transform muzzle)
    {
        if (player == null || ballPrefab == null || firePoints == null) return;

        // プレイヤー方向（XZ平面のみ）
        Vector3 direction = player.position - muzzle.position;
        var checkDirection = direction;
        checkDirection.y = 0f;
        direction.Normalize();
        checkDirection.Normalize();

        // 正面ベクトル（XZ平面のみ）
        Vector3 forward = muzzle.forward;
        forward.y = 0f;
        forward.Normalize();

        // 水平角度を
        float angle = Vector3.SignedAngle(forward, checkDirection, Vector3.up);
        //プレイヤーが範囲内にいるかチェックする
        if (Mathf.Abs(angle) <= fireAngleLimit)
        {
            // 発射
            GameObject ball = Instantiate(ballPrefab, muzzle.position, Quaternion.identity);
            if (ball.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = direction * projectileSpeed;
            }
            // Debug.Log($"弾を発射！（角度:{angle:F1}°）");
        }
        else
        {
            // Debug.Log($"プレイヤーは発射範囲外（角度:{angle:F1}°）");
        }
    }
    //------------------------------------------------------------------------------------------------
    // ====== 巡回 ======
    private void StartWalking()
    {
        if (agent == null || waypoints.Length == 0) return;

        isWalking = true;
        agent.isStopped = false;
        currentWaypoint = 0;
        agent.SetDestination(waypoints[currentWaypoint].position);
        Debug.Log("Boss started walking!");
    }

    private void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypoint].position);
    }


    //-----------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------
    public void PauseBoss()
    {
        // Update 系を止める
        _isPaused = true;
        // MonoBehaviour の Update を無効化する場合はこちらを使ってもよい
        // enabled = false;
        // アニメータを止めたいなら：
        // if (animator != null) animator.enabled = false;
    }
    /// <summary>
    /// ボスの動きを再開する
    /// </summary>
    public void ResumeBoss()
    {
        _isPaused = false;
        // enabled = true;
        if (animator != null) animator.enabled = true;
        // タイマー初期化
        timer = attackInterval;
    }
}
