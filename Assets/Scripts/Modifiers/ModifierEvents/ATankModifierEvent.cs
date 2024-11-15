using System.Collections;
using System.Collections.Generic;
using Tankito;
using Tankito.Netcode;
using UnityEngine;

public abstract class ATankModifierEvent : ScriptableObject, IModifierEvent
{
    public abstract void StartEvent(TankController tank);
}
