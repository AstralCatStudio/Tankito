using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;

public class CommonBehaviours : MonoBehaviour
{

    private int indiceActual = 0;
    public Vector3[] puntosDePatrulla;



    ////////////////////////////////////////////////////////////////////////////////
    /////////////////////////// LLAMADAS CONCRETAS /////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////// BODY GUARD //////////////////////////////////////////////

    public void PatrolBodyGuard()
    {
        Patrol(puntosDePatrulla,5,10);
        Debug.Log("Patrol Body Guard");
    }

    public void SeekPlayer()
    {
        Seek("Player", 5, 1);
        Debug.Log("Seek Player");
    }
    
    public void DetectPlayer()
    {
        Detect("Player", 10);
        Debug.Log("Detect Player");
    }





    ////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////// ACCCIONES //////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PATROL

    public void Patrol(Vector3[] puntosPatrulla, float velocidad, float distanciaMinima)
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
        {
            Debug.LogWarning("Patrol: No se han definido puntos de patrulla.");
            return;
        }

        // Obtiene el punto actual
        Vector3 puntoActual = puntosPatrulla[indiceActual];

        // Calcula la distancia al punto actual
        float distancia = Vector3.Distance(transform.position, puntoActual);

        // Si se ha alcanzado el punto actual, pasa al siguiente
        if (distancia <= distanciaMinima)
        {
            Debug.Log($"Patrol: Punto alcanzado. Pasando al siguiente punto. �ndice actual: {indiceActual}");
            indiceActual = (indiceActual + 1) % puntosPatrulla.Length; // Ciclo infinito entre los puntos
            return;
        }

        // Mueve hacia el punto actual
        Move(puntoActual, velocidad);
    }




    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////// SEEK
    public void Seek(string targetTag, float velocidad, float distanciaMinima)
    {
        // Busca el objeto con el tag especificado
        GameObject objetivo = GameObject.FindWithTag(targetTag);
        if (objetivo == null)
        {
            Debug.LogWarning($"Seek: No se encontr� ning�n objeto con el tag '{targetTag}'.");
            return;
        }

        // Calcula la distancia al objetivo
        float distancia = Vector2.Distance(transform.position, objetivo.transform.position);

        // Si est� dentro de la distancia m�nima, no realizar movimiento
        if (distancia <= distanciaMinima)
        {
            Debug.Log($"Seek: Objetivo alcanzado. Distancia = {distancia}, Distancia m�nima = {distanciaMinima}");
            return;
        }

        // Mueve hacia el objetivo
        Move(objetivo.transform.position, velocidad);
    }









    ////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////// PERCEPCIONES /////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////// DETECT
    public bool Detect(string targetTag, float rango)
    {
        // Encuentra todos los objetos con el tag dado
        GameObject[] objetosConTag = GameObject.FindGameObjectsWithTag(targetTag);

        // Recorre los objetos para verificar si est�n dentro del rango
        foreach (GameObject objeto in objetosConTag)
        {
            float distancia = Vector2.Distance(transform.position, objeto.transform.position);

            if (distancia <= rango)
            {
                Debug.Log($"DetectInRange: Objeto con tag '{targetTag}' detectado dentro del rango ({distancia} <= {rango}).");
                return true; // Al menos un objeto est� en el rango
            }
        }

        Debug.Log($"DetectInRange: No se encontraron objetos con tag '{targetTag}' dentro del rango.");
        return false; // Ning�n objeto est� en el rango
    }





















    ////////////////////////////////////////////////////////////////////////////////
    /////////////////////////// METODOS AUXILIARES /////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////

    // M�todo privado para mover el GameObject hacia una posici�n
    private void Move(Vector3 posicion, float velocidad)
    {
        // Calcula la direcci�n en el plano 2D (X-Y)
        Vector2 direccion = (posicion - transform.position).normalized;

        // Calcula la nueva posici�n
        Vector2 nuevaPosicion = (Vector2)transform.position + direccion * velocidad * Time.deltaTime;

        // Actualiza la posici�n del GameObject
        transform.position = new Vector3(nuevaPosicion.x, nuevaPosicion.y, transform.position.z);

        // Gira hacia el objetivo
        GirarHacia(posicion);
    }

    // M�todo privado para girar hacia una posici�n
    private void GirarHacia(Vector3 posicionObjetivo)
    {
        // Calcula la direcci�n hacia el objetivo
        Vector2 direccion = (posicionObjetivo - transform.position).normalized;

        // Calcula el �ngulo en el plano X-Y
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        // Ajusta la rotaci�n del objeto
        transform.rotation = Quaternion.Euler(0, 0, angulo);
    }
}
