using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClearButton : MonoBehaviour
{
    [SerializeField] private Button nextCycleButton;

    void Start()
    {
        var gameCycleManager = FindObjectOfType<ProcessManager>();
        if (gameCycleManager != null && nextCycleButton != null)
        {
            Debug.Log("ボタンアタッチ");
            nextCycleButton.onClick.AddListener(gameCycleManager.GoToNextCycle);
        }
        else
        {
            Debug.LogWarning("GameCycleManager または Button が見つかりませんでした。");
        }
    }
}
