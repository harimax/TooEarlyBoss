/// <summary>
/// 1 回分の育成判定結果を表す、変更不可の値オブジェクト。
/// </summary>
public readonly struct TrainingResult
{
    /// <summary>
    /// 最終上昇量とイベント演出情報を持つ育成結果を作成。
    /// </summary>
    /// <param name="finalAdd">イベント補正後の最終上昇パラメータ。</param>
    /// <param name="eventType">TrainingEvent から返されたイベント種別。</param>
    public TrainingResult(PlayerGrowParameters finalAdd, TrainingEvent.TrainingEventType eventType)
    {
        FinalAdd = finalAdd;
        EventType = eventType;
    }

    public PlayerGrowParameters FinalAdd { get; }
    public TrainingEvent.TrainingEventType EventType { get; }
}
