using System.Collections;
using System.Collections.Generic;
using Tankito;
using UnityEngine;

public class PecesEscudoAnimacion : MonoBehaviour
{
    public List<GameObject> fishShields = new List<GameObject>(6);
    public TankData tankData;
    public float rotationSpeed;
    public Vector3 offset;
    void Update()
    {
        for (int i = 0; i < fishShields.Count; ++i)
        {
            if (i < tankData.Health-1)
            {
                fishShields[i].SetActive(true);
            }
            else
            {
                fishShields[i].SetActive(false);
            }
        }
        
        transform.position = transform.parent.GetChild(1).position;
        transform.Rotate(new Vector3(0,0, -rotationSpeed * Time.deltaTime), Space.World);
    }
}
