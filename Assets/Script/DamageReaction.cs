using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReaction : MonoBehaviour
{
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator=gameObject.GetComponent<Animator>();
    }
    public void OnReaction()
    {
        animator.SetTrigger("TriggerReaction");
    }
}
