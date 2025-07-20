using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Skill/HP_up")]
public class HP_up : SkillBase
{
    private vThirdPersonController vPersonController;
    public int Up_HP;
    public override void ApplyEffect(GameObject player)
    {
        Debug.Log("HP上昇!!!");
        vPersonController = player.GetComponent<vThirdPersonController>();
        vPersonController.AddMaxHealth(Up_HP);
    }
}
