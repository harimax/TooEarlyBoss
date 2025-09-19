using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillBase : ScriptableObject
{
    public string skillName;
    public string description;
    public bool SpcialSkill = false;

    //条件を満たされたときに効果を適応させる
    public abstract void ApplyEffect(GameObject player);
    public virtual void UpdateConditional(GameObject player) {}

}
