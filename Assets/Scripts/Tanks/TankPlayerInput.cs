using Tankito.Netcode;
using Tankito.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static Tankito.TankController;

namespace Tankito
{
    // He puesto una interfaz porque asi se puede modularizar el uso del simulador de inputs para el Dead Reckoning
    public interface ITankInput
    {
        /// <summary>
        /// Must implement conditional getter that also works correctly in replay mode.
        /// </summary>

        InputPayload GetInput();
        /// <summary>
        /// Must implement conditional getter that also works correctly in replay mode.
        /// </summary>
        InputPayload LastInput { get; }

        /// <summary>
        /// Makes the <see cref="TankPlayerInput.GetInput()" /> method return cached input, starting from the given timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp from which to start replaying the input.</param>
        /// <remarks>
        /// This method sets the <see cref="m_inputReplayTick" /> to <see cref="timestamp"/>, indicating that the input replay mode is active.
        /// When <see cref="TankPlayerInput.GetInput()" /> is called during replay mode, it returns the cached input at the current replay tick.
        /// The replay tick is incremented with each call to <see cref="TankPlayerInput.GetInput()" /> until it reaches the end of the cached inputs,
        /// at which point <see cref="StopInputReplay()"/> should be called.
        /// </remarks>
        void StartInputReplay(int timestamp);
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns> Last replayed input tick.</returns>
        int StopInputReplay();

    }

    public class TankPlayerInput : MonoBehaviour, ITankInput
    {
        private int INPUT_CACHE_SIZE => SimulationParameters.SNAPSHOT_BUFFER_SIZE;
        private CircularBuffer<InputPayload> m_inputCache;
        [SerializeField] private int m_inputReplayTick = NO_REPLAY;
        private const int NO_REPLAY = -1;

        public InputPayload LastInput => m_inputReplayTick==NO_REPLAY ? m_currentInput : m_inputCache.Get(m_inputReplayTick);
        private InputPayload m_currentInput;
        [SerializeField]
        private TankController m_tankController;
        [SerializeField]
        private Animator m_turretAnimator, m_hullAnimator;
        [SerializeField]
        private Rigidbody2D m_turretRB;
        [SerializeField] private bool DEBUG = false;

        private Vector2? mousePosition;

        void Awake()
        {
            m_inputCache = new CircularBuffer<InputPayload>(INPUT_CACHE_SIZE);
        }

        void Start()
        {
            m_inputCache = new CircularBuffer<InputPayload>(INPUT_CACHE_SIZE);
            if (m_turretRB == null) Debug.LogWarning("Turret Rigidbody2D reference not set.");
            if (m_turretAnimator == null) Debug.LogWarning("Turret Animator reference not set!");
            if (m_hullAnimator == null) Debug.LogWarning("Hull Animator reference not set!");
            if (m_tankController == null) m_tankController = GetComponent<TankController>();
            if (m_tankController == null) Debug.LogWarning("Tank Controller reference not set!");
            m_inputReplayTick = NO_REPLAY;
        }

        private void OnEnable()
        {
            m_tankController.OnDashEnd += this.DashEnd;
        }

        private void OnDisable()
        {
            m_tankController.OnDashEnd -= this.DashEnd;
        }

        /// <summary>
        /// Returns <see cref="m_currentInput" />. Unless it is in replay mode (<see cref="m_inputReplayTick"/> == <see cref="NO_REPLAY"/>), this is to enable automatic input replay on rollback.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public InputPayload GetInput()
        {
            
            InputPayload gotPayload;
            if (m_inputReplayTick == NO_REPLAY)
            {
                // Live Input Mode
                Aim();
                m_currentInput.timestamp = SimClock.TickCounter;
                m_inputCache.Add(m_currentInput, SimClock.TickCounter);
                InputWindowBuffer.Instance.AddInputToWindow(m_currentInput);
                
                gotPayload = m_currentInput;
            }
            else
            {
                m_inputReplayTick++;
                // Input Replay Mode
                var replayedInput = m_inputCache.Get(m_inputReplayTick);
                gotPayload = replayedInput;
            }

            // The internal state of m_currentInput must be reset back to TankAction.None, in order to avoid
            // continuously posting the same action after all ticks.
            if (m_currentInput.action == TankAction.Fire || m_currentInput.action == TankAction.Dash || m_currentInput.action == TankAction.Parry)
            {
                m_currentInput.action = TankAction.None;
            }
            
            if (DEBUG)
            {
                Debug.Log($"GetInput{(m_inputReplayTick!=NO_REPLAY?("["+m_inputReplayTick+"]") : "")}:" + m_currentInput);
            }

            return gotPayload;
        }

        public void StartInputReplay(int timestamp)
        {
            m_inputReplayTick = timestamp;
        }
        
        public int StopInputReplay()
        {
            var lastReplayTick = m_inputReplayTick;
            m_inputReplayTick = NO_REPLAY;
            return lastReplayTick;
        }

        private void DashEnd()
        {
            if(m_currentInput.action == TankAction.Dash)
            {
                m_currentInput.action = TankAction.None;
            }
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {           
            var input = ctx.ReadValue<Vector2>();

            if (input.sqrMagnitude > 1) input.Normalize();

            m_currentInput.moveVector = input;
            m_currentInput.action = TankAction.None;
        }

        public void OnDash(InputAction.CallbackContext ctx)
        {
            m_currentInput.action = TankAction.Dash; 
        }
        void Aim()
        {
            Vector2 lookVector;
            if (mousePosition != null)
            {
                
                lookVector = (Vector2)mousePosition - (Vector2)Camera.main.WorldToScreenPoint(m_turretRB.position);
                
                if (lookVector.sqrMagnitude > 1)
                {
                    lookVector.Normalize();
                }
                m_currentInput.aimVector = lookVector;
            }
        }
        public void OnAim(InputAction.CallbackContext ctx)
        {
            var input = ctx.ReadValue<Vector2>();
            Vector2 lookVector;

            if (ctx.control.path != "/Mouse/position")
            {
                lookVector = new Vector2(input.x, input.y);
                mousePosition = null;
                if (lookVector.sqrMagnitude > 1)
                {
                    lookVector.Normalize();
                }
                m_currentInput.aimVector = lookVector;
            }
            else
            {
                // Mouse control fallback/input processing
                mousePosition = input;
                Aim();
                
            }

            

            
            
        }


        public void OnParry(InputAction.CallbackContext ctx)
        {
            // CAMBIAR POR CHECKS DE DISPARO?
            // (comprobar si el valor de ctx.ReadValue == 1
            // es redundante y ademas entra en conflicto con
            // el threshold de activacion configurado por el input system,
            // en otras palabras no activa la accion para controles analogicos como el trigger de un mando)

            if (ctx.performed)
            {
                //m_parrying = true; // Solo se utiliza para la animacion no lo entiendo ????
                m_currentInput.action = TankAction.Parry;
                m_turretAnimator.SetTrigger("Parry");
                m_hullAnimator.SetTrigger("Parry");
            }
            else
            {
                if (DEBUG) Debug.Log($"Parry {ctx.phase}");
            }
        }

        public void OnFire(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                m_currentInput.action = TankAction.Fire;
            }
            else
            {
                if (DEBUG) Debug.Log($"Fire {ctx.phase}");
            }
        }

    }
}