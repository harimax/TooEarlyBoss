using UnityEngine;
using Invector.vCharacterController;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

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
        // UI 選択処理は共通 Utility に集約
        startButtonComponent = UISelectionUtility.FindFirstButton(StartButton);
        clearButtonComponent = UISelectionUtility.FindFirstButton(ClearButton);
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
    }

    public void BossStartButton()
    {
        Transform playerTransform = PlayerLocator.FindTransform();
        if (playerTransform == null)
        {
            return;
        }

        player = playerTransform.gameObject;
        var skillManager = player.GetComponent<SkillManager>();

        var moveState = IsPlayerMove.GetInstance();
        if (moveState != null)
        {
            moveState.ClearAllBlocks();
        }
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
        // スロー演出中に破棄された場合は後続 UI 反映を止める
        bool completed = await TimeScaleDelayUtility.WaitWithTemporaryScaleAsync(
            0.3f,
            3000,
            this.GetCancellationTokenOnDestroy());

        if (!completed)
        {
            return;
        }

        // ヒットストップ後はゲーム時間を止める
        Time.timeScale = 0f;
        ClearText.text = "倒したぜ";
        Debug.Log("敵を倒した");
        var moveState = IsPlayerMove.GetInstance();
        if (moveState != null)
        {
            moveState.Block(PlayerMoveBlockReason.BossClear);
        }
        ClearButton.SetActive(true);
    }
    private async UniTask GameOverDelay()
    {
        // GameOver表示前にManagerが破棄された場合、後続のUI更新を行わない
        bool completed = await TimeScaleDelayUtility.WaitWithTemporaryScaleAsync(
            0.3f,
            3000,
            this.GetCancellationTokenOnDestroy());

        if (!completed)
        {
            return;
        }

        Time.timeScale = 0.0f;
         ClearText.text = "死んだぜ/nどうする？";
        GameOverButtons.SetActive(true);
    }

    private void UpdateDefaultSelection()
    {
        // ボス戦開始ボタンが表示中なら、そのボタンを優先して選択
        if (UISelectionUtility.SelectButtonIfRootActive(StartButton, startButtonComponent, ref lastSelected))
        {
            return;
        }

        // クリア後の次のステージボタンが表示中なら、そのボタンを選択
        if (UISelectionUtility.SelectButtonIfRootActive(ClearButton, clearButtonComponent, ref lastSelected))
        {
            return;
        }

        if (GameOverButtons != null && GameOverButtons.activeSelf)
        {
            // GameOverButtons 配下の最初のボタンを選択する
            var button = UISelectionUtility.FindFirstButton(GameOverButtons);
            if (button != null)
            {
                UISelectionUtility.SelectIfNeeded(button.gameObject, ref lastSelected);
            }
        }
    }
}
