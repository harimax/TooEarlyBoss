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
    public new void DamageReaction()
    {
        if (vHealthController.currentHealth > 0)
        {
            // ランダムでリアクション発生
            if (Random.value < Reaction_Pro)
            {
                base.DamageReaction();
                // 攻撃コライダーなどを一時無効にするクールダウン
                DamagecooldownCoroutine = Cooldown();  // UniTaskで処理を実行
            }
        }
        // 体力が0以下でまだ死亡状態になっていない場合
        else if (_status.State != StateEnum.Die)
        {
            base.OnDie(); // 基底クラスの死亡処理を実行
            DestroyCoroutine(1.5f).Forget(); // 4秒後にオブジェクト削除
        }
    }
    //死亡コルーチン--------------------------------------------------
    private async UniTask DestroyCoroutine(float time)
    {
        await UniTask.Delay((int)(time*1000));
        Destroy(gameObject);
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