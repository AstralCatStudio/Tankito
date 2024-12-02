using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Tankito.SinglePlayer
{
    internal enum BulletType { Player = 0, Enemy = 1 }
    public class SinglePlayerBulletCannon : MonoBehaviour, IBulletCannon
    {
        [SerializeField] float coolDown = 1;
        [SerializeField]
        float timer = 0;
        [SerializeField]
        float m_shootRadius;
        [SerializeField] BulletType m_bulletType;

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
                ShootBullet((Vector2)transform.parent.parent.parent.position + m_shootRadius * aimVector.normalized, direction, 1);
                Debug.Log($"LA POSICION DEL CAÑON ES {transform.position} y la del padre {transform.parent.parent.parent.position}");
            }
        }

        public void ShootBullet(Vector2 position, Vector2 direction, int spawnN)
        {
            var newBullet = SinglePlayerBulletPool.Instance.Get(position, direction, (int)m_bulletType);
            newBullet.GetComponent<SinglePlayerBulletController>().InitializeBullet(position, direction);
            Debug.Log($"CAÑON DISPARA BALA POSICION: {position}, DIRECCION: {direction}");
        }

    }
}
