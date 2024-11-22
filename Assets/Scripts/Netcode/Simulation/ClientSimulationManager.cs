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
        int SNAPSHOT_BUFFER_SIZE { get => Parameters.CLIENT_INPUT_WINDOW_SIZE; }
        CircularBuffer<SimulationSnapshot> m_snapshotBuffer;

        /// <summary>
        /// Relates NetworkClientId(ulong) to a specific <see cref="EmulatedTankInput"/>.  
        /// </summary>
        public Dictionary<ulong, EmulatedTankInput> emulatedInputTanks = new Dictionary<ulong,EmulatedTankInput>();

        [SerializeField] private TankDelta m_tankSimulationTolerance;
        [SerializeField] private BulletDelta m_bulletSimulationTolerance;

        [SerializeField] private bool DEBUG;

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
            m_snapshotBuffer.Add(newSnapshot, newSnapshot.timestamp);
        }

        public void EvaluateForReconciliation(SimulationSnapshot newAuthSnapshot)
        {
            if ((newAuthSnapshot.timestamp <= m_lastAuthSnapshotTimestamp) ||
                (newAuthSnapshot.timestamp < m_snapshotBuffer.Last.timestamp))
            {
                if (DEBUG) Debug.Log($"NOT Reconciling with snapshot[{newAuthSnapshot.timestamp}] (Too OLD)."+
                                    $"Oldest predicted snapshot[{m_snapshotBuffer.Last}]."+
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
            
            if (DEBUG) Debug.Log("Evaluating Desync for: "+ m_snapshotBuffer.Get(newAuthSnapshot.timestamp));

            SimulationSnapshot predictedSnapshot = m_snapshotBuffer.Where(s => s.timestamp == newAuthSnapshot.timestamp 
                && s.status == SnapshotStatus.Predicted).FirstOrDefault();

            if (!predictedSnapshot.Equals(default(SimulationSnapshot)))
            {
                foreach (var objSnapShot in predictedSnapshot.Keys)
                {
                    if (newAuthSnapshot.ContainsKey(objSnapShot))
                    {
                        if (CheckForDesync(predictedSnapshot[objSnapShot], newAuthSnapshot[objSnapShot]))
                        {
                            if (DEBUG)
                            {
                                Debug.Log($"[{SimClock.TickCounter}]Rolling back to [{newAuthSnapshot.timestamp}]"+
                                $"\nBecause {objSnapShot.NetworkObjectId} dind't meet the delta Thresholds");
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

            foreach(var obj in m_simulationObjects.Values)
            {
                obj.SetSimState(authSnapshot[obj]);

                
                // Put Input Components into replay mode
                if(obj is TankSimulationObject tank)
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
                if (obj is  TankSimulationObject tank)
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
            
            foreach(var obj in newSimSnapshot.Keys)
            {
                if (m_simulationObjects.Values.Contains(obj))
                {
                    obj.SetSimState(newSimSnapshot[obj]);
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
            foreach(var obj in pastSnapshot.Keys)
            {
                if (lastSnapshot.ContainsKey(obj))
                {
                    desyncs += $"[{obj.NetworkObjectId}]-> " + CheckForDesync(lastSnapshot[obj], pastSnapshot[obj]) + "   ";
                }
                else
                {
                    desyncs += $"[{obj.NetworkObjectId}]-> missing in LastSnapshot. ";
                }
            }
            foreach(var obj in lastSnapshot.Keys)
            {
                if (!pastSnapshot.ContainsKey(obj))
                {
                    desyncs += $"[{obj.NetworkObjectId}]-> missing in PastSnapshot. ";
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