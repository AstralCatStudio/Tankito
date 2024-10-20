using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SmoothTransition : MonoBehaviour
{
    [SerializeField] float lerpDuration = 3;
    float timeElapsed;
    Vector2 newTranslation;
    MenuController menu;
    Transform currentBg, currentMg, currentFg;
    Transform newBg, newMg, newFg;
    bool isTransitioning;

    // Start is called before the first frame update
    void Start()
    {
        menu = MenuController.Instance;
        timeElapsed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(isTransitioning)
        {
            MoveSmoothly();
        }
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

            Vector2 bgCurrent = new Vector2(menu.xUnit * menu.bgParallaxFactor * newTranslation.x, menu.yUnit * menu.bgParallaxFactor * newTranslation.y);
            Vector2 mgCurrent = new Vector2(menu.xUnit * menu.mgParallaxFactor * newTranslation.x, menu.yUnit * menu.mgParallaxFactor * newTranslation.y);
            Vector2 fgCurrent = new Vector2(menu.xUnit * menu.fgParallaxFactor * newTranslation.x, menu.yUnit * menu.fgParallaxFactor * newTranslation.y);

            bg[0] = Vector2.Lerp(bg[0], bgCurrent, t / lerpDuration);
            mg[0] = Vector2.Lerp(mg[0], mgCurrent, t / lerpDuration);
            fg[0] = Vector2.Lerp(fg[0], fgCurrent, t / lerpDuration);
            bg[1] = Vector2.Lerp(bg[1], Vector2.zero, t / lerpDuration);
            mg[1] = Vector2.Lerp(mg[1], Vector2.zero, t / lerpDuration);
            fg[1] = Vector2.Lerp(fg[1], Vector2.zero, t / lerpDuration);

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
        }
    }
}
