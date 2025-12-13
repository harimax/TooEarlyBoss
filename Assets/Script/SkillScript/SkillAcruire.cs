using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAcquirer : MonoBehaviour
{
    [Header("取得対象スキル")]
    public SkillBase skillToAcquire;
     [Header("プレイヤー(未設定の場合は自動検索)")]
    public GameObject player; 
    //スキルを獲得する処理
    public void OnAcquireButtonPressed()
    {
        // プレイヤーオブジェクトを探してアタッチする
        if (player == null)
        {
            player = GameObject.FindWithTag("Player"); // "Player" タグを利用
        }

        var manager = player.GetComponent<SkillManager>();
        if (manager != null && skillToAcquire != null)
        {
            manager.AcquireSkill(skillToAcquire);
            Debug.Log($"スキル {skillToAcquire.skillName} を獲得しました！");
        }
    }
}
