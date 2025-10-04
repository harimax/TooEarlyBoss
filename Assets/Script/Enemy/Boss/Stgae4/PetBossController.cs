using System.Collections;
using System.Collections.Generic;
using Invector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;

public class PetBossController : MonoBehaviour
{
    public enum PetStateEnum
    {
        Idle,//待機状態
        Chase,// 移動可能（通常状態）
        Attack,  // 攻撃中
        Die// 死亡状態
    }
    private NavMeshAgent _agent;
    private Transform player;
    protected Animator animator;
    [SerializeField] private float retreatDistance = 3.5f;
    [SerializeField] private float reengageDelay = 3.0f;
    [SerializeField] private Vector2 reengageJitter = new Vector2(-0.6f, 0.6f); // ランダムゆらぎ

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("MoveSpeed", _agent.velocity.magnitude);
        if (_agent.remainingDistance <= retreatDistance)
        {
            _agent.destination = player.position;
        }

    }

    /// <summary>
    /// 攻撃範囲にプレイヤーが入ったとき呼ばれる
    /// </summary>
    public void OnAttackRangeEnter(Collider collider)
    {
        Debug.Log(collider);
        if (!collider.CompareTag("Player")) return;
        //攻撃モーションをする
        // animator.SetTrigger("Attack");
        Debug.Log("攻撃");
        _agent.isStopped = true;
    }

    public void OnAttackRangeExit(Collider collider)
    {
        if (!collider.CompareTag("Player"))
            return;
        //攻撃モーションをする
        // animator.SetTrigger("Attack");
        Debug.Log("攻撃範囲外");
    }

}
