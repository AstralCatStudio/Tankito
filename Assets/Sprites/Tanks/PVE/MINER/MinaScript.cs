using UnityEngine;

public class Mine : MonoBehaviour
{
    public string activacionTrigger;
    public Animator animator;
    private bool isActivated = false;
    private bool isDestroyed = false;
    public LayerMask enemyLayer;
    public GameObject explosionEffect;

    private void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("Trigger");

        if (isDestroyed) return;

        if (collision.CompareTag("Player"))
        {
            Explode();
        }
        else if (enemyLayer == (enemyLayer | (1 << collision.gameObject.layer)) && !isActivated)
        {
            Activate();
        }
        else if (collision.CompareTag("Bullet"))
        {
            if (isActivated)
            {
                Explode();
            }
            else
            {
                Activate();
            }
        }
    }

    private void Activate()
    {
        isActivated = true;
        //Debug.Log("activa mina");
        if (animator != null && !string.IsNullOrEmpty(activacionTrigger))
        {
            animator.SetTrigger(activacionTrigger);
        }
    }

    private void Explode()
    {
        isDestroyed = true;
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
