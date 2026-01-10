using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class BattleManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject tempButton;
    [SerializeField] private SkillSelectUI battleUI;
    [SerializeField] private BattlePlayerController battlePlayerController;
    [SerializeField] private BattleUIController battleUIController;
    private bool isPlayerDeathListenerRegistered = false;
    private int enemyCount;
    private bool isMissionActive = false;
    private vThirdPersonController vPersonController;
    /// <summary>
    /// ゲーム開始時に呼ばれるメソッド　初期設定
    /// </summary>
    void Awake()
    {
        battleUIController.ResetText();
        //ゲームマネージャーから各種のコンポーネントを取得
        vPersonController = player.GetComponent<vThirdPersonController>();
    }

    /// <summary>
    /// プレイヤーの死亡イベントを受け取ったときの処理
    /// </summary>
    /// <param name="deadObject">死亡したプレイヤー</param>
    private void HandlePlayerDead(GameObject deadObject)
    {
        FailedMission().Forget();
    }

    /// <summary>
    /// プレイヤーの死亡イベントをOnDeadに登録する
    /// </summary>
    private void SubscribePlayerDeathEvent()
    {
        if (vPersonController == null || isPlayerDeathListenerRegistered)
        {
            return;
        }

        // 念のため同じリスナーを削除してから再登録する
        vPersonController.onDead.RemoveListener(HandlePlayerDead);
        vPersonController.onDead.AddListener(HandlePlayerDead);
        isPlayerDeathListenerRegistered = true;
    }

    /// <summary>
    /// プレイヤーの死亡イベントからリスナーを解除する
    /// </summary>
    private void UnsubscribePlayerDeathEvent()
    {
        if (vPersonController == null || !isPlayerDeathListenerRegistered)
        {
            return;
        }
        vPersonController.onDead.RemoveListener(HandlePlayerDead);
        isPlayerDeathListenerRegistered = false;
    }

    /// <summary>
    /// ミッションが開始されたときに起動するスクリプト 敵の数を数える
    /// </summary>
    public void StartMission()
    {
        // プレイヤーオブジェクトを探してアタッチする
        if (player == null)
        {
            player = GameObject.FindWithTag("Player"); // "Player" タグを利用
            vPersonController = player.GetComponent<vThirdPersonController>();
        }
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        Debug.Log("敵の数" + enemyCount);
        isMissionActive = true;
        ProcessManager.Instance.IncreaseEnemyCount();
        TrianingButton.Instance.DecreaseTurn();
        TrianingButton.Instance.UpdateUI();

        // OnDead イベントの重複登録を防ぎつつ監視を開始する
        SubscribePlayerDeathEvent();
    }

    /// <summary>
    /// ミッションが終わった際の処理
    /// </summary>
    /// <param name="isSuccess"></param>
    public async UniTask ClearMission()
    {
        Debug.Log("戦闘クリア");
        if (!isMissionActive) return;

        battleUIController.ShowClear();
        //クリアするときにスロー演出
        Time.timeScale = 0.5f;
        await UniTask.Delay(2000);
        Time.timeScale = 1.0f;
        isMissionActive = false;
        UnsubscribePlayerDeathEvent();
        battlePlayerController.DisableMovePlayer();
        battlePlayerController.ResetPlayerRigidbody();
        SelectSkillCard();
        battleUIController.ResetText();
    }
    /// <summary>
    /// ミッションが失敗したときこれはDeadイベントで呼び出される
    /// </summary>
    public async UniTask FailedMission()
    {
        isMissionActive = false;
        UnsubscribePlayerDeathEvent();
        battleUIController.ShowFailed();

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        await WaitForAnimationEndAsync(); // アニメーション終了後に実行
    }
    /// <summary>
    /// 敵を倒した際に呼び出されるメソッド mobEnemyのonDieをから呼び出される
    /// </summary>
    public void OnEnemyDefeated()
    {
        enemyCount--;
        Debug.Log("敵撃破！残り: " + enemyCount);
        if (enemyCount <= 0)
        {
            ClearMission().Forget(); // 成功
        }
    }
    private async UniTask WaitForAnimationEndAsync()
    {
        await UniTask.Delay(2500);
        battlePlayerController.ResetPlayerToInitialPosition();
        battleUIController.ResetText();
    }
    //プレイヤーを初期位置に戻す処理
    public void ReturnPlayerToInitialPosition()
    {
        isMissionActive = false;
        tempButton.SetActive(false);
        battlePlayerController.ResetPlayerToInitialPosition();
    }
    //ミッションクリア後にスキルカードを表示させる処理
    public void SelectSkillCard()
    {
        //スキルカードを選択する(仮にボタンにする)
        battleUI.ShowRandomSkillChoices();
    }
}
