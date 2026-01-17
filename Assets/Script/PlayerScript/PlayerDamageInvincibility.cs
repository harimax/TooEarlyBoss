using System;
using Invector;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class PlayerDamageInvincibility : MonoBehaviour
{
    // 無敵時間の長さ（秒）
    [SerializeField] private float invincibleDuration = 3f;

    // 参照キャッシュ
    private vHealthController healthController;
    private CancellationTokenSource invincibilityCts;
    private bool wasImmortalBefore;
    private bool isInvincibilityActive;

    // 初期化処理
    private void Awake()
    {
        // 事前にHealthControllerを取得
        healthController = GetComponent<vHealthController>();
    }


    // 有効化時のイベント登録
    private void OnEnable()
    {

        // 未取得の場合は再取得
        if (healthController == null)
        {
            healthController = GetComponent<vHealthController>();
        }


        // ダメージイベントを登録
        if (healthController != null)
        {
            healthController.onReceiveDamage.AddListener(OnDamageInvincible);
        }
        else
        {
            // HealthControllerが無い場合は警告
            Debug.LogWarning("PlayerDamageInvincibility requires vHealthController.", this);
        }
    }

    // 無効化時のイベント解除
    private void OnDisable()
    {

        // リスナー解除
        if (healthController != null)
        {
            healthController.onReceiveDamage.RemoveListener(OnDamageInvincible);
        }

        // 実行中タスクを停止
        if (invincibilityCts != null)
        {
            invincibilityCts.Cancel();
            invincibilityCts.Dispose();
            invincibilityCts = null;
        }

        // 無敵状態をリセット
        ResetInvincibilityState();
    }

    // ダメージを受けたら無敵を開始
    private void OnDamageInvincible(vDamage damage)
    {

        // HealthControllerが無い場合は何もしない
        if (healthController == null)
        {
            return;
        }


        // 初回の無敵開始時だけ元の状態を記録
        if (!isInvincibilityActive)
        {
            wasImmortalBefore = healthController.isImmortal;
        }

        // 無敵状態を有効化
        healthController.isImmortal = true;
        isInvincibilityActive = true;


        // 既存のタスクがあれば止めて再スタート
        if (invincibilityCts != null)
        {
            invincibilityCts.Cancel();
            invincibilityCts.Dispose();
        }

        // 無敵タイマー開始
        invincibilityCts = new CancellationTokenSource();
        StartInvincibilityTimer(invincibilityCts.Token).Forget();
    }


    // 無敵時間の計測
    private async UniTaskVoid StartInvincibilityTimer(CancellationToken cancellationToken)
    {
        // 無敵時間が経過するまで待機
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(invincibleDuration), cancellationToken: cancellationToken);
            // 無敵解除
            ResetInvincibilityState();
        }
        catch (OperationCanceledException)
        {
            // キャンセル時は何もしない
        }
    }


    // 無敵状態の解除と復元
    private void ResetInvincibilityState()
    {

        // 無敵中でなければ何もしない
        if (!isInvincibilityActive)
        {
            return;
        }


        // もともと無敵でなかった場合のみ解除
        if (healthController != null && !wasImmortalBefore)
        {
            healthController.isImmortal = false;
        }

        // 状態をリセット
        isInvincibilityActive = false;
        invincibilityCts?.Cancel();
        invincibilityCts?.Dispose();
        invincibilityCts = null;
        wasImmortalBefore = false;
    }
}
