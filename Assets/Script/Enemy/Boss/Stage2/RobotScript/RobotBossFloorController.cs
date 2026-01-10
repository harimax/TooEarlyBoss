using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
/// <summary>
/// TreeBoss の足場の上昇・リセット、矢印表示制御を担当するクラス。
/// </summary>
public class RobotBossFloorController : MonoBehaviour
{
    [Header("Floor")]
    [SerializeField] private GameObject floor;        // 足場オブジェクト
    [SerializeField] private Transform target;        // 上昇先
    [SerializeField] private float duration = 10f;    // 上昇にかける時間
    [Header("Arrows")]
    [SerializeField] private Transform upFloorPosArea;  // 矢印が配置されている親
    [SerializeField] private string keyshapeName = "Key"; // 矢印オブジェクト名
    [Header("Ground Enemy Manager")]
    [SerializeField] private MOGURAManager groundEnemyManager; // 上昇完了後に次スポーン許可
    private List<GameObject> upArrows = new();
    private Vector3 floorInitPos;

    private void Start()
    {
        floorInitPos = floor.transform.position;

        // 矢印(KeyShape)を収集（名前で拾う：非アクティブも対象）
        if (upFloorPosArea != null)
        {
            foreach (var t in upFloorPosArea.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == keyshapeName)
                {
                    upArrows.Add(t.gameObject);
                    t.gameObject.SetActive(false); // 初期は非表示
                }
            }
        }
    }
    /// <summary>
    /// 足場を上昇させる。
    /// 上昇完了後に MOGURAManager に「次のスポーン許可」を返す。
    /// </summary>
    public void UpFloor()
    {
        if (floor == null || target == null) return;

        // 既存の Tween を殺す
        DOTween.Kill(floor.transform);

        SetUpArrowsActive(true);

        floor.transform.DOMove(target.position, duration).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            // 上昇完了後に次スポーン許可
            if (groundEnemyManager != null)
            {
                groundEnemyManager.ReadyForNextSpawn();
            }
        });
    }
    /// <summary>
    /// 足場を初期位置にリセットする。
    /// 矢印も非表示にする。
    /// </summary>
    public void ResetFloor()
    {
        if (floor == null) return;

        // 既存の Tween を殺す
        DOTween.Kill(floor.transform);
        foreach (var t in floor.GetComponentsInChildren<Transform>(true))
        {
            DOTween.Kill(t);
        }

        floor.transform.position = floorInitPos;
    }
    /// <summary>矢印の表示/非表示を切り替える。</summary>
    private void SetUpArrowsActive(bool active)
    {
        if (upArrows == null) return;

        for (int i = 0; i < upArrows.Count; i++)
        {
            if (upArrows[i] != null)
            {
                upArrows[i].SetActive(active);
            }
        }
    }
}
