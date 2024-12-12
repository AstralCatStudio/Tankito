using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
namespace Tankito
{
    public class AnimatedModifiers : NetworkBehaviour
    {
        [Header("Params")]
        #region parameters
        [SerializeField, Range(0, 2)] private float popupTime = 0.5f;
        [SerializeField, Range(0, 2)] private float waitTime = 0.5f;
        [SerializeField, Range(0, 2)] private float shellTime = 0.5f;
        [SerializeField, Range(0, 2)] private float playerTransitionTime = 0.5f;
        [SerializeField, Range(0, 2)] private float playerScale = 1.5f;
        [SerializeField, Range(2, 4)] private int numPlayers = 2;
        private int turn = -1;

        [SerializeField] private RectTransform panelRT;
        [SerializeField] private GameObject shellPrefab;
        [SerializeField] private GameObject modifierPrefab;

        [SerializeField] private RectTransform playerChoosingPosition;
        [SerializeField] private GameObject otherPlayerPrefab;
        [SerializeField] private GameObject otherPlayersPanel;
        private Vector2 originalPlayerPosition;

        [SerializeField] private Transform row1;
        [SerializeField] private Transform row2;
        [SerializeField] private List<GameObject> shells;
        List<Modifier> modifiers;
        #endregion

        #region removeThis
        [Header("Remove this params")]
        private List<TankData> players = new();
        private Color[] colors = { Color.blue, Color.red, Color.green, Color.yellow };
        [SerializeField] private Sprite[] icons;
        #endregion

        #region UnityFunctions
        public override void OnNetworkSpawn()
        {
            
            base.OnNetworkSpawn();
            transform.SetParent(RoundUI.Instance.transform);
            RoundUI.Instance.PanelPowerUps = gameObject;
            gameObject.SetActive(false);
            players = RoundManager.Instance.Players.Values.ToList<TankData>();
            numPlayers = players.Count;
            Debug.Log("spawning" + numPlayers);

            //Instancia los objetos de los modificadores
            for (int i = 0; i < numPlayers + 1; i++) //Cambiarlo para que se haga por cada jugador que haya en partida (Creo que por los TankDatas)
            {
                GameObject instance;
                if (i / 3 == 0)
                {
                    instance = Instantiate(shellPrefab, row1);
                }
                else if (i / 3 == 1)
                {
                    instance = Instantiate(shellPrefab, row2);
                }
                else
                {
                    instance = null;
                }

                if (instance != null)
                    shells.Add(instance);
                instance.GetComponent<Button>().enabled = false;
                instance.GetComponent<HoverButton>().enabled = false;
            }
            if (IsServer)
            {
                GenerateNewModifiers();
            }
            //Instancia los objetos donde irá la información del resto de jugadores
            foreach (TankData player in players)
            {
                
                if (player.IsOwner)
                {
                    
                    player.SetClientDataServerRpc(player.Username, player.SkinSelected);
                }
            }
            foreach (TankData player in players)
            {
                GameObject instance;
                instance = Instantiate(otherPlayerPrefab, otherPlayersPanel.transform);
                player.playerInfo = instance;
                OtherPlayersLoadInfo(player);
            }

            LeanTween.scale(panelRT, Vector2.zero, 0f);
        }

        private void OnEnable()
        {
            if (IsSpawned)
            {
                
                SortPlayers();

                foreach (TankData player in players)
                {
                    UpdateValues(player);
                }

                LeanTween.scale(panelRT, Vector2.one, popupTime).setEase(LeanTweenType.easeOutElastic);
                shells[0].GetComponent<ShellAnimation>().onAnimationFinished += StartChoosing;
            }
            

        }
        #endregion

        private void OtherPlayersLoadInfo(TankData player)
        {
            
            OtherP_LoadInfo otherP = player.playerInfo.GetComponent<OtherP_LoadInfo>();
            otherP.icon.sprite = ClientData.Instance.characters[player.SkinSelected].data.characterIcon;
            Debug.Log(player.Username);
            otherP.username.text = player.Username;
            otherP.username.color = player.playerColor;
            otherP.score.text = player.Points.ToString();
            otherP.position.text = player.position.ToString() + ".";
        }

        private void UpdateValues(TankData player)
        {
            OtherP_LoadInfo otherP = player.playerInfo.GetComponent<OtherP_LoadInfo>();
            otherP.score.text = player.Points.ToString();
            otherP.position.text = player.position.ToString() + ".";
        }

