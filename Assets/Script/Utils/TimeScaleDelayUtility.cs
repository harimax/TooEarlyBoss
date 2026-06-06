using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Time.timeScale を一時変更して待機する共通処理。
/// 演出中にオブジェクトが破棄された場合は、呼び出し側へ失敗を返す。
/// </summary>
public static class TimeScaleDelayUtility
{
    /// <summary>
    /// 指定した timeScale に変更してから、実時間ベースで待機。
    /// </summary>
    /// <param name="temporaryTimeScale">待機中に設定する timeScale。</param>
    /// <param name="delayMilliseconds">待機ミリ秒。</param>
    /// <param name="token">破棄時キャンセル用トークン。</param>
    /// <returns>最後まで待機できた場合は true。</returns>
    public static async UniTask<bool> WaitWithTemporaryScaleAsync(
        float temporaryTimeScale,
        int delayMilliseconds,
        CancellationToken token)
    {
        Time.timeScale = temporaryTimeScale;

        try
        {
            await UniTask.Delay(delayMilliseconds, ignoreTimeScale: true, cancellationToken: token);
            return true;
        }
        catch (System.OperationCanceledException)
        {
            Time.timeScale = 1f;
            return false;
        }
    }
}
