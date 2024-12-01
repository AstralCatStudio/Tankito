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
        NavMeshAgent agent;
        protected override void Start()
        {
            base.Start();
            agent = GetComponent<NavMeshAgent>();
            agent.speed = m_speed;
            agent.angularSpeed = m_rotationSpeed;
        }

        private void FixedUpdate()
        {
            InputPayload newInput = m_tankInput.GetInput();
            ProcessInput(newInput, 0);
        }

        protected override void MoveTank(Vector2 moveVector, float deltaTime) //El parámetro moveVector no es el Vector de movimiento, sino el destino al que queramos que vaya
                                                                               //el agente. No creamos una nueva función para aprovechar el ProcessInput de la clase padre, que
                                                                               //se encarga de gestionar que hacer con cada accion
        {
            agent.SetDestination(moveVector);
        }
    }
}

