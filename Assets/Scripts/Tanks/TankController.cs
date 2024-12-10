using System;
using System.Collections.Generic;
using Tankito.Netcode;
using Tankito.Netcode.Simulation;
using UnityEngine;
using Tankito.Utils;

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
        
        /// <summary>
        /// Parry collider, must be set on a physics layer to not interfere with tanks and so on.
        /// </summary>
        [SerializeField] private Collider2D m_parryHitbox;

        /// <summary>
        /// Used to avoid Simulation State dependance on tank controller actions
        /// (feedback loops in control functions drastically deteriorate prediciton performance)
        /// </summary>
        private Vector2 m_lastAimVector;
        
        /// <summary>
        /// <inheritdoc cref="m_lastAimVector"/>
        /// </summary>
        private Vector2 m_lastMoveVector;

        // Used to calculate dash movement *NOT Animation!
        [SerializeField] private AnimationCurve m_dashSpeedCurve;
        [SerializeField] private Animator m_hullAnimator, m_turretAnimator, m_fishAnimator;
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
        /// During these ticks, actions are initiated but don't execute their behaviour, to compensate for network action propagation delay.
        /// </summary>
        private int ACTION_LEAD_TICKS => Mathf.CeilToInt((float)(SimulationParameters.WORST_CASE_LATENCY * 2 / SimulationParameters.SIM_DELTA_TIME));
        private float ACTION_LEAD_SECONDS => (float)SimulationParameters.WORST_CASE_LATENCY * 2;
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
        private (PlayerState action, int timestamp) m_lastAnimTrigger;
        private void RecordAnimTrigger()
        {
            m_lastAnimTrigger.action = m_playerState;
            m_lastAnimTrigger.timestamp = SimClock.TickCounter;
        }

        // Mucho texto, lo siento, pero simplifica el resto del codigo, lo prometo xd

        const int FIRE_TICK_DURATION = 0;
        // NO se añade el ACTION_LEAD_TICKS aposta, porque queda muy raro que afecte a tu cooldown y no es intuitivo
        private int FireReloadTick { get => m_lastFireTick  + FIRE_TICK_DURATION + m_cannon.ReloadTicks; }
                                     //set => m_lastFireTick = value - m_cannon.ReloadTicks - FIRE_TICK_DURATION - ACTION_LEAD_TICKS; }
        private int DashReloadTick { get => m_lastDashTick + m_dashTicks + m_dashCooldownTicks; }
                                     //set => m_lastDashTick = value - m_dashCooldownTicks - m_dashTicks - ACTION_LEAD_TICKS; }
        private int ParryReloadTick { get => m_lastParryTick + m_parryTicks + m_parryCooldownTicks; }
                                      //set => m_lastParryTick = value - m_parryCooldownTicks - m_parryTicks - ACTION_LEAD_TICKS; }

        // El Getter de Last....Tick no esta expuesto porque no deberia usarse al hacer GetSimState, se usa TicksSince.... para poder usar ushorts
        // YA NO, (muy complicao ahora mismo no tengo neuronas suficientes para hacerlo bien)
        public int LastFireTick { get => m_lastFireTick; set => m_lastFireTick = value; }
        public int LastDashTick { get => m_lastDashTick; set => m_lastDashTick = value; }
        public int LastParryTick { get => m_lastParryTick; set => m_lastParryTick = value; }

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

            var animSpeedScale = 1/(float)ACTION_LEAD_SECONDS;
            m_hullAnimator.SetFloat("Speed Multiplier", animSpeedScale);
            m_turretAnimator.SetFloat("Speed Multiplier", animSpeedScale);
            //m_fishAnimator.SetFloat("Speed Multiplier", animSpeedScale);
        }

        void OnDisable()
        {
            // Unsubscribe to SimulationObject Kinematics
            var tankSimObj = GetComponent<TankSimulationObject>();
            tankSimObj.OnComputeKinematics -= ProcessInput;
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
            if (DEBUG_INPUT_CALLS) Debug.Log($"\tProcessing [{GetComponent<TankSimulationObject>().SimObjId}] input(PlayerState={m_playerState}): {input}");
            

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

            // Hack para joysticks 😅
            var aimVector = (input.aimVector.sqrMagnitude > 0.1) ? input.aimVector : m_lastAimVector;

            switch(m_playerState)
            {
                // Moving the tank must always precede aiming, because opposite aim rotation is applied whenever rotating while moving.
                case PlayerState.Moving:
                    MoveTank(input.moveVector, deltaTime);
                    AimTank(aimVector, deltaTime);
                    break;

                case PlayerState.Firing:
                    int fireTick = input.timestamp - m_lastFireTick - ACTION_LEAD_TICKS;

                    Debug.Log($"[{input.timestamp}] FireTick: ({fireTick}/{FIRE_TICK_DURATION})");

                    if (fireTick >= 0)
                    {
                        FireTank(aimVector, input.timestamp);
                    }
                    // ACTION LEAD TIME PHASE
                    else
                    {

                    }
                    
                    //MoveTank(input.moveVector, deltaTime);

                    if (fireTick >= FIRE_TICK_DURATION)
                    {
                        m_playerState = PlayerState.Moving;
                    }
                    break;

                case PlayerState.Dashing:
                    // Action lead ticks  must be taken into account when computing the action tick, since they do count as part of the action.
                    int dashTick =  input.timestamp - m_lastDashTick - ACTION_LEAD_TICKS;

                    // Only execute dash behaviour past action lead time
                    if (dashTick >= 0)
                    {
                        DashTank(input.moveVector, dashTick, deltaTime);
                    }
                    // ACTION LEAD TIME PHASE
                    else
                    {
                        MoveTank(input.moveVector, deltaTime);
                    }

                    AimTank(aimVector, deltaTime);

                    if (dashTick >= m_dashTicks)
                    {
                        m_playerState = PlayerState.Moving;
                    }
                    break;

                case PlayerState.Parrying:
                    int parryTick = input.timestamp - m_lastParryTick - ACTION_LEAD_TICKS;
                    
                    // Only execute dash behaviour past action lead time
                    if (parryTick >= 0)
                    {
                        ParryTank(parryTick);
                    }
                    // ACTION LEAD TIME PHASE
                    else
                    {

                    }
                    
                    AimTank(input.moveVector, deltaTime);

                    if (parryTick >= m_parryTicks)
                    {
                        m_playerState = PlayerState.Moving;
                    }
                    break;
            }

            // Action Animations Logic Handling 
            SetActionAnimation();

            // Only execute parry behaviour past action lead time
            m_parryHitbox.enabled = (m_playerState == PlayerState.Parrying) &&
                                    (input.timestamp >= (m_lastParryTick + ACTION_LEAD_TICKS));
        }

        private void SetActionAnimation()
        {
            var currentState = m_playerState;
            if (currentState == PlayerState.Moving) return;
            int lastActionTick =
                    currentState == PlayerState.Firing ? m_lastFireTick
                    : currentState == PlayerState.Dashing ? m_lastDashTick
                    : currentState == PlayerState.Parrying ? m_lastParryTick
                    : throw new ArgumentOutOfRangeException();

            if (m_lastAnimTrigger.action != currentState || m_lastAnimTrigger.timestamp <= lastActionTick)
            {
                if (SimClock.Instance.Active)
                {
                    int actionTick = SimClock.TickCounter - lastActionTick - ACTION_LEAD_TICKS;
                    float normalizedLeadTime = (actionTick + ACTION_LEAD_TICKS)/(float)ACTION_LEAD_TICKS;
                    var turretAnimationState = m_turretAnimator.GetCurrentAnimatorStateInfo(0);
                    var hullAnimationState = m_hullAnimator.GetCurrentAnimatorStateInfo(0);

                    string actionAnimTrigger = 
                            currentState == PlayerState.Firing ? "Shoot"
                            : currentState == PlayerState.Dashing ? "Dash"
                            : currentState == PlayerState.Parrying ? "Parry"
                            : "";
                    string actionAnimState =
                            currentState == PlayerState.Firing ? "Shoot"
                            : currentState == PlayerState.Dashing ? "Dash"
                            : currentState == PlayerState.Parrying ? "Parry"
                            : "";

                    Debug.Log($"[{SimClock.TickCounter}] CurrentAnimationStates: \n" +
                        $"Is turretState name '{actionAnimState + " Turret"}':{turretAnimationState.IsName(actionAnimState + " Turret")}\n" +
                        $"Is hullState name '{actionAnimState + " Hull"}':{hullAnimationState.IsName(actionAnimState + " Hull")}\n" + 
                        $"normalizedLeadTime: {normalizedLeadTime} animatorNormalizedTime:(turret-{turretAnimationState.normalizedTime} hull-{hullAnimationState.normalizedTime})");

                    if (!turretAnimationState.IsName(actionAnimState + " Turret") && !m_turretAnimator.GetBool(actionAnimTrigger) ||
                        !hullAnimationState.IsName(actionAnimState + " Hull") && !m_hullAnimator.GetBool(actionAnimTrigger))
                    {
                        m_turretAnimator.ResetAllTriggers();
                        m_hullAnimator.ResetAllTriggers();
                        m_turretAnimator.SetBool(actionAnimTrigger, true);
                        m_hullAnimator.SetBool(actionAnimTrigger, true);
                        RecordAnimTrigger();
                    }
                    // JSAJSAJSAJAAAAAAAAAA APPROXIMATELY 🤣😂😁😀
                    else if (!Mathf.Approximately(turretAnimationState.normalizedTime, normalizedLeadTime) ||
                            !Mathf.Approximately(hullAnimationState.normalizedTime, normalizedLeadTime))
                    {
                        m_turretAnimator.Play(turretAnimationState.shortNameHash, -1, normalizedLeadTime);
                        m_hullAnimator.Play(hullAnimationState.shortNameHash, -1, normalizedLeadTime);
                        RecordAnimTrigger();
                    }
                }
            }
        }

        private void ParryTank(int parryTick)
        {
            if (DEBUG_PARRY) Debug.LogWarning($"[{SimClock.TickCounter}] Parry progressTicks( {parryTick}/{m_parryTicks} )");
        }

        private void FireTank(Vector2 aimVector, int inputTick)
        {
            if (DEBUG_FIRE) Debug.Log($"[{SimClock.TickCounter}] FireTank({GetComponent<TankSimulationObject>().SimObjId}) called.");

            m_cannon.Shoot(m_turretRB.position, m_turretRB.transform.right, inputTick);
        }

        private void MoveTank(Vector2 moveVector, float deltaTime)
        {
            if (moveVector.sqrMagnitude < 0.05) return;

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

            //var turrRot = m_turretRB.rotation;
            m_tankRB.MoveRotation(m_tankRB.rotation + rotDeg);
            m_turretRB.MoveRotation(m_turretRB.rotation);

            m_tankRB.MovePosition(m_tankRB.position + m_speed * moveVector * deltaTime);

            //if (moveVector.sqrMagnitude > 0.1) m_lastMoveVector = moveVector; // Queda un poco raro para cuando intentas dashear luego
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
                if (dashTick == 0)
                {
                    Debug.Log($"[{SimClock.TickCounter}]Comienza el dash");
                }
                Debug.Log($"[{SimClock.TickCounter}] DASH: progressTicks( {dashTick}/{m_dashTicks} )");
            }


            float dashSpeed = m_dashSpeedMultiplier * m_dashSpeedCurve.Evaluate((float)dashTick/m_dashTicks);

            if(moveVector.sqrMagnitude > 0.1f)
            {
                m_tankRB.MovePosition(m_tankRB.position + moveVector * deltaTime * dashSpeed);
            }
            else
            {
                //m_tankRB.MovePosition(m_tankRB.position + m_lastMoveVector * deltaTime * dashSpeed); // Queda raro el dash asi en teclado
                m_tankRB.MovePosition(m_tankRB.position + (Vector2)m_tankRB.transform.right * deltaTime * dashSpeed);
            }
        }

        private void AimTank(Vector2 aimVector, float deltaTime)
        {
            // this fucking sucks because of simulation state feedback, we need to change it so it's *less dependant on previous state 
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

            if (aimVector.sqrMagnitude > 0.1) m_lastAimVector = aimVector;
        }

        #region Modifiers & TankData

        #endregion
    } 
}