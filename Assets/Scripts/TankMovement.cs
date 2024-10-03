using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;

public class SimulationState
{
    public Vector3 position;
    public Quaternion rotation;
    public int simulationFrame;
}

public class TankMovement : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D m_tankRB;
    [SerializeField] private float m_speed = 25.0f;
    [SerializeField] private float m_rotationSpeed = 1.0f;
    [SerializeField] private Transform m_turret;
    private NetworkVariable<Vector2> _TankPosition = new NetworkVariable<Vector2>(new Vector2(), NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
    private NetworkVariable<Quaternion> _TankRotation = new NetworkVariable<Quaternion>(new Quaternion(), NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private ClientInputState<Vector2> inputState = new ClientInputState<Vector2>(); //Clase que guarda los input y sus frames
    private SimulationState simulationState; //Clase que guarda las salidas de simulación(en este caso posición y rotación)

    private const int SIZE_CACHE = 1024;
    private ClientInputState<Vector2>[] inputStateCache = new ClientInputState<Vector2>[SIZE_CACHE];
    private SimulationState[] simulationStateCache = new SimulationState[SIZE_CACHE];

    private SimulationState serverSimulationState; //Variable que almacena el estado de simulación del servidor
    private int lastCorrectedFrame = 0;

    private enum SimulationStateTankMovement{Position = 0, Rotation = 1}

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
            _TankPosition.Value = (Vector2)transform.position;
            _TankRotation.Value = transform.rotation;
        }
        if(IsOwner)
        {
            if (inputState.input.magnitude <= Mathf.Epsilon) 
            {
                simulationFrame++;
                return;
            }
            
            inputState.simulationFrame = simulationFrame;
            inputState.fixedDeltaTime = Time.fixedDeltaTime;

            ProcessInput(inputState);

            SendInputToServerRpc(inputState.input, inputState.simulationFrame, inputState.fixedDeltaTime);

            if (serverSimulationState != null) Reconciliate();

            simulationState = GetSimulationState(inputState);

            int cache_index = simulationFrame % SIZE_CACHE;

            inputStateCache[cache_index] = inputState;
            simulationStateCache[cache_index] = simulationState;

            simulationFrame++;

        }
        else
        {
            transform.position = _TankPosition.Value;
            transform.rotation = _TankRotation.Value;   
        }

        
    }

    [ServerRpc]
    private void SendInputToServerRpc(Vector2 input, int simulationFrame, float fixedDeltaTime)
    {
        ClientInputState<Vector2> inputState = new ClientInputState<Vector2>
        {
            input = input,
            simulationFrame = simulationFrame,
            fixedDeltaTime = fixedDeltaTime           
        };

        ProcessInput(inputState);

        SimulationState serverSimulation = GetSimulationState(inputState);

        SendServerSimulationToClientRpc(serverSimulation.position, serverSimulation.rotation, simulationFrame);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var newInput = context.ReadValue<Vector2>();
        inputState = new ClientInputState<Vector2>
        {
            input = new Vector2(newInput.x, newInput.y)
        };
        //m_movementVector = new Vector2(newInput.x, newInput.y);
        //Debug.Log($"OnMove input: {inputState.input}");
        //OnMoveServerRpc(newInput);
    }

    /*[ServerRpc]
    public void OnMoveServerRpc(Vector2 input)
    {
        m_movementVector = input;
    }*/

    private void ProcessInput(ClientInputState<Vector2> input)
    {
        var targetAngle = Vector2.SignedAngle(transform.right, input.input);
        float rotDeg = 0f;

        if (Mathf.Abs(targetAngle) >= input.fixedDeltaTime * m_rotationSpeed)
        {
            rotDeg = Mathf.Sign(targetAngle) * input.fixedDeltaTime * m_rotationSpeed;
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

        m_tankRB.MovePosition(m_tankRB.position + m_speed * input.fixedDeltaTime * input.input);
    }

    private SimulationState GetSimulationState(ClientInputState<Vector2> inputState)
    {
        return new SimulationState
        {
            position = transform.position,
            rotation = transform.rotation,
            simulationFrame = inputState.simulationFrame
        };       
    }

    [ClientRpc]
    private void SendServerSimulationToClientRpc(Vector2 simPosition, Quaternion simRotation, int simulationFrame)
    {
        if (IsOwner)
        {
            if (serverSimulationState?.simulationFrame < simulationFrame)
            {
                serverSimulationState = new SimulationState
                {
                    position = simPosition,
                    rotation = simRotation,
                    simulationFrame = simulationFrame
                };
            }
        }   
    }

    private void Reconciliate()
    {
        Debug.Log("Comienza la reconciliación");
        if (serverSimulationState.simulationFrame <= lastCorrectedFrame) return;

        int cache_index = serverSimulationState.simulationFrame % SIZE_CACHE;
        ClientInputState<Vector2> cachedInput = inputStateCache[cache_index];
        SimulationState cachedSimulation = simulationStateCache[cache_index];

        if(cachedInput == null || cachedSimulation == null)
        {
            transform.position = serverSimulationState.position;
            transform.rotation = serverSimulationState.rotation;

            lastCorrectedFrame = serverSimulationState.simulationFrame;
            return;
        }

        float tolerancePosition = 0;
        float toleranceRotation = 0;

        if((Vector3.Distance(transform.position, serverSimulationState.position) > tolerancePosition) || Quaternion.Angle(transform.rotation, 
            serverSimulationState.rotation) > toleranceRotation) 
        {
            transform.position = serverSimulationState.position;
            transform.rotation = serverSimulationState.rotation;

            int rewindFrame = serverSimulationState.simulationFrame;
            while(rewindFrame < simulationFrame)
            {
                int rewindCacheIndex = rewindFrame % SIZE_CACHE;
                ClientInputState<Vector2> rewindCachedInput = inputStateCache[cache_index];
                SimulationState rewindCachedSimulation = simulationStateCache[cache_index];

                if (rewindCachedInput == null || rewindCachedSimulation == null)
                {
                    rewindFrame++;
                    continue;
                }

                ProcessInput(rewindCachedInput);

                rewindCachedInput.simulationFrame = rewindFrame;

                SimulationState rewoundSimulationState = GetSimulationState(rewindCachedInput);
                simulationStateCache[cache_index] = rewoundSimulationState;
                rewindFrame++;
            }
        }

        lastCorrectedFrame = serverSimulationState.simulationFrame;
    }
}

