using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.UIElements;
using UnityEngine.InputSystem.XR;
using UnityEngine.Windows;
using System;

[Serializable]
public class ClientInputState<T>
{
    public T input;
    public int simulationFrame;
    public InputType inputType;
}

public class SimulationState
{
    public Vector3 position;
    public float rotation;
    public int simulationFrame;
}

public enum PlayerState { Moving, Dashing }
public enum InputType { MoveInput = 0, DashInput = 1 }

public class TankMovement : NetworkBehaviour
{
    private bool oAS = false;
    [SerializeField] private Rigidbody2D m_tankRB;
    [SerializeField] private float m_speed = 25.0f;
    [SerializeField] private float m_rotationSpeed = 1.0f;
    [SerializeField] private Transform m_turret;
    private NetworkVariable<Vector2> _TankPosition = new NetworkVariable<Vector2>(new Vector2(), NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
    private NetworkVariable<float> _TankRotation = new NetworkVariable<float>(new float(), NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private ClientInputState<Vector2> inputState = new ClientInputState<Vector2>(); //Clase que guarda los input y sus frames
    private Queue<ClientInputState<Vector2>> clientInputQueue = new Queue<ClientInputState<Vector2>>();
    private SimulationState simulationState; //Clase que guarda las salidas de simulación(en este caso posición y rotación)

    //Caches para almacenar los inputs y estados simulados en el cliente
    private const int SIZE_CACHE = 1024;
    private ClientInputState<Vector2>[] inputStateCache = new ClientInputState<Vector2>[SIZE_CACHE];
    private SimulationState[] simulationStateCache = new SimulationState[SIZE_CACHE];

    private SimulationState serverSimulationState = new SimulationState(); //Variable que almacena el estado de simulación del servidor
    private int lastCorrectedFrame = 0;

    private Queue<ClientInputState<Vector2>> serverQueue = new Queue<ClientInputState<Vector2>>(); //Cola de inputs recibidos por el servidor

    private int simulationFrame = 0;
    private float fixedDeltaTime;


    //Variables Dash
    [SerializeField] private float accelerationMultiplier = 6;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float fullDashDuration = 0.2f;
    [SerializeField] private float currentDashTick = 0;
    [SerializeField] private bool canDash = true;
    [SerializeField] private float dashReloadDuration = 1;
    [SerializeField] Vector2 inputWhileDash;
    [SerializeField] ClientInputState<Vector2> postDashInput = null;
    

    [SerializeField] private PlayerState playerState = PlayerState.Moving;

    // Start is called before the first frame update
    void Start()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;
        fixedDeltaTime = Time.fixedDeltaTime;
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
        serverSimulationState.simulationFrame = 0;

        if(IsOwner && IsServer)
        {
            oAS = true;
        }
    }

    void FixedUpdate()
    {
        if (IsOwner)
        {
            //Debug.Log(simulationFrame);
            if(postDashInput != null)
            {
                inputState = postDashInput;
                postDashInput = null;
                Debug.Log("Se copia el input: " + inputState.simulationFrame + " " + inputState.input + " " + inputState.inputType);
            }
            inputState.simulationFrame = simulationFrame;

            if (!oAS)
            {
                ProcessInput(inputState); //Se procesa la entrada  

                Physics2D.Simulate(Time.fixedDeltaTime);
            }   

            SendInputToServerRpc(inputState.input, inputState.simulationFrame, (int)inputState.inputType); //Se envía al servidor para que la simule

            if (serverSimulationState != null && !oAS) Reconciliate(); //En caso necesario, reconciliación

            simulationState = GetSimulationState(inputState.simulationFrame); //Se obtiene una simulación temporal de posición y rotación

            AddToCache(inputState, simulationState, simulationFrame % SIZE_CACHE, true);

            simulationFrame++;
        }
            
        else if (!IsServer)
        {
            m_tankRB.position = _TankPosition.Value;
            m_tankRB.rotation = _TankRotation.Value;  
        }
        if (IsServer)
        {
            ClientInputState<Vector2> serverIputState = null; //Variable temporal para almacenar el input
            
            while (serverQueue.Count > 0)
            {
                serverIputState = serverQueue.Dequeue();
                
                ProcessInput(serverIputState); //El servidor procesa el input
                
                Physics2D.Simulate(Time.fixedDeltaTime);
                
                SimulationState state = GetSimulationState(serverIputState.simulationFrame);

                SendServerSimulationToClientRpc(state.position, state.rotation, state.simulationFrame); //Se envía la simulación al cliente*/
                _TankPosition.Value = (Vector2)m_tankRB.position;
                _TankRotation.Value = m_tankRB.rotation;
            }
        }  
    }

    #region Functions

    private void AddToCache(ClientInputState<Vector2> newInputState, SimulationState newSimulationState, int cacheIndex, bool debug)
    {
        Debug.Log("CACHE INDEX: " + cacheIndex + " ESTADO: " + newSimulationState.position + " " + newInputState.simulationFrame + " " + newSimulationState.simulationFrame);
        inputStateCache[cacheIndex] = inputState;
        simulationStateCache[cacheIndex] = simulationState;
        //Debug.Log("CACHE INDEX: " + cacheIndex + " ESTADO: " + simulationStateCache[cacheIndex].position);
        //Debug.Log("CACHE INDEX: " + cacheIndex + " ESTADO: " + simulationStateCache[cacheIndex].position + " " + inputStateCache[cacheIndex].simulationFrame + " " + simulationStateCache[cacheIndex].simulationFrame);
    }

    private SimulationState GetSimulationState(int newSimulationFrame)
    {
        /*if (IsServer)
        {
            Debug.Log("SERVIDOR " + newSimulationFrame + "- ESTADO: " + transform.position + transform.rotation);
        }
        else
        {
            Debug.Log("CLIENTE " + newSimulationFrame +  "- ESTADO :" + transform.position + transform.rotation);
        }*/
        return new SimulationState
        {
            position = m_tankRB.position,
            rotation = m_tankRB.rotation,
            simulationFrame = newSimulationFrame
        };
    }

    IEnumerator DashReloading()
    {
        yield return new WaitForSeconds(dashReloadDuration);
        canDash = true;
    }

    #endregion

    #region PhysicsFunctions

    private void ProcessInput(ClientInputState<Vector2> input)
    {
        switch (input.inputType)
        {
            case InputType.MoveInput:
                ProcessMove(input); break;
            case InputType.DashInput:
                ProcessDash(input); break;
        }
    }

    private void ProcessMove(ClientInputState<Vector2> input)
    {
        var targetAngle = Vector2.SignedAngle(transform.right, input.input);
        float rotDeg = 0f;

        if (Mathf.Abs(targetAngle) >= fixedDeltaTime * m_rotationSpeed)
        {
            rotDeg = Mathf.Sign(targetAngle) * fixedDeltaTime * m_rotationSpeed;
        }
        else
        {
            // Si el angulo es demasiado pequeño entonces snapeamos a él (inferior a la mínima rotación por frame)
            rotDeg = targetAngle;
        }

        m_tankRB.MoveRotation(m_tankRB.rotation + rotDeg);
        //m_tankRB.rotation += rotDeg;
        m_turret.Rotate(new Vector3(0, 0, -rotDeg));

        //m_tankRB.position += m_speed * input.fixedDeltaTime * input.input;
        m_tankRB.MovePosition(m_tankRB.position + m_speed * fixedDeltaTime * input.input);
    }

    private void ProcessDash(ClientInputState <Vector2> input)
    {
        float dashTicks = dashDuration / Time.fixedDeltaTime;
        float fullDashTicks = fullDashDuration / Time.fixedDeltaTime;
        float currentAcceleration;
        
        if(currentDashTick < fullDashTicks)
        {
            currentAcceleration = accelerationMultiplier;
            if(currentDashTick == 0)
            {
                playerState = PlayerState.Dashing;
                canDash = false;
                Debug.Log("Dash comienza FRAME " + input.simulationFrame);
            }
        }
        else 
        {
            currentAcceleration = Mathf.Lerp(accelerationMultiplier, 1, (currentDashTick - fullDashTicks) / (dashTicks - fullDashTicks)); ;
        }
        if(input.input != Vector2.zero)
        {
            m_tankRB.MovePosition(m_tankRB.position + input.input * Time.fixedDeltaTime * m_speed * currentAcceleration);
        }
        else
        {
            m_tankRB.MovePosition(m_tankRB.position + (Vector2)transform.right * Time.fixedDeltaTime * m_speed * currentAcceleration);
        }
        
        if (currentDashTick >= dashTicks)
        {
            currentDashTick = 0;

            postDashInput = new ClientInputState<Vector2>
            {
                input = inputWhileDash,
                simulationFrame = simulationFrame,
                inputType = InputType.MoveInput,
            };
            inputWhileDash = Vector2.zero;

            playerState = PlayerState.Moving;
            Debug.Log("Dash termina FRAME " + input.simulationFrame);
            StartCoroutine("DashReloading");
            return;
        }
        currentDashTick++;
    }

    private void Reconciliate()
    {
        //Debug.Log("Comienza la reconciliación");
        if (serverSimulationState.simulationFrame <= lastCorrectedFrame) return;

        int cache_index = serverSimulationState.simulationFrame % SIZE_CACHE;
        ClientInputState<Vector2> cachedInput = inputStateCache[cache_index];
        SimulationState cachedSimulation = simulationStateCache[cache_index];
        //Debug.Log("En la cache " + cache_index + " esta " + cachedSimulation.position + " - " + cachedSimulation.rotation + " " + cachedInput.simulationFrame + " " + cachedSimulation.simulationFrame);

        if (cachedInput == null || cachedSimulation == null)
        {
            m_tankRB.position = serverSimulationState.position;
            m_tankRB.rotation = serverSimulationState.rotation;

            lastCorrectedFrame = serverSimulationState.simulationFrame;
            return;
        }

        float tolerancePosition = 0.1f;
        float toleranceRotation = 0.1f;

        float differenceX = Mathf.Abs(cachedSimulation.position.x - serverSimulationState.position.x);
        float differenceY = Mathf.Abs(cachedSimulation.position.y - serverSimulationState.position.y);
        float differenceZ = Mathf.Abs(cachedSimulation.position.z - serverSimulationState.position.z);
        float differenceRotation = Mathf.Abs(cachedSimulation.rotation - serverSimulationState.rotation);

        //Debug.Log("Posicion" + cachedSimulation.simulationFrame + ": " + cachedSimulation.position + "-" + serverSimulationState.simulationFrame + serverSimulationState.position);
        //Debug.Log("Rotacion" + cachedSimulation.simulationFrame + ": " + cachedSimulation.rotation + " - " + serverSimulationState.simulationFrame + serverSimulationState.rotation);

        if (differenceRotation > toleranceRotation || differenceX > tolerancePosition || differenceY > tolerancePosition
            || differenceZ > tolerancePosition)
        {
            m_tankRB.position = serverSimulationState.position;
            m_tankRB.rotation = serverSimulationState.rotation;

            Debug.Log("Posicion CACHE " + cache_index + " " + cachedSimulation.simulationFrame + ": " + cachedSimulation.position + "-" + serverSimulationState.simulationFrame + serverSimulationState.position);
            Debug.Log("Rotacion" + cachedSimulation.simulationFrame + ": " + cachedSimulation.rotation + " - " + serverSimulationState.simulationFrame + serverSimulationState.rotation);

            int rewindFrame = serverSimulationState.simulationFrame + 1;
            Debug.Log("Reconciliación en el frame " + serverSimulationState.simulationFrame);
            while (rewindFrame <= simulationFrame)
            {
                int rewindCacheIndex = rewindFrame % SIZE_CACHE;
                ClientInputState<Vector2> rewindCachedInput = inputStateCache[rewindCacheIndex];
                SimulationState rewindCachedSimulation = simulationStateCache[rewindCacheIndex];

                if (rewindCachedInput == null || rewindCachedSimulation == null)
                {
                    Debug.Log("Cache vacía posición " + rewindCacheIndex);
                    rewindFrame++;
                    continue;
                }

                if (rewindCachedInput.input.magnitude > Mathf.Epsilon)
                {
                    ProcessInput(rewindCachedInput);
                    Physics2D.Simulate(Time.fixedDeltaTime);
                }

                SimulationState rewoundSimulationState = GetSimulationState(rewindCachedInput.simulationFrame);
                rewoundSimulationState.simulationFrame = rewindFrame;
                simulationStateCache[rewindCacheIndex] = rewoundSimulationState;
                //Debug.Log("FRAME: " + rewindFrame + " - STATE: " + simulationStateCache[rewindCacheIndex].position);
                rewindFrame++;
            }
        }

        lastCorrectedFrame = serverSimulationState.simulationFrame;
    }
    #endregion




    #region RPCs

    //El servidor recibe el input de los jugadores y los almacena en la cola
    [ServerRpc]
    private void SendInputToServerRpc(Vector2 input, int simulationFrame, int inputType)
    {
        ClientInputState<Vector2> serverInputState = new ClientInputState<Vector2>
        {
            input = input,
            simulationFrame = simulationFrame,
            inputType = (InputType)inputType
        };

        serverQueue.Enqueue(serverInputState);
    }

    [ClientRpc]
    private void SendServerSimulationToClientRpc(Vector2 simPosition, float simRotation, int simulationFrame)
    {
        if (IsOwner)
        {
            if (serverSimulationState.simulationFrame < simulationFrame)
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

    #endregion

    #region Events 
    public void OnMove(InputAction.CallbackContext context)
    {
        if(playerState != PlayerState.Dashing)
        {
            var newInput = context.ReadValue<Vector2>();
            inputState = new ClientInputState<Vector2>
            {
                input = new Vector2(newInput.x, newInput.y),
                inputType = InputType.MoveInput
            };
        }
        else
        {
            inputWhileDash = context.ReadValue<Vector2>();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (canDash)
        {
            inputState.inputType = InputType.DashInput;
        }
    }
    #endregion
}

