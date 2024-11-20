using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tankito.Netcode.Simulation;
using System;
using UnityEngine.AI;

namespace Tankito
{
    public class NPCTankController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D m_hullRB;
        [SerializeField] private Rigidbody2D m_turretRB;

        [SerializeField] NavMeshAgent m_navMeshAgent;

        private Rigidbody2D m_chaseTarget;


        public Vector2 Position { get => m_hullRB.position; }


        void Start()
        {

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
            throw new NotImplementedException();
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

            m_navMeshAgent.SetDestination(position);
        }

        public void ClearChaseTarget()
        {
            m_chaseTarget = null;
        }

        private void Move()
        {
            if (m_chaseTarget != null)
            {
                m_navMeshAgent.SetDestination(m_chaseTarget.position);
            }

            Vector2 direccion = ((Vector2)m_navMeshAgent.nextPosition - m_hullRB.position).normalized;
            Vector2 nuevaPosicion = (Vector2)transform.position + direccion * m_navMeshAgent.speed * Time.deltaTime;
            transform.position = new Vector2(nuevaPosicion.x, nuevaPosicion.y);
            GirarHacia((Vector2)m_navMeshAgent.nextPosition);
        }

        private void GirarHacia(Vector3 posicionObjetivo)
        {
            Vector2 direccion = (posicionObjetivo - transform.position).normalized;
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angulo);
        }



    }

}