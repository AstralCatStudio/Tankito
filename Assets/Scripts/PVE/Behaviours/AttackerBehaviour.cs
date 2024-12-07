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
        [SerializeField] const int LOADER_SIZE = 5;
        int ammunition;
        [SerializeField] bool Bullets { get => ammunition > 0; }
        [SerializeField] float loaderReloadDuration = 5f;
        float loaderReloadTimer = 0f;
        List<GameObject> healerList = new List<GameObject>();
        float maxAttackerHp;
        float maxPlayerHp;

        #region USParameters
        int MAX_AGGRO_ATTACKERS = 5;
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
                    loaderReloadDuration = 0;
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

        #endregion

        #region UtilitySystem
        #region Variables

        public float NAFullAggro()
        {
            return USManager.Instance.NAttackerFullAggro;
        }

        public float HP_Attacker()
        {
            return (float)GetComponent<PVEEnemyData>().Health/maxAttackerHp;
        }

        public float HP_Player()
        {
            if(player != null)
            {
                return (float)player.GetComponent<PVECharacterData>().Health/maxPlayerHp;
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
            return (HP_A - 1 / maxAttackerHp) / (1 - 1 / maxAttackerHp);
        }

        public float AggroHPP(float HP_P)
        {
            return (-HP_P + 1) / (1 - 1 / maxPlayerHp);
        }
        #endregion
        #region UtilityActions
        public Status GoAggro()
        {
            genericTargets.Add(player);
            return Status.Success;
        }
        public Status GoIdealDis()
        {
            return Status.Success;
        }
        public Status GoDef()
        {
            return Status.Success;
        }
        public Status GoHeal()
        {
            return Status.Success;
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

        protected override bool CheckTargetInRange(float magnitude, float range) { return false; }
        #endregion
    }
}

