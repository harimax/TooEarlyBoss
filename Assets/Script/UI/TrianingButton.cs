using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrianingButton : MonoBehaviour
{
    public static TrianingButton Instance { get; private set; }
    public float PlayerPower = 1f;
    public int PlayerHealth = 1;
    public float PlayerStamina = 1f;
    public float PlayerSpecial = 1f;
    private int turnNumber;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI specialText;
    [SerializeField] private TextMeshProUGUI turn;
    [SerializeField] private int minIncrease = 1;
    [SerializeField] private int maxIncrease = 5;
    [SerializeField] private List<UnityEngine.UI.Button> BattleButtons;
    [SerializeField] private List<UnityEngine.UI.Button> trainingButtons;
    // Start is called before the first frame update

    void Awake()
    {
        if (ProcessManager.Instance.CurrentCycle == 1)
        {
            PlayerPower = 1f;
            PlayerHealth = 1;
            PlayerStamina = 1f;
            PlayerSpecial = 1f;
        }
        else
        {
            LoadParameter();
        }
        Instance = this;
        
    }
    void Start()
    {
        turnNumber = ProcessManager.Instance.TotalTurns;
        Debug.Log($"ターン数:{turnNumber}");
        // 最初はボタンを有効化
        UpdateUI();
    }
    //パワーボタンを押下してパワーがアップ
    public void TrainingAttack()
    {
        PlayerPower += increaceParameter(minIncrease, maxIncrease);
        DecreaseTurn();
        UpdateUI();
    }
    //体力ボタンを押下して体力がアップ
    public void TrainingHealth()
    {
        PlayerHealth += increaceParameter(minIncrease, maxIncrease);
        DecreaseTurn();
        UpdateUI();
    }

    //スタミナボタンを押下してスタミナがアップ
    public void TrainingStamina()
    {
        PlayerStamina += increaceParameter(minIncrease, maxIncrease);
        DecreaseTurn();
        UpdateUI();
    }
    //ラッキーボタンを押下してラッキーがアップ
    public void TrainingSpcial()
    {
        PlayerSpecial += increaceParameter(minIncrease, maxIncrease);
        DecreaseTurn();
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
        if (attackText != null)
        {
            attackText.text = PlayerPower.ToString();
        }
        if (healthText != null)
        {
            healthText.text = PlayerHealth.ToString();
        }
        if (staminaText != null)
        {
            staminaText.text = PlayerStamina.ToString();
        }
        if (specialText != null)
        {
            specialText.text = PlayerSpecial.ToString();    
        }
        turn.text = $"残り:{turnNumber}ターン";

        SetButtonsInteractable();
    }
    // ターン数に応じてボタンを有効/無効にするメソッド
    public void SetButtonsInteractable()
    {
        bool canTrain = (turnNumber % 5 != 0) && (turnNumber >= 0);

        foreach (var button in trainingButtons)
        {
            button.interactable = canTrain;
        }
        if (turnNumber == 0)
        {
            BattleButtons[0].interactable = false;
            BattleButtons[1].interactable = true;
        }
    }
    //ターンを減少させるメソッド
    public void DecreaseTurn()
    {
        turnNumber--;
    }
    public void SaveParameter()
    {
        string jsonPlayrtParam = JsonUtility.ToJson(new PlayerParamSaveData(PlayerPower, PlayerHealth, PlayerStamina, PlayerSpecial));
        PlayerPrefs.SetString("PlayerParamSave", jsonPlayrtParam);
        PlayerPrefs.Save();
    }
    public void LoadParameter()
    {
        if (PlayerPrefs.HasKey("PlayerParamSave"))
        {
            Debug.Log("パラメータ読み込み");
            string jsonPlayerParam = PlayerPrefs.GetString("PlayerParamSave");
            PlayerParamSaveData loadedData = JsonUtility.FromJson<PlayerParamSaveData>(jsonPlayerParam);
            PlayerPower = loadedData.playerPower;
            PlayerHealth = loadedData.playerHealth;
            PlayerStamina = loadedData.playerStamina;
            PlayerSpecial = loadedData.playerSpecial;
        }
    }
    //保持しているデータを削除するメソッド
    public void DeleteParameter()
    {
        PlayerPrefs.DeleteKey("PlayerParamSave");
        PlayerPrefs.Save();
    }
    private class PlayerParamSaveData
    {
        public float playerPower;
        public int playerHealth;
        public float playerStamina;
        public float playerSpecial;

        public PlayerParamSaveData(float power, int health, float stamina, float special)
        {
            playerPower = power;
            playerHealth = health;
            playerStamina = stamina;
            playerSpecial = special;
        }
    }
    
}
