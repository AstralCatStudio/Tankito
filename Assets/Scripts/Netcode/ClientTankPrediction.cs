using UnityEngine;
using Unity.Netcode;

namespace Tankito.Netcode
{
    public class ClientTankPrediction : NetworkBehaviour
    {
        [SerializeField]
        private TankMovement m_TankMovement;
        [SerializeField]
        private TankAim m_TankAim;

        void Start()
        {
            if (m_TankMovement == null)
                m_TankMovement = GetComponentInChildren<TankMovement>();
            if(m_TankMovement == null)
                Debug.LogWarning("No TankMovement Script attached to Tank!");

            if (m_TankAim == null)
                m_TankAim = GetComponentInChildren<TankAim>();
            if(m_TankAim == null)
                Debug.LogWarning("No TankAim Script attached to Tank!");
        }
    }
}