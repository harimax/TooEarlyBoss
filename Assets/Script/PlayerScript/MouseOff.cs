using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse_off : MonoBehaviour
{
    public static Mouse_off Instance { get; private set; }

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
    // Update is called once per frame
    void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
