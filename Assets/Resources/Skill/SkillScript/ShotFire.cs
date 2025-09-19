using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotFire : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Transform firePoint;

    public bool hasSkill = false;

    // アニメーションイベントから呼ばれる
    public void TryFireFireball()
    {
        if (!hasSkill) return;

        GameObject ball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        ball.GetComponent<Rigidbody>().linearVelocity = firePoint.forward.normalized * 10f;
    }
}
