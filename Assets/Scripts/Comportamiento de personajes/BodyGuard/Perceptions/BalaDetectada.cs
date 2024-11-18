using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardPerceptions")]
public class BalaDetectada : UnityPerception
{
    private Transform aliado;
    private string tagBala = "Bala";
    private float rangoDeteccionBala = 5f; // Rango para detectar balas cerca del aliado

    protected override void OnSetContext()
    {
        GameObject aliadoObj = GameObject.FindWithTag("NPC");
        if (aliadoObj != null)
        {
            aliado = aliadoObj.transform;
        }
        else
        {
            Debug.LogWarning("BalaDetectada: No se encontró un GameObject con el tag 'Aliado'.");
        }
    }

    public override void Initialize()
    {
        Debug.Log("BalaDetectada: Percepción inicializada.");
    }

    public override bool Check()
    {
        if (aliado == null) return false;

        Collider2D[] objetosCercanos = Physics2D.OverlapCircleAll(aliado.position, rangoDeteccionBala);
        foreach (Collider2D obj in objetosCercanos)
        {
            if (obj.CompareTag(tagBala))
            {
                Debug.Log("BalaDetectada: Bala encontrada cerca del aliado.");
                return true;
            }
        }

        Debug.Log("BalaDetectada: No se encontraron balas cerca del aliado.");
        return false;
    }

    public override void Reset()
    {
        Debug.Log("BalaDetectada: Reiniciando percepción.");
    }
}
