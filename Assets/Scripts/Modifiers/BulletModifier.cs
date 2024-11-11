using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tankito
{
    [System.Serializable]
    public struct BulletStatsModifier
    {
        public float speedMultiplier;
        public Vector2 sizeMultiplier;
        public float accelerationAdded;
        public int BouncesAdded;
        public float lifeTimeAdded;
        public float boomerangEffect;
    }

    [CreateAssetMenu(menuName = "Modificadores/ModificadorBalas", order = 2, fileName = "Nuevo Modificador Balas")]
    public class BulletModifier : ScriptableObject
    {
        ABullet bullet;

        public List<AModifierEvent> onSpawnEvents;
        public List<AModifierEvent> onFlyEvents;
        public List<AModifierEvent> onHitEvents;
        public List<AModifierEvent> onBounceEvents;
        public List<AModifierEvent> onDetonateEvents;
        public BulletStatsModifier bulletStatsModifier;
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
            foreach (var item in onSpawnEvents)
            {
                item.StartEvent(bullet);
            }
        }
        void OnFly()
        {
            foreach (var item in onFlyEvents)
            {
                item.StartEvent(bullet);
            }
        }
        void OnHit()
        {
            foreach (var item in onHitEvents)
            {
                item.StartEvent(bullet);
            }
        }
        void OnBounce()
        {
            foreach (var item in onBounceEvents)
            {
                item.StartEvent(bullet);
            }
        }
        void OnDetonate()
        {
            foreach (var item in onDetonateEvents)
            {
                item.StartEvent(bullet);
            }
        }
    }
}