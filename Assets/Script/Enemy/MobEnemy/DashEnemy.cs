using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;      // UniTask
using UnityEngine.AI;
using Unity.VisualScripting;

public class DashEnemy : MobEnemy
{
    [SerializeField] protected Collider AttackRangecollider;
    [SerializeField] protected Collider Damagecollider;
    [SerializeField] protected Collider chasecollider;
    [SerializeField] protected float Damagecooldown = 2.0f;
    [SerializeField] private float Reaction_Pro = 0.5f; //ダメージリアクションを起こす確率(値が大きいほど確率高い)
    private NavMeshAgent agent;
    protected MobEnemy _status;
    private vHealthController vHealthController;
    private UniTask DamagecooldownCoroutine;
    private UniTask HitStopcooldownCoroutine;
    private UniTask patrolCoroutine;
    private Transform player;
    private Vector3 initialPosition; // 敵の初期位置
    [SerializeField] private float patrolRadius = 10f; // 巡回範囲
    [Header("=== Behavior ===")]
    [Tooltip("ダッシュを開始するプレイヤー距離（XZ）")]
    [SerializeField] private float dashTriggerDistance = 7.0f;
    [Tooltip("予備動作（身構え）の時間")]
    [SerializeField] private float windupTime = 0.4f;
    [Tooltip("ダッシュ速度（コード移動）")]
    [SerializeField] private float dashSpeed = 11.0f;
    [Tooltip("ダッシュ継続時間")]
    [SerializeField] private float dashDuration = 0.65f;
    [Tooltip("ダッシュ開始直後だけ弱追尾する時間")]
    [SerializeField] private float softHomingTime = 0.15f;
    [Tooltip("ダッシュ後の硬直（反撃窓）")]
    [SerializeField] private float recoverTime = 0.8f;
    [Tooltip("1回ダッシュしてから次に試せるまでのクールダウン")]
    [SerializeField] private float dashCooldown = 2.2f;
    [SerializeField] private int time = 2;
    [Header("=== Follow-up (optional) ===")]
    [Tooltip("ダッシュ後に噛みつきフォローを行うか")]
    [SerializeField] private bool enableFollowUpBite = false;
    [Tooltip("フォローを行う確率（0〜1）。HPや難易度で調整してOK")]
    [Range(0f, 1f)][SerializeField] private float biteChance = 0.35f;
    [Tooltip("噛みつきを出す最大距離（近すぎ/離れすぎ防止）")]
    [SerializeField] private float biteRange = 2.3f;
    [Header("=== Animator Triggers ===")]
    [SerializeField] private string windupTrigger = "Windup"; // かがむ等
    [SerializeField] private string dashTrigger = "Dash";   // ダッシュ開始
    [SerializeField] private string biteTrigger = "Bite";   // 噛みつき
    [SerializeField] private int windupLayer = 0;        // レイヤー

    // 内部状態
    private bool isBusy;           // 予備動作〜リカバーの最中は true
    private float nextDashReadyTime; // クールダウン解除時刻
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start(); //基本クラスのstartを呼び出す
        agent = GetComponent<NavMeshAgent>();
        _status = GetComponent<MobEnemy>();
        vHealthController = GetComponent<vHealthController>();
        // 初期状態でも追尾方向を計算できるよう、共通のPlayer取得経路から参照を持っておく。
        player = PlayerLocator.FindTransform();

