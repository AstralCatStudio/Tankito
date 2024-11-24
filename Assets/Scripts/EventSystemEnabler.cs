using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemEnabler : MonoBehaviour
{
    void Update()
    {
        Debug.Log("EventSystem current input module: " + EventSystem.current.currentInputModule.name);
    }

    [ContextMenu("Test UpdateEventSystemModules")]
    public void TestUpdateEventSystemModules()
    {
        EventSystem.current.UpdateModules();
    }
}
