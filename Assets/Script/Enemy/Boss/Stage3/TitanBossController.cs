using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Game.Boss;
/**
 * タイタンボスの行動全体を制御するクラス
 * - 状態遷移（Idle / AttackXXX / CoolDown / Dead）
 * - 距離に応じた攻撃パターン選択
 * - 突進・岩投げ・ジャンプ衝撃波の実行
 */
public class TitanBossController : MonoBehaviour, IBossController
{
    enum TitanBossState
    {
        Idle,// 待機中（プレイヤーを見つめるだけ） 
        AttackSprint,//突進
        AttackThrow,//岩を投げる遠距離攻撃
        AttackJump,//ジャンプして地面に衝撃波
        CoolDown,// cooldown状態
        Dead//死亡時（今後追加）
    }
    [Header("References")]
    [SerializeField] private TitanBossSprintAttack sprintAttack;  // 突進担当
    [SerializeField] private TitanBossRangeAttack rangedAttack;  // 飛び道具担当
    [Header("Behaviour")]
    [SerializeField] private float playerDistance = 10f;          // 近距離/遠距離の境目
    [SerializeField, Range(0, 100)] private int jumpAttackProbability = 20; // ジャンプ攻撃選択確率（％）
    [SerializeField] private float throwFaceTime = 0.5f;          // 岩投げ前に向き直る時間
    private Transform player;
    private Animator animator;
    private TitanBossState currentState = TitanBossState.CoolDown;//最初はIdle状態にする
    private bool isPaused = false;
    private bool isjumpAttack = false;
    private bool cooling;
    CancellationTokenSource sprintCts;

    // Start is called before the first frame update
    void Awake()
    {
        player = PlayerLocator.FindTransform();
        animator = gameObject.GetComponent<Animator>();
    }
    private void OnDestroy()
    {
        sprintCts?.Cancel();
        sprintCts?.Dispose();
    }

    void Update()
    {
        if (isPaused || currentState == TitanBossState.Dead) return;

        switch (currentState)
        {
            case TitanBossState.CoolDown:
                HandleCoolDown().Forget();
                break;

            case TitanBossState.Idle:
                HandleIdle();
                break;

            // Attack 中はアニメーション / SprintAsync に任せる
            case TitanBossState.AttackSprint:
            case TitanBossState.AttackThrow:
            case TitanBossState.AttackJump:
                break;
        }
    }

    // =========================
    //  状態ごとのハンドラ
    // =========================
    /// <summary>クールダウン状態（1回だけ起動）</summary>
    private async UniTask HandleCoolDown()
    {
        if (cooling) return;

        cooling = true;
        try
        {
            Debug.Log("クールダウン中...");
            await UniTask.Delay(2000, cancellationToken: this.GetCancellationTokenOnDestroy());
            animator.SetTrigger("FinishBreakTime");
            currentState = TitanBossState.Idle;
        }
        finally
        {
            cooling = false;
            SelectJumpAttack(); // 次の攻撃でジャンプを使うか決める
        }
    }
    /// <summary>Idle状態：プレイヤーとの距離&ランダムに応じて攻撃を選ぶ</summary>
    private void HandleIdle()
    {
        if (!player) return;

        float distance = PlayerDistanceCheck();

        // まずジャンプ攻撃を優先
        if (isjumpAttack)
        {
            StartJumpAttack();
            return;
        }

        // 近距離 → 岩投げ
        if (distance < playerDistance)
        {
            StartThrowAttack();
        }
        // 遠距離 → 突進
        else
        {
            StartSprintAttack();
        }
    }
    // =========================
    //  攻撃開始メソッド
    // =========================

    private void StartSprintAttack()
    {
        if (!sprintAttack) return;

        sprintCts?.Cancel();
        sprintCts?.Dispose();
        sprintCts = new CancellationTokenSource();

        currentState = TitanBossState.AttackSprint;
        SprintAttack(sprintCts.Token).Forget();
    }

    private async UniTaskVoid SprintAttack(CancellationToken ct)
    {
        try
        {
            await sprintAttack.SprintAttackAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // キャンセル時は何もしない
        }

        if (!ct.IsCancellationRequested && currentState != TitanBossState.Dead)
        {
            currentState = TitanBossState.CoolDown;
        }
    }

    private void StartThrowAttack()
    {
        if (!sprintAttack) return;

        currentState = TitanBossState.AttackThrow;
        // 少しだけプレイヤー方向を向いてから投げる
        sprintAttack.FacePlayerForSeconds(throwFaceTime).Forget();
        animator.SetTrigger("AttackThrow");
    }

    private void StartJumpAttack()
    {
        currentState = TitanBossState.AttackJump;
        animator.SetTrigger("AttackJump");
    }
    public void PauseBoss()
    {
        isPaused = true;
        sprintCts?.Cancel();
    }
    /// <summary>
    /// ボスの動きを再開する
    /// </summary>
    public void ResumeBoss()
    {
        isPaused = false;
        if (animator != null) animator.enabled = true;
        // タイマー初期化
    }
    //プレイヤーとの距離を計算
    private float PlayerDistanceCheck()
    {
        return BossUtilities.DistanceToTarget(transform, player);
    }
    //攻撃選択ロジック
    private void SelectJumpAttack()
    {
        int roll = UnityEngine.Random.Range(0, 100);
        if (roll < 20)
        {
            isjumpAttack = true;
        }
        else
        {
            isjumpAttack = false;
        }
    }
    /// <summary>ジャンプ攻撃（アニメーションイベントから呼ばれる想定）</summary>
    public void JumpAttack()
    {
        rangedAttack?.JumpAttack();
    }

    /// <summary>岩投げ（アニメーションイベントから呼ばれる）</summary>
    public void OnRockThrowEvent()
    {
        rangedAttack?.RockThrowAttack();
    }
    /// <summary>
    /// Idle状態へ戻す
    /// </summary>
    public void OnAttackEnd()
    {
        currentState = TitanBossState.CoolDown;
    }
    public void DeadTrigger()
    {
        currentState = TitanBossState.Dead;
        sprintCts?.Cancel();
        sprintAttack?.StopImmediately();
        animator.SetTrigger("Dead");
    }
}
