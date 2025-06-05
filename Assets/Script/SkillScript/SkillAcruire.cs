using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAcquirer : MonoBehaviour
{
    public SkillBase skillToAcquire;
    public GameObject player; // プレイヤーをInspectorでアサイン
    //スキルを獲得する処理
    public void OnAcquireButtonPressed()
    {
        var manager = player.GetComponent<SkillManager>();
        if (manager != null && skillToAcquire != null)
        {
            manager.AcquireSkill(skillToAcquire);
            // Debug.Log($"スキル {skillToAcquire.skillName} を獲得しました！");
        }
    }
}
