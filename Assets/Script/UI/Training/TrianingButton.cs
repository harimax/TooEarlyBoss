using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 育成画面の UI 表示、ボタン入力、カットイン再生、能力値テキストの更新を担当。
/// 育成ルールと保存処理は <see cref="TrainingService"/> に任せ、この MonoBehaviour は UI 制御に責務を限定。
/// </summary>
public class TrianingButton : MonoBehaviour
{
    /// <summary>
    /// 育成で上昇させるプレイヤー能力値を識別。
    /// </summary>
    private enum TrainingStat
    {
        Power,
        Health,
        Stamina,
        Special
    }

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

    [Header("Training Parameters")]
    [SerializeField] private int minIncrease = 1;
    [SerializeField] private int maxIncrease = 5;
    [SerializeField] private float increaseDisplaySeconds = 1.2f;

    [SerializeField] private List<Button> BattleButtons;
    [SerializeField] private List<Button> trainingButtons;

    [Header("References")]
    [SerializeField] private CutIinAnimation cutInAnimation;
    [SerializeField] private TrainingEvent TrainingEvent;
    [SerializeField] private UIFocusSwitcher focusSwitcher;

    private readonly TrainingService trainingService = new TrainingService();
    private CancellationTokenSource increaseDisplayCts;

    /// <summary>
    /// 現在の残り育成ターン数。既存コードから参照できるよう公開。
    /// </summary>
    public int CurrentTurn => trainingService.CurrentTurn;

    /// <summary>
    /// 読み込み後、または育成更新後の現在のプレイヤー成長パラメータ。
    /// </summary>
    public PlayerGrowParameters CurrentParams => trainingService.CurrentParams;

    /// <summary>
    /// 進行状態から育成サービスを初期化し、互換用の公開フィールドへ値を同期。
    /// </summary>
    private void Awake()
    {
        trainingService.Initialize(
            ProcessManager.Instance.CurrentCycle,
            ProcessManager.Instance.TotalTurns);

        SyncFieldsFromParams();
        Instance = this;
    }

    /// <summary>
    /// 育成ボタンのコールバック登録と、初期 UI 表示を実行。
    /// </summary>
    private void Start()
    {
        Debug.Log($"Turn count:{CurrentTurn}");

        AddTrainingListener(0, TrainingAttack);
        AddTrainingListener(1, TrainingHealth);
        AddTrainingListener(2, TrainingStamina);
        AddTrainingListener(3, TrainingSpcial);

        UpdateUI();
    }

    /// <summary>
    /// UI オブジェクト破棄時に、上昇値表示の非同期処理をキャンセル。
    /// </summary>
    private void OnDestroy()
    {
        // 表示タスク未生成なら破棄処理不要
        if (increaseDisplayCts == null)
        {
            return;
        }

        increaseDisplayCts.Cancel();
        increaseDisplayCts.Dispose();
        increaseDisplayCts = null;
    }

    /// <summary>
    /// 攻撃力育成ボタンから呼ばれる UnityEvent 用の入口。
    /// </summary>
    public void TrainingAttack()
    {
        ApplyTraining(TrainingStat.Power);
    }

    /// <summary>
    /// 体力育成ボタンから呼ばれる UnityEvent 用の入口。
    /// </summary>
    public void TrainingHealth()
    {
        ApplyTraining(TrainingStat.Health);
    }

    /// <summary>
    /// スタミナ育成ボタンから呼ばれる UnityEvent 用の入口。
    /// </summary>
    public void TrainingStamina()
    {
        ApplyTraining(TrainingStat.Stamina);
    }

    /// <summary>
    /// 必殺技能力育成ボタンから呼ばれる UnityEvent 用の入口。
    /// </summary>
    public void TrainingSpcial()
    {
        ApplyTraining(TrainingStat.Special);
    }

    /// <summary>
    /// 選択された能力値だけを上昇対象にして、共通の育成処理を開始。
    /// </summary>
    /// <param name="stat">押された育成ボタンに対応する能力値。</param>
    private void ApplyTraining(TrainingStat stat)
    {
        ApplyTrainingAsync(CreateTrainingParameters(stat, IncreaseParameter(minIncrease, maxIncrease))).Forget();
    }

