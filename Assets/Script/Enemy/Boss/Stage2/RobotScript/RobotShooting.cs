using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.AI;
using Invector;

public class RobotShooting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ballPrefab;
    [Header("Attack Settings")]
    [SerializeField] private float attackInterval = 5f;   // 攻撃間隔（秒）
    [SerializeField] private float projectileSpeed = 12f; // 弾速
    [SerializeField] private float fireAngleLimit = 75f;  // 左右制限角度
    private Transform player;
    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) this.player = player.transform;
        BallShotAsync().Forget();   // ← 実行
    }
    /// <summary>
    /// ボールを定期的に発射する非同期処理
    /// </summary>
    private async UniTask BallShotAsync()
    {
        while (true)
        {
            await UniTask.Delay(5000); // 最初の1フレーム待機
            FireAtPlayer(this.transform);
            await UniTask.Yield(PlayerLoopTiming.Update); // 最低限 1 フレームだけ待ってガード解除（同フレーム連打防止）
        }
    }
    /// <summary>
    /// プレイヤーが範囲内なら弾を発射
    /// </summary>
    private void FireAtPlayer(Transform muzzle)
    {
        if (player == null || ballPrefab == null) return;

        // プレイヤー方向（XZ平面のみ）
        Vector3 direction = player.position - muzzle.position;
        var horizontalDir = new Vector3(direction.x, 0f, direction.z);

        direction.y += 0.3f;
        direction.Normalize();
        horizontalDir.Normalize();

        // 正面ベクトル（XZ平面のみ）
        Vector3 forward = new Vector3(muzzle.forward.x, 0f, muzzle.forward.z).normalized;

        // 水平角度を
        float angle = Vector3.SignedAngle(forward, horizontalDir, Vector3.up);
        //プレイヤーが範囲内にいるかチェックする
        if (Mathf.Abs(angle) > fireAngleLimit) return;
        // 発射
        GameObject ball = Instantiate(ballPrefab, muzzle.position, Quaternion.identity);
        if (ball.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = direction * projectileSpeed;
        }
    }
}
