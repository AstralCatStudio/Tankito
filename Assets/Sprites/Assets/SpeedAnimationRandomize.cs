using UnityEngine;

public class SpeedAnimationRandomize : MonoBehaviour
{
    public Animator animator; 
    public float minSpeed = 0.5f; 
    public float maxSpeed = 1.5f; 

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator != null)
        {
            RandomizeAnimationSpeed();
        }
    }


    private void RandomizeAnimationSpeed()
    {
        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        animator.speed = randomSpeed;
        //Debug.Log($"Velocidad de animación establecida a: {randomSpeed}");
    }
}
