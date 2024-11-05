
namespace Tankito.Netcode.Simulation
{
    public interface ISimulationState
    {
        public void SetState(SimulationObject simObj);
        public void GetState(SimulationObject simObj);
    }
}