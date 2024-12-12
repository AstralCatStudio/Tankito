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
            // ONLY PLACE THINGS PERTAINING TO SIMULATION (must be done durring rollback) under this comment


            // Avoid spawning explosion VFX on client as a result of rollback (during rollback the SimClock is stopped).
            if (SimClock.Instance.Active == false) return;

            MusicManager.Instance.PlaySoundPitch("snd_explosion", 0.15f);

            // TODO: Replace with explosion pool
            Explosion explosion = Instantiate<GameObject>(explosionPrefab, bullet.transform.position, bullet.transform.rotation).GetComponent<Explosion>();
            explosion.size *= size;
            explosion.transform.position += relativePosition;
            explosion.timeUntilBig = timeUntilBig;
            explosion.timeUntilDead = totalLifetime;
            if (NetworkManager.Singleton.IsServer)
            {
                explosion.damages = true;
            }
        }
    }
}
