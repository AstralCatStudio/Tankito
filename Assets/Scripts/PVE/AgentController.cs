using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

namespace Tankito.SinglePlayer
{
    public class AgentController : ATankController
    {
        public NavMeshAgent agent;
        [SerializeField] public NpcData npcData;
        protected override void Start()
        {
            base.Start();
            m_tankInput = GetComponent<ITankInput>();
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = npcData.speed;
            agent.angularSpeed = npcData.angularSpeed;
            m_aimSpeed = npcData.aimSpeed;
        }

        private void FixedUpdate()
        {
            InputPayload newInput = m_tankInput.GetInput();
            ProcessInput(newInput, Time.fixedDeltaTime);
        }

        protected override void MoveTank(Vector2 moveVector, float deltaTime) //El parámetro moveVector no es el Vector de movimiento, sino el destino al que queramos que vaya
                                                                               //el agente. No creamos una nueva función para aprovechar el ProcessInput de la clase padre, que
                                                                               //se encarga de gestionar que hacer con cada accion
        {
            agent.SetDestination(moveVector);
        }
    }
}

