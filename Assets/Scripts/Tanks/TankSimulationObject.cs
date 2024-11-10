using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class TankSimulationObject : ASimulationObject
    {
        [SerializeField] private Rigidbody2D m_tankRB;
        [SerializeField] private Rigidbody2D m_turretRB;
        [SerializeField] private ITankInput m_inputComponent;

        public void StartInputReplay(int timestamp) { m_inputComponent.StartInputReplay(timestamp); }
        public int StopInputReplay() { return m_inputComponent.StopInputReplay(); }

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

            if (m_turretRB == null)
            {
                Debug.Log("Error tank turret reference not set.");
            }
        }

        public override void OnNetworkSpawn()
        {
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
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsServer)
            {
                ServerSimulationManager.Instance.remoteInputTanks.Remove(NetworkObjectId);
            }
        }

        public override ISimulationState GetSimState()
        {
            return new TankSimulationState
            (
                m_tankRB.position,
                m_tankRB.rotation,
                m_tankRB.velocity,
                m_turretRB.rotation
            );
        }

        public override void SetSimState(in ISimulationState state)
        {
            if (state is TankSimulationState tankState)
            {
                m_tankRB.position = tankState.Position;
                m_tankRB.rotation = tankState.HullRotation;
                m_turretRB.rotation = tankState.TurretRotation;
                m_tankRB.velocity = tankState.Velocity;
            }
            else
            {
                throw new ArgumentException("Invalid state type"); 
            }
        }
    }
}