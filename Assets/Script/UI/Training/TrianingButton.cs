using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class TrianingButton : MonoBehaviour
{
    // グローバルアクセス用インスタンス（既存コード互換）
    public static TrianingButton Instance { get; private set; }
    [Header("Debug / Backward Compatibility")]
    public float PlayerPower = 1f;
    public int PlayerHealth = 1;
    public float PlayerStamina = 1f;
    public float PlayerSpecial = 1f;
    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI specialText;
    [SerializeField] private TextMeshProUGUI turn;
    [SerializeField] private TextMeshProUGUI increaseAttackText;
    [SerializeField] private TextMeshProUGUI increaseHealthText;
    [SerializeField] private TextMeshProUGUI increaseStaminaText;
    [SerializeField] private TextMeshProUGUI increaseSpecialText;

    [Header("成長パラメータ設定")]
    [SerializeField] private int minIncrease = 1;
    [SerializeField] private int maxIncrease = 5;
    [SerializeField] private float increaseDisplaySeconds = 1.2f;

    [SerializeField] private List<Button> BattleButtons;
    [SerializeField] private List<Button> trainingButtons;
    [Header("参照")]
    [SerializeField] private CutIinAnimation cutInAnimation;
    [SerializeField] private TrainingEvent TrainingEvent;
    [SerializeField] private UIFocusSwitcher focusSwitcher;
    private int turnNumber;
    // 一時表示のキャンセル用トークン
    private CancellationTokenSource increaseDisplayCts;
    /// <summary>
    /// 現在の残りターン数（読み取り専用）
    /// </summary>
    public int CurrentTurn => turnNumber;

    /// <summary>
    /// 現在のプレイヤー成長パラメータ。
    /// 実データはこの値オブジェクトに集約し、直接書き換えず Add() で新インスタンスに差し替える。
    /// </summary>
    private PlayerGrowParameters currentParams;

    /// <summary>
    /// 現在の成長パラメータ（読み取り専用）
    /// 他クラスはここから成長値を参照すればよい。
    /// </summary>
    public PlayerGrowParameters CurrentParams => currentParams;
    // Start is called before the first frame update

    void Awake()
    {
        // 周回1回目は初期値、それ以外はセーブからロード
        if (ProcessManager.Instance.CurrentCycle == 1)
        {
            // 値オブジェクトを生成
            currentParams = new PlayerGrowParameters(1f, 1, 1f, 1f);
        }
        else
        {
            // Repository からロード
            if (!PlayerGrowRepository.LoadParameters(out currentParams))
            {
                // 万一セーブがなければ初期値
                currentParams = new PlayerGrowParameters(1f, 1, 1f, 1f);
            }
        }
        // ミラー用の public フィールドに反映
        SyncFieldsFromParams();
        // シングルトン処理
        Instance = this;
    }
    void Start()
    {
        turnNumber = ProcessManager.Instance.TotalTurns;
        Debug.Log($"ターン数:{turnNumber}");
        // ボタンにリスナーを登録
        trainingButtons[0].GetComponent<Button>().onClick.AddListener(TrainingAttack);
        trainingButtons[1].GetComponent<Button>().onClick.AddListener(TrainingHealth);
        trainingButtons[2].GetComponent<Button>().onClick.AddListener(TrainingStamina);
        trainingButtons[3].GetComponent<Button>().onClick.AddListener(TrainingSpcial);

        // 最初はボタンを有効化
        UpdateUI();
    }
    // UniTaskの遅延表示を安全に破棄する
    private void OnDestroy()
    {
        if (increaseDisplayCts == null)
        {
            return;
        }
        increaseDisplayCts.Cancel();
        increaseDisplayCts.Dispose();
        increaseDisplayCts = null;
    }
    //パワーボタンを押下してパワーがアップ
    public void TrainingAttack()
    {
        var add = new PlayerGrowParameters(
            power: increaseParameter(minIncrease, maxIncrease),
            health: 0,
            stamina: 0, special: 0);
        ApprlyingTraining(add);
    }
    //体力ボタンを押下して体力がアップ
    public void TrainingHealth()
    {
        var add = new PlayerGrowParameters(
            power: 0,
            health: increaseParameter(minIncrease, maxIncrease),
            stamina: 0, special: 0);
        ApprlyingTraining(add);
    }

    //スタミナボタンを押下してスタミナがアップ
    public void TrainingStamina()
    {
        var add = new PlayerGrowParameters(
            power: 0,
            health: 0,
            stamina: increaseParameter(minIncrease, maxIncrease), special: 0);
        ApprlyingTraining(add);
    }
    //ラッキーボタンを押下してラッキーがアップ
    public void TrainingSpcial()
    {
        var add = new PlayerGrowParameters(
            power: 0,
            health: 0,
            stamina: 0, special: increaseParameter(minIncrease, maxIncrease));
        ApprlyingTraining(add);
    }

    //上昇値を決めるメソッド
    private int increaseParameter(int minIncrease, int maxIncrease)
    {
        return UnityEngine.Random.Range(minIncrease, maxIncrease);
    }
    /// <summary>
    /// トレーニング処理本体
    /// 渡された増分パラメータをcurrentParamsに加算し、
    /// ターン消費・保存・UI更新を一括でする
    /// </summary>
    private void ApprlyingTraining(PlayerGrowParameters addParams)
    {
        //ターンが残っていなければ処理しない
        if (turnNumber <= 0)
        {
            return;
        }
        var (finalAdd, eventType) = TrainingEvent.Apply(addParams);

        //イベント演出を再生
        if(eventType != TrainingEvent.TrainingEventType.None)
        {
            cutInAnimation.PlayFromButton();
        }

        // 成長パラメータを加算して新インスタンスに差し替え
        currentParams = currentParams.AddParameters(finalAdd);
        // ミラー用の public フィールドに反映
        SyncFieldsFromParams();
        // ターンを1減らす
        DecreaseTurn();
        // 成長パラメータを保存
        PlayerGrowRepository.SaveParameters(currentParams);
        // UI更新
        ShowIncrease(finalAdd);
        UpdateUI();
        
    }
    //UIの更新
    public void UpdateUI()
    {
        if (attackText != null)
        {
            attackText.text = PlayerPower.ToString();
        }
        if (healthText != null)
        {
            healthText.text = PlayerHealth.ToString();
        }
        if (staminaText != null)
        {
            staminaText.text = PlayerStamina.ToString();
        }
        if (specialText != null)
        {
            specialText.text = PlayerSpecial.ToString();
        }
        turn.text = $"残り:{turnNumber}ターン";

        SetButtonsInteractable();
    }
    // ターン数に応じてボタンを有効/無効にするメソッド
    public void SetButtonsInteractable()
    {
        bool canTrain = (turnNumber % 5 != 0) && (turnNumber >= 0);
        focusSwitcher.MoveFocusToBattleIfTrainingSelected().Forget();

        foreach (var button in trainingButtons)
        {
            button.interactable = canTrain;
        }
        if (turnNumber == 0)
        {
            BattleButtons[0].interactable = false;
            BattleButtons[1].interactable = true;
        }
    }
    //ターンを減少させるメソッド
    public void DecreaseTurn()
    {
        turnNumber--;
    }
    // ミラー同期用ヘルパーを追加
    private void SyncFieldsFromParams()
    {
        PlayerPower = currentParams.PlayerPower;
        PlayerHealth = currentParams.PlayerHealth;
        PlayerStamina = currentParams.PlayerStamina;
        PlayerSpecial = currentParams.PlayerSpecial;
    }

    // 増分があるステータスのみ一時的に「+X」を表示する
    private void ShowIncrease(PlayerGrowParameters add)
    {
        if (!HasAnyIncrease(add))
        {
            return;
        }

        if (increaseDisplayCts != null)
        {
            increaseDisplayCts.Cancel();
            increaseDisplayCts.Dispose();
        }
        increaseDisplayCts = new CancellationTokenSource();
        ShowIncreaseAsync(add, increaseDisplayCts.Token).Forget();
    }

    // 一定時間だけ増分表示を出してから通常UIに戻す
    private async UniTaskVoid ShowIncreaseAsync(PlayerGrowParameters add, CancellationToken token)
    {
        if (increaseAttackText != null && add.PlayerPower > 0f)
        {
            increaseAttackText.text = $"↑{FormatFloat(add.PlayerPower)}";
        }
        if (increaseHealthText != null && add.PlayerHealth > 0)
        {
            increaseHealthText.text = $"↑{FormatFloat(add.PlayerHealth)}";
        }
        if (increaseStaminaText != null && add.PlayerStamina > 0f)
        {
            increaseStaminaText.text = $"↑{FormatFloat(add.PlayerStamina)}";
        }
        if (increaseSpecialText != null && add.PlayerSpecial > 0f)
        {
            increaseSpecialText.text = $"↑{FormatFloat(add.PlayerSpecial)}";
        }
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(increaseDisplaySeconds), cancellationToken: token);
            // 増分表示をクリア
            increaseAttackText.text = "";
            increaseHealthText.text = "";
            increaseStaminaText.text = "";
            increaseSpecialText.text = "";
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    // いずれかのステータスが上昇しているか
    private static bool HasAnyIncrease(PlayerGrowParameters add)
    {
        return add.PlayerHealth > 0
            || add.PlayerPower > 0f
            || add.PlayerStamina > 0f
            || add.PlayerSpecial > 0f;
    }

    // 小数点以下を見やすく整形する
    private static string FormatFloat(float value)
    {
        return value.ToString("0.##");
    }
}
