using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Invector.vCharacterController;

[CreateAssetMenu(menuName = "Skill/RoundFire")]
public class RoundFire : SkillBase
{
    public string effectObjectName;  // エフェクトの名前
    private GameObject activeEffect;
    public float duration = 10f;         // エフェクトの持続時間
    public override void ApplyEffect(GameObject player)
    {
        var child = player.transform.Find(effectObjectName);
        activeEffect = child.gameObject;
    }
    // 条件付きスキルの毎フレーム監視処理（SkillManagerから呼ばれる）
    public override void UpdateConditional(GameObject player)
    {
         var vInput=player.GetComponent<vThirdPersonInput>();
        // 発動キーが押されたら
        // if (Input.GetKeyDown(KeyCode.F))
        if(vInput.skill1Input.GetButtonDown())
        {
            Debug.Log("スキル発動");
            // すでにエフェクトが存在しない場合のみ発動
            if (activeEffect.activeSelf == false)
            {
                HandleActiveEffectAsync(player).Forget();
            }
        }
    }
    // UniTask によるエフェクトの生成＆終了処理
    private async UniTaskVoid HandleActiveEffectAsync(GameObject player)
    {
        // エフェクトを表示
        activeEffect.SetActive(true);

        // duration 秒待ってからエフェクトを非表示に
        // GameObject が破棄されたら自動キャンセルされるトークンを使う
        await UniTask.Delay(TimeSpan.FromSeconds(duration),
                            cancellationToken: player.GetCancellationTokenOnDestroy());

        activeEffect.SetActive(false);
    }
}
