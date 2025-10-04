using System.Collections;
using System.Collections.Generic;
using Invector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;


public class KURIBO : MonoBehaviour
{
    public enum KURIBOStatus
    {
        Chase,
        Dead,
        WalkAround,
        Stunned     // ノックバック後の硬直
    }
    public float patrolRadius; // 巡回範囲の半径
    private NavMeshAgent _agent;
    private Animator _animator;
    private Rigidbody rb;
    private Vector3 patrolCenter; // 巡回の中心点

    public KURIBOStatus KURIBOstate = KURIBOStatus.WalkAround;


    void Start()
    {
        _agent = gameObject.GetComponent<NavMeshAgent>();
        _animator = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody>();
        patrolCenter = transform.position; // 初期位置を巡回の中心とする
        StartCoroutine(Patrol());

    }
    void Update()
    {
        _animator.SetFloat("MoveSpeed", _agent.velocity.magnitude);
    }
    /// <summary>
    /// 自身が死ぬメソッド
    /// </summary>
    public async void Die()
    {
        // NavMeshAgentを無効化して、移動を止める
        KURIBOstate = KURIBOStatus.Dead;

        //踏まれたら伸縮しする
        Vector3 originalScale = transform.localScale;
        for (float t = 0; t < 0.1f; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t / 0.1f);
            await UniTask.Yield();
        }
        Destroy(gameObject);//敵を消す処理
        Debug.Log("敵倒した");
    }
    /// <summary>
    /// 範囲内に入れば追いかけてくるメソッド
    /// </summary>
    /// <param name="collider"></param>
    public async void OnDetectObjectChase(Collider collider)
    {
        // 死亡している場合は追跡をしない
        if (KURIBOstate == KURIBOStatus.Dead || _agent.enabled == false)
        {
            return;
        }
        if (collider.CompareTag("Player"))
        {

            if (KURIBOstate == KURIBOStatus.WalkAround)
            {
                await DetectAction();
            }

            // Debug.Log("追跡中");
            _agent.destination = collider.transform.position;

        }

    }
    public async void ExitObject(Collider collider)
    {
        // プレイヤーが範囲外に出た場合、追跡を停止
        if (collider.CompareTag("Player"))
        {
            // Debug.Log("追跡停止");

            KURIBOstate = KURIBOStatus.WalkAround;
            if (_agent.enabled)
            {
                _agent.isStopped = true;  // NavMeshAgentが有効な場合に停止
                _agent.ResetPath();  // 進行方向をリセット
            }
            await CooldownChase();
            patrolCenter = transform.position; // 初期位置を巡回の中心とする
        }
    }
    /// <summary>
    /// 発見して飛び上がるメソッド
    /// </summary>
    /// <returns></returns>
    private async UniTask DetectAction()
    {

        // _agent.enabled = false;
        rb.AddForce(Vector3.up * 0.15f, ForceMode.Impulse);
        await UniTask.Delay(500);
        KURIBOstate = KURIBOStatus.Chase;
        // _agent.enabled = true;
        _agent.isStopped = false;
        // Debug.Log("見つけた");
    }
    /// <summary>
    /// 一度出たら追跡更新を止めさせるメソッドが
    /// </summary>
    /// <returns></returns>
    private async UniTask CooldownChase()
    {
        _agent.enabled = false;
        await UniTask.Delay(500);

        // すでにオブジェクトが削除されていれば処理を中断
        if (this == null || _agent == null) return;

        _agent.enabled = true;
    }

    //巡回処理
    IEnumerator Patrol()
    {
        //巡回中なら中心位置の周りを巡回する
        while (KURIBOstate == KURIBOStatus.WalkAround)
        {

            Vector3 randomPoint = patrolCenter + new Vector3(
                Random.Range(-patrolRadius, patrolRadius),
                0,
                Random.Range(-patrolRadius, patrolRadius)
            );

            //NavMesh内のポイントを設定
            _agent.SetDestination(randomPoint);
            yield return new WaitForSeconds(2f);
        }
    }

}