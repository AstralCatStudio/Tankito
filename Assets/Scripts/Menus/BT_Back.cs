using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;

public class BT_Back : MonoBehaviour
{
    [SerializeField] private int currentMenuIndex;
    private int previousMenuIndex = -1;
    // Start is called before the first frame update
    void Awake()
    {
        MenuController.Instance.onIndexChanged.AddListener(UpdatePreviousIndex);
    }

    private void UpdatePreviousIndex(int currentIdx, int newIdx)
    {
        
        if(newIdx == currentMenuIndex)
        {
            previousMenuIndex = currentIdx;
        }

        Debug.Log("Update previousIdx " + previousMenuIndex);
    }

    public void CallPreviousMenu()
    {
        if(previousMenuIndex > 0)
        {
            MenuController.Instance.ChangeToMenu(previousMenuIndex);
        }
        previousMenuIndex = -1;
    }
}
