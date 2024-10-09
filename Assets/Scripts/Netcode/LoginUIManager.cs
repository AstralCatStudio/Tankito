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
using Unity.Networking.Transport;

namespace Tankito.Netcode
{
        public class LoginGUIManager : MonoBehaviour
    {
        const int MAX_CONNECTIONS = 4;
        public static readonly string CONNECTION_TYPE = (Application.platform == RuntimePlatform.WebGLPlayer) ? "wss" : "dtls";
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
            Region
        }

        void Start()
        {
            #if UNITY_WEBGL
            var useWebSockets = true;
            #else
            var useWebSockets = false;
            #endif

            ((UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport).UseWebSockets = useWebSockets;
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            switch(m_stage)
            {
                case GUIStages.Mode:
                    if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
                    {
                        StartButtons();
                    }
                    else
                    {
                        StatusLabels();
                    }

                break;

                case GUIStages.Region:
                    RegionSelection();
                break;
            }
            GUILayout.EndArea();
        }

        async void StartButtons()
        {
            if (GUILayout.Button("Host"))
                if (m_region != "") StartHost();
                else { m_connectionMode = ConnectionMode.Host; await FetchRegions(); return; }
            if (GUILayout.Button("Server"))
                if (m_region != "") StartServer();
                else { m_connectionMode = ConnectionMode.Server; await FetchRegions(); return; }
            if (GUILayout.Button("Client"))
                { m_connectionMode = ConnectionMode.Client; StartClient(); }

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
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_CONNECTIONS);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, CONNECTION_TYPE));
            m_joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Relay Allocation Created");
        }

        async void StartHost()
        {
            await CreateRelayAllocation();
            Debug.Log("Debug point B");
            TextEditor te = new TextEditor(); te.text = m_joinCode; te.SelectAll(); te.Copy();
            NetworkManager.Singleton.StartHost();
        }

        async void StartClient()
        {
            await LoginUnityAuth();

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: m_joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, CONNECTION_TYPE));
            NetworkManager.Singleton.StartClient();
        }

        async void StartServer()
        {
            await CreateRelayAllocation();
            TextEditor te = new TextEditor(); te.text = m_joinCode; te.SelectAll(); te.Copy();
            NetworkManager.Singleton.StartServer();
        }
    }
}

