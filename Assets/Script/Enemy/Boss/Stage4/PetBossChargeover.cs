using UnityEngine;
using Cysharp.Threading.Tasks;
public class PetBossChargeMover : MonoBehaviour
{
    [Header("Charge Parameters")]
    [SerializeField] float chargeSpeed = 12.0f;     // 突進速度（m/s）
    [SerializeField] float overshootDistance = 5.0f;// プレイヤー位置をどれだけ通り過ぎるか（m）
    [SerializeField] float maxChargeTime = 4.0f; // ③ 突進の最大継続時間（秒）
    [SerializeField] bool lockYPosition = true;     // 地面がフラットならtrueでY固定
    public async UniTask ChargeAsync(Transform player, Animator animator, Collider sprintCollider)
    {
        Vector3 startPos = transform.position;
        Vector3 toPlayer = player.position - startPos;
        toPlayer.y = 0f;

        Vector3 chargeDir = toPlayer.sqrMagnitude > 0.0001f
            ? toPlayer.normalized
            : transform.forward;

        // プレイヤーを overshoot する距離をゴールに
        float targetDistance = toPlayer.magnitude + Mathf.Max(0f, overshootDistance);

        animator?.SetBool("Moveable", true);
        if (sprintCollider) sprintCollider.enabled = true;

        float traveled = 0f;
        float elapsed = 0f;
        float baseY = startPos.y;
        // 突進移動ループ
        while (elapsed < maxChargeTime && traveled < targetDistance)
        {
            float step = chargeSpeed * Time.deltaTime;
            transform.position += chargeDir * step;
            // Y位置固定
            if (lockYPosition)
            {
                var p = transform.position;
                p.y = baseY;
                transform.position = p;
            }

            elapsed += Time.deltaTime;
            traveled += step;
            await UniTask.Yield();
        }
    }
}
