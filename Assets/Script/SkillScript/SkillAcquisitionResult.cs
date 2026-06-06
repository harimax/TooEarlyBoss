using System.Collections.Generic;

/// <summary>
/// スキル取得判定の結果種別。
/// </summary>
public enum SkillAcquisitionResultType
{
    Invalid,
    AlreadyOwned,
    AddDirectly,
    RequireDiscard
}

/// <summary>
/// スキル取得時の判定結果。
/// 仕様変更時はこの結果型を拡張して UI 側の分岐を増やせる構造。
/// </summary>
public readonly struct SkillAcquisitionResult
{
    private SkillAcquisitionResult(
        SkillAcquisitionResultType type,
        IReadOnlyList<SkillBase> discardCandidates)
    {
        Type = type;
        DiscardCandidates = discardCandidates ?? System.Array.Empty<SkillBase>();
    }

    public SkillAcquisitionResultType Type { get; }
    public IReadOnlyList<SkillBase> DiscardCandidates { get; }

    /// <summary>
    /// 無効な取得要求を表す結果を作成。
    /// </summary>
    /// <returns>無効要求の取得結果。</returns>
    public static SkillAcquisitionResult Invalid()
    {
        return new SkillAcquisitionResult(
            SkillAcquisitionResultType.Invalid,
            System.Array.Empty<SkillBase>());
    }

    /// <summary>
    /// 取得済みスキルだった場合の結果を作成。
    /// </summary>
    /// <returns>取得済みの取得結果。</returns>
    public static SkillAcquisitionResult AlreadyOwned()
    {
        return new SkillAcquisitionResult(
            SkillAcquisitionResultType.AlreadyOwned,
            System.Array.Empty<SkillBase>());
    }

    /// <summary>
    /// そのまま追加可能な結果を作成。
    /// </summary>
    /// <returns>直接追加の取得結果。</returns>
    public static SkillAcquisitionResult AddDirectly()
    {
        return new SkillAcquisitionResult(
            SkillAcquisitionResultType.AddDirectly,
            System.Array.Empty<SkillBase>());
    }

    /// <summary>
    /// 破棄選択が必要な結果を作成。
    /// </summary>
    /// <param name="discardCandidates">破棄候補スキル。</param>
    /// <returns>破棄要求の取得結果。</returns>
    public static SkillAcquisitionResult RequireDiscard(IReadOnlyList<SkillBase> discardCandidates)
    {
        return new SkillAcquisitionResult(
            SkillAcquisitionResultType.RequireDiscard,
            discardCandidates);
    }
}