        private void SortPlayers()
        {
            MusicManager.Instance.PlaySoundPitch("snd_concha");
            Debug.Log("sorting" + players.Count);
            players.Sort();

            for (int i = 0; i < numPlayers; i++)
            {
                players[i].playerInfo.transform.SetSiblingIndex((numPlayers - 1) - i);
            }

            //Pone las posiciones de los jugadores, y se asegura de que, si dos tienen la misma puntuación, están en la misma posicion
            for (int i = 0; i < numPlayers; i++)
            {
                int position = i + 1;
                players[i].position = position;
                while (i + 1 < numPlayers && players[i].Points == players[i + 1].Points)
                {
                    players[i + 1].position = position;
                    i++;
                }
            }
        }

        private void StartChoosing()
        {
            turn = 0;
            int currentIndex = (numPlayers - 1) - turn;
            DisableButtonsModifiers();
            AnimatePlayerEnable(players[currentIndex]);
        }

        private void AnimatePlayerEnable(TankData player)
        {
            MusicManager.Instance.PlaySoundPitch("snd_concha_choose");
            originalPlayerPosition = player.playerInfo.GetComponent<RectTransform>().anchoredPosition;
            RectTransform playerRT = player.playerInfo.GetComponent<RectTransform>();
            LeanTween.move(playerRT, playerChoosingPosition.anchoredPosition, playerTransitionTime).setEase(LeanTweenType.easeInOutCubic);
            LeanTween.scale(playerRT, Vector3.one * playerScale, playerTransitionTime).setEase(LeanTweenType.easeInOutCubic);

            if (player.OwnerClientId  == NetworkManager.Singleton.LocalClientId)
            {
                Invoke("EnableButtonsModifiers", playerTransitionTime);
            }
            
        }

        private void AnimateModifierAppearingInPlayer(GameObject instance)
        {
            RectTransform rt = instance.GetComponent<RectTransform>();
            LeanTween.scale(rt, Vector2.zero, 0);
            LeanTween.scale(rt, Vector2.one, waitTime).setEase(LeanTweenType.easeOutBack);
        }

        private void AnimatePlayerDisable(TankData player)
        {
            MusicManager.Instance.PlaySoundPitch("snd_concha_select");
            DisableButtonsModifiers();
            RectTransform playerRT = player.playerInfo.GetComponent<RectTransform>();
            LeanTween.move(playerRT, originalPlayerPosition, playerTransitionTime).setEase(LeanTweenType.easeInOutCubic);
            LeanTween.scale(playerRT, Vector3.one, playerTransitionTime).setEase(LeanTweenType.easeInOutCubic);

            Invoke("ChangeTurn", playerTransitionTime);
        }

        /// <summary>
        /// Activa los modificadores y hace que se vea el sprite de la concha abierto.
        /// </summary>
        public void EnableModifiers()
        {
            ShellAnimation shellAnimation;
            //Para que los modificadores puedan ser seleccionables, llamamos a Enable
            foreach (GameObject shell in shells)
            {
                shellAnimation = shell.GetComponent<ShellAnimation>();
                shellAnimation.Enable();
            }
        }

        /// <summary>
        /// Activa la funcionalidad de los botones de los modificadores
        /// </summary>
        public void EnableButtonsModifiers()
        {
            ShellAnimation shellAnimation;
            //Para que los modificadores puedan ser seleccionables, llamamos a Enable
            foreach (GameObject shell in shells)
            {
                shellAnimation = shell.GetComponent<ShellAnimation>();
                shellAnimation.EnableButton();
            }
        }

        /// <summary>
        /// Desactiva la funcionalidad de los botones de los modificadores
        /// </summary>
        public void DisableButtonsModifiers()
        {
            ShellAnimation shellAnimation;
            //Para que los modificadores puedan ser seleccionables, llamamos a Enable
            foreach (GameObject shell in shells)
            {
                shellAnimation = shell.GetComponent<ShellAnimation>();
                shellAnimation.DisableButton();
            }
        }

        /// <summary>
        /// Desactiva los modificadores y hace que se vea el sprite de la concha abierto.
        /// </summary>
        public void DisableModifiers()
        {
            ShellAnimation shellAnimation;
            //Para que los modificadores puedan ser seleccionables, llamamos a Enable
            foreach (GameObject shell in shells)
            {
                shellAnimation = shell.GetComponent<ShellAnimation>();
                shellAnimation.Disable();
            }
        }

