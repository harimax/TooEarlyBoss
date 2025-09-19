using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Unity.VisualScripting;
using Cinemachine;
using UnityEngine.UIElements;
using Invector.vMelee;
using Invector.vCharacterController;
public class StartMission : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject MainCameraObject;//Invectorのやつをアタッチ
    [SerializeField] private GameObject trainingUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject brainCameraObject;//Brainのやつをアタッチ
    [SerializeField] private MissionManager missionManager;

    private static StartMission _instance;
    private CinemachineVirtualCamera mainCameraVirtual;
    private CinemachineBrain brain;
    private vThirdPersonController vPersonController;
    private bool IsGameUI;
    private bool IstrainingUI;
    private TrianingButton trainingButton;
    private vMeleeManager _vMeleeManager;
    private EnemyGenerator enemyGenerator;
    private SkillManager skillManager;

    // プレイヤーの修行値
    public float tempPlayerSpecial = 1f;
    private bool isMissionActive = false; // ミッションが進行中かどうか
    private void Awake()
    {
        _instance = this;

        AttachPlayerCameraData();
    }
    /// <summary>
    /// 戦闘準備モードへ移行する
    /// </summary>
    public void PrepareMissionButton()
    {
        // Debug.Log("戦闘移行");
        mainCameraVirtual.Priority = 20;
        SetAllUIInactive();
        IsGameUI = true;
        IstrainingUI = false;
        AttachPlayerCameraData();
        StartCoroutine(SwitchCamera());
    }
    /// <summary>
    /// ミッション開始ボタン
    /// </summary>
    public void StartMissionButton()
    {
        vPersonController.ResetMaxHealth();
        vPersonController.ResetMaxStamina();
        //修行したパラメータを加算させる
        vPersonController.AddMaxStamina(trainingButton.PlayerStamina);
        vPersonController.AddMaxHealth(trainingButton.PlayerHealth);
        _vMeleeManager.defaultDamage.damageValue = Mathf.RoundToInt(trainingButton.PlayerPower) + 10;
        _vMeleeManager.Init();

        // 行動可能状態に
        IsPlayerMove.GetInstance().CanMove = true;

        // UI非表示、敵出現、スキル発動、ミッション開始
        SetAllUIInactive();
        enemyGenerator.GenerateEnemy();
        missionManager.StartMission();//ミッション開始メソッドが呼ばれる
        skillManager.ActivePassiveSkill();//スキルを発動させる
    }

    /// <summary>
    /// 修行モードに戻るボタン
    /// </summary>
    public void ReturntTrainingButton()
    {
        // Debug.Log("修行に戻る");
        mainCameraVirtual.Priority = 5;
        SetAllUIInactive();
        IsGameUI = false;
        IstrainingUI = true;
        StartCoroutine(SwitchCamera());
    }
    // カメラ遷移完了まで待ってからUIとCanMoveを切り替える
    private IEnumerator SwitchCamera()
    {
        yield return new WaitForEndOfFrame(); // カメラの優先度変更反映待ち
        yield return new WaitUntil(() => !brain.IsBlending); // 遷移完了待ち
        //戦闘シーンの際はGameUIを起動
        if (IsGameUI)
        {
            gameUI.SetActive(true);
            IsGameUI = false;
        }
        //修行シーンの際はtrainingUIを起動
        if (IstrainingUI)
        {
            trainingUI.SetActive(true);
            IstrainingUI = false;
        }
    }

    /// <summary>
    /// UIをすべて非表示にする
    /// </summary>
    private void SetAllUIInactive()
    {
        trainingUI.SetActive(false);
        gameUI.SetActive(false);
    }
    //StartMissionクラスを受け取る
    public static StartMission GetInstance()
    {
        return _instance;
    }

    public void AttachPlayerCameraData()
    {
                // プレイヤー参照
        var player = GameObject.FindWithTag("Player");
        vPersonController = player.GetComponent<vThirdPersonController>();
        _vMeleeManager = player.GetComponent<vMeleeManager>();
        skillManager = player.GetComponent<SkillManager>();

        // カメラ関連
        var MainCameraObject = GameObject.FindWithTag("MainCamera");
        mainCameraVirtual = MainCameraObject.GetComponent<CinemachineVirtualCamera>();
        brain = brainCameraObject.GetComponent<CinemachineBrain>();
        // ゲームオブジェクト参照
        enemyGenerator = GameObject.Find("EnemyGenerator").GetComponent<EnemyGenerator>();
        trainingButton = this.gameObject.GetComponent<TrianingButton>();
    }
}
