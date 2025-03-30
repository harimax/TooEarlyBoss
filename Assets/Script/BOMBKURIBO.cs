using System.Collections;
using System.Collections.Generic;
using Invector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class BOMBKURIBO : MonoBehaviour
{
    //基本は歩いておいて、プレイヤーが近づくとずっと追いかけ来る
    //そして数秒後爆発して死ぬ
    public enum BOMBKURIBOstatus
    {
        Chase,
        WalkAround,
    }
    private NavMeshAgent _agent;
    [SerializeField] private GameObject exprosion;
    [SerializeField] private float patrolRadius; // 巡回範囲の半径
    private Animator _animator;
    private Rigidbody rb;
    private bool onDetectPlayer = false;
    private Collider target;
    private Vector3 patrolCenter; // 巡回の中心点
    private float time;


    public BOMBKURIBOstatus BOMBKURIBOstate = BOMBKURIBOstatus.WalkAround;
    // Start is called before the first frame update
    void Start()
    {
        _agent = gameObject.GetComponent<NavMeshAgent>();
        _animator = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody>();
        patrolCenter = transform.position; // 初期位置を巡回の中心とする
        StartCoroutine(Patrol());
    }

    // Update is called once per frame
    void Update()
    {
        _animator.SetFloat("MoveSpeed", _agent.velocity.magnitude);
        //追跡フラグがオンになると追跡する
        if (onDetectPlayer == true)
        {
            Debug.Log("追跡中");
            _agent.destination = target.transform.position;
            time += Time.deltaTime;
            //五秒間追いかけたのちに爆発する
            if (time > 5f)
            {
                Exprosion();
                time = 0;
            }

        }

    }
    //追跡範囲に入る追跡を行う
    public async void OnDetectObjectChase(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {

            if (BOMBKURIBOstate == BOMBKURIBOstatus.WalkAround)
            {
                await DetectAction();
            }
            target = collider;
            onDetectPlayer = true;
        }
    }
    //見つけたリアクションを起こすメソッド
    private async UniTask DetectAction()
    {
        rb.AddForce(Vector3.up * 0.15f, ForceMode.Impulse);
        await UniTask.Delay(500);
        BOMBKURIBOstate = BOMBKURIBOstatus.Chase;
        _agent.isStopped = false;
        // Debug.Log("見つけた");
    }
    //爆発するメソッド
    private async void Exprosion()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.3f; // 2倍のサイズにする
        for (float t = 0; t < 0.1f; t += Time.deltaTime)
        {
            if (this == null) return; // ★すでにオブジェクトが破棄されたら終了
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t / 0.1f);
            await UniTask.Yield();
        }
        Debug.Log("爆発");
        Instantiate(exprosion, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(gameObject);//敵を消す処理

    }

    //巡回処理
    IEnumerator Patrol()
    {
        //巡回中なら中心位置の周りを巡回する
        while (BOMBKURIBOstate == BOMBKURIBOstatus.WalkAround)
        {

            Vector3 randomPoint = patrolCenter + new Vector3(
                Random.Range(-patrolRadius, patrolRadius),
                0,
                Random.Range(-patrolRadius, patrolRadius)
            );

            //NavMesh内のポイントを設定
            _agent.SetDestination(randomPoint);
            yield return new WaitForSeconds(3f);
        }
    }
}
