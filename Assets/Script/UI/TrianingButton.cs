using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrianingButton : MonoBehaviour
{
    public float PlayerPower = 1f;
    public float PlayerHealth = 1f;
    public float PlayerStamina = 1f;
    public float PlayerLucky = 1f;
    public int turnNumber=0;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI luckyText;
    [SerializeField] private TextMeshProUGUI turn;
    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {

    }
    //パワーボタンを押下してパワーがアップ
    public void TrainingAttack()
    {
        PlayerPower += increaceParameter(1, 5);
        turnNumber--;
        UpdateUI();
    }
    //体力ボタンを押下して体力がアップ
    public void TrainingHealth()
    {
        PlayerHealth += increaceParameter(1, 5);
        turnNumber--;
        UpdateUI();
    }

    //スタミナボタンを押下してスタミナがアップ
    public void TrainingStamina()
    {
        PlayerStamina += increaceParameter(1, 5);
        turnNumber--;
        UpdateUI();
    }
    //ラッキーボタンを押下してラッキーがアップ
    public void TrainingLucky()
    {
        PlayerLucky += increaceParameter(1, 5);
        turnNumber--;
        UpdateUI();
    }

    //上昇値を決めるメソッド
    private int increaceParameter(int minIncrease, int maxIncrease)
    {
        return Random.Range(minIncrease, maxIncrease);
    }
    //UIの更新
    void UpdateUI()
    {
        attackText.text = PlayerPower.ToString();
        healthText.text = PlayerHealth.ToString();
        staminaText.text = PlayerStamina.ToString();
        luckyText.text = PlayerLucky.ToString();
        turn.text=$"残り: {turnNumber}ターン";
    }
}
