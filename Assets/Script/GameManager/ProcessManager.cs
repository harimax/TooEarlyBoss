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
    private const string mainSceneTitle = "BattleScene";
    [SerializeField] private int currentCycle = 1;// 現在のサイクル数
    [SerializeField] private int maxCycle = 5;// 現在のサイクル数
    public int currentBattleIndex = 1;// サイクル内の戦闘インデックス
    public int totalTurns = 14; // 各サイクルの総ターン数（初期値）

    void Awake()
    {
        // シングルトン処理
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

    }
    //現在の出現する敵の数を返す関数
    public int GetEnemyCountForCurrentBattle()
    {
        return currentCycle + currentBattleIndex; // 例：サイクル1→1,2,3体
    }
    //ボスを倒したときに次のサイクルに移る関数
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
    //敵の出現数を加算するメソッド
    public void IncreaseEnemyCount()
    {
        currentBattleIndex++;
    }
    //修行・戦闘シーンに移る時のメソッド
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
