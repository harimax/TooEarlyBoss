using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using Invector;
using UnityEngine.PlayerLoop;
using Invector.vCharacterController;
using Invector.vMelee;
public class PlayerHitStop : MonoBehaviour
{
    private Animator animator;
    [SerializeField] int hitStopTime = 50;
    private vThirdPersonController controller;
    private vMeleeManager meleeManager;
     // 多重実行防止
    private bool isHitStopping;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<vThirdPersonController>();
        meleeManager = GetComponent<vMeleeManager>();
        controller.onReceiveDamage.AddListener(DamagehitStop);
        meleeManager.onDamageHit.AddListener(AttackHitStop);
    }
    // ダメージ時のhitstop発動
    private void DamagehitStop(vDamage damage)
    {
        hitStop().Forget();
    }
    //攻撃が当たったときののhitstop発動
    private void AttackHitStop(vHitInfo hitInfo)
    {
        hitStop().Forget();
    }
    // ヒットストップ本体
    private async UniTask hitStop()
    {
        if (isHitStopping) return; // 多重発火を抑制（必要ならキューや最大値合成に変更）
        isHitStopping = true;
        // Debug.Log("HitStop");
        animator.speed = 0;
        await UniTask.Delay(hitStopTime);
        animator.speed = 1;
        isHitStopping = false;
    }
    
}
