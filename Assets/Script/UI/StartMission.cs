using System.Collections;
using UnityEngine;
using Invector;
using Cinemachine;
using Invector.vMelee;
using Invector.vCharacterController;
using Cysharp.Threading.Tasks;
using System;
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
    private CinemachineBrain brainCameraObj;
    private vThirdPersonController playerController;
    private bool IsGameUI;
    private bool IstrainingUI;
    private TrianingButton trainingButton;
    private vMeleeManager meleeManager;
    private EnemyGenerator enemyGenerator;
    private SkillManager skillManager;

    // プレイヤーの修行値
    public float tempPlayerSpecial = 1f;
    private bool isMissionActive = false; // ミッションが進行中かどうか
    private void Awake()
    {
        _instance = this;

        AttachPlayerData();
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
        AttachPlayerData();
        _ = SwitchCameraAsync(); // ← ここもUniTaskに変更
    }
    /// <summary>
    /// ミッション開始ボタン
    /// </summary>
    public void StartMissionButton()
    {
        playerController.ResetMaxHealth();
        playerController.ResetMaxStamina();
        //修行したパラメータを加算させる
        playerController.AddMaxStamina(trainingButton.PlayerStamina);
        playerController.AddMaxHealth(trainingButton.PlayerHealth);
        meleeManager.defaultDamage.damageValue = Mathf.RoundToInt(trainingButton.PlayerPower) + 10;
        meleeManager.Init();

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
        _ = SwitchCameraAsync(); // ← ここもUniTaskに変更
    }
    // カメラ遷移完了まで待ってからUIとCanMoveを切り替える
    private async UniTask SwitchCameraAsync()
    {
        // カメラの優先度変更反映待ち（1フレーム待つ）
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

        // ブレンド完了待ち
        await UniTask.WaitUntil(() => !brainCameraObj.IsBlending);
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
    /// <summary>
    /// ミッション開始に必要なデータを格納
    /// </summary>
    public void AttachPlayerData()
    {
                // プレイヤー参照
        var player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<vThirdPersonController>();
        meleeManager = player.GetComponent<vMeleeManager>();
        skillManager = player.GetComponent<SkillManager>();

        // カメラ関連
        var MainCameraObject = GameObject.FindWithTag("MainCamera");
        mainCameraVirtual = MainCameraObject.GetComponent<CinemachineVirtualCamera>();
        brainCameraObj = brainCameraObject.GetComponent<CinemachineBrain>();
        // ゲームオブジェクト参照
        enemyGenerator = GameObject.Find("EnemyGenerator").GetComponent<EnemyGenerator>();
        trainingButton = this.gameObject.GetComponent<TrianingButton>();
    }
}
