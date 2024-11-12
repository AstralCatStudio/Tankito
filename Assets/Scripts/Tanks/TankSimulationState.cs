using System;
using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode.Simulation;
using Unity.Netcode;
using UnityEngine;
using static Tankito.Netcode.ClientPredictedTankController;

namespace Tankito.Netcode.Simulation
{
    public struct TankSimulationState : ISimulationState
    {
        public Vector2 Position { get => position; }
        public float HullRotation { get => hullRotation; }
        public Vector2 Velocity { get => velocity; }
        public float TurretRotation { get => turretRotation; }

        private Vector2 position;
        private float hullRotation;
        private Vector2 velocity;
        private float turretRotation;

        //private TankTolerance tolerances;

        public const int MAX_SERIALIZED_SIZE = sizeof(float)*2 + sizeof(float)*2*2;

        public TankSimulationState(Vector2 position, float hullRotation, Vector2 velocity, float turretRotation)
        {
            this.position = position;
            this.hullRotation = hullRotation;
            this.velocity = velocity;
            this.turretRotation = turretRotation;
            //tolerances = new TankTolerance(0, 0, 0, 0);
        }

        /*
        public TankSimulationState(Vector2 position, float hullRotation, Vector2 velocity, float turretRotation, TankTolerance tankTolerance)
        {
            this.position = position;
            this.hullRotation = hullRotation;
            this.velocity = velocity;
            this.turretRotation = turretRotation;
            //tolerances = tankTolerance;
        }
        */

        /*
        public bool CheckReconcilation(ISimulationState state)
        {
            TankSimulationState bulletState = (TankSimulationState)state;
            (float posDiff, float rotHullDiff, float velDiff, float turretRotDiff) diffs = Diff(bulletState);
            return tolerances.CheckTolerances(new TankTolerance(diffs.posDiff, diffs.rotHullDiff, diffs.velDiff, diffs.turretRotDiff));
        }
        */

        internal (float posDiff, float rotHullDiff, float velDiff, float turrretRotDiff) Diff(TankSimulationState state)
        {
            float positionDiff = Vector2.SqrMagnitude(state.position - position);
            float rotationHullDiff = Mathf.Abs(state.hullRotation - hullRotation);
            float velocityDiff = Vector2.SqrMagnitude(state.velocity - velocity);
            float turretRotationDiff = Mathf.Abs(state.turretRotation - turretRotation);
            return (positionDiff, rotationHullDiff, velocityDiff, turretRotationDiff);
        }

        internal void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref hullRotation);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref turretRotation);
        }
    }
}

// Haria que las tolerancias simplemente fueran Deltas concretos en el simulation manager llamado tolerancias
/*
public struct TankTolerance
{
    public float posTol;
    public float rotHullTol;
    public float velTol;
    public float rotTurretTol;

    public TankTolerance(float posTolerance, float rotHullTolerance, float velTolerance, float rotTurretTolerance)
    {
        posTol = posTolerance;
        rotHullTol = rotHullTolerance;
        velTol = velTolerance;
        rotTurretTol = rotTurretTolerance;
    }

    public bool CheckTolerances(TankTolerance difs)
    {
        if (posTol < difs.posTol || rotHullTol < difs.rotHullTol || velTol < difs.velTol || rotTurretTol < difs.rotTurretTol) return true;
        else return false;
    }
}
*/