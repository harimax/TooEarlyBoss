using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Enemy_Attack : MonoBehaviour
{
    protected MobEnemy status;
    private ChaseEnemy chaseEnemy;

    [SerializeField] public Collider attackCollider;
    // [SerializeField] protected float attackcooldown=0.7f;
    private float directionDot;
    // Start is called before the first frame update
    private void Awake()
    {
        status = GetComponent<MobEnemy>();
        chaseEnemy = GetComponent<ChaseEnemy>();
    }

    /// <summary>
    /// 攻撃範囲にプレイヤーが入ったとき呼ばれる
    /// </summary>
    public virtual void OnAttackRangeEnter(Collider collider)
    {
        if (!collider.CompareTag("Player") || status.State != MobEnemy.StateEnum.Chase)
            return;

        directionDot = Vector3.Dot(transform.forward.normalized,
                                   (collider.transform.position - transform.position).normalized);

        if (status.State == MobEnemy.StateEnum.Damage)
            return;

        if (directionDot < 0.7f)
            return;

        TryAttack();
    }
    /// <summary>
    /// 攻撃できるかのチェック
    /// </summary>
    protected virtual void TryAttack()
    {
        status.TryAttack();
    }
    /// <summary>
    /// 攻撃開始時に呼ばれる
    /// </summary>
    protected virtual void OnAttackStart()
    {
        attackCollider.enabled = true;
    }
    //攻撃の終わり
    /// <summary>
    /// 攻撃終了時に呼ばれる
    /// </summary>
    public virtual void OnAttackFinished()
    {
        attackCollider.enabled = false;

        // async void は例外処理が難しいので本当は避けたいが、
        // イベントハンドラ的に「終わったらすぐ待機」する場合は許容される
        if (chaseEnemy != null)
        {
            chaseEnemy.Cooldown().Forget();
        }

    }
}
