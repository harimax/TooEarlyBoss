using UnityEngine;

/// <summary>
/// TreeBoss の上半身をプレイヤー方向に向けるクラス。
/// もともとの RotateUpperBody を単一責務化。
/// </summary>
public class RobotBossUuperBodyAimer : MonoBehaviour
{
    [SerializeField] private Transform upperBody; // 上半身ボーン
    private Transform player;
    private void Start()
    {
        player = PlayerLocator.FindTransform();

        if (upperBody == null)
        {
            upperBody = transform;// 自身が UpperBody の場合は self を使う
        }
    }
    private void LateUpdate()
    {
        if (player == null || upperBody == null) return;
        // プレイヤー方向を向く
        upperBody.LookAt(player);

        // 回転角を補正（Z は固定、Y は常に 180 に固定する元の仕様）
        Vector3 euler = upperBody.localEulerAngles;
        euler.z = 0f;
        euler.y = 180f;
        upperBody.localEulerAngles = euler;
    }
}
