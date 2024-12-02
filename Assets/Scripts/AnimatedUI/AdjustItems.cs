using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AdjustItems : MonoBehaviour
{
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;

    [Header("Proportions (0.0 - 1.0)")]
    [Range(0f, 1f)] public float widthProportion = 1f; // Proporción del ancho (1 = 100%)
    [Range(0f, 1f)] public float heightProportion = 1f; // Proporción del alto (1 = 100%)

    [Header("Width Constraints")]
    public float minWidth = 0f;  // Ancho mínimo permitido
    public float maxWidth = Mathf.Infinity;  // Ancho máximo permitido

    [Header("Height Constraints")]
    public float minHeight = 0f;  // Alto mínimo permitido
    public float maxHeight = Mathf.Infinity;  // Alto máximo permitido

    private void Awake()
    {
        // Obtén los RectTransforms del hijo y del padre
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (parentRectTransform != null)
        {
            //Esto es super cutre pero tengo sueño lo siento :(
            if(parentRectTransform.rect.width > 400)
            {
                if(parentRectTransform.rect.height >= 630)
                {
                    if (GetComponent<TextMeshProUGUI>())
                    {
                        widthProportion = 0.5f;
                        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 100);
                    }
                    else
                    {
                        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -100);
                    }
                }
                else if (parentRectTransform.rect.height < 630)
                {
                    if (GetComponent<TextMeshProUGUI>())
                    {
                        widthProportion = 0.15f;
                        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 50);
                    }
                    else
                    {
                        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -50);
                    }
                }
            } else if(parentRectTransform.rect.width <= 400)
            {
                if (parentRectTransform.rect.height >= 630)
                {
                    if (GetComponent<TextMeshProUGUI>())
                    {
                        widthProportion = 0.5f;
                        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 80);
                    }
                    else
                    {
                        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -80);
                    }
                } else if (parentRectTransform.rect.height < 630)
                {
                    if (GetComponent<TextMeshProUGUI>())
                    {
                        widthProportion = 0.3f;
                        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 50);
                    }
                    else
                    {
                        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -50);
                    }
                }
            }
            // Calcula el tamaño proporcional basado en el padre
            float targetWidth = parentRectTransform.rect.width * widthProportion;
            float targetHeight = parentRectTransform.rect.height * heightProportion;

            // Aplica los límites al ancho y alto
            float finalWidth = Mathf.Clamp(targetWidth, minWidth, maxWidth);
            float finalHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);

            // Ajusta el tamaño del RectTransform del hijo
            rectTransform.sizeDelta = new Vector2(finalWidth, finalHeight);
        }
    }
}
