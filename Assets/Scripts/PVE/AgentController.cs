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
        Vector2 lastDestiny;
        [SerializeField] public NpcData npcData;
        [SerializeField] float updateDesTimer = 0;
        [SerializeField] float updateTime = 0.5f;
        [SerializeField] float minDesDelta = 1f;
        [SerializeField] float maxDesDelta = 10f;
        [SerializeField] float posTolerance = 0.5f;
        private bool importantDestinyFlag = false;
        public bool ImportantDestiny { get => importantDestinyFlag; set => importantDestinyFlag = value; }
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

        private void Update()
        {
            updateDesTimer += Time.deltaTime;
            InputPayload newInput = m_tankInput.GetInput();
            ProcessInput(newInput, Time.fixedDeltaTime);
        }

        protected override void MoveTank(Vector2 moveVector, float deltaTime) //El parámetro moveVector no es el Vector de movimiento, sino el destino al que queramos que vaya
                                                                               //el agente. No creamos una nueva función para aprovechar el ProcessInput de la clase padre, que
                                                                               //se encarga de gestionar que hacer con cada accion
        {
            float destinyDelta = Vector2.Distance(lastDestiny, moveVector);
            if((updateDesTimer >= updateTime && destinyDelta > minDesDelta) || destinyDelta >= maxDesDelta || ImportantDestiny)
            {
                agent.SetDestination(moveVector);
                lastDestiny = moveVector;
                updateDesTimer = 0;
                ImportantDestiny = false;
            }

            float posDesDelta = Vector2.Distance(lastDestiny, transform.position);
            if (posDesDelta <= posTolerance)
            {
                agent.speed = 0;
            }
            else
            {
                agent.speed = npcData.speed;
            }
        }
    }
}

