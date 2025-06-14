using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SmileBossControll : MonoBehaviour
{
    public enum BossState { Idle, BallShot, Laser, RockFall }
    private BossState currentState;
    private Transform player;
    private Animator animator;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private int rockCount = 5;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float spawnHeight = 10f;
    // public Transform characterRoot; // キャラクターの正面（Y軸）を基準にする

    public Transform ObjectSpawnPoint;
    public float attackInterval = 5f;
    private float timer;

    private void Start()
    {
        currentState = BossState.Idle;
        timer = attackInterval;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = this.gameObject.GetComponent<Animator>();
    }

    private void Update()
    {
        //一定時間ごとに攻撃してくる
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            // ChooseAttack();
            timer = attackInterval;
        }
    }
    private async UniTask ChooseAttack()
    {
        int choice = Random.Range(0, 3);
        switch (choice)
        {
            case 0:
                await BallShotAsync();
                break;
            case 1:
                await FireLaserAsync();
                break;
            case 2:
                await RockFallAsync();
                break;
        }
    }
    private async UniTask BallShotAsync()
    {
        Debug.Log("弾攻撃");
        currentState = BossState.BallShot;
        animator.SetTrigger("ballAttack");
        await UniTask.Delay(3000); // 1.5秒待機

        currentState = BossState.Idle;

    }
    private async UniTask FireLaserAsync()
    {
        Debug.Log("レーザー攻撃");
        currentState = BossState.Laser;
        animator.SetTrigger("LaserAttack");

        await UniTask.Delay(3000); // 1.5秒待機

        currentState = BossState.Idle;

    }

    private async UniTask RockFallAsync()
    {
        Debug.Log("岩攻撃");
        currentState = BossState.RockFall;
        animator.SetTrigger("RockAttack");

        await UniTask.Delay(3000); // 1.5秒待機

        currentState = BossState.Idle;
    }
    //弾を飛ばずアニメーションイベント
    public void OnBallShotEvent()
    {
        float[] angles = { -30f, -15f, 0f, 15f, 30f }; // 左から右へ角度を振る


        // 「正しい発射方向」を補正して取得
        Vector3 baseForward = -ObjectSpawnPoint.right; // ← ここが正しい正面！
        Vector3 up = ObjectSpawnPoint.up;

        foreach (float angle in angles)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, up);
            Vector3 dir = rotation * baseForward;

            GameObject ball = Instantiate(ballPrefab, ObjectSpawnPoint.position, Quaternion.identity);
            ball.GetComponent<Rigidbody>().velocity = dir.normalized * 10f;
        }
    }

    public async UniTaskVoid OnLaserEvent()
    {
        laser.SetActive(true);
        float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float rotationSpeed = 90f; // 1秒間に90度回転
            gameObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            await UniTask.Yield(); // フレームごとに処理を継続
        }

        laser.SetActive(false);

        // 状態を戻す
        currentState = BossState.Idle;
    }

    public void OnRockFallEvent()
    {
        Vector3 center = gameObject.transform.position;
        for (int i = 0; i < rockCount; i++)
        {
            // ランダムなXZ位置に落とす
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(center.x + offset.x, center.y + spawnHeight, center.z + offset.y);

            Instantiate(rockPrefab, spawnPos, Quaternion.identity);
        }
    }

}
