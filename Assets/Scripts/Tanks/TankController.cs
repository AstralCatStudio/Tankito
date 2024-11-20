using System.Collections;
using Tankito.Netcode;
using Tankito.Netcode.Simulation;
using UnityEngine;

namespace Tankito
{
    public enum PlayerState
    {
        Moving,
        Dashing
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
        [SerializeField] private float currentDashTick = 0;
        public Vector2 postDashInput;
        [SerializeField] private float dashReloadDuration = 1;
        private int currentDashReloadTick = 0;
        float dashTicks;
        float fullDashTicks;
        float reloadDashTicks;

        [SerializeField] private PlayerState playerState = PlayerState.Moving;
        public PlayerState PlayerState { get => playerState; }
        [SerializeField] public bool canDash = true;

        public ITankInput TankInputComponent { get => m_tankInput; set { if (m_tankInput==null) m_tankInput = value; else Debug.LogWarning($"TankInputComponent for {this} was already set!");} }
        [SerializeField] private ITankInput m_tankInput;
        [SerializeField] private bool DEBUG = false;
        private InputPayload dashInput;

        public delegate void DashEnd();
        public event DashEnd OnDashEnd;

        [SerializeField] private BulletCannon cannon;
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
            dashTicks = dashDuration / SimClock.SimDeltaTime;
            fullDashTicks = fullDashDuration / SimClock.SimDeltaTime;
            reloadDashTicks = dashReloadDuration / SimClock.SimDeltaTime;
        }

        void OnEnable()
        {
            // Subscribe to SimulationObject Kinematics
            var tankSimObj = GetComponent<TankSimulationObject>();
            tankSimObj.OnComputeKinematics += ProcessInput;
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
            if (DEBUG) Debug.Log($"GetInput called, received input: {input}");
            if(playerState == PlayerState.Dashing)
            {
                input.moveVector = dashInput.moveVector;
               //nput.aimVector = m_lastInput.aimVector;
                if (DEBUG) Debug.Log("GetInput:" + input);
            }
            
            ProcessInput(input, deltaTime);
           //_lastInput = input;
        }
        
        
            
            
            //if (input.action != TankAction.Dash) // No puedes hacer esto asi, si vas a tener una variable de can dash la tienes que usar aqui, NO cuando estas RECOGIENDO inputs
            //{
            //    MoveTank(input.moveVector, deltaTime);
            //}
            //else
            //{
            //    DashTank(input.moveVector, deltaTime);
            //}
        private void FireTank(Vector2 aimVector, float deltaTime)
        {
            cannon.Shoot();
        }
            private void ProcessInput(InputPayload input, float deltaTime)
        {
            switch (input.action)
            {
                case TankAction.None:

                    break;
                case TankAction.Dash:
                    break;            
                case TankAction.Parry:

                    break;
                case TankAction.Fire:
                    FireTank(input.aimVector, deltaTime);
                    break;
                default:
                    break;
            }
            if (DEBUG) Debug.Log($"Processing {gameObject} input: {input}");
            MoveTank(input, deltaTime);
            AimTank(input.aimVector, deltaTime);
        }

        private void MoveTank(InputPayload input, float deltaTime)
        {
            var targetAngle = Vector2.SignedAngle(m_tankRB.transform.right, input.moveVector);
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

            float currentAcceleration = 1;
            if ((input.action == TankAction.Dash && canDash) || playerState == PlayerState.Dashing)
            {
                if (currentDashTick < fullDashTicks)
                {
                    currentAcceleration = accelerationMultiplier;
                    if (currentDashTick == 0)
                    {
                        if (DEBUG) Debug.Log("Comienza el dash");
                        playerState = PlayerState.Dashing;
                    }
                }
                else
                {
                    currentAcceleration = Mathf.Lerp(accelerationMultiplier, 1, (currentDashTick - fullDashTicks) / (dashTicks - fullDashTicks)); ;
                }

                if (currentDashTick >= dashTicks)
                {
                    currentDashTick = 0;

                    canDash = false;
                    OnDashEnd?.Invoke();
                    playerState = PlayerState.Moving;
                    if (DEBUG) Debug.Log("Se termina el dash");
                }
                else
                {
                    currentDashTick++;
                }
            }
            else if (!canDash)
            {
                if(currentDashReloadTick < reloadDashTicks)
                {
                    currentDashReloadTick++;
                }
                else
                {
                    currentDashReloadTick = 0;
                    canDash = true;
                }
            }

            m_tankRB.MovePosition(m_tankRB.position + m_speed * input.moveVector * deltaTime * currentAcceleration);
        }

        private void DashTank(Vector2 movementVector, float deltaTime)
        {
            

            
            
        }
        
        IEnumerator DashReloading()
        {
            yield return new WaitForSeconds(dashReloadDuration);
            canDash = true;
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
    }
}