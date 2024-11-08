using Tankito.Netcode;
using Tankito.Utils;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tankito
{
    // He puesto una interfaz porque asi se puede modularizar el uso del simulador de inputs para el Dead Reckoning
    public interface ITankInput
    {
        InputPayload GetInput();
        void StartInputReplay(int timestamp);
        int StopInputReplay();

    }

    public class TankPlayerInput : MonoBehaviour, ITankInput
    {
        private const int INPUT_CACHE_SIZE = 256;
        private CircularBuffer<InputPayload> m_inputCache = new CircularBuffer<InputPayload>(INPUT_CACHE_SIZE);
        private int m_inputReplayTick = NO_REPLAY;
        private const int NO_REPLAY = -1;

        private InputPayload m_currentInput;
        [SerializeField]
        private TankController m_tankController;
        [SerializeField]
        private Animator m_turretAnimator, m_hullAnimator;
        [SerializeField]
        private Rigidbody2D m_turretRB;

        void Awake()
        {
            m_inputCache = new CircularBuffer<InputPayload>(INPUT_CACHE_SIZE);
        }

        void Start()
        {
            if (m_turretRB == null) Debug.LogWarning("Turret Rigidbody2D reference not set.");
            if (m_turretAnimator == null) Debug.LogWarning("Turret Animator reference not set!");
            if (m_hullAnimator == null) Debug.LogWarning("Hull Animator reference not set!");
            if (m_tankController == null) m_tankController = GetComponent<TankController>();
            if (m_tankController == null) Debug.LogWarning("Tank Controller reference not set!");
            m_inputReplayTick = NO_REPLAY;
        }

        /// <summary>
        /// Returns <see cref="m_currentInput" />. Unless it is in replay mode (<see cref="m_inputReplayTick"/> == <see cref="NO_REPLAY"/>), this is to enable automatic input replay on rollback.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public InputPayload GetInput()
        {
            if (m_inputReplayTick == NO_REPLAY)
            {
                // Live Input Mode
                Debug.Log("Tick:" + ClockManager.TickCounter);
                m_inputCache.Add(m_currentInput, ClockManager.TickCounter);
                InputWindowBuffer.Instance.AddInputToWindow(m_currentInput);
                return m_currentInput;
            }
            else
            {
                // Input Replay Mode
                var replayedInput = m_inputCache.Get(m_inputReplayTick);
                m_inputReplayTick++;
                return replayedInput;
            }
        }

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
        public void StartInputReplay(int timestamp)
        {
            m_inputReplayTick = timestamp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> Last replayed input tick.</returns>
        public int StopInputReplay()
        {
            var lastReplayTick = m_inputReplayTick;
            m_inputReplayTick = NO_REPLAY;
            return lastReplayTick;
        }

        public void OnMove(InputAction.CallbackContext ctx)
        {
            if (m_tankController.PlayerState != PlayerState.Dashing)
            {
                var input = ctx.ReadValue<Vector2>();

                if (input.sqrMagnitude > 1) input.Normalize();

                m_currentInput.moveVector = input;
                m_currentInput.action = TankAction.None;
            }
            else
            {
                Debug.Log("SE RECOGE EL INPUT WHILE DASH");
                m_tankController.inputWhileDash = ctx.ReadValue<Vector2>(); // Esto no deberia de hacerse asi....
            }
        }

        public void OnDash(InputAction.CallbackContext ctx)
        {
            if (m_tankController.canDash) // Esto esta mal, se deberia recoger el dash siempre y controlar los efectos que tiene en la logica del controlador
                        //  Mas que nada porque es que si no no es servidor autoritativo y se pueden hacer trampas.
            {
                m_currentInput.action = TankAction.Dash;
            }
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
                lookVector = input - (Vector2)Camera.main.WorldToScreenPoint(m_turretRB.position);
            }

            if (lookVector.sqrMagnitude > 1)
            {
                lookVector.Normalize();
            }

            m_currentInput.aimVector = lookVector;
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
                Debug.Log($"Parry {ctx.phase}");
            }
        }

        public void OnFire(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                m_currentInput.action = TankAction.Fire;
                //m_cannon.Shoot(); // Esto va a haber que cambiarlo a que se realice el evento de disparo a golpe de simulacion, no cuando se haga el input!
            }
            else
            {
                Debug.Log($"Fire {ctx.phase}");
            }
        }

    }
}