using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        float excaveTimer = 0;

        protected override void Start()
        {
            base.Start();
            nMines = MAX_MINES;
            colliderDiameter = GetComponent<CircleCollider2D>().radius * 2;
            disPerStep = colliderDiameter / 10;
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
            excaveTimer += Time.deltaTime;
        }


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
            return genericTargets.Count;
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
            if(excaveTimer >= excaveCooldown)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        #endregion 
        #endregion

        #region Utilities
        void OnPlayerDetected(GameObject gameObject)
        {
            if(maxPlayerHp == 0)
            {
                maxPlayerHp = player.GetComponent<PVECharacterData>().Max_Health;
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

