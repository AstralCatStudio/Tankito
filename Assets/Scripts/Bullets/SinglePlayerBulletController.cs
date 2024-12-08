using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class SinglePlayerBulletController : ABulletController
    {
        public GameObject miniExplosionPrefab;

        [SerializeField] float bulletVelocity;
        [SerializeField] float lifeTimeTreshold;
        [SerializeField] BulletType bulletType;
        BulletType originalType;
        bool detonated = false;
        GameObject parriedby;
        public GameObject creator;
        private void OnEnable()
        {
            m_bouncesLeft = 0;
        }

        public void InitializeBullet(Vector2 position, Vector2 direction, bool triggerOnSpawnEvents = true)
        {
            parriedby = null;
            detonated = false;
            originalType = bulletType;
            transform.position = position;
            m_rb.velocity = direction.normalized * bulletVelocity;
            transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), m_rb.velocity.normalized);

            if (triggerOnSpawnEvents) OnSpawn?.Invoke(this);

            MusicManager.Instance.PlaySoundPitch("snd_bala_impacta",0.3f);
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
                Debug.Log("detonado por lifetime");
                Detonate(true);
            }
        }

        public override void Detonate(bool lifeTimeOver = false)
        {
            
            
            if (!detonated)
            {
                Instantiate(miniExplosionPrefab, transform.position, Quaternion.identity);

                MusicManager.Instance.PlaySoundPitch("snd_disparo");

                OnDetonate.Invoke(this);
                detonated = true;
                SinglePlayerBulletPool.Instance.Release(this.gameObject, (int)bulletType);
            }
            
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == 14 && collision.gameObject != parriedby)
            {
                parriedby = collision.gameObject;
                m_rb.velocity = -m_rb.velocity * 1.5f;
                bulletType = BulletType.Parry;
                creator = collision.gameObject;
            }
            else
            if (collision.gameObject.GetComponent<SinglePlayerBulletController>()!=null &&bulletType != BulletType.Parry &&bulletType != BulletType.Attacker && collision.gameObject.CompareTag("EnemyBullet") && collision.gameObject.GetComponent<SinglePlayerBulletController>()?.bulletType == BulletType.Attacker)
            {
                Debug.Log("chocando con bala de atacker");
                Detonate();
            }
            else if (collision.gameObject.GetComponent<SinglePlayerBulletController>() != null && bulletType != BulletType.Parry && (collision.gameObject.CompareTag("EnemyBullet") || collision.gameObject.CompareTag("Bullet")) && collision.gameObject.GetComponent<SinglePlayerBulletController>()?.bulletType == BulletType.Parry)
            {
                Debug.Log("chocando con bala parreada");
                Detonate();
            }
        }
        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.gameObject == creator)
            {
                return;
            }
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
                                Detonate();
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
                    Debug.Log("bala chocando");
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
                            Debug.Log("bala curando");
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
                                Detonate();
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
                case BulletType.Parry:
                    
                    switch (collision.gameObject.tag)
                    {
                        case "NormalWall":
                            if (m_bouncesLeft <= 0)
                            {
                                bulletType = originalType;
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
                            bulletType = originalType;
                            Detonate();
                            collision.gameObject.GetComponent<PVEEnemyData>().TakeDamage(1);
                            break;

                        case "Player":
                            bulletType = originalType;
                            Detonate();
                            collision.gameObject.GetComponent<PVECharacterData>().TakeDamage(1);

                            break;

                        case "Bullet":
                            break;
                        case "EnemyBullet":
                            break;

                        default:
                            bulletType = originalType;
                            Detonate();
                            break;
                    }
                    break;
            }
            
        }
    }
}

