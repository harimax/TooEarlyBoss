using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDrone : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint; // 弾の発射位置
    [SerializeField] private float fireCooldown = 3f;

    private float lastFireTime = -Mathf.Infinity;

    public void OnDetectEnemy(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("敵を捕捉しました");
            if (Time.time - lastFireTime >= fireCooldown)
            {
                FireAtTarget(other.transform);
                lastFireTime = Time.time;
            }
        }
    }
    private void FireAtTarget(Transform target)
    {
        if (projectilePrefab == null) return;

        // 弾を生成
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        //特殊パラメータの攻撃力を弾に割り当てる
        var damageComponent = projectileObj.GetComponentInChildren<Invector.vObjectDamage>();
        if (damageComponent != null)
        {
            if (PlayerGrowRepository.LoadParameters(out var growth))
            {
                damageComponent.damage.damageValue = Mathf.Ceil(growth.PlayerSpecial / 2f);
            }
        }

        // 弾に方向を教える
        Vector3 direction = (target.position - firePoint.position).normalized;
        bullentMove projectile = projectileObj.GetComponent<bullentMove>();
        if (projectile != null)
        {
            projectile.SetDirection(direction);
        }
    }
}
