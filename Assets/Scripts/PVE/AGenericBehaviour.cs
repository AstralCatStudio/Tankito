using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using UnityEngine;
using Tankito;
using BehaviourAPI.Core;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.AI;
using System.Linq;

namespace Tankito.SinglePlayer
{
    public abstract class AGenericBehaviour : MonoBehaviour, ITankInput
    {
        [SerializeField] protected AgentController agentController;
        [SerializeField] SinglePlayerBulletCannon cannon;
        protected InputPayload m_currentInput;

        #region TargetList
        protected List<GameObject>  genericTargets = new List<GameObject>();
        #endregion

        #region Idle_Variables
        bool patrolPointFound = false;
        #endregion

        #region Chase_Aim_Controllers
        bool noObstaclesBetween;
        bool cannonReloaded = true;
        float timerShoot = 0;
        bool targetInRange = false;
        #endregion

        #region Aim_Shoot_Controllers
        bool turretInPosition = false;
        #endregion

        #region Shoot_Controllers
        bool hasShot = false;
        #endregion

        private void OnEnable()
        {
            transform.GetChild(2).GetComponent<AreaDetection>().OnSubjectDetected += OnSubjectDetected;
            transform.GetChild(2).GetComponent<AreaDetection>().OnSubjectDissapear += OnSubjectDissapear;
        }

        private void OnDisable()
        {
            transform.GetChild(2).GetComponent<AreaDetection>().OnSubjectDetected -= OnSubjectDetected;
            transform.GetChild(2).GetComponent<AreaDetection>().OnSubjectDissapear -= OnSubjectDissapear;
        }

        void Start()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }

        private void Update()
        {
            timerShoot += Time.deltaTime;
            if(timerShoot >= agentController.npcData.reloadTime) 
            {
                cannonReloaded = true;
            }
        }

        #region TankInputMethods
        public InputPayload GetInput() { return GetCurrentInput(); }

        public InputPayload GetCurrentInput() {  return m_currentInput; }

        public void SetCurrentInput(InputPayload newCurrentInput) {  m_currentInput = newCurrentInput; }

        public void SetInput(InputPayload input) {  m_currentInput = input; }

        public void StartInputReplay(int timestamp) { }

        public int StopInputReplay() { return 0; }
        #endregion

        #region States
        public Status IdleState()
        {
            if (new Vector2(transform.position.x, transform.position.y) == m_currentInput.moveVector)
            {
                patrolPointFound = false;
            }

            if (!patrolPointFound)
            {
                m_currentInput.moveVector = PatrolManager.Instance.GetPatrolPoint().position;
            }

            return Status.Running;
        }

        public Status ChaseState()
        {
            genericTargets.OrderBy(obj => Vector2.Distance(obj.transform.position, transform.position));
            Vector3 targetPos = genericTargets[0].transform.position;
            Vector2 targetToNpc = transform.position - targetPos;
            Vector2 nextPosition;
            if (CheckObstacles(targetPos, targetToNpc))
            {
                nextPosition = targetPos;
            }
            else
            {
                if (targetToNpc.magnitude < agentController.npcData.runAwayDistance)
                {
                    nextPosition = (Vector2)transform.position + targetToNpc.normalized * agentController.npcData.speed;
                }
                else
                {
                    nextPosition = (Vector2)targetPos + targetToNpc.normalized * agentController.npcData.idealDistance;
                }
            }

            CheckTargetInRange(targetToNpc.magnitude, agentController.npcData.attackRange);
            
            NavMeshHit closestEdge;
            if (NavMesh.FindClosestEdge(nextPosition, out closestEdge, NavMesh.AllAreas)) //En caso de que la posicion sea un obstaculo
            {
                nextPosition = closestEdge.position;
                Debug.Log("Destino ajustado al borde más cercano.");
            }
            m_currentInput.moveVector = nextPosition;

            return Status.Running;
        }

        public Status AimState()
        {
            Vector2 aimVec = (genericTargets[0].transform.position - transform.position).normalized;
            Vector2 turretVec = (cannon.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(aimVec, turretVec);
            if(angle < agentController.npcData.angleErrorAccepted)
            {
                turretInPosition = true;
            }
            m_currentInput.aimVector = aimVec;
            return Status.Running;
        }

        public Status ShootState()
        {
            m_currentInput.action = TankAction.Fire;
            StartCoroutine(SafeShootCourutine());
            return Status.Running;
        }

        #endregion

        #region Perceptions
        public bool CheckIdleToChase()
        {
            if (genericTargets.Count > 0)
            {
                patrolPointFound = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckChaseToIdle()
        {
            if (genericTargets.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckChaseToAim()
        {
            if(noObstaclesBetween && cannonReloaded && targetInRange)
            {
                noObstaclesBetween = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckAimPOP()
        {
            if (!targetInRange)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckAimToShoot()
        {
            if (turretInPosition)
            {
                turretInPosition = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckShootPOP()
        {
            if(hasShot)
            {
                cannonReloaded = false;
                hasShot = false;
                m_currentInput.action = TankAction.None;
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion


        #region Utilis
        private void OnSubjectDetected(GameObject gameObject)
        {
            genericTargets.Add(gameObject);
        }

        private void OnSubjectDissapear(GameObject gameObject)
        {
            genericTargets.Remove(gameObject); 
        }

        protected bool CheckObstacles(Vector3 targetPosition, Vector2 targetToNpc)
        {
            if (Physics2D.Raycast(targetPosition, targetToNpc.normalized, targetToNpc.magnitude))
            {
                noObstaclesBetween = false;
            }
            else
            {
                noObstaclesBetween = true;
            }
            return !noObstaclesBetween;
        }

        protected bool CheckTargetInRange(float magnitude, float range)
        {
            if(magnitude <= range)
            {
                targetInRange = true;
            }
            else
            {
                targetInRange = false;
            }
            return targetInRange;
        }

        IEnumerator SafeShootCourutine()    //Corutina que asegura que se consume el input
        {
            yield return Time.fixedDeltaTime;
            hasShot = true;
        }
    }
    #endregion
}

