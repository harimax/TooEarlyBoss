using UnityEngine;
using UnityEngine.AI;
using System;

public class MutantBossMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform[] waypoints; // 巡回ポイント
    private float waypointThreshold = 0.5f; // 到達判定距離
    [SerializeField] private float runTime;
    [SerializeField] private float stopTime;
    private NavMeshAgent agent;
    private float timer;               // 累積タイマー（秒）
    private int currentWaypoint = 0;
    /// <summary>現在「走行フェーズ」かどうか</summary>
    public bool IsRunning { get; private set; }
    /// <summary>移動速度（アニメーター用）</summary>
    public float CurrentSpeed => agent ? agent.velocity.magnitude : 0f;
    /// <summary>走行フェーズに入った瞬間</summary>
    public event Action OnRunStarted;
    /// <summary>停止フェーズに入った瞬間</summary>
    public event Action OnStopStarted;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// 毎フレーム呼び出して移動を更新する
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        // フェーズ判定
        timer += deltaTime;
        float cycle = runTime + stopTime;
        float t = timer % cycle;
        bool shouldRun = t < runTime;

        // フェーズ切り替わり
        if (shouldRun != IsRunning)
        {
            IsRunning = shouldRun;
            if (IsRunning) StartRunning();
            else StartStopping();
        }

        // フェーズごとの処理
        if (IsRunning)
        {
            HandleRunning();
        }
    }
    /// <summary>
    /// 走行開始時の初期化
    /// </summary>
    private void StartRunning()
    {
        agent.isStopped = false;

        if (!agent.hasPath)
        {
            GoToNextWaypoint();
        }

        OnRunStarted?.Invoke();
    }
    /// <summary>
    /// 停止開始時の初期化
    /// </summary>
    private void StartStopping()
    {
        agent.isStopped = true;
        OnStopStarted?.Invoke();
    }
    /// <summary>
    /// 走行中：目的地到達チェックと速度反映
    /// </summary>
    private void HandleRunning()
    {
        if (!agent.pathPending && agent.remainingDistance <= waypointThreshold)
        {
            GoToNextWaypoint();
        }
    }

    private void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypoint].position);
    }

    public void Pause()
    {
        if (agent == null) return;
        agent.isStopped = true;
    }

    public void Resume()
    {
        if (agent == null) return;
        // 「走行フェーズ中」のみ再開する
        agent.isStopped = !IsRunning;
    }
}