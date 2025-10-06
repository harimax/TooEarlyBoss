using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController;
using Invector.PlayerController;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class MissionManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private TextMeshProUGUI clearText;
    [SerializeField] Fade fade;
    [SerializeField] private GameObject tempButton;
    [SerializeField] private SkillSelectUI MissionUI;
    private Vector3 InitPosition;
    private StartMission startMission;
    private TrianingButton trianingButton;
    private bool isPlayerDeathListenerRegistered = false;
    private int enemyCount;
    private bool isMissionActive = false;
    vThirdPersonController vPersonController;
    /// <summary>
    /// ゲーム開始時に呼ばれるメソッド　初期設定
    /// </summary>
    void Awake()
    {
        InitPosition = player.transform.position;
        clearText.text = "";
        //ゲームマネージャーから各種のコンポーネントを取得
        GameObject gameManager = GameObject.Find("GameManager");
        startMission = gameManager.GetComponent<StartMission>();
        trianingButton = gameManager.GetComponent<TrianingButton>();
        // skillAcquirer = gameManager.GetComponent<SkillAcquirer>();
        vPersonController = player.GetComponent<vThirdPersonController>();
    }

    void Update()
    {
        Debug.Log("isMissionActive: " + isMissionActive);
        //敵の数でミッションを監視する
        if (isMissionActive && enemyCount <= 0)
        {
            ClearMission(true); // 成功
        }
    }

    /// <summary>
    /// プレイヤーの死亡イベントを受け取ったときの処理
    /// </summary>
    /// <param name="deadObject">死亡したプレイヤー</param>
    private void HandlePlayerDead(GameObject deadObject)
    {
        // 現状は引数を使用しないが、イベントシグネチャに合わせて受け取っておく
        FailedMission();
    }

    /// <summary>
    /// プレイヤーの死亡イベントへ安全に登録する
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
    public void ClearMission(bool isSuccess)
    {
        // Debug.Log("戦闘クリア");
        if (!isMissionActive) return;

        isMissionActive = false;
        UnsubscribePlayerDeathEvent();
        DisableMovePlayer();
        ResetPlayerRigidbody();

        clearText.text = "クリア";
        SelectSkillCard();//元の位置に戻る
    }
    /// <summary>
    /// ミッションが失敗したときこれはDeadイベントで呼び出される
    /// </summary>
    public async void FailedMission()
    {
        isMissionActive = false;
        UnsubscribePlayerDeathEvent();
        clearText.text = "死んだぜ";

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        await WaitForAnimationEndAsync(); // アニメーション終了後に実行
    }
    //ミッションクリア後にスキルカードを表示させる処理
    public void SelectSkillCard()
    {
        //スキルカードを選択する(仮にボタンにする)
        // tempButton.SetActive(true);
        MissionUI.ShowRandomSkillChoices();
    }
    //プレイヤーを初期位置に戻す処理
    public void ReturnPlayerToInitialPosition()
    {
        isMissionActive = false;
        tempButton.SetActive(false);
        clearText.text = "";

        player.transform.position = InitPosition;
        startMission.ReturntTrainingButton();
        trianingButton.SetButtonsInteractable();

        vThirdPersonController vPersonController = player.GetComponent<vThirdPersonController>();
        vPersonController._currentHealth = vPersonController.maxHealth;
        vPersonController.isDead = false;
        DisableMovePlayer();


    }
    //プレイヤーのRigidBodyで移動を止める
    private void ResetPlayerRigidbody()
    {
        var rb = player.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;

        vPersonController.enabled = false;
        vPersonController.enabled = true;
    }
    //プレイヤーを動けなくする
    public void DisableMovePlayer()
    {
        IsPlayerMove.GetInstance().CanMove = false;

    }
    /// <summary>
    /// 敵を倒した際に呼び出されるメソッド mobEnemyのonDieをから呼び出される
    /// </summary>
    public void OnEnemyDefeated()
    {
        enemyCount--;
        Debug.Log("敵撃破！残り: " + enemyCount);
    }
    private async UniTask WaitForAnimationEndAsync()
    {
        await UniTask.Delay(2500);
        ReturnPlayerToInitialPosition();
    }
}
