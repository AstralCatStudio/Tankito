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
        private bool importantDestinyFlag = false;
        public bool ImportantDestiny { get => importantDestinyFlag; set => importantDestinyFlag = value; }
        protected override void Start()
        {
            base.Start();
            m_tankInput = GetComponent<ITankInput>();
            agent = GetComponent<NavMeshAgent>();
            //agent.updateRotation = false;
            //agent.updateUpAxis = false;
            agent.speed = npcData.speed;
            agent.angularSpeed = npcData.angularSpeed;
            m_aimSpeed = npcData.aimSpeed;

            m_tankRB.MoveRotation(90);
        }

        private void Update()
        {
            updateDesTimer += Time.deltaTime;
            InputPayload newInput = m_tankInput.GetInput();
            ProcessInput(newInput, Time.fixedDeltaTime);
        }

        private void FixedUpdate()
        {
            // Obtener el vector de dirección hacia el destino actual del agente
            Vector2 moveDirection = agent.velocity;

            // Normalizar el vector si tiene una magnitud significativa
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                moveDirection.Normalize();

                // Calcular el ángulo hacia la dirección del movimiento
                var targetAngle = Vector2.SignedAngle(m_tankRB.transform.right, moveDirection);

                // Definir un umbral mínimo para considerar la rotación
                float angleThreshold = 20f; // Solo girar si la diferencia angular es mayor a este valor
                float rotDeg = 0f;

                if (Mathf.Abs(targetAngle) > angleThreshold)
                {
                    if (Mathf.Abs(targetAngle) >= Time.fixedDeltaTime * agent.angularSpeed)
                    {
                        rotDeg = Mathf.Sign(targetAngle) * Time.fixedDeltaTime * agent.angularSpeed;
                    }
                    else
                    {
                        rotDeg = targetAngle;
                    }

                    // Aplicar la rotación calculada
                    m_tankRB.MoveRotation(m_tankRB.rotation + rotDeg);
                }
            }
        }






        protected override void MoveTank(Vector2 moveVector, float deltaTime) //El parámetro moveVector no es el Vector de movimiento, sino el destino al que queramos que vaya
                                                                              //el agente. No creamos una nueva función para aprovechar el ProcessInput de la clase padre, que
                                                                              //se encarga de gestionar que hacer con cada accion
        {
            float destinyDelta = Vector2.Distance(lastDestiny, moveVector);
            if ((updateDesTimer >= updateTime && destinyDelta > minDesDelta) || destinyDelta >= maxDesDelta || ImportantDestiny)
            {
                agent.SetDestination(moveVector);
                lastDestiny = moveVector;
                updateDesTimer = 0;
                ImportantDestiny = false;
            }

            /*float posDesDelta = Vector2.Distance(lastDestiny, transform.position);
            if (posDesDelta <= posTolerance)
            {
                agent.speed = 0;
            }
            else
            {
                agent.speed = npcData.speed;
            }*/
        }
    }
}

