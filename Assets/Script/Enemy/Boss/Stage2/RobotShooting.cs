using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.AI;
using Invector;

public class RobotShooting : MonoBehaviour
{
    private Transform player;
    [SerializeField] private GameObject ballPrefab;
    [Header("Attack Settings")]
    [SerializeField] private float attackInterval = 5f;   // 攻撃間隔（秒）
    [SerializeField] private float projectileSpeed = 12f; // 弾速
    [SerializeField] private float fireAngleLimit = 75f;  // 左右制限角度
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private float _timer;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        _timer = attackInterval;
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            BallShotAsync().Forget();   // ← 実行
            _timer = attackInterval;    // 次の発射までのインターバル
        }
    }
    private async UniTask BallShotAsync()
    {

        FireAtPlayer(this.transform);
        // 発射後ちょっと待つ（演出やディレイ用）
        // 最低限 1 フレームだけ待ってガード解除（同フレーム連打防止）
        await UniTask.Yield(PlayerLoopTiming.Update);
    }
    /// <summary>
    /// プレイヤーが範囲内なら弾を発射
    /// </summary>
    private void FireAtPlayer(Transform muzzle)
    {
        if (player == null || ballPrefab == null) return;

        // プレイヤー方向（XZ平面のみ）
        Vector3 direction = player.position - muzzle.position;
        var checkDirection = direction;
        checkDirection.y = 0f;
        direction.y += 0.3f;
        direction.Normalize();
        checkDirection.Normalize();

        // 正面ベクトル（XZ平面のみ）
        Vector3 forward = muzzle.forward;
        forward.y = 0f;
        forward.Normalize();

        // 水平角度を
        float angle = Vector3.SignedAngle(forward, checkDirection, Vector3.up);
        //プレイヤーが範囲内にいるかチェックする
        if (Mathf.Abs(angle) <= fireAngleLimit)
        {
            // 発射
            GameObject ball = Instantiate(ballPrefab, muzzle.position, Quaternion.identity);
            if (ball.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = direction * projectileSpeed;
            }
            // Debug.Log($"弾を発射！（角度:{angle:F1}°）");
        }
        else
        {
            // Debug.Log($"プレイヤーは発射範囲外（角度:{angle:F1}°）");
        }
    }
}
