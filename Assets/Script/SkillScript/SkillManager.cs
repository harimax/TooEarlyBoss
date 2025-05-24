using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{   
    //得たスキルを格納する
    public List<SkillBase> acquiredSkills = new List<SkillBase>();
    
    //ミッション開始時に常時発動するスキルを適応させる
    public void ActivePassiveSkill()
    {
        foreach (var skill in acquiredSkills)
        {
            // Debug.Log(skill);
            skill.ApplyEffect(gameObject);
        }
    }

    void Update()
    {
        foreach (var skill in acquiredSkills)
        {
            skill.UpdateConditional(gameObject);
        }
    }
    public void AcquireSkill(SkillBase newSkill)
    {
        if (!acquiredSkills.Contains(newSkill))
        {
            acquiredSkills.Add(newSkill);
        }
    }
}
