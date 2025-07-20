using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vMelee;
[CreateAssetMenu(menuName = "Skill/LowHealthPowerBoost")]
public class LowHealthPowerBoost : SkillBase
{
    public float thresholdPercent = 0.5f;
    public int bonusAttack = 10;
    private bool isEffectApplied = false;
    public override void ApplyEffect(GameObject player)
    {

        
    }
    public override void UpdateConditional(GameObject player)
    {
        var hp = player.GetComponent<vThirdPersonController>();
        var attack = player.GetComponent<vMeleeManager>();

        float hpRatio = hp.currentHealth / hp.maxHealth;
        // 条件を満たしていればスキルが発動する
        if (hpRatio < thresholdPercent)
        {
            Debug.Log("パワースキル発動");
            if (!isEffectApplied)
            {
                attack.defaultDamage = new vDamage(Mathf.RoundToInt(attack.defaultDamage.damageValue) + bonusAttack);
                isEffectApplied = true;
            }
        }
        //スキルが発動中で体力が回復すればスキルは解除される
        else
        {
            if (isEffectApplied)
            {
                attack.defaultDamage = new vDamage(Mathf.RoundToInt(attack.defaultDamage.damageValue) - bonusAttack);
                isEffectApplied = false;
            }
        }

    }
}
