using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Tankito;
using Tankito.InputFields;

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

        [SerializeField]
        GameObject buttonPrefab;
        [SerializeField]
        GameObject inputFieldPrefab;

        private TMP_InputField joinCodeInputField;

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

        private void Awake()
        {
            GetComponent<Canvas>().worldCamera = Camera.main;
            m_joinCode = "Enter room code...";
        }
        void Start()
        {
            ((UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport).UseWebSockets = USE_WEB_SOCKETS;
            Debug.Log("UseWebSockets: " + USE_WEB_SOCKETS);

            m_region = "";

            StartButtons();
        }

        /*void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 500, 500));
            switch(m_stage)
            {
                case GUIStages.Mode:
                    if (!buttonsCreated)
                    {
                        StartButtons(); 
                    }
                    break;

                case GUIStages.Region: RegionSelection(); break;

                case GUIStages.Run: StatusLabels(); break;
            }
            GUILayout.EndArea();
        }*/

        private void StartButtons()
        {
            transform.GetChild(0).gameObject.SetActive(true);

            GameObject buttonHost = GameObject.Instantiate(buttonPrefab, transform.GetChild(0));
            ConfigButton(buttonHost, HostButton, "Host");

            /*GameObject buttonServer = GameObject.Instantiate(buttonPrefab, transform.GetChild(0));
            ConfigButton(buttonServer, ServerButton, "Server");*/

            GameObject buttonClient = GameObject.Instantiate(buttonPrefab, transform.GetChild(0));
            ConfigButton(buttonClient, ClientButton, "Client");

            GameObject inputField = GameObject.Instantiate(inputFieldPrefab, transform.GetChild(0));
            joinCodeInputField = inputField.GetComponent<TMP_InputField>();
            inputField.GetComponent<JoinCodeField>().OnEnterPressedEvent.AddListener(ClientButton);
        }

        void RegionSelection()
        {
            transform.GetChild(1).gameObject.SetActive(true);

            foreach (var reg in m_relayRegions)
            {
                Debug.Log($"{reg.desc}: {reg.id}");
                GameObject buttonRegion = GameObject.Instantiate(buttonPrefab, transform.GetChild(1).GetChild(0).GetChild(0));
                ConfigRegionButton(buttonRegion, reg.desc, reg.id);
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
            //m_stage = GUIStages.Region;
            transform.GetChild(0).gameObject.SetActive(false);
            RegionSelection();
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
            //TextEditor te = new TextEditor(); te.text = m_joinCode; te.SelectAll(); te.Copy();
            CopyToClipboard(m_joinCode);

            GameManager.Instance.OnSceneLoading();

            NetworkManager.Singleton.StartHost();

            SceneLoader.Singleton.LoadGameScene();
            GameManager.Instance.joinCode = m_joinCode;
        }

        async void StartClient()
        {
            await LoginUnityAuth();

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: m_joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, CONNECTION_TYPE));

            GameManager.Instance.OnSceneLoading();

            NetworkManager.Singleton.StartClient();

            SceneLoader.Singleton.LoadGameScene();
            GameManager.Instance.joinCode = m_joinCode;
        }

        async void StartServer()
        {
            await CreateRelayAllocation();
            TextEditor te = new TextEditor(); te.text = m_joinCode; te.SelectAll(); te.Copy();

            GameManager.Instance.OnSceneLoading();

            NetworkManager.Singleton.StartServer();

            SceneLoader.Singleton.LoadGameScene();
            GameManager.Instance.joinCode = m_joinCode;
        }

        #region Buttons
        private void ConfigButton(GameObject button, UnityEngine.Events.UnityAction func, string text)
        {
            button.GetComponent<Button>().onClick.AddListener(func);
            button.GetComponentInChildren<TextMeshProUGUI>().text = text;
        }

        async void HostButton()
        {
            if (m_region != "") StartHost();
            else
            {
                m_connectionMode = ConnectionMode.Host; await FetchRegions(); return;
            }
        }

        async void ServerButton()
        {
            if (m_region != "") StartServer();
            else
            {
                m_connectionMode = ConnectionMode.Server; await FetchRegions(); return;
            }
        }

        private void ClientButton()
        {
            m_connectionMode = ConnectionMode.Client;
            m_joinCode = joinCodeInputField.text;
            StartClient();
        }

        private void ConfigRegionButton(GameObject button, string text, string regId)
        {
            button.GetComponent<Button>().onClick.AddListener(() => RegionButton(regId));
            button.GetComponentInChildren<TextMeshProUGUI>().text = text;
        }

        private void RegionButton(string regId)
        {
            Debug.Log(regId);

            m_region = regId;
            switch (m_connectionMode)
            {
                case ConnectionMode.Server: StartServer(); break;
                case ConnectionMode.Host: StartHost(); break;
            }
        }

        #endregion

        private void CopyToClipboard(string text)
        {
            CopyToClipboard copyToClipboard = gameObject.AddComponent<CopyToClipboard>();
            copyToClipboard.CopyTextToClipboard(text);
        }
    }
}

