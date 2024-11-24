using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.UI;
using UnityEngine;

public class EventSystemEnabler : MonoBehaviour
{
    private InputSystemUIInputModule _inputSystemUIInputModule;

    void Awake()
    {
        _inputSystemUIInputModule = GetComponentInParent<InputSystemUIInputModule>();
    }

    private IEnumerator RefreshInputSystemUIInputModule()
    {
        yield return new WaitForEndOfFrame();
        _inputSystemUIInputModule.enabled = false;
        yield return new WaitForEndOfFrame();
        _inputSystemUIInputModule.enabled = true;
    }
}
