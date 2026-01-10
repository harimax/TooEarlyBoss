using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;              // ★ 追加

public class PatrolRangedShooter : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("アニメーターの攻撃トリガー名（例：Throw / Shoot / Attack など）")]
    [SerializeField] private string triggerName = "Throw";

    [Header("When")]
    [Tooltip("パトロール中のみトリガーする")]
    [SerializeField] private bool onlyWhenPatrolling = true;

    [Tooltip("トリガー判定の最小/最大間隔（秒）。この範囲でランダムに待ってから判定します")]
    [SerializeField] private Vector2 intervalSeconds = new Vector2(2.5f, 4.0f);

    [Tooltip("判定時に実際にトリガーを発火する確率（0〜1）")]
    [Range(0f, 1f)][SerializeField] private float triggerChance = 0.4f;

    [Header("Safety")]
    [Tooltip("現在のアニメーションが Attack タグの間は新規トリガーを抑止する")]
    [SerializeField] private bool blockWhileAttacking = true;

    [Tooltip("アニメーターの Attack タグ名（必要に応じて変更）")]
    [SerializeField] private string attackTagName = "Attack";

    private Animator animator;
    private MobEnemy status;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform ObjectSpawnPoint;
    // Enable/Disableで止める用
    private CancellationTokenSource _cts;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        status = GetComponent<MobEnemy>();
    }

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();
        // fire-and-forgetで回す（例外はUniTaskのUnobservedExceptionハンドラに流れる）
        Loop().Forget();
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>
    /// ランダム間隔で「今トリガーして良いか？」を判定→OKならトリガーを叩く（UniTask版）
    /// ※ 要求に合わせて private async UniTask で実装
    /// </summary>
    private async UniTask Loop()
    {
        // OnDestroyでも止まるよう、Destroyトークンとリンク
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            _cts.Token,
            this.GetCancellationTokenOnDestroy()
        );
        var ct = linked.Token;

        // 入力値保護
        float min = Mathf.Max(0.1f, Mathf.Min(intervalSeconds.x, intervalSeconds.y));
        float max = Mathf.Max(min, Mathf.Max(intervalSeconds.x, intervalSeconds.y));

        try
        {
            while (!ct.IsCancellationRequested)
            {
                // ランダム待機（コルーチンの WaitForSeconds の代替）
                float wait = UnityEngine.Random.Range(min, max);
                await UniTask.Delay(TimeSpan.FromSeconds(wait), cancellationToken: ct);

                if (ct.IsCancellationRequested) break;

                // 条件1: ステート（パトロール中のみ運用したい場合）
                if (onlyWhenPatrolling && status != null && status.State != MobEnemy.StateEnum.Patrol)
                    continue;

                // 条件2: すでに攻撃再生中なら抑止
                if (blockWhileAttacking && animator != null)
                {
                    var st = animator.GetCurrentAnimatorStateInfo(0);
                    if (st.IsTag(attackTagName))
                        continue;
                }

                // 条件3: 確率判定
                if (UnityEngine.Random.value > triggerChance)
                    continue;

                // 最終：トリガー発火（実ダメージはアニメーションイベント側で行う）
                if (animator != null && !string.IsNullOrEmpty(triggerName))
                    animator.SetTrigger(triggerName);
            }
        }
        catch (OperationCanceledException)
        {
            // Disable/Destroyでのキャンセルは想定内：握りつぶし
        }
    }
    //弾を飛ばずアニメーションイベント
    public void OnBallShotEvent()
    {
        float[] angles = {  -15f, 0f, 15f}; // 左から右へ角度を振る
        // 「正しい発射方向」を補正して取得
        Vector3 baseForward = -ObjectSpawnPoint.right; // ← ここが正しい正面！
        Vector3 up = ObjectSpawnPoint.up;

        foreach (float angle in angles)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, up);
            Vector3 dir = rotation * baseForward;

            GameObject ball = Instantiate(ballPrefab, ObjectSpawnPoint.position, Quaternion.identity);
            ball.GetComponent<Rigidbody>().linearVelocity = dir.normalized * 10f;
        }
    }
}
