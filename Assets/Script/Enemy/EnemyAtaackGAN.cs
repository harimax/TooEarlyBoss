using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyAtaackGAN : MonoBehaviour
{
    [SerializeField] private GameObject GANObj;
    [SerializeField] private GameObject preliminaryEffect;
    [SerializeField] private float shotSpeed;
    [SerializeField] protected float attackcooldown;
    [SerializeField] private GameObject ParentObj;
    public string targetTag = "Target";

    private AudioSource bulletSound;
    private Vector3 childPosition;
    private bool isOnCooldown;

    private void Start()
    {
        bulletSound = GetComponent<AudioSource>();
    }

    public void OnAttackGANEnter(Collider collider)
    {
        OnAttackGAN(collider).Forget();
    }

    public void OnShotEvent()
    {
        Shot().Forget();
    }

    //プレイヤーが範囲内に入れば起動するメソッド-------------------------------------
    private async UniTask OnAttackGAN(Collider collider)
    {
        // Player以外のColliderでは照準・発射処理を進めない。
        if (!PlayerLocator.TryGetTransformFromCollider(collider, out var player))
        {
            return;
        }

        // 弾の狙い先はPlayer本体ではなく、Player配下のTargetタグ位置を優先してキャッシュする。
        CacheTargetPosition(player);

        Vector3 directionToTarget = player.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        ParentObj.transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);

        // クールダウン中は同じTrigger滞在で弾が連射されないようにする。
        if (!isOnCooldown)
        {
            await Shot();
        }
    }
    //弾の発射--------------------------------------------------------------------------
    public async UniTask Shot()
    {
        Debug.Log("Fire");
        isOnCooldown = true;
        preliminaryEffect.SetActive(true);

        // 予備動作エフェクトを見せてから、キャッシュ済みTarget方向へ発射する。
        await UniTask.Delay(TimeSpan.FromSeconds(attackcooldown));

        Vector3 direction = (Targetpos() - transform.position).normalized;
        GameObject shot = Instantiate(GANObj.gameObject, transform.position, transform.rotation);
        shot.GetComponent<Rigidbody>().linearVelocity = direction * shotSpeed;

        if (bulletSound != null)
        {
            bulletSound.Play();
        }

        preliminaryEffect.SetActive(false);
        await UniTask.Delay(TimeSpan.FromSeconds(attackcooldown));
        isOnCooldown = false;
        Destroy(shot, 3.0f);
    }

    private Vector3 Targetpos()
    {
        // 発射直前にPlayer/Target位置を取り直し、古い座標に撃ち続けないようにする。
        if (PlayerLocator.TryFindTransform(out var player))
        {
            CacheTargetPosition(player);
        }

        return childPosition;
    }

    private void CacheTargetPosition(Transform player)
    {
        // Target子オブジェクトが見つからない場合は、前回キャッシュした座標を維持する。
        if (PlayerLocator.TryFindChildWithTag(player, targetTag, out var target))
        {
            childPosition = target.position;
        }
    }
}
