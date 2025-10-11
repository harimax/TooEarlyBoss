using UnityEngine;
using Invector;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

public class MessengerBossController : MonoBehaviour, IBossController
{
    private NavMeshAgent _agent;
    private Animator animator;
    private float waypointThreshold = 0.5f; // 到達判定距離
    private int currentWaypoint = 0;
    private Transform player;
    [SerializeField] private float runTime;
    [SerializeField] private float stopTime;
    private float timer;               // 累積タイマー（秒）
    private bool wasRunning = false;   // 前フレームが走行だったか
    private bool _isPaused = false;
    [SerializeField] private Transform[] waypoints; // 巡回ポイント
    [SerializeField] private GameObject AttackBeam;
    [SerializeField] private GameObject ChargeEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;

        // 1) 周期から今のフェーズを算出
        timer += Time.deltaTime;
        float cycle = runTime + stopTime;
        float t = timer % cycle;
        bool running = t < runTime;

        // 2) フェーズ遷移の“瞬間”だけ処理
        if (running != wasRunning)
        {
            if (running)
            {
                // StopからRun
                _agent.isStopped = false;
                if (!_agent.hasPath) GoToNextWaypoint();
            }
            else
            {
                // RunからStop
                _agent.isStopped = true;
                animator.SetFloat("MoveSpeed", 0f);
            }
            wasRunning = running;
        }

        // 3) 各フェーズ中の処理（浅い if だけ）
        if (running)
        {
            // 走行中は目的地更新＆到達チェックのみ
            if (!_agent.pathPending && _agent.remainingDistance <= waypointThreshold)
                GoToNextWaypoint();

            animator.SetFloat("MoveSpeed", _agent.velocity.magnitude);
        }
        else
        {
            // 停止中はプレイヤーの方向だけ向く
            // LookAtPlayerXZ();
            animator.SetBool("IsAttack", true);
        }
    }
    private void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        // Debug.Log("現在のポイント:" + currentWaypoint);
        _agent.SetDestination(waypoints[currentWaypoint].position);
        animator.SetBool("IsAttack", false);

    }
    public void PauseBoss()
    {
        // Update 系を止める
        _isPaused = true;
        // MonoBehaviour の Update を無効化する場合はこちらを使ってもよい
        // enabled = false;
        // アニメータを止めたいなら：
        // if (animator != null) animator.enabled = false;
    }
    /// <summary>
    /// ボスの動きを再開する
    /// </summary>
    public void ResumeBoss()
    {
        _isPaused = false;
        // enabled = true;
        if (animator != null) animator.enabled = true;
        // タイマー初期化
    }
    private void LookAtPlayerXZ()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // Y方向無視してXZ平面で回転

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    public void StartAttackBeam()
    {
        Debug.Log("ビーム発射", this);
        AttackBeam.SetActive(true);
    }
    public void StopAttackBeam()
    {
        Debug.Log("ビーム停止", this);
    }
    public void StartChargeEffect()
    {
        Debug.Log("チャージ開始", this);
        ChargeEffect.SetActive(true);
    }
    public void StopChargeEffect()
    {
        Debug.Log("チャージ停止", this);
        ChargeEffect.SetActive(false);
    }
}
