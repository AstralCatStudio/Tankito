using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System;

namespace Tankito.SyncronizedButtons
{
    public class PlayAgainButton : SyncButton
    {
        public void ActivatePlayAgain(bool active)
        {
            if (IsClient)
            {
                _button.gameObject.SetActive(active);
                _text.gameObject.SetActive(active);
            }
        }

        #region Button

        protected override void UpdateText()
        {
            _text.text = $"Players to play again: {CalcClickedCount()} / {_buttonClickStatusList.Count}";
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
                            _button.GetComponentInChildren<TextMeshProUGUI>().text = status.ButtonClicked ? "Let's Play!" : "Play Again";
                        }
                        Debug.Log(status.ButtonClicked ? "Let's Play!" : "Play Again");
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
            ResetGame();
        }

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
