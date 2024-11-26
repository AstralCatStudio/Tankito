using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class TankSimulationObject : ASimulationObject
    {
        [SerializeField] private Rigidbody2D m_tankRB;
        [SerializeField] private Rigidbody2D m_turretRB;
        [SerializeField] private ITankInput m_inputComponent;
        [SerializeField] private TankController m_controller;
        public void StartInputReplay(int timestamp) { m_inputComponent.StartInputReplay(timestamp); }
        public int StopInputReplay() { return m_inputComponent.StopInputReplay(); }
        public override SimulationObjectType SimObjType => SimulationObjectType.Tank;
        
        void Start()
        {
            if (m_tankRB == null)
            {
                m_tankRB = GetComponent<Rigidbody2D>();
                if (m_tankRB == null)
                {
                    Debug.Log("Error tank Rigibody2D reference not set.");
                }
            }
            if(m_controller == null)
            {
                m_controller = GetComponent<TankController>();
                if (m_tankRB == null)
                {
                    Debug.Log("Error tank controller reference not set.");
                }
            }

            if (m_turretRB == null)
            {
                Debug.Log("Error tank turret reference not set.");
            }

        }

        public override void OnNetworkSpawn()
        {
            if (SimObjId == default)
            {
                GenerateSimObjId(OwnerClientId, -1,0);
            }

            base.OnNetworkSpawn();
            
            var playerInput = gameObject.GetComponent<TankPlayerInput>();

            if (playerInput == null)
            {
                Debug.LogWarning("Incorrect ITankInput setup! It should start ONLY with the TankPlayerInput component connected, so it can be then disconnected if it doesn't need it.");
            }

            if (IsLocalPlayer)
            {
                m_inputComponent = playerInput;
                
                GameManager.Instance.BindInputActions(playerInput);
            }
            else if (IsServer)
            {
                Destroy(playerInput);

                m_inputComponent = gameObject.AddComponent<RemoteTankInput>();
                ServerSimulationManager.Instance.remoteInputTanks[OwnerClientId] = (RemoteTankInput)m_inputComponent;
            }
            else if (IsClient)
            {
                Destroy(playerInput);

                m_inputComponent = gameObject.AddComponent<EmulatedTankInput>();
                ClientSimulationManager.Instance.emulatedInputTanks[OwnerClientId] = (EmulatedTankInput)m_inputComponent;
            }

            GetComponent<TankController>().BindInputSource(m_inputComponent);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsServer)
            {
                ServerSimulationManager.Instance.remoteInputTanks.Remove(NetworkObjectId);
            }

            if (IsLocalPlayer)
            {
                if (m_inputComponent is TankPlayerInput localTankInput)
                {
                    GameManager.Instance.UnbindInputActions(localTankInput);
                }
                else
                {
                    throw new InvalidOperationException($"Can't unbind inpunt actions because input component is not {typeof(TankPlayerInput)}");
                }
            }
        }

        public override ISimulationState GetSimState()
        {
            PlayerState playerState = m_controller.PlayerState;
            int stateInitTick;
            if(playerState == PlayerState.Moving)
            {
                stateInitTick = 0;
            }
            else
            {
                stateInitTick = m_controller.StateInitTick;
            }

            return new TankSimulationState
            (
                m_tankRB.position,
                m_tankRB.rotation,
                m_tankRB.velocity,
                m_turretRB.rotation,
                m_inputComponent.GetCurrentInput().action,
                playerState,
                stateInitTick
            );
        }

        public override void SetSimState(in ISimulationState state)
        {
            if (state is TankSimulationState tankState)
            {
                m_tankRB.transform.position = tankState.Position;
                m_tankRB.transform.rotation = Quaternion.AngleAxis(tankState.HullRotation, Vector3.forward);
                m_turretRB.transform.rotation = Quaternion.AngleAxis(tankState.TurretRotation, Vector3.forward);
                m_tankRB.velocity = tankState.Velocity;
                m_controller.PlayerState = tankState.PlayerState;
                m_controller.StateInitTick = tankState.StateInitTick;
            }
            else
            {
                throw new ArgumentException("Invalid state type"); 
            }
        }
    }
}