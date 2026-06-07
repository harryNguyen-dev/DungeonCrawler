using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using Global;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyController
{
    /// <summary>
    /// Imp Mischief Junior Boss: phase 1 bắn 4 pattern; phase 2 thêm triệu hồi ImpMischief / Imp Mischief Ranger.
    /// Cần: ImpMischiefJuniorBossProjectileAttack + Health + EnemySO (isBoss).
    /// </summary>
    public class ImpMischiefJuniorBossController : BaseAIController
    {
        [Header("Phase")]
        [SerializeField] [Range(0.1f, 0.9f)] private float phase2HealthThreshold = 0.55f;
        [SerializeField] private Color phase2FlashColor = new Color(0.85f, 0.2f, 1f);

        [Header("Ranged AI")]
        [SerializeField] private float safeDistanceRatio = 0.55f;

        [Header("Summon")]
        [SerializeField] private GameObject impMischiefPrefab;
        [SerializeField] private GameObject impMischiefRangerPrefab;
        [SerializeField] private int summonCountPhase1 = 1;
        [SerializeField] private int summonCountPhase2 = 2;
        [SerializeField] private float minionSpawnRadius = 3.5f;
        [SerializeField] private int maxActiveMinions = 6;
        [SerializeField] private int summonWindUpMs = 600;
        [SerializeField] [Range(0f, 1f)] private float rangerSpawnWeight = 0.5f;

        private ImpMischiefJuniorBossProjectileAttack projectileAttack;
        private EnemyEvents enemyEvents;

        private bool isPhase2;
        private bool isActing;
        private int patternCycleIndex;
        private int trackedMaxHealth;
        private readonly List<GameObject> trackedMinions = new();

        protected override void Awake()
        {
            base.Awake();
            projectileAttack = GetComponent<ImpMischiefJuniorBossProjectileAttack>();
            enemyEvents = GetComponent<EnemyEvents>();
        }

        public override void OnSpawnedFromPool()
        {
            base.OnSpawnedFromPool();
            ResetBossState();
        }

        public override void OnReturnedToPool()
        {
            UnsubscribeHealth();
            base.OnReturnedToPool();
            isActing = false;
            trackedMinions.Clear();
        }

        protected override void OnPlayerInitialized()
        {
            if (projectileAttack != null)
                projectileAttack.SetPlayer(player);

            SubscribeHealth();
            if (health != null)
                trackedMaxHealth = health.MaxHealth;
        }

        private void ResetBossState()
        {
            isPhase2 = false;
            isActing = false;
            patternCycleIndex = 0;
            trackedMinions.Clear();
            UnsubscribeHealth();
            SubscribeHealth();
            if (health != null)
                trackedMaxHealth = health.MaxHealth;
        }

        private void SubscribeHealth()
        {
            if (enemyEvents == null) return;
            enemyEvents.OnHealthChange -= OnHealthChanged;
            enemyEvents.OnHealthChange += OnHealthChanged;
        }

        private void UnsubscribeHealth()
        {
            if (enemyEvents == null) return;
            enemyEvents.OnHealthChange -= OnHealthChanged;
        }

        private void OnHealthChanged(int currentHealth)
        {
            if (isPhase2 || trackedMaxHealth <= 0) return;

            float ratio = (float)currentHealth / trackedMaxHealth;
            if (ratio <= phase2HealthThreshold)
                EnterPhase2();
        }

        private void EnterPhase2()
        {
            isPhase2 = true;
            Debug.Log("[ImpMischiefJuniorBoss] Phase 2 — summon + projectile patterns.");

            if (health != null && health.HitFlash != null)
                health.HitFlash.Play(phase2FlashColor, 0.35f).Forget();
        }

        protected override void ExecuteBehaviour()
        {
            if (isActing || projectileAttack == null || player == null || enemyData == null) return;

            float distance = Vector3.Distance(transform.position, player.position);
            float maxRange = enemyData.AttackRange;
            float minSafe = maxRange * safeDistanceRatio;

            if (distance < minSafe)
            {
                if (agent.enabled)
                {
                    agent.isStopped = false;
                    Vector3 retreat = (transform.position - player.position).normalized;
                    agent.SetDestination(transform.position + retreat * 4f);
                }
            }
            else if (distance <= maxRange)
            {
                if (agent.enabled) agent.isStopped = true;
                LookAtPlayer();

                if (projectileAttack.CanAttack())
                    TriggerBossAction().Forget();
            }
            else if (agent.enabled)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }

        private async UniTaskVoid TriggerBossAction()
        {
            isActing = true;
            LookAtPlayer();

            var pattern = GetNextPattern();
            if (pattern == ImpMischiefJuniorBossAttackPattern.SummonMinions)
                await PerformSummon();
            else
                await projectileAttack.PerformPattern(pattern);

            isActing = false;
        }

        private ImpMischiefJuniorBossAttackPattern GetNextPattern()
        {
            const int patternCount = 5;
            var pattern = (ImpMischiefJuniorBossAttackPattern)(patternCycleIndex % patternCount);
            patternCycleIndex++;
            return pattern;
        }

        private async UniTask PerformSummon()
        {
            if (impMischiefPrefab == null && impMischiefRangerPrefab == null)
            {
                await projectileAttack.PerformPattern(ImpMischiefJuniorBossAttackPattern.CircleBurst);
                return;
            }

            if (baseEnemyAnimation != null)
                baseEnemyAnimation.SetAttackTrigger();

            await UniTask.Delay(summonWindUpMs);
            if (this == null) return;

            int slotsLeft = maxActiveMinions - CountActiveMinions();
            int desiredCount = isPhase2 ? summonCountPhase2 : summonCountPhase1;
            int toSpawn = Mathf.Min(desiredCount, slotsLeft);
            if (toSpawn <= 0) return;

            float angleStep = 360f / toSpawn;
            float startAngle = transform.eulerAngles.y + 45f;

            for (int i = 0; i < toSpawn; i++)
            {
                var prefab = PickMinionPrefab();
                if (prefab == null) continue;

                float angle = startAngle + i * angleStep;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * minionSpawnRadius;
                Vector3 rawPos = transform.position + offset;

                if (!TrySampleSpawnPosition(rawPos, out Vector3 spawnPos))
                    TrySampleSpawnPosition(transform.position, out spawnPos);

                var instance = SpawnEnemyInstance(prefab, spawnPos, Quaternion.LookRotation(offset.normalized));
                if (instance == null) continue;

                GlobalEntities.Instance?.RegisterEnemy(instance);
                trackedMinions.Add(instance);
            }
        }

        private static bool TrySampleSpawnPosition(Vector3 rawPos, out Vector3 spawnPos)
        {
            if (NavMesh.SamplePosition(rawPos, out NavMeshHit hit, 2.5f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
                return true;
            }

            spawnPos = default;
            return false;
        }

        private int CountActiveMinions()
        {
            for (int i = trackedMinions.Count - 1; i >= 0; i--)
            {
                var minion = trackedMinions[i];
                if (minion == null || !minion.activeInHierarchy)
                {
                    trackedMinions.RemoveAt(i);
                    continue;
                }

                var minionHealth = minion.GetComponent<Health>();
                if (minionHealth != null && minionHealth.IsDead)
                    trackedMinions.RemoveAt(i);
            }

            return trackedMinions.Count;
        }

        private GameObject PickMinionPrefab()
        {
            bool wantRanger = Random.value < rangerSpawnWeight;
            if (wantRanger && impMischiefRangerPrefab != null)
                return impMischiefRangerPrefab;
            if (impMischiefPrefab != null)
                return impMischiefPrefab;
            return impMischiefRangerPrefab;
        }

        private static GameObject SpawnEnemyInstance(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            PoolId poolId = PoolId.None;
            if (prefab != null && prefab.TryGetComponent<PooledObject>(out var pooledObject))
                poolId = pooledObject.PoolId;

            if (poolId != PoolId.None && EnemyPool.Instance != null)
            {
                var fromPool = EnemyPool.Instance.Get(poolId, position, rotation);
                if (fromPool != null)
                    return fromPool;
            }

            var instance = Object.Instantiate(prefab, position, rotation);
            ObjectPoolBase.NotifySpawnedFromPool(instance);
            return instance;
        }

        private void LookAtPlayer()
        {
            if (player == null) return;
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction.normalized);
        }
    }
}
