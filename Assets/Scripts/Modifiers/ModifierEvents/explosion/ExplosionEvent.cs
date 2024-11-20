using UnityEngine;

namespace Tankito
{
    
    [CreateAssetMenu(menuName = "Modificadores/Eventos/Explosion", order = 1, fileName = "Nuevo evento de explosion")]
    public class ExplosionEvent : ABulletModifierEvent
    {
        [SerializeField]
        GameObject explosionPrefab;
        [SerializeField]
        float size =1;
        [SerializeField]
        Vector3 relativePosition;
        [SerializeField]
        float totalLifetime = 1f;
        [SerializeField]
        float timeUntilBig = 0.5f;
        public override void StartEvent(BulletController bullet)
        {
            //createExplosion = bullet.gameObject.GetComponent<CreateExplosion>()? bullet.gameObject.GetComponent<CreateExplosion>():bullet.gameObject.AddComponent<CreateExplosion>();
            //createExplosion.explosionPrefab = explosionPrefab;
            //createExplosion.size = size;
            //createExplosion.relativePosition = relativePosition;
            //createExplosion.totalLifetime = totalLifetime;
            //createExplosion.timeUntilBig = timeUntilBig;
            GameObject explosion = Instantiate<GameObject>(explosionPrefab, bullet.transform.position, bullet.transform.rotation);
            explosion.GetComponent<Explosion>().size *= size;
            explosion.transform.position += relativePosition;
            explosion.GetComponent<Explosion>().timeUntilBig = timeUntilBig;
            explosion.GetComponent<Explosion>().timeUntilDead = totalLifetime;
            //createExplosion.StartEvent(bullet);
        }
    }
}
