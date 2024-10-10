using UnityEngine;
using System.Collections.Generic;

public class DebugConsole : MonoBehaviour
{
    private Queue<string> logMessages = new Queue<string>();
    private string myLog = "";
    private bool showConsole = true;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLog = logString;
        logMessages.Enqueue(myLog);

        if (logMessages.Count > 10) // Limitar a 10 mensajes
        {
            logMessages.Dequeue();
        }
    }

    void OnGUI()
    {
        if (showConsole)
        {
            GUILayout.BeginVertical();
            foreach (string log in logMessages)
            {
                GUILayout.Label(log);
            }
            GUILayout.EndVertical();
        }

        if (GUILayout.Button("Toggle Console"))
        {
            showConsole = !showConsole;
        }
    }
}
