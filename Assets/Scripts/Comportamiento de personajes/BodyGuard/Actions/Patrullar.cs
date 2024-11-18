using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardActions")]
public class Patrullar : UnityAction
{
    public Transform[] puntosPatrulla;
    private int indiceActual = 0;
    private float velocidad = 3f;


    protected override void OnSetContext()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
        {
            Debug.LogWarning("PatrullarAction: No se han asignado puntos de patrullaje.");
        }
    }


    public override void Start()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
        {
            Debug.LogError("PatrullarAction: No hay puntos de patrullaje asignados. Acción fallida.");
            return;
        }

        //Debug.Log("PatrullarAction: Iniciando patrullaje.");
        indiceActual = 0;
    }



    public override Status Update()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
        {
            return Status.Failure; // Sin puntos
        }


        Transform puntoActual = puntosPatrulla[indiceActual];


        context.Transform.position = Vector3.MoveTowards(
            context.Transform.position,
            puntoActual.position,
            velocidad * Time.deltaTime
        );

        if (Vector3.Distance(context.Transform.position, puntoActual.position) < 0.5f)
        {
            //Debug.Log($"PatrullarAction: Punto alcanzado {indiceActual + 1}/{puntosPatrulla.Length}.");
            indiceActual = (indiceActual + 1) % puntosPatrulla.Length; // Pasamos al siguiente punto 
        }

        return Status.Running;
    }

    // detener la acción
    public override void Stop()
    {
        Debug.Log("PatrullarAction: Patrullaje detenido.");
    }
}
