using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Tankito
{
    public class ShopMoneyButton : MonoBehaviour
    {
        [SerializeField] private GameObject dropdownMoney;
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private float duration = 0.3f;

        void Start()
        {
            UpdateMoney();
            ClientData clientData = ClientData.Instance;
            clientData.onMoneyChanged.AddListener(UpdateMoney);  //Suscribe la llamada de UpdateMoney dentro de dropdownCharacter al evento de OnMoneyChanged de client Data
        }

        public void OpenMoneyDropdown()
        {
            var menu = MenuController.Instance;
            var parent = menu.menus[(int)MenuController.menuName.Shop].transform;
            var dropdownInstance = Instantiate(dropdownMoney, parent);
            var moneyPanel = dropdownInstance.transform.GetChild(1).GetComponent<RectTransform>();
            LeanTween.scale(moneyPanel, Vector2.zero, 0f);
            LeanTween.scale(moneyPanel, Vector2.one, duration).setEase(LeanTweenType.easeOutElastic);
        }

        public void UpdateMoney()
        {
            moneyText.text = ClientData.Instance.money.ToString();
        }
    }
}
