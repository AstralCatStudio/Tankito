using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tankito
{

    public enum SceneToLoad
    {
        mainMenu,
        lobby
    }

    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Singleton;
        [SerializeField] private bool DEBUG = false;
        public SceneToLoad m_startingScene;
        void Awake()
        {
            if (Singleton == null) Singleton = this;
            else Destroy(this);

            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            //LoadMainMenu();
            loadScene(m_startingScene);


        }
        void loadScene(SceneToLoad scene)
        {
            switch (scene)
            {
                case SceneToLoad.lobby:
                    LoadEmptyCamera();
                    LoadLobby();
                    break;
                case SceneToLoad.mainMenu:
                    LoadMainMenu();
                    break;
            }
        }

        public void LoadEmptyCamera()
        {
            SceneManager.LoadScene("EmptyCamera", LoadSceneMode.Additive);
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

        public void ReloadMainMenu()
        {
            StartCoroutine("ReloadMainMenuAsync");
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

            if (NetworkManager.Singleton.IsServer)
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

        IEnumerator ReloadMainMenuAsync()
        {
            if (!SceneManager.GetSceneByName("GameInitTest").IsValid()) throw new InvalidOperationException("You shouldn't be reloading this scene without having loaded the game scene!");

            SceneManager.LoadScene("Loading");

            GameManager.Instance.UnloadScene();

            if (DEBUG) Debug.Log("Loading Main Menu...");

            ClientData.Instance.firstLoad = false;

            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);

            if (DEBUG) Debug.Log("Main Menu Loaded!");

            SceneManager.UnloadSceneAsync("Loading");
        }

    }

}