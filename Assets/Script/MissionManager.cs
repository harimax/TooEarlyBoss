using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class MissionManager : MonoBehaviour
{
    private int enemyCount;
    private bool isMissionActive = false;
    [SerializeField] private GameObject player;
    [SerializeField] private TextMeshProUGUI ClearText;
    [SerializeField] Fade fade;
    [SerializeField] private GameObject tempButton;
    private Vector3 firstPos;
    private StartMission startDugeon;
    private TrianingButton trianingButton;
    private SkillAcquirer skillAcquirer;

    /// <summary>
    /// ゲーム開始時に呼ばれるメソッド　初期設定
    /// </summary>
    void Awake()
    {
        firstPos = player.transform.position;
        ClearText.text = "";
        //ゲームマネージャーから各種のコンポーネントを取得
        GameObject gameManager = GameObject.Find("GameManager");
        startDugeon = gameManager.GetComponent<StartMission>();
        trianingButton = gameManager.GetComponent<TrianingButton>();
        skillAcquirer=gameManager.GetComponent<SkillAcquirer>();
    }

    void Update()
    {
        //敵の数でミッションを監視する
        if (isMissionActive && enemyCount <= 0)
        {
            EndMission(true); // 成功
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
    public void EndMission(bool isSuccess)
    {
        isMissionActive = false;
        global::StartMission.GetInstance().CanMove = false;     //プレイヤーを動けなくする

        Rigidbody rb = player.GetComponent<Rigidbody>();
        vThirdPersonController vPersonController = player.GetComponent<vThirdPersonController>();

        rb.velocity = Vector3.zero;
        vPersonController.enabled = false;
        vPersonController.enabled = true;

        ClearText.text = "クリア";
        SelectSkillCard();//元の位置に戻る
    }

    public void SelectSkillCard()
    {
        //スキルカードを選択する(仮にボタンにする)
        tempButton.SetActive(true);
    }
    public void ReturnFirstPos()
    {
        isMissionActive = false;
        tempButton.SetActive(false);
        ClearText.text = "";

        TrianingButton.turnNumber--;//ターンが経過される
        player.transform.position = firstPos;
        skillAcquirer.OnAcquireButtonPressed();
        startDugeon.ReturntTrainingButton();
        trianingButton.SetButtonsInteractable();
    }
}
