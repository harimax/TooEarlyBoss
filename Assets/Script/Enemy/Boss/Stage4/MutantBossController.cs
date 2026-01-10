using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

public class MutantBossController : MonoBehaviour, IBossController
{
    [Header("Attack & VFX")]
    [SerializeField] private GameObject[] AttackBeam;
    [SerializeField] private GameObject ChargeEffect;
    private MutantBossMovementController movement;
    private Animator animator;
    private Transform player;
    private bool _isPaused = false;
    private bool isLooking = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        movement = GetComponent<MutantBossMovementController>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // 最初は「停止中はプレイヤーの方を見る」状態にしておく
        isLooking = true;

        // 移動フェーズ切り替えにあわせてアニメーション等を制御
        if (movement != null)
        {
            movement.OnRunStarted  += HandleRunStarted;
            movement.OnStopStarted += HandleStopStarted;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPaused) return;
        // 移動更新
        movement?.Tick(Time.deltaTime);
        //移動中なら走るアニーメーション
        if (movement.IsRunning)
        {
            animator.SetFloat("MoveSpeed", movement.CurrentSpeed);
        }
        // 停止中：攻撃モーション＋向き合わせ
        else
        {
            animator.SetBool("IsAttack", true);

            if (isLooking)
            {
                LookAtPlayerXZ();
            }
        }
    }

    /// <summary>
    /// 走行フェーズ開始時
    /// </summary>
    private void HandleRunStarted()
    {
        animator.SetBool("IsAttack", false);
        // 走行を開始したら、次の停止までは「向きを合わせる」前提にしておく
        isLooking = true;
        StopAttackBeam();
    }
    /// <summary>
    /// 停止フェーズ開始時
    /// </summary>
    private void HandleStopStarted()
    {
        animator.SetFloat("MoveSpeed", 0f);
        // 停止中の基本は「向きを合わせる」が、チャージ中は isLooking=false にする
    }
    public void PauseBoss()
    {
        // Update 系を止める
        _isPaused = true;
        movement?.Pause();

    }
    /// <summary>
    /// ボスの動きを再開する
    /// </summary>
    public void ResumeBoss()
    {
        _isPaused = false;
        if (animator != null) animator.enabled = true;
        movement?.Resume();
    }
    /// <summary>
    /// プレイヤーの方を向く    
    /// </summary>
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
    /// <summary>
    /// ビームのON/OFFを一括変更
    /// </summary>
    private void ChangeBeamBool(bool value)
    {
        foreach (GameObject beam in AttackBeam)
        {
            if (beam == null) continue;            // ★ Destroy済みは飛ばす
            if (beam.activeSelf == value) continue;
            beam.SetActive(value);
        }
    }
    /// <summary>
    /// ビームの数が減少するメソッド
    /// </summary>
    public void DeleteBeam()
    {
        List<GameObject> aliveBeamsList = new List<GameObject>();
        //残っているビームをリストアップ
        foreach (GameObject beam in AttackBeam)
        {
            if (beam != null) aliveBeamsList.Add(beam);
        }
        // 消す本数を決定
        int countToDelete = 2;
        // ランダムに選んで削除
        for (int i = 0; i < countToDelete; i++)
        {
            int rand = Random.Range(0, aliveBeamsList.Count);
            GameObject target = aliveBeamsList[rand];
            aliveBeamsList.RemoveAt(rand); // リストから除外

            if (!target) continue;

            Destroy(target);
            Debug.Log($"[BeamManager] ビームを削除しました。({target.name})");
        }
    }
    public void DeadTrigger()
    {
        // ボス死亡時の処理
        animator.SetTrigger("Dead");
        PauseBoss();
    }
}
