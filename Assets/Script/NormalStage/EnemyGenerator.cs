using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] private GameObject MobEnemy;
    [SerializeField] private List<ForbiddenVolume> forbiddenVolumes;
    [SerializeField] private int minGenerateNum;
    [SerializeField] private int maxGenerateNum;
    [SerializeField] private int generateRange_X;
    [SerializeField] private int generateRange_Y;
    private int generaterNumber;
    
    //敵を生成するメソッド
    public void GenerateEnemy()
    {
        generaterNumber = Random.Range(minGenerateNum, maxGenerateNum);
        for (int i = 0; i < generaterNumber; i++)
        {
            Instantiate(MobEnemy, generatePosition(), gameObject.transform.rotation);
        }
    }
    //敵の生成位置を決めるメソッド
    private Vector3 generatePosition()
    {
        const int maxAttempts = 100;

    for (int attempt = 0; attempt < maxAttempts; attempt++)
    {
        Vector3 pos = new Vector3(
            Random.Range(0, generateRange_X),
            2,
            Random.Range(0, generateRange_Y)
        );

        if (!IsInForbiddenArea(pos))
            return pos;
    }

    Debug.LogWarning("有効な生成位置が見つかりませんでした。デフォルト位置を返します。");
    return new Vector3 (100f,2f,100f); // 最後の保険
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

