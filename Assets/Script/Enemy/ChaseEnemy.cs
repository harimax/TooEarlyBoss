using System.Collections;
using System.Collections.Generic;
using Invector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;

public class ChaseEnemy : MobEnemy
{
    // private float chargeTime = 5.0f;
    // private float timeCount;

    [SerializeField] protected Collider AttackRangecollider;
    [SerializeField] protected Collider Damagecollider;
    [SerializeField] protected Collider chasecollider;
    [SerializeField] protected float Damagecooldown = 2.0f;
    [SerializeField] private float Reaction_Pro = 0.5f; //ダメージリアクションを起こす確率(値が大きいほど確率高い)
    [SerializeField] private LayerMask raycastLayerMask;
    private NavMeshAgent _agent;
    protected MobEnemy _status;
    protected Enemy_Attack enemy_Attack;
    private RaycastHit[] _raycastHits = new RaycastHit[10];
    private vHealthController vHealthController;
    private UniTask DamagecooldownCoroutine;
    private UniTask HitStopcooldownCoroutine;
    private UniTask patrolCoroutine;
    private Vector3 initialPosition; // 敵の初期位置
    [SerializeField] private float patrolRadius = 10f; // 巡回範囲
    protected override void Start()
    {
        base.Start(); //基本クラスのstartを呼び出す
        _agent = GetComponent<NavMeshAgent>();
        _status = GetComponent<MobEnemy>();
        vHealthController = GetComponent<vHealthController>();
        // 敵の初期位置を記録
        initialPosition = transform.position;
    }
    void Update()
    {
        animator.SetFloat("MoveSpeed", _agent.velocity.magnitude);
        //        Debug.Log(_agent.velocity.magnitude);
        if (_status.State == StateEnum.Patrol && patrolCoroutine.Status != UniTaskStatus.Pending)
        {
            Debug.Log("パトロール中");
            patrolCoroutine = Patrol();
        }
    }
    //ダメージリアクション関数継承
    // ダメージリアクション処理
    public new void DamageReaction()
    {
        if (vHealthController.currentHealth > 0)
        {
            // ランダムでリアクション発生
            if (Random.value < Reaction_Pro)
            {
                base.DamageReaction();
                // 攻撃コライダーなどを一時無効にするクールダウン
                DamagecooldownCoroutine = Cooldown();  // UniTaskで処理を実行

                // 現在ステートがダメージなら、ヒットストップ発生
                if (_status.State == StateEnum.Damage)
                    HitStopcooldownCoroutine = HitStop(0.2f);  // UniTaskで処理を実行
            }
        }
        // 体力が0以下でまだ死亡状態になっていない場合
        else if (_status.State != StateEnum.Die)
        {
            base.OnDie(); // 基底クラスの死亡処理を実行
            StartCoroutine(DestoryCoroutine(4.0f)); // 4秒後にオブジェクト削除
        }
    }

    //範囲に入れば追跡するメソッド
    public void OnDetectObjectChase(Collider collider)
    {
        // 攻撃・死亡・ダメージ状態のときは追跡しない
        if (_status.State == StateEnum.Attack || _status.State == StateEnum.Die || _status.State == StateEnum.Damage)
        {
            // Debug.Log("攻撃中・死亡・ダメージ状態なので追跡しません");
            _agent.isStopped = true;
            return;
        }
        // パトロール状態またはチェイス状態のときは追跡を始める
        else if (_status.State == StateEnum.Patrol || _status.State == StateEnum.Chase)
        {
            // パトロール中に追跡開始するなら巡回を止める
            if (patrolCoroutine.Status != UniTaskStatus.Pending)
            {
                patrolCoroutine =default;
            }
            State = StateEnum.Chase;
            // プレイヤーのタグを追跡する
            if (collider.CompareTag("Player"))
            {
                // Debug.Log("プレイヤー発見！追跡開始");
                _agent.isStopped = false;
                _agent.destination = collider.transform.position;
            }
        }
    }
    //死亡コルーチン
    private IEnumerator DestoryCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
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
        DamagecooldownCoroutine =default;
        _status.ReturnToNormal();
        _agent.isStopped = false;
        initialPosition = transform.position;
    }
    public async UniTask HitStop(float stoptime)
    {
        // Debug.Log("ヒットストップ");
        animator.speed = 0f;
        await UniTask.Delay((int)(stoptime * 1000)); // 秒からミリ秒に変換
        animator.speed = 1f;
    }
    // コルーチンを止めてから再起動する汎用関数
    private void RestartCoroutine(ref Coroutine coroutine, IEnumerator routine)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(routine);
    }
    // 巡回メソッド
    private async UniTask Patrol()
    {
        while (_status.State == StateEnum.Patrol)
        {
            Vector3 randomPoint = initialPosition + new Vector3(
                Random.Range(-patrolRadius, patrolRadius),
                0,
                Random.Range(-patrolRadius, patrolRadius)
            );

            _agent.SetDestination(randomPoint);
            await UniTask.Delay(2000); // 2000ms = 2秒
        }

        // 状態が変わった＝Patrol終わり
        patrolCoroutine = default;
    }
    // 攻撃アニメーション中かチェックする関数
    private bool IsPlayingAttackAnimation()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // レイヤー0
        return stateInfo.IsTag("Attack"); // または stateInfo.IsName("Attack") にしてもいい
    }

}
