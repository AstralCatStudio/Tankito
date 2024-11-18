using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardPerceptions")]
public class AliadoEnRango : UnityPerception
{
    private Transform aliado; // Referencia al aliado
    private float rangoDeteccion = 10f; // Rango de detección del aliado
    private string tagAliado = "Aliado"; // Tag para identificar al aliado

    protected override void OnSetContext()
    {
        // Busca al aliado usando el tag
        GameObject aliadoObj = GameObject.FindWithTag(tagAliado);
        if (aliadoObj != null)
        {
            aliado = aliadoObj.transform;
            Debug.Log("AliadoEnRango: Aliado encontrado correctamente.");
        }
        else
        {
            Debug.LogWarning("AliadoEnRango: No se encontró un GameObject con el tag 'Aliado'.");
        }
    }

    public override void Initialize()
    {
        Debug.Log("AliadoEnRango: Percepción inicializada.");
    }

    public override bool Check()
    {
        if (aliado == null)
        {
            Debug.LogWarning("AliadoEnRango: No se encontró al aliado.");
            return false;
        }

        // Calcula la distancia en el plano 2D (X-Y)
        float distancia = Vector2.Distance(context.Transform.position, aliado.position);
        bool enRango = distancia <= rangoDeteccion;

        Debug.Log($"AliadoEnRango: Distancia al aliado = {distancia}, En rango = {enRango}");
        return enRango;
    }

    public override void Reset()
    {
        Debug.Log("AliadoEnRango: Reiniciando percepción.");
    }
}
