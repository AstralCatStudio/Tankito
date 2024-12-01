using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;

namespace Tankito.SyncronizedButtons
{
    public class SyncButton : NetworkBehaviour
    {
        [SerializeField] protected Button _button;
        [SerializeField] protected TMP_Text _text;

        [Serializable]
        protected struct ButtonClickStatus : INetworkSerializable, IEquatable<ButtonClickStatus>
        {
            public ulong ClientId;
            public bool ButtonClicked;

            public ButtonClickStatus(ulong clientId, bool buttonClicked)
            {
                ClientId = clientId;
                ButtonClicked = buttonClicked;
            }

            public bool Equals(ButtonClickStatus other)
            {
                throw new NotImplementedException();
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref ClientId);
                serializer.SerializeValue(ref ButtonClicked);
            }
        }

        protected NetworkList<ButtonClickStatus> _buttonClickStatusList;

        protected virtual void Awake()
        {
            _buttonClickStatusList = new NetworkList<ButtonClickStatus>();

            UpdateText();
        }

        private void Update()
        {
            //Debug.Log($"Clientes que han clicado: {CalcReadyCount()} {_readyStatusList.Count}");
        }

        protected virtual void Start()
        {
            if (IsClient)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _buttonClickStatusList.OnListChanged += OnClickedCountChanged;
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

        #region ClientConnection

        public void OnClientConnected(ulong clientId)
        {
            _buttonClickStatusList.Add(new ButtonClickStatus { ClientId = clientId, ButtonClicked = false });
        }

        public void OnClientDisconnected(ulong clientId)
        {
            for (int i = 0; i < _buttonClickStatusList.Count; i++)
            {
                if (_buttonClickStatusList[i].ClientId == clientId)
                {
                    _buttonClickStatusList.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion

        #region Button

        private void OnButtonClicked()
        {
            Debug.Log($"{_button.gameObject.name} button clicked");
            SetButtonClickedServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetButtonClickedServerRpc(ulong clientId)
        {
            // Cambia el estado de clicado del cliente en la lista
            for (int i = 0; i < _buttonClickStatusList.Count; i++)
            {
                if (_buttonClickStatusList[i].ClientId == clientId)
                {
                    var status = _buttonClickStatusList[i];
                    status.ButtonClicked = !status.ButtonClicked;
                    _buttonClickStatusList[i] = status;
                    break;
                }
            }
        }

        protected virtual void OnClickedCountChanged(NetworkListEvent<ButtonClickStatus> playAgainCountChanged)
        {
            if (IsServer)
            {
                bool allReady = true;

                foreach (var status in _buttonClickStatusList)
                {
                    if (!status.ButtonClicked)
                    {
                        allReady = false;
                        break;
                    }
                }

                if (allReady)
                {
                    FinalFunction();
                }
            }

            UpdateText();
            UpdateLocalText();
        }

        private void ResetButtonState()
        {
            for (int i = 0; i < _buttonClickStatusList.Count; i++)
            {
                var status = _buttonClickStatusList[i];
                status.ButtonClicked = false;
                _buttonClickStatusList[i] = status;
            }
        }

        protected int CalcClickedCount()
        {
            int clickedCount = 0;
            foreach (var status in _buttonClickStatusList)
            {
                if (status.ButtonClicked)
                {
                    clickedCount++;
                }
            }
            return clickedCount;
        }

        protected virtual void UpdateText()
        {
            _text.text = $"{CalcClickedCount()} / {_buttonClickStatusList.Count}";
        }

        protected virtual void UpdateLocalText()
        {
            if (IsClient)
            {
                ulong clientId = NetworkManager.Singleton.LocalClientId;
                foreach (var status in _buttonClickStatusList)
                {
                    if (status.ClientId == clientId)
                    {
                        if (_button.GetComponentInChildren<TextMeshProUGUI>().text != null)
                        {
                            _button.GetComponentInChildren<TextMeshProUGUI>().text = status.ButtonClicked ? "Clicked" : "Not clicked";
                        }
                        Debug.Log(status.ButtonClicked ? "Cliked" : "Not clicked");
                        break;
                    }
                }
            }
        }

        #endregion

        #region ButtonFunc

        // Overridear con la funcion que se quiera
        protected virtual void FinalFunction()
        {
            Debug.Log("Button final function called");
            if (IsServer)
            {
                ActivateButtonClientRpc(false);
                ResetButtonState();
            }
        }

        [ClientRpc]
        public void ActivateButtonClientRpc(bool active)
        {
            ActivateButton(active);
        }

        public void ActivateButton(bool active)
        {
            _button.gameObject.SetActive(active);
            _text.gameObject.SetActive(active);
            GetComponent<CanvasGroup>().blocksRaycasts = active;
            GetComponent<CanvasGroup>().interactable = active;
        }

        #endregion

    }

}
