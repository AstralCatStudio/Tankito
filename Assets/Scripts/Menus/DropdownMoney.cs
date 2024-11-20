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
    [SerializeField] private NVideoPlayer nVideoPlayer;
    [SerializeField] private VideoClip videoclip;
    [SerializeField] private AudioClip audioClip;
    private AudioSource audioSource;
    private Coroutine videoCoroutine;

    private void Start()
    {
        nVideoPlayer.Config.VideoSrcPath = Application.dataPath + "/Videos/Weefager.mp4";
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        nVideoPlayer.Config.AudioSource = audioSource;
        
    }

    public void PurchaseVirtualMoney(int moneyAmount)
    {
        ClientData clientData = ClientData.Instance;
        clientData.ChangeMoney(+moneyAmount);

        MusicManager.Instance.PlaySoundPitch("snd_monedas", 0.2f);
    }
    public void WatchAd()
    {
        Debug.Log("hola");
        ad.SetActive(true);
        
        //Hacer que la musica pare y no sea interactuable el resto del men�
        videoCoroutine = StartCoroutine(EndAd());
        MusicManager.Instance.PlaySound("cancelar");
    }

    public void StopAd()
    {
        MusicManager.Instance.enabled = true;
        StopCoroutine(videoCoroutine);
        ad.SetActive(false);
    }

    IEnumerator EndAd()
    {
        float i = 0;
        while (i < videoclip.length)
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
