using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.AI;
using Invector;
using DG.Tweening;

public class RobotBossController : MonoBehaviour, IBossController
{
    // ボスの状態
    private enum BossState
    {
        Idle,       // 待機
        Shooting,   // 弾発射
        ChargeShoot, // チャージレーザー
        Dead// 死亡

    };
    private BossState currentState;

    // ---------------- 参照 ----------------
    [Header("Attack Objects")]
    [SerializeField] private GameObject bulletShooters;
    [SerializeField] private GameObject chargeLaser;

    [Header("Components")]
    [SerializeField] private MOGURAManager groundEnemyManager;// 参照を追加
    private Animator animator;
    [Header("Barrier Settings")]
    [SerializeField] private GameObject Barrier;
    private SphereCollider ejectCollider;
    [SerializeField] private float expandDuration = 1f;
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private float shrinkDuration = 1f;
    [SerializeField] private float minRadius = 0.1f;
    [SerializeField] private float maxRadius = 12f;
    [Header("Floor Controller")]
    [SerializeField] private RobotBossFloorController floorController;
    private CancellationToken _ct;
    private CancellationTokenSource cts; // 状態ループのキャンセル用
    // 一時停止フラグ（ボス全体の挙動を止める）
    private bool _isPaused = false;
    //シングルトン的に外部から参照されるインスタンス
    public static RobotBossController robotBossController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = this.gameObject.GetComponent<Animator>();
        _ct = this.GetCancellationTokenOnDestroy();
        ejectCollider = Barrier.GetComponent<SphereCollider>();
        robotBossController = this;

        // 地上モグラ撃破イベントを購読
        if (groundEnemyManager != null)
        {
            groundEnemyManager.OnGroundEnemyKilled += OnGroundEnemyKilledHandler;
        }
        ChangeState(BossState.Shooting);
    }

    private void OnDestroy()
    {
        if (groundEnemyManager != null)
            groundEnemyManager.OnGroundEnemyKilled -= OnGroundEnemyKilledHandler;
    }
    /// <summary>シングルトンインスタンス取得。</summary>
    public static RobotBossController GetInstance() => robotBossController;





    //-----------------------------------------------------------------------------------------------------
    /// <summary>
    /// 吹き飛ばしバリア発動（外部：プレイヤーが一定時間乗った、HPしきい値など）。
    /// </summary>
    public async UniTask OnActiveBarriar()
    {
        await AnimateRadius(minRadius, maxRadius, expandDuration);

        // ② 待機
        await UniTask.Delay((int)(holdDuration * 1000));

        // ③ 縮小
        await AnimateRadius(maxRadius, minRadius, shrinkDuration);
        //アイドルに移動する
        ChangeState(BossState.Idle);
        floorController?.ResetFloor();
    }
    //------------------------------------------------------------------------------------------------


    /// <summary>
    /// ボスを一時停止（弾発射などを止める）。
    /// </summary>
    public void PauseBoss()
    {
        // Update 系を止める
        _isPaused = true;
        bulletShooters.SetActive(false);

    }
    /// <summary>
    /// ボスの一時停止を解除。
    /// </summary>
    public void ResumeBoss()
    {
        _isPaused = false;
        if (animator != null) animator.enabled = true;
        bulletShooters.SetActive(true);
    }

    //------------------------------------------------------------------------------------------------
    /// <summary>
    /// 状態を切り替える共通関数。
    /// 進行中の状態タスクをキャンセルし、新しい状態処理を開始する。
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

        // Debug.Log("Idle 開始");
        await UniTask.Delay(5000, cancellationToken: token); // 5秒待機
        // Debug.Log("Idle 終了 → Shootingへ");
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

        // Debug.Log("Shooting 開始");
        await UniTask.Delay(7000, cancellationToken: token); // 7秒後に遷移
    }
    /// <summary>
    /// ChargeShoot状態：5秒後にレーザー発射 → Shootingへ
    /// </summary>
    private async UniTask ChargeShootState(CancellationToken token)
    {
        bulletShooters.SetActive(false);
        chargeLaser.SetActive(false);

        // 足場を上げる（既に上がっていれば内部でDOTweenが再設定される）
        floorController?.UpFloor();
        await UniTask.Delay(10000, cancellationToken: token); // 溜め時間
        if (token.IsCancellationRequested) return;

        // レーザー発射
        chargeLaser.SetActive(true);
        // Debug.Log("レーザー発射！");

        await UniTask.Delay(3000, cancellationToken: token); // レーザー表示時間（任意）
        chargeLaser.SetActive(false);

        // Debug.Log("ChargeShoot 終了 → Shootingへ");
        if (!token.IsCancellationRequested)
            ChangeState(BossState.Shooting);
    }
    //----------------------------------------------------------------------------------------

    /// <summary>
    /// ボス死亡トリガ（HP0 などから呼ばれる）。
    /// </summary>
    public void DeadTrigger()
    {
        currentState = BossState.Dead;
        Debug.Log("ボス討伐");
        OnDestroyRobot().Forget();
    }
    //
    public async UniTaskVoid OnDestroyRobot()
    {
        PauseBoss();
        bulletShooters.SetActive(false);
        await UniTask.Delay(1000);
        Destroy(this.gameObject);

    }
    /// <summary>
    /// 地上敵が倒れた時に呼ばれる：足場を上昇
    /// </summary>
    private void OnGroundEnemyKilledHandler()
    {
        // ここで足場を上げる（上げ終わったら次スポーン許可を返す）
        floorController?.UpFloor();
        ChangeState(BossState.ChargeShoot);
    }

    private async UniTask AnimateRadius(float from, float to, float duration)
    {
        if (ejectCollider == null) return;
        float t = 0f;
        while (t < duration)
        {
            t += Time.fixedDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            if (ejectCollider == null) return;
            ejectCollider.radius = Mathf.Lerp(from, to, u);
            await UniTask.WaitForFixedUpdate();
        }
        if (ejectCollider == null) return;
        ejectCollider.radius = to;
    }
}
