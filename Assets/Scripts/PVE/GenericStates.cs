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
    public abstract class AGenericStates : MonoBehaviour, ITankInput
    {
        [SerializeField] AgentController agentController;
        [SerializeField] SinglePlayerBulletCannon cannon;
        InputPayload m_currentInput;

        #region TargetList
        List<GameObject>  genericTargets = new List<GameObject>();
        #endregion

        #region Idle_Variables
        bool patrolPointFound = false;
        #endregion

        #region Chase_AimControllers
        bool noObstaclesBetween;
        bool cannonReloaded = true;
        float timerShoot = 0;
        bool targetInRange = false;
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
            Vector2 targetToNpc = transform.position - genericTargets[0].transform.position;
            Vector2 nextPosition;
            if (Physics2D.Raycast(genericTargets[0].transform.position, targetToNpc.normalized, targetToNpc.magnitude))
            {
                nextPosition = genericTargets[0].transform.position;
                noObstaclesBetween = false;
            }
            else
            {
                noObstaclesBetween = true;
                if (targetToNpc.magnitude < agentController.npcData.runAwayDistance)
                {
                    nextPosition = (Vector2)transform.position + targetToNpc.normalized * agentController.npcData.speed;
                }
                else
                {
                    nextPosition = (Vector2)genericTargets[0].transform.position + targetToNpc.normalized * agentController.npcData.idealDistance;
                }
            }

            if(targetToNpc.magnitude <= agentController.npcData.attackRange)
            {
                targetInRange = true;
            }
            else
            {
                targetInRange = false;
            }
            
            NavMeshHit closestEdge;
            if (NavMesh.FindClosestEdge(nextPosition, out closestEdge, NavMesh.AllAreas)) //En caso de que la posicion sea un obstaculo
            {
                nextPosition = closestEdge.position;
                Debug.Log("Destino ajustado al borde más cercano.");
            }
            m_currentInput.moveVector = nextPosition;

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

        #endregion

        public Vector2 AimState()
        {
            return Vector2.zero;
        }

        public bool CheckAimToShoot(Vector2 aimVec)
        {
            //if(aimVec == transform.GetChild(1))
            return false;
        }

        public TankAction ShootState()
        {
            return TankAction.Fire;
        }

        private void OnSubjectDetected(GameObject gameObject)
        {
            genericTargets.Add(gameObject);
        }

        private void OnSubjectDissapear(GameObject gameObject)
        {
            genericTargets.Remove(gameObject); 
        }
    }
}

