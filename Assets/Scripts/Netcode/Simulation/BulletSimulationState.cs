using System;
using System.Collections;
using System.Collections.Generic;
using Tankito.Netcode.Simulation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public struct BulletSimulationState : ISimulationState
{

    public Vector2 Position { get => position; }
    public float Rotation { get => rotation; }
    public Vector2 Velocity { get => velocity; }
    public float LifeTime { get => lifeTime; }

    public float PositonTolerance { get => tolerances.posTol; set => tolerances.posTol = value; }
    public float RotationTolerance { get => tolerances.rotTol; set => tolerances.rotTol = value; }
    public float VelocityTolerance { get => tolerances.velTol; set => tolerances.velTol = value; }

    private Vector2 position;
    private float rotation;
    private Vector2 velocity;
    private float lifeTime; // In seconds
    //private int spawnTick;

    private BulletTolerance tolerances;

    public const int MAX_SERIALIZED_SIZE = sizeof(float)*2 + sizeof(float)*2*2;

    public BulletSimulationState(Vector2 position, float rotation, Vector2 velocity, float lifeTime)
    {
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.lifeTime = lifeTime;
        //this.spawnTick = spawnTick;

        tolerances = new BulletTolerance(0, 0, 0);
    }

    public BulletSimulationState(Vector2 position, float rotation, Vector2 velocity, float lifeTime, BulletTolerance tolerances)
    {
        this.position = position;
        this.rotation = rotation;
        this.velocity = velocity;
        this.lifeTime = lifeTime;
        //this.spawnTick = spawnTick;
        this.tolerances = tolerances;
    }

    public bool CheckReconcilation(ISimulationState state)
    {
        BulletSimulationState bulletState = (BulletSimulationState)state;
        (float posDiff, float rotDiff, float velDiff) diffs = Diff(bulletState);
        return tolerances.CheckTolerances(new BulletTolerance(diffs.posDiff, diffs.rotDiff, diffs.velDiff));
    }

    internal (float posDiff, float rotDiff,float velDiff) Diff(BulletSimulationState state)
    {
        float positionDiff = Vector2.SqrMagnitude(state.position - position);
        float rotationDiff = Mathf.Abs(state.rotation - rotation);
        float velocityDiff = Vector2.SqrMagnitude(state.velocity - velocity);
        return (positionDiff, rotationDiff, velocityDiff);
    }

    internal void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref velocity);
        serializer.SerializeValue(ref lifeTime);
    }
}

public struct BulletTolerance
{
    public float posTol;
    public float rotTol;
    public float velTol;

    public BulletTolerance(float posTolerance, float rotTolerance, float velTolerance)
    {
        posTol = posTolerance;
        rotTol = rotTolerance;
        velTol = velTolerance;
    }

    public bool CheckTolerances(BulletTolerance difs)
    {
        if (posTol < difs.posTol || rotTol < difs.rotTol || velTol < difs.velTol) return true;
        else return false;
    }
}


