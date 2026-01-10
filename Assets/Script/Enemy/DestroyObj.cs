using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyobj : MonoBehaviour
{
    [SerializeField] private float destroyTime;
    // Start is called before the first frame update
    void Start()
    {
        // 2秒後にこのゲームオブジェクトを破壊
        Destroy(gameObject, destroyTime);
    }
}
