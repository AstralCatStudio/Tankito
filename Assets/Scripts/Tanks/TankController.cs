using System.Collections;
using Tankito.Netcode;
using Tankito.Netcode.Simulation;
using UnityEngine;
using UnityEngine.Windows;

namespace Tankito
{
    public enum PlayerState
    {
        Moving,
        Dashing,
        Parrying
    }

    public class TankController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D m_tankRB;
        [SerializeField] private float m_speed = 2.2f;
        [SerializeField] private float m_rotationSpeed = 270.0f;
        [SerializeField] private Rigidbody2D m_turretRB;
        [Tooltip("How fast the turret can turn to aim in the specified direction.")]
        [SerializeField]
        private float m_aimSpeed = 720f;

        //Variables Dash
        [SerializeField] private float accelerationMultiplier = 3;
        [SerializeField] private float dashDuration = 0.25f;
        [SerializeField] private float fullDashDuration = 0.1f;
        [SerializeField] private float dashReloadDuration = 1;
        private int currentDashReloadTick = -1;
        int dashTicks;
        int fullDashTicks;
        int reloadDashTicks;
        [SerializeField] int stateInitTick;


        [SerializeField] private PlayerState playerState = PlayerState.Moving;

        public ITankInput TankInputComponent { get => m_tankInput; set { if (m_tankInput==null) m_tankInput = value; else Debug.LogWarning($"TankInputComponent for {this} was already set!");} }
        [SerializeField] private ITankInput m_tankInput;
        [SerializeField] private bool DEBUG = true;
        private Vector2 dashVec;

        public delegate void DashEnd();
        public event DashEnd OnDashEnd;

        public PlayerState PlayerState { get => playerState; set => playerState = value; }
        public int StateInitTick { get => stateInitTick; set => stateInitTick = value; }
        private bool CanDash { get => CheckCanDash() && playerState != PlayerState.Parrying; }

        void Start()
        {
            if (m_tankRB == null)
            {
                m_tankRB = GetComponent<Rigidbody2D>();
                if (m_tankRB == null)
                {
                    Debug.LogWarning("Error tank Rigibody2D reference not set.");
                }
            }

            if (m_turretRB == null)
            {
                Debug.LogWarning("Error tank turret reference not set.");
            }
            dashTicks = Mathf.CeilToInt(dashDuration / SimClock.SimDeltaTime);
            fullDashTicks = Mathf.CeilToInt(fullDashDuration / SimClock.SimDeltaTime);
            reloadDashTicks = Mathf.CeilToInt(dashReloadDuration / SimClock.SimDeltaTime);
            stateInitTick = 1;
        }

        void OnEnable()
        {
            // Subscribe to SimulationObject Kinematics
            var tankSimObj = GetComponent<TankSimulationObject>();
            tankSimObj.OnComputeKinematics += ProcessInput;
            tankSimObj.IsKinematic = true;
        }

        void OnDisable()
        {
            // Subscribe to SimulationObject Kinematics
            var tankSimObj = GetComponent<TankSimulationObject>();
            tankSimObj.OnComputeKinematics -= ProcessInput;
        }

        public void BindInputSource(ITankInput inputSource)
        {
            m_tankInput = inputSource;
        }

        public void ProcessInput(float deltaTime)
        {
            var input = m_tankInput.GetInput();
            //if (DEBUG) Debug.Log($"GetInput called, received input: {input}");
            if (DEBUG)
            {
                if(playerState == PlayerState.Dashing)
                {
                    Debug.Log($"GetInput called, received input: {input}");
                }
            }
            ProcessInput(input, deltaTime);
        }
        
        private void ProcessInput(InputPayload input, float deltaTime)
        {
            if((CanDash && input.action == TankAction.Dash) || playerState == PlayerState.Dashing)
            {
                DashTank(dashVec, input.timestamp, deltaTime);
            }
            else
            {
                MoveTank(input.moveVector, deltaTime);
            }
            
            AimTank(input.aimVector, deltaTime);
        }

        private void MoveTank(Vector2 moveVector, float deltaTime)
        {
            var targetAngle = Vector2.SignedAngle(m_tankRB.transform.right, moveVector);
            float rotDeg = 0f;

            if (Mathf.Abs(targetAngle) >= deltaTime * m_rotationSpeed)
            {
                rotDeg = Mathf.Sign(targetAngle) * deltaTime * m_rotationSpeed;
            }
            else
            {
                // Si el angulo es demasiado pequeño entonces snapeamos a él (inferior a la mínima rotación por frame)
                rotDeg = targetAngle;
            }

            m_tankRB.MoveRotation(m_tankRB.rotation + rotDeg);
            m_turretRB.MoveRotation(-rotDeg);

            m_tankRB.MovePosition(m_tankRB.position + m_speed * moveVector * deltaTime);
        }

        private void DashTank(Vector2 moveVector, int currentInputDashTick, float deltaTime)
        {
            if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]: PlayerState : {playerState}");
            float currentAcceleration;
            if (playerState != PlayerState.Dashing)
            {
                dashVec = moveVector;
                stateInitTick = currentInputDashTick;
                playerState = PlayerState.Dashing;
                if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]Comienza el dash");
            }
            if(currentInputDashTick < stateInitTick + fullDashTicks)
            {
                currentAcceleration = accelerationMultiplier;
            }
            else
            {
                currentAcceleration = Mathf.Lerp(accelerationMultiplier, 1, (currentInputDashTick - (stateInitTick + fullDashTicks)) / ((stateInitTick + dashTicks)) - (stateInitTick + fullDashTicks)); 
            }

            if(moveVector != Vector2.zero)
            {
                m_tankRB.MovePosition(m_tankRB.position + m_speed * moveVector * deltaTime * currentAcceleration);
            }
            else
            {
                m_tankRB.MovePosition(m_tankRB.position + m_speed * (Vector2)transform.right * deltaTime * currentAcceleration);
            }

            if (DEBUG)
            {
                Debug.Log($"[{SimClock.TickCounter}] DASH: CurrentDashTick->{currentInputDashTick}. CurrentAcceleration->{currentAcceleration}. TickToEnd->{stateInitTick+dashTicks - currentInputDashTick}");
            }

            if (currentInputDashTick >= stateInitTick + dashTicks)
            {
                currentDashReloadTick = 0;
                OnDashEnd?.Invoke();
                playerState = PlayerState.Moving;
                stateInitTick = 0;
                if (DEBUG) Debug.Log("Se termina el dash");
            }           
        }

        private void AimTank(Vector2 aimVector, float deltaTime)
        {
            var targetAngle = Vector2.SignedAngle(m_turretRB.transform.right, aimVector);
            float rotDeg = 0f;

            if (Mathf.Abs(targetAngle) >= deltaTime * m_aimSpeed)
            {
                rotDeg = Mathf.Sign(targetAngle) * deltaTime * m_aimSpeed;
            }
            else
            {
                rotDeg = targetAngle;
            }

            // MoveRotation doesn't work because the turretRB is not simulated
            // (we only use it for the uniform interface with rotation angle around Z).

            m_turretRB.MoveRotation(m_turretRB.rotation + rotDeg);
        }

        private bool CheckCanDash()
        {
            if (currentDashReloadTick == -1) return true;
            else
            {
                if (currentDashReloadTick < reloadDashTicks)
                {
                    currentDashReloadTick++;
                }
                else
                {
                    currentDashReloadTick = -1;
                }
                return false;
            }
    }
    } 
}