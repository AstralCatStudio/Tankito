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
        Debug.Log("Reviving tank: " + tankPrefab.name);
        GameObject revivedTank = Instantiate(tankPrefab, transform.position, Quaternion.identity);
        WaveManager.Instance.AddEnemy(revivedTank);
        Destroy(gameObject);
    }
}
