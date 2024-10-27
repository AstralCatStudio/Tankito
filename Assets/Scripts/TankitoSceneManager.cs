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
        // TODO: Implement Dictionary or the likes, that provides simple enum interface for scripting but allows to change scene names easily in the Unity Editor (perhaps copy Untiy "internal" way of doing such things idk)
        /*public enum SceneNames
        {
            Launch,
            Loading,
            Lobby,
            Game

            
        }*/


        void Awake()
        {
            if (Singleton == null) Singleton = this;
            else Destroy(this);

            DontDestroyOnLoad(gameObject);
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

            Debug.Log("Loading Lobby...");
            yield return SceneManager.LoadSceneAsync("Lobby", LoadSceneMode.Additive);
            Debug.Log("Lobby Loaded!");

            SceneManager.UnloadSceneAsync("Loading");
        }

        IEnumerator LoadGameScene()
        {
            if (!SceneManager.GetSceneByName("Lobby").IsValid()) throw new InvalidOperationException("You shouldn't be loading the game scene without having loaded the Lobby!");

            SceneManager.LoadScene("Loading");

            Debug.Log("Loading Game...");

            if (NetworkManager.Singleton.IsServer)
            {
                yield return NetworkManager.Singleton.SceneManager.LoadScene("BulletTesting", LoadSceneMode.Additive);
            }
            Debug.Log("Game Loaded!");

            SceneManager.UnloadSceneAsync("Loading");
        }

    }

}