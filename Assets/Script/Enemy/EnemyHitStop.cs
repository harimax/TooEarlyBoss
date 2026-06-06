using Invector;
using Invector.vMelee;
using UnityEngine;

public class EnemyHitStop : AnimatorHitStop
{
    private vHealthController healthController;

    private void Start()
    {
        healthController = GetComponent<vHealthController>();

        // 敵は被ダメージ時と攻撃ヒット時の両方で、共通のAnimator停止処理を呼び出す。
        if (healthController != null)
        {
            healthController.onReceiveDamage.AddListener(DamageHitStop);
        }

        // 攻撃判定を持たない敵もいるため、存在する場合だけヒットイベントを購読する。
        var damageComponent = GetComponentInChildren<vObjectDamage>();
        if (damageComponent != null)
        {
            damageComponent.onHit.AddListener(AttackHitStop);
        }
    }

    private void DamageHitStop(vDamage damage)
    {
        PlayHitStop();
    }

    private void AttackHitStop(Collider other)
    {
        PlayHitStop();
    }
}
