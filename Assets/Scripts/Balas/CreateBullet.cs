using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    public class CreateBullet : MonoBehaviour
    {
        [SerializeField]
        GameObject bala;
        [SerializeField]
        BulletProperties balaProperties;
        [SerializeField]
        float interval = 1;
        float timer = 0;
        void Start()
        {
            
        }
        void Update()
        {
            timer += Time.deltaTime;
            if(timer> interval)
            {
                balaProperties.direction = transform.up;
                balaProperties.startingPosition = transform.position;
                timer = 0;
                GameObject nuevaBala = Instantiate<GameObject>(bala);
                
                nuevaBala.GetComponent<ABullet>().bulletProperties = balaProperties;
            }
        }
    }
}
