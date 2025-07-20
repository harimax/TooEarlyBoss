using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vMelee;

[CreateAssetMenu(menuName = "Skill/ShotDarkBall")]
public class ShotDarkBall : SkillBase
{

    public override void ApplyEffect(GameObject player)
    {
        player.GetComponent<ShotFire>().hasFireballSkill = true;
    }
}
