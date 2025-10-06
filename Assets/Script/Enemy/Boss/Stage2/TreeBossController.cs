using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.AI;
using Invector;
using DG.Tweening;

public class TreeBossController : MonoBehaviour, IBossController
{
    // ボスの状態
    private enum BossState
    {
        Idle,       // 待機
        Shooting,   // 弾発射
        ChargeShoot // チャージレーザー
    };
    private BossState currentState;

    [SerializeField] private GameObject bulletShooters;
    [SerializeField] private GameObject chargeLaser;
    [SerializeField] private Transform UpperBody;
    [SerializeField] private GameObject Floor;
    [SerializeField] private Transform target; // 移動先
    [SerializeField] private MOGURAManager groundEnemyManager;// 参照を追加
    private Vector3 FloorInitPos;
    [SerializeField] private float duration = 10f; // 移動にかける時間
    private float baseY; // 初期のY角度（今回は180°）
    public static TreeBossController treeBossController;
    private Animator animator;
    private bool _isPaused = false;
    private Transform player;
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints; // 巡回ポイント
    [SerializeField] private float waypointThreshold = 0.5f; // 到達判定距離
    private int currentWaypoint = 0;
    private bool isWalking = false;
    private NavMeshAgent agent;
    // ----- 押し出し系 ----------------
    [SerializeField] private GameObject Barrier;
    private SphereCollider ejectCollider;
    [SerializeField] private float expandDuration = 1f;
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private float shrinkDuration = 1f;
    [SerializeField] private float minRadius = 0.1f;
    [SerializeField] private float maxRadius = 12f;
    // ===== 体力監視（子のモグラ） =====
    private vHealthController moleHealth; // 子に付いている vHealthController
    private float startWalkThreshold;     // 1/3 しきい値
    private float thTwoThird;   // 2/3 の閾値
    private float thOneThird;   // 1/3 の閾値
    private bool firedTwoThird; // 2/3イベントを発火済みか
    private bool firedOneThird; // 1/3イベントを発火済みか
    private bool walkTriggered = false;   // 多重起動防止
    private CancellationToken _ct;
    private CancellationTokenSource cts; // 状態ループのキャンセル用

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = this.gameObject.GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        _ct = this.GetCancellationTokenOnDestroy();
        agent = GetComponent<NavMeshAgent>();
        ejectCollider = Barrier.GetComponent<SphereCollider>();
        treeBossController = this;
        ChangeState(BossState.Shooting);
        if (agent != null)
        {
            agent.isStopped = true; // 最初は止まっている
        }
        baseY = UpperBody.localEulerAngles.y;
        FloorInitPos = Floor.transform.position;
        if (groundEnemyManager != null)
        {
            groundEnemyManager.OnGroundEnemyKilled += OnGroundEnemyKilledHandler;
        }

        Debug.Log("floorInit:" + FloorInitPos);

        // Unityは360°表記になるので正規化
        if (baseY > 180f) baseY -= 360f;
        // 子オブジェクトからモグラの vHealthController を取得して監視
        moleHealth = GetComponentInChildren<vHealthController>(includeInactive: true);
        if (moleHealth != null)
        {
            Debug.Log("TreeBossController: 子に vHealthController(モグラ)が見つかりました。");
            // Max の 1/3 をしきい値として計算（Maxは公開プロパティで参照可）
            startWalkThreshold = moleHealth.maxHealth / 3f;
            // しきい値：Max の 2/3・1/3
            thTwoThird = moleHealth.maxHealth * (2f / 3f);
            thOneThird = moleHealth.maxHealth * (1f / 3f);

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

        // 巡回処理（HPが1/3以下になったら）
        if (isWalking && agent != null)
        {
            if (agent.remainingDistance <= waypointThreshold)
            {
                GoToNextWaypoint();
            }
        }
    }
    void LateUpdate()
    {
        RotateUpperBody();
    }

    //=== HP監視コールバック ===
    private async void OnMoleHealthChanged(float current)
    {
        if (walkTriggered) return; // もう開始済み
        // 2/3 閾値：未発火 かつ 現在HPが 2/3 以下に落ちた瞬間
        if (!firedTwoThird && current <= thTwoThird)
        {
            firedTwoThird = true;
            Debug.Log("[BossEvent] HPが2/3以下：吹き飛ばし①予約");
            await UniTask.Delay(1000); // 1秒遅延
            await OnActiveBarriar();

        }

        // 1/3 閾値：未発火 かつ 現在HPが 1/3 以下に落ちた瞬間
        if (!firedOneThird && current <= thOneThird)
        {
            firedOneThird = true;
            Debug.Log("[BossEvent] HPが1/3以下：吹き飛ばし②予約");
            await UniTask.Delay(1000); // 1秒遅延
            await OnActiveBarriar();

        }

        if (current <= startWalkThreshold)
        {
            walkTriggered = true;
            StartWalking();
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
        animator.SetBool("Walking", true);
    }

    private void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        Debug.Log("現在のポイント:" + currentWaypoint);
        agent.SetDestination(waypoints[currentWaypoint].position);
    }


