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
        int MAX_DESYNC_COUNT { get => SimulationParameters.MAX_DESYNC_COUNT; }
        CircularBuffer<SimulationSnapshot> m_snapshotBuffer;

        /// <summary>
        /// Relates NetworkClientId(ulong) to a specific <see cref="EmulatedTankInput"/>.  
        /// </summary>
        public Dictionary<ulong, EmulatedTankInput> emulatedInputTankComponents = new Dictionary<ulong,EmulatedTankInput>();

        // We're leaving it in the back burner, input acknowledgement is too fucking hard for the little benefit of getting better
        // behaviour while working at the edge of the set Worst Case Latency.

        //public Dictionary<ulong, TankPlayerInput> localInputTanks = new Dictionary<ulong, TankPlayerInput>();
        
        [SerializeField] private TankDelta m_tankSimulationTolerance;
        [SerializeField] private BulletDelta m_bulletSimulationTolerance;

        [SerializeField] private bool DEBUG = false;

        const int NO_ROLLBACK = -1;
        int m_rollbackTick = NO_ROLLBACK;
        const int NO_SNAPSHOT = -1;
        private int m_lastAuthSnapshotTimestamp;//Por como funciona el rollback, igual esto no hace falta y unicamente podemos necesitar 
        private int m_desyncCounter = 0;

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
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("ClientSimulationManager is network node that is NOT a CLIENT (is server). this should not happen!");
                Destroy(this);
            }
            m_tankSimulationTolerance = new TankDelta(new Vector2(0.1f,0.1f), 0.5f, new Vector2(0.2f,0.2f), 1f, 0, 0, 0, 0, 0);
            m_bulletSimulationTolerance = new BulletDelta(new Vector2(0.1f,0.1f), new Vector2(0.1f,0.1f), 0.05f, 0, 0, 0);

            m_snapshotBuffer = new CircularBuffer<SimulationSnapshot>(SNAPSHOT_BUFFER_SIZE);
            m_lastAuthSnapshotTimestamp = NO_SNAPSHOT;
        }

        public override void Simulate()
        {
            // We don't want HOSTs to execute Simulate twice per tick.
            // They should only Simulate with server logic, no need for prediciton and/or simulation throttling.
            if (!NetworkManager.Singleton.IsServer)
            {
                //Debug.Log($"[{CaptureSnapshotTick}] Simulating {(m_rollbackTick == NO_ROLLBACK ? "Forward" : "Reconciliating")}");
                base.Simulate();

                // Cache Simulation Snapshot for prediction Rollback
                SimulationSnapshot newSnapshot = CaptureSnapshot();
                newSnapshot.SetStatus(SnapshotStatus.Predicted);
                m_snapshotBuffer.Add(newSnapshot, newSnapshot.Timestamp);
            }
            
        }

        /// <summary>
        /// This is used to check when we have all of client's inputs at any given tick,
        /// to trigger Rollback early and improve responsiveness.
        /// </summary>
        public void EvaluateEarlyInputReconciliation(int firstTick ,int lastTick)
        {
            if (lastTick > m_lastAuthSnapshotTimestamp ||
                m_snapshotBuffer.Last.Timestamp > lastTick ||
                m_snapshotBuffer.First.Timestamp < firstTick)
            {
                return;
            }

            for (int tick = firstTick; tick < lastTick; tick++)
            {
                // Only trigger early input reconciliation if we have ALL of the user's inputs at that tick
                if (m_snapshotBuffer[tick].Status == SnapshotStatus.Predicted &&
                    emulatedInputTankComponents.Values.Where(i => i.HasPayload(tick)).Count() == emulatedInputTankComponents.Values.Count())
                {
                    Debug.Log($"[{SimClock.TickCounter}] Early Rollback to tick [{tick}]");

                    // Solo es necesario hacer rollback al tick mas reciente que cumpla la condicion de early rollback
                    SimClock.Instance.StopClock();
                    Rollback(tick-1);
                    m_snapshotBuffer[tick].SetStatus(SnapshotStatus.CompletePrediction);
                    SimClock.Instance.ResumeClock();
                    return;
                }
            }
            
        }

        public void EvaluateForReconciliation(SimulationSnapshot newAuthSnapshot)
        {
            if ((newAuthSnapshot.Timestamp <= m_lastAuthSnapshotTimestamp) ||
                (newAuthSnapshot.Timestamp < m_snapshotBuffer.First.Timestamp))
            {
                if (DEBUG) Debug.Log($"NOT Reconciling with snapshot[{newAuthSnapshot.Timestamp}] (Too OLD)."+
                                    $"Oldest predicted snapshot[{m_snapshotBuffer.First}]."+
                                    $"Newest auth snapshot[{m_lastAuthSnapshotTimestamp}]."+
                                    $"Snapshot Buff Size: {m_snapshotBuffer.Capacity}");
                
                // Desync detection, sort of a patch but meh
                m_desyncCounter++;
                if (m_desyncCounter > MAX_DESYNC_COUNT)
                {
                    MessageHandlers.Instance.RequestSync();
                    m_desyncCounter = 0;
                }

                return;
            }

            m_lastAuthSnapshotTimestamp = newAuthSnapshot.Timestamp;

            SimClock.Instance.StopClock();

            // Jump forward in time to future sim state received in auth snapshot
            if (newAuthSnapshot.Timestamp >= SimClock.TickCounter)
            {
                if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]Jumping forward to future state[{newAuthSnapshot.Timestamp}]");
                
                SimClock.Instance.SetClock(newAuthSnapshot.Timestamp);
                SetSimulation(newAuthSnapshot);
                m_snapshotBuffer.Add(newAuthSnapshot, newAuthSnapshot.Timestamp);

                SimClock.Instance.ResumeClock();
                return;
            }


            if (DEBUG) Debug.Log("["+SimClock.TickCounter+"]"+"Evaluating Desync for: "+ m_snapshotBuffer.Get(newAuthSnapshot.Timestamp));

            SimulationSnapshot predictedSnapshot = m_snapshotBuffer.Where(s => s.Timestamp == newAuthSnapshot.Timestamp 
                                                                    && s.Status == SnapshotStatus.Predicted).FirstOrDefault();

            if (!predictedSnapshot.Equals(default(SimulationSnapshot)))
            {
                bool predictedObjectsMissMatch = predictedSnapshot.IDs.Except(newAuthSnapshot.IDs).Union(newAuthSnapshot.IDs.Except(predictedSnapshot.IDs)).Count() > 0;

                // We skip Delta comparison stage if we had wrongfuly predicted the objects in the snapshot,
                // if that's the case we force the rollback.
                if (predictedObjectsMissMatch == false)
                {
                    foreach (var objId in newAuthSnapshot.IDs)
                    {
                        if (DEBUG) Debug.Log($"[{SimClock.TickCounter}] Comparing [{objId}] prediction to auth state");
                        if (CheckForDesync(predictedSnapshot[objId].state, newAuthSnapshot[objId].state))
                        {
                            if (DEBUG)
                            {
                                Debug.Log($"[{SimClock.TickCounter}]Rolling back to [{newAuthSnapshot.Timestamp}]"+
                                $"\nBecause {objId} dind't meet the delta Thresholds");
                            }
                            
                            Rollback(newAuthSnapshot);
                            break;
                        }
                    }
                }
                else
                {
                    if (DEBUG) Debug.Log($"[{SimClock.TickCounter}] Rolling back to [{newAuthSnapshot.Timestamp}] because: Object Missmatch at predicted snapshot");
                    
                    Rollback(newAuthSnapshot);
                }

                m_snapshotBuffer.Add(newAuthSnapshot, newAuthSnapshot.Timestamp);
            }
            else
            {
                // To make sure it's marked as auth for the future
                m_snapshotBuffer.Add(newAuthSnapshot, newAuthSnapshot.Timestamp);
            }
            
            SimClock.Instance.ResumeClock();
        }

        /// <summary>
        /// WARNING!!! Calling this function is only safe when the <see cref="SimClock"/> is NOT active (stopped).
        /// It is recommended that calls to it are done between <see cref="SimClock.StopClock"/> and <see cref="SimClock.ResumeClock"/>.
        /// </summary>
        /// <param name="tick"></param>
        private void Rollback(int tick)
        {
            if (m_snapshotBuffer.Any(s => s.Timestamp == tick))
            {
                Rollback(m_snapshotBuffer[tick]);
            }
            else
            {
                //Debug.LogWarning($"Tick [{tick}] out of snapshot buffer bounds (snapshot with such timestamp not saved)");
            }
        }

        /// <summary>
        /// Strict setting of the simulation state to the auth is a pre-requisite of rollback.
        /// This also implies which simulationObjects are registered/present in the simulation (because Simulate is a global action).
        /// <para/> WARNING!!! Calling this function is only safe when the <see cref="SimClock"/> is NOT active (stopped).
        /// It is recommended that calls to it are done between <see cref="SimClock.StopClock"/> and <see cref="SimClock.ResumeClock"/>.
        /// </summary>
        /// <param name="authSnapshot"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void Rollback(SimulationSnapshot authSnapshot)
        {
            if (SimClock.Instance.Active) throw new InvalidOperationException($"[{SimClock.TickCounter}]The {SimClock.Instance} must be stopped while calling Rollback");

            m_rollbackTick = authSnapshot.Timestamp;

            SetSimulation(authSnapshot);
            // At this point all registered simObjs (e.g simulation objects in scene) should be the
            // same as the ones stored on the incoming auth snapshot.

            // Iterate over the simulation objects, they should already only be those present in the
            // incoming state and set input components into replay mode for resimulation purposes.

            // Setting of the state SetSimulation() to reconciliation evaluator function.
            foreach(var objId in m_simulationObjects.Keys)
            {
                if (authSnapshot.ContainsId(objId))
                {
                    // Put Tanks into input replay mode so they pull the correct (past) inputs from their respective buffers.
                    if (m_simulationObjects[objId] is TankSimulationObject tank)
                    {
                        tank.StartInputReplay(m_rollbackTick);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"[{SimClock.TickCounter}] Present simulation object miss-match! {objId} is not present in authSnapshot[{authSnapshot.Timestamp}]");
                }
            }

            // Sanity check --> NOT VIABLE BECAUSE OF BULLET DEFERRED SPAWNING!
            // foreach(var objId in authSnapshot.IDs)
            // {
            //     if (!m_simulationObjects.ContainsKey(objId))
            //     {
            //         throw new InvalidOperationException($"[{SimClock.TickCounter}] Present simulation object miss-match! {objId} from authSnapshot[{authSnapshot.timestamp}] is not present in current simulation state! ");
            //     }
            // }

            // We must resimulate all ticks up to (and including) the "present" tick,
            // in order to catch back up to our simlation predictions and not fall back in sim ticks,
            // which would cause a desync.
            while(m_rollbackTick </*=*/ SimClock.TickCounter)
            {
                m_rollbackTick++;
                Simulate();
            }

            // Set tank's input components back on live input mode
            foreach(var obj in m_simulationObjects.Values)
            {
                if (obj!= null && obj is  TankSimulationObject tank)
                {
                    var lastReplayTick = tank.StopInputReplay();
                    if (DEBUG) Debug.Log($"Tank({tank.NetworkObjectId})'s last replayed input was on Tick- {lastReplayTick}");
                }
            }

            m_rollbackTick = NO_ROLLBACK;
        }

        /// <summary>
        /// This sets the Simulation State to be the same as <paramref name="snapshotToSet"/>. This also includes 
        /// the necessary destruction/creation of simulation objects in order to match the objects present in the snapshot.
        /// <para/> WARNING!!! Calling this function is only safe when the <see cref="SimClock"/> is NOT active (stopped),
        /// it is recommended calls to it are done between <see cref="SimClock.StopClock"/> and <see cref="SimClock.ResumeClock"/>.
        /// </summary>
        /// <param name="snapshotToSet"></param>
        public void SetSimulation(SimulationSnapshot snapshotToSet)
        {
            if (SimClock.Instance.Active) throw new InvalidOperationException($"[{SimClock.TickCounter}]The {SimClock.Instance} must be stopped while calling SetSimulation");


            foreach(var objId in m_simulationObjects.Keys.ToArray()) // perform copy to be able to modify the dict properly
            {
                if (!snapshotToSet.ContainsId(objId))
                {
                    // We must despawn/remove those objects that are present in our simulation but not on the snapshot.
                    m_simulationObjects[objId].OnNetworkDespawn();
                }
            }

            foreach(var objId in snapshotToSet.IDs)
            {
                bool deferredSpawning = false;

                if (!m_simulationObjects.ContainsKey(objId))
                {
                    // We must spawn objects missing from our current simObjs to match states
                    if (snapshotToSet[objId].type == SimulationObjectType.Bullet)
                    {
                        var bulletState = (BulletSimulationState)snapshotToSet[objId].state;
                        
                        // If it's the bullet's first tick of lifetime then we reject the spawn attempt,
                        // because it must mean it will be spawned this same tick after calling Simulate's
                        // kinematic functions (from which bullets spawn).
                        if (bulletState.LifeTime >= SimClock.SimDeltaTime*2)
                        {
                            var ownerId =  ((BulletSimulationState)snapshotToSet[objId].state).OwnerId;
                            var newBullet = BulletPool.Instance.Get(bulletState.Position, ownerId, objId, autoSpawn:false);
                            //Manually added to simulation, because we require the SimClock to be inactive during set simulation calls.
                            newBullet.OnNetworkSpawn();
                        }
                        else
                        {
                            deferredSpawning = true;
                            if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]Handing spawning attempt over to reconciliation (remote client input replay) because {objId}'s lifetime is lower than 2 ticks (it was spawned on tick[{snapshotToSet.Timestamp}])");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"[{SimClock.TickCounter}]SimulationObjects doesn't contain [{objId}] and it isn't a bullet, feature not implemented!");
                    }
                }
                /*
                else if (snapshotToSet[objId].type == SimulationObjectType.Bullet && (BulletSimulationObject).)
                {
                    // Handle the fact that bullets must not exist before simulation when 
                }
                */

                if (!deferredSpawning)
                {
                    m_simulationObjects[objId].SetSimState(snapshotToSet[objId].state);
                }
            }
        }

        private bool CheckForDesync(in ISimulationState simStateA,in ISimulationState simStateB)
        {
            IStateDelta delta = SimExtensions.Delta(simStateA, simStateB);
            if(simStateA is TankSimulationState tankStateA && simStateB is TankSimulationState tankStateB)
            {
                if (DEBUG) Debug.Log($"\t[{CaptureSnapshotTick}] Comparing: {tankStateA} ~= {tankStateB}");
                TankDelta tankDelta = (TankDelta)delta;
                return SimExtensions.CompareDeltas(tankDelta, m_tankSimulationTolerance);
            }
            else if(simStateA is BulletSimulationState bulletStateA && simStateB is BulletSimulationState bulletStateB)
            {
                if (DEBUG) Debug.Log($"\t[{CaptureSnapshotTick}] Comparing: {bulletStateA} ~= {bulletStateB}");
                BulletDelta bulletDelta = (BulletDelta)delta;
                return SimExtensions.CompareDeltas(bulletDelta, m_bulletSimulationTolerance);
            }

            Debug.LogException(new InvalidOperationException("Desync check failed, simObj type mismatch: " + simStateA.GetType() + "-" + simStateB.GetType()));
            return false;
        }

        #region DEBUG_TESTING_METHODS

        [ContextMenu("TestGetSet")]
        public void TestGetSet()
        {
            ISimulationState stateToCopy = m_simulationObjects[0].GetSimState();
            m_simulationObjects[1].SetSimState(stateToCopy);
        }

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
            
            SimClock.Instance.StopClock();
            SetSimulation(pastSnapshot);
            SimClock.Instance.ResumeClock();
        }

        [ContextMenu("TestRollback")]
        public void TestRollback()
        {
            SimulationSnapshot testSnapShot = m_snapshotBuffer.Get(SimClock.TickCounter - 50);
            testSnapShot.SetTimestamp(testSnapShot.Timestamp-50);
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