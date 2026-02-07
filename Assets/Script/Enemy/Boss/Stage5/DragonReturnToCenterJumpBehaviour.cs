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
    private bool shouldReturnToCenter;
    // 毎フレーム transform 取得しないためのキャッシュ
    private Transform cachedTransform;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var boss = animator.GetComponentInParent<DragonBossController>();
        if (boss == null)
        {
            // 参照取得失敗時は安全側で処理しない
            shouldReturnToCenter = false;
            cachedTransform = null;
            return;
        }

        // ステート開始時に「戻す必要があるか」を確定しておく
        shouldReturnToCenter = boss.ShouldReturnToCenter();
        cachedTransform = boss.transform;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        shouldReturnToCenter = false;
        cachedTransform = null;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 戻す必要がない、または参照がない場合は何もしない
        if (!shouldReturnToCenter || cachedTransform == null) return;

        var boss = animator.GetComponentInParent<DragonBossController>();
        if (boss == null) return;

        Vector3 targetPosition = boss.StageCenterPosition;
        Vector3 currentPosition = cachedTransform.position;
        // 既に到着しているなら移動終了
        if (Vector3.Distance(currentPosition, targetPosition) <= arriveDistance) return;

        // Y回転のみで中央方向へ向ける
        Vector3 direction = targetPosition - currentPosition;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }

        // ジャンプステート中に中央へ向かって等速で位置補正
        Vector3 nextPosition = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);
        cachedTransform.position = nextPosition;
    }
}
