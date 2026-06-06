using Cysharp.Threading.Tasks;
using Invector;
using Invector.vCharacterController;
using Invector.vMelee;
using UnityEngine;

public abstract class AnimatorHitStop : MonoBehaviour
{
    // Animatorを停止させる時間。Player/Enemyで同じ停止処理を使い、値だけInspectorで調整できるようにする。
    [SerializeField] private int hitStopTime = 50;

    private Animator animator;
    private bool isHitStopping;

    protected virtual void Awake()
    {
        // 派生クラス側はイベント接続だけに集中できるよう、Animator取得は共通側で行う。
        animator = GetComponent<Animator>();
    }

    protected void PlayHitStop()
    {
        HitStopAsync().Forget();
    }

    private async UniTask HitStopAsync()
    {
        if (isHitStopping || animator == null)
        {
            return;
        }

        // 連続ヒット時に停止処理が重なってAnimatorの速度復帰タイミングが崩れないようにする。
        isHitStopping = true;
        float previousSpeed = animator.speed;

        animator.speed = 0f;
        await UniTask.Delay(hitStopTime, cancellationToken: this.GetCancellationTokenOnDestroy());
        animator.speed = previousSpeed;

        isHitStopping = false;
    }
}

public class PlayerHitStop : AnimatorHitStop
{
    private vThirdPersonController controller;
    private vMeleeManager meleeManager;

    private void Start()
    {
        controller = GetComponent<vThirdPersonController>();
        meleeManager = GetComponent<vMeleeManager>();

        // 被ダメージと攻撃命中のどちらでも同じヒットストップ処理を再利用する。
        if (controller != null)
        {
            controller.onReceiveDamage.AddListener(DamageHitStop);
        }

        if (meleeManager != null)
        {
            meleeManager.onDamageHit.AddListener(AttackHitStop);
        }
    }

    private void DamageHitStop(vDamage damage)
    {
        PlayHitStop();
    }

    private void AttackHitStop(vHitInfo hitInfo)
    {
        PlayHitStop();
    }
}
