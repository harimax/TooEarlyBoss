using UnityEngine;

/// <summary>
/// Back ボタン入力でポーズ状態を切り替える制御クラス。
/// 時間停止とビューの表示/非表示のみを担当する。
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PauseMenuView pauseMenuView;

    [Header("Input")]
    [SerializeField] private string backButtonName = "Back";
    [SerializeField] private bool allowEscapeKey = true;

    private bool isPaused;
    private float previousTimeScale = 1f;

    private void Update()
    {
        if (!IsPauseInputPressed()) return;

        TogglePause();
    }

    /// <summary>
    /// ポーズ入力が押されたかどうか判定する。
    /// </summary>
    private bool IsPauseInputPressed()
    {
        if (!string.IsNullOrEmpty(backButtonName) && Input.GetButtonDown(backButtonName))
        {
            return true;
        }

        return allowEscapeKey && Input.GetKeyDown(KeyCode.Escape);
    }

    /// <summary>
    /// ポーズ状態を切り替える。
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

    /// <summary>
    /// ゲームをポーズ状態にする。
    /// </summary>
    private void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (pauseMenuView == null) return;

        pauseMenuView.Show();
        pauseMenuView.Refresh();
    }

    /// <summary>
    /// ゲームを再開する。
    /// </summary>
    private void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = previousTimeScale;

        if (pauseMenuView == null) return;

        pauseMenuView.Hide();
    }

    private void OnDisable()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = previousTimeScale;

        if (pauseMenuView != null)
        {
            pauseMenuView.Hide();
        }
    }
}
