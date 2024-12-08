using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class ExplosionScript : MonoBehaviour
    {
        [SerializeField] private float explosionRadius = 2.5f;
        [SerializeField] private int damage = 1;
        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();
            Explode();

            MusicManager.Instance.PlaySoundPitch("snd_explosion");
        }

        private void Update()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.normalizedTime >= 1.0f && stateInfo.IsName("Explosion"))
            {
                Destroy(gameObject);
            }
        }

        private void Explode()
        {
            Debug.Log("Explosion triggered!");

            Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (var hit in hitObjects)
            {
                switch (hit.gameObject.tag)
                {
                    case "Player":
                        ApplyDamageToPlayer(hit);
                        break;

                    case "Enemy":
                        ApplyDamageToEnemy(hit);
                        break;

                    default:
                        Debug.Log($"Object {hit.name} with tag {hit.tag} is not affected by the explosion.");
                        break;
                }
            }
        }

        private void ApplyDamageToPlayer(Collider2D hit)
        {
            var playerData = hit.GetComponent<PVECharacterData>();
            if (playerData != null)
            {
                playerData.TakeDamage(damage);
                //Debug.Log($"Player {hit.name} took {damage} damage.");
            }
            else
            {
                Debug.LogWarning($"PVECharacterData not found on object: {hit.name}");
            }
        }

        private void ApplyDamageToEnemy(Collider2D hit)
        {
            var enemyData = hit.GetComponent<PVEEnemyData>();
            if (enemyData != null)
            {
                enemyData.TakeDamage(damage);
                //Debug.Log($"Enemy {hit.name} took {damage} damage.");
            }
            else
            {
                Debug.LogWarning($"PVEEnemyData not found on object: {hit.name}");
            }
        }
    }
}
