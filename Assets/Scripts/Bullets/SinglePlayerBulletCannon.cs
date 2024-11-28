using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Tankito.SinglePlayer
{
    public class SinglePlayerBulletCannon : MonoBehaviour, IBulletCannon
    {
        float coolDown = 1;
        [SerializeField]
        float baseInterval = 0.5f;
        float timer = 0;
        List<GameObject> bulletsShot = new List<GameObject>();
        private List<Vector2> BulletDirections = new List<Vector2>();
        public int baseBulletAmount;
        public int m_bulletAmount;
        int spawnTickTime = 0;
        [SerializeField]
        float m_shootRadius, m_shootSpreadAngle, m_scatterAngle;

        void Update()
        {
            timer += Time.deltaTime;
        }

        public void Shoot(Vector2 aimVector)
        {
            if (timer >= coolDown)
            {
                timer = 0;
                float angle = Mathf.Atan2(aimVector.y, aimVector.x);
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                ShootBullet((Vector2)transform.parent.parent.parent.position + m_shootRadius * aimVector, direction, 1);
            }
        }

        public void ShootBullet(Vector2 position, Vector2 direction, int spawnN)
        {
            var newBullet = BulletPool.Instance.Get(position, direction, OwnerClientId, SimClock.TickCounter, spawnN);
        }

    }
}
