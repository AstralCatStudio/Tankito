using UnityEngine;

public class ExplosionScript : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1.0f && stateInfo.IsName("Explosion"))
        {
            Destroy(gameObject);
        }
    }
}
