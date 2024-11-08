using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.UI;
using UnityEngine;

public class EventSystemEnabler : MonoBehaviour
{
    private InputSystemUIInputModule _inputSystemUIInputModule;

    void Start()
    {
        _inputSystemUIInputModule = GetComponentInParent<InputSystemUIInputModule>();
    }

    private void OnEnable()
    {
        StartCoroutine(Co_ActivateInputComponent());
    }

    private IEnumerator Co_ActivateInputComponent()
    {
        yield return new WaitForEndOfFrame();
        _inputSystemUIInputModule.enabled = false;
        yield return new WaitForSeconds(0.2f);
        _inputSystemUIInputModule.enabled = true;
    }
}
