using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Attack : MonoBehaviour
{
    protected MobEnemy _status;
    [SerializeField] private ChaseEnemy enemy;

    [SerializeField] public Collider attackcollider;
    // [SerializeField] protected float attackcooldown=0.7f;
    public float dic;
    // Start is called before the first frame update
    private void Start()
    {
        _status=GetComponent<MobEnemy>();
        enemy=GetComponent<ChaseEnemy>();
    }

    //攻撃範囲内に入れば呼ばれる
    public virtual void OnAttackRangeEnter(Collider collider)
    {
        if(collider.CompareTag("Player") && _status.state==MobEnemy.StatueEnum.Moveable)
        {
        // Debug.Log("攻撃範囲内");
            dic=Vector3.Dot(gameObject.transform.forward.normalized,(collider.gameObject.transform.position-gameObject.transform.position).normalized);
            //ダメージ状態なら何もしない
            if(_status.state==MobEnemy.StatueEnum.Damage)
            {
                return;
            } 
            //内積が0.5以下(60度以下だと範囲外で攻撃しない)
            else if(dic<0.7f)
            {
                Debug.Log("範囲外");
                return;
            }
            //何事もなければ攻撃
            else    AttackIF();
        }
    }
    //攻撃可能判断
    protected virtual void AttackIF()
    {
        _status.GOTOAttackIF();
    }
    //攻撃の始まり
    protected virtual void OnAttackStart()
    {
        attackcollider.enabled=true;
    }
    //攻撃の終わり
    public virtual void OnAttackFinished()
    {
        attackcollider.enabled=false;
        // Debug.Log("攻撃終わり");
        StartCoroutine(enemy.Cooldown());
    }
}
