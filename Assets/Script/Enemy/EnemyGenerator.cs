using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{

    [SerializeField] private GameObject[] MobEnemy;
    [SerializeField] private List<ForbiddenVolume> forbiddenVolumes;
    [SerializeField] private int generateRange_X;
    [SerializeField] private int generateRange_Y;
    [Header("生成範囲ギズモ設定")]
    [SerializeField] private bool showGenerateRangeGizmo = true;
    [SerializeField] private float gizmoY = 0f; // 表示する高さ（床がy=0なら0でOK）
    [SerializeField] private Color rangeFillColor = new Color(0f, 1f, 0f, 0.15f);
    [SerializeField] private Color rangeWireColor = new Color(0f, 1f, 0f, 0.9f);
    private int generaterNumber;
    private int[][] cycleEnemyArray = new int[][]
    {
        new int[]{0,1,2},          //サイクル1で出現する敵のインデックス
        new int[]{0,1,2,3,4},       //サイクル2で出現する敵のインデックス
        new int[]{0,1,2,3,4,5},     //サイクル3で出現する敵のインデックス
        new int[]{0,1,2,3,4,5,6},   //サイクル4で出現する敵のインデックス
        new int[]{0,1,2,3,4,5,6,7} //サイクル5で出現する敵のインデックス
    };

    //敵を生成するメソッド
    public void GenerateEnemy()
    {
        generaterNumber = ProcessManager.Instance.GetEnemyCountForCurrentBattle();
        for (int i = 0; i < generaterNumber; i++)
        {
            var selectedEnemy = MobEnemy[Random.Range(0, cycleEnemyArray[ProcessManager.Instance.CurrentCycle - 1].Length)];
            Instantiate(selectedEnemy, generatePosition(), gameObject.transform.rotation);
        }
    }
    //敵の生成位置を決めるメソッド
    private Vector3 generatePosition()
    {
        const int maxAttempts = 100;

        // EnemyGenerator の位置を中心にする
        Vector3 center = transform.position;

        // 矩形範囲の半分（中心から左右に広げる）
        float halfX = generateRange_X * 0.5f;
        float halfZ = generateRange_Y * 0.5f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 pos = new Vector3(
                center.x + Random.Range(-halfX, halfX),
                center.y,
                center.z + Random.Range(-halfZ, halfZ)
            );

            if (!IsInForbiddenArea(pos))
                return pos;
        }

        Debug.LogWarning("有効な生成位置が見つかりませんでした。デフォルト位置を返します。");
        return new Vector3(100f, 2f, 100f); // 最後の保険
    }
    //敵の生成禁止区域かを判断するメソッド
    private bool IsInForbiddenArea(Vector3 position)
    {
        foreach (ForbiddenVolume volume in forbiddenVolumes)
        {
            Bounds bounds = volume.ToBounds();
            if (bounds.size.x <= 0 || bounds.size.y <= 0 || bounds.size.z <= 0)
                continue;

            if (bounds.Contains(position))
                return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

        foreach (var volume in forbiddenVolumes)
        {
            Gizmos.DrawCube(volume.center, volume.size);
        }
        if (!showGenerateRangeGizmo) return;

        // EnemyGeneratorの位置を中心に表示（生成範囲もこの中心に合わせたい場合はここが自然）
        Vector3 center = transform.position;
        center.y = gizmoY;

        // あなたの変数名に合わせる（generateRange_X, generateRange_Y）
        Vector3 size = new Vector3(
            Mathf.Max(0.01f, generateRange_X),
            0.05f, // 薄い板として見せる
            Mathf.Max(0.01f, generateRange_Y)
        );

        Gizmos.color = rangeFillColor;
        Gizmos.DrawCube(center, size);

        Gizmos.color = rangeWireColor;
        Gizmos.DrawWireCube(center, size);
    }

}
[System.Serializable]
public struct ForbiddenVolume
{
    public Vector3 center;
    public Vector3 size;

    public Bounds ToBounds()
    {
        Vector3 safeSize = new Vector3(
            Mathf.Max(0.01f, size.x),
            Mathf.Max(0.01f, size.y),
            Mathf.Max(0.01f, size.z)
        );
        return new Bounds(center, safeSize);
    }
}

