using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vMelee;

[CreateAssetMenu(menuName = "Skill/DamageNegateTwice")]
public class DamageNegateTwice : SkillBase
{
    public static int remainingBlocks; // 無効化できる残りの回数（初期は2回）
    public static bool noDamage = false;
    public override void ApplyEffect(GameObject player)
    {
        remainingBlocks = 2;//フレーム的に2回呼ばれるから2の倍数でするとよい
        noDamage = true;
        Debug.Log("ダメージ無効スキル適応");
    }
}
