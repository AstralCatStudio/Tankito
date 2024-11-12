using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITolerance 
{
    public bool CheckTolerances(ITolerance diff, ITolerance tolerance);
}