        /// <summary>
        /// Primero comprueba cual es el modificador seleccionado. Si el jugador no ha elegido ninguno, no hace nada. En caso de que lo haya elegido, da pie a las animaciones correspondientes.
        /// </summary>
        public void TryNextTurn()
        {
            GameObject shellSelected = CheckSelected();
            
            

            if (shellSelected == null)
            {
                Debug.LogWarning("You didn´t choose any modifier");
            }
            else
            {
                TryNextTurnServerRpc(shells.IndexOf(shellSelected), NetworkManager.Singleton.LocalClientId);
                
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void TryNextTurnServerRpc(int shellSelected, ulong client)
        {
            int currentIndex = (numPlayers - 1) - turn;
            if (players[currentIndex] == RoundManager.Instance.Players[client])
            {
                //turn++;
                BulletCannonRegistry.Instance[client].transform.parent.parent.parent.GetComponent<ModifiersController>().AddModifier(shells[shellSelected].GetComponent<ShellAnimation>().modifier);
                AddModifierClientRpc(client, shellSelected);
                goNextTurnClientRpc(shellSelected);
                
            }
        }
        [ClientRpc]
        void goNextTurnClientRpc(int shellSelected)
        {
            shells[shellSelected].GetComponent<ShellAnimation>().SetAlreadyTaken(true);
            shells[shellSelected].GetComponent<ShellAnimation>().Disable(); //desactiva el potenciador elegido
            int currentIndex = (numPlayers - 1) - turn;
            GameObject parent = players[currentIndex].playerInfo.GetComponent<OtherP_LoadInfo>().modifiers;
            GameObject instance = Instantiate(modifierPrefab, parent.transform);
            instance.GetComponent<Image>().sprite = shells[shellSelected].GetComponent<ShellAnimation>().modifier.GetSprite();
            DeselectAllModifiers();
            AnimateModifierAppearingInPlayer(instance);
            AnimatePlayerDisable(players[currentIndex]);
        }
        [ClientRpc]
        void AddModifierClientRpc(ulong playerClientId, int shellSelected)
        {
            if (!IsServer)
            {
                BulletCannonRegistry.Instance[playerClientId].transform.parent.parent.parent.GetComponent<ModifiersController>().AddModifier(shells[shellSelected].GetComponent<ShellAnimation>().modifier);

            }
        }
        private void ChangeTurn()
        {
            turn++;
            if (turn >= numPlayers)
            {
                Disappear();
                RoundManager.Instance.EndPowerUpSelection();
            }
            else
            {
                int currentIndex = (numPlayers - 1) - turn;
                AnimatePlayerEnable(players[currentIndex]);
            }
        }

        private GameObject CheckSelected()
        {
            ShellAnimation selection;
            foreach (GameObject shell in shells)
            {
                selection = shell.GetComponent<ShellAnimation>();
                if (selection.selected)
                {
                    return shell;
                }
            }
            return null;
        }

        public void DeselectAllModifiers()
        {
            foreach (GameObject s in shells)
            {
                s.GetComponent<ShellAnimation>().selected = false;
                s.GetComponent<Outline>().enabled = false;
            }
        }

        public void Disappear()
        {
            LeanTween.scale(panelRT, Vector2.zero, popupTime).setEase(LeanTweenType.easeInBack);
            DeselectAllModifiers();
            foreach (GameObject s in shells)
            {
                s.GetComponent<Button>().enabled = false;
                s.GetComponent<HoverButton>().enabled = false;
                s.GetComponent<ShellAnimation>().SetAlreadyTaken(false);
            }
            Invoke("Disable", popupTime);
        }

        private void Disable()
        {
            GenerateNewModifiers();
            gameObject.SetActive(false);
            shells[0].GetComponent<ShellAnimation>().onAnimationFinished -= StartChoosing;
        }


        [ClientRpc]
        void SyncronizeModifiersClientRpc(int[] modificadores)
        {
            for (int i = 0; i < shells.Count; i++)
            {
                shells[i].GetComponent<ShellAnimation>().modifier= ModifierRegistry.Instance.GetModifier(modificadores[i]);
            }
        }
        void GenerateNewModifiers()
        {
            modifiers = ModifierRegistry.Instance.GetRandomModifiers(6);
            List<int> indexModificadores = new List<int>();
            for (int i = 0; i < shells.Count; i++)
            {
                indexModificadores.Add(ModifierRegistry.Instance.GetModifierIndex(modifiers[i]));
                shells[i].GetComponent<ShellAnimation>().modifier = modifiers[i];
            }
            SyncronizeModifiersClientRpc(indexModificadores.ToArray());
        }
    }
}
