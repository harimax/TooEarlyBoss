using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestory : MonoBehaviour
{
    public static DontDestory Instance { get; private set; }
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
