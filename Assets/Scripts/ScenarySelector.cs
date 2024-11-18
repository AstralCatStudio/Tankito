using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ScenarySelector : NetworkBehaviour
{
    private int _currentScenary;
    private List<GameObject> _scenaries;

    void Start()
    {
        // Activar la ui del escenario en el server
        // Pillar los hijos, activar el primero y desactivar el resto

    }

    private void ChangeScenaryLeft()
    {
        // Desplaza a la izquierda el array de escenarios
        // Llama al spawn manager
    }

    private void ChangeScenaryRight()
    {
        // Desplaza a la derecha el array de escenarios
        // Llama al spawn manager
    }

    private void DisableScenarySelection()
    {
        // Desactivar lo relativo a la UI e incluso este script
    }
}
