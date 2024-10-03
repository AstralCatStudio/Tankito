using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;

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
    private SimulationState<object> simulationState; //Clase que guarda las salidas de simulación(en este caso posición y rotación)

    private const int SIZE_CACHE = 1024;
    private int cache_index = 0;
    private ClientInputState<Vector2>[] inputStateCache = new ClientInputState<Vector2>[SIZE_CACHE];
    private SimulationState<object>[] simulationStateCache = new SimulationState<object>[SIZE_CACHE];

    private enum SimulationStateTankMovement{Position, Rotation}

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

    /*private void Update()
    {
        if (IsOwner)
        {
            inputState = new ClientInputState<Vector2>
            {
                input = m_movementVector,
            };
        }
    }*/

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
            if (inputState.input.magnitude <= Mathf.Epsilon) 
            {
                simulationFrame++;
                return;
            }
            
            inputState.simulationFrame = simulationFrame;

            ProcessInput(inputState.input);

            simulationState = GetSimulationState(inputState);

            cache_index = simulationFrame % SIZE_CACHE;

            inputStateCache[cache_index] = inputState;
            simulationStateCache[cache_index] = simulationState;

            simulationFrame++;
            /*transform.position = _TankPosition.Value;
            transform.rotation = _TankRotation.Value;*/
            
        }
        
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var newInput = context.ReadValue<Vector2>();

        if (IsOwner)
        {
            inputState = new ClientInputState<Vector2>
            {
                input = newInput
            };
        }
        //m_movementVector = new Vector2(newInput.x, newInput.y);
        Debug.Log($"OnMove input: {newInput}");
        //OnMoveServerRpc(newInput);
    }

    [ServerRpc]
    public void OnMoveServerRpc(Vector2 input)
    {
        m_movementVector = input;
    }

    private void ProcessInput(Vector2 input)
    {
        var targetAngle = Vector2.SignedAngle(transform.right, input);
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

        m_tankRB.MovePosition(m_tankRB.position + m_speed * Time.fixedDeltaTime * input);
    }

    private SimulationState<object> GetSimulationState(ClientInputState<Vector2> inputState)
    {
        return new SimulationState<object> (new List<object> {transform.position, transform.rotation}, inputState.simulationFrame);       
    }
}

