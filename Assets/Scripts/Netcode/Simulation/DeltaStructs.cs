using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public interface IStateDelta<TState> where TState : ISimulationState
    {
        
    }

    public readonly struct TankStateDelta : IStateDelta<TankSimulationState>
    {
        public readonly float posDiff;
        public readonly float hullRotDiff;
        public readonly float velDiff;
        public readonly float turrRotDiff;

        public TankStateDelta(float posDiff, float hullRotDiff, float velDiff, float turrRotDiff)
        {
            this.posDiff = posDiff;
            this.hullRotDiff = hullRotDiff;
            this.velDiff = velDiff;
            this.turrRotDiff = turrRotDiff;
        }
    }
}