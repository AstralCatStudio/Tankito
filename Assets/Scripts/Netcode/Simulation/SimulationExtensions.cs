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
            if (a is TankSimulationState tstateA && b is TankSimulationState tstateB)
            {
                return (IStateDelta<TState>)(object) ComputeTankStateDelta(in tstateA, in tstateB);

            }
            else if(a is BulletSimulationState bstateA && b is BulletSimulationState bstateB)
            {
                return (IStateDelta<TState>)(object)ComputeBulletStateDelta(in bstateA, in bstateB);
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

        private static BulletStateDelta ComputeBulletStateDelta(in BulletSimulationState a, in BulletSimulationState b)
        {
            float positionDiff = Vector2.SqrMagnitude(b.Position - a.Position);
            float rotationDiff = Mathf.Abs(b.Rotation - a.Rotation);
            float velocityDiff = Vector2.SqrMagnitude(b.Velocity - a.Velocity);

            return new BulletStateDelta(positionDiff, rotationDiff, velocityDiff);
        }

        public static bool CompareDeltas<TState>(in IStateDelta<TState> a, in IStateDelta<TState> b) where TState : ISimulationState
        {
            if (a is TankStateDelta tDeltaA && b is TankStateDelta tDeltaB)
            {
                return CompareTankStateDeltas(tDeltaA, tDeltaA);
            }
            else if (a is BulletStateDelta bDeltaA && b is BulletStateDelta bDeltaB)
            {
                return CompareBulletStateDeltas(bDeltaA, bDeltaB);
            }

            throw new InvalidOperationException("CompareDeltas computation not supported for this type.");
        }

        private static bool CompareTankStateDeltas(TankStateDelta a, TankStateDelta b)
        {
            return (a.posDiff > b.posDiff || a.hullRotDiff > b.hullRotDiff || a.velDiff > b.velDiff || a.turrRotDiff > b.turrRotDiff);
        }

        private static bool CompareBulletStateDeltas(BulletStateDelta a, BulletStateDelta b)
        {
            return (a.posDiff > b.posDiff || a.rotDiff > b.rotDiff || a.velDiff > b.velDiff);
        }
    }
}