using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Game.Boss;
/**
 * タイタンボスの「突進攻撃」と「向き直り」を担当するクラス
 * - Sprint のアニメーション遷移待ち
 * - 一定時間だけ突進し、休憩時間を取る
 * - SprintCollider の ON/OFF
 */
public class TitanBossSprintAttack : MonoBehaviour
{
    [Header("Sprint Settings")]
    [SerializeField] private float faceTime = 0.5f;      //  向き直り（ため）時間
    [SerializeField] private float minSprintTime = 1.0f; //  突進の最大継続時間（秒）
    [SerializeField] private float maxSprintTime = 1.5f; //  突進の最大継続時間（秒）
    [SerializeField] private float restTime = 3.5f;      //  休憩時間（秒）
    [SerializeField] private Collider sprintCollider; // 突進攻撃の当たり判定
    [SerializeField] private float turnSpeed = 10f;         // 向き合わせスピード
    private Transform player;
    private Animator animator;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = gameObject.GetComponent<Animator>();
    }
    /// <summary>
    /// 一定時間プレイヤー方向に向き続ける（Yawのみ）
    /// </summary>
    public async UniTask FacePlayerForSeconds(float duration, CancellationToken ct = default)
    {
        float t = 0f;

        while (t < duration && !ct.IsCancellationRequested)
        {
            BossUtilities.FaceTargetYaw(transform, player, turnSpeed);
            t += Time.deltaTime;
            await UniTask.Yield();
        }
    }

    /// <summary>
    /// 突進攻撃 1 回分を実行する（終了まで待つ）
    /// </summary>
    public async UniTask SprintAttackAsync(CancellationToken ct)
    {
        if (!player) return;

        // --- 突進準備 ---
        //向く（ため）
        await FacePlayerForSeconds(faceTime, ct);
        if (ct.IsCancellationRequested) return;

        // 攻撃アニメーション開始
        animator.SetTrigger("AttackSprint");

        // PreDash → dashAttack への遷移を待つ
        AnimatorStateInfo state;
        do
        {
            await UniTask.Yield();
            state = animator.GetCurrentAnimatorStateInfo(0);
        }
        while (!state.IsName("dashAttack") && !ct.IsCancellationRequested);

        if (ct.IsCancellationRequested) return;

        float sprintTime = UnityEngine.Random.Range(minSprintTime, maxSprintTime);
        float elapsed = 0f;

        if (sprintCollider) sprintCollider.enabled = true;

        // 一定時間だけ突進（移動自体はアニメーションに任せる想定）
        while (elapsed < sprintTime && !ct.IsCancellationRequested)
        {
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }
        if (sprintCollider) sprintCollider.enabled = false;

        // 突進終了トリガ
        animator.SetTrigger("FinishSprint");

        // 突進後の休憩
        if (!ct.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(restTime), cancellationToken: ct);
        }
    }
    /// <summary>
    /// 即座に突進を止める（死亡など）
    /// </summary>
    public void StopImmediately()
    {
        if (sprintCollider) sprintCollider.enabled = false;
    }
}
