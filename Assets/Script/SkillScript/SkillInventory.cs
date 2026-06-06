using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 取得済みスキル一覧と上限判定を担当。
/// </summary>
public sealed class SkillInventory
{
    private readonly List<SkillBase> skills;
    private readonly int maxTotalSkills;
    private readonly int maxSpecialSkills;

    /// <summary>
    /// 既存の List を受け取り、外部互換を保ったまま管理用 API を提供。
    /// </summary>
    /// <param name="skills">取得済みスキル一覧。</param>
    /// <param name="maxTotalSkills">総スキル所持上限。</param>
    /// <param name="maxSpecialSkills">特殊スキル所持上限。</param>
    public SkillInventory(List<SkillBase> skills, int maxTotalSkills, int maxSpecialSkills)
    {
        this.skills = skills ?? new List<SkillBase>();
        this.maxTotalSkills = maxTotalSkills;
        this.maxSpecialSkills = maxSpecialSkills;
    }

    public IReadOnlyList<SkillBase> Skills => skills;

    /// <summary>
    /// 指定スキルを取得済みか確認。
    /// </summary>
    /// <param name="skill">確認対象のスキル。</param>
    /// <returns>取得済みなら true。</returns>
    public bool Contains(SkillBase skill)
    {
        return skill != null && skills.Contains(skill);
    }

    /// <summary>
    /// スキルを一覧へ追加。
    /// </summary>
    /// <param name="skill">追加対象のスキル。</param>
    /// <returns>追加できた場合は true。</returns>
    public bool Add(SkillBase skill)
    {
        // null と重複は所持一覧へ入れない
        if (skill == null || skills.Contains(skill))
        {
            return false;
        }

        skills.Add(skill);
        return true;
    }

    /// <summary>
    /// スキルを一覧から削除。
    /// </summary>
    /// <param name="skill">削除対象のスキル。</param>
    /// <returns>削除できた場合は true。</returns>
    public bool Remove(SkillBase skill)
    {
        return skill != null && skills.Remove(skill);
    }

    /// <summary>
    /// 総スキル所持数が上限に達しているか確認。
    /// </summary>
    /// <returns>上限到達なら true。</returns>
    public bool IsTotalLimitReached()
    {
        return skills.Count >= maxTotalSkills;
    }

    /// <summary>
    /// 特殊スキル所持数が上限に達しているか確認。
    /// </summary>
    /// <returns>上限到達なら true。</returns>
    public bool IsSpecialLimitReached()
    {
        return skills.Count(skill => skill != null && skill.SpcialSkill) >= maxSpecialSkills;
    }

    /// <summary>
    /// 特殊スキルだけを破棄候補として取得。
    /// </summary>
    /// <returns>特殊スキル一覧。</returns>
    public IReadOnlyList<SkillBase> GetSpecialSkills()
    {
        return skills
            .Where(skill => skill != null && skill.SpcialSkill)
            .ToList();
    }
}
