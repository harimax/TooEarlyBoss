using UnityEngine;

public static class PlayerGrowRepository
{
    private const string PlayerGrowKey = "PlayerGrowParameters"; //保存するキー
    /// <summary>
    /// JSON化するための内部データクラス
    /// （PlayerGrowthParameters は直接 JSON 化できないため）
    /// </summary>
    [System.Serializable]
    private class PlayerGrowSaveData
    {
        public float PlayerPower;
        public int PlayerHealth;
        public float PlayerStamina;
        public float PlayerSpecial;
        public PlayerGrowSaveData(PlayerGrowParameters parameters)
        {
            PlayerPower = parameters.PlayerPower;
            PlayerHealth = parameters.PlayerHealth;
            PlayerStamina = parameters.PlayerStamina;
            PlayerSpecial = parameters.PlayerSpecial;
        }
        /// <summary>
        /// PlayerGrowParameters に変換する
        /// </summary>
        public PlayerGrowParameters ToPlayerGrowParameters()
        {
            return new PlayerGrowParameters(PlayerPower, PlayerHealth, PlayerStamina, PlayerSpecial);
        }
    }
    /// <summary>
    /// プレイヤーの成長パラメータを保存する
    /// </summary>
    public static void SaveParameters(PlayerGrowParameters parameters)
    {
        var saveData = new PlayerGrowSaveData(parameters);
        var json = JsonUtility.ToJson(saveData);

        PlayerPrefs.SetString(PlayerGrowKey, json);
        PlayerPrefs.Save();//データを保存
    }
    /// <summary>
    /// プレイヤーの成長パラメータを読み込む
    /// </summary>
    public static bool LoadParameters(out PlayerGrowParameters param)
    {
        if (!PlayerPrefs.HasKey(PlayerGrowKey))
        {
            param = null;
            return false;
        }

        string json = PlayerPrefs.GetString(PlayerGrowKey);
        var data= JsonUtility.FromJson<PlayerGrowSaveData>(json);

        param = data.ToPlayerGrowParameters();
        return true;
    }
    /// <summary>
    /// 保存された成長パラメータを削除する
    /// </summary>
    public static void DeleteParameters()
    {
        PlayerPrefs.DeleteKey(PlayerGrowKey);
        PlayerPrefs.Save();
    }
}

