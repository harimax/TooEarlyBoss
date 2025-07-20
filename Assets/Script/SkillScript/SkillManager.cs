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
    //ゲーム中に条件を満たすと発動する
    void Update()
    {
        Debug.Log("スキル確認中");
        foreach (var skill in acquiredSkills)
        {
            skill.UpdateConditional(gameObject);
        }
    }
    //スキルを獲得する処理
    public void AcquireSkill(SkillBase newSkill)
    {
        if (!acquiredSkills.Contains(newSkill))
        {
            acquiredSkills.Add(newSkill);
            Debug.Log(newSkill.skillName + "を獲得しました");
        }
    }
}
