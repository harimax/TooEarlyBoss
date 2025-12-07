using UnityEngine;

[CreateAssetMenu(menuName = "Setting/ProcessSetting")]
public class ProcessSetting : ScriptableObject
{
    [Header("サイクル設定")]
    public int startCycle = 1;
    public int maxCycle = 5;
    [Header("ターン設定")]
    public int totalTurnsPerCycle = 14;
    [Header("敵の初期値設定")]
    public int baseEnemyCount = 1;
}
