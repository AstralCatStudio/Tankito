using Unity.Netcode;
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
            // Avoid spawning explosion VFX on client as a result of rollback (during rollback the SimClock is stopped).
            if (SimClock.Instance.Active == false) return;

            // TODO: Replace with explosion pool
            GameObject explosion = Instantiate<GameObject>(explosionPrefab, bullet.transform.position, bullet.transform.rotation);
            explosion.GetComponent<Explosion>().size *= size;
            explosion.transform.position += relativePosition;
            explosion.GetComponent<Explosion>().timeUntilBig = timeUntilBig;
            explosion.GetComponent<Explosion>().timeUntilDead = totalLifetime;
            if (NetworkManager.Singleton.IsServer)
            {
                explosion.GetComponent<Explosion>().damages = true;
            }
        }
    }
}
