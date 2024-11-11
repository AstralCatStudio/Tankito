using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Tankito
{
    [System.Serializable]
    public abstract class AModifierEvent : ScriptableObject, IModifierEvent
    {
        public abstract void StartEvent(ABullet bullet);
    }
}
