using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tankito
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Singleton;


        void Awake()
        {
            if (Singleton == null) Singleton = this;
            else Destroy(this);

            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            LoadMainMenu();
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

        IEnumerator LoadLobbyAsync()
        {
            SceneManager.LoadScene("Loading", LoadSceneMode.Additive);

            Debug.Log("Loading Lobby...");
            yield return SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Additive);
            Debug.Log("Lobby Loaded!");

            SceneManager.UnloadSceneAsync("Loading");
        }

        IEnumerator LoadMainMenuAsync()
        {
            SceneManager.LoadScene("Loading", LoadSceneMode.Additive);

            Debug.Log("Loading Main Menu...");
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
            Debug.Log("Main Menu Loaded!");

            SceneManager.UnloadSceneAsync("Loading");
        }

        IEnumerator LoadGameSceneAsync()
        {
            if (!SceneManager.GetSceneByName("Lobby").IsValid()) throw new InvalidOperationException("You shouldn't be loading the game scene without having loaded the Lobby!");

            SceneManager.LoadScene("Loading");

            Debug.Log("Loading Game...");

            
            SceneManager.LoadSceneAsync("GameInitTest", LoadSceneMode.Additive);


            yield return NetworkManager.Singleton.SceneManager.LoadScene("BulletTesting", LoadSceneMode.Additive);

            Debug.Log("Game Loaded!");

            SceneManager.UnloadSceneAsync("Loading");
        }

    }

}