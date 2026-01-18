using UnityEngine;

/// <summary>
/// ボス関連の共通ロジックをまとめたユーティリティ。
/// </summary>
///
namespace Game.Boss
{
    public static class BossUtilities
    {
        /// <summary>
        /// ターゲットへYawのみで回頭する（Y軸回転のみ）。
        /// </summary>
        public static void FaceTargetYaw(Transform owner, Transform target, float slerpSpeed)
        {
            if (!owner || !target) return;

            Vector3 direction = target.position - owner.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            owner.rotation = Quaternion.Slerp(owner.rotation, targetRotation, Time.deltaTime * slerpSpeed);
        }

        /// <summary>
        /// owner と target の距離を返す。参照が無い場合は Infinity を返す。
        /// </summary>
        public static float DistanceToTarget(Transform owner, Transform target)
        {
            if (!owner || !target) return float.PositiveInfinity;

            return Vector3.Distance(owner.position, target.position);
        }
    }
}
