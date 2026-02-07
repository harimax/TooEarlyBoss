using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController;

public abstract class SkillBase : ScriptableObject
{
    //特殊スキルの入力割り当て
    public enum SkillActivationInput
    {
        Skill1,
        Skill2
    }
    public string skillName;
    public string description;
    public bool SpcialSkill = false;
    [SerializeField] private SkillActivationInput activationInput = SkillActivationInput.Skill1;

    //条件を満たされたときに効果を適応させる
    public abstract void ApplyEffect(GameObject player);
    public virtual void UpdateConditional(GameObject player) {}
    //特殊スキルボタンが押されたかどうか
    protected bool IsActivationInputPressed(GameObject player)
    {
        if (!player)
        {
            return false;
        }

        var vInput = player.GetComponent<vThirdPersonInput>();
        if (!vInput)
        {
            return false;
        }

        return activationInput switch
        {
            SkillActivationInput.Skill2 => vInput.skill2Input.GetButtonDown(),
            _ => vInput.skill1Input.GetButtonDown()
        };
    }

}
