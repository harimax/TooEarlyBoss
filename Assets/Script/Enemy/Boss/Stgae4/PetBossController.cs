using System.Collections;
using System.Collections.Generic;
using Invector;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Cysharp.Threading.Tasks.CompilerServices;


public class PetBossController : MonoBehaviour
{
    enum BossState { Idle, Face, Charge, Rest, RangedAttack }

    [Header("Refs")]
    private Animator animator;
    Transform player;

    [Header("Flow Timings")]
    [SerializeField] float faceTime = 0.7f;      // ② 向き直り（ため）時間
    [SerializeField] float maxChargeTime = 4.0f; // ③ 突進の最大継続時間（秒）
    [SerializeField] float restTime = 2.5f;      // ④ 休憩時間（秒）

    [Header("Charge Parameters")]
    [SerializeField] float chargeSpeed = 12.0f;     // 突進速度（m/s）
    [SerializeField] float overshootDistance = 5.0f;// プレイヤー位置をどれだけ通り過ぎるか（m）
    [SerializeField] float turnSpeed = 10f;         // ②の向き合わせスピード
    [SerializeField] bool lockYPosition = true;     // 地面がフラットならtrueでY固定

    [Header("VFX (Optional)")]
    [SerializeField] GameObject AttackEffect;
    BossState phase = BossState.Idle;
    [Header("Pattern")]
    [SerializeField] int chargesBeforeRanged = 3; // ★ 3回突進したら遠距離
    private int chargeCount = 0;
    private bool _isPaused = true;
    [Header("Ranged (Optional)")]
    [SerializeField] private GameObject rock;       // ★ 玉のPrefab（未設定なら遠距離はスキップ）
    [SerializeField] private Transform throwPoint;   // ★ 発射位置（未設定なら本体前方から）
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private string rangedTriggerName = "RangedAttack"; // アニメトリガ（必要なら）
    private bool isRockAttack = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!animator) animator = GetComponent<Animator>();
        if (rock) isRockAttack = true;
        Debug.Log(rock, this);
        BattleLoop().Forget();
    }
    async UniTaskVoid BattleLoop()
    {
        Debug.Log("BattleLoop開始", this);
        while (true)
        {
            // ② 向く（ため）
            phase = BossState.Face;
            float t = 0f;
            while (t < faceTime)
            {
                FacePlayerOnlyYaw(turnSpeed);
                animator?.SetFloat("MoveSpeed", 0f);
                t += Time.deltaTime;
                await UniTask.Yield();
            }
            // ③ 突進：向きをロックして一直線に移動
            phase = BossState.Charge;
            Vector3 startPos = transform.position;
            Vector3 toPlayer = player.position - startPos; toPlayer.y = 0f;
            Vector3 chargeDir = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : transform.forward;

            // プレイヤーを overshoot する距離をゴールに
            float targetDistance = toPlayer.magnitude + Mathf.Max(0f, overshootDistance);
            animator?.SetBool("Moveable", true);

            float traveled = 0f;
            float elapsed = 0f;
            float baseY = startPos.y;

            while (elapsed < maxChargeTime && traveled < targetDistance)
            {
                float step = chargeSpeed * Time.deltaTime;
                transform.position += chargeDir * step;
                if (lockYPosition)
                {
                    var p = transform.position; p.y = baseY; transform.position = p;
                }
                elapsed += Time.deltaTime;
                traveled += step;
                await UniTask.Yield();
            }
            // ★ 突進を1回終えたのでカウントアップ
            chargeCount++;

            // ④ 休憩
            phase = BossState.Rest;
            animator?.SetBool("Moveable", false);
            Debug.Log("突進終了", this);
            await UniTask.Delay(TimeSpan.FromSeconds(restTime));

            // === ★ ここで「次は遠距離か？」を判定 ===
            if (isRockAttack && chargeCount >= Mathf.Max(1, chargesBeforeRanged))
            {
                phase = BossState.RangedAttack;
                // 遠距離攻撃ルーチン（中で向き直し→アニメ→1発撃つ→クールダウン → 復帰）
                await RangedAttackAysnc();
                // 遠距離後はパターンをリセット
                chargeCount = 0;
            }
        }       
    }
    async UniTask RangedAttackAysnc()
    {
        // すこしだけ向き直し（見た目を整える・必要十分）
        float t = 0f;
        float quickFace = 0.35f; // 速めのため
        while (t < quickFace)
        {
            FacePlayerOnlyYaw(turnSpeed * 2f);
            t += Time.deltaTime;
            await UniTask.Yield();
        }
        // アニメトリガ（ある場合）
        if (!string.IsNullOrEmpty(rangedTriggerName))
        {
            animator?.SetTrigger(rangedTriggerName);
        }
        // 遠距離後の短いクールダウン（休憩）
        float rangedRest = Mathf.Max(0.6f, restTime * 0.5f); // 例：最低0.6秒 or restTimeの半分
        await UniTask.Delay(TimeSpan.FromSeconds(rangedRest));
    }

    void FacePlayerOnlyYaw(float slerpSpeed)
    {
        if (!player) return;
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        var target = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * slerpSpeed);
    }

    public void ThorwDamageRock()
    {
        float[] angles = { -5f, 5f }; // 左から右へ角度を振る
        Vector3 baseForward = throwPoint.forward;
        foreach (float angle in angles)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, throwPoint.up);
            Vector3 dir = rotation * baseForward;
            GameObject Initrock = Instantiate(rock, throwPoint.position, Quaternion.identity);
            Initrock.GetComponent<Rigidbody>().linearVelocity = dir.normalized * projectileSpeed;
        }
    }

    public void DeadTrigger()
    {
        animator.SetTrigger("Dead");
    }
    public void EnableEffect()
    {
        if (AttackEffect) AttackEffect.SetActive(true);
    }
    public void DisableEffect()
    {
        if (AttackEffect) AttackEffect.SetActive(false);
    }
    public void ResumeBoss()
    {
        _isPaused = false;
    }
}