using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallSounds : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }


    public void CallSound(string sound)
    {
        MusicManager.Instance.PlaySound(sound);
    }
}
