using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using UnityEngine;
using Tankito;
using BehaviourAPI.Core;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.AI;

namespace Tankito.SinglePlayer
{
    public class GenericStates : MonoBehaviour
    {
        [SerializeField] AgentController agentController;
        ITankInput npcInput;
        [SerializeField] SinglePlayerBulletCannon cannon;
        Vector2 nextPosition;

        #region TargetList
        List<GameObject>  genericTargets = new List<GameObject>();
        #endregion

        #region IdleControllers
        bool patrolPointFound = false;
        #endregion

        private void Start()
        {
            npcInput = GetComponent<ITankInput>();    
        }

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

        #region States
        public Status IdleState()
        {
            if (new Vector2(transform.position.x, transform.position.y) == nextPosition)
            {
                patrolPointFound = false;
            }

            if (!patrolPointFound)
            {
                nextPosition = PatrolManager.Instance.GetPatrolPoint().position;
            }

            InputPayload inputPayload = new InputPayload();
            inputPayload.moveVector = nextPosition;
            npcInput.SetCurrentInput(inputPayload);

            return Status.Running;
        }

        public Status ChaseState()
        {
            Vector2 targetToNpc = transform.position - genericTargets[0].transform.position;
            if(targetToNpc.magnitude < agentController.npcData.runAwayDistance)
            {
                nextPosition = (Vector2)transform.position + targetToNpc.normalized * agentController.npcData.speed;
            }
            else
            {
                nextPosition = (Vector2)genericTargets[0].transform.position + targetToNpc.normalized * agentController.npcData.idealDistance;
            }
            NavMeshHit closestEdge;
            if (NavMesh.FindClosestEdge(nextPosition, out closestEdge, NavMesh.AllAreas)) //En caso de que la posicion sea un obstaculo
            {
                nextPosition = closestEdge.position;
                Debug.Log("Destino ajustado al borde más cercano.");
            }

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

