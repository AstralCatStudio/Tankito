using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public interface IStateDelta<TState> where TState : ISimulationState
    {
        
    }

    [Serializable]
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

    [Serializable]
    public readonly struct BulletStateDelta : IStateDelta<BulletSimulationState>
    {
        public readonly float posDiff;
        public readonly float rotDiff;
        public readonly float velDiff;

        public BulletStateDelta(float posDiff, float rotDiff, float velDiff)
        {
            this.posDiff = posDiff;
            this.rotDiff = rotDiff;
            this.velDiff = velDiff;
        }
    }
}