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
    private bool _isPaused = false;
    [SerializeField] private Transform[] waypoints; // 巡回ポイント
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("MoveSpeed", _agent.velocity.magnitude);
        if (_agent.remainingDistance <= waypointThreshold)
        {
            GoToNextWaypoint();
        }
    }
    private void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        // Debug.Log("現在のポイント:" + currentWaypoint);
        _agent.SetDestination(waypoints[currentWaypoint].position);

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
}
