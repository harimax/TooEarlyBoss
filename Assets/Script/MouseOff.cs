using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse_off : MonoBehaviour
{
    //プレイヤーを
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
