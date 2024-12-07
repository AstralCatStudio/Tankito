using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualExplosions : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.normalizedTime >= 0.9f) // && stateInfo.IsName("Explosion")
        {
            Destroy(gameObject);
        }
    }
}
