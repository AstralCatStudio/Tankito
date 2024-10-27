using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito.Utils
{
    /// <summary>
    /// Utilizar esta clase como padre de objetos instanciados en partida (para evitar que se carguen en otras escenas y asegurar que su lifetime es el mismo de la escena en la que se encuentra el Instance Parent)
    /// </summary>
    public class GameInstanceParent : Singleton<GameInstanceParent>
    {
        
    }

}