using UnityEngine;
using Cysharp.Threading.Tasks;
using Game.Boss;

public class SmileBossControll : MonoBehaviour,IBossController
{
    public enum BossState { Idle, BallShot, Laser, RockFall, ShotSmile, Dead }
    [Header("参照")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform ObjectSpawnPoint;
    private BossState currentState;
    private Transform player;
    private Animator animator;
    private bool _isPaused = false;
    
    [SerializeField] private int rockCount = 5;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float spawnHeight = 10f;
    // public Transform characterRoot; // キャラクターの正面（Y軸）を基準にする
    public float attackInterval = 5f;
    private float timer;

    private void Start()
    {
        currentState = BossState.Idle;
        timer = attackInterval;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = gameObject.GetComponent<Animator>();
    }

    private void Update()
    {
        if (_isPaused)
        {
            return;
        }
        //一定時間ごとに攻撃してくる
        timer -= Time.deltaTime;
        if (currentState == BossState.Idle)
        {
            BossUtilities.FaceTargetYaw(transform, player, 5f);
        }
        if (timer <= 0f && currentState == BossState.Idle)
        {
            ChooseAttack().Forget();
            timer = attackInterval;
        }
    }
    /// <summary>
    /// ランダムに攻撃を選択する   
    /// </summary>
    private async UniTask ChooseAttack()
    {
        int choice = Random.Range(0, 4);
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
            case 3:
                await ShotSmileAsync();
                break;
        }
    }
    private async UniTask BallShotAsync()
    {
        Debug.Log("弾攻撃");
        currentState = BossState.BallShot;
        animator.SetTrigger("ballAttack");
        await UniTask.Delay(4000); // 4秒待機

        currentState = BossState.Idle;

    }
    private async UniTask FireLaserAsync()
    {
        Debug.Log("レーザー攻撃");
        currentState = BossState.Laser;
        animator.SetTrigger("laserAttack");

        await UniTask.Delay(7000); // 7秒待機

    }

    private async UniTask RockFallAsync()
    {
        Debug.Log("岩攻撃");
        currentState = BossState.RockFall;
        animator.SetTrigger("RockAttack");

        await UniTask.Delay(3000); // 3秒待機
        currentState = BossState.Idle;

    }
    private async UniTask ShotSmileAsync()
    {
        Debug.Log("突進攻撃");
        currentState = BossState.ShotSmile;
        animator.SetTrigger("DashAttack");

        await UniTask.Delay(3000); // 3秒待機

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
            ball.GetComponent<Rigidbody>().linearVelocity = dir.normalized * 10f;
        }
    }

    public async UniTask OnLaserEvent()
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
    //岩を落とす攻撃イベント
    public void OnRockFallEvent()
    {
        if (player == null) return;

        Vector3 center = player.transform.position;
        for (int i = 0; i < rockCount; i++)
        {
            // ランダムなXZ位置に落とす
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(center.x + offset.x, center.y + spawnHeight, center.z + offset.y);

            Instantiate(rockPrefab, spawnPos, Quaternion.identity);
        }
    }
    public void DeadTrigger()
    {
        currentState = BossState.Dead;
        animator.SetTrigger("Dead");
    }
    /// <summary>
    /// ボスを完全に止める
    /// </summary>
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
        // enabled = true;
        if (animator != null) animator.enabled = true;
        // タイマー初期化
        timer = attackInterval;
    }
}
