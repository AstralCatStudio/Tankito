using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tankito
{

    public enum SceneToLoad{
        lobby,
        mainMenu,
        single_player
    }
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Singleton;
        [SerializeField] private bool DEBUG = false;
        public SceneToLoad escenaInicial;
        void Awake()
        {
            if (Singleton == null) Singleton = this;
            else Destroy(this);

            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            //LoadMainMenu();
            loadScene(escenaInicial);
        }

        void loadScene(SceneToLoad scene)
        {
            switch (scene)
            {
                case SceneToLoad.lobby:
                    LoadLobby();
                    break;

                case SceneToLoad.mainMenu:
                    LoadMainMenu();
                    break;

                case SceneToLoad.single_player:
                    LoadSinglePlayerLobby();
                    break;
            }
        }

        private void LoadSinglePlayerLobby()
        {
            StartCoroutine("LoadSinglePlayerLobbyAsync");
        }

        internal void LoadSinglePlayerGameScene()
        {
            StartCoroutine("LoadSinglePlayerGameSceneAsync");
        }

        public void LoadLobby()
        {
            StartCoroutine("LoadLobbyAsync");
        }

        public void LoadMainMenu()
        {
            StartCoroutine("LoadMainMenuAsync");
        }

        public void LoadGameScene()
        {
            StartCoroutine("LoadGameSceneAsync");
        }

        IEnumerator LoadSinglePlayerLobbyAsync()
        {
            SceneManager.LoadScene("Loading", LoadSceneMode.Additive);

            if (DEBUG) Debug.Log("Loading SinglePlayer Lobby...");
            yield return SceneManager.LoadSceneAsync("SinglePlayer_Lobby", LoadSceneMode.Additive);
            if (DEBUG) Debug.Log("SinglePlayer Lobby Loaded!");
            
        }

        IEnumerator LoadSinglePlayerGameSceneAsync()
        {
            yield return SceneManager.UnloadSceneAsync("SinglePlayer_Lobby");
            if (DEBUG) Debug.Log("SinglePlayer Lobby Unloaded (finished spinning up server)!");
            
            if (DEBUG) Debug.Log("Loading SinglePlayer GameScene...");
            if(NetworkManager.Singleton.IsServer)
            {
                yield return NetworkManager.Singleton.SceneManager.LoadScene("SinglePlayer_Game", LoadSceneMode.Additive);
            }
            else
            {
                throw new InvalidOperationException("Should be spinning up the local server for single player");
            }
            if (DEBUG) Debug.Log("SinplePlayer Game Scene Loaded!");

            SceneManager.UnloadSceneAsync("Loading");
        }

        IEnumerator LoadLobbyAsync()
        {
            SceneManager.LoadScene("Loading", LoadSceneMode.Additive);

            if (DEBUG) Debug.Log("Loading Lobby...");
            yield return SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Additive);
            if (DEBUG) Debug.Log("Lobby Loaded!");

            SceneManager.UnloadSceneAsync("Loading");
        }

        IEnumerator LoadMainMenuAsync()
        {
            SceneManager.LoadScene("Loading", LoadSceneMode.Additive);

            if (DEBUG) Debug.Log("Loading Main Menu...");
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
            if (DEBUG) Debug.Log("Main Menu Loaded!");

            SceneManager.UnloadSceneAsync("Loading");
        }

        IEnumerator LoadGameSceneAsync()
        {
            if (!SceneManager.GetSceneByName("Lobby").IsValid()) throw new InvalidOperationException("You shouldn't be loading the game scene without having loaded the Lobby!");

            SceneManager.LoadScene("Loading");

            if (DEBUG) Debug.Log("Loading Game...");


            //SceneManager.LoadSceneAsync("GameInitTest", LoadSceneMode.Additive);

            if(NetworkManager.Singleton.IsServer)
            {
                yield return NetworkManager.Singleton.SceneManager.LoadScene("GameInitTest", LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.LoadScene("GameInitTest", LoadSceneMode.Additive);
            }

            if (DEBUG) Debug.Log("Game Loaded!");

            SceneManager.UnloadSceneAsync("Loading");
        }
    }

}