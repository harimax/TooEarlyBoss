using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryEffect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 2秒後にこのゲームオブジェクトを破壊
        Destroy(gameObject, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
