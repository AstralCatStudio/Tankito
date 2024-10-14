using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    public struct bulletStatsModifier
    {

    }
    public class Modifier : ScriptableObject
    {
        ABullet bullet;
        public List<IBulletEvent> onSpawnEevents;
        BulletProperties bulletProperties;
        public void ConnectModifier(ABullet newBullet)
        {
            newBullet.OnSpawn += OnSpawn;
            newBullet.OnFly += OnFly;
            newBullet.OnHit += OnHit;
            newBullet.OnBounce += OnBounce;
            newBullet.OnDetonate += OnDetonate;
            bullet = newBullet;
        }
        void OnSpawn()
        {
            foreach (var item in onSpawnEevents)
            {
                item.StartEvent(bullet);
            }
        }
        void OnFly()
        {

        }
        void OnHit()
        {

        }
        void OnBounce()
        {

        }
        void OnDetonate()
        {

        }
    }
}
