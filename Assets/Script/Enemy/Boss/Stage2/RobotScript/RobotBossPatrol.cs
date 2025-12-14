using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// TreeBoss の巡回移動を担当するクラス。
/// - NavMeshAgent でウェイポイントを巡回
/// - 各ポイントで一定時間停止
/// </summary>

public class RobotBossPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints;     // 巡回ポイント
    [SerializeField] private float waypointThreshold = 0.5f; // 到達判定距離
    [SerializeField] private float waitSeconds = 5f;    // 各地点で止まる時間

    private NavMeshAgent agent;
    private Animator animator;

    private int currentWaypoint = 0;
    private bool isWalking = false;
    private bool isWaitingAtWaypoint = false;

    private CancellationToken _ct;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        _ct = this.GetCancellationTokenOnDestroy();

        if (agent != null)
            agent.isStopped = true; // 最初は停止
    }

    // Update is called once per frame
    void Update()
    {
        if (!isWalking || agent == null) return;

        // 経路計算中・待機中はスキップ
        if (agent.pathPending || isWaitingAtWaypoint) return;

        // 到達判定
        if (agent.remainingDistance <= waypointThreshold)
        {
            WaitAtWaypointAsync().Forget();
        }
    }
    /// <summary>
    /// HP が一定値以下になったら巡回開始(RobotBossControllerから呼ぶ)
    /// </summary>
    public void StartPatrol()
    {
        if (waypoints.Length == 0 || agent == null) return;

        isWalking = true;
        agent.isStopped = false;
        currentWaypoint = 0;

        agent.SetDestination(waypoints[currentWaypoint].position);
        animator.SetBool("Walking", true);
    }

    /// <summary>
    /// 各ポイントで停止する非同期処理
    /// </summary>
    private async UniTaskVoid WaitAtWaypointAsync()
    {
        isWaitingAtWaypoint = true;

        //一旦停止
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // 指定時間待機
        await UniTask.Delay((int)(waitSeconds * 1000), cancellationToken: _ct);

        if (isWalking && agent != null)
        {
            // 次のポイントへ移動
            agent.isStopped = false;
            GoToNextWaypoint();
        }

        isWaitingAtWaypoint = false;
    }

    /// <summary>
    /// 次の巡回ポイントへ移動
    /// </summary>
    private void GoToNextWaypoint()
    {
        if (waypoints.Length == 0 || agent == null) return;

        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypoint].position);
    }
}
