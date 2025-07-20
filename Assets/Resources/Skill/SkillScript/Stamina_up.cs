using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Skill/Stamina_up")]
public class Stamina_up : SkillBase
{
    private vThirdPersonController vPersonController;
    public int Up_Stamina;
    public override void ApplyEffect(GameObject player)
    {
        Debug.Log("HP上昇!!!");
        vPersonController = player.GetComponent<vThirdPersonController>();
        vPersonController.AddMaxStamina(Up_Stamina);
    }
}
