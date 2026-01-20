using System;
using UnityEngine;

/// <summary>
/// ポーズ状態と Time.timeScale を管理するクラス。
/// 時間制御のみに責務を限定する。
/// </summary>
public class PauseStateController : MonoBehaviour
{
    private bool isPaused;
    private float previousTimeScale = 1f;

    /// <summary>
    /// ポーズ状態が変化したときに呼ばれるイベント。
    /// true = ポーズ中, false = 再開中
    /// </summary>
    public event Action<bool> PauseStateChanged;

    /// <summary>
    /// 現在ポーズ中かどうか。
    /// </summary>
    public bool IsPaused => isPaused;

    /// <summary>
    /// ポーズ状態を切り替える。
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
            return;
        }

        Pause();
    }

    /// <summary>
    /// ポーズ状態にする。
    /// </summary>
    public void Pause()
    {
        if (isPaused) return;

        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        PauseStateChanged?.Invoke(true);
    }

    /// <summary>
    /// ゲームを再開する。
    /// </summary>
    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = previousTimeScale;
        PauseStateChanged?.Invoke(false);
    }

    private void OnDisable()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = previousTimeScale;
        PauseStateChanged?.Invoke(false);
    }
}
