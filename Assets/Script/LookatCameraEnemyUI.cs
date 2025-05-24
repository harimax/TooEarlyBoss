using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookatCameraEnemyUI : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        //敵の体力UIをカメラに向ける
        transform.LookAt(Camera.main.transform);
    }
}
