using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardActions")]
public class PatrullarAction : UnityAction
{
    public Transform[] puntosPatrulla; // Array de puntos de patrulla asignados en el editor
    private int indiceActual = 0;      // Índice del punto de patrullaje actual
    private float velocidad = 3f;      // Velocidad de movimiento del NPC

    // Inicializa referencias antes de la ejecución
    protected override void OnSetContext()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
        {
            Debug.LogWarning("PatrullarAction: No se han asignado puntos de patrullaje.");
        }
    }

    // Configuración inicial al iniciar la acción
    public override void Start()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
        {
            Debug.LogError("PatrullarAction: No hay puntos de patrullaje asignados. Acción fallida.");
            return;
        }

        Debug.Log("PatrullarAction: Iniciando patrullaje.");
        indiceActual = 0; // Empezamos desde el primer punto
    }

    // Lógica principal que se ejecuta cada frame mientras la acción está activa
    public override Status Update()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
        {
            return Status.Failure; // La acción falla si no hay puntos asignados
        }

        // Obtenemos el punto de patrulla actual
        Transform puntoActual = puntosPatrulla[indiceActual];

        // Movemos el NPC hacia el punto actual
        context.Transform.position = Vector3.MoveTowards(
            context.Transform.position,
            puntoActual.position,
            velocidad * Time.deltaTime
        );

        // Si alcanzamos el punto actual, pasamos al siguiente
        if (Vector3.Distance(context.Transform.position, puntoActual.position) < 0.5f)
        {
            Debug.Log($"PatrullarAction: Punto alcanzado {indiceActual + 1}/{puntosPatrulla.Length}.");
            indiceActual = (indiceActual + 1) % puntosPatrulla.Length; // Pasamos al siguiente punto de forma cíclica
        }

        return Status.Running; // La acción sigue ejecutándose
    }

    // Lógica al detener la acción
    public override void Stop()
    {
        Debug.Log("PatrullarAction: Patrullaje detenido.");
    }
}
