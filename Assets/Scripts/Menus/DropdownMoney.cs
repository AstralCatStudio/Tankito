using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;

// Clase auxiliar para parsear el JSON
[System.Serializable]
public class VideoData
{
    public string[] videos;
}

public class DropdownMoney : MonoBehaviour
{
    [SerializeField] private GameObject ad;
    [SerializeField] private string[] videoNames;
    [SerializeField] private string url = "https://astralcatstudio.github.io/AdVideo/index.json";
    [SerializeField] private VideoPlayer videoPlayer;
    private IEnumerator videoCoroutine;

    private void Start()
    {
        StartCoroutine(GetVideoNames());
    }

    public void PurchaseVirtualMoney(int moneyAmount)
    {
        ClientData clientData = ClientData.Instance;
        clientData.ChangeMoney(+moneyAmount);

        MusicManager.Instance.PlaySoundPitch("snd_monedas", 0.2f);
    }
    public void WatchAd()
    {
        ad.SetActive(true);
        MusicManager.Instance.MuteSong();
        MusicManager.Instance.MuteBackground();
        if (videoPlayer)
        {
            string videoUrl = "https://astralcatstudio.github.io/AdVideo/" + videoNames[Random.Range(0, videoNames.Length)];
            videoPlayer.url = videoUrl;
            videoPlayer.playOnAwake = false;
            videoPlayer.Prepare();

            videoPlayer.prepareCompleted += OnVideoPrepared;
        }
        
        MusicManager.Instance.PlaySound("cancelar");
    }

    public void OnVideoPrepared(VideoPlayer source)
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.Play();
        videoCoroutine = EndAd();
        StartCoroutine(videoCoroutine);
    }

    public void StopAd()
    {
        MusicManager.Instance.enabled = true;
        StopCoroutine(videoCoroutine);
        videoCoroutine = null;
        MusicManager.Instance.ResumeSong();
        MusicManager.Instance.ResumeBackground();
        ad.SetActive(false);
    }

    IEnumerator EndAd()
    {
        float i = 0;
        while (i < videoPlayer.length)
        {

            i += Time.deltaTime;
            yield return null;
        }
        MusicManager.Instance.ResumeSong();
        MusicManager.Instance.ResumeBackground();
        ad.SetActive(false);
        int moneyAmount = Random.Range(1, 3);
        ClientData.Instance.ChangeMoney(+moneyAmount);
        MusicManager.Instance.PlaySoundPitch("snd_monedas", 0.2f);  
    }

    IEnumerator GetVideoNames()
    {
        UnityWebRequest request = UnityWebRequest.Get(url);

        // Enviar la solicitud
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Parsear el JSON
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Respuesta JSON: " + jsonResponse);
            if (string.IsNullOrEmpty(jsonResponse))
            {
                Debug.LogError("El JSON descargado está vacío.");
                yield break;
            }
            try
            {
                // Crear una clase auxiliar para manejar los datos
                VideoData videoData = JsonUtility.FromJson<VideoData>(jsonResponse);

                // Obtener los nombres
                videoNames = videoData.videos;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al parsear el JSON: " + e.Message);
            }

            // Mostrar los nombres en la consola
            foreach (string name in videoNames)
            {
                Debug.Log("Video encontrado: " + name);
            }
        }
        else
        {
            Debug.LogError("Error al obtener los datos: " + request.error);
        }
    }
}

