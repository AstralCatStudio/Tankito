using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShellSelection : MonoBehaviour
{
    public bool selected;
    public AnimatedModifiers modifiersObject;

    private void Awake()
    {
        modifiersObject = FindObjectOfType<AnimatedModifiers>();
        if (modifiersObject == null)
        {
            Debug.LogWarning("Objeto no encontrado");
        }
    }
    public void Select()
    {
        modifiersObject.DeselectAllModifiers();
        gameObject.GetComponent<Outline>().enabled = true;
        selected = true;
    }
}
