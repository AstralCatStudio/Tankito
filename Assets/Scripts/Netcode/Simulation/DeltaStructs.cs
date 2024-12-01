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
        public short actionDiff;
        public short playerStateDiff;
        public short ticksSinceFireDiff;
        public short ticksSinceDashDiff;
        public short ticksSinceParryDiff;
#else
    public readonly struct TankDelta : IStateDelta
    {
        public readonly Vector2 posDiff;
        public readonly float hullRotDiff;
        public readonly Vector2 velDiff;
        public readonly float turrRotDiff;
        public readonly short actionDiff;
        public readonly short playerStateDiff;
        public readonly short ticksSinceFireDiff;
        public readonly short ticksSinceDashDiff;
        public readonly short ticksSinceParryDiff;
#endif


        public TankDelta(Vector2 posDiff, float hullRotDiff, Vector2 velDiff, float turrRotDiff, short actionDiff, short playerStateDiff, short ticksSinceFireDiff, short ticksSinceDashDiff, short ticksSinceParryDiff)
        {
            this.posDiff = posDiff;
            this.hullRotDiff = hullRotDiff;
            this.velDiff = velDiff;
            this.turrRotDiff = turrRotDiff;
            this.actionDiff = actionDiff;
            this.playerStateDiff = playerStateDiff;
            this.ticksSinceFireDiff = ticksSinceFireDiff;
            this.ticksSinceDashDiff = ticksSinceDashDiff;
            this.ticksSinceParryDiff = ticksSinceParryDiff;
        }

        public TankDelta(in TankSimulationState tankState)
        {
            posDiff = tankState.Position;
            hullRotDiff = tankState.HullRotation;
            velDiff = tankState.Velocity;
            turrRotDiff = tankState.TurretRotation;
            actionDiff = (short)tankState.PerformedAction;
            playerStateDiff = (short) tankState.PlayerState;
            ticksSinceFireDiff = (short) tankState.TicksSinceFire;
            ticksSinceDashDiff = (short) tankState.TicksSinceDash;
            ticksSinceParryDiff = (short) tankState.TicksSinceParry;
        }

        public override string ToString()
        {
            return "[ " +
                    " Δ_pos:" + posDiff +
                    " | Δ_vel:" + velDiff +
                    " | Δ_hull_rot" + hullRotDiff +
                    " | Δ_turr_rot:" + turrRotDiff +
                    " | Δ_action: " + actionDiff +
                    " | Δ_playerState: " + playerStateDiff +
                    " | Δ_ticksSinceFire: " + ticksSinceFireDiff +
                    " | Δ_ticksSinceDash: " + ticksSinceDashDiff +
                    " | Δ_ticksSinceParry: " + ticksSinceParryDiff +
                    " ]";
        }
    }

#if UNITY_EDITOR
    [Serializable]
    public struct BulletDelta : IStateDelta
    {
        public Vector2 posDiff;
        public float rotDiff;
        public Vector2 velDiff;
        public float lifeTimeDiff;
        public int bouncesLeftDiff;
        public long ownerIdDiff;
#else
    public readonly struct BulletDelta : IStateDelta
    {
        public readonly Vector2 posDiff;
        public readonly float rotDiff;
        public readonly Vector2 velDiff;
        public readonly float lifeTimeDiff;
        public readonly int bouncesLeftDiff;
        public readonly long ownerIdDiff;
#endif
        public BulletDelta(Vector2 posDiff, float rotDiff, Vector2 velDiff, float lifeTimeDiff, int bouncesLeftDiff, long ownerIdDiff)
        {
            this.posDiff = posDiff;
            this.rotDiff = rotDiff;
            this.velDiff = velDiff;
            this.lifeTimeDiff = lifeTimeDiff;
            this.bouncesLeftDiff = bouncesLeftDiff;
            this.ownerIdDiff = ownerIdDiff;
        }

        public BulletDelta(in BulletSimulationState bulletState)
        {
            posDiff = bulletState.Position;
            rotDiff = bulletState.Rotation;
            velDiff = bulletState.Velocity;
            lifeTimeDiff = bulletState.LifeTime;
            bouncesLeftDiff = bulletState.BouncesLeft;
            ownerIdDiff = (long)bulletState.OwnerId;
        }
        public override string ToString()
        {
            return "[ " + 
                    "Δ_pos: " + posDiff +
                    " | Δ_vel: " + velDiff +
                    " | Δ_rot: " + rotDiff +
                    " | Δ_lifeTime: " + lifeTimeDiff +
                    " | Δ_bouncesLeft: " + bouncesLeftDiff +
                    " | Δ_ownerId: " + ownerIdDiff +
                    " ]";
        }
    }

}