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
        public int amountAdded;
        public int amountMultiplier;
        public float spreadMultiplier;
        public float reloadTimeAdded;
        public List<Vector2> BulletDirections;
        public BulletStatsModifier(float speed, Vector2 size, float acceleration, int bounces, float lifetime, float boomerang, int amount, int amountmult, float spread, float reload)
        {
            speedMultiplier=speed;
            sizeMultiplier=size;
            accelerationAdded=acceleration;
            BouncesAdded=bounces;
            lifeTimeAdded = lifetime;
            boomerangEffect=boomerang;
            amountAdded=amount;
            amountMultiplier=amountmult;
            spreadMultiplier=spread;
            reloadTimeAdded = reload;
            BulletDirections = new List<Vector2>();
        }
    }

    [CreateAssetMenu(menuName = "Modificadores/ModificadorBalas", order = 2, fileName = "Nuevo Modificador Balas")]
    public class BulletModifier : ScriptableObject
    {
        

        public List<ABulletModifierEvent> onSpawnEvents;
        public List<ABulletModifierEvent> onFlyEvents;
        public List<ABulletModifierEvent> onHitEvents;
        public List<ABulletModifierEvent> onBounceEvents;
        public List<ABulletModifierEvent> onDetonateEvents;
        public BulletStatsModifier bulletStatsModifier = new BulletStatsModifier(1,Vector2.one,0,0,0,0,0,1,1,0);
        public void BindBulletEvents(ABulletController newBullet)
        {
            newBullet.OnSpawn += OnSpawn;
            newBullet.OnFly += OnFly;
            newBullet.OnHit += OnHit;
            newBullet.OnBounce += OnBounce;
            newBullet.OnDetonate += OnDetonate;
        }
        void OnSpawn(ABulletController bullet)
        {
            foreach (var item in onSpawnEvents)
            {
                item.StartEvent(bullet);
            }
        }
        void OnFly(ABulletController bullet)
        {
            foreach (var item in onFlyEvents)
            {
                item.StartEvent(bullet);
            }
        }
        void OnHit(ABulletController bullet)
        {
            foreach (var item in onHitEvents)
            {
                item.StartEvent(bullet);
            }
        }
        void OnBounce(ABulletController bullet)
        {
            foreach (var item in onBounceEvents)
            {
                item.StartEvent(bullet);

            }
        }
        void OnDetonate(ABulletController bullet)
        {
            foreach (var item in onDetonateEvents)
            {
                item.StartEvent(bullet);
            }
        }
    }
}