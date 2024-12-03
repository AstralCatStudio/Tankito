using System;
using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShellSelection : MonoBehaviour
{
    public bool selected;
    public bool alreadyTaken;
    public Modifier modifier;
    public AnimatedModifiers modifiersObject;

    private void Awake()
    {
        modifiersObject = FindObjectOfType<AnimatedModifiers>();
        if (modifiersObject == null)
        {
            Debug.LogWarning("Objeto no encontrado");
        }
    }

    private void OnEnable()
    {
        modifier = modifiersObject.modifierList.GetModifier(UnityEngine.Random.Range(0, 11));
        GetComponent<ShellAnimation>().modifier.transform.GetChild(0).GetComponent<Image>().sprite = modifier.GetSprite();  //obtiene el sprite del modificador
        GetComponent<ShellAnimation>().description.GetComponent<TextMeshProUGUI>().text = modifier.GetDescription();   //obtiene la descripcion
    }
    public void Select()
    {
        if(!alreadyTaken)
        {
            modifiersObject.DeselectAllModifiers();
            gameObject.GetComponent<Outline>().enabled = true;
            selected = true;
        } else
        {
            Debug.LogWarning("This modifier is already taken.");
        }
        
    }
}
