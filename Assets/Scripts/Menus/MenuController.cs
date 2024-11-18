using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Tankito
{
    public class MenuController : Singleton<MenuController>
    {
        public enum menuName
        {
            InitialScreenLogIn = 0,
            MainMenu = 1,
            Settings = 2,
            Credits = 3,
            LogOut = 4,
            Shop = 5,
            PlayMenu = 6,
            Lobby = 7,
            CharacterSelection = 8,
            Singleplayer = 9
        }
        public GameObject[] menus;
        public GameObject[] bgMenus;
        public Vector2[] bgPositions; //Posiciones del "grid"
        public ParticleSystem bubbleParticles;
        public float duration = 1f;
        
        public int currentMenuIndex;
        public float xUnit = 56, yUnit = 30;  
        public float bgParallaxFactor = 1.2f, mgParallaxFactor = 1f, fgParallaxFactor = 0.7f;

        public UnityEvent<Vector2,GameObject,GameObject> menuChanged;

        // Start is called before the first frame update
        void Start()
        {
            BgReposition();
        }

        // Update is called once per frame
        void Update()
        {

        }
        
        public void ChangeToMenu(int newMenuIndex)
        {
            if (menus[currentMenuIndex].activeSelf)
            {
                menus[currentMenuIndex].GetComponent<MenuAnimations>().DisablingAnimation();
            }
            menus[newMenuIndex].gameObject.SetActive(true);
            
            MoveBG(newMenuIndex);
            currentMenuIndex = newMenuIndex;
        }

        public void SceneLoaderCallback(string functionIdx)
        {
            SceneLoader.Singleton.Invoke(functionIdx,0);
        }

        private void MoveBG(int newMenuIndex)
        {
            Transform bg;
            Transform mg;
            Transform fg;
            //newTranslation determina la direccion en la que se mueven los fondos
            Vector2 newTranslation = bgPositions[currentMenuIndex] - bgPositions[newMenuIndex];
            Vector2 bubbleDirection = newTranslation.normalized;

            // Calcula el �ngulo en el plano 2D
            float angle = Mathf.Atan2(-bubbleDirection.x, bubbleDirection.y) * Mathf.Rad2Deg;
            // Aplica la rotaci�n al sistema de part�culas
            var shape = bubbleParticles.shape;
            shape.position = new Vector3(-newTranslation.x * xUnit/2, -newTranslation.y * yUnit/2, shape.position.z);
            shape.rotation = Quaternion.Euler(0f, 0f, angle).eulerAngles;

            bubbleParticles.Play();

            menuChanged?.Invoke(newTranslation, bgMenus[currentMenuIndex], bgMenus[newMenuIndex]);

            for (int i = 0; i < bgMenus.Length; i++)
            {
                if(i == currentMenuIndex || i == newMenuIndex)
                {
                    continue;
                }
                bg = bgMenus[i].transform.GetChild(0);
                mg = bgMenus[i].transform.GetChild(1);
                fg = bgMenus[i].transform.GetChild(2);

                bg.Translate(xUnit * bgParallaxFactor * newTranslation.x, yUnit * bgParallaxFactor * newTranslation.y, 0);
                mg.Translate(xUnit * mgParallaxFactor * newTranslation.x, yUnit * mgParallaxFactor * newTranslation.y, 0);
                fg.Translate(xUnit * fgParallaxFactor * newTranslation.x, yUnit * fgParallaxFactor * newTranslation.y, 0);
            }
        }

        private void BgReposition()
        {
            Transform bg;
            Transform mg;
            Transform fg;
            for(int i=0; i < bgMenus.Length; i++)
            {
                bg = bgMenus[i].transform.GetChild(0);
                mg = bgMenus[i].transform.GetChild(1);
                fg = bgMenus[i].transform.GetChild(2);

                bg.Translate(xUnit * bgParallaxFactor * bgPositions[i].x, yUnit * bgParallaxFactor * bgPositions[i].y, 0);
                mg.Translate(xUnit * mgParallaxFactor * bgPositions[i].x, yUnit * mgParallaxFactor * bgPositions[i].y, 0);
                fg.Translate(xUnit * fgParallaxFactor * bgPositions[i].x, yUnit * fgParallaxFactor * bgPositions[i].y, 0);
            }
        }

        public void UnloadLobby()
        {
            SceneManager.UnloadSceneAsync("Lobby");
        }

        public void SetActiveBackgrounds(bool active)
        {
            bgMenus[0].transform.parent.gameObject.SetActive(active);
        }

        public void SetActiveInteractions(bool active)
        {
            CanvasGroup canvasGroup = menus[6].GetComponent<CanvasGroup>();
            canvasGroup.interactable = active;
        }
    }
}