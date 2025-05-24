using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vMelee;

[CreateAssetMenu(menuName = "Skill/RecoveryStaminaSpeedUp")]
public class RecoveryStaminaSpeedUp : SkillBase
{
    public float addRecoveryStamina=0.5f;
    private bool isApplied = false;
    public override void ApplyEffect(GameObject player)
    {
        //既に発動中なら起きないようにする
        if (isApplied) return;

        Debug.Log("スタミナ消費減少スキル発動");
        var hp = player.GetComponent<vThirdPersonController>();
        hp.staminaRecovery+=addRecoveryStamina;
        isApplied=true;
    }
}
