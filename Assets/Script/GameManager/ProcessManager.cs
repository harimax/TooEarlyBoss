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
    private const string mainScene = "BattleScene";
    private const string titleScene = "TitleScene";
    private const string GameClear = "GameClearScene";
    [SerializeField] private ProcessSetting processSetting;
    private int currentCycle;
    private int currentBattleIndex;
    private int totalTurns;
    private int maxCycle;
    // シーン遷移後にコントローラを再有効化する必要があるかどうか
    private bool pendingEnableController;
    public int CurrentCycle //外部に公開するゲームサイクル
    {
        get { return currentCycle; }
        set { currentCycle = value; }
    }
    public int TotalTurns //外部に公開するターン数
    {
        get { return totalTurns; }
        set { totalTurns = value; }
    }
    void Awake()
    {
        // シングルトン処理
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //設定したパラメータを読み込む
        currentCycle = processSetting.startCycle;
        maxCycle = processSetting.maxCycle;
        currentBattleIndex = processSetting.baseEnemyCount;
        totalTurns = processSetting.totalTurnsPerCycle;
        Debug.Log($"サイクル{currentCycle}開始" + $"敵の初期数{currentBattleIndex}" + $"ターン数{totalTurns}");
        // シーン読み込み完了タイミングでプレイヤーコントローラを復帰させるために登録
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        // シーンイベントの購読解除（重複実行を防ぐ）
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }
    //現在の出現する敵の数を返す関数
    public int GetEnemyCountForCurrentBattle()
    {
        return currentCycle + currentBattleIndex; // 例：サイクル1→1,2,3体
    }
    //ボスを倒したときに次のサイクルに移る関数
    public void GoToNextCycle()
    {
        fade = FindAnyObjectByType<Fade>();

        currentCycle++;
        currentBattleIndex=processSetting.baseEnemyCount+currentCycle - 1;
        //すべてのボスを倒したらゲームクリアシーンへ
        if (currentCycle > maxCycle)
        {
            LoadScene(GameClear);
            return;
        }
        //次のサイクルへ移行
        else
        {
            Debug.Log($"サイクル{currentCycle}開始");
            LoadScene(mainScene);
        }
    }
    //現在のサイクルを再開するメソッド
    public void RestartCurrentCycle()
    {
        fade = FindAnyObjectByType<Fade>();

        Debug.Log($"サイクル{currentCycle}再開");
        LoadScene(mainScene);
    }
    //タイトルシーンに戻るメソッド
    public void ReturnToTitleScene()
    {
        fade = FindAnyObjectByType<Fade>();

        Debug.Log("タイトルシーンに戻る");
        PlayerPrefs.DeleteKey("PlayerParamSave");
        PlayerPrefs.Save();
        DestroyPlayerAndManagers();// プレイヤーとマネージャーを破壊
        LoadScene(titleScene);
    }
    //敵の出現数を加算するメソッド
    public void IncreaseEnemyCount()
    {
        currentBattleIndex++;
    }
    //修行・戦闘シーンに移る時のメソッド
    private void LoadScene(string sceneName)
    {
        // シーン遷移直前にコントローラを停止して、移動/補正が走らないようにする
        DisablePlayerControllerBeforeSceneLoad();
        if (Time.timeScale != 1f)
        {
            Time.timeScale = 1f;
        }
        if (fade)
        {
            fade.FadeOut(1f, () => SceneManager.LoadScene(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 事前に停止したコントローラがある場合のみ復帰処理を行う
        if (!pendingEnableController) return;

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<Invector.vCharacterController.vThirdPersonController>();
            if (controller != null)
            {
                controller.enabled = true;
            }
        }
        // 復帰処理が完了したのでフラグを下ろす
        pendingEnableController = false;
    }

    private void DisablePlayerControllerBeforeSceneLoad()
    {
        // 遷移前のプレイヤーを探してコントローラを停止する
        var player = GameObject.FindWithTag("Player");
        var capsuleCollider = player.GetComponent<CapsuleCollider>();
        if (player == null) return;

        var controller = player.GetComponent<Invector.vCharacterController.vThirdPersonController>();
        if (controller == null) return;

        // コントローラを無効化して入力/接地補正を停止する
        controller.enabled = false;
        // 遷移後に再有効化するためのフラグを立てる
        pendingEnableController = true;
        if (capsuleCollider.enabled==false)
        {
            capsuleCollider.enabled = true;
        }
    }
    private void DestroyPlayerAndManagers()
    {
        // プレイヤー破壊
        var player = GameObject.FindWithTag("Player");
        if (player != null) Destroy(player);

        // ★ ProcessManager を破壊（自分自身ならここで消える）
        if (Instance != null)
            Destroy(Instance.gameObject);
    }
}
