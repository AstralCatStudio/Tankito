using System.Collections;
using System.Collections.Generic;
using Tankito;
using Tankito.Netcode;
using UnityEngine;

public enum PlayerState
{
    Moving,
    Dashing,
    Parrying,
    Firing
}

public abstract class ATankController : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D m_tankRB;
    [SerializeField] protected Rigidbody2D m_turretRB;
    [Tooltip("How fast the turret can turn to aim in the specified direction.")]
    [SerializeField] protected float m_speed;
    [SerializeField] protected float m_rotationSpeed;
    [SerializeField] protected float m_aimSpeed = 900f;

    //Variables Dash
    [SerializeField] protected AnimationCurve m_dashSpeedCurve;
    [SerializeField] protected float m_dashSpeedMultiplier = 1f;
    [SerializeField] protected float m_dashDistance;

    protected int currentDashReloadTick = CAN_DASH;
    protected int m_dashTicks;
    //int fullDashTicks;
    [SerializeField] protected int m_reloadDashTicks;
    [SerializeField] protected int stateInitTick;
    protected const int CAN_DASH = -1;

    [SerializeField] protected ITankInput m_tankInput;

    [SerializeField] protected PlayerState playerState = PlayerState.Moving;
    [SerializeField] GameObject parryHitbox;
    public ITankInput TankInputComponent { get => m_tankInput; set { if (m_tankInput == null) m_tankInput = value; else Debug.LogWarning($"TankInputComponent for {this} was already set!"); } }

    [SerializeField] protected IBulletCannon cannon;

    public PlayerState PlayerState { get => playerState; set => playerState = value; }
    public int StateInitTick { get => stateInitTick; set => stateInitTick = value; }
    protected bool CanDash { get => CheckCanDash() && playerState != PlayerState.Parrying && playerState != PlayerState.Firing; }
    protected bool CanParry { get => CheckCanDash() && playerState != PlayerState.Dashing && playerState != PlayerState.Firing; }
    protected Vector2 dashVec;

    [SerializeField] protected bool DEBUGCONT = false;
    [SerializeField] protected bool DEBUGDASh = false;


    [SerializeField]
    private Animator m_turretAnimator, m_hullAnimator;
    [SerializeField]
    float parryTotalTime, parryStart, parryWindow;
    float parryTimer =0;

    protected virtual void Start()
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

        Transform cannonTransform = transform.GetChild(1).GetChild(0).GetChild(0);
        if(cannonTransform != null)
        {
            cannon = cannonTransform.gameObject.GetComponent<IBulletCannon>();
            if(cannon == null)
            {
                Debug.LogWarning("Error cannon doesnt have IBulletCannon component");
            }
        }
        else
        {
            Debug.LogWarning("Error getting cannon gameObject");
        }

        //ApplyModifierList();
        stateInitTick = 0;
    }

    protected void ProcessInput(InputPayload input, float deltaTime)
    {

        if (DEBUGCONT) Debug.Log($"Processing {gameObject} input: {input}");
        if ((CanDash && input.action == TankAction.Dash) || playerState == PlayerState.Dashing)
        {
            DashTank(input.moveVector, input.timestamp, deltaTime);
        }
        else
        if ((CanParry && input.action == TankAction.Parry) || playerState == PlayerState.Parrying)
        {
            Parry(input.timestamp, deltaTime);
        }
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
    void Parry(int currentInputDashTick, float deltaTime)
    {
        if (playerState != PlayerState.Parrying)
        {
            parryHitbox.SetActive(false);
            parryTimer = 0;
            playerState = PlayerState.Parrying;
            stateInitTick = currentInputDashTick;
            m_turretAnimator.SetTrigger("Parry");
            m_hullAnimator.SetTrigger("Parry");
        }
        if (parryTimer >= parryStart)
        {
            parryHitbox.SetActive(true);
        }
        if (parryTimer >= parryStart+parryWindow)
        {
            parryHitbox.SetActive(false);
        }
        if (parryTimer >= parryTotalTime)
        {
            playerState = PlayerState.Moving;
            parryHitbox.SetActive(false);
        }
        parryTimer += deltaTime;
        
    }

    protected virtual void MoveTank(Vector2 moveVector, float deltaTime)
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

    protected virtual void DashTank(Vector2 moveVector, int currentInputDashTick, float deltaTime)
    {
        if (DEBUGDASh) Debug.Log($"[{SimClock.TickCounter}]: PlayerState : {playerState}, VelocidadDash: {m_dashSpeedMultiplier}");

        if (playerState != PlayerState.Dashing)
        {
            dashVec = moveVector;
            stateInitTick = currentInputDashTick;
            playerState = PlayerState.Dashing;
            if (DEBUGDASh) Debug.Log($"[{SimClock.TickCounter}]Comienza el dash");
        }

        //currentAcceleration = Mathf.Lerp(accelerationMultiplier, 0, (currentInputDashTick - (stateInitTick + fullDashTicks)) / (stateInitTick + dashTicks) - (stateInitTick + fullDashTicks));
        if (DEBUGDASh) Debug.Log($"[{SimClock.TickCounter}]: m_dashSpeedMultiplier: {m_dashSpeedMultiplier}");
        if (DEBUGDASh) Debug.Log($"[{SimClock.TickCounter}]: parámetros dash {currentInputDashTick}, {stateInitTick}, {m_dashTicks}");
        if (DEBUGDASh) Debug.Log($"[{SimClock.TickCounter}]: curve value: {m_dashSpeedCurve.Evaluate((float)(currentInputDashTick - stateInitTick) / m_dashTicks)}");
        float dashSpeed = m_speed * m_dashSpeedMultiplier * m_dashSpeedCurve.Evaluate((float)(currentInputDashTick - stateInitTick) / m_dashTicks);

        if (dashVec != Vector2.zero)
        {
            m_tankRB.MovePosition(m_tankRB.position + dashVec * deltaTime * dashSpeed);
        }
        else
        {
            m_tankRB.MovePosition(m_tankRB.position + (Vector2)transform.right * deltaTime * dashSpeed);
        }

        if (DEBUGDASh)
        {
            Debug.Log($"[{SimClock.TickCounter}] DASH: CurrentDashTick->{currentInputDashTick}. CurrentSpeedMult->{dashSpeed}. TickToEnd->{stateInitTick + m_dashTicks - currentInputDashTick}");
        }

        if (currentInputDashTick >= stateInitTick + m_dashTicks)
        {
            currentDashReloadTick = 0;
            playerState = PlayerState.Moving;
            stateInitTick = 0;
            dashVec = Vector2.zero;
            if (DEBUGDASh) Debug.Log("Se termina el dash");
        }
    }

    private void FireTank(Vector2 aimVector, float deltaTime)
    {
        cannon.Shoot(aimVector);
    }

    protected void AimTank(Vector2 aimVector, float deltaTime)
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

    protected bool CheckCanDash()
    {
        if (currentDashReloadTick == CAN_DASH) return true;
        else
        {
            if (currentDashReloadTick < m_reloadDashTicks)
            {
                if (SimClock.Instance.Active || SimClock.Instance.IsSingle)   //Este check es para que no se reduzca el cooldown en caso de que se este reconciliando
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

}
    