    /// <summary>
    /// 育成可能判定、イベント反映、カットイン再生、結果保存、UI 更新までの 1 回分の育成処理を実行。
    /// </summary>
    /// <param name="addParams">イベント補正前の基礎上昇パラメータ。</param>
    private async UniTask ApplyTrainingAsync(PlayerGrowParameters addParams)
    {
        // ターン切れや二重クリック時は育成処理を開始しない
        if (!trainingService.TryBeginTraining())
        {
            return;
        }

        try
        {
            TrainingResult result = trainingService.ResolveTraining(TrainingEvent, addParams);

            // イベント発生時だけカットインを再生し、未設定なら演出をスキップ
            if (result.EventType != TrainingEvent.TrainingEventType.None && cutInAnimation != null)
            {
                await cutInAnimation.Play(result.EventType);
            }

            trainingService.Commit(result.FinalAdd);
            SyncFieldsFromParams();
            ShowIncrease(result.FinalAdd);
            UpdateUI();
        }
        finally
        {
            trainingService.EndTraining();
        }
    }

    /// <summary>
    /// 選択された能力値だけに上昇量を持つ加算用パラメータを作成。
    /// </summary>
    /// <param name="stat">上昇対象の能力値。</param>
    /// <param name="value">生成された上昇量。</param>
    /// <returns>現在の成長値へ加算できるパラメータ。</returns>
    private static PlayerGrowParameters CreateTrainingParameters(TrainingStat stat, int value)
    {
        return stat switch
        {
            TrainingStat.Power => new PlayerGrowParameters(value, 0, 0, 0),
            TrainingStat.Health => new PlayerGrowParameters(0, value, 0, 0),
            TrainingStat.Stamina => new PlayerGrowParameters(0, 0, value, 0),
            TrainingStat.Special => new PlayerGrowParameters(0, 0, 0, value),
            _ => new PlayerGrowParameters(0, 0, 0, 0)
        };
    }

    /// <summary>
    /// Inspector で設定された育成ボタンにクリック処理を安全に登録。
    /// </summary>
    /// <param name="index">育成ボタンリスト内のインデックス。</param>
    /// <param name="action">ボタン押下時に実行する処理。</param>
    private void AddTrainingListener(int index, UnityEngine.Events.UnityAction action)
    {
        // Inspector 未設定や範囲外アクセスを防ぐガード
        if (trainingButtons == null || index < 0 || index >= trainingButtons.Count || trainingButtons[index] == null)
        {
            return;
        }

        trainingButtons[index].onClick.AddListener(action);
    }

