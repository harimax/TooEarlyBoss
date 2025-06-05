using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Invector;

public class EnemyHP : MonoBehaviour
{
    //Health_Controllerからデータを取得する
    private vHealthController vHealthController;

    public Slider hpSlider;
    //敵のHPをUIに反映させる
    void Awake()
    {
        vHealthController = gameObject.GetComponent<vHealthController>();
        hpSlider.maxValue = vHealthController.maxHealth;
        hpSlider.value = vHealthController.currentHealth;
        
        // Debug.Log(hpSlider.value);
    }
    //HPを更新する処理
    public void UpdateEnemyHP()
    {
        hpSlider.value = vHealthController.currentHealth;
    }

}
