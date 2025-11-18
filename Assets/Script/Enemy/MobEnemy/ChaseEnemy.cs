using System.Collections;
using System.Collections.Generic;
using Invector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class ChaseEnemy : MobEnemy
{

    [SerializeField] protected Collider AttackRangecollider;
    [SerializeField] protected Collider Damagecollider;
    [SerializeField] protected Collider chasecollider;
    [SerializeField] protected float Damagecooldown = 2.0f;
    [SerializeField] private float Reaction_Pro = 0.5f; //ダメージリアクションを起こす確率(値が大きいほど確率高い)
    private NavMeshAgent _agent;
    protected MobEnemy _status;
    protected Enemy_Attack enemy_Attack;
    private vHealthController vHealthController;
    private UniTask DamagecooldownCoroutine;
    private UniTask patrolCoroutine;
    private Vector3 initialPosition; // 敵の初期位置
    [SerializeField] private float patrolRadius = 10f; // 巡回範囲
    [SerializeField] private int time=2;
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
        // パトロール状態の場合、巡回を開始
        if (_status.State == StateEnum.Patrol && patrolCoroutine.Status != UniTaskStatus.Pending)
        {
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
            }
        }
        // 体力が0以下でまだ死亡状態になっていない場合
        else if (_status.State != StateEnum.Die)
        {
            base.OnDie(); // 基底クラスの死亡処理を実行
            DestoryCoroutine(2.0f).Forget(); 
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
                patrolCoroutine = default;
            }

            // プレイヤーのタグを追跡する
            if (collider.CompareTag("Player"))
            {
                State = StateEnum.Chase;
                // Debug.Log("プレイヤー発見！追跡開始");
                _agent.isStopped = false;
                _agent.destination = collider.transform.position;
            }
        }
    }
    public void ChangePatrol()
    {
        ReturnToNormal();
    }
    //死亡コルーチン
    private async UniTask DestoryCoroutine(float time)
    {
        await UniTask.Delay((int)(time*1000));
        Destroy(gameObject);
    }
    //攻撃を受けると追跡・攻撃・当たりのコライダーを一時的に非表示
    public async UniTask Cooldown()
    {
        AttackRangecollider.enabled = Damagecollider.enabled = chasecollider.enabled = false;
        animator.ResetTrigger("Attack");
        // 指定された時間待機
        await UniTask.Delay((int)(Damagecooldown * 1000)); // 秒からミリ秒に変換
        AttackRangecollider.enabled = chasecollider.enabled = true;
        DamagecooldownCoroutine = default;
        _status.ReturnToNormal();
        _agent.isStopped = false;
        initialPosition = transform.position;
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
            await UniTask.Delay(time*1000); // 2000ms = 2秒
        }
        // 状態が変わった＝Patrol終わり
        patrolCoroutine = default;
    }
}
