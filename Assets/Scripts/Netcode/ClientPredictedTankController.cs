 using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Tankito.Utils;
using System;

namespace Tankito.Netcode
{
    public class ClientPredictedTankController : NetworkBehaviour
    {
#region Variables

        [SerializeField] private Rigidbody2D m_tankRB;
        [SerializeField] private float m_speed = 25.0f;
        [SerializeField] private float m_rotationSpeed = 1.0f;
        [SerializeField] private Transform m_turret;
        [Tooltip("How fast the turret can turn to aim in the specified direction.")]
        [SerializeField]
        private float m_aimSpeed = 720f;

        //private NetworkVariable<Vector2> _TankPosition = new NetworkVariable<Vector2>(new Vector2(), NetworkVariableReadPermission.Everyone, 
        //    NetworkVariableWritePermission.Server);
        //private NetworkVariable<Quaternion> _TankRotation = new NetworkVariable<Quaternion>(new Quaternion(), NetworkVariableReadPermission.Everyone,
        //    NetworkVariableWritePermission.Server);

#endregion

#region Client Netcode Variables

        private InputPayload m_latestInputState; // Almacena el ultimo input percibido (eg. ultimo estado de un mando con polling rate de 1000Hz)
        private StatePayload m_lastAuthState; //Variable que almacena el estado de simulación del servidor
        private const int CACHE_SIZE = 1024;
        private CircularBuffer<InputPayload> m_inputStateCache = new CircularBuffer<InputPayload>(CACHE_SIZE);
        private CircularBuffer<StatePayload> m_simulationStateCache = new CircularBuffer<StatePayload>(CACHE_SIZE);

#endregion

#region Server Netcode Variables

        private Queue<InputPayload> m_serverInputQueue = new Queue<InputPayload>();
        private StatePayload m_reportedClientState; // Latest reported client state

#endregion

        // Start is called before the first frame update
        void Start()
        {
            if (m_tankRB == null)
            {
                m_tankRB = GetComponent<Rigidbody2D>();
                if (m_tankRB == null)
                {
                    Debug.Log("Error tank Rigibody2D reference not set.");
                }
            }

            if (m_turret == null)
            {
                Debug.Log("Error tank turret reference not set.");
            }
        }

        private void Update()
        {
            while(ClockManager.simulationClock.TicksLeft())
            {
                int currentTick = ClockManager.simulationClock.CurrentTick;
                //Debug.Log("Tick: currentTick");

                if (IsOwner)
                {
                    m_latestInputState.timestamp = currentTick; // MUY IMPORTANTE timestampear el input antes de pushearlo

                    ProcessInput(m_latestInputState);
                    Physics2D.Simulate(ClockManager.SERVER_SIMULATION_DELTA_TIME);
                    var currentState = GetSimulationState(currentTick);
                    
                    m_inputStateCache.Add(m_latestInputState, currentTick);
                    m_simulationStateCache.Add(currentState, currentTick);
                    
                    SendPayloadsServerRpc(m_latestInputState, currentState);

                    Debug.Log($"Client: Updated simulation [{currentTick}]");
                }
                else if (!IsServer)
                {
                    // RECEIVE STATE DATA FROM SERVER ABOUT OTHER CLIENTS' TANKS  
                }
                if (IsServer)
                {
                    // ESTO ESTA MAL AQUI TIENE QUE FALTAR ALGO, NO PUEDE SER QUE SIN INPUTS EN COLA EL SERVER NO HAGA NADA Y AUMENTEN LOS TICKS Y YA, NO?
                    // me voy a dormir
                    // Obtain CharacterInputState's from the queue. 
                    while (m_serverInputQueue.Count > 0)
                    {
                        InputPayload clientInput = m_serverInputQueue.Dequeue();
                        Debug.Log("La psoición inicial en el SERVIDOR antes de procesar FRAME" + clientInput.timestamp + " es: " + m_tankRB.position + "-" + m_tankRB.rotation);
                        // Process the input.
                        ProcessInput(clientInput);
                    }
                    Physics2D.Simulate(ClockManager.SERVER_SIMULATION_DELTA_TIME);

                    // Obtain the current SimulationState.
                    StatePayload newAuthState = GetSimulationState(currentTick);

                    // Send the state back to the client.
                    SendAuthStateClientRpc(newAuthState);
                }
            }
        }


#region Input Methods
        public void OnMove(InputAction.CallbackContext ctx)
        {
            m_latestInputState.moveVector = ctx.ReadValue<Vector2>();
        }
        
