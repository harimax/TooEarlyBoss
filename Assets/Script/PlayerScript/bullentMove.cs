using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullentMove : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 direction;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }
    void Start()
    {
        // 4秒後にこのゲームオブジェクトを破壊
        Destroy(gameObject, 4f);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}
