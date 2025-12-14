using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Invector; // vHealthController 用

public class MOGURAManager : MonoBehaviour
{
     [Header("Spawn Settings")]
    [SerializeField] private GameObject MOGURAPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnDelay = 0.5f;     // スポーン前の溜め（任意）
    private GameObject currentEnemy;
    private bool canSpawn = true;                         // 「次を出して良い」フラグ
    public event Action OnGroundEnemyKilled;

    /// <summary>
    /// ボス側から呼ぶ：このタイミングで1体だけ湧かせたい
    /// 既にフィールドに居る場合は何もしない
    /// </summary>
    public async UniTask SpawnOnceIfNone()
    {
        if (!canSpawn) return;           // ボスからOKが出るまでスポーンしない
        if (currentEnemy != null) return;

        if (!CanSpawn())
        {
            Debug.LogWarning("[MOGURAManager] Prefab or SpawnPoints 未設定");
            return;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(spawnDelay));

        // === 修正箇所 ===
        // nextIndex の代わりにランダムで選ぶ
        var spawnPoint = PickRandomSpawnPoint();
        currentEnemy = Instantiate(MOGURAPrefab, spawnPoint.position, spawnPoint.rotation);

        SetupDeathCallback(currentEnemy);

        // この1体が倒れるまで追加スポーンは止める
        canSpawn = false;

        // vHealthController の死亡検知にフック（onChangeHealthでHP<=0を拾う）
        var hc = currentEnemy.GetComponentInChildren<vHealthController>();
        if (hc != null)
        {
            bool notified = false;
            hc.onChangeHealth.AddListener((cur) =>
            {
                if (!notified && cur <= 0f)
                {
                    notified = true;
                    HandleEnemyKilled();
                }
            });
        }
        else
        {
            Debug.LogWarning("[MOGURAManager] vHealthController が見つからないため、手動で HandleEnemyKilled を呼ぶ必要があります。");
        }

        // この1体が倒れるまで追加スポーンは止める
        canSpawn = false;
    }

    /// <summary>
    /// スポーンに必要な設定が揃っているか。
    /// </summary>
    private bool CanSpawn()
    {
        return MOGURAPrefab != null && spawnPoints != null && spawnPoints.Length > 0;
    }

    /// <summary>
    /// ランダムなスポーン地点を１つ選ぶ。
    /// </summary>
    private Transform PickRandomSpawnPoint()
    {
        int index = UnityEngine.Random.Range(0, spawnPoints.Length);
        return spawnPoints[index];
    }

    /// <summary>
    /// vHealthController に死亡時コールバックを登録する。
    /// <summary>
    private void SetupDeathCallback(GameObject enemy)
    {
        var hc =enemy.GetComponentInChildren<vHealthController>();
        if(hc==null)
        {
            Debug.LogWarning("[MOGURAManager] vHealthController が見つからないため、手動で HandleEnemyKilled を呼ぶ必要があります。");
            return;
        }
        bool notified = false;
        hc.onChangeHealth.AddListener((cur) =>
        {
            if (!notified && cur <= 0f)
            {
                notified = true;
                HandleEnemyKilled();
            }
        });
    }

    /// <summary>
    /// 敵が倒れた時に呼ばれる内部処理
    /// </summary>
    private void HandleEnemyKilled()
    {
        // 現在の敵はもう管理対象外
        currentEnemy = null;

        // ボスへ通知（足場上昇トリガ）
        OnGroundEnemyKilled?.Invoke();
    }

    /// <summary>
    /// ボスから呼ぶ：「足場上昇が完了した。次のスポーンを許可する」
    /// </summary>
    public void ReadyForNextSpawn()
    {
        canSpawn = true;
    }

    /// <summary>
    /// いま敵が居るか？
    /// </summary>
    public bool HasAliveEnemy() => currentEnemy != null;
}