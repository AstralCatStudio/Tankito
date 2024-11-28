using System.Runtime.InteropServices;
using UnityEngine;

public class CopyToClipboard : MonoBehaviour
{
    // Funci�n de JavaScript que copia al portapapeles
    [DllImport("__Internal")]
    private static extern void passCopyToBrowser(string text);

    public void CopyTextToClipboard(string text)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        passCopyToBrowser(text);  // Llama a la funci�n JavaScript para copiar el texto
#else
        // En plataformas que no sean WebGL, usa el enfoque est�ndar
        TextEditor te = new TextEditor();
        te.text = text;
        te.SelectAll();
        te.Copy();
#endif
    }
}
