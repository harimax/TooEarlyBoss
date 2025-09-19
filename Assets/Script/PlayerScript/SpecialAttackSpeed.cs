using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAttackSpeed : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    void SlowAttack()
    {
        animator.speed = 0.3f;
    }
    void SpeedAttack()
    {
        animator.speed = 2.0f;
    }
    void FinishAttack()
    {
        animator.speed = 1.0f;
    }
}
