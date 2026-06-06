using UnityEngine;

/**
 * タイタンボスの「飛び道具」を担当するクラス
 * - ジャンプ攻撃（扇状に弾をばらまく）
 * - 岩投げ（プレイヤーの子オブジェクト Target を狙う）
 */
public class TitanBossRangeAttack : MonoBehaviour
{
    [Header("Jump Attack")]
    [SerializeField] private GameObject ballPrefab;
    [Header("Rock Throw")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform shotPos;
    [SerializeField] private float shotSpeed = 15f;
    [SerializeField] private string targetTag = "Target";
    private Transform player;
    private Transform targetPoint;
    private Vector3 cachedTargetPos;

    private void Awake()
    {
        player = PlayerLocator.FindTransform();
        CacheTargetPoint();
    }

    /// <summary>
    /// ジャンプ攻撃：扇状に飛び道具を発射する
    /// </summary>
    public void JumpAttack()
    {
        if (!ballPrefab) return;

        float[] angles = { -150f, -120f, -90f, -60f, -30f, 0f, 30f, 60f, 90f, 120f, 150f }; // 左から右へ角度を振る
        Vector3 baseForward = -transform.right; // 正面方向（調整済み前提）
        Vector3 up = transform.up;

        // 各角度に飛び道具を発射
        foreach (float angle in angles)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, up);
            Vector3 dir = rotation * baseForward;

            GameObject ball = Instantiate(ballPrefab, transform.position, Quaternion.identity);
            ball.GetComponent<Rigidbody>().linearVelocity = dir.normalized * 10f;
        }
    }

    /// <summary>
    /// 岩投げ：プレイヤーの Target 子を狙って発射
    /// （アニメーションイベントから呼ばれる前提）
    /// </summary>
    public void RockThrowAttack()
    {
        if (!rockPrefab) return;
        // 発射位置と向きの指定
        Vector3 origin = shotPos.position;
        Vector3 target = GetTargetPosition();
        Vector3 direction = (target - origin).normalized;

        var shot = Instantiate(rockPrefab.gameObject, origin, shotPos.rotation);
        shot.GetComponent<Rigidbody>().linearVelocity = direction * shotSpeed;
    }

    /// <summary>
    /// プレイヤーの子オブジェクトのうち targetTag を持つ Transform の位置を返す
    /// </summary>
    private Vector3 GetTargetPosition()
    {
        if (!targetPoint)
        {
            CacheTargetPoint();
        }

        if (targetPoint)
        {
            cachedTargetPos = targetPoint.position;
        }

        return cachedTargetPos;
    }

    /// <summary>
    /// ターゲット用子 Transform をキャッシュ
    /// </summary>
    private void CacheTargetPoint()
    {
        // Player配下のTarget探索はPlayerLocatorに寄せ、同じ探索処理を各攻撃クラスへ増やさない。
        if (PlayerLocator.TryFindChildWithTag(player, targetTag, out var target))
        {
            targetPoint = target;
        }
    }
}
