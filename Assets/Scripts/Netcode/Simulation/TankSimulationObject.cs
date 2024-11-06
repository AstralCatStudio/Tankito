

using System;
using Unity.Android.Types;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class TankSimulationObject : ASimulationObject
    {
        [SerializeField] private Rigidbody2D m_tankRB;
        [SerializeField] private Rigidbody2D m_turretRB;

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