        public void OnAim(InputAction.CallbackContext ctx)
        {
            m_latestInputState.aimVector = ctx.ReadValue<Vector2>();
        }

        public void OnDash(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<bool>())
            {
                m_latestInputState.action =  TankAction.Dash;
            } else {
                Debug.Log("DASH false positive??? function called but action value false");
            }
        }

        public void OnParry(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<bool>())
            {
                m_latestInputState.action =  TankAction.Parry;
            } else {
                Debug.Log("PARRY false positive??? function called but action value false");
            }
        }
#endregion

        private void ProcessInput(InputPayload input)
        {
            MoveTank(input.moveVector);
            AimTank(input.aimVector);
            /*
            if (IsServer)
            {
                Debug.Log("SERVIDOR " + input.timestamp + ": ENTRADA " + input.timestamp + ClockManager.SERVER_SIMULATION_DELTA_TIME + "- SALIDA " + m_tankRB.position + m_tankRB.rotation);
            }
            else
            {
                Debug.Log("CLIENTE " + input.timestamp + ": ENTRADA " + input.timestamp + ClockManager.SERVER_SIMULATION_DELTA_TIME + "- SALIDA " + m_tankRB.position + m_tankRB.rotation);

            }
            */
        }

        private void MoveTank(Vector2 movementVector)
        {
            var targetAngle = Vector2.SignedAngle(transform.right, movementVector);
            float rotDeg = 0f;

            if (Mathf.Abs(targetAngle) >= ClockManager.SERVER_SIMULATION_DELTA_TIME * m_rotationSpeed)
            {
                rotDeg = Mathf.Sign(targetAngle) * ClockManager.SERVER_SIMULATION_DELTA_TIME * m_rotationSpeed;
            }
            else
            {
                // Si el angulo es demasiado pequeño entonces snapeamos a él (inferior a la mínima rotación por frame)
                rotDeg = targetAngle;
            }

            m_tankRB.MoveRotation(m_tankRB.rotation + rotDeg);
            m_turret.Rotate(new Vector3(0, 0, -rotDeg));
            
            m_tankRB.MovePosition(m_tankRB.position + m_speed * movementVector * ClockManager.SERVER_SIMULATION_DELTA_TIME);
        }

        private void AimTank(Vector2 aimVector)
        {
            var targetAngle = Vector2.SignedAngle(m_turret.transform.right, aimVector);
            float rotDeg = 0f;

            if(Mathf.Abs(targetAngle) >= Time.fixedDeltaTime*m_aimSpeed)
            {
                rotDeg = Mathf.Sign(targetAngle)*Time.fixedDeltaTime*m_aimSpeed;
            }
            else
            {
                rotDeg = targetAngle;
            }

            m_turret.transform.rotation = Quaternion.Euler(0,0,m_turret.transform.eulerAngles.z+rotDeg);
        }

        private StatePayload GetSimulationState(int timestamp)
        {
            return new StatePayload
            {
                timestamp = timestamp,
                position = m_tankRB.position,
                rotation = m_tankRB.rotation,
                velocity = m_tankRB.velocity
            };
        }

#region RPC Calls

        [ServerRpc]
        private void SendPayloadsServerRpc(InputPayload input, StatePayload state)
        {
            m_serverInputQueue.Enqueue(input);
            m_reportedClientState = state;

            /*ProcessInput(serverInputState);

            SimulationState serverSimulation = GetSimulationState(serverInputState);

            SendServerSimulationToClientRpc(serverSimulation.position, serverSimulation.rotation, serverSimulation.simulationFrame);*/
        }
        [ClientRpc]
        private void SendAuthStateClientRpc(StatePayload authState)
        {
            if (IsOwner)
            {
                if (m_lastAuthState.timestamp < authState.timestamp)
                {
                    m_lastAuthState = authState;
                }
                //Debug.Log("La simulación del SERVIDOR en el frame " + serverSimulationState.simulationFrame + " es: " + serverSimulationState.position + "-" + serverSimulationState.rotation);

                SetState(authState);
            }   
        }

