using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MobEnemy : MonoBehaviour
{
    //状態定義
    public enum StateEnum
    {
        Patrol,
        Chase,// 移動可能（通常状態）
        Attack,  // 攻撃中
        Damage,// ダメージリアクション中
        Die// 死亡状態
    }
    public StateEnum State { get; protected set; } = StateEnum.Patrol;
    protected Animator animator;
    private bool canAttack = true; // 攻撃可能フラグ（連続攻撃防止用）
    // Start is called before the first frame update
    //アニメーターを取得
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }
    //死ぬ処理
    protected virtual void OnDie()
    {
        //死亡状態ならなにもしない
        if (State == StateEnum.Die) return;

        State = StateEnum.Die;
        animator.SetTrigger("Dead");
        FindObjectOfType<MissionManager>().OnEnemyDefeated(); // ミッションに通知

    }
    //攻撃判断処理
    public void TryAttack()
    {
        if (State == StateEnum.Chase && canAttack)
        {
            State = StateEnum.Attack;
            canAttack = false;  // 攻撃中は再度攻撃できないようにする
            animator.SetTrigger("Attack");
            // Debug.Log("攻撃開始");
        }
    }
    // 通常状態に戻る処理（攻撃後やダメージ後）
    public void ReturnToNormal()
    {
        if (State == StateEnum.Die)
            return;

        State = StateEnum.Patrol;
        canAttack = true;
        // Debug.Log("IDLEに戻ります");
    }
    //ダメージリアクション処理
    public virtual void DamageReaction()
    {
        if (State == StateEnum.Die)
            return;

        State = StateEnum.Damage;
        canAttack = false;  // ダメージ中は攻撃無効
        animator.SetTrigger("Damage");
         StartCoroutine(WaitForDamageRecovery());
    }
    // ダメージ後に一定時間待って通常状態に戻す処理
    private IEnumerator WaitForDamageRecovery()
    {
        // ダメージリアクションが終わるまで待つ（例えば1秒）
        yield return new WaitForSeconds(1f);
        ReturnToNormal();
    }

}

