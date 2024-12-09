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
        public float lifeTimeMultiplier;
        public BulletStatsModifier(float speed, Vector2 size, float acceleration, int bounces, float lifetime, float boomerang, int amount, int amountmult, float spread, float reload, float lifeTimeMult)
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
            lifeTimeMultiplier = lifeTimeMult;
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
        public BulletStatsModifier bulletStatsModifier = new BulletStatsModifier(1,Vector2.one,0,0,0,0,0,1,1,0,1);
        public Sprite bulletSprite;
        public int bulletSpritePriority;
        public void BindBulletEvents(BulletController newBullet)
        {
            newBullet.OnSpawn += OnSpawn;
            newBullet.OnFly += OnFly;
            newBullet.OnHit += OnHit;
            newBullet.OnBounce += OnBounce;
            newBullet.OnDetonate += OnDetonate;
        }
        void OnSpawn(BulletController bullet)
        {
            foreach (var item in onSpawnEvents)
            {
                item.StartEvent(bullet);
            }
        }
        void OnFly(BulletController bullet)
        {
            foreach (var item in onFlyEvents)
            {
                item.StartEvent(bullet);
            }
        }
        void OnHit(BulletController bullet)
        {
            foreach (var item in onHitEvents)
            {
                item.StartEvent(bullet);
            }
        }
        void OnBounce(BulletController bullet)
        {
            foreach (var item in onBounceEvents)
            {
                item.StartEvent(bullet);

            }
        }
        void OnDetonate(BulletController bullet)
        {
            foreach (var item in onDetonateEvents)
            {
                item.StartEvent(bullet);
            }
        }
    }
}