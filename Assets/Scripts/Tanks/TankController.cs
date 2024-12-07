using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
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
        Parrying,
        Firing
    }

    public class TankController : MonoBehaviour
    {
        public static readonly HullStatsModifier BaseTankStats = 
            new HullStatsModifier(
                speed: 3f,
                rotSpeed: 360f,
                health: 2,
                parryTime: 0.15f,
                parryCooldown: 1.5f,
                dashSpeed: 3f,
                dashDistance: 0.5f,
                dashCooldown: 2f
            );
        private List<HullModifier> m_modifiers = new List<HullModifier>();
        public List<HullModifier> Modifiers => m_modifiers;

        [SerializeField] private Rigidbody2D m_tankRB;
        [SerializeField] private Rigidbody2D m_turretRB;
        [Tooltip("How fast the turret can turn to aim in the specified direction.")]
        [SerializeField] private float m_speed;
        [SerializeField] private float m_rotationSpeed;
        [SerializeField] private float m_aimSpeed = 900f;

        //Variables Dash
        [SerializeField] private AnimationCurve m_dashSpeedCurve;
        private float m_dashSpeedMultiplier = 1f;
        private float m_dashDistance;

        private int currentDashReloadTick = CAN_DASH;
        int m_dashTicks;
        //int fullDashTicks;
        int m_reloadDashTicks;
        [SerializeField] int stateInitTick;
        const int CAN_DASH = -1;


        [SerializeField] private PlayerState playerState = PlayerState.Moving;

        public ITankInput TankInputComponent { get => m_tankInput; set { if (m_tankInput==null) m_tankInput = value; else Debug.LogWarning($"TankInputComponent for {this} was already set!");} }
        [SerializeField] private ITankInput m_tankInput;
        [SerializeField] private bool DEBUGCONT = false;
        [SerializeField] private bool DEBUGDASh = false;

        private Vector2 dashVec;

        public delegate void DashEnd();
        public event DashEnd OnDashEnd;

        [SerializeField] private BulletCannon cannon;

        public PlayerState PlayerState { get => playerState; set => playerState = value; }
        public int StateInitTick { get => stateInitTick; set => stateInitTick = value; }
        private bool CanDash { get => CheckCanDash() && playerState != PlayerState.Parrying && playerState != PlayerState.Firing; }

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

            //ApplyModifierList();
            stateInitTick = 0;
        }

        void OnEnable()
        {
            stateInitTick = 0;
            // Subscribe to SimulationObject Kinematics
            var tankSimObj = GetComponent<TankSimulationObject>();
            tankSimObj.OnComputeKinematics += ProcessInput;

            //Subscribe to Round Countdown Start
            //RoundManager.Instance.OnPreRoundStart += ApplyModifierList;
        }

        void OnDisable()
        {
            // Unsubscribe to SimulationObject Kinematics
            var tankSimObj = GetComponent<TankSimulationObject>();
            tankSimObj.OnComputeKinematics -= ProcessInput;
            
            //Unsubscribe to Round Countdown Start
            //RoundManager.Instance.OnPreRoundStart -= ApplyModifierList;
        }

        public void ApplyModifierList(int nRound = 0)
        {
            ApplyModifier(BaseTankStats, true);
            foreach(var mod in m_modifiers)
            {
                ApplyModifier(mod.hullStatsModifier, false);
            }
        }

        void ApplyModifier(HullStatsModifier mod, bool reset = false)
        {
            //Debug.Log("tanque aplicando mods");
            if (reset)
            {
                m_speed = mod.speedMultiplier;
                m_rotationSpeed = mod.rotationSpeedMultiplier;
                GetComponent<TankData>().SetHealth(mod.extraHealth);
            }
            else
            {
                m_speed *= mod.speedMultiplier;
                m_rotationSpeed *= mod.rotationSpeedMultiplier;
                GetComponent<TankData>().AddHealth(mod.extraHealth);
            }
            SetParryTicksFromSeconds(mod.extraParryTime, mod.parryCooldownTimeAdded, reset);
            SetDashParams(mod.dashDistanceMultiplier, mod.dashSpeedMultiplier, mod.dashCooldownTimeAdded, reset);
        }

        private void SetParryTicksFromSeconds(float parryDuration, float parryCooldown, bool overwrite)
        {
            if (overwrite)
            {

            }
            else
            {

            }
        }

        private void SetDashParams(float dashDistance, float dashSpeed, float dashCooldown, bool overwrite)
        {
            if (overwrite)
            {
                m_dashSpeedMultiplier = dashSpeed;
                m_dashDistance = dashDistance;
                m_reloadDashTicks = Mathf.CeilToInt(dashCooldown / SimClock.SimDeltaTime);
            }
            else
            {
                m_dashSpeedMultiplier *= dashSpeed;
                m_dashDistance += dashDistance;
                m_reloadDashTicks += Mathf.CeilToInt(dashCooldown / SimClock.SimDeltaTime);
            }

            m_dashTicks = Mathf.CeilToInt((m_dashDistance/m_dashSpeedMultiplier) / SimClock.SimDeltaTime);
        }

        public void BindInputSource(ITankInput inputSource)
        {
            m_tankInput = inputSource;
        }

        public void ProcessInput(float deltaTime)
        {
            var input = m_tankInput.GetInput();
            if (DEBUGCONT) Debug.Log($"GetInput called, received input: {input}");
            ProcessInput(input, deltaTime);
        }
        
        private void FireTank(Vector2 aimVector, float deltaTime)
        {
            // TO DO : TIPO DE BALA 
            if (SimClock.Instance.Active)
                MusicManager.Instance.PlayDisparo(); //////////////////////////////////////////////////////////
            cannon.Shoot(aimVector);
        }
        
        private void ProcessInput(InputPayload input, float deltaTime)
        {
            
            if (DEBUGCONT) Debug.Log($"Processing {gameObject} input: {input}");
            if((CanDash && input.action == TankAction.Dash) || playerState == PlayerState.Dashing)
            {
                DashTank(dashVec, input.timestamp, deltaTime);
            }
            else
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
            if (DEBUGDASh) Debug.Log($"[{SimClock.TickCounter}]: PlayerState : {playerState}, VelocidadDash: {m_dashSpeedMultiplier}");
            
            if (playerState != PlayerState.Dashing)
            {
                dashVec = moveVector;
                stateInitTick = currentInputDashTick;
                playerState = PlayerState.Dashing;
                if (DEBUGDASh) Debug.Log($"[{SimClock.TickCounter}]Comienza el dash");
                if(SimClock.Instance.Active)
                    MusicManager.Instance.PlaySound("snd_dash"); //////////////////////////////////////////////////////////
            }

            //currentAcceleration = Mathf.Lerp(accelerationMultiplier, 0, (currentInputDashTick - (stateInitTick + fullDashTicks)) / (stateInitTick + dashTicks) - (stateInitTick + fullDashTicks));
            if (DEBUGDASh) Debug.Log($"[{SimClock.TickCounter}]: m_dashSpeedMultiplier: {m_dashSpeedMultiplier}");
            if (DEBUGDASh) Debug.Log($"[{SimClock.TickCounter}]: parámetros dash {currentInputDashTick}, {stateInitTick}, {m_dashTicks}");
            if (DEBUGDASh) Debug.Log($"[{SimClock.TickCounter}]: curve value: {m_dashSpeedCurve.Evaluate((float)(currentInputDashTick - stateInitTick) / m_dashTicks)}");
            float dashSpeed = m_speed * m_dashSpeedMultiplier * m_dashSpeedCurve.Evaluate((float)(currentInputDashTick-stateInitTick)/m_dashTicks);

            if(moveVector != Vector2.zero)
            {
                m_tankRB.MovePosition(m_tankRB.position + moveVector * deltaTime * dashSpeed);
            }
            else
            {
                m_tankRB.MovePosition(m_tankRB.position + (Vector2)transform.right * deltaTime * dashSpeed);
            }

            if (DEBUGDASh)
            {
                Debug.Log($"[{SimClock.TickCounter}] DASH: CurrentDashTick->{currentInputDashTick}. CurrentSpeedMult->{dashSpeed}. TickToEnd->{stateInitTick+m_dashTicks - currentInputDashTick}");
            }

            if (currentInputDashTick >= stateInitTick + m_dashTicks)
            {
                currentDashReloadTick = 0;
                OnDashEnd?.Invoke();
                playerState = PlayerState.Moving;
                stateInitTick = 0;
                if (DEBUGDASh) Debug.Log("Se termina el dash");
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
            if (currentDashReloadTick == CAN_DASH) return true;
            else
            {
                if (currentDashReloadTick < m_reloadDashTicks)
                {
                    if (SimClock.Instance.Active)   //Este check es para que no se reduzca el cooldown en caso de que se este reconciliando
                    {
                        currentDashReloadTick++;
                    } 
                }
                else
                {
                    currentDashReloadTick = CAN_DASH;
                }
                return false;
            }
        }

        #region Modifiers & TankData

        #endregion
    } 
}