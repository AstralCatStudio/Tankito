using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Vector2 movement = new Vector2(10f, 0f);
    [SerializeField] private float time = 0.4f;
    private IEnumerator animationCoroutine;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (arrow.gameObject.activeSelf)
        {
            animationCoroutine = ArrowAnimation();
            StartCoroutine(animationCoroutine);
        }
    }

    private void OnDisable()
    {
        StopCoroutine(animationCoroutine);
    }

    IEnumerator ArrowAnimation()
    {
        int direction = -1;
        Vector2 newPosition;
        while (true)
        {
            newPosition = arrow.anchoredPosition;
            newPosition += movement * -direction;
            direction *= -1;
            LeanTween.move(arrow, newPosition, time).setEase(LeanTweenType.easeInOutCubic);
            yield return new WaitForSeconds(time);
        }
    }

    public void StopAnimation()
    {
        StopCoroutine(animationCoroutine);
        arrow.gameObject.SetActive(false);
    }
}
