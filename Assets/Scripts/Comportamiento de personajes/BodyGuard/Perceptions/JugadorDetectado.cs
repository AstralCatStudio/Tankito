using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardPerceptions")]
public class JugadorDetectado : UnityPerception
{
    private Transform jugador; // Referencia al transform del jugador
    private float rangoDeteccion = 4f; // Rango de detecci�n del jugador

    protected override void OnSetContext()
    {
        jugador = GameObject.FindWithTag("Player")?.transform;
        if (jugador == null)
        {
            Debug.LogWarning("JugadorDetectado: No se encontr� un GameObject con el tag 'Player'.");
        }

        Debug.Log("ONSETCONTEXT");
    }

    public override void Initialize()
    {
        Debug.Log("JugadorDetectado: Percepci�n inicializada.");
    }

    public override bool Check()
    {
        if (jugador == null)
        {
            Debug.LogWarning("JugadorDetectado: Jugador no est� definido. �Est� configurado el tag correctamente?");
            return false;
        }

        // C�lculo de distancia ignorando el eje Y (para juegos top-down)
        float distancia = Vector3.Distance(
            new Vector3(context.Transform.position.x, 0, context.Transform.position.z),
            new Vector3(jugador.position.x, 0, jugador.position.z)
        );

        bool detectado = distancia <= rangoDeteccion;

        Debug.Log($"JugadorDetectado: Distancia al jugador = {distancia}, Detectado = {detectado}");
        return detectado;
    }

    public override void Reset()
    {
        Debug.Log("JugadorDetectado: Reiniciando percepci�n.");
    }
}
