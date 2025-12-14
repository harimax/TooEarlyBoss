using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Invector;

/// <summary>
/// 子オブジェクトの vHealthController(モグラ) を監視し、
/// HP 2/3・1/3 を下回ったタイミングで TreeBoss へイベントを飛ばす。
/// </summary>
public class RobotBossMoguraHPWatcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RobotBossController boss;   // バリア発動用
    [SerializeField] private RobotBossPatrol patrol;     // 歩行開始用
    private vHealthController moguraHealth;// HP 監視対象

    // 閾値
    private float startWalkThreshold; // 1/3 で歩行開始
    private float thTwoThird;         // 2/3
    private float thOneThird;         // 1/3

    // フラグ
    private bool firedTwoThird = false;
    private bool firedOneThird = false;
    private bool walkTriggered = false;
    private int DelayTime = 1000;

    private CancellationToken _ct;

    void Start()
    {
        _ct = this.GetCancellationTokenOnDestroy();
        // 子オブジェクトからモグラの vHealthController を取得
        moguraHealth = GetComponentInChildren<vHealthController>();
        if (moguraHealth == null)
        {
            Debug.LogWarning("TreeBossHpWatcher: 子に vHealthController(モグラ) が見つかりません。");
            return;
        }
        // 閾値計算
        startWalkThreshold = moguraHealth.maxHealth / 3f;
        thTwoThird = moguraHealth.maxHealth * (2f / 3f);
        thOneThird = moguraHealth.maxHealth * (1f / 3f);

        // HP変化イベント登録
        moguraHealth.onChangeHealth.AddListener(OnMoleHealthChanged);
        Debug.Log("TreeBossHpWatcher: vHealthController(モグラ) 監視開始");
    }

    private void OnDestroy()
    {
        if (moguraHealth != null)
            moguraHealth.onChangeHealth.RemoveListener(OnMoleHealthChanged);
    }

    /// <summary>
    /// HPが変化した際に呼ばれるコールバック。
    /// HP 2/3 / 1/3 を下回ったタイミングでバリア発動を予約。
    /// HP 1/3 を下回ったらボス歩行を開始。
    /// </summary>
    private async void OnMoleHealthChanged(float currentHealth)
    {
        // すべてのイベントを発火済みなら監視終了
        if(walkTriggered && firedTwoThird && firedOneThird)
        {
            return;
        }
        // 2/3 閾値
        if (!firedTwoThird && currentHealth <= thTwoThird)
        {
            firedTwoThird = true;
            await UniTask.Delay(DelayTime, cancellationToken: _ct); // 1秒遅延
            if (moguraHealth.currentHealth > 0 && boss != null)
            {
                await boss.OnActiveBarriar();
            }
        }
        // 1/3 閾値
        if (!firedOneThird && currentHealth <= thOneThird)
        {
            firedOneThird = true;
            await UniTask.Delay(DelayTime, cancellationToken: _ct); // 1秒遅延
            if (moguraHealth.currentHealth > 0 && boss != null)
            {
                await boss.OnActiveBarriar();
            }
        }

        // 歩行開始閾値
        if (!walkTriggered && currentHealth <= startWalkThreshold)
        {
            walkTriggered = true;
            patrol?.StartPatrol();
        }

    }


}
