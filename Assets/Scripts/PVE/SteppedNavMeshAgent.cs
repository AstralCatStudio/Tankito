using System;
using UnityEngine;
using UnityEngine.AI;

namespace Tankito.PVE
{
    public class SteppedNavMeshAgent : MonoBehaviour
    {
        private NavMeshAgent m_navMeshAgent;

        private bool m_stepQueued;


        internal void SetDestination(Vector2 position)
        {
            QueueStep();
            m_navMeshAgent.SetDestination(position);
        }

        public Vector2 StepAlongPath(float deltaTime)
        {
            QueueStep();
            
            float stepDistance = m_navMeshAgent.velocity.magnitude * deltaTime;
            NavMeshHit navMeshHit;

            m_navMeshAgent.SamplePathPosition(m_navMeshAgent.areaMask, stepDistance, out navMeshHit);

            return navMeshHit.position;

        }

        public void QueueStep()
        {
            m_stepQueued = true;
            m_navMeshAgent.enabled = true;
        }

        public void DequeueStep()
        {
            m_stepQueued = false;
            m_navMeshAgent.enabled = false;
        }

        void Awake()
        {
            if (m_navMeshAgent == null)
            {
                m_navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            }
            
            m_navMeshAgent.enabled = false;
            m_stepQueued = false;
        }

        void Update()
        {
            if(m_stepQueued)
            {
                DequeueStep();
            }
        }

    }
}