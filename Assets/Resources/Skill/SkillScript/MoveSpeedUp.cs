using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vMelee;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;

[CreateAssetMenu(menuName = "Skill/MoveSpeedUp")]
public class MoveSpeedUp : SkillBase
{
    private bool isEffectApplied = false;
    public float duration = 5f;         // エフェクトの持続時間
    private float tempWalkSpeed;
    private float tempRunnigSpeed;
    private float tempSprintSpeed;
    private const string MeshTrailComponentName = "MeshTrailTut";

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
                ActiveSpeedUp(player).Forget();
            }
        }
    }
    //移動速度が速くなる処理
    private async UniTask ActiveSpeedUp(GameObject player)
    {
        var playerComp = player.GetComponent<vThirdPersonController>();
        tempWalkSpeed = playerComp.freeSpeed.walkSpeed;
        tempRunnigSpeed = playerComp.freeSpeed.runningSpeed;
        tempSprintSpeed = playerComp.freeSpeed.sprintSpeed;

        playerComp.freeSpeed.walkSpeed = tempWalkSpeed * 1.3f;
        playerComp.freeSpeed.runningSpeed = tempRunnigSpeed * 1.3f;
        playerComp.freeSpeed.sprintSpeed = tempSprintSpeed * 1.3f;
        SetMeshTrailEnabled(player, true);
        isEffectApplied = true;
        await FinishSpeedUp(duration, player);
    }
    //スキルが終わるタイミングと速度が元に戻る
    private async UniTask FinishSpeedUp(float time, GameObject player)
    {

        await UniTask.Delay(TimeSpan.FromSeconds(time));
        var playerComp = player.GetComponent<vThirdPersonController>();
        playerComp.freeSpeed.walkSpeed = tempWalkSpeed;
        playerComp.freeSpeed.runningSpeed = tempRunnigSpeed;
        playerComp.freeSpeed.sprintSpeed = tempSprintSpeed;
        SetMeshTrailEnabled(player, false);
        isEffectApplied = false;
        Debug.Log("スピードアップ終わり");
    }

    private void SetMeshTrailEnabled(GameObject player, bool enabled)
    {
        Debug.Log("メッシュトレイルの有効化状態を変更");
        var updated = false;

        var components = player.GetComponentsInChildren<Component>(true);
        foreach (var component in components)
        {
            if (component == null)
            {
                continue;
            }

            if (component.GetType().Name != MeshTrailComponentName)
            {
                continue;
            }

            if (component is Behaviour behaviour)
            {
                behaviour.enabled = enabled;
                updated = true;
            }
        }

        if (!updated)
        {
            Debug.LogWarning($"{MeshTrailComponentName} が見つかりませんでした。");
        }
    }
}
