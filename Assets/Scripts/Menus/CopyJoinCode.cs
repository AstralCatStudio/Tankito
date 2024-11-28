using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;
using UnityEngine.UI;
using WebGLSupport;
using static System.Net.Mime.MediaTypeNames;

public class CopyJoinCode : MonoBehaviour
{
    private event KeyboardEventHandler OnKeyPressed;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(CopyCode);
    }

    private void CopyCode()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        KeyboardEvent copyEvent = new KeyboardEvent("c", 67, false, true, false);
        OnKeyPressed?.Invoke(null, copyEvent);
#else
        GUIUtility.systemCopyBuffer = GameManager.Instance.joinCode;
#endif
    }
}
