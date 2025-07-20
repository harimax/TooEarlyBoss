using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Threading.Tasks;
public class ProcessManager : MonoBehaviour
{
    private Fade fade;
    public static ProcessManager Instance { get; private set; }
    private const string mainSceneTitle = "DunegonScene";
    [SerializeField] private int currentCycle = 1;
    [SerializeField] private int maxCycle = 5;
    public int currentBattleIndex = 1;
    public int totalTurns = 14;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

    }
    public int GetEnemyCountForCurrentBattle()
    {
        return currentCycle + currentBattleIndex; // 例：サイクル1→1,2,3体
    }
    public void GoToNextCycle()
    {
        fade = FindObjectOfType<Fade>();

        currentCycle++;
        currentBattleIndex = 1;

        if (currentCycle > maxCycle)
        {
            Debug.Log("ゲームクリア");

        }
        else
        {
            Debug.Log($"サイクル{currentCycle}開始");
            // シーン読み込み後イベントを一度だけ登録
            SceneManager.sceneLoaded += OnSceneLoaded;

            // フェード後にシーン遷移
            fade.FadeIn(1f, () => SceneManager.LoadScene(mainSceneTitle));
        }
    }

    public void IncreaseEnemyCount()
    {
        currentBattleIndex++;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // プレイヤーとスタート地点を探す
        GameObject player = GameObject.FindWithTag("Player");
        GameObject startPoint = GameObject.FindWithTag("StartPoint");

        if (player != null && startPoint != null)
        {
            player.transform.position = startPoint.transform.position;
        }

        // イベント登録解除（多重呼び出し防止）
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


}
