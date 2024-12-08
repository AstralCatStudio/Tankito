using Tankito.SinglePlayer;
using UnityEngine;

public class Mine : MonoBehaviour
{
    private MinerBehaviour minerReference;
    public string activacionTrigger;
    public Animator animator;
    private bool isActivated = false;
    private bool isDestroyed = false;
    public LayerMask enemyLayer;
    public GameObject explosionEffect;

    private void Start()
    {
        MusicManager.Instance.PlaySound("snd_teletransporte");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Explode();
        }
        else if (collision.CompareTag("Enemy") && collision.GetComponent<MinerBehaviour>() == null && !isActivated)
        {
            Activate();
        }
        else if (collision.CompareTag("Bullet"))
        {
            if (collision.isTrigger)
            {
                HandleBulletCollision();
            }
        }
    }

    private void HandleBulletCollision()
    {
        if (!isActivated)
        {
            Activate();
        }
        else
        {
            Explode();
        }
    }

    private void Activate()
    {
        if (isActivated) return; // Evitar múltiples activaciones

        isActivated = true;
        MusicManager.Instance.PlaySound("snd_mina");
        if (animator != null && !string.IsNullOrEmpty(activacionTrigger))
        {
            animator.SetTrigger(activacionTrigger);
        }
    }

    private void Explode()
    {
        if (isDestroyed) return; // Evitar múltiples explosiones

        isDestroyed = true;
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    public void SetMinerReference(MinerBehaviour miner)
    {
        minerReference = miner;
    }
}
