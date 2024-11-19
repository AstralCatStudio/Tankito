using System.Collections;
using System.Collections.Generic;
using Tankito;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

public class DropdownShopCharacter : MonoBehaviour
{
    public Character character;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI price;
    [SerializeField] private Image characterImage;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject message;
    [SerializeField] private GameObject messageText;

    private Coroutine disableCoroutine;

    public float openMessageDuration = 0.3f;
    public float closeMessageDuration = 2f;

    // Start is called before the first frame update
    void Start()
    {
        title.text = character.data.characterName;
        description.text = character.data.description;
        int number = character.data.price;
        price.text = $"{number}";
        characterImage.sprite = character.data.sprite;

        if (character.unlocked)
        {
            ChangeButtonToOwned();
        }
    }
    public void PurchaseItem()
    {
        int price = character.data.price;
        if (character.unlocked)
        {
            ShowMessage("You already own this item.");
        } else if (ClientData.Instance.money < price)
        {
            ShowMessage("You can't afford this item. Buy some vBucks");
        } else
        {
            ClientData.Instance.UnlockCharacter(character);
            ClientData.Instance.ChangeMoney(-price);
            ChangeButtonToOwned();
        }
    }
    private void ChangeButtonToOwned()
    {
        buyButton.GetComponent<Image>().color = Color.gray;
        buyButton.transform.GetComponentInChildren<TextMeshProUGUI>().text = "Owned";
    }

    private void ShowMessage(string text)
    {
        message.gameObject.SetActive(true);
        message.GetComponentInChildren<TextMeshProUGUI>().text = text;
        Transition();
    }
    private void Transition()
    {
        
        RectTransform messageRect = message.GetComponentInChildren<RectTransform>();
        Color alphaZero = new Color(0, 0, 0, 0);
        Color alphaOne = new Color(255, 255, 255, 1);

        LeanTween.cancelAll();
        
        LeanTween.alpha(messageRect, 1f, 0f);
        LeanTween.value(messageText.gameObject, updateAlphaCallback,  alphaZero, alphaOne, 0f);

        LeanTween.scale(messageRect, Vector2.one, openMessageDuration).setEase(LeanTweenType.easeOutElastic);

        LeanTween.alpha(messageRect, 0f, closeMessageDuration);
        LeanTween.value(messageText.gameObject, updateAlphaCallback, alphaOne, alphaZero, closeMessageDuration);

        if (disableCoroutine != null)
        {
            StopCoroutine(disableCoroutine);
        }
        disableCoroutine = StartCoroutine(DisableMessage());
    }

    private void updateAlphaCallback(Color val, object child)
    {
        messageText.GetComponent<TextMeshProUGUI>().color = val;
    }

    private IEnumerator DisableMessage()
    {
        float i = 0f;
        float disableDuration = openMessageDuration + closeMessageDuration;
        LeanTween.scale(message, Vector2.zero, 0f);
        while (i < disableDuration)
        {
            i += Time.deltaTime;
            yield return null;
        }
        message.gameObject.SetActive(false);
    }    
}
