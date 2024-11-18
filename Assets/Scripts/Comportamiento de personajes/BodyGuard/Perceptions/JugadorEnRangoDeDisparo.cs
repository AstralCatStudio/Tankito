using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardPerceptions")]
public class JugadorEnRangoDeDisparo : UnityPerception
{
    private Transform jugador;
    private float rangoDisparo = 10f; // Rango de disparo del NPC

    protected override void OnSetContext()
    {
        GameObject jugadorObj = GameObject.FindWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
        else
        {
            Debug.LogWarning("JugadorEnRangoDeDisparo: No se encontró un GameObject con el tag 'Player'.");
        }
    }

    public override void Initialize()
    {
        Debug.Log("JugadorEnRangoDeDisparo: Percepción inicializada.");
    }

    public override bool Check()
    {
        if (jugador == null) return false;

        float distancia = Vector2.Distance(context.Transform.position, jugador.position);
        bool enRango = distancia <= rangoDisparo;

        Debug.Log($"JugadorEnRangoDeDisparo: Distancia al jugador = {distancia}, En rango = {enRango}");
        return enRango;
    }

    public override void Reset()
    {
        Debug.Log("JugadorEnRangoDeDisparo: Reiniciando percepción.");
    }
}
