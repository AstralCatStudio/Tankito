using UnityEngine;
using UnityEngine.UI;

public class EasterEggMenu : MonoBehaviour
{
    [SerializeField] private Button[] buttons;

    private int clicks = 0;

    void Start()
    {
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        clicks++;
        Debug.Log($"CLICK: {clicks}");
        if (clicks >= 6)
        {
            Debug.Log("EASTER EGG");
            clicks = 0;
        }
    }
}