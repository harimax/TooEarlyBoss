using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExprosion : MonoBehaviour
{
    [SerializeField] private GameObject ExplosionObj;

    public void DeadExplosion()
    {
        Instantiate(ExplosionObj.gameObject, transform.position, transform.rotation);
    }
}
