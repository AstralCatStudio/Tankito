using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider soundSlider; 
    private void Start()
    {
        if (MusicManager.Instance == null)
        {
            Debug.LogError("MusicManager no está inicializado en la escena.");
            return;
        }


        musicSlider.value = MusicManager.Instance.volMusic;
        soundSlider.value = MusicManager.Instance.volSounds;

        musicSlider.onValueChanged.AddListener(UpdateMusicVolume);
        soundSlider.onValueChanged.AddListener(UpdateSoundVolume);
    }

    private void UpdateMusicVolume(float value)
    {
        Debug.Log($"Nuevo volumen de música: {value}");
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.volMusic = value;
        }
    }

    private void UpdateSoundVolume(float value)
    {
        Debug.Log($"Nuevo volumen de sonido: {value}");
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.volSounds = value;
        }
    }


    private void OnDestroy()
    {
        musicSlider.onValueChanged.RemoveListener(UpdateMusicVolume);
        soundSlider.onValueChanged.RemoveListener(UpdateSoundVolume);
    }
}
