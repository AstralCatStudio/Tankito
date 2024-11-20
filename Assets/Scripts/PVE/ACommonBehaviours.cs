using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using Tankito;
using System.Linq;

namespace Tankito.PVE
{
    public abstract class ACommonBehaviours : MonoBehaviour
    {
        [SerializeField] protected float m_moveSpeed = 5.0f;
        [SerializeField] protected float m_turnSpeed = 5.0f;
        [SerializeField] protected int m_health = 10;
        [SerializeField] protected int m_damage = 1;
        [SerializeField] protected float m_cooldownAttack = 1.0f;
        [SerializeField] protected float m_distanceToTarget = 3.0f;
        [SerializeField] protected float m_detectRange = 8.0f;
        [SerializeField] protected Transform[] m_patrolWaypoints;

        protected int m_waypointIndex = 0;

        [SerializeField] protected NPCTankController m_npcTankController;

        protected ContactFilter2D m_tankFilter;
        protected ContactFilter2D m_bulletFilter;

        private void Start()
        {
            if (m_npcTankController == null)
            {
                m_npcTankController = GetComponent<NPCTankController>();
            }

            var tankMask = LayerMask.GetMask("TankPhysics");

            m_tankFilter = new ContactFilter2D();
            m_tankFilter.SetLayerMask(tankMask);
            
            var bulletMask = LayerMask.GetMask("BulletPhysics");

            m_bulletFilter = new ContactFilter2D();
            m_bulletFilter.SetLayerMask(bulletMask);

        }

        #region Acciones
        ////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////// ACCCIONES //////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////// PATROL

        public Status Patrol(Transform[] puntosPatrulla, float velocidad, float distanciaMinima)
        {
            if (puntosPatrulla == null || puntosPatrulla.Length == 0)
            {
                Debug.LogWarning("Patrol: No se han definido puntos de patrulla.");
                return Status.Failure;
            }

            // Obtiene el punto actual
            Transform puntoActual = puntosPatrulla[m_waypointIndex];

            // Calcula la distancia al punto actual
            float distancia = Vector3.Distance(transform.position, puntoActual.position);

            // Si se ha alcanzado el punto actual, pasa al siguiente
            if (distancia <= distanciaMinima)
            {
                Debug.Log($"Patrol: Punto alcanzado. Pasando al siguiente punto. �ndice actual: {m_waypointIndex}");
                m_waypointIndex = (m_waypointIndex + 1) % puntosPatrulla.Length; // Ciclo infinito entre los puntos
                return Status.Running;
            }

            m_npcTankController.SetTargetPosition(puntoActual.position);

            return Status.Running;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////// SEEK
        public Status Seek(string targetTag, float velocidad, float distanciaMinima)
        {
            // Busca el objeto con el tag especificado
            GameObject objetivo = GameObject.FindWithTag(targetTag);
            if (objetivo == null)
            {
                Debug.LogWarning($"Seek: No se encontr� ning�n objeto con el tag '{targetTag}'.");
                return Status.Failure;
            }

            // Calcula la distancia al objetivo
            float distancia = Vector2.Distance(transform.position, objetivo.transform.position);

            // Si est� dentro de la distancia m�nima, no realizar movimiento
            if (distancia <= distanciaMinima)
            {
                Debug.Log($"Seek: Objetivo alcanzado. Distancia = {distancia}, Distancia m�nima = {distanciaMinima}");
                return Status.Success;
            }

            // Mueve hacia el objetivo
            //NPCTankController.SetTargetPosition(objetivo);

            return Status.Running;
        }
        #endregion


        #region Percepciones
        ////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////// PERCEPCIONES /////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////// DETECT
        public RaycastHit2D[] Detect(string targetTag, float rango, ContactFilter2D collisionFilter)
        {
            List<RaycastHit2D> collided = new List<RaycastHit2D>();
            Physics2D.CircleCast(m_npcTankController.Position, m_detectRange, Vector2.zero, collisionFilter, collided);

            RaycastHit2D[] collidedMatches = collided.Where(c => c.collider.CompareTag(targetTag)).OrderBy(c => c.distance).ToArray();

            Debug.Log("DEBUG COLLIDED MATCHES:");
            foreach(var i in collidedMatches)
            {
                Debug.Log(i.collider.tag + ": " + i.distance);
            }

            return collidedMatches;
        }
        #endregion
    }

}