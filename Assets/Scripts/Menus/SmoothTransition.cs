using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SmoothTransition : MonoBehaviour
{
    MenuController menuController;
    float xUnit, yUnit;
    float bgParallaxFactor, mgParallaxFactor, fgParallaxFactor;
    //parallax params
    [SerializeField] float parallaxMoveSpeed = 5f;
    [SerializeField] float lerpDuration = 3;
    Vector2 maxPosition = new Vector2(0.9f, 0.9f);
    //transition params
    float timeElapsed;
    Vector2 newTranslation;
    Transform currentBg, currentMg, currentFg;
    Transform newBg, newMg, newFg;
    public bool isTransitioning;

    // Start is called before the first frame update
    void Start()
    {
        menuController = MenuController.Instance;
        xUnit = menuController.xUnit;
        yUnit = menuController.yUnit;
        bgParallaxFactor = menuController.bgParallaxFactor;
        mgParallaxFactor = menuController.mgParallaxFactor;
        fgParallaxFactor = menuController.fgParallaxFactor;
        currentBg = menuController.bgMenus[menuController.currentMenuIndex].transform.GetChild(0);
        currentMg = menuController.bgMenus[menuController.currentMenuIndex].transform.GetChild(1);
        currentFg = menuController.bgMenus[menuController.currentMenuIndex].transform.GetChild(2);
        timeElapsed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(isTransitioning)
        {
            MoveSmoothly();
        } else
        {
            MoveBgParallax();
        }
    }

    private void MoveBgParallax()
    {
        
        Vector2 mousePos = Input.mousePosition;
        Vector2 mousePosCentered = new Vector2(mousePos.x - (Screen.width / 2), mousePos.y - (Screen.height / 2)).normalized;
        mousePosCentered = -mousePosCentered;
        Vector2 unit = new Vector2(xUnit, yUnit);

        mousePosCentered.x = Mathf.Clamp(mousePosCentered.x, -maxPosition.x, maxPosition.x);
        mousePosCentered.y = Mathf.Clamp(mousePosCentered.y, -maxPosition.y, maxPosition.y);

        Vector2 targetBgPos = mousePosCentered * bgParallaxFactor;
        Vector2 targetMgPos = mousePosCentered * mgParallaxFactor;
        Vector2 targetFgPos = mousePosCentered * fgParallaxFactor;

        currentBg.position = Vector2.Lerp(currentBg.position, targetBgPos, Time.deltaTime * parallaxMoveSpeed);
        currentMg.position = Vector2.Lerp(currentMg.position, targetMgPos, Time.deltaTime * parallaxMoveSpeed);
        currentFg.position = Vector2.Lerp(currentFg.position, targetFgPos, Time.deltaTime * parallaxMoveSpeed);
    }

    public void MoveEvent(Vector2 newTranslation, GameObject currentBgMenu, GameObject newBgMenu)
    {
        this.newTranslation = newTranslation;

        currentBg = currentBgMenu.transform.GetChild(0);
        currentMg = currentBgMenu.transform.GetChild(1);
        currentFg = currentBgMenu.transform.GetChild(2);

        newBg = newBgMenu.transform.GetChild(0);
        newMg = newBgMenu.transform.GetChild(1);
        newFg = newBgMenu.transform.GetChild(2);

        Debug.Log(currentBgMenu);
        Debug.Log(newBgMenu);

        isTransitioning = true;
        timeElapsed = 0;
    }

    private void MoveSmoothly()
    {
        if (timeElapsed < lerpDuration)
        {
            float t = timeElapsed;

            Vector2[] bg = new Vector2[2];
            Vector2[] mg = new Vector2[2];
            Vector2[] fg = new Vector2[2];

            bg[0] = (currentBg.transform.position); bg[1] = (newBg.transform.position);
            mg[0] = (currentMg.transform.position); mg[1] = (newMg.transform.position);
            fg[0] = (currentFg.transform.position); fg[1] = (newFg.transform.position);

            Vector2 bgCurrent = new Vector2(xUnit * bgParallaxFactor * newTranslation.x, yUnit * bgParallaxFactor * newTranslation.y);
            Vector2 mgCurrent = new Vector2(xUnit * mgParallaxFactor * newTranslation.x, yUnit * mgParallaxFactor * newTranslation.y);
            Vector2 fgCurrent = new Vector2(xUnit * fgParallaxFactor * newTranslation.x, yUnit * fgParallaxFactor * newTranslation.y);

            float duration = lerpDuration * 30;
            bg[0] = Vector2.Lerp(bg[0], bgCurrent, t / duration);
            mg[0] = Vector2.Lerp(mg[0], mgCurrent, t / duration);
            fg[0] = Vector2.Lerp(fg[0], fgCurrent, t / duration);
            bg[1] = Vector2.Lerp(bg[1], Vector2.zero, t / duration);
            mg[1] = Vector2.Lerp(mg[1], Vector2.zero, t / duration);
            fg[1] = Vector2.Lerp(fg[1], Vector2.zero, t / duration);

            currentBg.transform.position = bg[0];
            currentMg.transform.position = mg[0];
            currentFg.transform.position = fg[0];
            newBg.transform.position = bg[1];
            newMg.transform.position = mg[1];
            newFg.transform.position = fg[1];

            timeElapsed += Time.deltaTime;
        } else
        {
            isTransitioning = false;
            timeElapsed = 0;

            currentBg = menuController.bgMenus[menuController.currentMenuIndex].transform.GetChild(0);
            currentMg = menuController.bgMenus[menuController.currentMenuIndex].transform.GetChild(1);
            currentFg = menuController.bgMenus[menuController.currentMenuIndex].transform.GetChild(2);
        }
    }
}
