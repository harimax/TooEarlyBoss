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

    public void StartBattle()
    {
        //プレイヤーの情報取得
        var player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<vThirdPersonController>();
        meleeManager = player.GetComponent<vMeleeManager>();
        skillManager = player.GetComponent<SkillManager>();
        //敵を生成するコンポーネントを取得
        enemyGenerator = gameObject.GetComponent<EnemyGenerator>();

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
        meleeManager.defaultDamage.damageValue = Mathf.RoundToInt(growth.PlayerPower) + meleeManager.defaultDamage.damageValue;
        meleeManager.Init();

        // 行動可能状態に
        IsPlayerMove.GetInstance().CanMove = true;

        skillManager.ActivePassiveSkill();//スキルを発動させる
        enemyGenerator.GenerateEnemy();//敵を出現させる
        battleManager.StartMission();//ミッション開始メソッドが呼ばれる
    }

}
