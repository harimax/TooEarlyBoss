using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class StopEnemy : MobEnemy
{
    [SerializeField] protected Collider AttackRangecollider;
    [SerializeField] protected Collider Damagecollider;
    [SerializeField] protected float Damagecooldown=2.0f;
    [SerializeField] private GameObject GanGenerator;
    protected MobEnemy _status;
    [SerializeField] private float destroytime;
    [SerializeField] private  float Reaction_Pro=0.5f; //ダメージリアクションを起こす確率(値が大きいほど確率高い)
    private UniTask DamagecooldownCoroutine;
    private vHealthController vHealthController;
    
    protected override void Start()
    {
        base.Start(); //基本クラスのstartを呼び出す
        _status=GetComponent<MobEnemy>();
        vHealthController = GetComponent<vHealthController>();
    }
    //ダメージリアクション関数継承
    public override void DamageReaction()
    {
        // 固定砲台型の敵は、共通のHP/死亡判定にクールダウン開始だけを渡す。
        ResolveDamageReaction(
            vHealthController,
            Reaction_Pro,
            1.5f,
            () => DamagecooldownCoroutine = Cooldown());
    }
    //ダメージ受けたらクールダウンする
    public async UniTask Cooldown()
    {
        // Debug.Log("クールダウン");
        AttackRangecollider.enabled=false;
        Damagecollider.enabled=false;
        await UniTask.Delay((int)(Damagecooldown * 1000)); // 秒からミリ秒に変換
        AttackRangecollider.enabled=true;
        // Debug.Log("再開");
        _status.ReturnToNormal();
    }
 }
