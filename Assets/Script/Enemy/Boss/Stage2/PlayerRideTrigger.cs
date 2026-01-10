using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;


public class PlayerRideTrigger : MonoBehaviour
{
    private CancellationTokenSource cts;
    //プレイヤーが乗れば子にする
    void OnTriggerStay(Collider other)
    {
        if (!IsValidPlayer(other)) return;
        StartEjectTimer(other.transform);
    }
    // 弾き飛ばしのタイマー開始
    private async void StartEjectTimer(Transform player)
    {
        CancelTimer(); // 既存のタイマーをキャンセル
        cts = new CancellationTokenSource();
        // 自動キャンセル（自分がDestroyされたら止まる）を連結
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            cts.Token,
            this.GetCancellationTokenOnDestroy()
        );
        var token = linked.Token;

        // await 前に platform Transform をキャッシュしておく（Destroy 安全性のため）
        Transform platform = this.transform;

        try
        {
            // 5秒待つ
            await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: token);

            if (!player || !platform) return; // Destroy 済み

            // GetInstance() 側も null ガードしたほうが安全
            var boss = RobotBossController.GetInstance();
            if (boss != null)
            {
                await boss.OnActiveBarriar(); // ここが UniTask なら await、voidならそのまま呼ぶ
            }
        }
        catch (OperationCanceledException)
        {
             Debug.Log("PlayerRideTrigger: タイマーはキャンセルされました。");
        }
        finally
        {
            CancelTimer(); // 後始末
        }
    }
    /// <summary
    /// >既存のタイマーを停止し、リソースを解放する。
    /// </summary>
    private void CancelTimer()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }
    /// <summary>
    /// トリガの対象になるプレイヤーかどうか判定。
    /// - Player タグ
    /// - Invector のキャラクターコンポーネントを持っている
    /// - まだこの足場の子になっていない
    /// </summary>
    private bool IsValidPlayer(Collider other)
    {
        if (!other.CompareTag("Player")) return false;
        if (other.transform.parent == transform) return false;

        return other.GetComponent<Invector.vCharacterController.vCharacter>() != null;
    }
}
