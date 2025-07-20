using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vMelee;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Skill/DeclineStamina")]

public class DeclineStamina : SkillBase
{
    private vThirdPersonController vPersonController;
    private bool isApplied = false;
    public override void ApplyEffect(GameObject player)
    {
        //既に発動中なら起きないようにする
        if (isApplied) return;
        Debug.Log("スタミナ減少スキル発動");
        vPersonController = player.GetComponent<vThirdPersonController>();
        var attack = player.GetComponent<vMeleeManager>();

        vPersonController.sprintStamina -= 10f;
        vPersonController.jumpStamina -= 10f;
        vPersonController.rollStamina -= 10f;

        attack.defaultStaminaCost -= 10f;
        isApplied = true;
    }
}
