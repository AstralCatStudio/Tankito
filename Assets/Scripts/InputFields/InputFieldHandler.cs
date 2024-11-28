using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;

namespace Tankito.InputFields
{
    public class InputFieldHandler : MonoBehaviour
    {
        protected TMP_InputField inputField;

        public UnityEvent OnEnterPressedEvent;

        protected virtual void Start()
        {
            inputField = GetComponent<TMP_InputField>();
            if(inputField != null )
            {
                inputField.onSubmit.AddListener(OnEnterPressed);
            }
            else
            {
                Debug.LogWarning("Input field component not found");
            }
        }

        protected virtual void OnDestroy()
        {
            if(OnEnterPressedEvent != null)
            {
                OnEnterPressedEvent.RemoveAllListeners();
            }

            if (inputField != null)
            {
                inputField.onSubmit.RemoveListener(OnEnterPressed);
            }
        }

        public virtual void OnEnterPressed(string text)
        {
            Debug.Log($"Enter pressed on TMP_InputField. Text: {text}");
            OnEnterPressedEvent?.Invoke(); // Invoca evento publico
        }
    }
}