    /// <summary>
    /// 設定された範囲からランダムな育成上昇量を生成。
    /// </summary>
    /// <param name="min">上昇量の最小値。この値は含む。</param>
    /// <param name="max">上昇量の最大値。UnityEngine.Random.Range(int, int) に合わせ、この値は含まない。</param>
    /// <returns>生成された育成上昇量。</returns>
    private int IncreaseParameter(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    /// <summary>
    /// 表示中の能力値、残りターン、ボタン操作可否を再描画。
    /// </summary>
    public void UpdateUI()
    {
        SetText(attackText, PlayerPower);
        SetText(healthText, PlayerHealth);
        SetText(staminaText, PlayerStamina);
        SetText(specialText, PlayerSpecial);

        // ターン表示 UI がある画面だけ残りターンを描画
        if (turn != null)
        {
            turn.text = $"Remaining:{CurrentTurn} turns";
        }

        SetButtonsInteractable();
    }

    /// <summary>
    /// 現在のターン数に応じて、育成ボタンと戦闘ボタンの操作可否を更新。
    /// </summary>
    public void SetButtonsInteractable()
    {
        bool canTrain = (CurrentTurn % 5 != 0) && (CurrentTurn >= 0);

        // 育成不能状態で育成ボタンにフォーカスが残る問題への対策
        if (focusSwitcher != null)
        {
            focusSwitcher.MoveFocusToBattleIfTrainingSelected().Forget();
        }

        // ボタンリスト未設定のシーンでも処理を止めないための保険
        if (trainingButtons != null)
        {
            foreach (var button in trainingButtons)
            {
                // Inspector の空要素を許容
                if (button != null)
                {
                    button.interactable = canTrain;
                }
            }
        }

        // 育成ターン終了時は通常戦闘を止め、次フェーズ用ボタンを有効化
        if (CurrentTurn == 0 && BattleButtons != null && BattleButtons.Count >= 2)
        {
            // 通常の戦闘開始ボタンを無効化
            if (BattleButtons[0] != null)
            {
                BattleButtons[0].interactable = false;
            }

            // ターン終了後に使うボタンを有効化
            if (BattleButtons[1] != null)
            {
                BattleButtons[1].interactable = true;
            }
        }
    }

    /// <summary>
    /// 残りターン数を 1 減少。既存の外部呼び出しに合わせて public のまま保持。
    /// </summary>
    public void DecreaseTurn()
    {
        trainingService.DecreaseTurn();
    }

    /// <summary>
    /// サービスが保持するパラメータを、UI と既存コード向けの公開フィールドへコピー。
    /// </summary>
    private void SyncFieldsFromParams()
    {
        // 初期化前に呼ばれた場合の null ガード
        if (CurrentParams == null)
        {
            return;
        }

        PlayerPower = CurrentParams.PlayerPower;
        PlayerHealth = CurrentParams.PlayerHealth;
        PlayerStamina = CurrentParams.PlayerStamina;
        PlayerSpecial = CurrentParams.PlayerSpecial;
    }

    /// <summary>
    /// 上昇した能力値がある場合、一時的な上昇量表示を開始。
    /// </summary>
    /// <param name="add">イベント補正後の最終上昇パラメータ。</param>
    private void ShowIncrease(PlayerGrowParameters add)
    {
        // 上昇がないイベント結果なら、上昇量 UI を出さない
        if (!HasAnyIncrease(add))
        {
            return;
        }

        // 直前の表示待ちを止め、新しい上昇量表示へ差し替え
        if (increaseDisplayCts != null)
        {
            increaseDisplayCts.Cancel();
            increaseDisplayCts.Dispose();
        }

        increaseDisplayCts = new CancellationTokenSource();
        ShowIncreaseAsync(add, increaseDisplayCts.Token).Forget();
    }

    /// <summary>
    /// 上昇量の表示を短時間だけ出し、その後クリア。
    /// </summary>
    /// <param name="add">イベント補正後の最終上昇パラメータ。</param>
    /// <param name="token">表示の差し替えや破棄時に使うキャンセルトークン。</param>
    private async UniTaskVoid ShowIncreaseAsync(PlayerGrowParameters add, CancellationToken token)
    {
        SetIncreaseText(increaseAttackText, add.PlayerPower);
        SetIncreaseText(increaseHealthText, add.PlayerHealth);
        SetIncreaseText(increaseStaminaText, add.PlayerStamina);
        SetIncreaseText(increaseSpecialText, add.PlayerSpecial);

        try
        {
            // 指定秒数だけ上昇量を残してから消去
            await UniTask.Delay(TimeSpan.FromSeconds(increaseDisplaySeconds), cancellationToken: token);
            ClearIncreaseTexts();
        }
        // 新しい表示や画面破棄でキャンセルされた場合はクリア処理を引き継がない
        catch (OperationCanceledException)
        {
            return;
        }
    }

    /// <summary>
    /// 育成結果でいずれかの能力値が上昇しているか確認。
    /// </summary>
    /// <param name="add">イベント補正後の最終上昇パラメータ。</param>
    /// <returns>いずれかの能力値が上昇していれば true。</returns>
    private static bool HasAnyIncrease(PlayerGrowParameters add)
    {
        return add.PlayerHealth > 0
            || add.PlayerPower > 0f
            || add.PlayerStamina > 0f
            || add.PlayerSpecial > 0f;
    }

    /// <summary>
    /// 対象の TextMeshPro が存在する場合、整形した能力値を表示。
    /// </summary>
    /// <param name="text">表示先の UI テキスト。</param>
    /// <param name="value">表示する値。</param>
    private static void SetText(TextMeshProUGUI text, float value)
    {
        // UI 参照が未設定の項目は表示更新を省略
        if (text != null)
        {
            text.text = FormatFloat(value);
        }
    }

    /// <summary>
    /// 対象の TextMeshPro に一時的な上昇量表示を書き込み。
    /// </summary>
    /// <param name="text">表示先の UI テキスト。</param>
    /// <param name="value">表示する上昇量。</param>
    private static void SetIncreaseText(TextMeshProUGUI text, float value)
    {
        // 上昇していない能力値や未設定 UI は表示対象外
        if (text != null && value > 0f)
        {
            text.text = $"\u2191{FormatFloat(value)}";
        }
    }

    /// <summary>
    /// すべての一時的な上昇量テキストをクリア。
    /// </summary>
    private void ClearIncreaseTexts()
    {
        ClearText(increaseAttackText);
        ClearText(increaseHealthText);
        ClearText(increaseStaminaText);
        ClearText(increaseSpecialText);
    }

    /// <summary>
    /// 対象の TextMeshPro が存在する場合、テキストを空に変更。
    /// </summary>
    /// <param name="text">クリア対象の UI テキスト。</param>
    private static void ClearText(TextMeshProUGUI text)
    {
        // UI 参照未設定時の null 回避
        if (text != null)
        {
            text.text = "";
        }
    }

    /// <summary>
    /// 小数値を能力値表示向けに短く整形。
    /// </summary>
    /// <param name="value">整形する値。</param>
    /// <returns>不要な小数桁を省いた表示文字列。</returns>
    private static string FormatFloat(float value)
    {
        return value.ToString("0.##");
    }
}
