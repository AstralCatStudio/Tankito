using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    public class VirtualMoneyButton : MonoBehaviour
    {
        [SerializeField] private GameObject dropdownMoney;
        [SerializeField] private float duration = 0.3f;
        public void OpenMoneyDropdown()
        {
            var menu = MenuController.Instance;
            var parent = menu.menus[(int)MenuController.menuName.Shop].transform;
            var dropdownInstance = Instantiate(dropdownMoney, parent);
            var moneyPanel = dropdownInstance.transform.GetChild(1).GetComponent<RectTransform>();
            LeanTween.scale(moneyPanel, Vector2.zero, 0f);
            LeanTween.scale(moneyPanel, Vector2.one, duration).setEase(LeanTweenType.easeOutElastic);
        }
    }
}
