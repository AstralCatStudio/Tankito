using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;
using System.Linq;
using System.Collections.Generic;

namespace Tankito.Mobile
{
    public enum ScreenHotspot
    {
        Left = 0,
        Right = 1
    }

    public enum StickType
    {
        Fixed = 0,
        Floating = 1,
        Follow = 2 // TODO: implement
    }

    public class TouchJoystick : OnScreenControl
    {
        [SerializeField]
        private Vector2 m_joystickSize = new Vector2(300, 300);
        [SerializeField]
        private RectTransform m_joystickRect;
        [SerializeField]
        private RectTransform JoystickHandleRect;

        [InputControl(layout = "Vector2")]
        [SerializeField]
        private string m_ControlPath;
        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }

        private Finger m_joystickFinger;
        private Vector2 m_displacementAmount;
        private Canvas m_parentCanvas;
        private TouchControlManager m_touchControlManager;
        [SerializeField] private bool m_hideStick;
        [SerializeField] private StickType m_stickType;
        [SerializeField] private ScreenHotspot m_hotspotPosition;
        [SerializeField] private float m_hotspotDeadzone = 100f;

        [SerializeField] private EmulatedButtonControl m_pressControl;
        [SerializeField] private float m_pressControlValue = 1f;

        [SerializeField] private EmulatedButtonControl m_releaseControl;
        [SerializeField] private float m_releaseControlValue = 1f;

        public bool debugGUI;
        public bool DEBUG = false;
        public override string ToString()
        {
            return $"{base.ToString()}[{m_hotspotPosition})]";
        }

        void Awake()
        {
            if (m_joystickRect == null)
            {
                Debug.LogWarning($"JosytickRect for {this} was not set!");
                m_joystickRect = GameObject.Find( (m_hotspotPosition == ScreenHotspot.Left) ? "LeftJoystick" : "RightJoystick").GetComponent<RectTransform>();
            }
            if (m_parentCanvas == null || m_touchControlManager == null)
            {
                m_touchControlManager = m_joystickRect.GetComponentInParent<TouchControlManager>(true);
            }
            m_parentCanvas = m_touchControlManager.GetComponentInChildren<Canvas>(true);
            if (DEBUG) Debug.Log($"{this} m_parentCanvas:{m_parentCanvas} | m_touchControlManager:{m_touchControlManager}");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_hideStick)
            {
                m_joystickRect.gameObject.SetActive(false);
            }

