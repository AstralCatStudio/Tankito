using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Unity.Collections;
using UnityEngine;


namespace Tankito.Netcode.Simulation
{
    using SimObjT = SimulationObjectType;
    using ISimS = ISimulationState;
    using TankSimS = TankSimulationState;
    using BullSimS = BulletSimulationState;

    public static class SimExtensions
    {
        public static unsafe ulong HashSimObj(ulong ownerId, int spawnTick, int spawnN)
        {
            // Allocate unmanaged memory for the input data
            byte* dataPtr = (byte*)Marshal.AllocHGlobal(sizeof(ulong) + 2 * sizeof(int));

            try
            {
                // Copy data into unmanaged memory
                *(ulong*)dataPtr = ownerId;
                *(int*)(dataPtr + sizeof(ulong)) = spawnTick;
                *(int*)(dataPtr + sizeof(ulong) + sizeof(int)) = spawnN;

                // Compute the hash using the pointer and length of the data
                var hashResult = xxHash3.Hash64(dataPtr, sizeof(ulong) + 2 * sizeof(int));

                // Return the 64-bit hash result
                return hashResult.x;  // Assuming x represents the 64-bit hash value
            }
            finally
            {
                // Free the unmanaged memory after use
                Marshal.FreeHGlobal((IntPtr)dataPtr);
            }
        }

        public static IStateDelta[] Delta(in SimulationSnapshot snapA, in SimulationSnapshot snapB)
        {
            IStateDelta[] snapshotDeltas = new IStateDelta[Math.Max(snapA.Count, snapB.Count)];
            int i = 0;

            foreach(var obj in snapA.IDs)
            {
                if (snapB.ContainsId(obj))
                {
                    snapshotDeltas[i] = Delta(snapA[obj].state, snapB[obj].state);
                }
                else
                {
                    snapshotDeltas[i] = Delta(snapA[obj].state);
                }
            }

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
                //Debug.Log($"[{SimClock.TickCounter}] Comparing States: {bulletStateA} ~= {bulletStateB}");
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
            float hullRotationDiff = Mathf.Repeat(b.HullRotation, 360) - Mathf.Repeat(a.HullRotation, 360);
            Vector2 velocityDiff = b.Velocity - a.Velocity;
            float turretRotationDiff = Mathf.Repeat(b.TurretRotation, 360) - Mathf.Repeat(a.TurretRotation, 360);
            int actionDiff = b.PerformedAction - a.PerformedAction;
            int playerStateDiff = b.PlayerState - a.PlayerState;
            int lastFireTickDiff = b.LastFireTick - a.LastFireTick;
            int lastDashTickDiff = b.LastDashTick - a.LastDashTick;
            int lastParryTickDiff = b.LastParryTick - a.LastParryTick;

            var deltas = new TankDelta(positionDiff,
                                hullRotationDiff,
                                velocityDiff,
                                turretRotationDiff,
                                (short)actionDiff,
                                (short)playerStateDiff,
                                lastFireTickDiff,
                                lastDashTickDiff,
                                lastParryTickDiff);

            //Debug.Log("TankDeltas: " + deltas);

            return deltas;
        }

