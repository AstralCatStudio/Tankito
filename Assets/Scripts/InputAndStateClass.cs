using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientInputState<T>
{
    public T input;
    public int simulationFrame;
    public float fixedDeltaTime;
}

public class SimulationState<T>
{
    public List<T> simulationList = new List<T>();
    public int simulationFrame;

    /*public SimulationState(List<T> list, int simulationFrame)
    {
        simulationList = new List<T>(list);
        this.simulationFrame = simulationFrame;
    }*/

}
