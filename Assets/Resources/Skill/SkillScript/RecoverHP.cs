using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController;

[CreateAssetMenu(menuName = "Skill/RecoverHP")]
public class RecoverHP : SkillBase
{
    private bool isEffectApplied = false;
    public float RecoverHealthValue = 2f;
    public float duration = 10f;

    private bool isActive = false;
    private float timer = 0f;
    private float tickTimer = 0f;
    // Start is called before the first frame update
    public override void ApplyEffect(GameObject player)
    {

    }
    public override void UpdateConditional(GameObject player)
    {
        var hp = player.GetComponent<vThirdPersonController>();
        // 発動キーが押されたら
        if (IsActivationInputPressed(player) && !isActive && hp.currentHealth < hp.maxHealth)
        {
            isActive = true;
            timer = duration;
            tickTimer = 0f;
            Debug.Log("回復スキル発動：10秒間継続");
        }
        if (isActive)
        {
            hp.healthRecovery = RecoverHealthValue;
        }
        // 10秒経過したら終了
        if (timer <= 0f)
        {
            isActive = false;
            Debug.Log("回復スキル終了");
        }
    }
}