            //EnhancedTouchSupport.Enable(); Moved to overarching TouchControls (should be in parent canvas)
            ETouch.Touch.onFingerDown += HandleFingerDown;
            ETouch.Touch.onFingerUp += HandleFingerUp;
            ETouch.Touch.onFingerMove += HandleFingerMove;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            ETouch.Touch.onFingerDown -= HandleFingerDown;
            ETouch.Touch.onFingerUp -= HandleFingerUp;
            ETouch.Touch.onFingerMove -= HandleFingerMove;
            //EnhancedTouchSupport.Disable(); Moved to overarching TouchControls (should be in parent canvas)
        }
        
        private void HandleFingerDown(Finger touchedFinger)
        {
            if (m_joystickFinger != null) return;
            
            if (m_touchControlManager.CheckIfTouched(touchedFinger.screenPosition, typeof(TouchButton))) return;
            if (m_parentCanvas == null)
            {
                Debug.LogWarning($"{this}: m_parentCanvas is null");
                return;
            }
            Vector2 touchScreenPosition = touchedFinger.screenPosition * 1f/m_parentCanvas.scaleFactor;

            switch(m_stickType)
            {
                case StickType.Fixed:
                    
                    if (Vector2.Distance(touchScreenPosition, m_joystickRect.anchoredPosition) > m_joystickSize.x/2f)
                    {
                    if (DEBUG) Debug.Log($"FAILED touch at {touchScreenPosition} | stick anchor {m_joystickRect.anchoredPosition}");
                        return;
                    }
                    if (DEBUG) Debug.Log($"SUCCESFUL touch at {touchScreenPosition} | stick anchor {m_joystickRect.anchoredPosition}");

                break;

                case StickType.Follow:
                case StickType.Floating:
                    var scaledDeadzone = m_hotspotDeadzone * 1f/m_parentCanvas.scaleFactor;
                    switch(m_hotspotPosition)
                    {
                        case ScreenHotspot.Left:
                            if (touchScreenPosition.x+Mathf.Abs(scaledDeadzone) >= Screen.width / 2f * 1f/m_parentCanvas.scaleFactor)
                                return;
                        break;

                        case ScreenHotspot.Right:
                            if (touchScreenPosition.x-Mathf.Abs(scaledDeadzone) <= Screen.width / 2f * 1f/m_parentCanvas.scaleFactor)
                                return;
                        break;
                        
                        default:
                            Debug.LogWarning("Invalid hotspotPosition configured");
                            return;
                    }
                    
                    
                    m_joystickRect.anchoredPosition = ClampStartPosition(touchScreenPosition);
                    m_displacementAmount = Vector2.zero;
                    
                break;
            }

    
            m_joystickFinger = touchedFinger;
            if (m_hideStick)    m_joystickRect.gameObject.SetActive(true);
            m_joystickRect.sizeDelta = m_joystickSize;
            JoystickHandleRect.sizeDelta *= m_joystickSize/m_joystickRect.sizeDelta;
            HandleFingerMove(touchedFinger);

            if (m_pressControl != null)
            {
                m_pressControl.EmulateInput(m_pressControlValue, 0.01f);
            }
        }

        private void HandleFingerMove(Finger movedFinger)
        {
            if (movedFinger == m_joystickFinger)
            {
                Vector2 knobPosition;
                float maxMovement = m_joystickSize.x / 2f ;
                ETouch.Touch currentTouch = movedFinger.currentTouch;
                Vector2 currentTouchScreenPosition = currentTouch.screenPosition * 1f/m_parentCanvas.scaleFactor;

                if (Vector2.Distance(currentTouchScreenPosition, m_joystickRect.anchoredPosition) > maxMovement)
                {
                    if (DEBUG) Debug.Log("Finger outside yolk");
                    if (m_stickType == StickType.Follow)
                    {
                        if (DEBUG) Debug.Log("TODO: Implement Follow stick");
                    }
                    knobPosition = (currentTouchScreenPosition - m_joystickRect.anchoredPosition).normalized * maxMovement;
                }
                else
                {
                    knobPosition = currentTouchScreenPosition - m_joystickRect.anchoredPosition;
                }

                JoystickHandleRect.anchoredPosition = knobPosition;
                m_displacementAmount = knobPosition / maxMovement;
                SendValueToControl(m_displacementAmount);
            }
        }

        private void HandleFingerUp(Finger lostFinger)
        {
            if (lostFinger != m_joystickFinger)
                return;
                
            m_joystickFinger = null;
            JoystickHandleRect.anchoredPosition = Vector2.zero;
            if (m_hideStick)    m_joystickRect.gameObject.SetActive(false);
            m_displacementAmount = Vector2.zero;
            SendValueToControl(Vector2.zero);
        
            if (m_releaseControl != null)
            {
                m_releaseControl.EmulateInput(m_releaseControlValue, 0.01f);
            }
        }


        /// <summary>
        /// "scaledStartPosition" must be in Cavnas coordinates
        /// </summary>
        /// <param name="scaledStartPosition"></param>
        /// <returns></returns>
        private Vector2 ClampStartPosition(Vector2 scaledStartPosition)
        {
            var scaledScreenSize = new Vector2(Screen.width * 1f/m_parentCanvas.scaleFactor, Screen.height * 1f/m_parentCanvas.scaleFactor);
            
            if (scaledStartPosition.x < m_joystickSize.x / 2)
            {
                scaledStartPosition.x = m_joystickSize.x / 2;
            }
            else if (m_hotspotPosition == ScreenHotspot.Right && scaledStartPosition.x > scaledScreenSize.x - m_joystickSize.x / 2)
            {
                scaledStartPosition.x = scaledScreenSize.x - m_joystickSize.x / 2;
            }

            if (scaledStartPosition.y < m_joystickSize.y / 2)
            {
                scaledStartPosition.y = m_joystickSize.y / 2;
            }
            else if (scaledStartPosition.y > scaledScreenSize.y - m_joystickSize.y / 2)
            {
                scaledStartPosition.y = scaledScreenSize.y - m_joystickSize.y / 2;
            }

            return scaledStartPosition ;
        }

        private void OnGUI()
        {
            if (DEBUG)
            {
                GUIStyle labelStyle = new GUIStyle()
                {
                    fontSize = 24,
                    normal = new GUIStyleState()
                    {
                        textColor = Color.white
                    }
                };
                if (m_joystickFinger != null)
                {
                    GUI.Label(new Rect(10, 35, 500, 20), $"Finger Start Position: {m_joystickFinger.currentTouch.startScreenPosition}", labelStyle);
                    GUI.Label(new Rect(10, 65, 500, 20), $"Finger Current Position: {m_joystickFinger.currentTouch.screenPosition}", labelStyle);
                }
                else
                {
                    GUI.Label(new Rect(10, 35, 500, 20), "No Current Movement Touch", labelStyle);
                }

                GUI.Label(new Rect(10, 10, 500, 20), $"Screen Size ({Screen.width}, {Screen.height})", labelStyle);
            }
            
        }


        #if UNITY_EDITOR
        [CustomEditor(typeof(TouchJoystick))]
        class TouchJoystickEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                TouchJoystick self = (TouchJoystick)target;

                serializedObject.Update();

                List<string> excludedProperties = new List<string>();

                if (self.m_stickType != StickType.Floating)
                {
                    excludedProperties.Add("m_hotspotPosition");
                    excludedProperties.Add("m_hotspotDeadzone");
                }

                if(ReferenceEquals(self.m_pressControl, null))
                {
                    excludedProperties.Add("m_pressControlRef");
                    excludedProperties.Add("m_pressControlValue");
                }

                if(ReferenceEquals(self.m_releaseControl, null))
                {
                    excludedProperties.Add("m_releaseControlRef");
                    excludedProperties.Add("m_releaseControlValue");
                }

                if (excludedProperties.Count == 0)
                {
                    DrawDefaultInspector();
                }
                else
                {
                    DrawPropertiesExcluding(serializedObject,excludedProperties.ToArray());
                }
                
                serializedObject.ApplyModifiedProperties();
            }
        }
        #endif
    }

}
