using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tankito.SinglePlayer;
using UnityEngine;
using static AreaDetection;

public class BodyGuardBehaviour : AGenericBehaviour
{
    [SerializeField] float protectDistance = 1.0f;
    private float maxCast;
    GameObject bulletToStop;
    GameObject allyToProtect;
    [SerializeField] AreaDetection allyAreaDetection;
    [SerializeField] AreaDetection bulletAreaDetection;
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
    public Status ProtectAllyState()
    {
        Vector2 allyToBulletDir = (bulletToStop.transform.position - allyToProtect.transform.position).normalized;
        Vector2 newPosition = (Vector2)allyToProtect.transform.position + allyToBulletDir * protectDistance;
        m_currentInput.moveVector = newPosition;
        if(genericTargets.Count > 0)
        {
            Vector2 playerPos = genericTargets[0].transform.position;
            Vector2 playerToNpc = (Vector2)transform.position - playerPos;
            CheckObstacles(playerPos, playerToNpc);
            CheckTargetInRange(playerToNpc.magnitude, agentController.npcData.attackRange);
        }
        return Status.Running;
    }
    #endregion

    #region Perceptions
    public bool CheckChaseToProtect()
    {
        return CheckBulletRoute();
    }

    public bool CheckProtectToChase()
    {
        return !CheckBulletRoute();
    }
    #endregion

    #region Utilis
    private bool CheckBulletRoute()
    {
        bulletsInRange.OrderBy(obj => bulletsInRange.OrderBy(obj => Vector2.Distance(obj.transform.position, transform.position)));
        for (int i = 0; i < bulletsInRange.Count; i++)
        {
            var targetBullet = bulletsInRange[i];
            Vector2 bulletVec = targetBullet.GetComponent<Rigidbody2D>().velocity.normalized;
            RaycastHit2D hit = Physics2D.Raycast(targetBullet.transform.position, bulletVec, maxCast);
            if(hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
                hit = Physics2D.Raycast(transform.position, bulletVec, maxCast);
            }
            if (hit.collider != null && alliesInRange.Contains(hit.collider.gameObject))
            {
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
