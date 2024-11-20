using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using System.Linq;

namespace Tankito.PVE
{


    public class BodyGuardBehaviours : ACommonBehaviours
    {
        ////////////////////////////////////////////////////////////////////////////////
        /////////////////////////// LLAMADAS CONCRETAS /////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////// BODY GUARD //////////////////////////////////////////////

        public Status PatrolBodyGuard()
        {
            return Patrol(m_patrolWaypoints, m_moveSpeed, m_distanceToTarget);
        }

        public Status SeekPlayer()
        {
            return Seek("Player", m_moveSpeed, m_distanceToTarget);
        }

        public bool DetectPlayer()
        {
            var detectedPlayers = Detect("Player", m_detectRange, m_tankFilter);

            if (detectedPlayers.Length > 0)
            {
                m_npcTankController.SetChaseTarget(detectedPlayers.First().rigidbody);
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}