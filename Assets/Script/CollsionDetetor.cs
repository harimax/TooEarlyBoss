using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CollsionDetetor : MonoBehaviour
{
    [SerializeField] private TriggerEvent onTriggerEnter= new TriggerEvent();
    [SerializeField] private TriggerEvent onTriggerStay= new TriggerEvent();
    [SerializeField] private TriggerEvent onTriggerExit =new TriggerEvent();
    //コライダー内に居続ける設定
    private  void OnTriggerStay(Collider other)
    {
        onTriggerStay.Invoke(other);
    }
    //攻撃コライダーの設定
    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnter.Invoke(other);
    }
    private void OnTriggerExit(Collider other)
    {
        onTriggerExit.Invoke(other);
    }

    [Serializable]
    public class TriggerEvent: UnityEvent<Collider>
    {

    }
}
