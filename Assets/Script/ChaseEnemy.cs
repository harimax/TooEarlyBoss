using System.Collections;
using System.Collections.Generic;
using Invector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ChaseEnemy : MobEnemy
{
    // private float chargeTime = 5.0f;
    // private float timeCount;
   
    [SerializeField] protected Collider AttackRangecollider;
    [SerializeField] protected Collider Damagecollider;
    [SerializeField] protected Collider chasecollider;
    [SerializeField] protected float Damagecooldown=2.0f;
    [SerializeField] private  float Reaction_Pro=0.5f; //ダメージリアクションを起こす確率(値が大きいほど確率高い)
    [SerializeField] private LayerMask raycastLayerMask;
    private NavMeshAgent _agent;
    protected MobEnemy _status;
    protected Enemy_Attack enemy_Attack;
    private RaycastHit[] _raycastHits= new RaycastHit[10];
    private vHealthController vHealthController;
    private Coroutine DamagecooldownCoroutine;
    private Coroutine HitStopcooldownCoroutine;
    private Vector3 AwayPos;
    protected override void Start()
    {
        base.Start(); //基本クラスのstartを呼び出す
        _agent=GetComponent<NavMeshAgent>();
        _status=GetComponent<MobEnemy>();
        vHealthController=GetComponent<vHealthController>();
    }
    void Update()
    {
        animator.SetFloat("MoveSpeed", _agent.velocity.magnitude);
//        Debug.Log(_agent.velocity.magnitude);
    }
    //ダメージリアクション関数継承
    public new void DamageReaction()
    {
        //体力が0より大きいとき
        if(vHealthController.currentHealth>0)
        {
            //ランダムでダメージリアクションを起こす
            if(Random.value<Reaction_Pro)   
            {
                base.DamageReaction();
                //ヒットストップのコルーチンを保持していればコルーチンを更新する
                if (DamagecooldownCoroutine != null)
                {
                    // Debug.Log("ダメージ処理中止");
                    StopCoroutine(DamagecooldownCoroutine);
                }
                DamagecooldownCoroutine = StartCoroutine(Cooldown());

                //ヒットストップの条件分岐
                if(_status.state==StatueEnum.Damage)
                {
                    if (HitStopcooldownCoroutine != null)
                    {
                        // Debug.Log("ヒットストップ中止");
                        StopCoroutine(HitStopcooldownCoroutine);
                    }
                    HitStopcooldownCoroutine=StartCoroutine(hitstop(0.2f));
                }
            }
        } 
        //攻撃を受けた際に体力が0を切っていたら
        else
        {
            // Debug.Log("体力なし！！");
            //死亡状態に入っていなかったら実行
            if(_status.state!=StatueEnum.Die)
            {
                base.OnDie();//死亡基底メソッド実行
                Debug.Log("死亡");   
                StartCoroutine(DestoryCoroutine(4.0f));
            }
        }
    }

    //範囲に入れば追跡するメソッド
    public void OnDetectObjectChase(Collider collider)
    {
        //Moveable状態以外のときは追跡しない
        if(_status.state!=StatueEnum.Moveable)
        {
            // Debug.Log("動けません");
            _agent.isStopped=true;
            return;
        }
        //playerのタグを追跡する
        else if(collider.CompareTag("Player"))
        {
            
            // Debug.Log("追跡範囲内");
            var positionDiff=collider.transform.position-transform.position;
            var distance=positionDiff.magnitude;
            var direction= positionDiff.normalized;
            var hitcount =Physics.RaycastNonAlloc(transform.position,direction,_raycastHits,distance,raycastLayerMask);
            // Debug.Log("hitcount"+hitcount);
            //ヒット数が０ならば追従
            if(hitcount==0)
            {
                _agent.isStopped=false;
                _agent.destination=collider.transform.position;
            }
            //なかったら停止
            else
            {
                _agent.isStopped=true;
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
    public IEnumerator Cooldown()
    {
        // Debug.Log("クールダウン");
        AttackRangecollider.enabled=false;
        animator.ResetTrigger("Attack");
        Damagecollider.enabled=false;
        chasecollider.enabled=false;
        yield return new WaitForSeconds(Damagecooldown);
        AttackRangecollider.enabled=true;
        // Debug.Log("再開");
        chasecollider.enabled=true;
        DamagecooldownCoroutine = null;
        _status.GOTONormalIF();
    }
    public IEnumerator hitstop(float stoptime)
    {
        // Debug.Log("ヒットストップ");
        animator.speed=0f;
        yield  return new  WaitForSeconds(stoptime);
        animator.speed=1f;
        HitStopcooldownCoroutine= null;
    }
 
}
