using UnityEngine;

public class DragonAttackRangeRotationBehaviour : StateMachineBehaviour
{
    [SerializeField] float faceDuration = 0.14f; // プレイヤーを向き続ける時間（秒）
    private float elapsed;
    private DragonBossController boss;
    [SerializeField] private float turnSpeed = 10f;         // 向き合わせスピード

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        boss = animator.GetComponentInParent<DragonBossController>();
        Debug.Log(boss);
        elapsed = 0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (boss == null) return;

        // faceDuration 経過まではプレイヤー方向へ向き続ける
        if (elapsed < faceDuration)
        {
            boss.FacePlayerOnlyYaw(turnSpeed); // 既存の回頭関数を利用
            elapsed += Time.deltaTime;
        }
        // 経過後は何もしない＝向きを固定
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // アニメーション終了後に必要があれば初期化処理を書く
        elapsed = 0f;
    }
}
