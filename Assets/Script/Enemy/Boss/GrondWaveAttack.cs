using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrondWaveAttack : MonoBehaviour
{
    [SerializeField] private GameObject GrondWave;

    public void OnStartGrondWave()
    {
        Debug.Log("OnStartGrondWave");
        GameObject grondWave = Instantiate(GrondWave, gameObject.transform.position, gameObject.transform.rotation);
    }
    
}
