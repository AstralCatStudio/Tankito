using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tankito.Netcode.Simulation;
using System;
using UnityEngine.AI;

namespace Tankito.PVE
{
    public class NPCTankController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D m_hullRB;
        [SerializeField] private Rigidbody2D m_turretRB;

        [SerializeField] SteppedNavMeshAgent m_steppedNavMeshAgent;
        [SerializeField] private float m_hullRotationSpeed = 720f;

        private Rigidbody2D m_chaseTarget;

        public Vector2 Position { get => m_hullRB.position; }


        void Awake()
        {
            if (m_steppedNavMeshAgent == null)
            {
                m_steppedNavMeshAgent = GetComponent<SteppedNavMeshAgent>();
            }
        }

        void Update()
        {

        }

        void OnEnable()
        {
            // Subscribe to SimulationObject Kinematics
            var tankSimObj = GetComponent<TankSimulationObject>();
            tankSimObj.OnComputeKinematics += NPCMovement;
            tankSimObj.IsKinematic = true;
        }


        void OnDisable()
        {
            // Subscribe to SimulationObject Kinematics
            var tankSimObj = GetComponent<TankSimulationObject>();
            tankSimObj.OnComputeKinematics -= NPCMovement;
        }


        private void NPCMovement(float deltaTime)
        {
            Move(deltaTime);
            Aim(deltaTime);
        }


        ////////////////////////////////////////////////////////////////////////////////
        /////////////////////////// METODOS AUXILIARES /////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////
        
        public void SetChaseTarget(Rigidbody2D target)
        {
            m_chaseTarget = target;
        }

        public void SetTargetPosition(Vector2 position)
        {
            ClearChaseTarget();

            m_steppedNavMeshAgent.SetDestination(position);
        }

        public void ClearChaseTarget()
        {
            m_chaseTarget = null;
        }

        private void Move(float deltaTime)
        {
            if (m_chaseTarget != null)
            {
                m_steppedNavMeshAgent.SetDestination(m_chaseTarget.position);
            }

            Vector2 nextPosition = m_steppedNavMeshAgent.StepAlongPath(deltaTime);
            Vector2 movementVector = nextPosition - m_hullRB.position;
            var targetAngle = Vector2.SignedAngle(m_hullRB.transform.right, movementVector);
            float rotDeg = 0f;

            if (Mathf.Abs(targetAngle) >= deltaTime * m_hullRotationSpeed)
            {
                rotDeg = Mathf.Sign(targetAngle) * deltaTime * m_hullRotationSpeed;
            }
            else
            {
                // Si el angulo es demasiado pequeño entonces snapeamos a él (inferior a la mínima rotación por frame)
                rotDeg = targetAngle;
            }

            m_hullRB.MoveRotation(m_hullRB.rotation + rotDeg);
            m_turretRB.MoveRotation(-rotDeg);

            m_hullRB.MovePosition(nextPosition);
        }

        private void Aim(float deltaTime)
        {

        }

    }

}