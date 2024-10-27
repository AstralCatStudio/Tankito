using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

namespace Tankito.Netcode
{
        public class LoginGUIManager : MonoBehaviour
    {
        const int MAX_CONNECTIONS = 4;
        public static readonly string CONNECTION_TYPE =

        #if UNITY_WEBGL
        "wss";
        const bool USE_WEB_SOCKETS = true;
        #else
        "dtls";
        const bool USE_WEB_SOCKETS = false;
        #endif


        string m_joinCode = "Enter room code...";
        public static string m_region = "";
        private List<(string id, string desc)> m_relayRegions;
        private GUIStages m_stage;
        private ConnectionMode m_connectionMode;

        private enum ConnectionMode
        {
            Client,
            Server,
            Host
        }

        private enum GUIStages
        {
            Mode,
            Region,
            Run
        }

        void Start()
        {
            ((UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport).UseWebSockets = USE_WEB_SOCKETS;
            Debug.Log("UseWebSockets: " + USE_WEB_SOCKETS);
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 500, 500));
            switch(m_stage)
            {
                case GUIStages.Mode: StartButtons(); break;

                case GUIStages.Region: RegionSelection(); break;

                case GUIStages.Run: StatusLabels(); break;
            }
            GUILayout.EndArea();
        }

        async void StartButtons()
        {
            if (GUILayout.Button("Host"))
            {
                if (m_region != "") StartHost();
                else {
                    m_connectionMode = ConnectionMode.Host; await FetchRegions(); return;
                }
            }
            if (GUILayout.Button("Server"))
            {
                if (m_region != "") StartServer();
                else {
                    m_connectionMode = ConnectionMode.Server; await FetchRegions(); return;
                }
            }
            if (GUILayout.Button("Client"))
            {
                m_connectionMode = ConnectionMode.Client; StartClient();
            }

            m_joinCode = GUILayout.TextField(m_joinCode);
        }

        void RegionSelection()
        {            
            foreach(var reg in m_relayRegions)
            {
                if(GUILayout.Button(reg.desc))
                {
                    m_region = reg.id;
                    switch (m_connectionMode)
                    {
                        case ConnectionMode.Server: StartServer(); break;
                        case ConnectionMode.Host: StartHost(); break;
                    }
                    m_stage = GUIStages.Run;
                }
            }
        }

        void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ?
                "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
            GUILayout.Label("Region: " + m_region);
            GUILayout.Label("Transport: " +
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + m_connectionMode);
            GUILayout.Label("Room: " + m_joinCode);

        }

        async Task FetchRegions()
        {
            await LoginUnityAuth();
            m_relayRegions = await RelayRegionFetcher.FetchRelayRegionsAsync(AuthenticationService.Instance.AccessToken);
            m_stage = GUIStages.Region;
        }

        async Task LoginUnityAuth()
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }

        async Task CreateRelayAllocation()
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_CONNECTIONS, m_region);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, CONNECTION_TYPE));
            m_joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Relay Allocation Created - " + "Region: " + m_region
                        + "Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name
                        + "Mode: " + m_connectionMode
                        + "Room: " + m_joinCode);
        }

        async void StartHost()
        {
            await CreateRelayAllocation();
            Debug.Log("Debug point B");
            TextEditor te = new TextEditor(); te.text = m_joinCode; te.SelectAll(); te.Copy();
            NetworkManager.Singleton.StartHost();
            
            SceneLoader.Singleton.LoadGameScene();
        }

        async void StartClient()
        {
            await LoginUnityAuth();

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: m_joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, CONNECTION_TYPE));
            NetworkManager.Singleton.StartClient();

            SceneLoader.Singleton.LoadGameScene();
        }

        async void StartServer()
        {
            await CreateRelayAllocation();
            TextEditor te = new TextEditor(); te.text = m_joinCode; te.SelectAll(); te.Copy();
            NetworkManager.Singleton.StartServer();

            SceneLoader.Singleton.LoadGameScene();
        }
    }
}
