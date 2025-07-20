using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        // 発動キーが押されたら
        if (Input.GetKeyDown(KeyCode.F))
        {
            // すでにエフェクトが存在しない場合のみ発動
            if (activeEffect.activeSelf == false)
            {
                ActiveEffect(player);
            }
        }
    }
    //エフェクトを生成して配置するスクリプト
    private void ActiveEffect(GameObject player)
    {
        activeEffect.SetActive(true);
        player.GetComponent<MonoBehaviour>().StartCoroutine(DestoryEffectAfterTime(duration));
    }
    // 指定時間後にエフェクトを削除するコルーチン
    private System.Collections.IEnumerator DestoryEffectAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        activeEffect.SetActive(false);
    }
}
