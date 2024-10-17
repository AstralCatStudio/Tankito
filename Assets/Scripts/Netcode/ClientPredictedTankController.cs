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
        [SerializeField] private Rigidbody2D m_turretRB;
        [Tooltip("How fast the turret can turn to aim in the specified direction.")]
        [SerializeField]
        private float m_aimSpeed = 720f;
        [SerializeField] private CreateBullet m_cannon;
        public bool m_parrying = false;
        private Vector2 m_previousFrameAim;
        [SerializeField] private Animator m_TurretAnimator, m_HullAnimator;
        #endregion

        #region Client Netcode Variables

        // A lo mejor hay que cambiar esto porque es posible que de problemas cuando se hagan inputs mas rapidos que el tickRate
        // (creo (?) que la unidad minima de interaccion pasa a ser: LAST_INPUT durante TICK_RATE, emulando mantener ese input durante el tick completo)
        // - Bernat
        private InputPayload m_currentInput; // Almacena el ultimo, no es exactamente el "current", input percibido (eg. ultimo estado de un mando con polling rate de 1000Hz)
        private StatePayload m_lastAuthState; //Variable que almacena el estado de simulación autoritativo (enviado por servidor)
        private StatePayload m_reconciledState; // Ultimo estado al que se ha reconciliado
        private const int CACHE_SIZE = 1024;
        private CircularBuffer<InputPayload> m_inputStateCache = new CircularBuffer<InputPayload>(CACHE_SIZE);
        private CircularBuffer<StatePayload> m_simulationStateCache = new CircularBuffer<StatePayload>(CACHE_SIZE);        [SerializeField]
        private Tolerances m_reconciliationTolerance;


#endregion

