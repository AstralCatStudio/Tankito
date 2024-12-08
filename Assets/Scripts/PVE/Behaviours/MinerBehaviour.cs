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
        [SerializeField] GameObject player;
        [SerializeField] AreaDetection playerAreaDetection;
        float maxPlayerHp;
        int nMines;
        const int MAX_MINES = 10;

        [SerializeField] int nRayCastCover = 10;
        float colliderDiameter;
        float disPerStep;
        [SerializeField] LayerMask coverLayers;

        [SerializeField] float digCooldown = 10;
        float digTimer = 0;
        const int MAX_NA = 5;

        [SerializeField] private GameObject minePrefab;
        private float stateTimer = 0f;
        float currentUtility;
        int currentDirection;

        #region PutMine
        [SerializeField] bool putMineAction = false;
        bool hasPutMine = false;
        [SerializeField] private float putMineDuration = 3f;
        [SerializeField] private float timerRechargeMine = 45f;
        private Coroutine rechargeCoroutine; // PARA RECARGAR LAS MINAS
        #endregion

        #region Dig
        [SerializeField] bool digAction = false;
        [SerializeField] private GameObject digObject;
        [SerializeField] int maxDigDistance = 30;
        [SerializeField] private float digDuration = 5f;
        [SerializeField] List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
        bool hasDigged = false;
        #endregion

        protected void OnEnable()
        {
            if (rechargeCoroutine == null)
            {
                rechargeCoroutine = StartCoroutine(RechargeMineTimer()); // CORRUTINA PARA RECARGAR LAS MINAS
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            playerAreaDetection.OnSubjectDetected -= OnPlayerDetected;
            playerAreaDetection.OnSubjectDissapear -= OnPlayerDissapear;

            if (rechargeCoroutine != null)
            {
                StopCoroutine(rechargeCoroutine); // PARAR LA CORRUTINA DE RECARGAR MINAS
                rechargeCoroutine = null;
            }
        }

        private IEnumerator RechargeMineTimer()
        {
            while (true)
            {
                yield return new WaitForSeconds(timerRechargeMine);
                AddMine();
            }
        }

        protected override void Start()
        {
            base.Start();
            nMines = MAX_MINES;
            colliderDiameter = GetComponent<CircleCollider2D>().radius * 2;
            disPerStep = colliderDiameter / 10;
            spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>().ToList();
            playerAreaDetection.OnSubjectDetected += OnPlayerDetected;
            playerAreaDetection.OnSubjectDissapear += OnPlayerDissapear;

            StartCoroutine(RechargeMineTimer());
        }

        protected override void Update()
        {
            base.Update();
            digTimer += Time.deltaTime;
        }

        #region States

        public void InitDigState()
        {
            Debug.Log("INICIA DIG");
            foreach (var sprite in spriteRenderers)
            {
                sprite.enabled = false;
            }
            GetComponent<CircleCollider2D>().enabled = false;
            Instantiate(digObject, transform.position, Quaternion.identity);
        }

        public Status DigState()
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= digDuration)
            {
                transform.position = PatrolManager.Instance.GetDigAppearPoint(maxDigDistance).position;
                foreach (var sprite in spriteRenderers)
                {
                    sprite.enabled = true;
                }
                GetComponent<CircleCollider2D>().enabled = true;
                Instantiate(digObject, transform.position, Quaternion.identity);
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

        public Status ActionMinerUSExit()
        {
            hasDigged = false;
            hasPutMine = false;
            putMineAction = false;
            digAction = false;
            m_currentInput.moveVector = transform.position;
            importantPos = true;
            stateTimer = 0;
            return Status.Success;
        }

        public Status ActionIdleToMinerUS()
        {
            patrolPointFound = false;
            ActionMinerUSEnter();
            return Status.Success;
        }

        public Status ActionPutMineToMinerUS()
        {
            ActionShootPOP();
            ActionMinerUSEnter();
            return Status.Success;
        }

        public Status ActionMinerUSEnter()
        {
            currentDirection = Random.Range(0, 2);
            if (currentDirection == 0) currentDirection = -1;
            return Status.Success;
        }
        #endregion

        #region UtilitySystem
        #region Variables
        public float HP_Player()
        {
            if (player != null)
            {
                return (float)(player.GetComponent<PVECharacterData>().Health / maxPlayerHp);
            }
            return 0;
        }

        public float NAllies()
        {
            float uNa = (float)(genericTargets.Count / MAX_NA);
            if (uNa >= 1) return 1;
            return uNa;
        }

        public float NMines()
        {
            //Debug.Log(nMines);
            //Debug.Log(MAX_MINES);
            //Debug.Log((float)nMines / (float)MAX_MINES);
            return (float)nMines / (float)MAX_MINES;
        }

        public float Distance()
        {
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                float distanceNormalized = distance / agentController.npcData.idealDistance;
                if (distanceNormalized > 1)
                {
                    distanceNormalized = 0;
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
                for (int i = 0; i < nRayCastCover; i++)
                {
                    if (Physics2D.Raycast(nextRayPos, minerToPlayerDir, minerToPlayer.magnitude, coverLayers))
                    {
                        nHits++;
                    }
                    nextRayPos = nextRayPos - radiusDir * disPerStep;
                }
                return (float)(nHits / nRayCastCover);
            }
            return 0;
        }

        public float CanPutMine()
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

        public float CanDig()
        {
            if (digTimer >= digCooldown)
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
            return 1 / (maxPlayerHp * HPP);
        }

        public float AggroNA(float NA)
        {
            return (1 / MAX_NA) / (NA + 1 / MAX_NA);
        }
        #endregion
        #region UtlityActions
        public Status MoveAggro()
        {
            if (player != null)
            {
                float angle = currentDirection * 90 * (1 - currentUtility);
                Vector2 minerToPlayer = (player.transform.position - transform.position).normalized;
                SetNewMinerUSPosition(angle, minerToPlayer);
                m_currentInput.moveVector = CheckNewPosition(m_currentInput.moveVector);
            }

            return Status.Running;
        }

        public Status MoveDef()
        {
            float angle = currentDirection * 90 * (1 - currentUtility);
            Vector2 playerToMiner = (transform.position - player.transform.position).normalized;
            SetNewMinerUSPosition(angle, playerToMiner);
            m_currentInput.moveVector = CheckNewPosition(m_currentInput.moveVector);
            return Status.Running;
        }

        public Status Dig()
        {
            digAction = true;
            return Status.Running;
        }

        public Status PutMine()
        {
            putMineAction = true;
            return Status.Running;
        }
        #endregion 
        #endregion

        #region Utilities
        private void PlaceMine()
        {
            if (minePrefab != null)
            {
                GameObject mineObject = Instantiate(minePrefab, transform.position, Quaternion.identity);
                Mine mineScript = mineObject.GetComponent<Mine>();
                if (mineScript != null)
                {
                    mineScript.SetMinerReference(this);
                }
                Debug.Log("Minero ha colocado una mina.");
                nMines--;
            }
            else
            {
                Debug.LogError("fallo al poner mina");
            }
        }


        public void AddMine()
        {
            if (nMines < MAX_MINES)
            {
                nMines++;
                MusicManager.Instance.PlaySound("snd_rango_danio");
            }
        }

        void OnPlayerDetected(GameObject gameObject)
        {
            if (maxPlayerHp == 0)
            {
                maxPlayerHp = gameObject.GetComponent<PVECharacterData>().Max_Health;
            }
            player = gameObject;
        }

        private void OnPlayerDissapear(GameObject gameObject)
        {
            player = null;
        }

        private void SetNewMinerUSPosition(float angle, Vector2 vec)
        {
            float angleInRadians = angle * Mathf.Deg2Rad;

            Vector2 dirVec = new Vector2(
                vec.x * Mathf.Cos(angleInRadians) - vec.y * Mathf.Sin(angleInRadians),
                vec.x * Mathf.Sin(angleInRadians) + vec.y * Mathf.Cos(angleInRadians)
            );
            m_currentInput.moveVector = (Vector2)transform.position + dirVec * agentController.agent.speed * 3;
        }

        public void OnGetUtility(float utility)
        {
            currentUtility = utility;
        }
        #endregion
    }
}

