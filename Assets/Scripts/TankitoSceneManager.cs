using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tankito
{
    public class TankitoSceneManager : MonoBehaviour
    {
        public static TankitoSceneManager Singleton;
        // TODO: Implement Dictionary or the likes, that provides simple enum interface for scripting but allows to change scene names easily in the Unity Editor (perhaps copy Untiy "internal" way of doing such things idk)
        /*public enum SceneNames
        {
            Launch,
            Loading,
            Lobby,
            Game

            
        }*/
        private List<string> m_loadedScenes;


        void Awake()
        {
            if (Singleton == null) Singleton = this;
            else Destroy(this);

            DontDestroyOnLoad(gameObject);

            m_loadedScenes = new List<string>();
            m_loadedScenes.Add("Launch");
        }

        void Start()
        {
            LoadLobbyAsync();
        }

        public void LoadLobbyAsync()
        {
            StartCoroutine("LoadLobby");
        }

        public void LoadGameSceneAsync()
        {
            StartCoroutine("LoadGameScene");
        }

        IEnumerator LoadLobby()
        {
            SceneManager.LoadScene("Loading", LoadSceneMode.Additive);
            m_loadedScenes.Add("Loading");

            Debug.Log("Loading Lobby...");
            yield return SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Additive);
            m_loadedScenes.Add("Lobby");
            Debug.Log("Lobby Loaded!");

            SceneManager.UnloadSceneAsync("Loading");
            m_loadedScenes.Remove("Loading");
        }

        IEnumerator LoadGameScene()
        {
            if (!m_loadedScenes.Contains("Lobby")) throw new InvalidOperationException("You shouldn't be loading the game scene without having loaded the Lobby!");

            SceneManager.LoadScene("Loading");
            m_loadedScenes.Add("Loading");

            Debug.Log("Loading Game...");
            yield return NetworkManager.Singleton.SceneManager.LoadScene("InputTesting", LoadSceneMode.Additive);
            m_loadedScenes.Add("InputTesting");
            Debug.Log("Game Loaded!");

            GameManager.Instance.gameSceneLoaded = true;
            if (NetworkManager.Singleton.IsHost) GameManager.Instance.CreatePlayer();
            GameManager.Instance.FindPlayerInput();
            GameManager.Instance.BindInputActions();

            SceneManager.UnloadSceneAsync("Loading");
            m_loadedScenes.Remove("Loading");
        }

    }

}