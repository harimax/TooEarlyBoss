using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using UnityEngine.UI;
using TMPro;

public class ReciveDamageUI : MonoBehaviour
{
    private vHealthController vHealthController;
    private TextMeshProUGUI damageText;
    //ダメージ数を受け取りUIに表示させる
    // Start is called before the first frame update
    void Start()
    {
        vHealthController = transform.parent.parent.GetComponent<vHealthController>();
        damageText = gameObject.GetComponent<TextMeshProUGUI>();
        damageText.text = "";
        // Debug.Log(vHealthController);
    }
    //ダメージを受けたときにUIにダメージ数を表示させる
    public void DamageHitNumber()
    {
        // Debug.Log(vHealthController.reciveDamage);
        damageText.text = vHealthController.reciveDamage.ToString();
        StartCoroutine(ResetDamageText(1.0f));
    }
    //時間経過でダメージ数が0になる処理
    private IEnumerator ResetDamageText(float time)
    {
        float elapsed = 0f;
        Color originalColor = damageText.color;

        //指定の時間になるまで上昇しながら透明になる
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;

            // 徐々に透明に
            damageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1 - t);

            yield return null;
        }
        // テキストを消して初期位置に戻す
        damageText.text = "";
        damageText.color = originalColor; // 色を元に戻す
    }
}
