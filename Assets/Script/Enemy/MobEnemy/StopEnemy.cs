using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;

public class StopEnemy : MobEnemy
{
    [SerializeField] protected Collider AttackRangecollider;
    [SerializeField] protected Collider Damagecollider;
    [SerializeField] protected float Damagecooldown=2.0f;
    [SerializeField] private GameObject GanGenerator;
    protected MobEnemy _status;
    [SerializeField] private float destroytime;
    [SerializeField] private  float Reaction_Pro=0.5f; //ダメージリアクションを起こす確率(値が大きいほど確率高い)
    private Coroutine DamagecooldownCoroutine;
    private Coroutine HitStopcooldownCoroutine;
    private vHealthController vHealthController;
    
    protected override void Start()
    {
        base.Start(); //基本クラスのstartを呼び出す
        _status=GetComponent<MobEnemy>();
        vHealthController=GetComponent<vHealthController>();
    }
    //ダメージリアクション関数継承
    public new void DamageReaction()
    {
        if(vHealthController.currentHealth>0)
        {
            //ランダムでダメージリアクションを起こす
            if(Random.value<Reaction_Pro)   
            {
                base.DamageReaction();

                if (DamagecooldownCoroutine != null)
                {
                    // Debug.Log("ダメージ処理中止");
                    StopCoroutine(DamagecooldownCoroutine);
                }
                DamagecooldownCoroutine = StartCoroutine(Cooldown());

                //ヒットストップの条件分岐
                if(_status.State==StateEnum.Damage)
                {
                    if (HitStopcooldownCoroutine != null)
                    {
                        // Debug.Log("ヒットストップ中止");
                        StopCoroutine(HitStopcooldownCoroutine);
                    }
                    HitStopcooldownCoroutine=StartCoroutine(hitstop(0.1f));
                }
            }
        } 
        //攻撃を受けた際に体力が0を切っていたら
        else
        {
            // Debug.Log("体力なし！！");
            //死状態に入っていなかったら実行
            if(_status.State!=StateEnum.Die)
            {
                base.OnDie();//死亡基底メソッド実行
                GanGenerator.SetActive(false);
                Debug.Log("死亡");   
                StartCoroutine(DestoryCoroutine(destroytime));
            }
        }
    }
    //死亡コルーチン--------------------------------------------------
    private IEnumerator DestoryCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
    //ダメージ受けたらクールダウンする
    public IEnumerator Cooldown()
    {
        // Debug.Log("クールダウン");
        AttackRangecollider.enabled=false;
        Damagecollider.enabled=false;
        yield return new WaitForSeconds(Damagecooldown);
        AttackRangecollider.enabled=true;
        // Debug.Log("再開");
        DamagecooldownCoroutine = null;
        _status.ReturnToNormal();
    }
    //ヒットストップ--------------------------------------------------
    public IEnumerator hitstop(float stoptime)
    {
        // Debug.Log("ヒットストップ");
        animator.speed=0f;
        yield  return new  WaitForSeconds(stoptime);
        animator.speed=1f;
        HitStopcooldownCoroutine= null;
    }
 }