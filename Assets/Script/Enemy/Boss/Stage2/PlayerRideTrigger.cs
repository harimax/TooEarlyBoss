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
        if (other.transform.parent != transform && other.transform.CompareTag("Player") && other.GetComponent<Invector.vCharacterController.vCharacter>() != null)
        {
            other.transform.parent = transform;
            StartEjectTimer(other.transform);
        }
    }
    //プレイヤーが降りると子を外す
    void OnTriggerExit(Collider other)
    {
        if (other.transform.parent == transform && other.transform.CompareTag("Player"))
        {
            other.transform.parent = null;
            other.transform.eulerAngles = new Vector3(0, other.transform.eulerAngles.y, 0);
        }
    }
    // タイマー開始
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

        // ★ await 前に transform をキャッシュ（これが重要）
        Transform platform = this.transform;

        try
        {
            // 5秒待つ（コメントは3秒と書いてますが値は5000ms）
            await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: token);

            // 以降は “Unity 的 null” ガードを必ず通す
            if (!player) return;    // player が Destroy 済み
            if (!platform) return;  // 自分（のTransform）が Destroy 済み

            // まだ乗っていれば実行（transform を使わず platform を使う）
            if (player.parent == platform)
            {
                // GetInstance() 側も null ガードしたほうが安全
                var boss = TreeBossController.GetInstance();
                if (boss != null)
                {
                    await boss.OnActiveBarriar(); // ここが UniTask なら await、voidならそのまま呼ぶ
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("エラー起きたけど無視!!!!!");
        }
        finally
        {
            CancelTimer(); // 後始末
        }

    }
    // タイマー停止
    private void CancelTimer()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }
}
