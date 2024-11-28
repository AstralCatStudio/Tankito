using System.Runtime.InteropServices;
using UnityEngine;

public class CopyToClipboard : MonoBehaviour
{
    public void CopyTextToClipboard(string text)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        GUIUtility.systemCopyBuffer = text;
        WebGLCopyAndPasteAPI.passCopyToBrowser(text);
#else
        GUIUtility.systemCopyBuffer = text;
#endif
    }
}
