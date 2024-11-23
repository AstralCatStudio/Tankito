using System;
using System.Collections.Generic;
using System.Linq;
using Tankito.Netcode.Messaging;
using Tankito.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Tankito.Netcode.Simulation
{
    public class ClientSimulationManager : NetSimulationManager<ClientSimulationManager>
    {
        //GlobalSimulationSnapshot m_authSnapshot;
        const int SNAPSHOT_BUFFER_SIZE = 256;
        CircularBuffer<SimulationSnapshot> m_snapshotBuffer = new CircularBuffer<SimulationSnapshot>(SNAPSHOT_BUFFER_SIZE);

        /// <summary>
        /// Relates NetworkClientId(ulong) to a specific <see cref="RemoteTankInput"/>.  
        /// </summary>
        public Dictionary<ulong, EmulatedTankInput> emulatedInputTanks = new Dictionary<ulong,EmulatedTankInput>();

        [SerializeField] private TankDelta m_tankSimulationTolerance;// = new TankDelta(new Vector2(0.1f,0.1f), 1f, new Vector2(0.1f,0.1f), 1f, 0);
        [SerializeField] private BulletDelta m_bulletSimulationTolerance;// = new BulletDelta(new Vector2(0.1f,0.1f), 1f, new Vector2(0.1f,0.1f));

        [SerializeField] private bool DEBUG;// = true;

        private SimulationSnapshot AuthSnapshot //Por como funciona el rollback, igual esto no hace falta y unicamente podemos necesitar 
                                                //que se guarde el timestamp
        {
            get
            {
                var authStates = m_snapshotBuffer.Where(s => s.status == SnapshotStatus.Authoritative);
                return (authStates.Count() > 0) ? authStates.MaxBy(s => s.timestamp) : default;
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
            DEBUG = false;
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
            m_snapshotBuffer.Add(newSnapshot, newSnapshot.timestamp);
        }
        
        // public bool ContainedSimObj(ulong simObjId, int tick)
        // {
        //     return m_snapshotBuffer[tick].ContainsId(simObjId);
        // }

        public void EvaluateForReconciliation(SimulationSnapshot newAuthSnapshot)
        {
            /*Debug.Log($"[{SimClock.TickCounter}]Se recibe snapshot[{newAuthSnapshot.timestamp}] autoritativo");
            Debug.Log(AuthSnapshot.timestamp + " " + m_snapshotBuffer.Last.timestamp + " " + m_snapshotBuffer.Count);*/
            
            if ((!AuthSnapshot.Equals(default(SimulationSnapshot)) && newAuthSnapshot.timestamp <= AuthSnapshot.timestamp) ||
                (m_snapshotBuffer.Full() && newAuthSnapshot.timestamp < (m_snapshotBuffer.Last.timestamp - m_snapshotBuffer.Count)))
            {
                return;
            }

            // Jump forward in time to sim state
            if (newAuthSnapshot.timestamp >= SimClock.TickCounter)
            {
                Debug.Log($"[{SimClock.TickCounter}]Jumping forward to future state[{newAuthSnapshot.timestamp}]");
                SimClock.Instance.SetClock(newAuthSnapshot.timestamp);
                SetSimulation(newAuthSnapshot);
                m_snapshotBuffer.Add(newAuthSnapshot, newAuthSnapshot.timestamp);
                return;
            }


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
                        if (predictedSnapshot[authObjId].type == SimulationObjectType.Bullet)
                        {
                            // Si es su 1er tick de vida, dejamos que intente el propio rollback instanciar la bala
                            if (((BulletSimulationState)predictedSnapshot[authObjId].state).LifeTime >= SimClock.SimDeltaTime*2)
                            {
                                missingObjects = true;
                                var ownerId =  ((BulletSimulationState)predictedSnapshot[authObjId].state).OwnerId;
                                BulletPool.Instance.Get(authObjId, ownerId);
                                if (DEBUG) Debug.Log($"[{SimClock.TickCounter}]reconBullet successfully added to sim? => " + m_simulationObjects.ContainsKey(authObjId));
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

            int rollbackCounter = authSnapshot.timestamp;
            
            //Pause Simulation Clock
            SimClock.Instance.StopClock();

            // We DON'T have to re-simulate the tick which we are getting as auth,
            // because it's already simulated. So just advance the counter
            rollbackCounter++;
            
            foreach(var objId in m_simulationObjects.Keys)
            {
                if (authSnapshot.ContainsId(objId))
                {
                    m_simulationObjects[objId].SetSimState(authSnapshot[objId].state);
                }
                else
                {
                    // Remove miss-predicted object from simulation
                    m_simulationObjects[objId].OnNetworkDespawn();
                }
                
                // Put Input Components into replay mode
                if(m_simulationObjects[objId] is TankSimulationObject tank)
                {
                    tank.StartInputReplay(rollbackCounter);
                }
                // Habra que hacer algo para restaurar objetos que puedieran haber deespawneado y todo eso supongo
            }
            
            while(rollbackCounter < SimClock.TickCounter)
            {
                // - Input Replay - DONE => Implicitly consumes inputs from input caches when pulling InputPayloads on Kinematic Functions
                Simulate();
                rollbackCounter++;
            }

            // Set tank's input components back on live input mode
            foreach(var obj in m_simulationObjects.Values)
            {
                if (obj!= null && obj is  TankSimulationObject tank)
                {
                    var lastReplayTick = tank.StopInputReplay();
                    //if (DEBUG) Debug.Log($"Tank({tank.NetworkObjectId})'s last replayed input was on Tick- {lastReplayTick}");
                }
            }

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
            
            Debug.Log("Thresholds check: " + desyncs);
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