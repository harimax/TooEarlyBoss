using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MobEnemy : MonoBehaviour
{
    //状態定義
    public enum StatueEnum
    {
        Moveable,
        Attack,
        Damage,
        Die
    }
    public StatueEnum state=StatueEnum.Moveable;
    protected Animator animator;
    private bool Attack_Limit; //攻撃中に攻撃がさらに呼ばれないようにする条件定数
    // Start is called before the first frame update
    //アニメーターを取得
    protected virtual void Start()
    {
        animator=GetComponent<Animator>();
    }
    //死ぬ処理
    protected virtual void OnDie()
    {
        //死亡状態ならなにもしない
        if(state==StatueEnum.Die)
        {
            return;
        }
        else
        {
        // Debug.Log("死んだ");
        state=StatueEnum.Die;
        animator.SetTrigger("Dead");
        }

    }
    //攻撃判断処理
    public void GOTOAttackIF()
    {
        //Moveable状態以外なら攻撃しない
        if(state!=StatueEnum.Moveable) 
        {
            return;
        }
        else if(Attack_Limit==false)
        {
            state=StatueEnum.Attack;
            Debug.Log("攻撃");
            animator.SetTrigger("Attack");
            Attack_Limit=true;
        }        
    }
    //アイドル状態に戻る処理
    public void GOTONormalIF()
    {
        //死亡状態ならなにもしない
        if(state==StatueEnum.Die) 
        {
            return;
        }
        else
        {
        // Debug.Log("IDLEに戻ります");
        state=StatueEnum.Moveable;
        Attack_Limit=false;
        }
    }
    //ダメージリアクション処理
    public virtual void DamageReaction()
    {
        //死亡状態ならなにもしない
        if(state==StatueEnum.Die) 
        {
            return;
        }
        else
        {
            animator.SetTrigger("Damage");
            state=StatueEnum.Damage;
            Attack_Limit=false;
        }
    }

}

