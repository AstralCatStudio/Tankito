using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.OnScreen;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

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
        private Vector2 JoystickSize = new Vector2(300, 300);
        [SerializeField]
        private RectTransform JoystickRect;
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

        private Finger JoystickFinger;
        private Vector2 DisplacementAmount;
        private Canvas m_parentCanvas;
        [SerializeField] private bool m_hideStick;
        [SerializeField] private StickType m_stickType;
        [SerializeField] private ScreenHotspot m_hotspotPosition;
        [SerializeField] private float m_hotspotDeadzone = 100f;

        public bool debugGUI;

        void Awake()
        {
            if (m_parentCanvas == null)
            {
                Canvas[] c = JoystickRect.GetComponentsInParent<Canvas>();
                m_parentCanvas = c[c.Length - 1];
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_hideStick)
            {
                JoystickRect.gameObject.SetActive(false);
            }

            EnhancedTouchSupport.Enable();
            ETouch.Touch.onFingerDown += HandleFingerDown;
            ETouch.Touch.onFingerUp += HandleLoseFinger;
            ETouch.Touch.onFingerMove += HandleFingerMove;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            ETouch.Touch.onFingerDown -= HandleFingerDown;
            ETouch.Touch.onFingerUp -= HandleLoseFinger;
            ETouch.Touch.onFingerMove -= HandleFingerMove;
            EnhancedTouchSupport.Disable();
        }

        // Ayuda no se como arreglar este bug nada funciona // void FixedUpdate()
        // Ayuda no se como arreglar este bug nada funciona // {
        // Ayuda no se como arreglar este bug nada funciona //     if (JoystickFinger != null && JoystickFinger.isActive)
        // Ayuda no se como arreglar este bug nada funciona //     {
        // Ayuda no se como arreglar este bug nada funciona //         SendValueToControl(DisplacementAmount);
        // Ayuda no se como arreglar este bug nada funciona //     }
        // Ayuda no se como arreglar este bug nada funciona // }

        private void HandleFingerMove(Finger MovedFinger)
        {
            if (MovedFinger == JoystickFinger)
            {
                Vector2 knobPosition;
                float maxMovement = JoystickSize.x / 2f ;
                ETouch.Touch currentTouch = MovedFinger.currentTouch;

                if (Vector2.Distance(currentTouch.screenPosition * 1f/m_parentCanvas.scaleFactor, JoystickRect.anchoredPosition) > maxMovement)
                {
                    Debug.Log("Finger outside yolk");
                    if (m_stickType == StickType.Follow)
                    {
                        Debug.Log("TODO: Implement Follow stick");
                    }
                    knobPosition = (currentTouch.screenPosition * 1f/m_parentCanvas.scaleFactor - JoystickRect.anchoredPosition).normalized * maxMovement;
                }
                else
                {
                    knobPosition = currentTouch.screenPosition * 1f/m_parentCanvas.scaleFactor - JoystickRect.anchoredPosition;
                }

                JoystickHandleRect.anchoredPosition = knobPosition;
                DisplacementAmount = knobPosition / maxMovement;
                Debug.Log($"Joystick displacement vector: {DisplacementAmount} | {DisplacementAmount.magnitude}");
                SendValueToControl(DisplacementAmount);
            }
        }

        private void HandleLoseFinger(Finger LostFinger)
        {
            if (LostFinger == JoystickFinger)
            {
                JoystickFinger = null;
                JoystickHandleRect.anchoredPosition = Vector2.zero;
                if (m_hideStick)    JoystickRect.gameObject.SetActive(false);
                DisplacementAmount = Vector2.zero;
                SendValueToControl(Vector2.zero);
            }
        }

        private void HandleFingerDown(Finger TouchedFinger)
        {
            if (JoystickFinger != null) return;

            switch(m_stickType)
            {
                case StickType.Fixed:
                    if (Vector2.Distance(TouchedFinger.screenPosition, JoystickRect.position) > JoystickSize.x/2f)
                        return;

                break;

                case StickType.Follow:
                case StickType.Floating:

                    switch(m_hotspotPosition)
                    {
                        case ScreenHotspot.Left:
                            if (TouchedFinger.screenPosition.x+Mathf.Abs(m_hotspotDeadzone * m_parentCanvas.scaleFactor) >= Screen.width / 2f)
                                return;
                        break;

                        case ScreenHotspot.Right:
                            if (TouchedFinger.screenPosition.x-Mathf.Abs(m_hotspotDeadzone * m_parentCanvas.scaleFactor) <= Screen.width / 2f)
                                return;
                        break;
                        
                        default:
                            Debug.LogWarning("Invalid hotspotPosition configured");
                            return;
                        //break;
                    }
                    
                    // NO FUNCIONA AYUDA!!!
                    JoystickRect.anchoredPosition = ClampStartPosition(TouchedFinger.screenPosition);
                    
                break;
            }

    
            JoystickFinger = TouchedFinger;
            DisplacementAmount = Vector2.zero;
            if (m_hideStick)    JoystickRect.gameObject.SetActive(true);
            JoystickRect.sizeDelta = JoystickSize;
            
        }

