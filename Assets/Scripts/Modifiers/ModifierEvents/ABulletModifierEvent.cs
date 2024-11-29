using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;

public abstract class ABulletModifierEvent : ScriptableObject, IModifierEvent
{
    public abstract void StartEvent(ABulletController bullet);
}
