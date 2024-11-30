using System.Collections;
using System.Collections.Generic;
using Tankito;
using Tankito.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tankito.SinglePlayer
{
    public class TankSinglePlayerInput : TankPlayerInput
    {
        TankitoInputActions m_inputActions;
        PlayerInput m_playerInput;
        private void OnEnable()
        {
            m_playerInput = GameObject.FindObjectOfType<PlayerInput>(true);
            m_inputActions = new TankitoInputActions();
            m_playerInput.actions = m_inputActions.asset;

            m_inputActions.Player.Move.performed += OnMove;
            m_inputActions.Player.Move.canceled += OnMove;
            m_inputActions.Player.Look.performed += OnAim;
            m_inputActions.Player.Look.canceled += OnAim;
            m_inputActions.Player.Dash.performed += OnDash;
            m_inputActions.Player.Dash.canceled += OnDash;
            m_inputActions.Player.Parry.performed += OnParry;
            m_inputActions.Player.Parry.canceled += OnParry;
            m_inputActions.Player.Fire.performed += OnFire;
            m_inputActions.Player.Fire.canceled += OnFire;
        }

        private void OnDisable()
        {
            m_inputActions.Player.Move.performed -= OnMove;
            m_inputActions.Player.Move.canceled -= OnMove;
            m_inputActions.Player.Look.performed -= OnAim;
            m_inputActions.Player.Look.canceled -= OnAim;
            m_inputActions.Player.Dash.performed -= OnDash;
            m_inputActions.Player.Dash.canceled -= OnDash;
            m_inputActions.Player.Parry.performed -= OnParry;
            m_inputActions.Player.Parry.canceled -= OnParry;
            m_inputActions.Player.Fire.performed -= OnFire;
            m_inputActions.Player.Fire.canceled -= OnFire;
        }

        public override InputPayload GetInput()
        {
            Aim();
            InputPayload gotInput = GetCurrentInput();
            gotInput.timestamp = SimClock.TickCounter;
            SetCurrentAction(TankAction.None);
            return gotInput;
        }
    }
}

