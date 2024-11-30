using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System;

namespace Tankito
{
    public class PlayAgainButton : NetworkBehaviour
    {
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private TMP_Text _playersText;

        [Serializable]
        private struct PlayAgainStatus : INetworkSerializable, IEquatable<PlayAgainStatus>
        {
            public ulong ClientId;
            public bool PlayAgain;

            public PlayAgainStatus(ulong clientId, bool isReady)
            {
                ClientId = clientId;
                PlayAgain = isReady;
            }

            public bool Equals(PlayAgainStatus other)
            {
                throw new NotImplementedException();
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref ClientId);
                serializer.SerializeValue(ref PlayAgain);
            }
        }

        private NetworkList<PlayAgainStatus> _playAgainStatusList;
        private void Awake()
        {
            _playAgainStatusList = new NetworkList<PlayAgainStatus>();

            UpdatePlayersText();
        }

        private void Update()
        {
            //Debug.Log($"Clientes listos en la lista: {CalcReadyCount()} {_readyStatusList.Count}");
        }

        private void Start()
        {
            if (IsClient)
            {
                _playAgainButton.onClick.AddListener(OnPlayAgainClicked);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _playAgainStatusList.OnListChanged += OnPlayersCountChanged;
            if (IsServer)
            {
                //Debug.Log("Entro en network spawn");
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

                if (IsClient)
                {
                    OnClientConnected(NetworkManager.Singleton.LocalClientId);
                }
            }
        }

        public override void OnDestroy()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
            base.OnDestroy();
        }

        public void ActivatePlayAgain(bool active)
        {
            if (IsClient)
            {
                _playAgainButton.gameObject.SetActive(active);
                _playersText.gameObject.SetActive(active);
            }
        }

        #region ClientConnection
        private void OnClientConnected(ulong clientId)
        {
            _playAgainStatusList.Add(new PlayAgainStatus { ClientId = clientId, PlayAgain = false });
        }

        private void OnClientDisconnected(ulong clientId)
        {
            for (int i = 0; i < _playAgainStatusList.Count; i++)
            {
                if (_playAgainStatusList[i].ClientId == clientId)
                {
                    _playAgainStatusList.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion

        #region PlayAgain
        private void OnPlayAgainClicked()
        {
            Debug.Log("Pulsaste Play Again");
            SetPlayAgainServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetPlayAgainServerRpc(ulong clientId)
        {
            // Cambia el estado de "Lets play" del cliente en la lista
            for (int i = 0; i < _playAgainStatusList.Count; i++)
            {
                if (_playAgainStatusList[i].ClientId == clientId)
                {
                    var status = _playAgainStatusList[i];
                    status.PlayAgain = !status.PlayAgain;
                    _playAgainStatusList[i] = status;
                    break;
                }
            }
        }

        private void OnPlayersCountChanged(NetworkListEvent<PlayAgainStatus> playAgainCountChanged)
        {
            if (IsServer)
            {
                bool allReady = true;

                foreach (var status in _playAgainStatusList)
                {
                    if (!status.PlayAgain)
                    {
                        allReady = false;
                        break;
                    }
                }

                if (allReady && _playAgainStatusList.Count > 1)
                {
                    ResetGame();
                }
            }

            UpdatePlayersText();
            UpdateLocalText();
        }

        private int CalcReadyCount()
        {
            int playAgainCount = 0;
            foreach (var status in _playAgainStatusList)
            {
                if (status.PlayAgain)
                {
                    playAgainCount++;
                }
            }
            return playAgainCount;
        }

        private void UpdatePlayersText()
        {
            _playersText.text = $"{CalcReadyCount()} / {_playAgainStatusList.Count}";
        }

        private void UpdateLocalText()
        {
            if (IsClient)
            {
                ulong clientId = NetworkManager.Singleton.LocalClientId;
                foreach (var status in _playAgainStatusList)
                {
                    if (status.ClientId == clientId)
                    {
                        if (_playAgainButton.GetComponentInChildren<TextMeshProUGUI>().text != null)
                        {
                            _playAgainButton.GetComponentInChildren<TextMeshProUGUI>().text = status.PlayAgain ? "Let's Play!" : "Play Again";
                        }
                        Debug.Log(status.PlayAgain ? "Let's Play!" : "Play Again");
                        break;
                    }
                }
            }
        }

        #endregion

        #region ResetGame

        public void ResetGame()
        {
            if (IsServer)
            {
                Debug.LogWarning("Reseting game scene...");
                //Llamada a scene loader
                DestroyButtonClientRpc();
            }
        }

        [ClientRpc]
        private void DestroyButtonClientRpc()
        {
            Destroy(gameObject);
        }

        #endregion
    }
}
