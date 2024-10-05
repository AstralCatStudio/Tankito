using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.UIElements;
using UnityEngine.InputSystem.XR;

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

    private SimulationState serverSimulationState = new SimulationState(); //Variable que almacena el estado de simulación del servidor
    private int lastCorrectedFrame = 0;

    private Queue<ClientInputState<Vector2>> serverQueue = new Queue<ClientInputState<Vector2>>();
    private enum SimulationStateTankMovement{Position = 0, Rotation = 1}

    private int simulationFrame = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner)
        {
            GetComponent<PlayerInput>().enabled = true;
            serverSimulationState.simulationFrame = 0;
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
        if (IsOwner)
        {
            int cache_index;
            if (inputState.input.magnitude <= Mathf.Epsilon)
            {
                cache_index = simulationFrame % SIZE_CACHE;

                /*inputStateCache[cache_index] = inputState;
                simulationStateCache[cache_index] = simulationState;*/
                simulationFrame++;
                return;
            }

            inputState.simulationFrame = simulationFrame;
            inputState.fixedDeltaTime = Time.fixedDeltaTime;

            ProcessInput(inputState);

            SendInputToServerRpc(inputState.input, inputState.simulationFrame, inputState.fixedDeltaTime);

            if (serverSimulationState != null) Reconciliate();

            simulationState = GetSimulationState(inputState);

            //Debug.Log("La simulación del CLIENTE en el frame " + simulationState.simulationFrame + " es: " + simulationState.position + "-" + simulationState.rotation);


            cache_index = simulationFrame % SIZE_CACHE;

            inputStateCache[cache_index] = inputState;
            simulationStateCache[cache_index] = simulationState;
            //Debug.Log(simulationFrame + " " + inputStateCache[cache_index].simulationFrame + " " + simulationStateCache[cache_index].simulationFrame+ " " + cache_index);
            //Debug.Log("SOY OWNER " + simulationFrame + ": " + );
            simulationFrame++;

        }
        else if (!IsServer)
        {
            transform.position = _TankPosition.Value;
            transform.rotation = _TankRotation.Value;   
        }
        if (IsServer)
        {
            ClientInputState<Vector2> serverIputState = null;

            // Obtain CharacterInputState's from the queue. 
            while (serverQueue.Count > 0 && (serverIputState = serverQueue.Dequeue()) != null)
            {
                Debug.Log("La psoición inicial en el SERVIDOR antes de procesar FRAME" + serverIputState.simulationFrame + " es: " + transform.position + "-" + transform.rotation);
                // Process the input.
                ProcessInput(serverIputState);

                // Obtain the current SimulationState.
                SimulationState state = GetSimulationState(serverIputState);

                // Send the state back to the client.
                SendServerSimulationToClientRpc(state.position, state.rotation, state.simulationFrame);
            }

            _TankPosition.Value = (Vector2)transform.position;
            _TankRotation.Value = transform.rotation;
        }
        

        
    }

    [ServerRpc]
    private void SendInputToServerRpc(Vector2 input, int simulationFrame, float fixedDeltaTime)
    {
        ClientInputState<Vector2> serverInputState = new ClientInputState<Vector2>
        {
            input = input,
            simulationFrame = simulationFrame,
            fixedDeltaTime = fixedDeltaTime           
        };

        serverQueue.Enqueue(serverInputState);

        /*ProcessInput(serverInputState);

        SimulationState serverSimulation = GetSimulationState(serverInputState);

        SendServerSimulationToClientRpc(serverSimulation.position, serverSimulation.rotation, serverSimulation.simulationFrame);*/
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

        m_tankRB.MoveRotation(m_tankRB.rotation + rotDeg);
        m_turret.Rotate(new Vector3(0, 0, -rotDeg));
        

        m_tankRB.MovePosition(m_tankRB.position + m_speed * input.fixedDeltaTime * input.input);
        if (IsServer)
        {
            Debug.Log("SERVIDOR " + input.simulationFrame + ": ENTRADA " + input.input + input.fixedDeltaTime + "- SALIDA " + transform.position + transform.rotation);
        }
        else
        {
            Debug.Log("CLIENTE " + input.simulationFrame + ": ENTRADA " + input.input + input.fixedDeltaTime + "- SALIDA " + transform.position + transform.rotation);

        }
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
            //Debug.Log("La simulación del SERVIDOR en el frame " + serverSimulationState.simulationFrame + " es: " + serverSimulationState.position + "-" + serverSimulationState.rotation);
        }   
    }

    private void Reconciliate()
    {
        //Debug.Log("Comienza la reconciliación");
        if (serverSimulationState.simulationFrame <= lastCorrectedFrame) return;

        int cache_index = serverSimulationState.simulationFrame % SIZE_CACHE;
        ClientInputState<Vector2> cachedInput = inputStateCache[cache_index];
        SimulationState cachedSimulation = simulationStateCache[cache_index];

        if (cachedInput == null || cachedSimulation == null)
        {
            transform.position = serverSimulationState.position;
            transform.rotation = serverSimulationState.rotation;

            lastCorrectedFrame = serverSimulationState.simulationFrame;
            return;
        }

        float tolerancePosition = 0.1f;
        float toleranceRotation = 0.1f;

        float differenceX = Mathf.Abs(cachedSimulation.position.x - serverSimulationState.position.x);
        float differenceY = Mathf.Abs(cachedSimulation.position.y - serverSimulationState.position.y);
        float differenceZ = Mathf.Abs(cachedSimulation.position.z - serverSimulationState.position.z);
        float differenceRotationX = Mathf.Abs(cachedSimulation.rotation.x - serverSimulationState.rotation.x);
        float differenceRotationY = Mathf.Abs(cachedSimulation.rotation.y - serverSimulationState.rotation.y);
        float differenceRotationZ = Mathf.Abs(cachedSimulation.rotation.z - serverSimulationState.rotation.z);
        float differenceRotationW = Mathf.Abs(cachedSimulation.rotation.w - serverSimulationState.rotation.w);

       //Debug.Log("Posicion" + cachedSimulation.simulationFrame + ": "+ cachedSimulation.position + "-" + serverSimulationState.simulationFrame + serverSimulationState.position);
       //Debug.Log("Rotacion" + cachedSimulation.simulationFrame + ": "+ cachedSimulation.rotation + " - " + serverSimulationState.simulationFrame + serverSimulationState.rotation);

        if (differenceRotationX > toleranceRotation || differenceRotationY > toleranceRotation || differenceRotationZ > toleranceRotation ||
            differenceRotationW > toleranceRotation || differenceX > tolerancePosition || differenceY > tolerancePosition 
            || differenceZ > tolerancePosition)
        {
            transform.position = serverSimulationState.position;
            transform.rotation = serverSimulationState.rotation;

            int rewindFrame = serverSimulationState.simulationFrame;
            while(rewindFrame < simulationFrame)
            {
                int rewindCacheIndex = rewindFrame % SIZE_CACHE;
                ClientInputState<Vector2> rewindCachedInput = inputStateCache[rewindCacheIndex];
                SimulationState rewindCachedSimulation = simulationStateCache[rewindCacheIndex];

                if (rewindCachedInput == null || rewindCachedSimulation == null)
                {
                    rewindFrame++;
                    continue;
                }

                ProcessInput(rewindCachedInput);

                SimulationState rewoundSimulationState = GetSimulationState(rewindCachedInput);
                rewoundSimulationState.simulationFrame = simulationFrame;
                simulationStateCache[rewindCacheIndex] = rewoundSimulationState;
                rewindFrame++;
            }
        }

        lastCorrectedFrame = serverSimulationState.simulationFrame;
    }
}

