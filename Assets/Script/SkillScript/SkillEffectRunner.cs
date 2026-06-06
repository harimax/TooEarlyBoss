using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所持スキルの効果発動と条件更新を担当。
/// </summary>
public sealed class SkillEffectRunner
{
    /// <summary>
    /// 戦闘開始時にパッシブ効果を一括適用。
    /// </summary>
    /// <param name="owner">効果適用先の GameObject。</param>
    /// <param name="skills">適用対象スキル一覧。</param>
    public void ApplyPassiveSkills(GameObject owner, IReadOnlyList<SkillBase> skills)
    {
        if (owner == null || skills == null)
        {
            return;
        }

        foreach (var skill in skills)
        {
            if (skill == null)
            {
                continue;
            }

            skill.ApplyEffect(owner);
        }
    }

    /// <summary>
    /// 条件付きスキルの発動条件を更新。
    /// </summary>
    /// <param name="owner">効果判定に使う GameObject。</param>
    /// <param name="skills">更新対象スキル一覧。</param>
    public void UpdateConditionalSkills(GameObject owner, IReadOnlyList<SkillBase> skills)
    {
        if (owner == null || skills == null)
        {
            return;
        }

        foreach (var skill in skills)
        {
            if (skill == null)
            {
                continue;
            }

            skill.UpdateConditional(owner);
        }
    }
}
