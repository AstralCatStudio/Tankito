using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Tankito.Netcode;
using UnityEngine.UIElements;
using Tankito.Netcode.Simulation;

namespace Tankito {

    public enum BulletMoveType { Normal, Boomerang }
    public class BulletController : MonoBehaviour
    {
        BulletSimulationObject m_simObj;
        public int m_bouncesLeft = 0;
        public GameObject explosionVisual;
        public float LifeTime { get => m_lifetime; set => m_lifetime = value; }
        private float m_lifetime = 0; // Life Time counter
        protected Vector2 lastCollisionNormal = Vector2.zero;
        private Rigidbody2D m_rb;
        public float selfCollisionTreshold = 0.1f;
        public ulong LastShooterObjId => m_lastShooterObjId;
        private ulong m_lastShooterObjId; // Used to identify the object that "shot" or redirected the bullet last. To avoid desyncs when multiple parries happen.
        private const float PARRY_SPEED_BOOST = 1.5f;
        public Action<BulletController> OnSpawn = (ABullet) => { }, OnFly = (ABullet) => { },
                                        OnHit = (ABullet) => { }, OnBounce = (ABullet) => { },
                                        OnDetonate = (ABullet) => { };
        [SerializeField] private bool PREDICT_DESTRUCTION = true;
        public Sprite bulletSprite;
        int maxBulletSpritePriority =0;

        #region Boomerang
        BulletMoveType bulletType = BulletMoveType.Normal;
        public BulletMoveType BulletType { get => bulletType; set => bulletType = value; }
        [SerializeField] float spinbackTime = 1.5f;
        [SerializeField] float boomeramgEffectDuration = 1f;
        int boomerangTicks;
        [SerializeField] float amplitude = 1.5f;
        [SerializeField] float rotAngle = 270;
        #endregion

        private void Awake()
        {
            m_simObj = GetComponent<BulletSimulationObject>();
            m_rb = GetComponent<Rigidbody2D>();
        }

        public void SetLastShooterObjId(ulong simObjId)
        {
            m_lastShooterObjId = simObjId;
            //Debug.Log($"[{SimClock.TickCounter}] LastShooterSimObjId[{LastShooterObjId}]");
        }

