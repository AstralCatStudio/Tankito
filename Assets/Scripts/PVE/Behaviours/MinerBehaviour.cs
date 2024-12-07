using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI;
using BehaviourAPI.Core;
using System.Linq;

namespace Tankito.SinglePlayer
{
    public class MinerBehaviour : AGenericBehaviour
    {
        GameObject player;
        [SerializeField] AreaDetection playerAreaDetection;
        float maxPlayerHp;
        int nMines;
        const int MAX_MINES = 10;

        [SerializeField] int nRayCastCover = 10;
        float colliderDiameter;
        float disPerStep;
        [SerializeField] LayerMask coverLayers;

        [SerializeField]float excaveCooldown = 10;
        float digTimer = 0;
        const int MAX_NA = 5;

        [SerializeField] private GameObject minePrefab; 
        private float stateTimer = 0f;

        #region PutMine
        [SerializeField] bool putMineAction = false;
        bool hasPutMine = false;
        [SerializeField] private float putMineDuration = 3f;
        #endregion

        #region Dig
        [SerializeField] bool digAction = false;
        [SerializeField] private GameObject digObject;
        [SerializeField] int maxDigDistance = 50;
        [SerializeField] private float digDuration = 5f;
        List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
        bool hasDigged = false;
        #endregion

        protected override void Start()
        {
            base.Start();
            nMines = MAX_MINES;
            colliderDiameter = GetComponent<CircleCollider2D>().radius * 2;
            disPerStep = colliderDiameter / 10;
            spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>().ToList();
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
            digTimer += Time.deltaTime;
        }

        #region States

        public void InitDigState()
        {
            foreach (var sprite in spriteRenderers)
            {
                sprite.enabled = false;
            }
            GameObject.Instantiate(digObject, transform.position, Quaternion.identity);
        }

        public Status DigState()
        {
            stateTimer += Time.deltaTime;
            if(stateTimer >= digDuration)
            {
                transform.position = PatrolManager.Instance.GetDigAppearPoint(maxDigDistance).position;
                foreach (var sprite in spriteRenderers)
                {
                    sprite.enabled = true;
                }
                GameObject.Instantiate(digObject, transform.position, Quaternion.identity);
                hasDigged = true;
            }
            
            return Status.Running;
        }

        public Status PutMineState()
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= putMineDuration)
            {
                PlaceMine();
                hasPutMine = true;
            }
            return Status.Running;
        }
        #endregion

        #region Perceptions
        public bool CheckIdleToMinerUS()
        {
            return player != null;
        }

        public bool CheckMinerUSToIdle()
        {
            return player == null;
        }

        public bool CheckMinerUSToDig()
        {
            return digAction;
        }

        public bool CheckDigToMinerUS()
        {
            return hasDigged;
        }

        public bool CheckMinerUSToPutMine()
        {
            return putMineAction;
        }

        public bool CheckPutMineToMinerUS()
        {
            return hasPutMine;
        }

        public void ActionMinerUSExit()
        {
            hasDigged = false;
            hasPutMine = false;
            putMineAction = false;
            digAction = false;
            stateTimer = 0;
        }
        #endregion

        #region UtilitySystem
        #region Variables
        public float HPPlayer()
        {
            if (player != null)
            {
                return (float)player.GetComponent<PVECharacterData>().Health / maxPlayerHp;
            }
            return 0;
        }

        public float NAllies()
        {
           float uNa = (float)genericTargets.Count / MAX_NA;
            if (uNa >= 1) return 1;
            return uNa;
        }

        public float NMines()
        {
            return nMines / MAX_MINES;
        }

        public float Distance()
        {
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                float distanceNormalized = distance / agentController.npcData.idealDistance;
                if(distanceNormalized > 1)
                {
                    distanceNormalized = 1;
                }
                return distanceNormalized;
            }
            return 0;
        }

        public float Cover()
        {
            if (player != null)
            {
                int nHits = 0;
                Vector2 minerToPlayer = player.transform.position - transform.position;
                Vector2 minerToPlayerDir = minerToPlayer.normalized;
                Vector2 radiusDir = new Vector2(-minerToPlayerDir.y, minerToPlayerDir.x);
                Vector2 nextRayPos = (Vector2)transform.position + radiusDir * colliderDiameter / 2;
                for(int i = 0; i < nRayCastCover; i++)
                {
                    if(Physics2D.Raycast(nextRayPos, minerToPlayerDir, minerToPlayer.magnitude, coverLayers))
                    {
                        nHits++;
                    }
                    nextRayPos = nextRayPos - radiusDir * disPerStep;
                }
                return (float)nHits / nRayCastCover;
            }
            return 0;
        }

        public float CanMine()
        {
            if (player == null || cannonReloaded == false || nMines == 0)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public float CanExcave()
        {
            if(digTimer >= excaveCooldown)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        #endregion
        #region Functions
        public float AggroHPP(float HPP)
        {
            if (HPP == 0) HPP = 1 / maxPlayerHp;
            return 1 / (maxPlayerHp *  HPP);
        }

        public float AggroNA(float NA)
        {
            return (1 / MAX_NA) / (NA + 1 / MAX_NA);
        }
        #endregion
        #region UtlityActions
        public Status MoveAggro()
        {
            return Status.Success;
        }

        public Status MoveDef()
        {
            return Status.Success;
        }

        public Status Dig()
        {
            digAction = true;
            return Status.Success;
        }

        public Status PutMine()
        {
            putMineAction = true;
            return Status.Success;
        }
        #endregion 
        #endregion

        #region Utilities
        private void PlaceMine()
        {
            if (minePrefab != null)
            {
                Instantiate(minePrefab, transform.position, Quaternion.identity);
                Debug.Log("Minero ha colocado una mina.");
                nMines--;
            }
            else
            {
                Debug.LogError("fallo al poner mina");
            }
        }

        void OnPlayerDetected(GameObject gameObject)
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
        #endregion
    }
}

