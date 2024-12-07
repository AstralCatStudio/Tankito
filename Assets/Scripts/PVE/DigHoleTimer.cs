using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigHoleTimer : MonoBehaviour
{
    const int DIG_HOLE_DURATION = 15;
    float timer;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= DIG_HOLE_DURATION)
        {
            Destroy(gameObject);
        }
    }
}
