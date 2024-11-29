using Nfynt;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class DropdownMoney : MonoBehaviour
{
    [SerializeField] private GameObject ad;
    [SerializeField] private string videoUrl = "https://astralcatstudio.github.io/AdVideo/weefagerTrailer.mp4";
    [SerializeField] private VideoPlayer videoPlayer;
    private IEnumerator videoCoroutine;

    private void Start()
    {
        
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

        if (videoPlayer)
        {
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
        //Hacer que la musica pare y no sea interactuable el resto del menú
        videoCoroutine = EndAd();
        StartCoroutine(videoCoroutine);
    }

    public void StopAd()
    {
        MusicManager.Instance.enabled = true;
        StopCoroutine(videoCoroutine);
        videoCoroutine = null;
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
        ad.SetActive(false);
        int moneyAmount = Random.Range(1, 3);
        ClientData.Instance.ChangeMoney(+moneyAmount);
        MusicManager.Instance.PlaySoundPitch("snd_monedas", 0.2f);  
    }

}
