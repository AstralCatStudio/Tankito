using System.Collections;
using System.Collections.Generic;
using Tankito;
using Tankito.Netcode;
using UnityEngine;

public class TankSinglePlayerInput : TankPlayerInput
{
    public override InputPayload GetInput()
    {
        Aim();
        InputPayload gotInput = GetCurrentInput();
        SetCurrentAction(TankAction.None);
        return gotInput;
    }
}