/// <summary>
/// "scaledStartPosition" must be in Cavnas coordinates
/// </summary>
/// <param name="scaledStartPosition"></param>
/// <returns></returns>
        private Vector2 ClampStartPosition(Vector2 scaledStartPosition) // NO FUNCIONA AYUDA!!!
        {
            var scaledJoystickSize =  JoystickSize * 1f/m_parentCanvas.scaleFactor;
            var scaledScreenSize = new Vector2(Screen.width * 1f/m_parentCanvas.scaleFactor, Screen.height * 1f/m_parentCanvas.scaleFactor);
            
            if (scaledStartPosition.x < scaledJoystickSize.x / 2)
            {
                scaledStartPosition.x = scaledJoystickSize.x / 2;
            }

            if (scaledStartPosition.y < scaledJoystickSize.y / 2)
            {
                scaledStartPosition.y = scaledJoystickSize.y / 2;
            }
            else if (scaledStartPosition.y > scaledScreenSize.y - scaledJoystickSize.y / 2)
            {
                scaledStartPosition.y = scaledScreenSize.y - scaledJoystickSize.y / 2;
            }

            return scaledStartPosition * 1f/m_parentCanvas.scaleFactor;
        }

        private void OnGUI()
        {
            if (debugGUI)
            {
                GUIStyle labelStyle = new GUIStyle()
                {
                    fontSize = 24,
                    normal = new GUIStyleState()
                    {
                        textColor = Color.white
                    }
                };
                if (JoystickFinger != null)
                {
                    GUI.Label(new Rect(10, 35, 500, 20), $"Finger Start Position: {JoystickFinger.currentTouch.startScreenPosition}", labelStyle);
                    GUI.Label(new Rect(10, 65, 500, 20), $"Finger Current Position: {JoystickFinger.currentTouch.screenPosition}", labelStyle);
                }
                else
                {
                    GUI.Label(new Rect(10, 35, 500, 20), "No Current Movement Touch", labelStyle);
                }

                GUI.Label(new Rect(10, 10, 500, 20), $"Screen Size ({Screen.width}, {Screen.height})", labelStyle);
            }
            
        }

        // private void OnDrawGizmosSelected()
        // {
        //     if (m_stickType == StickType.Floating)
        //     {
        //         
        //         float side = 0;
        //         switch (m_hotspotPosition)
        //         {
        //             case ScreenHotspot.Left:
        //             Gizmos.color = Color.blue;
        //             side = -1;
        //             break;
// 
        //             case ScreenHotspot.Right:
        //             Gizmos.color = Color.red;
        //             side = 1;
        //             break;
        //             
        //             default:
        //             Gizmos.color = Color.white;
        //             break;
        //         }
// 
        //         Vector3 hotspotSize = Camera.main.ScreenToWorldPoint(new Vector2 (Screen.width/2f - m_hotspotDeadzone, Screen.height)) - Camera.main.ScreenToWorldPoint(Vector2.zero);
        //         Debug.Log($"hotspotSize = {hotspotSize}");
        //         Vector3 hotspotCenter = Camera.main.transform.position + (Camera.main.ScreenToWorldPoint(new Vector2(Screen.width/2f + m_hotspotDeadzone,0)) - Camera.main.ScreenToWorldPoint(Vector2.zero))*side;
        //         Gizmos.DrawWireCube(hotspotCenter, hotspotSize);
        //     }       
        // }

        #if UNITY_EDITOR
        [CustomEditor(typeof(TouchJoystick))]
        class MyClassEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                TouchJoystick self = (TouchJoystick)target;
                serializedObject.Update();
                if (self.m_stickType == StickType.Floating)
                    DrawDefaultInspector();
                else {
                    DrawPropertiesExcluding(serializedObject,"m_hotspotPosition","m_hotspotDeadzone");
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
        #endif
    }

}
