using UnityEngine;

/// <summary>
/// ポーズ入力と時間制御、ビュー更新を接続するクラス。
/// 接着層のみに責務を限定する。
/// </summary>
public class PauseMenuPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PauseInputHandler inputHandler;
    [SerializeField] private PauseStateController pauseStateController;
    [SerializeField] private PauseMenuView pauseMenuView;

    [Header("Lifetime")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    private void Awake()
    {
        if (!dontDestroyOnLoad) return;
    }

    private void OnEnable()
    {
        if (inputHandler != null)
        {
            inputHandler.PauseRequested += HandlePauseRequested;
        }

        if (pauseStateController != null)
        {
            pauseStateController.PauseStateChanged += HandlePauseStateChanged;
        }
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.PauseRequested -= HandlePauseRequested;
        }

        if (pauseStateController != null)
        {
            pauseStateController.PauseStateChanged -= HandlePauseStateChanged;
        }
    }

    /// <summary>
    /// 入力を受け取ったときにポーズをトグルする。
    /// </summary>
    private void HandlePauseRequested()
    {
        if (pauseStateController == null) return;

        pauseStateController.TogglePause();
    }

    /// <summary>
    /// ポーズ状態に応じてビューを更新する。
    /// </summary>
    private void HandlePauseStateChanged(bool paused)
    {
        if (pauseMenuView == null) return;

        if (paused)
        {
            pauseMenuView.Show();
            pauseMenuView.Refresh();
            return;
        }

        pauseMenuView.Hide();
    }
}