#endregion

#region Client Reconciliation

        private void SetState(StatePayload stateToSet)
        {
            m_tankRB.MovePosition(stateToSet.position);
            m_tankRB.MoveRotation(stateToSet.rotation);
            m_tankRB.velocity = stateToSet.velocity;

            // DO SOMETHING ABOUT TANK ACTIONS...
        }

/*
        private void Reconciliate()
        {
            //Debug.Log("Comienza la reconciliación");
            if (m_lastAuthState.simulationFrame <= lastCorrectedFrame) return;

            int cache_index = m_lastAuthState.simulationFrame % CACHE_SIZE;
            ClientInputState<Vector2> cachedInput = m_inputStateCache[cache_index];
            SimulationState cachedSimulation = m_simulationStateCache[cache_index];

            if (cachedInput == null || cachedSimulation == null)
            {
                transform.position = m_lastAuthState.position;
                transform.rotation = m_lastAuthState.rotation;

                lastCorrectedFrame = m_lastAuthState.simulationFrame;
                return;
            }

            float tolerancePosition = 0.1f;
            float toleranceRotation = 0.1f;

            float differenceX = Mathf.Abs(cachedSimulation.position.x - m_lastAuthState.position.x);
            float differenceY = Mathf.Abs(cachedSimulation.position.y - m_lastAuthState.position.y);
            float differenceZ = Mathf.Abs(cachedSimulation.position.z - m_lastAuthState.position.z);
            float differenceRotationX = Mathf.Abs(cachedSimulation.rotation.x - m_lastAuthState.rotation.x);
            float differenceRotationY = Mathf.Abs(cachedSimulation.rotation.y - m_lastAuthState.rotation.y);
            float differenceRotationZ = Mathf.Abs(cachedSimulation.rotation.z - m_lastAuthState.rotation.z);
            float differenceRotationW = Mathf.Abs(cachedSimulation.rotation.w - m_lastAuthState.rotation.w);

        //Debug.Log("Posicion" + cachedSimulation.simulationFrame + ": "+ cachedSimulation.position + "-" + serverSimulationState.simulationFrame + serverSimulationState.position);
        //Debug.Log("Rotacion" + cachedSimulation.simulationFrame + ": "+ cachedSimulation.rotation + " - " + serverSimulationState.simulationFrame + serverSimulationState.rotation);

            if (differenceRotationX > toleranceRotation || differenceRotationY > toleranceRotation || differenceRotationZ > toleranceRotation ||
                differenceRotationW > toleranceRotation || differenceX > tolerancePosition || differenceY > tolerancePosition 
                || differenceZ > tolerancePosition)
            {
                transform.position = m_lastAuthState.position;
                transform.rotation = m_lastAuthState.rotation;

                int rewindFrame = m_lastAuthState.simulationFrame;
                while(rewindFrame < simulationFrame)
                {
                    int rewindCacheIndex = rewindFrame % CACHE_SIZE;
                    ClientInputState<Vector2> rewindCachedInput = m_inputStateCache[rewindCacheIndex];
                    SimulationState rewindCachedSimulation = m_simulationStateCache[rewindCacheIndex];

                    if (rewindCachedInput == null || rewindCachedSimulation == null)
                    {
                        rewindFrame++;
                        continue;
                    }

                    ProcessInput(rewindCachedInput);

                    SimulationState rewoundSimulationState = GetSimulationState(rewindCachedInput);
                    rewoundSimulationState.simulationFrame = simulationFrame;
                    m_simulationStateCache[rewindCacheIndex] = rewoundSimulationState;
                    rewindFrame++;
                }
            }

            lastCorrectedFrame = m_lastAuthState.simulationFrame;
        }
        */

#endregion
    }
}

