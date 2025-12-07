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
            Debug.LogWarning(
                $"複数の Player が存在しています。古い方({Instance.name})を残し、新しい方({name})を破棄します。");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);  // 最初の1体だけ生き残る
    }
    // Update is called once per frame
    void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
