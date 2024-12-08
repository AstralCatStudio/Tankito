using BehaviourAPI.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tankito.SinglePlayer
{
    public class AttackerBehaviour : AGenericBehaviour
    {
        GameObject player;
        [SerializeField] AreaDetection playerAreaDetection;
        [SerializeField]  int LOADER_SIZE = 5;
        [SerializeField]
        int ammunition;
        [SerializeField] bool Bullets { get => ammunition > 0; }
        [SerializeField] float loaderReloadDuration = 5f;
        float loaderReloadTimer = 0f;
        List<GameObject> healerList = new List<GameObject>();
        public float maxAttackerHp;
        float maxPlayerHp;

        #region USParameters
        int MAX_AGGRO_ATTACKERS = 5;
        [SerializeField]
        float aggroRadiusMult, normalRadiusMult, defRadiusMult, baseDistanceAggroRadius, baseDistanceNormalRadius, baseDistanceDefRadius;
        #endregion

        protected override void Start()
        {
            base.Start();
            ammunition = LOADER_SIZE;
            maxAttackerHp = GetComponent<PVEEnemyData>().Max_Health;
            playerAreaDetection.OnSubjectDetected += OnPlayerDetected;
            playerAreaDetection.OnSubjectDissapear += OnPlayerDissapear;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            playerAreaDetection.OnSubjectDetected -= OnPlayerDetected;
            playerAreaDetection.OnSubjectDissapear -= OnPlayerDissapear;
        }

        protected override void Update()
        {
            base.Update();
            if (!Bullets)
            {
                loaderReloadTimer += Time.deltaTime;
                if(loaderReloadTimer >= loaderReloadDuration)
                {
                    loaderReloadTimer = 0;
                    ammunition = LOADER_SIZE;
                }
            }
        }

        #region States

        public override Status ChaseState()
        {
            base.ChaseState();
            m_currentInput.moveVector = CheckNewPosition(m_currentInput.moveVector);
            return Status.Running;
        }

        public Status CoverState()
        {
            if (genericTargets.Count > 0 && player != null)
            {
                genericTargets.OrderBy(obj => obj, GenericListOrder());
                Vector2 bGPos = genericTargets[0].transform.position;
                Vector2 playerPos = player.transform.position;
                Vector2 playerToBG = (bGPos - playerPos).normalized;
                m_currentInput.moveVector = bGPos + playerToBG * agentController.npcData.idealDistance;
            }
            return Status.Running;
        }
        #endregion

        #region Perceptions
        public bool CheckChaseToCover()
        {
            return player != null && !Bullets;
        }

        public bool CheckCoverToChase()
        {
            return player == null;
        }

        public bool CheckChaseAndCoverToUS()
        {
            return player != null && Bullets;
        }

        public bool CheckUSToChase()
        {
            return player == null && genericTargets.Count > 0;
        }
        public bool CheckUSToAim()
        {
            return CheckChaseToAim() && Bullets;
        }
        public bool CheckUSToCover()
        {
            return player != null && genericTargets.Count > 0 && !Bullets;
        }

        public bool CheckIdleToUS()
        {
            return player != null;
        }

        public bool CheckUSToIdle()
        {
            return player == null && genericTargets.Count == 0;
        }

        public override void ActionShootPOP()
        {
            base.ActionShootPOP();
            ammunition--;
        }

        public void ActionAttackerUSToAim()
        {
            noObstaclesBetween = false;
            target = player;
        }

        #endregion

        #region UtilitySystem
        #region Variables

        public float NAFullAggro()
        {
            return USManager.Instance.AttackerFullAggro.Count;
        }

        public float HP_Attacker()
        {

            float factor = (float)(GetComponent<PVEEnemyData>().Health / maxAttackerHp);
            //Debug.Log(" "+ GetComponent<PVEEnemyData>().Health+ " " + maxAttackerHp+""+ factor);
            return factor;
        }

        public float HP_Player()
        {
            if(player != null)
            {
                return (float)(player.GetComponent<PVECharacterData>().Health/maxPlayerHp);
            }
            return 0;
        }

        public float Healer()
        {
            if(healerList.Count == 0)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
        #endregion

        #region Function
        public float AggroNA(float NA)
        {
            return - (NA * NA) + 1;
        }

        public float AggroHPA(float HP_A)
        {
            //Debug.Log(HP_A);
            return (HP_A - 1 / maxAttackerHp) / (1 - 1 / maxAttackerHp);
        }

        public float AggroHPP(float HP_P)
        {
            return (-HP_P + 1) / (1 - 1 / maxPlayerHp);
        }
        #endregion
        #region UtilityActions
        Vector2 CircleIntersection(float extraradius, Vector2 pos1, Vector2 pos2, Vector2 pos)
        {
            List<Vector2> outVectors = new List<Vector2>();
            float distance = Vector2.Distance(pos1, pos2);
            float rad1 = distance / 2;
            float rad2 = rad1 * extraradius;
            Vector2 firstpart = 0.5f* (pos1+ pos2)+ ((Mathf.Pow(rad1,2)- Mathf.Pow(rad2,2))/(2* Mathf.Pow(distance,2) ))*(pos2-pos1) ;
            Vector2 secondpart = 0.5f * Mathf.Sqrt(2*((Mathf.Pow(rad1,2)+ Mathf.Pow(rad2,2)) /Mathf.Pow(distance,2))- (Mathf.Pow((Mathf.Pow(rad1, 2) - Mathf.Pow(rad2, 2)),2) / Mathf.Pow(distance, 4))-1 ) *(new Vector2(pos2.y-pos1.y,pos1.x-pos2.x));
            outVectors.Add(firstpart+secondpart);
            outVectors.Add(firstpart - secondpart);
            if (Vector2.Distance(pos, outVectors[0])< Vector2.Distance(pos, outVectors[1]))
            {
                return outVectors[0];
            }
            else
            {
                return outVectors[1];
            }
            
        }
        public Status GoAggro()
        {
            if (player == null)
            {
                return Status.Running;
            }
            if (!USManager.Instance.AttackerFullAggro.Contains(this)) USManager.Instance.AttackerFullAggro.Add(this);
            if (genericTargets.Count>0)
            {
                m_currentInput.moveVector = CircleIntersection(aggroRadiusMult, genericTargets[0].transform.position, player.transform.position,transform.position);
            }
            else
            {
                m_currentInput.moveVector = player.transform.position + (transform.position-player.transform.position).normalized * baseDistanceAggroRadius+ Vector3.Cross((transform.position - player.transform.position).normalized, Vector3.forward)*0.1f;
            }
            
            m_currentInput.moveVector = CheckNewPosition(m_currentInput.moveVector);
            CheckObstacles(player.transform.position, transform.position-player.transform.position);
            CheckTargetInRange(Vector2.Distance(transform.position,player.transform.position),20);
            return Status.Running;
        }

        public Status GoIdealDis()
        {
            if (player == null)
            {
                return Status.Running;
            }
            if (USManager.Instance.AttackerFullAggro.Contains(this)) USManager.Instance.AttackerFullAggro.Remove(this);
            if (genericTargets.Count > 0)
            {
                m_currentInput.moveVector = CircleIntersection(normalRadiusMult, genericTargets[0].transform.position, player.transform.position, transform.position);
            }
            else
            {
                m_currentInput.moveVector = player.transform.position + (transform.position - player.transform.position).normalized * baseDistanceNormalRadius;
            }
            m_currentInput.moveVector = CheckNewPosition(m_currentInput.moveVector);
            CheckObstacles(player.transform.position, transform.position - player.transform.position);
            CheckTargetInRange(Vector2.Distance(transform.position, player.transform.position), 20);
            return Status.Running;
        }

        public Status GoDef()
        {
            if (player == null)
            {
                return Status.Running;
            }
            if (USManager.Instance.AttackerFullAggro.Contains(this)) USManager.Instance.AttackerFullAggro.Remove(this);
            if (genericTargets.Count > 0)
            {
                m_currentInput.moveVector = CircleIntersection(defRadiusMult, genericTargets[0].transform.position, player.transform.position, transform.position);
            }
            else
            {
                m_currentInput.moveVector = player.transform.position + (transform.position - player.transform.position).normalized * baseDistanceDefRadius;
            }
            m_currentInput.moveVector = CheckNewPosition(m_currentInput.moveVector);
            CheckObstacles(player.transform.position, transform.position - player.transform.position);
            CheckTargetInRange(Vector2.Distance(transform.position, player.transform.position), 20);
            return Status.Running;
        }

        public Status GoHeal()
        {
            
            if (USManager.Instance.AttackerFullAggro.Contains(this)) USManager.Instance.AttackerFullAggro.Remove(this);
            if (player == null)
            {
                return Status.Running;
            }
            if (healerList.Count>0)
            {
                m_currentInput.moveVector = player.transform.position + (healerList[0].transform.position - player.transform.position) + (healerList[0].transform.position - player.transform.position).normalized*3;
            }
            m_currentInput.moveVector = CheckNewPosition(m_currentInput.moveVector);
            CheckObstacles(player.transform.position, transform.position - player.transform.position);
            CheckTargetInRange(Vector2.Distance(transform.position, player.transform.position), 20);
            return Status.Running;
        }
        #endregion
        #endregion

        #region Utilis
        protected override void OnSubjectDetected(GameObject gameObject)
        {
            if(gameObject.GetComponent<BodyGuardBehaviour>() != null)
            {
                base.OnSubjectDetected(gameObject);
            }
            else if(gameObject.GetComponent<HealerBehaviour>() != null)
            {
                healerList.Add(gameObject);
            }
        }

        protected override void OnSubjectDissapear(GameObject gameObject)
        {
            base.OnSubjectDissapear(gameObject);
            if(healerList.Contains(gameObject)) healerList.Remove(gameObject);
        }

        private void OnPlayerDetected(GameObject gameObject)
        {
            if(maxPlayerHp == 0)
            {
                maxPlayerHp = gameObject.GetComponent<PVECharacterData>().Max_Health;
            }
            player = gameObject;
        }

        private void OnPlayerDissapear(GameObject gameObject)
        {
            player = null;
        }

        protected override bool CheckTargetInRange(float magnitude, float range) { 
            base.CheckTargetInRange(magnitude, range);
            return false; 
        }
        #endregion
    }
}

