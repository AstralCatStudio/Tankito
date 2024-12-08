using System;
using Unity.Mathematics;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public interface IStateDelta
    {
        
    }

#if UNITY_EDITOR
    [System.Serializable]
    public struct TankDelta : IStateDelta
    {
        public Vector2 posDiff;
        public float hullRotDiff;
        public Vector2 velDiff;
        public float turrRotDiff;
        public int actionDiff;
#else
    public readonly struct TankDelta : IStateDelta
    {
        public readonly Vector2 posDiff;
        public readonly float hullRotDiff;
        public readonly Vector2 velDiff;
        public readonly float turrRotDiff;
        public readonly int actionDiff;
#endif


        public TankDelta(Vector2 posDiff, float hullRotDiff, Vector2 velDiff, float turrRotDiff, int actionDiff)
        {
            this.posDiff = posDiff;
            this.hullRotDiff = hullRotDiff;
            this.velDiff = velDiff;
            this.turrRotDiff = turrRotDiff;
            this.actionDiff = actionDiff; 
        }

        public TankDelta(in TankSimulationState tankState)
        {
            posDiff = tankState.Position;
            hullRotDiff = tankState.HullRotation;
            velDiff = tankState.Velocity;
            turrRotDiff = tankState.TurretRotation;
            actionDiff = (int)tankState.PerformedAction;
        }

        public override string ToString()
        {
            return "[ Δpos:" + posDiff + " | Δvel:" + velDiff + " | Δhull_rot" + hullRotDiff + " | Δturr_rot:" + turrRotDiff + " ]";
        }
    }

#if UNITY_EDITOR
    [Serializable]
    public struct BulletDelta : IStateDelta
    {
        public  Vector2 posDiff;
        public  float rotDiff;
        public  Vector2 velDiff;
#else
    public readonly struct BulletDelta : IStateDelta
    {
        public readonly Vector2 posDiff;
        public readonly float rotDiff;
        public readonly Vector2 velDiff;
#endif
        public BulletDelta(Vector2 posDiff, float rotDiff, Vector2 velDiff)
        {
            this.posDiff = posDiff;
            this.rotDiff = rotDiff;
            this.velDiff = velDiff;
        }

        public BulletDelta(in BulletSimulationState bulletState)
        {
            posDiff = bulletState.Position;
            rotDiff = bulletState.Rotation;
            velDiff = bulletState.Velocity;
        }
        public override string ToString()
        {
            return "[ Δpos:" + posDiff + " | Δvel:" + velDiff + " | Δrot" + rotDiff + " ]";
        }
    }

}