using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonReady : MonoBehaviour
{
    public RoundManager _roundManager;

    void Start()
    {
        _roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
    }

    public void ButtonOne()
    {
        _roundManager.InitializeRound();
    }

    public void ButtonTwo()
    {
        _roundManager.DebugDamagePlayer();
    }
}
