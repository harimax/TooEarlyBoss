using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NormalStageManagaer : MonoBehaviour
{
    public static NormalStageManagaer instance;

    public bool IsGameStarted { get; private set; } = false;
    public bool IsGamecleared { get; private set; } = false;
    // Start is called before the first frame update
    void Awake()
    {
        instance=this;
        Time.timeScale=0f;
    }
    //ゲーム開始メソッド
    public void StartStage()
    {
         Time.timeScale = 1f;
        IsGameStarted = true;
        // UIManager.Instance.HideStartText();
    }
    public void GameClear()
    {
        IsGamecleared=true;
        Time.timeScale=0f;
        // UIManager.Instance.ShowClearText();
    }
}
