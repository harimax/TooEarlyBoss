using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;

public class TitanBossController : MonoBehaviour
{
    enum TitanBossState
    {
        Idle,// 待機中（プレイヤーを見つめるだけ） 
        Preparing, //攻撃前の準備
        DashAttack,//突進
        ThrowRock,//岩を投げる遠距離攻撃
        SpinAttack,//回転弾幕攻撃
        JumpShockWave,//ジャンプして地面に衝撃波
        Retreating,// 距離を取る動き
        Dead//死亡時（今後追加）
    }
    [Header("Reference")]
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Ranges")]
    [SerializeField] private float detectRange;
    [SerializeField] private float meleeRange;
    [SerializeField] private float tooCloseRange;
    [SerializeField] private float retreatDistance;

    [Header("Timing")]
    [SerializeField] private float idleWaitTime;

    [Header("Special Attack Chances")]
    [Range(0f, 1f)][SerializeField] private float spinAttackChance = 0.2f;  // 回転攻撃が出る確率
    [Range(0f, 1f)][SerializeField] private float jumpAttackChance = 0.2f;  // ジャンプ攻撃が出る確率

    private TitanBossState currentState = TitanBossState.Idle;//最初はIdle状態にする
    private bool isActing = false;

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = this.gameObject.GetComponent<NavMeshAgent>();
        animator = this.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == TitanBossState.Idle)
        {
            transform.LookAt(player);
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance < detectRange && !isActing)
            {
                DecideNextAction().Forget(); // 非同期実行;
            }
        }
    }
    /// <summary>
    /// 次に行う攻撃を決定するロジック
    /// </summary>

    private async UniTask DecideNextAction()
    {
        isActing = true;
        currentState = TitanBossState.Preparing;

        await UniTask.Delay(1000); // 攻撃のための「溜め」時間

        float distance = Vector3.Distance(transform.position, player.position);

        //特殊攻撃(ランダム)
        if (Random.value < spinAttackChance)
        {
            currentState = TitanBossState.SpinAttack;
            await SpinAttack();
            ResetToIdle();
            return;
        }

        if (Random.value < jumpAttackChance)
        {
            currentState = TitanBossState.JumpShockWave;
            await JumpShockwave();
            ResetToIdle();
            return;
        }

        //通常攻撃
        if (distance < tooCloseRange)
        {
            // 近すぎる → 距離を取って遠距離攻撃
            currentState = TitanBossState.Retreating;
            await RetreatThenThrowRock();
        }

        else if (distance < meleeRange)
        {
            // 適正距離 → 突進
            currentState = TitanBossState.DashAttack;
            await DashAttack();
        }
        else
        {
            // 遠すぎる → 岩投げ
            currentState = TitanBossState.ThrowRock;
            await DashAttack();
        }
        ResetToIdle();
    }

    /// <summary>
    /// 突進攻撃（アニメのみ）
    /// </summary>
    private async UniTask DashAttack()
    {
        Debug.Log("突進攻撃");
        animator.SetTrigger("DashAttack");
        await UniTask.Delay(1000); // 攻撃のための「溜め」時間
        //当たり判定
    }

    /// <summary>
    /// 遠距離攻撃（岩を投げる）
    /// </summary>
    private async UniTask ThrowRock()
    {
        Debug.Log("岩投げ攻撃");
        animator.SetTrigger("Throw");
        await UniTask.Delay(1000); // 攻撃のための「溜め」時間
        // 岩の生成処理を追加予定
    }

    /// <summary>
    /// 回転して弾幕を撒く攻撃
    /// </summary>
    private async UniTask SpinAttack()
    {
        Debug.Log("回転攻撃");
        animator.SetTrigger("SpinAttack");
        await UniTask.Delay(1000); // 攻撃のための「溜め」時間
        // 弾の発射処理を追加
    }

    /// <summary>
    /// ジャンプして地面に衝撃波
    /// </summary>
    private async UniTask JumpShockwave()
    {
        Debug.Log("ジャンプ衝撃波");
        animator.SetTrigger("JumpAttack");
        await UniTask.Delay(1000); // 攻撃のための「溜め」時間
        // 衝撃波の生成処理
    }

    /// <summary>
    /// プレイヤーが近すぎるとき、後退してから岩投げ
    /// </summary>
    private async UniTask RetreatThenThrowRock()
    {
        Debug.Log("後退してから岩投げ");

        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 retreatPos = transform.position + dir * retreatDistance;

        agent.SetDestination(retreatPos);
        animator.SetTrigger("BackThrowLock");

        // NavMeshAgentが目的地に到達するまで待機
        await UniTask.WaitUntil(() => !agent.pathPending && agent.remainingDistance < 1f);

        // animator.SetBool("isWalking", false);

        // 後退完了 → 岩投げ
        currentState = TitanBossState.ThrowRock;
        await ThrowRock();
    }


    /// <summary>
    /// Idle状態へ戻す
    /// </summary>
    void ResetToIdle()
    {
        isActing = false;
        currentState = TitanBossState.Idle;
    }
}
