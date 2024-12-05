using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class SinglePlayerBulletController : ABulletController
    {
        [SerializeField] float bulletVelocity;
        [SerializeField] float lifeTimeTreshold;
        [SerializeField] BulletType bulletType;

        private void OnEnable()
        {
            m_bouncesLeft = 0;
        }

        public void InitializeBullet(Vector2 position, Vector2 direction, bool triggerOnSpawnEvents = true)
        {
            transform.position = position;
            m_rb.velocity = direction.normalized * bulletVelocity;
            transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), m_rb.velocity.normalized);

            if (triggerOnSpawnEvents) OnSpawn?.Invoke(this);
        }

        private void FixedUpdate()
        {
            MoveBullet(Time.fixedDeltaTime);
        }

        protected override void MoveBullet(float deltaTime)
        {
            if (m_lifetime >= selfCollisionTreshold)
            {
                gameObject.layer = 0;
            }
            m_lifetime += deltaTime;
            OnFly.Invoke(this);
            if (m_lifetime >=lifeTimeTreshold)
            {
                //Debug.Log($"lifetime: {m_lifetime}/{BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.lifetimeTotal}");
                Detonate(true);
            }
        }

        public override void Detonate(bool lifeTimeOver = false)
        {
            OnDetonate.Invoke(this);
            SinglePlayerBulletPool.Instance.Release(this.gameObject, (int)bulletType);
        }

        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            lastCollisionNormal = collision.GetContact(0).normal;
            switch (bulletType)
            {
                case BulletType.Player:
                    switch (collision.gameObject.tag)
                    {
                        case "NormalWall":
                            if (m_bouncesLeft <= 0)
                            {
                                Detonate();
                            }
                            else
                            {
                                m_bouncesLeft--;
                            }
                            break;

                        case "BouncyWall":
                            break;

                        case "Player":
                            Detonate();
                            collision.gameObject.GetComponent<PVECharacterData>().TakeDamage(1);
                            break;

                        case "Enemy":
                            Detonate();
                            collision.gameObject.GetComponent<PVEEnemyData>().TakeDamage(1);
                            break;

                        default:
                            Detonate();
                            break;
                    }
                    break;

                case BulletType.BodyGuard:
                    switch (collision.gameObject.tag)
                    {
                        case "NormalWall":
                            if (m_bouncesLeft <= 0)
                            {
                                Detonate();
                            }
                            else
                            {
                                m_bouncesLeft--;
                            }
                            break;

                        case "BouncyWall":
                            break;

                        case "Enemy":
                            break;

                        case "Player":
                            Detonate();
                            collision.gameObject.GetComponent<PVECharacterData>().TakeDamage(1);
                            break;

                        default:
                            Detonate();
                            break;
                    }
                    break;
                case BulletType.Healer:
                    switch (collision.gameObject.tag)
                    {
                        case "NormalWall":
                            if (m_bouncesLeft <= 0)
                            {
                                Detonate();
                            }
                            else
                            {
                                m_bouncesLeft--;
                            }
                            break;

                        case "BouncyWall":
                            break;

                        case "Enemy":
                            Detonate();
                            collision.gameObject.GetComponent<PVEEnemyData>().AddHealth(1);
                            break;

                        case "Player":
                            Detonate();
                            collision.gameObject.GetComponent<PVECharacterData>().AddHealth(1);
                            break;

                        default:
                            Detonate();
                            break;
                    }
                    break;
                case BulletType.Attacker:
                    switch (collision.gameObject.tag)
                    {
                        case "NormalWall":
                            if (m_bouncesLeft <= 0)
                            {
                                Detonate();
                            }
                            else
                            {
                                m_bouncesLeft--;
                            }
                            break;

                        case "BouncyWall":
                            break;

                        case "Enemy":
                            break;

                        case "Player":
                            Detonate();
                            collision.gameObject.GetComponent<PVECharacterData>().TakeDamage(1);
                            break;

                        case "Bullet":
                            break;

                        default:
                            Detonate();
                            break;
                    }
                    break;
            }
            
        }
    }
}

