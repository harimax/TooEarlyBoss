using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] private GameObject MobEnemy;
    private int generaterNumber;
    [SerializeField] private int minGenerateNum;
    [SerializeField] private int maxGenerateNum;
    [SerializeField] private int generateRange;
    void Start()
    {
        //生成する数を決める
        generaterNumber = Random.Range(minGenerateNum, maxGenerateNum);
        for (int i = 0; i < generaterNumber; i++)
        {
            Instantiate(MobEnemy,generatePosition(),gameObject.transform.rotation);
        }

    }

    private Vector3 generatePosition()
    {
        return new Vector3(Random.Range(0,generateRange),2,Random.Range(0,generateRange));
    }

}
