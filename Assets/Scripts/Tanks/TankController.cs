using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tankito.Netcode;
using Tankito.Netcode.Simulation;
using UnityEngine;
using UnityEngine.Windows;

namespace Tankito
{
    // realmente esto necesita un nombre mejor
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
                parryTime: 0.2f,
                parryCooldown: 1.5f,
                dashSpeed: 4f,
                dashDistance: 1.5f,
                dashCooldown: 2f
            );
        private List<HullModifier> m_modifiers = new List<HullModifier>();
        public List<HullModifier> Modifiers => m_modifiers;

        [SerializeField] private Rigidbody2D m_tankRB;
        [SerializeField] private Rigidbody2D m_turretRB;
        [SerializeField] private float m_speed;
        [SerializeField] private float m_rotationSpeed;
        [SerializeField] private float m_aimSpeed = 900f;

        // Used to calculate dash movement *NOT Animation!
        [SerializeField] private AnimationCurve m_dashSpeedCurve;
        [SerializeField] private Animator m_hullAnimator, m_turretAnimator;
        private float m_dashSpeedMultiplier;
        private float m_dashDistance;
        int m_dashTicks;
        int m_dashCooldownTicks;
        int m_parryTicks;
        int m_parryCooldownTicks;

        [SerializeField] private PlayerState m_playerState = PlayerState.Moving;

        [SerializeField] private ITankInput m_tankInput;
        [SerializeField] private bool DEBUG_INPUT_CALLS = false;
        [SerializeField] private bool DEBUG_DASH = false;
        [SerializeField] private bool DEBUG_PARRY = false;
        [SerializeField] private bool DEBUG_FIRE = false;

        public PlayerState PlayerState { get => m_playerState; set => m_playerState = value; }

        [SerializeField] private BulletCannon m_cannon;
        /// <summary>
        /// Tick when the fire action was last triggered.
        /// </summary>
        private int m_lastFireTick;
        /// <summary>
        /// Tick when the dash action was last triggered.
        /// </summary>
        private int m_lastDashTick;
        /// <summary>
        /// Tick when the parry action was last triggered.
        /// </summary>
        private int m_lastParryTick;


        // Mucho texto, lo siento, pero simplifica el resto del codigo, lo prometo xd

        private int FireReloadTick { get => m_lastFireTick + m_cannon.ReloadTicks;
                                     set => m_lastFireTick = value - m_cannon.ReloadTicks; }
        private int DashReloadTick { get => m_lastDashTick + m_dashCooldownTicks;
                                     set => m_lastDashTick = value - m_dashCooldownTicks; }
        private int ParryReloadTick { get => m_lastParryTick + m_parryCooldownTicks;
                                      set => m_lastParryTick = value - m_parryCooldownTicks; }
        // El Getter de Last....Tick no esta expuesto porque no deberia usarse al hacer GetSimState, se usa TicksSince.... para poder usar ushorts
        public int LastFireTick { get => m_lastFireTick; set => m_lastFireTick = value; }
        public int LastDashTick { get => m_lastDashTick; set => m_lastDashTick = value; }
        public int LastParryTick { get => m_lastParryTick; set => m_lastParryTick = value; }

        /// <summary>
        /// NOT SAFE FOR INTERNAL USE, because using tick measurements against <see cref="SimClock.TickCounter"/> is not <see cref="ClientSimulationManager.Rollback"/> compatible.
        /// </summary>
        public ushort TicksSinceFire { get => (ushort)(SimClock.TickCounter - m_lastFireTick); }
        /// <inheritdoc cref="TicksSinceFire"/>
        public ushort TicksSinceDash { get => (ushort)(SimClock.TickCounter - m_lastDashTick); }
        /// <inheritdoc cref="TicksSinceFire"/>
        public ushort TicksSinceParry { get => (ushort)(SimClock.TickCounter - m_lastParryTick); }

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
            SetParryTicks(mod.extraParryTime, mod.parryCooldownTimeAdded, reset);
            SetDashParams(mod.dashDistanceMultiplier, mod.dashSpeedMultiplier, mod.dashCooldownTimeAdded, reset);
        }

        private void SetParryTicks(float parryDuration, float parryCooldown, bool overwrite)
        {
            if (overwrite)
            {
                m_parryTicks = Mathf.CeilToInt(parryDuration / SimClock.SimDeltaTime);
                m_parryCooldownTicks = Mathf.CeilToInt(parryCooldown / SimClock.SimDeltaTime);
            }
            else
            {
                m_parryTicks += Mathf.CeilToInt(parryDuration / SimClock.SimDeltaTime);
                m_parryCooldownTicks += Mathf.CeilToInt(parryCooldown / SimClock.SimDeltaTime);
            }
        }

        private void SetDashParams(float dashDistance, float dashSpeed, float dashCooldown, bool overwrite)
        {
            if (overwrite)
            {
                m_dashSpeedMultiplier = dashSpeed;
                m_dashDistance = dashDistance;
                m_dashCooldownTicks = Mathf.CeilToInt(dashCooldown / SimClock.SimDeltaTime);
            }
            else
            {
                m_dashSpeedMultiplier *= dashSpeed;
                m_dashDistance += dashDistance;
                m_dashCooldownTicks += Mathf.CeilToInt(dashCooldown / SimClock.SimDeltaTime);
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
            //if (DEBUG_INPUT_CALLS) Debug.Log($"GetInput called, received input: {input}");
            ProcessInput(input, deltaTime);
        }
        
        private void ProcessInput(InputPayload input, float deltaTime)
        {
            if (DEBUG_INPUT_CALLS) Debug.Log($"Processing {gameObject} input(PlayerState={m_playerState}): {input}");
            

            //if (DEBUG_INPUT_CALLS) Debug.Log($"[{SimClock.TickCounter}] Input action: {input.action} | Reloads ticks: Dash({DashReloadTick}) Parry({ParryReloadTick}) Fire({FireReloadTick})");
            switch (input.action)
            {

                case TankAction.None:
                    if (m_playerState != PlayerState.Dashing && m_playerState != PlayerState.Parrying && m_playerState != PlayerState.Firing)
                    m_playerState = PlayerState.Moving;
                    break;

                case TankAction.Parry:
                    if (input.timestamp >= ParryReloadTick && m_playerState != PlayerState.Dashing && m_playerState != PlayerState.Firing)
                    {
                        m_playerState = PlayerState.Parrying;
                        m_lastParryTick = input.timestamp;
                    }
                    break;

                case TankAction.Dash:
                    if (input.timestamp >= DashReloadTick && m_playerState != PlayerState.Firing && m_playerState != PlayerState.Parrying)
                    {
                        m_playerState = PlayerState.Dashing;
                        m_lastDashTick = input.timestamp;
                    }
                    break;

                case TankAction.Fire:
                    if (input.timestamp >= FireReloadTick && m_playerState != PlayerState.Dashing && m_playerState != PlayerState.Parrying)
                    {
                        m_playerState = PlayerState.Firing;
                        m_lastFireTick = input.timestamp;
                    }
                    break;

                default:
                    break;
            }

            switch(m_playerState)
            {
                // Moving the tank must always precede aiming, because opposite aim rotation is applied whenever rotating while moving.
                case PlayerState.Moving:
                    MoveTank(input.moveVector, deltaTime);
                    AimTank(input.aimVector, deltaTime);
                    break;

                case PlayerState.Firing:
                    MoveTank(input.moveVector, deltaTime);
                    AimTank(input.aimVector, deltaTime);

                    // Hack para joysticks 😅
                    var aimVector = (input.aimVector.magnitude>0.1) ? input.aimVector : (Vector2)m_turretRB.transform.right;
                    FireTank(aimVector, input.timestamp);

                    // Reset state to movement for next tick (firing is considered to only take 0 ticks right now)
                    m_playerState = PlayerState.Moving;
                    break;

                case PlayerState.Dashing:
                    int dashTick =  input.timestamp - m_lastDashTick;
                    DashTank(input.moveVector, dashTick, deltaTime);
                    AimTank(input.aimVector, deltaTime);

                    if (dashTick >= m_dashTicks)
                    {
                        m_playerState = PlayerState.Moving;
                    }
                    break;

                case PlayerState.Parrying:
                    int parryTick = input.timestamp - m_lastParryTick;
                    MoveTank(input.moveVector, deltaTime);
                    ParryTank(parryTick);

                    if (parryTick >= m_parryTicks)
                    {
                        m_playerState = PlayerState.Moving;
                    }
                    break;
            }
        }

        private void ParryTank(int parryTick)
        {
            if (DEBUG_PARRY) Debug.LogWarning($"TODO: Implement Parry. progressTicks( {parryTick}/{m_parryTicks} )");

            // Only trigger Parry Animations during the first parry tick, and only if NOT rolling back
            if (parryTick == 0 && SimClock.Instance.Active)
            {
                m_turretAnimator.SetTrigger("Parry");
                m_hullAnimator.SetTrigger("Parry");
            }
        }

        private void FireTank(Vector2 aimVector, int inputTick)
        {
            if (DEBUG_FIRE) Debug.Log($"[{SimClock.TickCounter}] FireTank({GetComponent<TankSimulationObject>().SimObjId}) called.");

            m_cannon.Shoot(m_turretRB.position, aimVector , inputTick);
            
            m_lastFireTick = inputTick;
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

        /// <summary>
        /// <paramref name="dashTick"/> is the tick marking the progress of the dash (will be 0 the first tick the dash is being performed).
        /// </summary>
        /// <param name="moveVector"></param>
        /// <param name="deltaTime"></param>
        /// <param name="dashTick"></param>
        private void DashTank(Vector2 moveVector, int dashTick, float deltaTime)
        {
            if (DEBUG_DASH)
            {
                Debug.Log($"[{SimClock.TickCounter}] DASH: progressTicks( {dashTick}/{m_dashTicks} )");
            }

            if (dashTick == 0)
            {
                m_playerState = PlayerState.Dashing;
                if (DEBUG_DASH) Debug.Log($"[{SimClock.TickCounter}]Comienza el dash");
            }

            float dashSpeed = m_dashSpeedMultiplier * m_dashSpeedCurve.Evaluate((float)dashTick/m_dashTicks);

            if(moveVector != Vector2.zero)
            {
                // CONSIDER REMOVAL TO IMPROVE PREDICTABILITY ?
                m_tankRB.MovePosition(m_tankRB.position + moveVector * deltaTime * dashSpeed);
            }
            else
            {
                m_tankRB.MovePosition(m_tankRB.position + (Vector2)transform.right * deltaTime * dashSpeed);
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

        #region Modifiers & TankData

        #endregion
    } 
}