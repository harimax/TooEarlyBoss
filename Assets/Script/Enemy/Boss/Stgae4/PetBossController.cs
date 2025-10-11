using System.Collections;
using System.Collections.Generic;
using Invector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.XR;

public class PetBossController : MonoBehaviour
{
    public enum PetStateEnum
    {
        Idle,//待機状態
        Chase,// 移動可能（通常状態）
        Attack,  // 攻撃中
        Die// 死亡状態
    }
    private PetStateEnum _state = PetStateEnum.Idle;

    [Header("Refs")]
    private NavMeshAgent _agent;
    private Transform player;
    protected Animator animator;
    [Header("Attack Timing")]
    [SerializeField] float attackWindupSec = 1.5f;  // 攻撃前の“ため”
    [SerializeField] float attackCooldownSec = 5.0f; // 攻撃後の待機
    private bool isAttackRange=false;
    [SerializeField] private GameObject Effect;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        _state = PetStateEnum.Chase;

    }

    // Update is called once per frame
    void Update()
    {
        // 範囲外 or クールタイム中の移動制御
        if (_state == PetStateEnum.Chase)
        {
            _agent.isStopped = false;
            _agent.SetDestination(player.position);
        }
        animator.SetFloat("MoveSpeed", _agent.velocity.magnitude);
    }

    /// <summary>
    /// 攻撃範囲にプレイヤーが入ったとき呼ばれる
    /// </summary>
    public void OnAttackRangeEnter(Collider collider)
    {
        // Debug.Log(collider);
        if (!collider.CompareTag("Player")) return;
        Debug.Log("攻撃範囲内");
        _agent.isStopped = true;
        isAttackRange=true;
        AttackLoopAsync().Forget();

    }
    public void OnAttackRangeExit(Collider collider)
    {
        if (!collider.CompareTag("Player"))
            return;
        isAttackRange = false;
        Debug.Log("攻撃範囲外");
    }

    /// <summary>
    /// 範囲内にいる間だけ：1.5s ため → Attack → 5s 待機 を繰り返す
    /// 範囲外になったら終了して追跡に戻る
    /// </summary>
    private async UniTask AttackLoopAsync()
    {
        _state = PetStateEnum.Attack;
        await UniTask.Delay(System.TimeSpan.FromSeconds(attackWindupSec));
        Debug.Log("攻撃開始");

        animator.SetTrigger("Attack"); //攻撃開始
        _state = PetStateEnum.Idle;
        await UniTask.Delay(System.TimeSpan.FromSeconds(attackCooldownSec));// 攻撃後のクールダウン
        Debug.Log("攻撃終了");
        if (!isAttackRange == true)
        {
            _state = PetStateEnum.Chase;
        }
        else
        {
            AttackLoopAsync().Forget();
        }
    }

    public void startAttackEffect()
    {
        Effect.SetActive(true);
    }
    public void stopAttackEffect()
    {
        Effect.SetActive(false);
    }
}
