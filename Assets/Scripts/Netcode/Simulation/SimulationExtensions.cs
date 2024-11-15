using System;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public static class SimExtensions
    {
        public static IStateDelta[] Delta(in SimulationSnapshot snapA, in SimulationSnapshot snapB)
        {
            IStateDelta[] snapshotDeltas = new IStateDelta[Math.Max(snapA.Count, snapB.Count)];
            int i = 0;

            foreach(var obj in snapA.Keys)
            {
                if (snapB.ContainsKey(obj))
                {
                    snapshotDeltas[i] = Delta(snapA[obj], snapB[obj]);
                }
                else
                {
                    snapshotDeltas[i] = Delta(snapA[obj]);
                }
            }

            throw new NotImplementedException("TODO: falta recoger los objetos que estan presentes en el snapshot B pero no en el A");

            return snapshotDeltas;
        }

        public static IStateDelta Delta<TState>(in TState a, in TState b) where TState : ISimulationState
        {
            if (a is TankSimulationState tankStateA && b is TankSimulationState tankStateB)
            {
                return ComputeTankStateDelta(in tankStateA, in tankStateB);

            }
            else if(a is BulletSimulationState bulletStateA && b is BulletSimulationState bulletStateB)
            {
                return ComputeBulletStateDelta(in bulletStateA, in bulletStateB);
            }

            throw new InvalidOperationException($"Delta computation not supported for type{typeof(TState)}.");
        }
        public static IStateDelta Delta<TState>(in TState state) where TState : ISimulationState
        {
            if (state is TankSimulationState tankState)
            {
                return new TankDelta(in tankState);

            }
            else if(state is BulletSimulationState bulletState)
            {
                return new BulletDelta(in bulletState);
            }

            throw new InvalidOperationException($"Delta constructor not supported for type{typeof(TState)}.");
        }

        // Specialized ComputeDelta method for TankSimulationState
        private static TankDelta ComputeTankStateDelta(in TankSimulationState a, in TankSimulationState b)
        {
            Vector2 positionDiff = b.Position - a.Position;
            float hullRotationDiff = b.HullRotation - a.HullRotation;
            Vector2 velocityDiff = b.Velocity - a.Velocity;
            float turretRotationDiff = b.TurretRotation - a.TurretRotation;
            int actionDiff = b.PerformedAction - a.PerformedAction;

            return new TankDelta(positionDiff, hullRotationDiff, velocityDiff, turretRotationDiff, actionDiff);
        }

        private static BulletDelta ComputeBulletStateDelta(in BulletSimulationState a, in BulletSimulationState b)
        {
            Vector2 positionDiff = b.Position - a.Position;
            float rotationDiff = b.Rotation - a.Rotation;
            Vector2 velocityDiff = b.Velocity - a.Velocity;

            return new BulletDelta(positionDiff, rotationDiff, velocityDiff);
        }

        public static bool CompareDeltas(in IStateDelta a, in IStateDelta b)
        {
            if (a is TankDelta tankDeltaA && b is TankDelta tankDeltaB)
            {
                return CompareTankStateDeltas(tankDeltaA, tankDeltaB);
            }
            else if (a is BulletDelta bulletDeltaA && b is BulletDelta bulletDeltaB)
            {
                return CompareBulletStateDeltas(bulletDeltaA, bulletDeltaB);
            }

            throw new InvalidOperationException($"CompareDeltas computation not supported for {a} and {b}.");
        }

        private static bool CompareTankStateDeltas(in TankDelta a, in TankDelta b)
        {
            return  a.posDiff.sqrMagnitude > b.posDiff.sqrMagnitude ||
                    Mathf.Abs(a.hullRotDiff) > Mathf.Abs(b.hullRotDiff) ||
                    a.velDiff.sqrMagnitude > b.velDiff.sqrMagnitude ||
                    Mathf.Abs(a.turrRotDiff) > Mathf.Abs(b.turrRotDiff) ||
                    a.actionDiff > b.actionDiff;
        }

        private static bool CompareBulletStateDeltas(in BulletDelta a, in BulletDelta b)
        {
            return  a.posDiff.sqrMagnitude > b.posDiff.sqrMagnitude ||
                    a.rotDiff > b.rotDiff ||
                    a.velDiff.sqrMagnitude > b.velDiff.sqrMagnitude;
        }
    }
}