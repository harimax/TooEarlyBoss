using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrianingButton : MonoBehaviour
{
    public float PlayerPower = 1f;
    public int PlayerHealth = 1;
    public float PlayerStamina = 1f;
    public float PlayerSpecial = 1f;
    public static int turnNumber = 0;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI specialText;
    [SerializeField] private TextMeshProUGUI turn;
    [SerializeField] private int minIncrease = 1;
    [SerializeField] private int maxIncrease = 5;
    [SerializeField] private List<UnityEngine.UI.Button> trainingButtons;
    // Start is called before the first frame update
    void Start()
    {
        turnNumber = 19;
        // 最初はボタンを有効化
        UpdateUI();
    }

    // Update is called once per frame
    void OnEnable()
    {
        // Debug.Log("オブジェクトがアクティブ化されました！");
        UpdateUI();
    }
    //パワーボタンを押下してパワーがアップ
    public void TrainingAttack()
    {
        PlayerPower += increaceParameter(minIncrease, maxIncrease);
        turnNumber--;
        UpdateUI();
    }
    //体力ボタンを押下して体力がアップ
    public void TrainingHealth()
    {
        PlayerHealth += increaceParameter(minIncrease, maxIncrease);
        turnNumber--;
        UpdateUI();
    }

    //スタミナボタンを押下してスタミナがアップ
    public void TrainingStamina()
    {
        PlayerStamina += increaceParameter(minIncrease, maxIncrease);
        turnNumber--;
        UpdateUI();
    }
    //ラッキーボタンを押下してラッキーがアップ
    public void TrainingSpcial()
    {
        PlayerSpecial += increaceParameter(minIncrease, maxIncrease);
        turnNumber--;
        UpdateUI();
    }

    //上昇値を決めるメソッド
    private int increaceParameter(int minIncrease, int maxIncrease)
    {
        return Random.Range(minIncrease, maxIncrease);
    }
    //UIの更新
    public void UpdateUI()
    {
        attackText.text = PlayerPower.ToString();
        healthText.text = PlayerHealth.ToString();
        staminaText.text = PlayerStamina.ToString();
        specialText.text = PlayerSpecial.ToString();
        turn.text = $"残り: {turnNumber}ターン";

        SetButtonsInteractable();
    }
    // ターン数に応じてボタンを有効/無効にするメソッド
    public void SetButtonsInteractable()
    {
        bool canTrain = (turnNumber % 5 != 0) && (turnNumber > 0);

        foreach (var button in trainingButtons)
        {
            button.interactable = canTrain;
        }
    }
}
