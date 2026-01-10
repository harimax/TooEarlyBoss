using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneNotRotate : MonoBehaviour
{
    [SerializeField] private Quaternion worldRotation = Quaternion.identity;

    void LateUpdate()
    {
        transform.rotation = worldRotation;
    }
}