        private static BulletDelta ComputeBulletStateDelta(in BulletSimulationState a, in BulletSimulationState b)
        {
            Vector2 positionDiff = b.Position - a.Position;
            Vector2 velocityDiff = b.Velocity - a.Velocity;
            float lifeTimeDiff = b.Lifetime - a.Lifetime;
            int bouncesLeftDiff = b.BouncesLeft - a.BouncesLeft;
            long ownerIdDiff = (long)b.OwnerId - (long)a.OwnerId;
            long lastShooterObjIdDiff = (long)b.LastShooterObjId - (long)a.LastShooterObjId;

            var deltas = new BulletDelta(positionDiff,
                                    velocityDiff,
                                    lifeTimeDiff,
                                    bouncesLeftDiff,
                                    ownerIdDiff,
                                    lastShooterObjIdDiff);

            //Debug.Log("BulletDeltas: " + deltas);

            return deltas;
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
            bool deltaComparison = a.posDiff.sqrMagnitude > b.posDiff.sqrMagnitude ||
                    Mathf.Abs(a.hullRotDiff) > Mathf.Abs(b.hullRotDiff) ||
                    a.velDiff.sqrMagnitude > b.velDiff.sqrMagnitude ||
                    Mathf.Abs(a.turrRotDiff) > Mathf.Abs(b.turrRotDiff) ||
                    Mathf.Abs(a.actionDiff) > Mathf.Abs(b.actionDiff) ||
                    Mathf.Abs(a.playerStateDiff) > Mathf.Abs(b.playerStateDiff) ||
                    Mathf.Abs(a.lastFireTickDiff) > Mathf.Abs(b.lastFireTickDiff) ||
                    Mathf.Abs(a.lastDashTickDiff) > Mathf.Abs(b.lastDashTickDiff) ||
                    Mathf.Abs(a.lastParryTickDiff) > Mathf.Abs(b.lastParryTickDiff);

            return  deltaComparison;
        }

        private static bool CompareBulletStateDeltas(in BulletDelta a, in BulletDelta b)
        {
            return  a.posDiff.sqrMagnitude > b.posDiff.sqrMagnitude ||
                    a.velDiff.sqrMagnitude > b.velDiff.sqrMagnitude ||
                    Mathf.Abs(a.lifeTimeDiff) > Mathf.Abs(b.lifeTimeDiff) ||
                    Mathf.Abs(a.bouncesLeftDiff) > Mathf.Abs(b.bouncesLeftDiff) ||
                    Mathf.Abs(a.ownerIdDiff) > Mathf.Abs(b.ownerIdDiff) ||
                    Mathf.Abs(a.lastShooterObjIdDiff) > Mathf.Abs(b.lastShooterObjIdDiff);
        }

        public static (SimObjT type, ISimS state) InterpolateTo(this (SimObjT type, ISimS state) typeStatePair, (SimObjT type, ISimS state) nextStatePair, float t)
        {
            if (typeStatePair.type != nextStatePair.type)
            {
                throw new InvalidOperationException();
            }

            ISimS interpolatedState;

            switch(typeStatePair.type)
            {
                case SimObjT.Tank:
                    interpolatedState = ((TankSimS)typeStatePair.state).InterpolateTo((TankSimS)nextStatePair.state, t);
                    break;

                case SimObjT.Bullet:
                    interpolatedState = ((BullSimS)typeStatePair.state).InterpolateTo((BullSimS)nextStatePair.state, t);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return (typeStatePair.type, interpolatedState);
        }

        public static TankSimS InterpolateTo(this TankSimS state, TankSimS nextState, float t)
        {
            var position = Vector2.Lerp(state.Position, nextState.Position, t);
            var hullRotation = Mathf.Lerp(state.HullRotation, nextState.HullRotation, t);
            var velocity = Vector2.Lerp(state.Velocity, nextState.Velocity, t);
            var turretRotation = Mathf.Lerp(state.TurretRotation, nextState.TurretRotation, t);

            TankSimS interpolatedState =
                new TankSimS(
                    position,
                    hullRotation,
                    velocity,
                    turretRotation,
                    state.PerformedAction,
                    state.PlayerState,
                    state.LastFireTick,
                    state.LastDashTick,
                    state.LastParryTick);

            return interpolatedState;
        }

        public static BullSimS InterpolateTo(this BullSimS state, BullSimS nextState, float t)
        {
            var position = Vector2.Lerp(state.Position, nextState.Position, t);
            var velocity = Vector2.Lerp(state.Velocity, nextState.Velocity, t);
            var lifetime = Mathf.Lerp(state.Lifetime, nextState.Lifetime, t);
            
            BullSimS interpolatedState = 
                new BullSimS(
                    position,
                    velocity,
                    lifetime,
                    state.BouncesLeft,
                    state.OwnerId,
                    state.LastShooterObjId);

            return interpolatedState;
        }
    }
}