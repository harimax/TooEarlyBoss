
using System;
[Serializable]

//成長パラメータの型クラス
public class PlayerGrowParameters
{
    //修行で上げたパラメータ
    public float PlayerPower;
    public int PlayerHealth;
    public float PlayerStamina;
    public float PlayerSpecial;
    //コンストラクタ(初期化)
    /// 初期値またはロードされた値を使用する。
    public PlayerGrowParameters(float power, int health, float stamina, float special)
    {
        PlayerPower = power;
        PlayerHealth = health;
        PlayerStamina = stamina;
        PlayerSpecial = special;
    }
    /// <summary>
    /// 成長パラメータに増加量を加算し「新しい成長パラメータ」を返す。
    /// ※元のインスタンスは書き換えず、新しい値を返すことで安全性を高める。
    /// </summary>
    public PlayerGrowParameters AddParameters(PlayerGrowParameters other)
    {
        return new PlayerGrowParameters(
            PlayerPower + other.PlayerPower,
            PlayerHealth + other.PlayerHealth,
            PlayerStamina + other.PlayerStamina,
            PlayerSpecial + other.PlayerSpecial
        );
    }
}
