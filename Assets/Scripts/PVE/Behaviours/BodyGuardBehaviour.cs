using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito.Netcode;
using Tankito.SinglePlayer;
using UnityEngine;
using static AreaDetection;

public class BodyGuardBehaviour : AGenericBehaviour
{
    const float SPEED_MULTIPLIER = 3;

    [SerializeField] float protectDistance = 1.0f;
    private float maxCast;
    GameObject bulletToStop;
    GameObject allyToProtect;
    [SerializeField] AreaDetection allyAreaDetection;
    [SerializeField] AreaDetection bulletAreaDetection;
    [SerializeField] bool DEBUG_BG = true;
    [SerializeField] LayerMask enemyLayer;
    
    #region TargetList
    List<GameObject> alliesInRange = new List<GameObject>();
    List<GameObject> bulletsInRange = new List<GameObject>();
    #endregion

    protected override void Start()
    {
        base.Start();
        maxCast = allyAreaDetection.gameObject.GetComponent<CircleCollider2D>().radius * 2;
        allyAreaDetection.OnSubjectDetected += OnAllyDetected;
        allyAreaDetection.OnSubjectDissapear += OnAllyDissapear;
        bulletAreaDetection.OnSubjectDetected += OnBulletDetected;
        bulletAreaDetection.OnSubjectDissapear += OnBulletDissapear;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        allyAreaDetection.OnSubjectDetected -= OnAllyDetected;
        allyAreaDetection.OnSubjectDissapear -= OnAllyDissapear;
        bulletAreaDetection.OnSubjectDetected -= OnBulletDetected;
        bulletAreaDetection.OnSubjectDissapear -= OnBulletDissapear;
    }

    #region States
    public override Status ChaseState()
    {
        base.ChaseState();
        m_currentInput.moveVector = CheckNewPosition(m_currentInput.moveVector);
        return Status.Running;
    }

    public Status ProtectAllyState()
    {
        if (bulletToStop != null && allyToProtect != null)
        {
            Vector2 allyToBulletDir = (bulletToStop.transform.position - allyToProtect.transform.position).normalized;
            Vector2 newPosition = (Vector2)allyToProtect.transform.position + allyToBulletDir * protectDistance;
            m_currentInput.moveVector = newPosition;
            if (genericTargets.Count > 0)
            {
                Vector2 playerPos = genericTargets[0].transform.position;
                Vector2 playerToNpc = (Vector2)transform.position - playerPos;
                CheckObstacles(playerPos, playerToNpc);
                CheckTargetInRange(playerToNpc.magnitude, agentController.npcData.attackRange);
            }
        }
        return Status.Running;
    }
    #endregion

    #region Perceptions_Actions
    public bool CheckChaseToProtect()
    {
        return CheckBulletRoute();
    }

    public void ActionChaseToProtect()
    {
        importantPos = true;
        agentController.agent.speed *= SPEED_MULTIPLIER;
    }

    public bool CheckProtectToChase()
    {
        return !CheckBulletRoute();
    }

    public void ActionProtectToChase()
    {
        agentController.agent.speed /= SPEED_MULTIPLIER;
    }
    #endregion

    #region Utilis
    private bool CheckBulletRoute()
    {
        //if (DEBUG_BG) Debug.Log("SE CHEQUEA LA RUTA DE BALA");
        bulletsInRange.OrderBy(obj => bulletsInRange.OrderBy(obj => Vector2.Distance(obj.transform.position, transform.position)));
        for (int i = 0; i < bulletsInRange.Count; i++)
        {
            var targetBullet = bulletsInRange[i];
            Vector2 bulletVec = targetBullet.GetComponent<Rigidbody2D>().velocity.normalized;
            RaycastHit2D hit = Physics2D.CircleCast(targetBullet.transform.position, bulletRadius, bulletVec, maxCast, enemyLayer);
            
            if (DEBUG_BG)
            {
                if(hit.collider == null)
                {
                    Debug.Log("NO COLISIONA");
                }
                else if(!alliesInRange.Contains(hit.collider.gameObject))
                {
                    Debug.Log("El objeto es " + hit.collider.gameObject);
                }
            }

            if (hit.collider != null && alliesInRange.Contains(hit.collider.gameObject))
            {
                if (DEBUG_BG) Debug.Log("LA BALA VA A COLISIONAS CON UN ALIADO");
                bulletToStop = targetBullet;
                allyToProtect = hit.collider.gameObject;
                return true;
            }
        }
        bulletToStop = null;
        allyToProtect = null;
        return false;
    }

    private void OnAllyDetected(GameObject gameObject)
    {
        alliesInRange.Add(gameObject);
    }

    private void OnAllyDissapear(GameObject gameObject)
    {
        alliesInRange.Remove(gameObject);
    }

    private void OnBulletDetected(GameObject gameObject)
    {
        bulletsInRange.Add(gameObject);
    }

    private void OnBulletDissapear(GameObject gameObject)
    {
        bulletsInRange.Remove(gameObject);
    }
    #endregion
}
