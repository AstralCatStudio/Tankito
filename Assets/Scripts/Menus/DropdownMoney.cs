using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.Video;

public class DropdownMoney : MonoBehaviour
{
    [SerializeField] private GameObject ad;
    [SerializeField] private VideoPlayer videoPlayer;
    private Coroutine videoCoroutine;

    public void PurchaseVirtualMoney(int moneyAmount)
    {
        ClientData clientData = ClientData.Instance;
        clientData.ChangeMoney(+moneyAmount);

        MusicManager.Instance.PlaySoundPitch("snd_monedas", 0.2f);
    }
    public void WatchAd()
    {
        ad.SetActive(true);

        //Hacer que la musica pare y no sea interactuable el resto del menú
        videoCoroutine = StartCoroutine(EndAd());

        MusicManager.Instance.PlaySound("cancelar");
    }

    public void StopAd()
    {
        StopCoroutine(videoCoroutine);
        ad.SetActive(false);
    }

    IEnumerator EndAd()
    {
        float i = 0;
        while (i < videoPlayer.clip.length)
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
