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
        [SerializeField] protected SinglePlayerBulletCannon cannon;
        protected InputPayload m_currentInput;
        [SerializeField] AreaDetection genericAreaDetection;
        [SerializeField] LayerMask wallLayer;
        [SerializeField] GameObject debugPosition;
        protected GameObject target;

        [SerializeField] protected bool DEBUG = true;

        protected virtual Comparer<GameObject> GenericListOrder()
        {
            return Comparer<GameObject>.Create((obj1, obj2) =>
            {
                float d1 = Vector2.Distance(obj1.transform.position, transform.position);
                float d2 = Vector2.Distance(obj2.transform.position, transform.position);
                return d1.CompareTo(d2);
            });
        }

        #region TargetList
        [SerializeField]
        protected List<GameObject>  genericTargets = new List<GameObject>();
        #endregion

        #region Idle_Variables
        [SerializeField] protected bool patrolPointFound = false;
        #endregion

        #region Chase_Aim_Controllers
        [SerializeField] protected bool noObstaclesBetween;
        [SerializeField] protected bool cannonReloaded = true;
        [SerializeField] protected float timerShoot = 0;
        [SerializeField] protected bool targetInRange = false;
        #endregion

        #region Aim_Shoot_Controllers
        [SerializeField] bool turretInPosition = false;
        #endregion

        #region Shoot_Controllers
        [SerializeField] protected bool hasShot = false;
        #endregion

        protected virtual void Start()
        {
            genericAreaDetection.OnSubjectDetected += OnSubjectDetected;
            genericAreaDetection.OnSubjectDissapear += OnSubjectDissapear;
            var genericOrder = GenericListOrder();
            m_currentInput.moveVector = PatrolManager.Instance.GetPatrolPoint().position;
        }

        protected virtual void OnDisable()
        {
            genericAreaDetection.OnSubjectDetected -= OnSubjectDetected;
            genericAreaDetection.OnSubjectDissapear -= OnSubjectDissapear;
        }

        protected virtual void Update()
        {
            //Debug.Log("HOLA HOLA HOLA");
            timerShoot += Time.deltaTime;
            if(timerShoot >= agentController.npcData.reloadTime) 
            {
                cannonReloaded = true;
            }
        }

        #region TankInputMethods
        public virtual InputPayload GetInput() 
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
        public virtual Status IdleState()
        {
            if (DEBUG) Debug.Log("IDLESTATE");
            if (new Vector2(transform.position.x, transform.position.y) == m_currentInput.moveVector)
            {
                patrolPointFound = false;        
            }
            if(!patrolPointFound)
            {
                m_currentInput.moveVector = PatrolManager.Instance.GetPatrolPoint().position;
                patrolPointFound = true;
            }
            return Status.Running;
        }

        public virtual Status ChaseState()
        {
            if (DEBUG) Debug.Log("CHASESTATE");
            if(genericTargets.Count > 0) 
            {
                var orderedList = new List<GameObject>(genericTargets.OrderBy(obj => obj, GenericListOrder()));
                Vector2 targetPos = orderedList[0].transform.position;
                Vector2 targetToNpc = (Vector2)transform.position - targetPos;
                Vector2 nextPosition;
                if (CheckObstacles(targetPos, targetToNpc))
                {
                    if (DEBUG)
                    Debug.Log("Se han encontrado obstaculos. Se va a " + targetPos);
                    nextPosition = targetPos;
                }
                else
                {
                    nextPosition = targetPos + targetToNpc.normalized * agentController.npcData.idealDistance;
                }

                CheckTargetInRange(targetToNpc.magnitude, agentController.npcData.attackRange);
              
                m_currentInput.moveVector = nextPosition;
            }
            
            return Status.Running;
        }

        public Status AimState()
        {
            if (DEBUG) Debug.Log("AIMSTATE");
            if(target != null)
            {
                Vector2 aimVec = target.transform.position - transform.position;
                Vector2 aimDir = aimVec.normalized;
                Vector2 turretVec = (cannon.transform.position - transform.position).normalized;
                float angle = Vector2.Angle(aimDir, turretVec);
                if (DEBUG) Debug.Log(angle);
                if (angle < agentController.npcData.angleErrorAccepted)
                {
                    turretInPosition = true;
                }
                m_currentInput.aimVector = aimDir;
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

        #region Perceptions_Actions
        public bool CheckIdleToChase()
        {
            return genericTargets.Count > 0;
        }

        public void ActionIdleToChase()
        {
            patrolPointFound = false;
        }

        public bool CheckChaseToIdle()
        {
            return genericTargets.Count == 0;
        }

        public bool CheckChaseToAim()
        {
            return noObstaclesBetween && cannonReloaded && targetInRange;
        }

        public void ActionChaseToAim()
        {
            noObstaclesBetween = false;
            target = genericTargets[0];
        }

        public bool CheckAimPOP()
        {
            return !targetInRange;
        }

        public bool CheckAimToShoot()
        {
            return turretInPosition;
        }

        public void ActionAimToShoot()
        {
            turretInPosition = false;
        }

        public bool CheckShootPOP()
        {
            return hasShot;
        }

        public virtual void ActionShootPOP()
        {
            cannonReloaded = false;
            hasShot = false;
            timerShoot = 0;
        }

        #endregion


        #region Utilis

        public Vector2 CheckNewPosition(Vector2 nextPos)
        {
            /*if (DEBUG)*/
            NavMeshHit closestEdge;
            if (NavMesh.FindClosestEdge(nextPos, out closestEdge, wallLayer)) //En caso de que la posicion sea un obstaculo
            {
                nextPos = closestEdge.position;
                if (DEBUG) Debug.Log("Destino ajustado al borde más cercano.");
            }
            return nextPos;
        }

        protected virtual void OnSubjectDetected(GameObject gameObject)
        {
            if (DEBUG) Debug.Log("Se añade un objeto generico a la lista");
            genericTargets.Add(gameObject);
        }

        protected virtual void OnSubjectDissapear(GameObject gameObject)
        {
            if (DEBUG) Debug.Log("Se elimina un elemento generico de la lista");
            if (genericTargets.Contains(gameObject))
            {
                genericTargets.Remove(gameObject);
            }  
        }

        protected bool CheckObstacles(Vector2 targetPosition, Vector2 targetToNpc)
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

        protected virtual bool CheckTargetInRange(float magnitude, float range)
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

