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
        CircularBuffer<GlobalSimulationSnapshot> m_snapshotBuffer = new CircularBuffer<GlobalSimulationSnapshot>(SNAPSHOT_BUFFER_SIZE);

        /// <summary>
        /// Relates NetworkClientId(ulong) to a specific <see cref="RemoteTankInput"/>.  
        /// </summary>
        public Dictionary<ulong, EmulatedTankInput> emulatedInputTanks = new Dictionary<ulong,EmulatedTankInput>();

        private GlobalSimulationSnapshot AuthSnapshot //Por como funciona el rollback, igual esto no hace falta y unicamente podemos necesitar 
                                                      //que se guarde el timestamp
        {
            get => m_snapshotBuffer
                .Where(s => s.state == SnapshotState.Authoritative)
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
                GlobalSimulationSnapshot newSnapshot = CaptureSnapshot();
                m_snapshotBuffer[newSnapshot.timestamp] = newSnapshot;
        }

        public void Rollback(GlobalSimulationSnapshot authSnapshot)
        {
            int rollbackCounter = authSnapshot.timestamp;
            
            //Pause Simulation Clock
            SimClock.Instance.StopClock();
            Debug.Log("Se inicia reconciliacion");

            foreach(var obj in simulationObjects)
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
                // ---TODO: Input Replay--- DONE => Implicitly consumes inputs from input caches when pulling InputPayloads on Kinematic Functions
                Simulate();
                rollbackCounter++;
            }

            // Set tank's input components back on live input mode
            foreach(var tank in simulationObjects.OfType<TankSimulationObject>())
            {
                var lastReplayTick = tank.StopInputReplay();
                Debug.Log($"{tank}'s last replayed input was on Tick- {lastReplayTick}");
            }

            //Resume Simulation Clock
            SimClock.Instance.StartClock();
        }

        // WIP !!
        public void SetSimulation(GlobalSimulationSnapshot newSimSnapshot)
        {
            SimClock.Instance.StopClock();
            
            foreach(var obj in newSimSnapshot.objectSnapshots.Keys)
            {
                if (simulationObjects.Contains(obj))
                {
                    obj.SetSimState(newSimSnapshot[obj]);
                }
            }

            SimClock.Instance.ResumeClock();
        }

        public void CheckNewGlobalSnapshot(GlobalSimulationSnapshot newAuthSnapshot)
        {
            Debug.Log("Se recibe estado autoritativo");
            if (newAuthSnapshot.timestamp <= AuthSnapshot.timestamp) return;
            GlobalSimulationSnapshot clientSnapShot = m_snapshotBuffer.Where(s => s.timestamp == newAuthSnapshot.timestamp).FirstOrDefault();
            foreach(var objSnapShot in clientSnapShot.objectSnapshots.Keys)
            {
                if (newAuthSnapshot.objectSnapshots.ContainsKey(objSnapShot))
                {
                    if (clientSnapShot.objectSnapshots[objSnapShot].CheckReconcilation(newAuthSnapshot.objectSnapshots[objSnapShot]))
                    {
                        Rollback(newAuthSnapshot);
                        break;
                    }
                }
            }
            newAuthSnapshot.state = SnapshotState.Authoritative;
            m_snapshotBuffer.Add(newAuthSnapshot, newAuthSnapshot.timestamp);
        }

        #region DEBUG_TESTING_METHODS

        [ContextMenu("TestGetSet")]
        public void TestGetSet()
        {
            ISimulationState stateToCopy = simulationObjects[0].GetSimState(); // Explicit casting is not necessary
            simulationObjects[1].SetSimState(stateToCopy);
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
            GlobalSimulationSnapshot testSnapShot = m_snapshotBuffer.Get(SimClock.TickCounter - 50, true);
            testSnapShot.timestamp -= 50;
            CheckNewGlobalSnapshot(testSnapShot);
        }

        [ContextMenu("TestInputWindowMessaging")]
        public void TestInputWindowMessaging()
        {
            MessageHandlers.Instance.SendInputWindowToServer(InputWindowBuffer.Instance.inputWindow);
        }

        #endregion
    }
}