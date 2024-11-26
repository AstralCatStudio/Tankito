using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Tankito.Netcode.Messaging;
using Tankito.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class ClientSimulationManager : NetSimulationManager<ClientSimulationManager>
    {
        int SNAPSHOT_BUFFER_SIZE { get => SimulationParameters.SNAPSHOT_BUFFER_SIZE; }
        CircularBuffer<SimulationSnapshot> m_snapshotBuffer;

        /// <summary>
        /// Relates NetworkClientId(ulong) to a specific <see cref="EmulatedTankInput"/>.  
        /// </summary>
        public Dictionary<ulong, EmulatedTankInput> emulatedInputTanks = new Dictionary<ulong,EmulatedTankInput>();

        [SerializeField] private TankDelta m_tankSimulationTolerance;
        [SerializeField] private BulletDelta m_bulletSimulationTolerance;

        [SerializeField] private bool DEBUG = true;

        const int NO_ROLLBACK = -1;
        int m_rollbackTick = NO_ROLLBACK;
        const int NO_SNAPSHOT = -1;
        private int m_lastAuthSnapshotTimestamp;//Por como funciona el rollback, igual esto no hace falta y unicamente podemos necesitar 
                                                //que se guarde el timestamp. EDIT: Vale, en efecto, lo estoy cambiando para desligar
                                                //el tamaño del snapshot buffer de nuestra capacidad de recordar el último AuthSnapshot
                                                //recibido (o al menos su timestamp que es realmente lo único que nos interesa).
        //{
        //    get
        //    {
        //        var authStates = m_snapshotBuffer.Where(s => s.status == SnapshotStatus.Authoritative);
        //        return (authStates.Count() > 0) ? authStates.MaxBy(s => s.timestamp) : default;
        //    }
        //}

        protected override int CaptureSnapshotTick 
        {
            get
            {
                if(m_rollbackTick ==  NO_ROLLBACK)
                {
                    return SimClock.TickCounter;
                }
                else
                {
                    return m_rollbackTick;
                }
            }
        }

        void Start()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                Debug.LogWarning("ClientSimulationManager is network node that is NOT a CLIENT (is server). this should not happen!");
                Destroy(this);
            }
            m_tankSimulationTolerance = new TankDelta(new Vector2(0.1f,0.1f), 3f, new Vector2(0.2f,0.2f), 60f, 0);
            m_bulletSimulationTolerance = new BulletDelta(new Vector2(0.1f,0.1f), 1f, new Vector2(0.1f,0.1f));

            m_snapshotBuffer = new CircularBuffer<SimulationSnapshot>(SNAPSHOT_BUFFER_SIZE);
            m_lastAuthSnapshotTimestamp = NO_SNAPSHOT;
        }

        public override void Simulate()
        {
            //Debug.Log("CLIENT SimulationManager Simulate() called!");
            
            //Debug.Log("TODO: Implement Input Sampling and Sending on client.");


            if (!NetworkManager.Singleton.IsServer)
            {
                // We don't want HOSTs to execute Simulate twice per tick.
                // They should only Simulate with server logic, no need for prediciton and/or simulation throttling.

                // Input Sending ocurrs within the implicit TankController calls.
                // |
                // --> SampleInput(); // Ensamblar ventana de inputs y mandarla al servidor.
                // Tambien dead reckoning!

                base.Simulate();
                
            }
            // Cache Simulation State
            SimulationSnapshot newSnapshot = CaptureSnapshot();
            newSnapshot.status = SnapshotStatus.Predicted;
            //if (!SimClock.Instance.Active) Debug.Log($"[{SimClock.TickCounter}]ReconciliatedSnapshot-> saved to buffer at({newSnapshot.timestamp})");
            m_snapshotBuffer.Add(newSnapshot, newSnapshot.timestamp);
        }
        
        // public bool ContainedSimObj(ulong simObjId, int tick)
        // {
        //     return m_snapshotBuffer[tick].ContainsId(simObjId);
        // }

        public void EvaluateForReconciliation(SimulationSnapshot newAuthSnapshot)
        {
            if ((newAuthSnapshot.timestamp <= m_lastAuthSnapshotTimestamp) ||
                (newAuthSnapshot.timestamp < m_snapshotBuffer.First.timestamp))
            {
                if (DEBUG) Debug.Log($"NOT Reconciling with snapshot[{newAuthSnapshot.timestamp}] (Too OLD)."+
                                    $"Oldest predicted snapshot[{m_snapshotBuffer.First}]."+
                                    $"Newest auth snapshot[{m_lastAuthSnapshotTimestamp}]."+
                                    $"Snapshot Buff Size: {m_snapshotBuffer.Capacity}");
                return;
            }

            m_lastAuthSnapshotTimestamp = newAuthSnapshot.timestamp;

            // Jump forward in time to sim state
            if (newAuthSnapshot.timestamp >= SimClock.TickCounter)
            {
                if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]Jumping forward to future state[{newAuthSnapshot.timestamp}]");

                SimClock.Instance.SetClock(newAuthSnapshot.timestamp);
                SetSimulation(newAuthSnapshot);
                m_snapshotBuffer.Add(newAuthSnapshot, newAuthSnapshot.timestamp);
                return;
            }
            
            if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]Evaluating Desync for: "+ m_snapshotBuffer.Get(newAuthSnapshot.timestamp));


            if (DEBUG) Debug.Log("Evaluating Desync for: "+ m_snapshotBuffer.Get(newAuthSnapshot.timestamp));
            SimulationSnapshot predictedSnapshot = m_snapshotBuffer.Where(s => s.timestamp == newAuthSnapshot.timestamp 
                && s.status == SnapshotStatus.Predicted).FirstOrDefault();

            if (!predictedSnapshot.Equals(default(SimulationSnapshot)))
            {
                bool missingObjects = false;
                
                foreach(var authObjId in newAuthSnapshot.IDs)
                {
                    //if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]: {snapshotObj}");
                    if (!predictedSnapshot.ContainsId(authObjId))
                    {
                        // Auth Obj NOT in Snapshot
                        if (newAuthSnapshot[authObjId].type == SimulationObjectType.Bullet)
                        {
                            var bulletState = (BulletSimulationState)newAuthSnapshot[authObjId].state;
                            // Si es su 1er tick de vida, dejamos que intente el propio rollback instanciar la bala
                            if (bulletState.LifeTime >= SimClock.SimDeltaTime*2)
                            {
                                missingObjects = true;
                                var ownerId =  ((BulletSimulationState)newAuthSnapshot[authObjId].state).OwnerId;
                                var authBullet = BulletPool.Instance.Get(bulletState.Position, bulletState.Rotation, ownerId, authObjId, autoSpawn:false);
                                authBullet.OnNetworkSpawn();
                                if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]Reconciliated Bullet[{authObjId}] successfully added to sim? => " + m_simulationObjects.ContainsKey(authObjId));
                            }
                            else
                            {
                                Debug.Log($"[{SimClock.TickCounter}]Handing spawning attempt over to reconciliation (remote client input replay) because {authObjId}'s lifetime is lower than 2 ticks (it was spawned on tick[{predictedSnapshot.timestamp}])");
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"Pero que cojones estas intentando hacer? (sim doesn't contain {authObjId} and it isn't a bullet)");
                        }
                    }
                }

                if (missingObjects)
                {
                    Rollback(newAuthSnapshot);
                    m_snapshotBuffer.Add(newAuthSnapshot, newAuthSnapshot.timestamp);
                    return;
                }

                foreach (var objId in predictedSnapshot.IDs)
                {
                    if (newAuthSnapshot.ContainsId(objId))
                    {
                        if (CheckForDesync(predictedSnapshot[objId].state, newAuthSnapshot[objId].state))
                        {
                            if (DEBUG)
                            {
                                Debug.Log($"[{SimClock.TickCounter}]Rolling back to [{newAuthSnapshot.timestamp}]"+
                                $"\nBecause {objId} dind't meet the delta Thresholds");
                            }
                            
                            Rollback(newAuthSnapshot);
                            break;
                        }
                    }
                }
            }
            
            //newAuthSnapshot.status = SnapshotStatus.Authoritative;   DEBE LLEGAR AUTH DESDE SERVER
            m_snapshotBuffer.Add(newAuthSnapshot, newAuthSnapshot.timestamp);
        }

        public void Rollback(SimulationSnapshot authSnapshot)
        {

            m_rollbackTick = authSnapshot.timestamp;
            
            //Pause Simulation Clock
            SimClock.Instance.StopClock();

            // We DON'T have to re-simulate the tick which we are getting as auth,
            // because it's already simulated. So just advance the counter
            m_rollbackTick++;

            // We must only reconcile those objects that were present on the predicted snapshot (and therefore, predicted themselves).
            var predictedSimObjs = m_simulationObjects.Where(obj => m_snapshotBuffer[m_rollbackTick].ContainsId(obj.Key));
            
            foreach(var objIdPair in predictedSimObjs)
            {
                if (authSnapshot.ContainsId(objIdPair.Key))
                {
                    m_simulationObjects[objIdPair.Key].SetSimState(authSnapshot[objIdPair.Key].state);
                }
                else
                {
                    if (DEBUG) Debug.Log($"Queueing [{objIdPair}] for despawn (NOT found in authSnapshot)");
                    QueueForDespawn(objIdPair.Key);
                }
                
                // Put Input Components into replay mode
                if(m_simulationObjects[objIdPair.Key] is TankSimulationObject tank)
                {
                    tank.StartInputReplay(m_rollbackTick);
                }
                // Habra que hacer algo para restaurar objetos que puedieran haber deespawneado y todo eso supongo
            }
            
            while(m_rollbackTick < SimClock.TickCounter)
            {
                // - Input Replay - DONE => Implicitly consumes inputs from input caches when pulling InputPayloads on Kinematic Functions
                Simulate();
                m_rollbackTick++;
            }

            // Set tank's input components back on live input mode
            foreach(var objIdPair in predictedSimObjs)
            {
                if(m_simulationObjects[objIdPair.Key] is TankSimulationObject tank)
                {
                    var lastReplayTick = tank.StopInputReplay();
                    //if (DEBUG) Debug.Log($"Tank({tank.NetworkObjectId})'s last replayed input was on Tick- {lastReplayTick}");
                }
            }

            m_rollbackTick = NO_ROLLBACK;
            //Resume Simulation Clock
            SimClock.Instance.ResumeClock();
        }

        // WIP !!
        public void SetSimulation(SimulationSnapshot newSimSnapshot)
        {
            SimClock.Instance.StopClock();
            
            foreach(var objId in newSimSnapshot.IDs)
            {
                if (m_simulationObjects.Keys.Contains(objId))
                {
                    m_simulationObjects[objId].SetSimState(newSimSnapshot[objId].state);
                }
                else
                {
                    Debug.LogException(new NotImplementedException());
                }
            }

            SimClock.Instance.ResumeClock();
        }

        private bool CheckForDesync(in ISimulationState simStateA,in ISimulationState simStateB)
        {
            IStateDelta delta = SimExtensions.Delta(simStateA, simStateB);
            if(simStateA is TankSimulationState && simStateB is TankSimulationState)
            {
                TankDelta tankDelta = (TankDelta)delta;
                return SimExtensions.CompareDeltas(tankDelta, m_tankSimulationTolerance);
            }
            else if(simStateA is BulletSimulationState && simStateB is BulletSimulationState)
            {
                BulletDelta bulletDelta = (BulletDelta)delta;
                return SimExtensions.CompareDeltas(bulletDelta, m_bulletSimulationTolerance);
            }

            Debug.LogException(new InvalidOperationException("Desync check failed, type mismatch: " + simStateA.GetType() + "-" + simStateB.GetType()));
            return false;
        }

        #region DEBUG_TESTING_METHODS

        [ContextMenu("TestGetSet")]
        public void TestGetSet()
        {
            ISimulationState stateToCopy = m_simulationObjects[0].GetSimState(); // Explicit casting is not necessary
            m_simulationObjects[1].SetSimState(stateToCopy);
        }

        /*[ContextMenu("StateComparison")]
        public void StateComparison()
        {
            TankSimulationState tankA = new TankSimulationState(Vector2.right, 90, Vector2.zero, 0, TankAction.None);
            TankSimulationState tankB = new TankSimulationState(Vector2.right*0.9f, 90, Vector2.zero, 0, TankAction.Fire);
            Debug.Log(SimExtensions.Delta(tankA, tankB));
        }*/

        [ContextMenu("StateComparison")]
        public void SnapshotComparison()
        {
            const float rewindTime = 1;
            int rewindTicks = (int)(rewindTime/SimClock.SimDeltaTime);
            // Warp back 1s in time
            int warpTick = SimClock.TickCounter-rewindTicks;
            var pastSnapshot = m_snapshotBuffer[warpTick];

            var lastSnapshot =  m_snapshotBuffer.Last();

            var deltas = SimExtensions.Delta(pastSnapshot, lastSnapshot);
            
            if (DEBUG) Debug.Log("DeltaSnapshot: " + deltas.Select(d => d.ToString()));

            string desyncs = "Desyncs: ";
            foreach(var objId in pastSnapshot.IDs)
            {
                if (lastSnapshot.ContainsId(objId))
                {
                    desyncs += $"[{objId}]-> " + CheckForDesync(lastSnapshot[objId].state, pastSnapshot[objId].state) + "   ";
                }
                else
                {
                    desyncs += $"[{objId}]-> missing in LastSnapshot. ";
                }
            }
            foreach(var obj in lastSnapshot.IDs)
            {
                if (!pastSnapshot.ContainsId(obj))
                {
                    desyncs += $"[{obj}]-> missing in PastSnapshot. ";
                }
            }
            
            if (DEBUG) Debug.Log("Thresholds check: " + desyncs);
        }

        [ContextMenu("TestTimeTravel")]
        public void TestTimeTravel()
        {
            const float rewindTime = 1;
            int rewindTicks = (int)(rewindTime/SimClock.SimDeltaTime);
            // Warp back 1s in time
            int warpTick = SimClock.TickCounter-rewindTicks;
            var pastSnapshot = m_snapshotBuffer[warpTick];

            if (pastSnapshot.Equals(default))
            {
                throw new IndexOutOfRangeException($"No snapshot to warp to at tick - {warpTick}");
            }
            
            SetSimulation(pastSnapshot);
        }

        [ContextMenu("TestRollback")]
        public void TestRollback()
        {
            SimulationSnapshot testSnapShot = m_snapshotBuffer.Get(SimClock.TickCounter - 50);
            testSnapShot.timestamp -= 50;
            EvaluateForReconciliation(testSnapShot);
        }

        [ContextMenu("TestInputWindowMessaging")]
        public void TestInputWindowMessaging()
        {
            MessageHandlers.Instance.SendInputWindowToServer(InputWindowBuffer.Instance.inputWindow);
        }

        #endregion
    }
}