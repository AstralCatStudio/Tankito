using System;
using System.Collections.Generic;
using System.Linq;
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

        private GlobalSimulationSnapshot AuthSnapshot
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
                var newSnapshot = new GlobalSimulationSnapshot();
                newSnapshot.Initialize();
                newSnapshot.timestamp = ClockManager.TickCounter;
                
                foreach(var simObj in simulationObjects)
                {
                    newSnapshot[simObj] = simObj.GetSimState();
                }
                m_snapshotBuffer[newSnapshot.timestamp] = newSnapshot;
        }

        public void Rollback()
        {
            int rollbackCounter = AuthSnapshot.timestamp;
            
            //Pause Simulation Clock
            ClockManager.Instance.StopClock();

            foreach(var obj in simulationObjects)
            {
                obj.SetSimState(AuthSnapshot[obj]);
                // Put Input Components into replay mode
                if(obj is TankSimulationObject tank)
                {
                    tank.StartInputReplay(rollbackCounter);
                }
                // Habra que hacer algo para restaurar objetos que puedieran haber deespawneado y todo eso supongo
            }
            
            while(rollbackCounter < ClockManager.TickCounter)
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
            ClockManager.Instance.StartClock();
        }

        
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
            int rewindTicks = (int)(rewindTime/ClockManager.SimDeltaTime);
            // Warp back 1s in time
            int warpTick = ClockManager.TickCounter-rewindTicks;
            var pastSnapshot = m_snapshotBuffer[warpTick];

            if (pastSnapshot.Equals(default))
            {
                throw new IndexOutOfRangeException($"No snapshot to warp to at tick - {warpTick}");
            }

            ClockManager.Instance.StopClock();
            
            foreach(var obj in pastSnapshot.objectSnapshots.Keys)
            {
                if (simulationObjects.Contains(obj))
                {
                    obj.SetSimState(pastSnapshot[obj]);
                }
            }

            ClockManager.Instance.ResumeClock();
        }
    }
}