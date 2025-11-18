using UnityEngine;

public class DragonAttackBehaviour : StateMachineBehaviour
{
    // アニメーション再生開始時
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 攻撃中などの初期処理が必要ならここに書く
        var boss = animator.GetComponent<DragonBossController>();
        if (boss != null)
        {
            Debug.Log("RangeAttack 開始");
        }
    }

    // アニメーション終了時（抜けた瞬間）
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var boss = animator.GetComponent<DragonBossController>();
        if (boss != null)
        {
            Debug.Log("RangeAttack 終了 → CoolDownへ遷移");
            boss.OnAttackEnd(); // DragonBossControllerに用意した関数を呼ぶ
        }
    }
}
