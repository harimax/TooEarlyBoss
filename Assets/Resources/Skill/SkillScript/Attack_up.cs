using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vMelee;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Skill/Attack_up")]
public class Attack_up : SkillBase
{
    private vMeleeManager _vMeleeManager;
    public int AttackUPValue;
    public override void ApplyEffect(GameObject player)
    {
        Debug.Log("攻撃力上昇!!!");
        _vMeleeManager = player.GetComponent<vMeleeManager>();
        _vMeleeManager.defaultDamage.damageValue += AttackUPValue;
        _vMeleeManager.Init();
    }
}
