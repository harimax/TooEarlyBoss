using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Invector.vMelee;
using Invector.vCharacterController;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

public class BossStageManager : MonoBehaviour
{
    public GameObject StartButton;
    [SerializeField] private TextMeshProUGUI ClearText;
    [SerializeField] private GameObject Boss;
    [SerializeField] private GameObject BossCamera;
    [SerializeField] private GameObject ClearButton;
    [SerializeField] private GameObject GameOverButtons;
    [SerializeField] private GameObject[] petBoss; 
    private GameObject player;
    private IBossController bossController;
    private IBossController[] petBossControllers;
    private Button startButtonComponent;
    private Button clearButtonComponent;
    private GameObject lastSelected;
    // Start is called before the first frame update
    void Start()
    {
        ClearText.text = "";
        bossController = Boss.GetComponent<IBossController>();
        bossController.PauseBoss();
        // UIボタンを取得して、Aボタン(Submit)で押せるように準備する
        if (StartButton != null)
        {
            startButtonComponent = StartButton.GetComponentInChildren<Button>(true);
        }
        if (ClearButton != null)
        {
            clearButtonComponent = ClearButton.GetComponentInChildren<Button>(true);
        }
        //ステージ4のペットボスも取得して一時停止
        if (petBoss.Length > 0)
        {
            petBossControllers = new IBossController[petBoss.Length];
            for (int i = 0; i < petBoss.Length; i++)
            {
                petBossControllers[i] = petBoss[i].GetComponent<IBossController>();
                petBossControllers[i].PauseBoss();
            }
        }
    }

    void Update()
    {
        // 表示中のボタンを選択状態にして「Aボタン=Submit」で押せるようにする
        UpdateDefaultSelection();

        // ゲームでよくある「Aボタン(Submit)で進む」入力
        if (!IsSubmitPressed())
        {
            return;
        }
        // ボス戦開始ボタンが表示されている時はAボタンで開始
        if (StartButton != null && StartButton.activeSelf && startButtonComponent != null)
        {
            // UIのクリック処理をそのまま呼び出す
            startButtonComponent.onClick.Invoke();
            return;
        }
        // ボス撃破後の次のステージボタンが表示されている時はAボタンで遷移
        if (ClearButton != null && ClearButton.activeSelf && clearButtonComponent != null)
        {
            // UIのクリック処理をそのまま呼び出す
            clearButtonComponent.onClick.Invoke();
        }
    }

    public void BossStartButton()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        var skillManager = player.GetComponent<SkillManager>();

        IsPlayerMove.GetInstance().CanMove = true;
        StartButton.SetActive(false);
        BossCamera.SetActive(false);
        bossController.ResumeBoss();
        //ステージ4のペットボスも再開
        if (petBossControllers != null && petBossControllers.Length > 0)
        {
            for (int i = 0; i < petBossControllers.Length; i++)
            {
                petBossControllers[i].ResumeBoss();
            }
        }

        if (skillManager != null)
        {
            skillManager.ActivePassiveSkill();
        }
        //プレイヤーの.OnDeadにGAMEOVER処理を追加する
        var vCharacter = player.GetComponent<vThirdPersonController>(); 
        vCharacter.onDead.AddListener((GameObject deadObject) =>
        {
            GameOverDelay().Forget();
        });

    }
    public void DefeatBoss()
    {
        DefeatBossDelay().Forget();
    }
    private async UniTask DefeatBossDelay()
    {
        // ヒットストップ演出
        Time.timeScale = 0.3f;
        await UniTask.Delay(1500, ignoreTimeScale: true);
        // ヒットストップ後はゲーム時間を止める
        Time.timeScale = 0f;
        ClearText.text = "倒したぜ";
        Debug.Log("敵を倒した");
        ClearButton.SetActive(true);
    }
    private async UniTask GameOverDelay()
    {
        Time.timeScale = 0.3f;
        await UniTask.Delay(1500, ignoreTimeScale: true);
        Time.timeScale = 0.0f;
         ClearText.text = "死んだぜ/nどうする？";
        GameOverButtons.SetActive(true);
    }

    private bool IsSubmitPressed()
    {
        // InputManagerのSubmitはゲームパッドAボタンと紐づいていることが多い
        return Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.JoystickButton0);
    }

    private void UpdateDefaultSelection()
    {
        // EventSystem が無い場合は何もしない
        if (EventSystem.current == null)
        {
            return;
        }

        // ボス戦開始ボタンが表示中なら、そのボタンを優先して選択
        if (StartButton != null && StartButton.activeSelf && startButtonComponent != null)
        {
            SelectButtonIfNeeded(startButtonComponent.gameObject);
            return;
        }

        // クリア後の次のステージボタンが表示中なら、そのボタンを選択
        if (ClearButton != null && ClearButton.activeSelf && clearButtonComponent != null)
        {
            SelectButtonIfNeeded(clearButtonComponent.gameObject);
        }
    }

    private void SelectButtonIfNeeded(GameObject target)
    {
        // 既に選択されている場合は何もしない
        if (lastSelected == target)
        {
            return;
        }

        lastSelected = target;
        EventSystem.current.SetSelectedGameObject(target);
    }
}
