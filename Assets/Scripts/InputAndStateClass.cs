using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientInputState<T>
{
    public T input;
    public int simulationFrame;
}

public class SimulationState<T> where T: Object
{
    public List<T> simulationList;
    public int simulationFrame;
}
