using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownMoney : MonoBehaviour
{
    public void PurchaseVirtualMoney(int moneyAmount)
    {
        ClientData clientData = ClientData.Instance;
        clientData.ChangeMoney(+moneyAmount);
    }
    public void WatchAd()
    {
        int moneyAmount = Random.Range(1, 3);
        ClientData clientData = ClientData.Instance;
        clientData.ChangeMoney(+moneyAmount);
    }
}