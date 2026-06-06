using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System;
using Invector;

public class MobEnemy : MonoBehaviour
{
    //状態定義
    public enum StateEnum
    {
        Patrol, //巡回
        Chase,  //移動可能（通常状態）
        Attack, // 攻撃中
        Damage, // ダメージリアクション中
        Die     // 死亡状態
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
        FindFirstObjectByType<BattleManager>()?.OnEnemyDefeated(); // ミッションに通知

    }
    //攻撃判断処理
    public void TryAttack()
    {
        if (State == StateEnum.Chase && canAttack)
        {
            State = StateEnum.Attack;
            canAttack = false;  // 攻撃中は再度攻撃できないようにする
            animator.SetTrigger("Attack");
            Debug.Log("攻撃開始");
        }
    }
    // 通常状態に戻る処理（攻撃後やダメージ後）
    public void ReturnToNormal()
    {
        if (State == StateEnum.Die)
            return;

        State = StateEnum.Patrol;
        canAttack = true;
        Debug.Log("IDLEに戻ります");
    }
    // 派生クラスから直接呼ばれても通常のダメージリアクションだけを再生する。
    public virtual void DamageReaction()
    {
        PlayDamageReaction();
    }

    protected void PlayDamageReaction()
    {
        if (State == StateEnum.Die)
            return;

        State = StateEnum.Damage;
        canAttack = false;  // ダメージ中は攻撃無効
        animator.SetTrigger("Damage");
        WaitForDamageRecovery().Forget(); // UniTaskで処理を実行
    }

    // HP判定、リアクション抽選、死亡時の遅延破棄をまとめた被ダメージ共通処理。
    // 派生クラス固有のクールダウンやコライダー停止はコールバックで差し込む。
    protected bool ResolveDamageReaction(
        vHealthController healthController,
        float reactionProbability,
        float destroyDelaySeconds,
        Action onReaction = null,
        Action beforeDestroy = null)
    {
        if (healthController == null)
        {
            return false;
        }

        if (healthController.currentHealth > 0)
        {
            if (UnityEngine.Random.value < reactionProbability)
            {
                PlayDamageReaction();
                onReaction?.Invoke();
            }

            return false;
        }

        if (State == StateEnum.Die)
        {
            return true;
        }

        OnDie();
        DestroyAfterDelay(destroyDelaySeconds, beforeDestroy).Forget();
        return true;
    }

    // 死亡直後に無効化したい処理を実行してから、演出用の待ち時間後に敵を消す。
    protected async UniTaskVoid DestroyAfterDelay(float delaySeconds, Action beforeDestroy = null)
    {
        beforeDestroy?.Invoke();
        await UniTask.Delay((int)(delaySeconds * 1000));
        Destroy(gameObject);
    }
    // ダメージ後に一定時間待って通常状態に戻す処理
    private async UniTask WaitForDamageRecovery()
    {
        // ダメージリアクションが終わるまで待つ（例えば1秒）
        await UniTask.Delay(1000);
        ReturnToNormal();
    }

}

