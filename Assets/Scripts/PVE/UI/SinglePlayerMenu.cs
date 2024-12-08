using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tankito
{
    public class SinglePlayerMenu : MonoBehaviour
    {
        public GameObject bgMenus;
        Vector2 maxPosition = new Vector2(0.9f, 0.9f);
        Transform bg, mg, fg;

        public float xUnit = 56, yUnit = 30;
        public float bgParallaxFactor = 1.2f, mgParallaxFactor = 1f, fgParallaxFactor = 0.7f;
        public float parallaxMoveSpeed = 5f;

        [SerializeField] private float inDuration = 2f;
        [SerializeField] private float outDuration = 2f;
        [SerializeField] private GameObject firstMenu;
        [SerializeField] private GameObject title;

        void Start()
        {
            bg = bgMenus.transform.GetChild(0);
            mg = bgMenus.transform.GetChild(1);
            fg = bgMenus.transform.GetChild(2);

            RectTransform ltRect = title.GetComponent<RectTransform>();
            //0
            LeanTween.alpha(ltRect, 0f, 0f);
            //FadeIn
            LeanTween.alpha(ltRect, 1f, inDuration).setEase(LeanTweenType.easeInSine);
            //FadeOut
            LeanTween.alpha(ltRect, 0f, outDuration).setEase(LeanTweenType.easeOutSine).setDelay(inDuration);
            
            Invoke("EnableDelayed", inDuration + outDuration);
            
            MusicManager.Instance.SetSong("MENU");
            MusicManager.Instance.SetPhase(0);
            MusicManager.Instance.PlayBackgroundSound("amb_underwater");
        }

        public void SceneLoaderCallback(string functionIdx)
        {
            SceneManager.LoadScene(functionIdx);
        }

        private void EnableDelayed()
        {
            firstMenu.SetActive(true);
            
        }

        void Update()
        {
            MoveBgParallax();
        }

        private void MoveBgParallax()
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 mousePosCentered = new Vector2(mousePos.x - (Screen.width / 2), mousePos.y - (Screen.height / 2)).normalized;
            mousePosCentered = -mousePosCentered;

            mousePosCentered.x = Mathf.Clamp(mousePosCentered.x, -maxPosition.x, maxPosition.x);
            mousePosCentered.y = Mathf.Clamp(mousePosCentered.y, -maxPosition.y, maxPosition.y);

            Vector2 targetBgPos = mousePosCentered * bgParallaxFactor;
            Vector2 targetMgPos = mousePosCentered * mgParallaxFactor;
            Vector2 targetFgPos = mousePosCentered * fgParallaxFactor;

            bg.position = Vector2.Lerp(bg.position, targetBgPos, Time.deltaTime * parallaxMoveSpeed);
            mg.position = Vector2.Lerp(mg.position, targetMgPos, Time.deltaTime * parallaxMoveSpeed);
            fg.position = Vector2.Lerp(fg.position, targetFgPos, Time.deltaTime * parallaxMoveSpeed);
        }
    }
}

