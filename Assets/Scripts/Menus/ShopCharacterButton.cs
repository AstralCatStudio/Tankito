using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tankito
{
    public class ShopCharacterButton : MonoBehaviour
    {
        //Character Data
        public Character character;
        //ButtonMedia
        [SerializeField] private GameObject priceTag;
        [SerializeField] private GameObject ownTag;
        [SerializeField] private TextMeshProUGUI textPrice;
        [SerializeField] private TextMeshProUGUI textName;
        [SerializeField] private Image characterImage;
        //Animation Data
        [SerializeField] private GameObject dropdown;
        [SerializeField] private float duration = 0.3f;

        private void Start()
        {
            characterImage.sprite = character.data.sprite;
            textName.text = character.data.characterName;
            textPrice.text = character.data.price.ToString();
            if (character.unlocked)
            {
                priceTag.SetActive(false);
                ownTag.SetActive(true);
            }
            else
            {
                ClientData.Instance.onCharacterPurchased += UnlockCharacter;
            }
        }
        public void OpenShopItem()
        {
            var menu = MenuController.Instance;
            var parent = menu.menus[(int)MenuController.menuName.Shop].transform;
            var dropdownInstance = Instantiate(dropdown, parent);
            dropdownInstance.GetComponent<DropdownShopCharacter>().character = character;
            var characterPanel = dropdownInstance.transform.GetChild(1).GetComponent<RectTransform>();    //Tiene que coger el hijo que sea el CharacterPanel
            LeanTween.scale(characterPanel, Vector2.zero, 0f);
            LeanTween.scale(characterPanel, Vector2.one, duration).setEase(LeanTweenType.easeOutElastic);
        }
        private void UnlockCharacter()
        {
            if (character.unlocked)
            {
                priceTag.SetActive(false);
                ownTag.SetActive(true);
                ClientData.Instance.onCharacterPurchased -= UnlockCharacter;
            }
        }
    }
}
