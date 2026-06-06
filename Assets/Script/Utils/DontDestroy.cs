using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    public static DontDestroy Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 重複を削除
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

public static class PlayerLocator
{
    public const string PlayerTag = "Player";

    // Playerタグ検索を1か所に集約し、各敵・ボス側でタグ文字列を直書きしないようにする。
    public static Transform FindTransform()
    {
        return GameObject.FindGameObjectWithTag(PlayerTag)?.transform;
    }

    // 呼び出し側でnull判定を忘れにくくするため、取得成功/失敗をboolで返す。
    public static bool TryFindTransform(out Transform player)
    {
        player = FindTransform();
        return player != null;
    }

    // Trigger/CollisionからPlayerだけを受け取りたい処理用。タグ判定とTransform取得を同じ経路にまとめる。
    public static bool TryGetTransformFromCollider(Collider collider, out Transform player)
    {
        player = null;

        if (collider == null || !collider.CompareTag(PlayerTag))
        {
            return false;
        }

        player = collider.transform;
        return true;
    }

    // Player配下のTargetなど、狙い先用の子オブジェクト探索を共通化する。
    public static bool TryFindChildWithTag(Transform parent, string childTag, out Transform child)
    {
        child = null;

        if (parent == null || string.IsNullOrEmpty(childTag))
        {
            return false;
        }

        foreach (Transform current in parent)
        {
            if (current.CompareTag(childTag))
            {
                child = current;
                return true;
            }
        }

        return false;
    }
}