        // 敵の初期位置を記録
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);
        //        Debug.Log(_agent.velocity.magnitude);
        if (_status.State == StateEnum.Patrol && patrolCoroutine.Status != UniTaskStatus.Pending)
        {
            // Debug.Log("パトロール中");
            patrolCoroutine = Patrol();
        }
    }
    /// <summary>
    /// 追跡センサー（CollsionDetetor）の OnTriggerEnter/Stay から呼ばれる想定
    /// プレイヤー検知→Chaseへ
    /// </summary>
    public void OnDetectObjectChase(Collider other)
    {
        // Player検知時は最新のTransformに差し替え、ダッシュ中の誘導や追撃判定に使う。
        if (!PlayerLocator.TryGetTransformFromCollider(other, out player)) return;

        // ダッシュ中やクールダウン中は、同じ検知で攻撃を重ねて起動しない。
        if (isBusy || Time.time < nextDashReadyTime) return;
        if (State == StateEnum.Die || State == StateEnum.Damage) return;

        // ここで1サイクル開始（以降は内部で完結）
        PrepareAndDash().Forget();
    }

    //ダメージリアクション関数継承
    // ダメージリアクション処理
    public override void DamageReaction()
    {
        // ダッシュ敵だけのヒットストップと死亡時コライダー停止を、共通処理へ差し込む。
        ResolveDamageReaction(
            vHealthController,
            Reaction_Pro,
            2f,
            () =>
            {
                DamagecooldownCoroutine = Cooldown();

                if (_status.State == StateEnum.Damage)
                    HitStopcooldownCoroutine = HitStop(0.2f);
            },
            () => AttackRangecollider.enabled = Damagecollider.enabled = chasecollider.enabled = false);
    }
    /// <summary>
    /// 検知後の一連（予備動作→ダッシュ→リカバー→（任意）噛みつき）
    /// </summary>
    private async UniTask PrepareAndDash()
    {
        isBusy = true;
        var ct = this.GetCancellationTokenOnDestroy();

        // 予備動作が終わってから実際のダッシュに入ることで、見た目と当たり判定のタイミングを合わせる。
        State = StateEnum.Attack;
        if (agent) agent.isStopped = true;
        if (!string.IsNullOrEmpty(windupTrigger)) animator.SetTrigger(windupTrigger);

        // ▼ここを差し替え
        await WaitForStateEndAsync(animator, windupLayer, windupTrigger, ct); // Windupが終わるまで待つ

        // ダッシュ開始（見た目トリガー）
        if (!string.IsNullOrEmpty(dashTrigger)) animator.SetTrigger(dashTrigger);

        // コード移動ダッシュ
        await DashMoveAsync(ct);

        // リカバー
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(recoverTime), cancellationToken: ct);
        }
        catch (OperationCanceledException) { isBusy = false; return; }

        // 任意：近距離なら噛みつき
        if (enableFollowUpBite && player != null && UnityEngine.Random.value < biteChance)
        {
            // 追撃は水平距離だけで判定し、高低差で外れにくくする。
            float dist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                          new Vector3(player.position.x, 0, player.position.z));
            if (dist <= biteRange && !string.IsNullOrEmpty(biteTrigger))
                animator.SetTrigger(biteTrigger); // ダメージはアニメイベントの Attack() で
        }

        // 終了処理
        isBusy = false;
        nextDashReadyTime = Time.time + dashCooldown;

        // 以降の移動は自由（追従させたいなら再開、巡回に戻すならそのまま）
        if (agent) agent.isStopped = false;
        // ここで State を Patrol/Chase どちらに戻すかはお好みで
        State = StateEnum.Patrol;
    }

    public void ChangePatrol()
    {
        ReturnToNormal();
    }
    public async UniTask HitStop(float stoptime)
    {
        // Debug.Log("ヒットストップ");
        animator.speed = 0f;
        await UniTask.Delay((int)(stoptime * 1000)); // 秒からミリ秒に変換
        animator.speed = 1f;
    }
    // 巡回メソッド
    private async UniTask Patrol()
    {
        while (_status.State == StateEnum.Patrol)
        {
            Vector3 randomPoint = initialPosition + new Vector3(
                UnityEngine.Random.Range(-patrolRadius, patrolRadius),
                0,
                UnityEngine.Random.Range(-patrolRadius, patrolRadius)
            );

            agent.SetDestination(randomPoint);
            await UniTask.Delay(time * 1000); // 2000ms = 2秒
        }

        // 状態が変わった＝Patrol終わり
        patrolCoroutine = default;
    }
    //攻撃を受けると追跡・攻撃・当たりのコライダーを一時的に非表示
    public async UniTask Cooldown()
    {
        // Debug.Log("クールダウン");
        AttackRangecollider.enabled = Damagecollider.enabled = chasecollider.enabled = false;
        animator.ResetTrigger("Attack");
        // 指定された時間待機
        await UniTask.Delay((int)(Damagecooldown * 1000)); // 秒からミリ秒に変換
        AttackRangecollider.enabled = true;
        // Debug.Log("再開");
        AttackRangecollider.enabled = chasecollider.enabled = true;
        DamagecooldownCoroutine = default;
        _status.ReturnToNormal();
        agent.isStopped = false;
        initialPosition = transform.position;
    }
    /// <summary>
    /// ダッシュ本体（短時間だけ弱追尾→固定方向）※RootMotionはOFF推奨
    /// </summary>
    private async UniTask DashMoveAsync(CancellationToken ct)
    {
        float elapsed = 0f;
        Vector3 dir = DirToPlayerXZOrForward();

        while (elapsed < dashDuration && !ct.IsCancellationRequested &&
               State != StateEnum.Die && State != StateEnum.Damage)
        {
            // 出始めだけプレイヤー方向を追い直し、その後は直線的に突進する。
            if (elapsed < softHomingTime) dir = DirToPlayerXZOrForward();

            transform.position += dir * (dashSpeed * Time.deltaTime);

            if (dir.sqrMagnitude > 0.0001f)
            {
                var rot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 15f);
            }

            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
    }


    /// <summary>XZ 平面のプレイヤー方向（いなければ正面）</summary>
    private Vector3 DirToPlayerXZOrForward()
    {
        // Playerが未取得または消滅済みなら、現在向いている方向へ直進して処理を継続する。
        if (player == null)
            return new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;

        var d = player.position - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude < 0.0001f) d = transform.forward;
        return d.normalized;
    }
    private static async UniTask WaitForStateEndAsync(
    Animator anim, int layer, string stateName, CancellationToken ct)
    {
        int target = Animator.StringToHash(stateName);

        // まず対象ステートへ入るのを待ち、その後 normalizedTime で終了を待つ。
        while (!ct.IsCancellationRequested)
        {
            var st = anim.GetCurrentAnimatorStateInfo(layer);
            if (st.shortNameHash == target && !anim.IsInTransition(layer)) break;
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        // そのステートが終わるまで待つ（normalizedTime >= 1）
        while (!ct.IsCancellationRequested)
        {
            var st = anim.GetCurrentAnimatorStateInfo(layer);
            if (st.shortNameHash != target) break;                         
            if (st.normalizedTime >= 1f && !anim.IsInTransition(layer)) break;
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
    }
}
