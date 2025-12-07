using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameClearSceneManager : MonoBehaviour
{
    private Fade fade;
    [SerializeField] private Button ReturnTitleSceneButton;
    [SerializeField] private Button QuitGameButton;
    private const string titleSceneTitle = "TitleScene";
    void Start()
    {
        ReturnTitleSceneButton.onClick.AddListener(OnReturnTitleSceneButtonClicked);
        QuitGameButton.onClick.AddListener(OnQuitGameButtonClicked);
    }

    private void OnReturnTitleSceneButtonClicked()
    {
        var processManager = FindAnyObjectByType<ProcessManager>();
        if (processManager != null)
        {
            processManager.ReturnToTitleScene();
        }
        else
        {
            PlayerPrefs.DeleteKey("PlayerParamSave");
            PlayerPrefs.Save();
            DestroyPlayerAndManagers();// プレイヤーとマネージャーを破壊
            SceneManager.LoadScene(titleSceneTitle);
        }
    }
    //ゲーム終了ボタンが押されたときの処理
    private void OnQuitGameButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
        Application.Quit();//ゲームプレイ終了
#endif
    }
    private void DestroyPlayerAndManagers()
    {
        // プレイヤー破壊
        var player = GameObject.FindWithTag("Player");
        if (player != null) Destroy(player);

        // ★ ProcessManager を破壊（自分自身ならここで消える）
        if (ProcessManager.Instance != null)
            Destroy(ProcessManager.Instance.gameObject);
    }
}

