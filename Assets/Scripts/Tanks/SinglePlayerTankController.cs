using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class SinglePlayerTankController : ATankController
    {
        [SerializeField] float m_dashCooldown = 2f;

        private void Awake()
        {
            TankInputComponent = GetComponent<TankSinglePlayerInput>();
        }
        protected override void Start()
        {
            base.Start();
            float dashTime = (m_dashDistance / m_dashSpeedMultiplier);
            m_dashTicks = (int)(dashTime / Time.fixedDeltaTime);
            Debug.Log(m_dashTicks);
            m_reloadDashTicks = (int)(m_dashCooldown / Time.fixedDeltaTime);
        }

        private void FixedUpdate()
        {
            InputPayload newInput = TankInputComponent.GetInput();
            ProcessInput(newInput, Time.fixedDeltaTime);
        }
    }
}

