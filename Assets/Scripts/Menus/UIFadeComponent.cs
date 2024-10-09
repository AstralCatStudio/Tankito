using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFadeComponent : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    public float _fadeDuration = 1.0f;

    private void Start()
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    public void FadeIn()
    {

    }

    public void FadeOut()
    {

    }

    private IEnumerator FadeInRoutine()
    {
        float elapsedTime = 0.0f;

        _canvasGroup.alpha = 0.0f;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            _canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / _fadeDuration);
            yield return null;
        }

        //_canvasGroup.alpha = 1.0f;
    }
    
    private IEnumerator FadeOutRoutine()
    {
        float elapsedTime = 0.0f;

        _canvasGroup.alpha = 1.0f;

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            _canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / _fadeDuration);
            yield return null;
        }

        //_canvasGroup.alpha = 0.0f;
    }
}
