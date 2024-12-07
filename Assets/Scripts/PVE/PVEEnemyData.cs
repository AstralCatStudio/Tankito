using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class PVEEnemyData : PVECharacterData
    {
        [SerializeField]
        private Sprite positionIndicatorSprite;
        [SerializeField]
        private GameObject player;
        private GameObject positionIndicator;
        public delegate void EnemyDeathEvent();
        public event EnemyDeathEvent OnDeath;
        public Vector2 hitPoint;

        protected override void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            positionIndicator = CreatePositionIndicator();
            m_maxHealth = GetComponent<AgentController>().npcData.health;
            AttackerBehaviour attacker = GetComponent<AttackerBehaviour>();
            if(attacker != null)
            {
                attacker.maxAttackerHp = m_maxHealth;
            }
            base.Start();
        }

        public override void Die()
        {
            OnDeath?.Invoke();
            base.Die();
            Debug.Log("Se crea resto para revivir con el necromancer");


            //Instantiate(aguaExplosionPrefab, transform.position, Quaternion.identity);
            //Instantiate(cristalExplosionPrefab, transform.position, Quaternion.identity);


            if (GetComponent<AgentController>().npcData.leftoversInDeath != null)
            {
                Instantiate(GetComponent<AgentController>().npcData.leftoversInDeath, transform.position, Quaternion.identity);
            }
        }

        void Update()
        {
            if (IsEnemyOutOfScreen())
            {
                positionIndicator.SetActive(true);
                int layerMask = LayerMask.GetMask("ScreenEdges");
                Vector2 direction = (player.transform.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, layerMask);
                Debug.DrawRay(transform.position, direction * 100, Color.red);
                hitPoint = hit.point;

                if (hit.collider != null)
                {
                    //positionIndicator.transform.position = hit.point;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
                    positionIndicator.transform.rotation = Quaternion.Euler(0, 0, angle);
                    DetectCollisionSide(hit);
                    float distance = (player.transform.position - transform.position).magnitude;
                    float maxDistance = 100f;
                    float normalizedDistance = distance / maxDistance;
                    Vector2 scaleFromDistance = (Vector2.one/5) / Mathf.Clamp(normalizedDistance, 0.2f, 0.7f);
                    positionIndicator.transform.localScale = scaleFromDistance;
                }
            } else
            {
                positionIndicator.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitPoint, 0.5f);
        }

        void DetectCollisionSide(RaycastHit2D hit)
        {
            BoxCollider2D boxCollider = hit.collider.GetComponent<BoxCollider2D>();
            if (boxCollider == null) return;

            Bounds bounds = boxCollider.bounds;
            Vector2 hitPoint = hit.point;
            Vector2 pos = Vector2.zero;
            float offset = 1f;

            // Calcula distancias desde el punto de impacto a los lados
            float leftDistance = Mathf.Abs(hitPoint.x - bounds.min.x);
            float rightDistance = Mathf.Abs(hitPoint.x - bounds.max.x);
            float topDistance = Mathf.Abs(hitPoint.y - bounds.max.y);
            float bottomDistance = Mathf.Abs(hitPoint.y - bounds.min.y);

            // Determina el lado m�s cercano
            float minDistance = Mathf.Min(leftDistance, rightDistance, topDistance, bottomDistance);

            if (Mathf.Approximately(minDistance, leftDistance))
            {
                pos = new Vector2(hitPoint.x + offset, hitPoint.y);
            }
            else if (Mathf.Approximately(minDistance, rightDistance))
            {
                pos = new Vector2(hitPoint.x - offset, hitPoint.y);
            }
            else if (Mathf.Approximately(minDistance, topDistance))
            {
                pos = new Vector2(hitPoint.x, hitPoint.y - offset);
            }
            else if (Mathf.Approximately(minDistance, bottomDistance))
            {
                pos = new Vector2(hitPoint.x, hitPoint.y + offset);
            }
            positionIndicator.transform.position = pos;
        }

        private bool IsEnemyOutOfScreen()
        {
            Camera cam = Camera.main;
            Vector3 viewportPosition = cam.WorldToViewportPoint(transform.position);

            if (viewportPosition.x < 0 || viewportPosition.x > 1 ||
                viewportPosition.y < 0 || viewportPosition.y > 1)
            {
                return true;
            }
            return false;
        }

        GameObject CreatePositionIndicator()
        {
            GameObject positionIndicator = new GameObject("PositionIndicator");
            positionIndicator.transform.SetParent(transform);
            SpriteRenderer renderer = positionIndicator.AddComponent<SpriteRenderer>();
            renderer.sprite = positionIndicatorSprite;
            renderer.color = Color.red;
            renderer.renderingLayerMask = 3;
            positionIndicator.SetActive(false);
            return positionIndicator;
        }
    }
}

