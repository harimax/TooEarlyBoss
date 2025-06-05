using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerStomp : MonoBehaviour
{
    private Rigidbody rb;

    private void Start() 
    {
        rb=gameObject.GetComponent<Rigidbody>();
    }
    public float bounceForce=10f;//踏んだ時の跳ね返り
    
    private void OnTriggerEnter(Collider other) {
        
        if(other.CompareTag("KURIBO"))
        {
            KURIBO enemy =other.GetComponent<KURIBO>();
            
            //敵が存在しており、落下中に踏みつけると
            if(enemy!=null && rb.velocity.y<0)
            {
                enemy.Die();//敵が死ぬ
                rb.velocity=new Vector3(rb.velocity.x,bounceForce,rb.velocity.z);//上方向に跳ね返る
            }
        }
    }
}
