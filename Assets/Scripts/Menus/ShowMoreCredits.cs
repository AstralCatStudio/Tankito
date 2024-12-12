using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMoreCredits : MonoBehaviour
{
    [SerializeField] private GameObject fullCredits;
    private GameObject elements;
    [SerializeField] private float creditsSpeed = 1f;
    private float originalSpeed;
    private Vector2 creditsSize;
    private Vector2 initialPos;

    // Start is called before the first frame update
    void Start()
    {
        originalSpeed = creditsSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            creditsSpeed *= 1.02f;
        }
        if (Input.GetMouseButtonUp(0))
        {
            creditsSpeed = originalSpeed;
        }
    }

    public void DisplayFullCredits()
    {
        elements = fullCredits.transform.GetChild(0).gameObject;
        fullCredits.SetActive(true);
        RectTransform rect = elements.GetComponent<RectTransform>();
        creditsSize = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y);
        initialPos = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y);
        StartCoroutine(MoveCredits());
        MusicManager.Instance.SetPhase(4);
    }

    private IEnumerator MoveCredits()
    {
        RectTransform rect = elements.GetComponent<RectTransform>();
        Vector2 move;
        Debug.Log(creditsSize.ToString());
        Debug.Log(-creditsSize.y / 2);

        yield return new WaitForSeconds(1f);

        while (rect.anchoredPosition.y < (creditsSize.y/2))
        {
            move = new Vector2(0, rect.anchoredPosition.y + Time.deltaTime * creditsSpeed);
            rect.anchoredPosition = move;
            yield return null;
        }
        DisableCredits();
    }

    void DisableCredits()
    {
        RectTransform rect = elements.GetComponent<RectTransform>();
        rect.anchoredPosition = initialPos;
        fullCredits.SetActive(false);
        MusicManager.Instance.SetPhase(0);
    }
}
