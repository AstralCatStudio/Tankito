using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothTransition : MonoBehaviour
{
    MenuController menuController;
    float xUnit, yUnit;
    float bgParallaxFactor, mgParallaxFactor, fgParallaxFactor;
    //transition params
    Transform currentBg, currentMg, currentFg;
    //parallax params
    [SerializeField] float parallaxMoveSpeed = 5f;
    [SerializeField] float duration = 1;
    Vector2 maxPosition = new Vector2(0.9f, 0.9f);
    

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
    }

    // Update is called once per frame
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

        currentBg.position = Vector2.Lerp(currentBg.position, targetBgPos, Time.deltaTime * parallaxMoveSpeed);
        currentMg.position = Vector2.Lerp(currentMg.position, targetMgPos, Time.deltaTime * parallaxMoveSpeed);
        currentFg.position = Vector2.Lerp(currentFg.position, targetFgPos, Time.deltaTime * parallaxMoveSpeed);
    }

    public void MoveEvent(Vector2 newTranslation, GameObject currentBgMenu, GameObject newBgMenu)
    {
        Vector2 bgCurrent = new Vector2(xUnit * bgParallaxFactor * newTranslation.x, yUnit * bgParallaxFactor * newTranslation.y);
        Vector2 mgCurrent = new Vector2(xUnit * mgParallaxFactor * newTranslation.x, yUnit * mgParallaxFactor * newTranslation.y);
        Vector2 fgCurrent = new Vector2(xUnit * fgParallaxFactor * newTranslation.x, yUnit * fgParallaxFactor * newTranslation.y);

        LeanTween.move(currentBgMenu.transform.GetChild(0).gameObject, bgCurrent, duration).setEase(LeanTweenType.easeInOutSine);
        LeanTween.move(currentBgMenu.transform.GetChild(1).gameObject, mgCurrent, duration).setEase(LeanTweenType.easeInOutSine);
        LeanTween.move(currentBgMenu.transform.GetChild(2).gameObject, fgCurrent, duration).setEase(LeanTweenType.easeInOutSine);
        LeanTween.move(newBgMenu.transform.GetChild(0).gameObject, Vector2.zero, duration).setEase(LeanTweenType.easeInOutSine);
        LeanTween.move(newBgMenu.transform.GetChild(1).gameObject, Vector2.zero, duration).setEase(LeanTweenType.easeInOutSine);
        LeanTween.move(newBgMenu.transform.GetChild(2).gameObject, Vector2.zero, duration).setEase(LeanTweenType.easeInOutSine);

        currentBg = newBgMenu.transform.GetChild(0);
        currentMg = newBgMenu.transform.GetChild(1);
        currentFg = newBgMenu.transform.GetChild(2);
    }
}