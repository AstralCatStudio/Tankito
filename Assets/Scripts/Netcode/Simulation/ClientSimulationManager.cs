using System;
using System.Collections.Generic;
using System.Linq;
using Tankito.Netcode.Messaging;
using Tankito.Utils;
using Unity.Netcode;
using UnityEditor.PackageManager.UI;
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
        [SerializeField] private TankDelta m_tankSimulationTolerance;
        [SerializeField] private BulletDelta m_bulletSimulationTolerance;

        private SimulationSnapshot AuthSnapshot //Por como funciona el rollback, igual esto no hace falta y unicamente podemos necesitar 
                                                      //que se guarde el timestamp
        {
            get => m_snapshotBuffer
                .Where(s => s.status == SnapshotStatus.Authoritative)
                .OrderByDescending(s => s.timestamp)
                .FirstOrDefault();
        }

        void Start()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                Debug.LogWarning("ClientSimulationManager is network node that is NOT a CLIENT (is server). this should not happen!");
                Destroy(this);
            }
            m_tankSimulationTolerance = new TankDelta(new Vector2(0.1f,0.1f), 0.1f, new Vector2(0.1f,0.1f), 0.1f, 100000);
            m_bulletSimulationTolerance = new BulletDelta(new Vector2(0.1f,0.1f), 0.1f, new Vector2(0.1f,0.1f));
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
                m_snapshotBuffer.Add(newSnapshot, newSnapshot.timestamp);
        }

        public void Rollback(SimulationSnapshot authSnapshot)
        {
            int rollbackCounter = authSnapshot.timestamp;
            
            //Pause Simulation Clock
            SimClock.Instance.StopClock();
            Debug.Log("Se inicia reconciliacion");

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
            foreach(var tank in m_simulationObjects.OfType<TankSimulationObject>())
            {
                var lastReplayTick = tank.StopInputReplay();
                Debug.Log($"{tank}'s last replayed input was on Tick- {lastReplayTick}");
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

        public void EvaluateForReconciliation(SimulationSnapshot newAuthSnapshot)
        {
            Debug.Log($"[{SimClock.TickCounter}]Se recibe snapshot[{newAuthSnapshot.timestamp}] autoritativo");
            Debug.Log(AuthSnapshot.timestamp + " " + m_snapshotBuffer.Last.timestamp + " " + m_snapshotBuffer.Count);
            if (m_snapshotBuffer.Count > 0 && newAuthSnapshot.timestamp >= AuthSnapshot.timestamp && (!m_snapshotBuffer.Full() || newAuthSnapshot.timestamp < (m_snapshotBuffer.Last.timestamp - m_snapshotBuffer.Count)))
            {        
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
                                Rollback(newAuthSnapshot);
                                break;
                            }
                        }
                    }
                } 
            }
            
            //newAuthSnapshot.status = SnapshotStatus.Authoritative;   DEBE LLEGAR AUTH DESDE SERVER
            m_snapshotBuffer.Add(newAuthSnapshot, newAuthSnapshot.timestamp);
        }

        // Bernat: A lo mejor no queremos hacer esto asi y queremos implementar una metrica mas sofisticada para decidir si reconciliar o no,
        // de momento lo dejo como esta con el bool por simplicidad.
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
            return false;
        }

        #region DEBUG_TESTING_METHODS

        [ContextMenu("TestGetSet")]
        public void TestGetSet()
        {
            ISimulationState stateToCopy = m_simulationObjects[0].GetSimState(); // Explicit casting is not necessary
            m_simulationObjects[1].SetSimState(stateToCopy);
        }

        [ContextMenu("StateComparison")]
        public void StateComparison()
        {
            TankSimulationState tankA = new TankSimulationState(Vector2.right, 90, Vector2.zero, 0, TankAction.None);
            TankSimulationState tankB = new TankSimulationState(Vector2.right*0.9f, 90, Vector2.zero, 0, TankAction.Fire);
            Debug.Log(SimExtensions.Delta(tankA, tankB));
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
            
            Debug.Log("DeltaSnapshot: " + deltas.Select(d => d.ToString()));

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
            SimulationSnapshot testSnapShot = m_snapshotBuffer.Get(SimClock.TickCounter - 50, true);
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