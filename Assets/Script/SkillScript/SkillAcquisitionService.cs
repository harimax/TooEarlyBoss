/// <summary>
/// 新しいスキルを取得できるか、破棄が必要かを判定。
/// </summary>
public sealed class SkillAcquisitionService
{
    private readonly SkillInventory inventory;

    /// <summary>
    /// 所持一覧を使って取得判定サービスを作成。
    /// </summary>
    /// <param name="inventory">スキル所持管理。</param>
    public SkillAcquisitionService(SkillInventory inventory)
    {
        this.inventory = inventory;
    }

    /// <summary>
    /// スキル取得時の処理方針を返却。
    /// </summary>
    /// <param name="newSkill">取得対象のスキル。</param>
    /// <returns>取得結果の種類と破棄候補。</returns>
    public SkillAcquisitionResult Resolve(SkillBase newSkill)
    {
        if (newSkill == null || inventory == null)
        {
            return SkillAcquisitionResult.Invalid();
        }

        // 既に取得済みなら何もしない
        if (inventory.Contains(newSkill))
        {
            return SkillAcquisitionResult.AlreadyOwned();
        }

        // 特殊スキル上限は総数上限より先に判定し、破棄候補を特殊スキルに限定
        if (newSkill.SpcialSkill && inventory.IsSpecialLimitReached())
        {
            return SkillAcquisitionResult.RequireDiscard(inventory.GetSpecialSkills());
        }

        // 総数上限到達時は全スキルを破棄候補にする
        if (inventory.IsTotalLimitReached())
        {
            return SkillAcquisitionResult.RequireDiscard(inventory.Skills);
        }

        return SkillAcquisitionResult.AddDirectly();
    }
}
