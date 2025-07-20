using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReaction : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private float triggerChance = 0.5f; // 確率をパラメータ化、デフォルトは50%
    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }
    public void OnReaction()
    {
        // 指定された確率でトリガーを発動
        if (Random.Range(0f, 1f) <= triggerChance)
        {
            animator.SetTrigger("TriggerReaction");
        }
        else
        {
            Debug.Log("TriggerReaction skipped this time!");
        }
    }
}
