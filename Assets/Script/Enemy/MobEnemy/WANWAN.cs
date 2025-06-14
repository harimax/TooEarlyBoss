using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WANWAN : MonoBehaviour
{
    enum State { Patrolling, Alert, Charging, Cooldown }
    private State currentState = State.Patrolling; // 初期状態は巡回

    private Transform player;
    public float chargeDelay;
    public float chargeSpeed; //突進スピード
    public float chargeDistance;//移動速度の限界値
    public float cooldownTime;//クールタイム時間
    public float patrolRadius; // 巡回範囲の半径

    private NavMeshAgent agent;
    private Vector3 patrolCenter; // 巡回の中心点
    private Vector3 chargeDirection;//プレイヤーに向くベクトル
    private Vector3 chargeStartPosition; // 突進開始時の位置
    private bool isCharging = false;    //突進中のフラグ
    private bool playerDetected = false; // プレイヤー検知フラグ

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolCenter = transform.position; // 初期位置を巡回の中心とする
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(Patrol());
    }
    private void Update()
    {
        //巡回中にプレイヤーに視界に入ったら警戒状態へ
        if (currentState == State.Patrolling)
        {
            if (playerDetected)
            {
                StartCoroutine(Alert());
            }
        }
        //追跡ステートかつ追跡中ならば追跡をする
        else if (currentState == State.Charging && isCharging)
        {
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            //移動制限を超えると追跡を辞める
            if (Vector3.Distance(chargeStartPosition, transform.position) >= chargeDistance)
            {
                StopCharging();
            }
        }
    }

    //巡回処理
    IEnumerator Patrol()
    {
        //巡回中なら中心位置の周りを巡回する
        while (currentState == State.Patrolling)
        {

            Vector3 randomPoint = patrolCenter + new Vector3(
                Random.Range(-patrolRadius, patrolRadius),
                0,
                Random.Range(-patrolRadius, patrolRadius)
            );

            //NavMesh内のポイントを設定
            agent.SetDestination(randomPoint);
            yield return new WaitForSeconds(3f);
        }
    }
    //プレイヤーを見つけたら数秒待機
    IEnumerator Alert()
    {
        chargeStartPosition = gameObject.transform.position;
        currentState = State.Alert;
        agent.isStopped = true;
        yield return new WaitForSeconds(chargeDelay);
        StartCharging();

    }

    //突進開始
    void StartCharging()
    {
        currentState = State.Charging;//状態を突進に変更
        chargeDirection = (player.position - transform.position).normalized;
        isCharging = true;
        Debug.Log("プレイヤー検知");
    }

    //突進終了処理
    void StopCharging()
    {
        isCharging = false;
        StartCoroutine(Cooldown()); // クールダウンに移行
    }
    // クールダウン処理
    IEnumerator Cooldown()
    {
        currentState = State.Cooldown;
        yield return new WaitForSeconds(cooldownTime);
        agent.isStopped = false;
        currentState = State.Patrolling;
        StartCoroutine(Patrol());
    }

    // **警戒エリアにプレイヤーが入ったら検知**
    public void OnDetectObject(Collider collider)
    {

        if (collider.CompareTag("Player"))
        {
            playerDetected = true;

        }
    }

    // **警戒エリアからプレイヤーが出たらリセット**
    public void ExitOnDetectObject(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            playerDetected = false;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (currentState == State.Charging)
        {
            StopCharging();
        }
    }


}
