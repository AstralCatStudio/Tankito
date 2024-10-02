using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Globalization;

public class TankMovement : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D m_tankRB;
    [SerializeField] private float m_speed = 25.0f;
    [SerializeField] private float m_rotationSpeed = 1.0f;
    [SerializeField] private Transform m_turret;
    private Vector2 m_movementVector = Vector2.zero;
    private NetworkVariable<Vector2> _TankPosition = new NetworkVariable<Vector2>(new Vector2(), NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
    private NetworkVariable<Quaternion> _TankRotation = new NetworkVariable<Quaternion>(new Quaternion(), NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private ClientInputState<Vector2> inputState = new ClientInputState<Vector2>(); //Clase que guarda los input y sus frames
    private int simulationFrame = 0;


    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner)
        {
            GetComponent<PlayerInput>().enabled = true;
        }
        if (m_tankRB == null)
        {
            m_tankRB = GetComponent<Rigidbody2D>();
            if (m_tankRB == null)
            {
                Debug.Log("Error tank Rigibody2D reference not set.");
            }
        }

        if (m_turret == null)
        {
            Debug.Log("Error tank turret reference not set.");
        }
    }

    void FixedUpdate()
    {
        if (IsServer)

        {
            if (m_movementVector.magnitude <= Mathf.Epsilon) return;

            ProcessInput(m_movementVector);
            
            _TankPosition.Value = (Vector2)transform.position;
            _TankRotation.Value = transform.rotation;
        }
        else
        {
            transform.position = _TankPosition.Value;
            transform.rotation = _TankRotation.Value;
        }
        
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<Vector2>();
        //m_movementVector = new Vector2(input.x, input.y);
        Debug.Log($"OnMove input: {input}");
        OnMoveServerRpc(input);
    }

    [ServerRpc]
    public void OnMoveServerRpc(Vector2 input)
    {
        m_movementVector = input;
    }

    private void ProcessInput(Vector2 input)
    {
        var targetAngle = Vector2.SignedAngle(transform.right, m_movementVector);
        float rotDeg = 0f;

        if (Mathf.Abs(targetAngle) >= Time.fixedDeltaTime * m_rotationSpeed)
        {
            rotDeg = Mathf.Sign(targetAngle) * Time.fixedDeltaTime * m_rotationSpeed;
        }
        else
        {
            // Si el angulo es demasiado pequeño entonces snapeamos a él (inferior a la mínima rotación por frame)
            rotDeg = targetAngle;
        }

        if (Mathf.Abs(rotDeg) > Mathf.Epsilon)
        {
            m_tankRB.MoveRotation(m_tankRB.rotation + rotDeg);
            m_turret.Rotate(new Vector3(0, 0, -rotDeg));
        }

        m_tankRB.MovePosition(m_tankRB.position + m_speed * Time.fixedDeltaTime * m_movementVector);
    }
}

