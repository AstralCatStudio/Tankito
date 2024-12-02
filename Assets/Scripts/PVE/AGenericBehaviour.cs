using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using UnityEngine;
using Tankito;
using BehaviourAPI.Core;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.Splines;

namespace Tankito.SinglePlayer
{
    public abstract class AGenericBehaviour : MonoBehaviour, ITankInput
    {
        [SerializeField] protected AgentController agentController;
        [SerializeField] SinglePlayerBulletCannon cannon;
        protected InputPayload m_currentInput;
        [SerializeField] AreaDetection genericAreaDetection;
        [SerializeField] LayerMask wallLayer;
        [SerializeField] GameObject debugPosition;

        [SerializeField] protected bool DEBUG = true;

        #region TargetList
        [SerializeField] protected List<GameObject>  genericTargets = new List<GameObject>();
        #endregion

        #region Idle_Variables
        [SerializeField] bool patrolPointFound = false;
        #endregion

        #region Chase_Aim_Controllers
        [SerializeField] bool noObstaclesBetween;
        [SerializeField] bool cannonReloaded = true;
        [SerializeField] float timerShoot = 0;
        [SerializeField] bool targetInRange = false;
        #endregion

        #region Aim_Shoot_Controllers
        [SerializeField] bool turretInPosition = false;
        #endregion

        #region Shoot_Controllers
        [SerializeField] bool hasShot = false;
        #endregion

        protected virtual void Start()
        {
            genericAreaDetection.OnSubjectDetected += OnSubjectDetected;
            genericAreaDetection.OnSubjectDissapear += OnSubjectDissapear;
        }

        protected virtual void OnDisable()
        {
            genericAreaDetection.OnSubjectDetected -= OnSubjectDetected;
            genericAreaDetection.OnSubjectDissapear -= OnSubjectDissapear;
        }

        private void Update()
        {
            Debug.Log("HOLA HOLA HOLA");
            timerShoot += Time.deltaTime;
            if(timerShoot >= agentController.npcData.reloadTime) 
            {
                cannonReloaded = true;
            }
        }

        #region TankInputMethods
        public InputPayload GetInput() 
        { 
            InputPayload newInput = m_currentInput;
            m_currentInput.action = TankAction.None;
            debugPosition.transform.position = newInput.moveVector;
            return newInput;
        }

        public InputPayload GetCurrentInput() {  return m_currentInput; }

        public void SetCurrentInput(InputPayload newCurrentInput) {  m_currentInput = newCurrentInput; }

        public void SetInput(InputPayload input) {  m_currentInput = input; }

        public void StartInputReplay(int timestamp) { }

        public int StopInputReplay() { return 0; }
        #endregion

        #region States
        public Status IdleState()
        {
            if (DEBUG) Debug.Log("IDLESTATE");
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
            if (DEBUG) Debug.Log("CHASESTATE");
            if(genericTargets.Count > 0) 
            {
                genericTargets.OrderBy(obj => Vector2.Distance(obj.transform.position, transform.position));
                Vector3 targetPos = genericTargets[0].transform.position;
                Vector2 targetToNpc = transform.position - targetPos;
                Vector2 nextPosition;
                if (CheckObstacles(targetPos, targetToNpc))
                {
                    if (DEBUG)
                    Debug.Log("Se han encontrado obstaculos. Se va a " + targetPos);
                    nextPosition = targetPos;
                }
                else
                {
                    if (targetToNpc.magnitude < agentController.npcData.runAwayDistance)
                    {
                        nextPosition = (Vector2)transform.position + targetToNpc.normalized * agentController.npcData.speed * agentController.agent.speed;
                    }
                    else
                    {
                        nextPosition = (Vector2)targetPos + targetToNpc.normalized * agentController.npcData.idealDistance;
                    }
                }

                CheckTargetInRange(targetToNpc.magnitude, agentController.npcData.attackRange);

                /*if (DEBUG)*/ 
                NavMeshHit closestEdge;
                if (NavMesh.FindClosestEdge(nextPosition, out closestEdge, wallLayer)) //En caso de que la posicion sea un obstaculo
                {
                    nextPosition = closestEdge.position;
                    if(DEBUG) Debug.Log("Destino ajustado al borde más cercano.");
                }
                m_currentInput.moveVector = nextPosition;
            }
            
            return Status.Running;
        }

        public Status AimState()
        {
            if (DEBUG) Debug.Log("AIMSTATE");
            if(genericTargets.Count > 0)
            {
                Vector2 aimVec = genericTargets[0].transform.position - transform.position;
                Vector2 aimDir = aimVec.normalized;
                Vector2 turretVec = (cannon.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(aimDir, turretVec);
                if (DEBUG) Debug.Log(angle);
                if (angle < agentController.npcData.angleErrorAccepted)
                {
                    turretInPosition = true;
                }
                m_currentInput.aimVector = aimVec;
                CheckTargetInRange(aimVec.magnitude, agentController.npcData.attackRange);
            }   
            return Status.Running;
        }

        public Status ShootState()
        {
            if (DEBUG) Debug.Log("SHOOTSTATE");
            m_currentInput.action = TankAction.Fire;
            hasShot = true;
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
                timerShoot = 0;
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
            if (DEBUG) Debug.Log("Se añade un objeto generico a la lista");
            genericTargets.Add(gameObject);
        }

        private void OnSubjectDissapear(GameObject gameObject)
        {
            if (DEBUG) Debug.Log("Se elimina un elemento generico de la lista");
            genericTargets.Remove(gameObject); 
        }

        protected bool CheckObstacles(Vector3 targetPosition, Vector2 targetToNpc)
        {
            if (Physics2D.Raycast(targetPosition, targetToNpc.normalized, targetToNpc.magnitude, wallLayer))
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
    }
    #endregion
}

