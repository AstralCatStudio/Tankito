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
        public int lastFireTickDiff;
        public int lastDashTickDiff;
        public int lastParryTickDiff;
#else
    public readonly struct TankDelta : IStateDelta
    {
        public readonly Vector2 posDiff;
        public readonly float hullRotDiff;
        public readonly Vector2 velDiff;
        public readonly float turrRotDiff;
        public readonly short actionDiff;
        public readonly short playerStateDiff;
        public readonly int lastFireTickDiff;
        public readonly int lastDashTickDiff;
        public readonly int lastParryTickDiff;
#endif


        public TankDelta(Vector2 posDiff, float hullRotDiff, Vector2 velDiff, float turrRotDiff, short actionDiff, short playerStateDiff, int lastFireTickDiff, int lastDashTickDiff, int lastParryTickDiff)
        {
            this.posDiff = posDiff;
            this.hullRotDiff = hullRotDiff;
            this.velDiff = velDiff;
            this.turrRotDiff = turrRotDiff;
            this.actionDiff = actionDiff;
            this.playerStateDiff = playerStateDiff;
            this.lastFireTickDiff = lastFireTickDiff;
            this.lastDashTickDiff = lastDashTickDiff;
            this.lastParryTickDiff = lastParryTickDiff;
        }

        public TankDelta(in TankSimulationState tankState)
        {
            posDiff = tankState.Position;
            hullRotDiff = Mathf.Repeat(tankState.HullRotation, 360);
            velDiff = tankState.Velocity;
            turrRotDiff = Mathf.Repeat(tankState.TurretRotation, 360);
            actionDiff = (short)tankState.PerformedAction;
            playerStateDiff = (short) tankState.PlayerState;
            lastFireTickDiff = tankState.LastFireTick;
            lastDashTickDiff = tankState.LastDashTick;
            lastParryTickDiff = tankState.LastParryTick;
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
                    " | Δ_lastFireTick: " + lastFireTickDiff +
                    " | Δ_lastDashTick: " + lastDashTickDiff +
                    " | Δ_lastParryTick: " + lastParryTickDiff +
                    " ]";
        }
    }

#if UNITY_EDITOR
    [Serializable]
    public struct BulletDelta : IStateDelta
    {
        public Vector2 posDiff;
        public Vector2 velDiff;
        public float lifeTimeDiff;
        public int bouncesLeftDiff;
        public long ownerIdDiff;
        public long lastShooterObjIdDiff;
#else
    public readonly struct BulletDelta : IStateDelta
    {
        public readonly Vector2 posDiff;
        public readonly Vector2 velDiff;
        public readonly float lifeTimeDiff;
        public readonly int bouncesLeftDiff;
        public readonly long ownerIdDiff;
        public readonly long lastShooterObjIdDiff;
#endif
        public BulletDelta(Vector2 posDiff, Vector2 velDiff, float lifeTimeDiff, int bouncesLeftDiff, long ownerIdDiff, long lastShooterObjIdDiff)
        {
            this.posDiff = posDiff;
            this.velDiff = velDiff;
            this.lifeTimeDiff = lifeTimeDiff;
            this.bouncesLeftDiff = bouncesLeftDiff;
            this.ownerIdDiff = ownerIdDiff;
            this.lastShooterObjIdDiff = lastShooterObjIdDiff;
        }

        public BulletDelta(in BulletSimulationState bulletState)
        {
            posDiff = bulletState.Position;
            velDiff = bulletState.Velocity;
            lifeTimeDiff = bulletState.LifeTime;
            bouncesLeftDiff = bulletState.BouncesLeft;
            ownerIdDiff = (long)bulletState.OwnerId;
            lastShooterObjIdDiff = (long)bulletState.LastShooterObjId;
        }
        public override string ToString()
        {
            return "[ " + 
                    "Δ_pos: " + posDiff +
                    " | Δ_vel: " + velDiff +
                    " | Δ_lifeTime: " + lifeTimeDiff +
                    " | Δ_bouncesLeft: " + bouncesLeftDiff +
                    " | Δ_ownerId: " + ownerIdDiff +
                    " | Δ_lastShooterId: " + lastShooterObjIdDiff +
                    " ]";
        }
    }

}