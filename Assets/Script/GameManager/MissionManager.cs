using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class MissionManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private TextMeshProUGUI ClearText;
    [SerializeField] Fade fade;
    [SerializeField] private GameObject tempButton;
    [SerializeField] private SkillSelectUI MissionUI;
    private int enemyCount;
    private bool isMissionActive = false;
    private Vector3 InitPosition;
    private StartMission startMission;
    private TrianingButton trianingButton;
    private SkillAcquirer skillAcquirer;

    /// <summary>
    /// ゲーム開始時に呼ばれるメソッド　初期設定
    /// </summary>
    void Awake()
    {
        InitPosition = player.transform.position;
        ClearText.text = "";
        //ゲームマネージャーから各種のコンポーネントを取得
        GameObject gameManager = GameObject.Find("GameManager");
        startMission = gameManager.GetComponent<StartMission>();
        trianingButton = gameManager.GetComponent<TrianingButton>();
        skillAcquirer = gameManager.GetComponent<SkillAcquirer>();
    }

    void Update()
    {
        //敵の数でミッションを監視する
        if (isMissionActive && enemyCount <= 0)
        {
            ClearMission(true); // 成功
        }
    }
    /// <summary>
    /// ミッションが開始されたときに起動するスクリプト 敵の数を数える
    /// </summary>
    public void StartMission()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        Debug.Log("敵の数" + enemyCount);
        isMissionActive = true;
    }
    /// <summary>
    /// 敵を倒した際に呼び出されるメソッド mobEnemyのonDieをから呼び出される
    /// </summary>
    public void OnEnemyDefeated()
    {
        enemyCount--;
        Debug.Log("敵撃破！残り: " + enemyCount);
    }
    /// <summary>
    /// ミッションが終わった際の処理
    /// </summary>
    /// <param name="isSuccess"></param>
    public void ClearMission(bool isSuccess)
    {
        isMissionActive = false;
        DisableMovePlayer();

        ClearText.text = "クリア";
        SelectSkillCard();//元の位置に戻る
    }
    /// <summary>
    /// ミッションが失敗したとき
    /// </summary>
    public void FailedMission()
    {
        isMissionActive = false;
        ClearText.text = "死んだぜ";

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        ReturnPlayerToInitialPosition();//元の位置に戻る
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
        ClearText.text = "";

        trianingButton.DecreaseTurn();//ターンが経過される
        player.transform.position = InitPosition;
        skillAcquirer.OnAcquireButtonPressed();
        startMission.ReturntTrainingButton();
        trianingButton.SetButtonsInteractable();

        vThirdPersonController vPersonController = player.GetComponent<vThirdPersonController>();
        vPersonController._currentHealth = vPersonController.maxHealth;
        vPersonController.isDead = false;
        DisableMovePlayer();

    }
    //プレイヤーを動けなくする
    public void DisableMovePlayer()
    {
        global::IsPlayerMove.GetInstance().CanMove = false;
        Rigidbody rb = player.GetComponent<Rigidbody>();
        vThirdPersonController vPersonController = player.GetComponent<vThirdPersonController>();

        rb.velocity = Vector3.zero;
        vPersonController.enabled = false;
        vPersonController.enabled = true;
    }
}
