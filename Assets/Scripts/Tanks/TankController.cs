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
        public bool postDash;
        public Vector2 postDashInput;
        [SerializeField] private float dashReloadDuration = 1;
        [SerializeField] public Vector2 inputWhileDash;

        [SerializeField] private PlayerState playerState = PlayerState.Moving;
        public PlayerState PlayerState { get => playerState; }
        [SerializeField] public bool canDash = true;

        public ITankInput TankInputComponent { get => m_tankInput; set { if (m_tankInput==null) m_tankInput = value; else Debug.LogWarning($"TankInputComponent for {this} was already set!");} }
        [SerializeField] private ITankInput m_tankInput;
        [SerializeField] private bool DEBUG = false;

        [SerializeField] private CreateBullet cannon;
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
            if (DEBUG) Debug.Log($"GetInput called, received input: {input}");
            ProcessInput(input, deltaTime);
        }
        
        private void ProcessInput(InputPayload input, float deltaTime)
        {
            if (DEBUG) Debug.Log($"Processing {gameObject} input: {input}");
            switch (input.action)
            {
                case TankAction.None:
                    MoveTank(input.moveVector, deltaTime);
                    break;
                case TankAction.Dash:
                    DashTank(input.moveVector, deltaTime);
                    break;            
                case TankAction.Parry:

                    MoveTank(input.moveVector, deltaTime);
                    break;
                case TankAction.Fire:
                    FireTank(input.aimVector, deltaTime);
                    MoveTank(input.moveVector, deltaTime);
                    break;
                default:
                    MoveTank(input.moveVector, deltaTime);
                    break;
            }
            
            //if (input.action != TankAction.Dash) // No puedes hacer esto asi, si vas a tener una variable de can dash la tienes que usar aqui, NO cuando estas RECOGIENDO inputs
            //{
            //    MoveTank(input.moveVector, deltaTime);
            //}
            //else
            //{
            //    DashTank(input.moveVector, deltaTime);
            //}
            AimTank(input.aimVector, deltaTime);
        }
        private void FireTank(Vector2 aimVector, float deltaTime)
        {
            cannon.Shoot();
        }
        private void MoveTank(Vector2 movementVector, float deltaTime)
        {
            var targetAngle = Vector2.SignedAngle(m_tankRB.transform.right, movementVector);
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

            m_tankRB.MovePosition(m_tankRB.position + m_speed * movementVector * deltaTime);
        }

        private void DashTank(Vector2 movementVector, float deltaTime)
        {
            float dashTicks = dashDuration / deltaTime;
            float fullDashTicks = fullDashDuration / deltaTime;
            float currentAcceleration;

            if (currentDashTick < fullDashTicks)
            {
                currentAcceleration = accelerationMultiplier;
                if (currentDashTick == 0)
                {
                    playerState = PlayerState.Dashing;
                    inputWhileDash = movementVector; // No entiendo muy bien para que sirve esto, sinceramente
                    canDash = false;
                }
            }
            else
            {
                currentAcceleration = Mathf.Lerp(accelerationMultiplier, 1, (currentDashTick - fullDashTicks) / (dashTicks - fullDashTicks)); ;
            }
            if (movementVector != Vector2.zero)
            {
                m_tankRB.MovePosition(m_tankRB.position + movementVector * deltaTime * m_speed * currentAcceleration);
            }
            else
            {
                m_tankRB.MovePosition(m_tankRB.position + (Vector2)transform.right * deltaTime * m_speed * currentAcceleration);
            }

            if (currentDashTick >= dashTicks)
            {
                currentDashTick = 0;

                postDash = true;
                postDashInput = inputWhileDash;
                inputWhileDash = Vector2.zero;

                playerState = PlayerState.Moving;
                StartCoroutine("DashReloading");
                return;
            }
            currentDashTick++;
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