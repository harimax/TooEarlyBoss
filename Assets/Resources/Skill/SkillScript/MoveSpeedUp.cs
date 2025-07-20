using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vMelee;

[CreateAssetMenu(menuName = "Skill/MoveSpeedUp")]
public class MoveSpeedUp : SkillBase
{
    private bool isEffectApplied = false;
    public float duration = 10f;         // エフェクトの持続時間
    private float tempWalkSpeed;
    private float tempRunnigSpeed;
    private float tempSprintSpeed;

    public override void ApplyEffect(GameObject player)
    {

    }
    // 条件付きスキルの毎フレーム監視処理（SkillManagerから呼ばれる）
    public override void UpdateConditional(GameObject player)
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isEffectApplied)
            {
                ActiveSpeedUp(player);
            }
        }
    }
    //移動速度が速くなる処理
    private void ActiveSpeedUp(GameObject player)
    {
        var playerComp = player.GetComponent<vThirdPersonController>();
        tempWalkSpeed = playerComp.freeSpeed.walkSpeed;
        tempRunnigSpeed = playerComp.freeSpeed.runningSpeed;
        tempSprintSpeed = playerComp.freeSpeed.sprintSpeed;

        playerComp.freeSpeed.walkSpeed = tempWalkSpeed * 1.3f;
        playerComp.freeSpeed.runningSpeed = tempRunnigSpeed * 1.3f;
        playerComp.freeSpeed.sprintSpeed = tempSprintSpeed * 1.3f;
        isEffectApplied = true;
        player.GetComponent<MonoBehaviour>().StartCoroutine(FinishSpeedUp(duration, player));
    }
    //スキルが終わるタイミングと速度が元に戻る
    private System.Collections.IEnumerator FinishSpeedUp(float time, GameObject player)
    {

        yield return new WaitForSeconds(time);
        var playerComp = player.GetComponent<vThirdPersonController>();
        playerComp.freeSpeed.walkSpeed = tempWalkSpeed;
        playerComp.freeSpeed.runningSpeed = tempRunnigSpeed;
        playerComp.freeSpeed.sprintSpeed = tempSprintSpeed;
        isEffectApplied = false;
        Debug.Log("スピードアップ終わり");
    }
}
