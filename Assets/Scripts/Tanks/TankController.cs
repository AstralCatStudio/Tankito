using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tankito.Netcode;
using Tankito.Netcode.Simulation;
using UnityEngine;
using UnityEngine.Windows;

namespace Tankito
{
    public class TankController : ATankController
    {
        public static readonly HullStatsModifier BaseTankStats = 
            new HullStatsModifier(
                speed: 3f,
                rotSpeed: 360f,
                health: 2,
                parryTime: 0.15f,
                parryCooldown: 1.5f,
                dashSpeed: 3f,
                dashDistance: 0.5f,
                dashCooldown: 2f
            );
        private List<HullModifier> m_modifiers = new List<HullModifier>();
        public List<HullModifier> Modifiers => m_modifiers;
        
        void OnEnable()
        {
            StateInitTick = 0;
            // Subscribe to SimulationObject Kinematics
            var tankSimObj = GetComponent<TankSimulationObject>();
            tankSimObj.OnComputeKinematics += ProcessInput;

            //Subscribe to Round Countdown Start
            //RoundManager.Instance.OnPreRoundStart += ApplyModifierList;
        }

        void OnDisable()
        {
            // Unsubscribe to SimulationObject Kinematics
            var tankSimObj = GetComponent<TankSimulationObject>();
            tankSimObj.OnComputeKinematics -= ProcessInput;
            
            //Unsubscribe to Round Countdown Start
            //RoundManager.Instance.OnPreRoundStart -= ApplyModifierList;
        }

        public void ApplyModifierList(int nRound = 0)
        {
            ApplyModifier(BaseTankStats, true);
            foreach(var mod in m_modifiers)
            {
                ApplyModifier(mod.hullStatsModifier, false);
            }
        }

        void ApplyModifier(HullStatsModifier mod, bool reset = false)
        {
            //Debug.Log("tanque aplicando mods");
            if (reset)
            {
                m_speed = mod.speedMultiplier;
                m_rotationSpeed = mod.rotationSpeedMultiplier;
                GetComponent<TankData>().SetHealth(mod.extraHealth);
            }
            else
            {
                m_speed *= mod.speedMultiplier;
                m_rotationSpeed *= mod.rotationSpeedMultiplier;
                GetComponent<TankData>().AddHealth(mod.extraHealth);
            }
            SetParryTicksFromSeconds(mod.extraParryTime, mod.parryCooldownTimeAdded, reset);
            SetDashParams(mod.dashDistanceMultiplier, mod.dashSpeedMultiplier, mod.dashCooldownTimeAdded, reset);
        }

        private void SetParryTicksFromSeconds(float parryDuration, float parryCooldown, bool overwrite)
        {
            if (overwrite)
            {

            }
            else
            {

            }
        }

        private void SetDashParams(float dashDistance, float dashSpeed, float dashCooldown, bool overwrite)
        {
            if (overwrite)
            {
                m_dashSpeedMultiplier = dashSpeed;
                m_dashDistance = dashDistance;
                m_reloadDashTicks = Mathf.CeilToInt(dashCooldown / SimClock.SimDeltaTime);
            }
            else
            {
                m_dashSpeedMultiplier *= dashSpeed;
                m_dashDistance += dashDistance;
                m_reloadDashTicks += Mathf.CeilToInt(dashCooldown / SimClock.SimDeltaTime);
            }

            m_dashTicks = Mathf.CeilToInt((m_dashDistance/m_dashSpeedMultiplier) / SimClock.SimDeltaTime);
        }

        public void BindInputSource(ITankInput inputSource)
        {
            m_tankInput = inputSource;
        }

        public void ProcessInput(float deltaTime)
        {
            var input = m_tankInput.GetInput();
            if (DEBUGCONT) Debug.Log($"GetInput called, received input: {input}");
            ProcessInput(input, deltaTime);
        }             

        #region Modifiers & TankData

        #endregion
    } 
}