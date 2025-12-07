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
    [SerializeField] private GameObject[] AttackBeam;
    [SerializeField] private GameObject ChargeEffect;
    private bool isLooking = false;

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
        if (_isPaused) return;
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
                ChangeBeamBool(false);
                animator.SetBool("IsAttack", false);

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

        if (isLooking == true)
        {
            LookAtPlayerXZ();
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
        
    }
    /// <summary>
    /// ボスの動きを再開する
    /// </summary>
    public void ResumeBoss()
    {
        _isPaused = false;
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
        ChangeBeamBool(true); ;
        isLooking = false;
    }
    public void StopAttackBeam()
    {
        Debug.Log("ビーム停止", this);
        ChangeBeamBool(false);
    }
    public void StartChargeEffect()
    {
        Debug.Log("チャージ開始", this);
        ChargeEffect.SetActive(true);
        isLooking = true;
    }
    public void StopChargeEffect()
    {
        Debug.Log("チャージ停止", this);
        ChargeEffect.SetActive(false);
    }

    private void ChangeBeamBool(bool value)
    {
        foreach (GameObject beam in AttackBeam)
        {
            if (beam == null) continue;            // ★ Destroy済みは飛ばす
            if (beam.activeSelf == value) continue;
            beam.SetActive(value);
        }
    }
    //ビームの数が減少するメソッド
    public void DeleteBeam()
    {
        List<GameObject> aliveBeamsList = new List<GameObject>();
        //残っているビームをリストアップ
        foreach (GameObject beam in AttackBeam)
        {
            if (beam != null) aliveBeamsList.Add(beam);
        }
        // 消す本数を決定（残り本数が2未満ならその数に調整）
        int countToDelete = 2;
        // ランダムに選んで削除
        for (int i = 0; i < countToDelete; i++)
        {
            int rand = Random.Range(0, aliveBeamsList.Count);
            GameObject target = aliveBeamsList[rand];
            aliveBeamsList.RemoveAt(rand); // リストから除外

            if (target != null)
            {
                Destroy(target);
                Debug.Log($"[BeamManager] ビームを削除しました。({target.name})");
            }
        }
    }
    public void DeadTrigger()
    {
        // ボス死亡時の処理
        animator.SetTrigger("Dead");
    }
}
