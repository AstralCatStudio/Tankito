using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public interface IStateDelta<TState> where TState : ISimulationState
    {
        
    }

    [Serializable]
    public readonly struct TankDelta : IStateDelta<TankSimulationState>
    {
        public readonly float posDiff;
        public readonly float hullRotDiff;
        public readonly float velDiff;
        public readonly float turrRotDiff;

        public TankDelta(float posDiff, float hullRotDiff, float velDiff, float turrRotDiff)
        {
            this.posDiff = posDiff;
            this.hullRotDiff = hullRotDiff;
            this.velDiff = velDiff;
            this.turrRotDiff = turrRotDiff;
        }

        public override string ToString()
        {
            return "[ Δpos:" + posDiff + " | Δvel:" + velDiff + " | Δhull_rot" + hullRotDiff + " | Δturr_rot:" + turrRotDiff + " ]";
        }
    }

    [Serializable]
    public readonly struct BulletDelta : IStateDelta<BulletSimulationState>
    {
        public readonly float posDiff;
        public readonly float rotDiff;
        public readonly float velDiff;

        public BulletDelta(float posDiff, float rotDiff, float velDiff)
        {
            this.posDiff = posDiff;
            this.rotDiff = rotDiff;
            this.velDiff = velDiff;
        }
        public override string ToString()
        {
            return "[ Δpos:" + posDiff + " | Δvel:" + velDiff + " | Δrot" + rotDiff + " ]";
        }
    }

}