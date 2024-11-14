using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public static class SimExtensions
    {
        public static IStateDelta<ISimulationState>[] Delta(in SimulationSnapshot snapA, in SimulationSnapshot snapB)
        {
            IStateDelta<ISimulationState>[] snapshotDeltas = new IStateDelta<ISimulationState>[Math.Max(snapA.objectStates.Count, snapB.objectStates.Count)];
            int i = 0;

            foreach(var obj in snapA.objectStates.Keys)
            {
                if (snapB.objectStates.ContainsKey(obj))
                {
                    snapshotDeltas[i] = Delta(snapA[obj], snapB[obj]);
                }
                else
                {
                    snapshotDeltas[i] = (IStateDelta<ISimulationState>)snapA[obj];
                }
            }

            return snapshotDeltas;
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
        private static TankDelta ComputeTankStateDelta(in TankSimulationState a, in TankSimulationState b)
        {
            float positionDiff = Vector2.SqrMagnitude(b.Position - a.Position);
            float hullRotationDiff = Mathf.Abs(b.HullRotation - a.HullRotation);
            float velocityDiff = Vector2.SqrMagnitude(b.Velocity - a.Velocity);
            float turretRotationDiff = Mathf.Abs(b.TurretRotation - a.TurretRotation);

            return new TankDelta(positionDiff, hullRotationDiff, velocityDiff, turretRotationDiff);
        }

        private static BulletDelta ComputeBulletStateDelta(in BulletSimulationState a, in BulletSimulationState b)
        {
            float positionDiff = Vector2.SqrMagnitude(b.Position - a.Position);
            float rotationDiff = Mathf.Abs(b.Rotation - a.Rotation);
            float velocityDiff = Vector2.SqrMagnitude(b.Velocity - a.Velocity);

            return new BulletDelta(positionDiff, rotationDiff, velocityDiff);
        }

        public static bool CompareDeltas<TState>(in IStateDelta<TState> a, in IStateDelta<TState> b) where TState : ISimulationState
        {
            if (a is TankDelta tDeltaA && b is TankDelta tDeltaB)
            {
                return CompareTankStateDeltas(tDeltaA, tDeltaA);
            }
            else if (a is BulletDelta bDeltaA && b is BulletDelta bDeltaB)
            {
                return CompareBulletStateDeltas(bDeltaA, bDeltaB);
            }

            throw new InvalidOperationException("CompareDeltas computation not supported for this type.");
        }

        private static bool CompareTankStateDeltas(TankDelta a, TankDelta b)
        {
            return (a.posDiff > b.posDiff || a.hullRotDiff > b.hullRotDiff || a.velDiff > b.velDiff || a.turrRotDiff > b.turrRotDiff);
        }

        private static bool CompareBulletStateDeltas(BulletDelta a, BulletDelta b)
        {
            return (a.posDiff > b.posDiff || a.rotDiff > b.rotDiff || a.velDiff > b.velDiff);
        }
    }
}