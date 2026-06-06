using System.Collections;
using UnityEngine;
using Invector;
using Cinemachine;
using Invector.vMelee;
using Invector.vCharacterController;
using Cysharp.Threading.Tasks;
using System;

/**
    * 戦闘開始時の初期化を行うクラス
    */
public class BattleStartController : MonoBehaviour
{
    private vThirdPersonController playerController;
    private vMeleeManager meleeManager;
    private EnemyGenerator enemyGenerator;
    private SkillManager skillManager;
    [SerializeField] private BattleManager battleManager;

    /// <summary>
    /// 戦闘開始時の初期化処理      
    /// </summary>
    public void StartBattle()
    {
        //プレイヤーの情報取得
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Debug.Log($"[StartBattle] FindWithTag Player = {player.name}");
            Debug.Log($"[StartBattle] hierarchy path = {GetHierarchyPath(player.transform)}");
        }
        playerController = player.GetComponent<vThirdPersonController>();
        meleeManager = player.GetComponent<vMeleeManager>();
        skillManager = player.GetComponent<SkillManager>();
        //敵を生成するコンポーネントを取得
        enemyGenerator = gameObject.GetComponent<EnemyGenerator>();

        if (playerController == null)
        {
            Debug.LogError("[StartBattle] vThirdPersonController が見つかりません");

        }

        if (meleeManager == null)
        {
            Debug.LogError("[StartBattle] vMeleeManager が見つかりません");

        }

        if (skillManager == null)
        {
            Debug.LogError("[StartBattle] SkillManager が見つかりません");

        }

        if (enemyGenerator == null)
        {
            Debug.LogError("[StartBattle] EnemyGenerator が見つかりません");

        }

        if (battleManager == null)
        {
            Debug.LogError("[StartBattle] battleManager が未設定です");

        }

        // -------------------------------
        // ① 育成パラメータを取得
        // -------------------------------
        PlayerGrowRepository.LoadParameters(out var growth);

        //プレイヤーの初期化を呼び出す
        playerController.ResetMaxHealth();
        playerController.ResetMaxStamina();
        meleeManager.InitAttackParameter();//攻撃力を初期値に戻す
        //修行したパラメータを加算させる
        playerController.AddMaxStamina(growth.PlayerStamina);
        playerController.AddMaxHealth(growth.PlayerHealth);
        meleeManager.defaultDamage.damageValue = Mathf.RoundToInt(growth.PlayerPower) / 2 + meleeManager.defaultDamage.damageValue;
        meleeManager.Init();

        // 戦闘開始時は育成・準備由来の移動停止をまとめて解除
        var moveState = IsPlayerMove.GetInstance();
        if (moveState != null)
        {
            moveState.ClearAllBlocks();
        }

        skillManager.ActivePassiveSkill();//スキルを発動させる
        enemyGenerator.GenerateEnemy();//敵を出現させる
        battleManager.StartMission();//ミッション開始メソッドが呼ばれる
    }
    private string GetHierarchyPath(Transform current)
    {
        string path = current.name;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }
        return path;
    }

}
