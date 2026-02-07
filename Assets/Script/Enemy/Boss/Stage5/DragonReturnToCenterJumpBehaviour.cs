using UnityEngine;

public class DragonReturnToCenterJumpBehaviour : StateMachineBehaviour
{
    // 中央へ戻るときの移動速度
    [SerializeField] private float moveSpeed = 8f;
    // この距離まで近づいたら到着扱い
    [SerializeField] private float arriveDistance = 0.5f;
    // 中央へ向く回頭速度
    [SerializeField] private float turnSpeed = 10f;

    // OnStateEnter で復帰が必要か判定して、Update で使い回す
    // 毎フレーム transform 取得しないためのキャッシュ
    private Transform cachedTransform;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var boss = animator.GetComponentInParent<DragonBossController>();
        if (boss == null)
        {
            // 参照取得失敗時は安全側で処理しない
            cachedTransform = null;
            return;
        }

        // ステート開始時に「戻す必要があるか」を確定しておく
        cachedTransform = boss.transform;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        cachedTransform = null;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 戻す必要がない、または参照がない場合は何もしない
        if (cachedTransform == null) return;

        var boss = animator.GetComponentInParent<DragonBossController>();
        var bossAnimator = animator.GetComponentInParent<Animator>();
        if (boss == null) return;

        Vector3 targetPosition = boss.StageCenterPosition;
        bossAnimator.MatchTarget(targetPosition, Quaternion.identity, AvatarTarget.RightFoot, new MatchTargetWeightMask(Vector3.one, 0f), 0.12f,
            0.73f
);


        Debug.Log("中央へ戻るジャンプ移動中...");
    }
}
