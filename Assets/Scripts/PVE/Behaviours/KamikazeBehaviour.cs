using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito.SinglePlayer;
using UnityEngine;
using static AreaDetection;
using Tankito.Netcode;

namespace Tankito.SinglePlayer
{
    public class KamikazeBehaviour : AGenericBehaviour
    {
        [SerializeField] private float explosionRange = 2.0f;
        [SerializeField] private float autoDestructDistance;
        private float autoDestructTime;
        private bool countDownStarted = false;

        private bool hasParried = false;
        private bool hasCollided = false;

        [SerializeField] private float parryDistance; 

        List<GameObject> bulletsInRange = new List<GameObject>();
        [SerializeField] AreaDetection bulletAreaDetection;
        public GameObject explosionEffect;

        protected override void Start()
        {
            base.Start();
            bulletAreaDetection.OnSubjectDetected += OnBulletDetected;
            bulletAreaDetection.OnSubjectDissapear += OnBulletDissapear;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            bulletAreaDetection.OnSubjectDetected -= OnBulletDetected;
            bulletAreaDetection.OnSubjectDissapear -= OnBulletDissapear;
        }
        #region States
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public override Status ChaseState()
        {
            if(genericTargets.Count > 0)
            {
                m_currentInput.moveVector = genericTargets[0].transform.position;
            }            
            return Status.Running;
        }

        public Status ParryState()
        {
            Debug.Log("PARRY KAMIKAZE");
            m_currentInput.action = TankAction.Parry;
            hasParried = true;
            return Status.Success;
        }

        public Status BlowState()
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Debug.Log("KAMIKAZE EXPLOTA");
            //Destroy(gameObject);
            GetComponent<PVEEnemyData>().Die();
            return Status.Success;
        }
        #endregion

        #region Perceptions

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool CheckChaseToParry()
        {
            if(bulletsInRange.Count > 0)
            {
                for(int i = 0; i < bulletsInRange.Count; i++)
                {
                    if (Vector2.Distance(bulletsInRange[i].transform.position, transform.position) <= parryDistance)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckParryToChase()
        {
            return hasParried;
        }

        public bool CheckChaseToBlow()
        {
            return hasCollided || AutoDestructionCountDown();
        }

        #endregion

        #region Utilities
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bullet"))
            {
                hasCollided = true;
                Debug.Log("COLLISION DE KAMIKAZE");
            }
        }

        private void OnBulletDetected(GameObject gameObject)
        {
            bulletsInRange.Add(gameObject);
        }

        private void OnBulletDissapear(GameObject gameObject)
        {
            bulletsInRange.Remove(gameObject);
        }

        private bool AutoDestructionCountDown()
        {
            if (!countDownStarted)
            {
                if (genericTargets.Count > 0)
                {
                    if (Vector2.Distance(genericTargets[0].transform.position, transform.position) <= autoDestructDistance)
                    {
                        autoDestructTime = autoDestructDistance / agentController.agent.speed;
                        countDownStarted = true;
                    }
                }
            }
            if(countDownStarted) 
            {
                autoDestructTime -= Time.deltaTime;
                if(autoDestructTime <= 0)
                {
                    return true;
                }
            }
            return false;           
        }
        #endregion
    }
}

