using UnityEngine;
using UnityEngine.UI;

public class GameOverButton : MonoBehaviour
{
    [SerializeField] private Button ReturnBattleSceneButton;
    [SerializeField] private Button ReturnTitleSceneButton;
    void Start()
    {
        var gameCycleManager = FindAnyObjectByType<ProcessManager>();
        if (gameCycleManager != null && ReturnBattleSceneButton != null && ReturnTitleSceneButton != null)
        {
            Debug.Log("ボタンアタッチ");
            ReturnBattleSceneButton.onClick.AddListener(gameCycleManager.RestartCurrentCycle);
            ReturnTitleSceneButton.onClick.AddListener(gameCycleManager.ReturnToTitleScene);
        }
        else
        {
            Debug.LogWarning("GameCycleManager または Button が見つかりませんでした。");
        }
    }
}
