using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class HealerBehaviour : AGenericBehaviour
    {
        GameObject player;
        [SerializeField] AreaDetection playerAreaDetection;
        [SerializeField] float healReloadTime = 10f;
        [SerializeField] float healTimer;
        bool HealReloaded { get => healTimer >= healReloadTime; }
        [SerializeField] float minDisFromPlayer = 3f;
        bool playerTooClose = false;

        [SerializeField] float idealFleeDistance;

        protected override Comparer<GameObject> GenericListOrder()
        {
            return Comparer<GameObject>.Create((obj1, obj2) =>
            {
                /*COMPARACION DE VIDA*/
                PVEEnemyData npcData1 = obj1.GetComponent<PVEEnemyData>();
                PVEEnemyData npcData2 = obj2.GetComponent<PVEEnemyData>();
                float hp1 = npcData1.Health / npcData1.Max_Health;
                float hp2 = npcData2.Health / npcData2.Max_Health;
                int hpComparation = hp1.CompareTo(hp2);
                if (hpComparation != 0) return hpComparation;
                float d1 = Vector2.Distance(obj1.transform.position, transform.position);
                float d2 = Vector2.Distance(obj2.transform.position, transform.position);
                return d1.CompareTo(d2);
            });
        }

        protected override void Start()
        {
            base.Start();
            healTimer = healReloadTime;
            playerAreaDetection.OnSubjectDetected += OnPlayerDetected;
            playerAreaDetection.OnSubjectDissapear += OnPlayerDissapear;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            playerAreaDetection.OnSubjectDetected -= OnPlayerDetected;
            playerAreaDetection.OnSubjectDissapear -= OnPlayerDissapear;
        }

        protected override void Update()
        {
            base.Update();
            healTimer += Time.deltaTime;
        }

        #region States
        public override Status ChaseState()
        {
            base.ChaseState();
            if (player != null && genericTargets.Count > 0)
            {
                Vector2 nextPos = m_currentInput.moveVector;
                Vector2 playerToHealer = transform.position - player.transform.position;
                Vector2 playerToHealerDir = playerToHealer.normalized;
                if (nextPos != (Vector2)genericTargets[0].transform.position && 
                    Vector2.Distance(nextPos, player.transform.position) < 
                    Vector2.Distance(genericTargets[0].transform.position, player.transform.position))
                {
                    
                    while (Vector2.Distance(nextPos, genericTargets[0].transform.position) < agentController.npcData.attackRange)
                    {
                        nextPos += playerToHealerDir;
                    }
                    m_currentInput.moveVector = nextPos;
                }
                if(playerToHealer.magnitude <= minDisFromPlayer)
                {
                    playerTooClose = true;
                }
            }
            m_currentInput.moveVector = CheckNewPosition(m_currentInput.moveVector);
            return Status.Running;
        }

        public Status FleeState()
        {
            if(player != null)
            {
                Vector2 playerToHealer = (transform.position - player.transform.position);
                Vector2 playerToHealerDir = playerToHealer.normalized;
                Vector2 nextPos = (Vector2)transform.position + playerToHealerDir * idealFleeDistance;
                CheckObstacles(player.transform.position, playerToHealer);
                CheckTargetInRange(playerToHealer.magnitude, agentController.npcData.attackRange);
                m_currentInput.moveVector = CheckNewPosition(nextPos);

            }
            return Status.Running;
        }
        #endregion

        #region Perceptions_Actions
        public bool CheckChaseToAimAlly()
        {
            if(genericTargets.Count > 0)
            {
                PVEEnemyData targetData = genericTargets[0].GetComponent<PVEEnemyData>();
                return noObstaclesBetween && HealReloaded && targetInRange && targetData.Health < targetData.Max_Health;
            }
            return false;
        }

        public void ActionChaseToAimAlly()
        {
            ActionChaseToAim();
            cannon.BulletType = BulletType.Healer;
        }

        public bool CheckChaseToAimPlayer()
        {
            return playerTooClose;
        }

        public void ActionChaseToAimPlayer()
        {
            noObstaclesBetween = false;
            target = player;
            cannon.BulletType = BulletType.BodyGuard;
            playerTooClose = false;
        }

        public bool CheckIdleToFlee()
        {
            return genericTargets.Count == 0 && player != null;
        }

        public bool CheckFleeToIdle()
        {
            return genericTargets.Count > 0 || player == null;
        }

        public bool CheckFleeToAimPlayer()
        {
            return noObstaclesBetween && genericTargets.Count == 0 && cannonReloaded && targetInRange;
        }

        public void ActionFleeToAimPlayer()
        {
            noObstaclesBetween = false;
            target = player;
            cannon.BulletType = BulletType.BodyGuard;
        }

        public void ActionHealAllyPOP()
        {
            healTimer = 0;
            hasShot = false;
        }
        #endregion

        #region Utilities
        protected override void OnSubjectDetected(GameObject gameObject)
        {
            if(gameObject.GetComponent<HealerBehaviour>() == null) base.OnSubjectDetected(gameObject);

        }
        
        private void OnPlayerDetected(GameObject gameObject)
        {
            player = gameObject;
        }

        private void OnPlayerDissapear(GameObject gameObject)
        {
            if (DEBUG) Debug.Log("SE TIENE QUE QUITAR EL PLAYER");
            player = null;
        }
        #endregion
    }
}

