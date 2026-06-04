using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using Invector.vCharacterController;
using Invector.vMelee;

[CreateAssetMenu(menuName = "Skill/DamageNegateTwice")]
public class DamageNegateTwice : SkillBase
{
    // ダメージを無効化する残り回数は、プレイヤー側のコンポーネントに持たせる。
    [SerializeField] private int blockCount = 2;

    public override void ApplyEffect(GameObject player)
    {
        if (!player)
        {
            return;
        }

        var invincibility = player.GetComponent<PlayerDamageInvincibility>();
        if (invincibility == null)
        {
            // 効果の実処理が無い状態で取得済みに見えるのを避けるため、警告だけ出して終了する。
            Debug.LogWarning("DamageNegateTwice requires PlayerDamageInvincibility on the player.", player);
            return;
        }

        // ScriptableObject に状態を残さず、現在のプレイヤーへ無効化回数を加算する。
        invincibility.AddDamageNegationBlocks(blockCount);
        Debug.Log("ダメージ無効スキル適応");
    }
}
