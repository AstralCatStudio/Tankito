using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tankito.Utils;

namespace Tankito.SyncronizedButtons
{
    public class ReadyButton : SyncButton
    {
        protected override void Awake()
        {
            base.Awake();

            _text.gameObject.SetActive(true);
        }

        protected override void Start()
        {
            base.Start();
            if (IsClient)
            {
                _button.gameObject.SetActive(true);
            }
        }

        #region Button

        protected override void OnClickedCountChanged(NetworkListEvent<ButtonClickStatus> playAgainCountChanged)
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

                if (allReady && _buttonClickStatusList.Count > 1)
                {
                    FinalFunction();
                }
            }

            UpdateText();
            UpdateLocalText();

            ///////////////////////////////////////////////////////////////////// sound effect
            MusicManager.Instance.PlaySound("aceptar");
        }

        protected override void UpdateText()
        {
            _text.text = $"Ready players: {CalcClickedCount()} / {_buttonClickStatusList.Count}";
        }

        protected override void UpdateLocalText()
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
                            _button.GetComponentInChildren<TextMeshProUGUI>().text = status.ButtonClicked ? "Ready!" : "Not ready";
                        }
                        Debug.Log(status.ButtonClicked ? "Ready!" : "Not ready");
                        break;
                    }
                }
            }
        }

        #endregion

        #region ButtonFunc

        protected override void FinalFunction()
        {
            base.FinalFunction();
            StartGame();
        }

        private void StartGame()
        {
            if (IsServer)
            {
                RoundUI.Instance.PanelPowerUps = Instantiate(RoundUI.Instance.PowerUpsPrefab, GameInstanceParent.Instance.transform);
                RoundUI.Instance.PanelPowerUps.GetComponent<NetworkObject>().Spawn();
                RoundManager.Instance.StartRoundCountdown();
            }
        }

        #endregion
    }

}