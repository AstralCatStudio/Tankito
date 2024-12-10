using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownSelectionCharacter : MonoBehaviour
{
    public Character character;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Image characterImage;
    [SerializeField] private Button selectButton;
    [SerializeField] private GameObject message;
    [SerializeField] private GameObject messageText;

    private Coroutine disableCoroutine;
    public float openMessageDuration = 0.3f;
    public float closeMessageDuration = 2f;
    private Color buttonDefaultColor;

    // Start is called before the first frame update
    void Start()
    {
        title.text = character.data.characterName;
        description.text = character.data.description;
        characterImage.sprite = character.data.sprite;
        buttonDefaultColor = selectButton.image.color;
        if (character.selected)
        {
            ChangeButtonToSelected();
        }
    }
    public void SelectItem()
    {
        if (character.selected)
        {
            ShowMessage("This character is already selected.");
            MusicManager.Instance.PlaySound("cancelar");
        } else if (!character.unlocked)
        {
            ShowMessage("You don't have this character yet.");
            MusicManager.Instance.PlaySound("cancelar");
        } else
        {
            DeselectAllButtons();
            ClientData.Instance.SelectCharacter(character);
            ShowMessage("Selected");
            ChangeButtonToSelected();

            MusicManager.Instance.PlaySound("snd_desbloquearpersonaje");
        }
    }
    public void DeselectAllButtons()
    {
        foreach (var c in ClientData.Instance.characters)
        {
            c.selected = false;
            selectButton.GetComponent<Image>().color = buttonDefaultColor;
        }
    }
    private void ChangeButtonToSelected()
    {
        selectButton.GetComponent<Image>().color = Color.gray;
        selectButton.transform.GetComponentInChildren<TextMeshProUGUI>().text = "Selected";
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
        LeanTween.value(messageText.gameObject, UpdateAlphaCallback, alphaZero, alphaOne, 0f);

        LeanTween.scale(messageRect, Vector2.one, openMessageDuration).setEase(LeanTweenType.easeOutElastic);

        LeanTween.alpha(messageRect, 0f, closeMessageDuration);
        LeanTween.value(messageText.gameObject, UpdateAlphaCallback, alphaOne, alphaZero, closeMessageDuration);

        if (disableCoroutine != null)
        {
            StopCoroutine(disableCoroutine);
        }
        disableCoroutine = StartCoroutine(DisableMessage());
    }

    private void UpdateAlphaCallback(Color val, object child)
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
