using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.UnityToolkit;

[SelectionGroup("BodyGuardActions")]
public class ProtegerAliado : UnityAction
{
    private Transform aliado; // Referencia al aliado
    private string tagBala = "Bala"; // Tag para identificar balas
    private float rangoDeteccionBala = 5f; // Rango para detectar balas cerca del aliado
    private float velocidad = 3f; // Velocidad de movimiento del NPC

    protected override void OnSetContext()
    {
        GameObject aliadoObj = GameObject.FindWithTag("Aliado");
        if (aliadoObj != null)
        {
            aliado = aliadoObj.transform;
            Debug.Log("ProtegerAliado: Aliado encontrado correctamente.");
        }
        else
        {
            Debug.LogWarning("ProtegerAliado: No se encontró un GameObject con el tag 'Aliado'.");
        }
    }

    public override void Start()
    {
        Debug.Log("ProtegerAliado: Iniciando acción para proteger al aliado.");
    }

    public override Status Update()
    {
        if (aliado == null)
        {
            Debug.LogWarning("ProtegerAliado: Aliado no está definido. Terminando acción.");
            return Status.Failure;
        }

        // Detecta balas cerca del aliado
        Collider2D[] objetosCercanos = Physics2D.OverlapCircleAll(aliado.position, rangoDeteccionBala);
        foreach (Collider2D obj in objetosCercanos)
        {
            if (obj.CompareTag(tagBala))
            {
                // Mueve el NPC para interceptar la bala
                Vector2 direccion = (obj.transform.position - context.Transform.position).normalized;
                context.Transform.position += (Vector3)direccion * velocidad * Time.deltaTime;
                Debug.Log("ProtegerAliado: Moviéndose para interceptar la bala.");
                return Status.Running;
            }
        }

        Debug.Log("ProtegerAliado: No hay balas para interceptar.");
        return Status.Success; // No hay balas, el NPC no necesita moverse
    }

    public override void Stop()
    {
        Debug.Log("ProtegerAliado: Acción detenida.");
    }
}
