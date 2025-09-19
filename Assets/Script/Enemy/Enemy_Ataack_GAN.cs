using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class Enemy_Ataack_GAN : MonoBehaviour
{
    [SerializeField] private GameObject GANObj;
    [SerializeField] private GameObject Preliminary_Effect;
    [SerializeField] private float shotSpeed;
    [SerializeField] protected float attackcooldown;
    [SerializeField] private GameObject ParentObj;
    AudioSource BulletSound;
    private Vector3 childPosition;
    private bool isOnCooldown = false; // クールダウン中かどうかのフラグ
    public string targetTag = "Target";
    void Start() 
    {
        BulletSound=GetComponent<AudioSource>();
    }

    //プレイヤーが範囲内に入れば起動するメソッド-------------------------------------
    public void OnAttackGAN(Collider collider)
    {
        if(collider.tag=="Player")
        {
            Transform parentObject = collider.transform;
            // 子オブジェクトの数だけループ(指定のタグを見つければループ停止)
            foreach (Transform child in parentObject)
            {
                if (child.CompareTag(targetTag))
                {
                    // ターゲットの子オブジェクトの位置を取得
                    childPosition = child.position;
                    break;
                }
            }
            Vector3 directionToTarget = collider.transform.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            ParentObj.transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            
            // Debug.Log(collider);
            if(isOnCooldown==false) StartCoroutine(Shot());
        }
    }
    //弾の発射--------------------------------------------------------------------------
    public IEnumerator Shot()
    {
        Debug.Log("発射");
        isOnCooldown = true; //弾を打てる状態になる
        Preliminary_Effect.SetActive(true);
        yield return new WaitForSeconds(attackcooldown);
        var direction=(Targetpos()-gameObject.transform.position).normalized;
        var shot=Instantiate(GANObj.gameObject,this.gameObject.transform.position,this.gameObject.transform.rotation);
        shot.GetComponent<Rigidbody>().linearVelocity = direction* shotSpeed;
        //オーディオリスナーが格納されているなら音を鳴らす
        if(BulletSound!=null)
        {
            BulletSound.Play();
        }
        Preliminary_Effect.SetActive(false);
        yield return new WaitForSeconds(attackcooldown);
        isOnCooldown = false; //弾を打てない状態にする
        Destroy(shot,3.0f);
    }
    //プレイヤーの位置を取得する------------------------------------------------------
    private Vector3 Targetpos()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Transform parentObject = player.transform;
        foreach (Transform child in parentObject)
            {
                if (child.CompareTag(targetTag))
                {
                    // ターゲットの子オブジェクトの位置を取得
                    childPosition = child.position;
                    break;
                }
            }
        return childPosition;
    }
 }
