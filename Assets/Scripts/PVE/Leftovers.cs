using System.Collections;
using System.Collections.Generic;
using Tankito.SinglePlayer;
using UnityEngine;

public class Leftovers : MonoBehaviour
{
    public tankType type;
    public GameObject tankPrefab;

    public void ReviveTank()
    {
        Instantiate(tankPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
