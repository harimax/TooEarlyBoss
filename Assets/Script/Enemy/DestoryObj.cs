using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destoryobj : MonoBehaviour
{
    [SerializeField] private float DestroyTime;
    // Start is called before the first frame update
    void Start()
    {
        // 2秒後にこのゲームオブジェクトを破壊
        Destroy(gameObject, DestroyTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
