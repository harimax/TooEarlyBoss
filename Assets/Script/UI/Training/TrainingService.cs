/// <summary>
/// 現在の成長パラメータ、残りターン、保存、育成結果の確定ルールを管理。
/// 単一責任の原則に沿うため、UI 用 MonoBehaviour から育成ロジックを分離。
/// </summary>
public sealed class TrainingService
{
    private static readonly PlayerGrowParameters InitialParameters = new PlayerGrowParameters(1f, 1, 1f, 1f);

    public PlayerGrowParameters CurrentParams { get; private set; }
    public int CurrentTurn { get; private set; }

    private bool isTrainingInProgress;

    /// <summary>
    /// 現在の周回数と保存済み成長データから、育成状態を初期化。
    /// </summary>
    /// <param name="currentCycle">現在のゲーム周回数。</param>
    /// <param name="totalTurns">この育成フェーズ開始時のターン数。</param>
    public void Initialize(int currentCycle, int totalTurns)
    {
        CurrentTurn = totalTurns;

        // 1 周目は保存値を使わず、初期値から育成を開始
        if (currentCycle == 1)
        {
            CurrentParams = CreateInitialParameters();
            PlayerGrowRepository.SaveParameters(CurrentParams);
            return;
        }

        // セーブデータがない場合の保険として初期値を採用
        if (!PlayerGrowRepository.LoadParameters(out var loadedParams))
        {
            loadedParams = CreateInitialParameters();
        }

        CurrentParams = loadedParams;
    }

    /// <summary>
    /// 残りターンがあり、他の育成処理が実行中でなければ、育成処理を開始状態へ変更。
    /// </summary>
    /// <returns>育成を進められる場合は true。</returns>
    public bool TryBeginTraining()
    {
        // ターン切れ、または二重実行中なら育成開始不可
        if (CurrentTurn <= 0 || isTrainingInProgress)
        {
            return false;
        }

        isTrainingInProgress = true;
        return true;
    }

    /// <summary>
    /// 現在の育成処理を完了状態へ戻す。
    /// </summary>
    public void EndTraining()
    {
        isTrainingInProgress = false;
    }

    /// <summary>
    /// 基礎上昇量に TrainingEvent の補正を反映し、確定可能な結果として返却。
    /// </summary>
    /// <param name="trainingEvent">育成イベントを抽選、適用するコンポーネント。</param>
    /// <param name="addParams">イベント補正前の基礎上昇パラメータ。</param>
    /// <returns>最終上昇量と UI 演出用のイベント種別。</returns>
    public TrainingResult ResolveTraining(TrainingEvent trainingEvent, PlayerGrowParameters addParams)
    {
        // イベント未設定のシーンでは、基礎上昇量だけで育成を成立
        if (trainingEvent == null)
        {
            return new TrainingResult(addParams, TrainingEvent.TrainingEventType.None);
        }

        var (finalAdd, eventType) = trainingEvent.Apply(addParams);
        return new TrainingResult(finalAdd, eventType);
    }

    /// <summary>
    /// 最終育成結果を現在値へ加算し、ターンを消費して保存。
    /// </summary>
    /// <param name="finalAdd">イベント補正後の最終上昇パラメータ。</param>
    public void Commit(PlayerGrowParameters finalAdd)
    {
        CurrentParams = CurrentParams.AddParameters(finalAdd);
        DecreaseTurn();
        PlayerGrowRepository.SaveParameters(CurrentParams);
    }

    /// <summary>
    /// 残りターン数を 1 減少。
    /// </summary>
    public void DecreaseTurn()
    {
        CurrentTurn--;
    }

    /// <summary>
    /// 初期成長パラメータの新しいインスタンスを作成。
    /// </summary>
    /// <returns>プレイヤーの初期成長パラメータ。</returns>
    private static PlayerGrowParameters CreateInitialParameters()
    {
        return new PlayerGrowParameters(
            InitialParameters.PlayerPower,
            InitialParameters.PlayerHealth,
            InitialParameters.PlayerStamina,
            InitialParameters.PlayerSpecial);
    }
}
