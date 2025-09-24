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

        try
        {
            await UniTask.Delay(5000, cancellationToken: cts.Token); // 3秒待つ
            if (player != null && player.parent == transform) // まだ乗っていれば
            {
                await TreeBossController.GetInstance().OnActiveBarriar();
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
