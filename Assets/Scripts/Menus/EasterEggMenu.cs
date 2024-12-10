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

        if (clicks >= 6)
        {
            MusicManager.Instance.SetPhase(4);
            clicks = 0;
        }
        else 
        {
            string[] sounds = { "gato1", "gato2", "gato3", "gato4" };
            string randomSound = sounds[Random.Range(0, sounds.Length)];
            MusicManager.Instance.PlaySoundPitch(randomSound);
        }
    }
}