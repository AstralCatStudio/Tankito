using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class AttackerBehavior : AGenericBehaviour
    {
        GameObject player;
        [SerializeField] AreaDetection playerAreaDetection;
        [SerializeField] const int LOADER_SIZE = 5;
        int ammunition;
        [SerializeField] bool Bullets { get => ammunition > 0; }
        [SerializeField] float loaderReloadDuration = 5f;
        float loaderReloadTimer = 0f;
         
        protected override void Start()
        {
            base.Start();
            ammunition = LOADER_SIZE;
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
            if (!Bullets)
            {
                loaderReloadTimer += Time.deltaTime;
                if(loaderReloadTimer >= loaderReloadDuration)
                {
                    loaderReloadDuration = 0;
                    ammunition = LOADER_SIZE;
                }
            }
        }

        #region States

        public override Status ChaseState()
        {
            base.ChaseState();
            m_currentInput.moveVector = CheckNewPosition(m_currentInput.moveVector);
            return Status.Running;
        }

        public Status CoverState()
        {
            if (genericTargets.Count > 0 && player != null)
            {
                genericTargets.OrderBy(obj => obj, GenericListOrder());
                Vector2 bGPos = genericTargets[0].transform.position;
                Vector2 playerPos = player.transform.position;
                Vector2 playerToBG = (bGPos - playerPos).normalized;
                m_currentInput.moveVector = bGPos + playerToBG * agentController.npcData.idealDistance;
            }
            return Status.Running;
        }
        #endregion

        #region Perceptions
        public bool CheckChaseToCover()
        {
            return player != null && !Bullets;
        }

        public bool CheckCoverToChase()
        {
            return player == null;
        }

        public bool CheckChaseAndCoverToUS()
        {
            return player != null && Bullets;
        }

        public bool CheckUSToChase()
        {
            return player == null && genericTargets.Count > 0;
        }

        public bool CheckUSToCover()
        {
            return player != null && genericTargets.Count > 0 && !Bullets;
        }

        public bool CheckIdleToUS()
        {
            return player != null;
        }

        public bool CheckUSToIdle()
        {
            return player == null && genericTargets.Count == 0;
        }

        public override void ActionShootPOP()
        {
            base.ActionShootPOP();
            ammunition--;
        }

        #endregion 

        #region Utilis
        protected override void OnSubjectDetected(GameObject gameObject)
        {
            if(gameObject.GetComponent<BodyGuardBehaviour>() != null)
            {
                base.OnSubjectDetected(gameObject);
            }  
        }

        private void OnPlayerDetected(GameObject gameObject)
        {
            player = gameObject;
        }

        private void OnPlayerDissapear(GameObject gameObject)
        {
            player = null;
        }

        protected override bool CheckTargetInRange(float magnitude, float range) { return false; }
        #endregion
    }
}

