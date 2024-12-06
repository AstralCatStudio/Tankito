using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito.Netcode;
using Tankito.SinglePlayer;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Tankito.SinglePlayer
{
    public class NecromancerBehaviour : AGenericBehaviour
    {
        [SerializeField] float resurrectTimer = 0f;
        [SerializeField] AreaDetection playerAreaDetection;
        [SerializeField] float resurrectDistance = 1f;

        GameObject player;

        protected override void Start()
        {
            base.Start();
            playerAreaDetection.OnSubjectDetected += OnPlayerDetected;
            playerAreaDetection.OnSubjectDissapear += OnPlayerDissapear;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            playerAreaDetection.OnSubjectDetected -= OnPlayerDetected;
            playerAreaDetection.OnSubjectDissapear -= OnPlayerDissapear;
        }

        #region Nodes
        public bool ConditionResurrection()
        {
            if(genericTargets.Count > 0 &&
                Vector2.Distance(genericTargets[0].transform.position, transform.position) <= resurrectDistance)
            {
                return true;
            }
            else
            {
                resurrectTimer = 0;
                return false;
            }
        }

        public Status Resurrect()
        {
            resurrectTimer += Time.deltaTime;
            m_currentInput.aimVector = (genericTargets[0].transform.position - transform.position).normalized;
            if(resurrectTimer >= agentController.npcData.reloadTime)
            {
                genericTargets[0].GetComponent<Leftovers>().ReviveTank();
            }
            return Status.Success;
        }

        public Status GoToDeathTank()
        { 
            genericTargets.OrderBy(obj => obj, GenericListOrder());
            Vector2 deathToNecroVec = (genericTargets[0].transform.position - transform.position).normalized;
            m_currentInput.moveVector = (Vector2)genericTargets[0].transform.position + deathToNecroVec * agentController.npcData.idealDistance;
            return Status.Success;
        }

        public bool ConditionHit()
        {
            return player != null && 
                Vector2.Distance(player.transform.position, transform.position) <= agentController.npcData.attackRange;
        }

        public Status Hit()
        {
            m_currentInput.action = TankAction.Parry;
            Debug.Log("PUMBA EL NECROMANCER TE GOLPEA");
            return Status.Success;
        }

        public bool ConditionFlee()
        {
            return player != null;
        }

        public Status Flee()
        {
            Vector2 playerToNecroVec = (transform.position - player.transform.position).normalized;
            m_currentInput.moveVector = (Vector2)transform.position + playerToNecroVec * agentController.agent.speed;
            return Status.Success;
        }

        public override Status IdleState()
        {
            base.IdleState();
            return Status.Success;
        }

        public Status PatrolPointFalse()
        {
            patrolPointFound = false;
            return Status.Success;
        }
        #endregion

        #region Utilis
        private void OnPlayerDetected(GameObject gameObject)
        {
            player = gameObject;
        }

        private void OnPlayerDissapear(GameObject gameObject)
        {
            player = null;
        }
        #endregion
    }
}

