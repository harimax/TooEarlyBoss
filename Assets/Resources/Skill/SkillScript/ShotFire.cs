using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotFire : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Transform firePoint;

    public bool hasFireballSkill = false;

    // アニメーションイベントから呼ばれる
    public void TryFireFireball()
    {
        if (!hasFireballSkill) return;

        GameObject ball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        ball.GetComponent<Rigidbody>().velocity = firePoint.forward.normalized * 10f;
    }
}
