using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    public interface IModifierEvent
    {
        void StartEvent(ABullet bullet)
        {

        }
        void StartEvent()
        {

        }
    }
}