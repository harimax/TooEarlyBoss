using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGenerator : MonoBehaviour
{
    [SerializeField] private GameObject Sphere;//生成する球体
    [SerializeField] private float waitTime;//間隔時間
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnSphere(waitTime));
    }

    /// <summary>
    /// waitTimeの感覚で球体を生成する
    /// </summary>
    /// <param name="waitTime"></param>
    /// <returns></returns>
    private IEnumerator SpawnSphere(float waitTime)
    {
        while(true)
        {
            Instantiate(Sphere,gameObject.transform.position,gameObject.transform.rotation);
            yield return new WaitForSeconds(waitTime);
        }
        
    }
}