#region Server Netcode Variables

        private Queue<InputPayload> m_serverInputQueue = new Queue<InputPayload>();
        private StatePayload m_lastClientPredictedState; // Latest reported client state

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

            if (m_turretRB == null)
            {
                Debug.Log("Error tank turret reference not set.");
            }
        }

        private void Update()
        {
            
        }

        void FixedUpdate()
        {
            // TODO: Implement Server-Client clock
            //while(ClockManager.simulationClock.TicksLeft())
            //int currentTick = ClockManager.simulationClock.CurrentTick;
            //Debug.Log("Tick: currentTick");

            if (IsOwner)
            {
                m_currentInput.timestamp = ClockManager.TickCounter; // MUY IMPORTANTE timestampear el input antes de pushearlo

                ProcessInput(m_currentInput);
                Physics2D.Simulate(ClockManager.SimDeltaTime);
                var currentState = GetSimulationState(ClockManager.TickCounter);
                
                m_inputStateCache.Add(m_currentInput, ClockManager.TickCounter);
                m_simulationStateCache.Add(currentState, ClockManager.TickCounter);
                
                SendPayloadsServerRpc(m_currentInput, currentState);

                if (!m_lastAuthState.Equals(default(StatePayload)) &&       // auth state recibido
                    m_lastAuthState.timestamp > m_reconciledState.timestamp &&      // estado reconciliado "caducado"
                    !CheckTolerance(m_lastAuthState.Diff(currentState), m_reconciliationTolerance))     // auth state fuera de limites
                {
                    Reconciliate(); //En caso necesario, reconciliación
                }
                m_reconciledState = m_lastAuthState;

            }
            else if (!IsServer)
            {
                // RECEIVE STATE DATA FROM SERVER ABOUT OTHER CLIENTS' TANKS  
            }
            if (IsServer)
            {
                // Obtain CharacterInputState's from the queue. 
                while (m_serverInputQueue.Count > 0)
                {
                    InputPayload clientInput = m_serverInputQueue.Dequeue();
                    ProcessInput(clientInput);
                    Physics2D.Simulate(ClockManager.SimDeltaTime);
                }

                // Obtain the current SimulationState.
                var newAuthState = GetSimulationState(ClockManager.TickCounter);

                // Send the state back to the client.
                SendAuthStateClientRpc(newAuthState);
            }

        }


        #region Input Methods
        public void OnMove(InputAction.CallbackContext ctx)
        {
            m_currentInput.moveVector = ctx.ReadValue<Vector2>();
        }
        
        public void OnAim(InputAction.CallbackContext ctx)
        {
            var input = ctx.ReadValue<Vector2>();
            Vector2 lookVector;

            if (ctx.control.path != "/Mouse/position")
            {
                lookVector = new Vector2(input.x, input.y);
            }
            else
            {
                // Mouse control fallback/input processing
                lookVector = input - (Vector2)Camera.main.WorldToScreenPoint(m_tankRB.position);
            }

            if (lookVector.magnitude > 1)
            {
                lookVector.Normalize();
            }

            m_currentInput.aimVector = lookVector;
        }

        public void OnDash(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<float>()==1)
            {
                m_currentInput.action =  TankAction.Dash;
            } else {
                Debug.Log("DASH false positive??? function called but action value false");
            }
        }

        public void OnParry(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<float>() == 1)
            {
                m_parrying = true;
                m_currentInput.action =  TankAction.Parry;
                m_TurretAnimator.SetTrigger("Parry");
                m_HullAnimator.SetTrigger("Parry");
            } else {
                Debug.Log("PARRY false positive??? function called but action value false");
            }
        }
        public void OnFire(InputAction.CallbackContext ctx)
        {
            if (ctx.ReadValue<float>() == 1)
            {
                m_currentInput.action = TankAction.Fire;
                m_cannon.Shoot();
            }
            else
            {
                Debug.Log("FIRE false positive??? function called but action value false");
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
            var targetAngle = Vector2.SignedAngle(m_tankRB.transform.right, movementVector);
            float rotDeg = 0f;

            if (Mathf.Abs(targetAngle) >= ClockManager.SimDeltaTime * m_rotationSpeed)
            {
                rotDeg = Mathf.Sign(targetAngle) * ClockManager.SimDeltaTime * m_rotationSpeed;
            }
            else
            {
                // Si el angulo es demasiado pequeño entonces snapeamos a él (inferior a la mínima rotación por frame)
                rotDeg = targetAngle;
            }

            m_tankRB.MoveRotation(m_tankRB.rotation + rotDeg);
            m_turretRB.MoveRotation(-rotDeg);
            
            m_tankRB.MovePosition(m_tankRB.position + m_speed * movementVector * ClockManager.SimDeltaTime);
        }

        private void AimTank(Vector2 aimVector)
        {
            var targetAngle = Vector2.SignedAngle(m_turretRB.transform.right, aimVector);
            float rotDeg = 0f;

            if(Mathf.Abs(targetAngle) >= ClockManager.SimDeltaTime * m_aimSpeed)
            {
                rotDeg = Mathf.Sign(targetAngle) * ClockManager.SimDeltaTime * m_aimSpeed;
            }
            else
            {
                rotDeg = targetAngle;
            }

            // MoveRotation doesn't work because the turretRB is not simulated
            // (we only use it for the uniform interface with rotation angle around Z).
            
            m_turretRB.MoveRotation(m_turretRB.rotation+rotDeg);
        }

        private StatePayload GetSimulationState(int timestamp)
        {
            return new StatePayload
            {
                timestamp = timestamp,
                position = m_tankRB.position,
                hullRot = m_tankRB.rotation,
                turretRot = m_turretRB.rotation,
                velocity = m_tankRB.velocity
            };
        }

#region RPC Calls

        [ServerRpc]
        private void SendPayloadsServerRpc(InputPayload input, StatePayload state)
        {
            m_serverInputQueue.Enqueue(input);
            m_lastClientPredictedState = state;

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

#region Client Utils

        private static bool CheckTolerance(Tolerances diff, Tolerances tolerance)
        {
            // Returns true if the diffs are under tolerance
            return (diff.pos <= tolerance.pos) && (diff.rot <= tolerance.rot) && (diff.vel <= tolerance.vel);
        }

#endregion

#region Client Reconciliation

        private void SetState(StatePayload stateToSet)
        {
            m_tankRB.MovePosition(stateToSet.position);
            m_tankRB.MoveRotation(stateToSet.hullRot);
            m_tankRB.velocity = stateToSet.velocity;

            // DO SOMETHING ABOUT TANK ACTIONS...
        }

        private void Reconciliate()
        {
            // Snap to newly received auth state
            m_tankRB.MovePosition(m_lastAuthState.position);
            m_tankRB.SetRotation(m_lastAuthState.hullRot);
            m_tankRB.velocity = m_lastAuthState.velocity;

            // Resimulate from there until we get back to the "present tick"
            int rewindTick = m_lastAuthState.timestamp;
            while(rewindTick < ClockManager.TickCounter)
            {
                // Get cached input payloads
                InputPayload rewindInput = m_inputStateCache.Get(rewindTick);
                
                ProcessInput(rewindInput);

                m_simulationStateCache.Add(GetSimulationState(rewindTick),rewindTick);
                rewindTick++;
            }
        }

#endregion
    }

    [Serializable]
    internal struct Tolerances
    {
        public float pos;
        public float rot;
        public float vel;

        public Tolerances(float pos, float rot, float vel)
        {
            this.pos = pos;
            this.rot = rot;
            this.vel = vel;
        }

        public override bool Equals(object obj)
        {
            return obj is Tolerances other &&
                   pos == other.pos &&
                   rot == other.rot &&
                   vel == other.vel;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(pos, rot, vel);
        }

        public void Deconstruct(out float pos, out float rot, out float vel)
        {
            pos = this.pos;
            rot = this.rot;
            vel = this.vel;
        }

        public static implicit operator (float pos, float rot, float vel)(Tolerances value)
        {
            return (value.pos, value.rot, value.vel);
        }

        public static implicit operator Tolerances((float pos, float rot, float vel) value)
        {
            return new Tolerances(value.pos, value.rot, value.vel);
        }
    }
}

