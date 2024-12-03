using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class HealerBehaviour : AGenericBehaviour
    {
        [SerializeField] GameObject player;
        [SerializeField] AreaDetection playerAreaDetection;

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

        #region States
        public Status RunAwayState()
        {
            return Status.Running;
        }

        public override Status ChaseState()
        {
            base.ChaseState();
            return Status.Running;
        }

        #endregion

        #region Perceptions
        public bool CheckIdleToRunAway()
        {
            return false;
        }

        public bool CheckRunAwayToIdle()
        {
            return false;
        }
        #endregion

        #region Utilities
        private void OnPlayerDetected(GameObject gameObject)
        {
            player = gameObject;
        }

        private void OnPlayerDissapear(GameObject player)
        {
            player = null;
        }
        #endregion
    }
}