    //-----------------------------------------------------------------------------------------------------
    //吹き飛ばし処理
    public async UniTask OnActiveBarriar()
    {
        await AnimateRadius(minRadius, maxRadius, expandDuration);

        // ② 待機
        await UniTask.Delay((int)(holdDuration * 1000));

        // ③ 縮小
        await AnimateRadius(maxRadius, minRadius, shrinkDuration);
        //アイドルに移動する
        ChangeState(BossState.Idle);
        ResetFloor();
    }

    private async UniTask AnimateRadius(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.fixedDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            ejectCollider.radius = Mathf.Lerp(from, to, u);
            await UniTask.WaitForFixedUpdate();
        }
        ejectCollider.radius = to;
    }
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
    }

    //------------------------------------------------------------------------------------------------
    /// <summary>
    /// 状態を切り替える共通関数
    /// </summary>
    private void ChangeState(BossState newState)
    {
        // 進行中の状態処理を止める
        cts?.Cancel();
        cts = new CancellationTokenSource();

        currentState = newState;

        // 状態ごとに処理開始
        switch (newState)
        {
            case BossState.Idle:
                IdleState(cts.Token).Forget();
                break;
            case BossState.Shooting:
                ShootingState(cts.Token).Forget();
                break;
            case BossState.ChargeShoot:
                ChargeShootState(cts.Token).Forget();
                break;
        }
    }
    /// <summary>
    /// Idle状態：5秒待機して Shooting へ
    /// </summary>
    private async UniTask IdleState(CancellationToken token)
    {
        bulletShooters.SetActive(false);
        chargeLaser.SetActive(false);

        Debug.Log("Idle 開始");
        await UniTask.Delay(5000, cancellationToken: token); // 5秒待機
        Debug.Log("Idle 終了 → Shootingへ");
        if (!token.IsCancellationRequested)
            ChangeState(BossState.Shooting);
    }
    /// <summary>
    /// Shooting状態：弾発射オブジェクトON → 7秒後に ChargeShoot
    /// </summary>
    private async UniTask ShootingState(CancellationToken token)
    {
        bulletShooters.SetActive(true);
        chargeLaser.SetActive(false);

        // ★ Shooting開始時に、地上敵を1体だけ湧かせる（既に居れば何もしない）
        groundEnemyManager?.SpawnOnceIfNone().Forget();

        Debug.Log("Shooting 開始");
        await UniTask.Delay(7000, cancellationToken: token); // 7秒後に遷移
        // Debug.Log("Shooting 終了 → ChargeShootへ");
        // if (!token.IsCancellationRequested)
        //     ChangeState(BossState.ChargeShoot);
    }
    /// <summary>
    /// ChargeShoot状態：5秒後にレーザー発射 → Shootingへ
    /// </summary>
    private async UniTask ChargeShootState(CancellationToken token)
    {
        bulletShooters.SetActive(false);
        chargeLaser.SetActive(false);

        UpFloor();
        Debug.Log("ChargeShoot 溜め開始");
        await UniTask.Delay(10000, cancellationToken: token); // 溜め時間
        if (token.IsCancellationRequested) return;

        // レーザー発射
        chargeLaser.SetActive(true);
        Debug.Log("レーザー発射！");

        await UniTask.Delay(3000, cancellationToken: token); // レーザー表示時間（任意）
        chargeLaser.SetActive(false);

        Debug.Log("ChargeShoot 終了 → Shootingへ");
        if (!token.IsCancellationRequested)
            ChangeState(BossState.Shooting);
    }
    //----------------------------------------------------------------------------------------

    //Shootingの時に上半身を回転させる処理
    private void RotateUpperBody()
    {
        if (player == null) return;
        Debug.Log("プレイヤーに向いています");
        UpperBody.LookAt(player);
        // 現在の回転角を取得
        Vector3 euler = UpperBody.localEulerAngles;

        // Z軸は固定（傾きをなくす）
        euler.z = 0f;
        euler.y = 180f;

        // 反映
        UpperBody.localEulerAngles = euler;
    }

    public static TreeBossController GetInstance()
    {
        return treeBossController;
    }
    //足場を上場させる処理
    private void UpFloor()
    {
        // 既存 UpFloor のDOTweenに OnComplete を付ける
        DOTween.Kill(Floor.transform);
        if (player.transform.position.y < target.position.y / 2)
        {
            Debug.Log(player.transform.position.y);
            ResetFloor();
        }
        Floor.transform
        .DOMove(target.position, duration)
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            // 足場上昇が終わったら「次のスポーンを許可」
            groundEnemyManager?.ReadyForNextSpawn();
        });
    }
    //足場の位置をリセットする処理
    private void ResetFloor()
    {
        Debug.Log("位置リセット");
        // Floor と子の Tween を全部停止
        DOTween.Kill(Floor.transform);
        foreach (var t in Floor.GetComponentsInChildren<Transform>(true))
            DOTween.Kill(t);
        Floor.transform.position = FloorInitPos;
    }
    //-----------------------------------------------------------------------------
    private void OnDestroy()
    {
        if (groundEnemyManager != null)
            groundEnemyManager.OnGroundEnemyKilled -= OnGroundEnemyKilledHandler;
    }
    /// <summary>
    /// 地上敵が倒れた時に呼ばれる：足場を上昇
    /// </summary>
    private void OnGroundEnemyKilledHandler()
    {
        // ここで足場を上げる（上げ終わったら次スポーン許可を返す）
        UpFloor();
        ChangeState(BossState.ChargeShoot);
    }
}
