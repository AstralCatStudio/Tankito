using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Tankito.Mobile
{
    public class EmulatedButtonControl : OnScreenControl
    {
        [InputControl(layout = "Button")]
        [SerializeField]
        private string m_ControlPath;
        float m_timeToEmulate = 0;
        double m_timer = 0;

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }

        void Update()
        {
            m_timer += Time.deltaTime;

            if (m_timer >= m_timeToEmulate)
            {
                m_timer = 0;
                SentDefaultValueToControl();
            }
        }

        public void EmulateInput(float value, float time)
        {
            m_timeToEmulate = time;
            SendValueToControl(value);
        }
    }
}