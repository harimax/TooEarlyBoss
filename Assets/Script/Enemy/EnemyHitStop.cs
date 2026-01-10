using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using Invector;
using UnityEngine.PlayerLoop;
using Invector.vCharacterController;
using Invector.vMelee;

public class EnemyHitStop : MonoBehaviour
{
    private Animator animator;
    [SerializeField] int hitStopTime = 50;
    private vHealthController healthController;
    // 多重実行防止
    private bool isHitStopping;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        healthController = GetComponent<vHealthController>();
        healthController.onReceiveDamage.AddListener(DamagehitStop);
        var damageComponents = GetComponentInChildren<vObjectDamage>();
        if (damageComponents != null)
        {
            damageComponents.onHit.AddListener(AttackHitStop);
        }
    }
    //攻撃が当たったときのhitStop
    private void DamagehitStop(vDamage damage)
    {
        hitStop().Forget();

    }
    //被弾時当たったときのhitStop
    private void AttackHitStop(Collider other)
    {
        hitStop().Forget();
    }
    private async UniTask hitStop()
    {
        if (isHitStopping) return; // 多重発火を抑制（必要ならキューや最大値合成に変更）
        isHitStopping = true;
        animator.speed = 0;
        await UniTask.Delay(hitStopTime);
        animator.speed = 1;
        isHitStopping = false;
    }
}
