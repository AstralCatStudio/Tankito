using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.InputSystem.Controls;

namespace Tankito.Mobile
{
    public class TouchButton : OnScreenControl
    {
        [InputControl(layout = "Button")]
        [SerializeField]
        private string m_ControlPath;
        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
        
        private Finger m_boundFinger;
        private Canvas m_parentCanvas;
        private TouchControlManager m_touchControlManager;

        
        void Awake()
        {
            if (m_parentCanvas == null)
            {
                Canvas[] c = GetComponentsInParent<Canvas>();
                m_parentCanvas = c[c.Length - 1];
            }
            m_touchControlManager = m_parentCanvas.GetComponent<TouchControlManager>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            ETouch.Touch.onFingerDown += HandleFingerDown;
            ETouch.Touch.onFingerUp += HandleLoseFinger;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            ETouch.Touch.onFingerDown -= HandleFingerDown;
            ETouch.Touch.onFingerUp -= HandleLoseFinger;
        }

        private void HandleFingerDown(Finger touchedFinger)
        {
            if (m_boundFinger != null) return;

            if (m_touchControlManager.CheckIfTouched(touchedFinger.screenPosition, this.GetType()))
            {
                m_boundFinger = touchedFinger;
                SendValueToControl(1.0f);
            }
        }

        private void HandleLoseFinger(Finger lostFinger)
        {
            if (lostFinger != m_boundFinger) return;
            m_boundFinger = null;
            SendValueToControl(0.0f);
        }
    }
}
