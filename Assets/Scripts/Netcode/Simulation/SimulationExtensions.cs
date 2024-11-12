using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public static class SimExtensions
    {
        public static void Foo()
        {
            Delta(default(TankSimulationState), default(TankSimulationState));
        }

        public static IStateDelta<TState> Delta<TState>(in TState a, in TState b) where TState : ISimulationState
        {
            if (a is TankSimulationState stateA && b is TankSimulationState stateB)
            {
                return ComputeTankStateDelta(in stateA, in stateB);

            }

            throw new InvalidOperationException("Delta computation not supported for this type.");
        }

        // Specialized ComputeDelta method for TankSimulationState
        private static TankStateDelta ComputeTankStateDelta(in TankSimulationState a, in TankSimulationState b)
        {
            float positionDiff = Vector2.SqrMagnitude(b.Position - a.Position);
            float hullRotationDiff = Mathf.Abs(b.HullRotation - a.HullRotation);
            float velocityDiff = Vector2.SqrMagnitude(b.Velocity - a.Velocity);
            float turretRotationDiff = Mathf.Abs(b.TurretRotation - a.TurretRotation);

            return new TankStateDelta(positionDiff, hullRotationDiff, velocityDiff, turretRotationDiff);
        }
    }
}