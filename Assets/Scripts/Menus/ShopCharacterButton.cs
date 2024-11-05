using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tankito
{
    public class ShopCharacterButton : MonoBehaviour
    {
        //Character Data
        public CharacterData characterData;
        //Text
        [SerializeField] private TextMeshProUGUI textPrice;
        //Animation Data
        [SerializeField] private GameObject dropdown;
        [SerializeField] private float duration = 0.3f;

        private void Start()
        {
            textPrice.text = characterData.price.ToString();
        }
        public void OpenShopItem()
        {
            var menu = MenuController.Instance;
            var parent = menu.menus[(int)MenuController.menuName.Shop].transform;
            var dropdownInstance = Instantiate(dropdown, parent);
            var characterPanel = dropdownInstance.transform.GetChild(1).GetComponent<RectTransform>();    //Tiene que coger el hijo que sea el CharacterPanel
            LeanTween.scale(characterPanel, Vector2.zero, 0f);
            LeanTween.scale(characterPanel, Vector2.one, duration).setEase(LeanTweenType.easeOutElastic);
        }
    }
}
