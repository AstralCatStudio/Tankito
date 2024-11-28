using System.Runtime.InteropServices;
using UnityEngine;

public class CopyToClipboard : MonoBehaviour
{
    // Función de JavaScript que copia al portapapeles
    [DllImport("__Internal")]
    private static extern void passCopyToBrowser(string text);

    public void CopyTextToClipboard(string text)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        passCopyToBrowser(text);  // Llama a la función JavaScript para copiar el texto
#else
        // En plataformas que no sean WebGL, usa el enfoque estándar
        TextEditor te = new TextEditor();
        te.text = text;
        te.SelectAll();
        te.Copy();
#endif
    }
}