        public void InitializeProperties(bool triggerOnSpawnEvents = true)
        {
            GetComponent<BulletSimulationObject>().OnComputeKinematics += MoveBullet;

            transform.position = BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.startingPosition;
            m_bouncesLeft = BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.bouncesTotal;
            m_rb.velocity = BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.velocity * BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.direction.normalized;

            // BERNAT: estoy super empanado ahora, probablmente ohay una forma mejor de 
            // hacerlo sin tener que cambiar nada pero no soy capaz de pensar en ella ahora mismo la verdad
            SetLastShooterObjId(BulletCannonRegistry.Instance[m_simObj.OwnerId].GetComponentInParent<TankSimulationObject>().SimObjId);

            transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), m_rb.velocity.normalized);
            foreach (var modifier in Tankito.BulletCannonRegistry.Instance[m_simObj.OwnerId].Modifiers)
            {
                modifier.BindBulletEvents(this);
                if (modifier.bulletSpritePriority > maxBulletSpritePriority)
                {
                    maxBulletSpritePriority = modifier.bulletSpritePriority;
                    if (modifier.bulletSprite != null)
                    {
                        bulletSprite = modifier.bulletSprite;
                    }
                }
            }
            maxBulletSpritePriority = 0;
            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = bulletSprite;
            if (triggerOnSpawnEvents) OnSpawn?.Invoke(this);
            Debug.Log(m_rb.velocity.magnitude);
        }

        private void OnDisable()
        {
            GetComponent<BulletSimulationObject>().OnComputeKinematics -= MoveBullet;
            
            ResetBulletData();
        }

        private void Update()
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), m_rb.velocity.normalized);
        }

        void MoveBullet(float deltaTime)
        {
            if (m_lifetime >= selfCollisionTreshold)
            {
                gameObject.layer = 0;
            }
            m_rb.velocity += (BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.acceleration != 0f) ? BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.acceleration * m_rb.velocity.normalized : Vector2.zero;
            if(bulletType == BulletMoveType.Boomerang)
            {
                if(m_lifetime >= spinbackTime && m_lifetime <= spinbackTime + boomeramgEffectDuration)
                {
                    boomerangTicks = (int)(boomeramgEffectDuration / SimClock.SimDeltaTime);
                    float anglePerTick = rotAngle / boomerangTicks;
                    Debug.Log(anglePerTick + " " + boomerangTicks);

                    float speed = m_rb.velocity.magnitude;
                    float angleInRadians = anglePerTick * Mathf.Deg2Rad;
                    float rotatedX = m_rb.velocity.x * Mathf.Cos(angleInRadians) - m_rb.velocity.y * Mathf.Sin(angleInRadians);
                    float rotatedY = m_rb.velocity.x * Mathf.Sin(angleInRadians) + m_rb.velocity.y * Mathf.Cos(angleInRadians);
                    m_rb.velocity = new Vector2(rotatedX, rotatedY).normalized * speed;
                }              
            }
            m_lifetime += deltaTime;
            OnFly.Invoke(this);
            if (m_lifetime >= BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.lifetimeTotal)
            {
                //Debug.Log($"lifetime: {m_lifetime}/{BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.lifetimeTotal}");
                Detonate(true);
            }
        }

        protected void ResetBulletData()
        {
            gameObject.layer = 8;
            m_bouncesLeft = int.MaxValue; // int.MaxValue PARA EVITAR Detonate prematuro en colisiones previas a la inicializacion y simulacion de la bala.
            m_lifetime = 0;
            OnSpawn = (ABullet) => {};
            OnFly = (ABullet) => {};
            OnHit = (ABullet) => {};
            OnBounce = (ABullet) => {};
            OnDetonate = (ABullet) => {};
            bulletType = BulletMoveType.Normal;
        }

        public void Detonate(bool lifeTimeOver = false)
        {
            
            OnDetonate.Invoke(this);

            if (SimClock.Instance.Active && NetworkManager.Singleton.IsClient)
                MusicManager.Instance.PlayBulletDestroy();


            if (NetworkManager.Singleton.IsServer)
            {
                BulletSimulationObject bulletSimObj = GetComponent<BulletSimulationObject>();
                ServerSimulationManager.Instance.QueueForDespawn(bulletSimObj.SimObjId);
                Instantiate(explosionVisual, transform.position, transform.rotation);
            }
            else if(PREDICT_DESTRUCTION || lifeTimeOver)
            {
                // Debug.Log("LifeTimeOver?=>" + lifeTimeOver);
                BulletSimulationObject bulletSimObj = GetComponent<BulletSimulationObject>();
                ClientSimulationManager.Instance.QueueForDespawn(bulletSimObj.SimObjId);
                Instantiate(explosionVisual, transform.position, transform.rotation);
            }
            else
            {
            }
        }   

        private void OnCollisionEnter2D(Collision2D collision)
        {
            lastCollisionNormal = collision.GetContact(0).normal;
            //Debug.Log($"[{SimClock.TickCounter}]Collided with: {collision.collider.name}(tag:{collision.collider.tag}) at {collision.contacts.First().point}");
            switch (collision.collider.tag)
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
                    if (BulletCannonRegistry.Instance[m_simObj.OwnerId].GetComponentInParent<TankSimulationObject>().SimObjId == m_lastShooterObjId && m_lifetime < 0.03f)
                    {
                        //Debug.Log("Ignoing firing self collision");
                    }
                    else
                    {
                        collision.gameObject.GetComponent<TankData>().TakeDamage(1);
                        Detonate();
                    }
                    break;
                
                case "Parry":
                    m_lifetime = 0;
                    m_bouncesLeft = BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.bouncesTotal;

                    Vector2 parriedDirection;

                    // If we are parrying our own bullet, then keep it flying in the same direction and just apply the speed boost
                    if (LastShooterObjId == collision.gameObject.GetComponent<ASimulationObject>().SimObjId)
                    {
                        parriedDirection = m_rb.velocity.normalized;
                    }
                    else
                    {
                        Vector2 newTargetPosition;
                        if (NetworkManager.Singleton.IsServer)
                        {
                            newTargetPosition = ServerSimulationManager.Instance.GetSimObj(LastShooterObjId).transform.position;
                        }
                        else
                        {
                            newTargetPosition = ClientSimulationManager.Instance.GetSimObj(LastShooterObjId).transform.position;
                        }

                        parriedDirection = (newTargetPosition - m_rb.position).normalized;
                    }

                    // We want the bullet to fly faster than before
                    m_rb.velocity = BulletCannonRegistry.Instance[m_simObj.OwnerId].Properties.velocity * PARRY_SPEED_BOOST * parriedDirection;
                    
                    // Update the last shooter variable before returning the bullet
                    SetLastShooterObjId(collision.gameObject.GetComponent<ASimulationObject>().SimObjId);
                    break;

                default:
                    Detonate();
                    break;
            }
        }

    }
}