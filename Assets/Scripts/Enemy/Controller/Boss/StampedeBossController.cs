using Cysharp.Threading.Tasks;
using UnityEngine;

namespace EnemyController
{
    /// <summary>
    /// Stampede Boss: &gt;50% HP — lao húc (Charge); ≤50% HP — 3 pattern đạn.
    /// Cần: EnemyChargeAttack + StampedeBossProjectileAttack + Health + EnemySO (isBoss).
    /// </summary>
    public class StampedeBossController : BaseAIController
    {
        [Header("Phase")]
        [SerializeField] [Range(0.1f, 0.9f)] private float phase2HealthThreshold = 0.5f;
        [SerializeField] private Color phase2FlashColor = new Color(1f, 0.35f, 0.1f);

        [Header("Phase 2 — di chuyển")]
        [SerializeField] private float phase2SafeDistanceRatio = 0.55f;

        private EnemyChargeAttack chargeAttack;
        private StampedeBossProjectileAttack projectileAttack;
        private EnemyEvents enemyEvents;

        private bool isPhase2;
        private bool isCharging;
        private bool isCasting;
        private int trackedMaxHealth;
        private int patternCycleIndex;

        protected override void Awake()
        {
            base.Awake();
            chargeAttack = GetComponent<EnemyChargeAttack>();
            projectileAttack = GetComponent<StampedeBossProjectileAttack>();
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
            isCharging = false;
            isCasting = false;
        }

        protected override void OnPlayerInitialized()
        {
            if (chargeAttack != null)
                chargeAttack.SetPlayer(player);
            if (projectileAttack != null)
                projectileAttack.SetPlayer(player);

            SubscribeHealth();
            if (health != null)
                trackedMaxHealth = health.MaxHealth;
        }

        private void ResetBossState()
        {
            isPhase2 = false;
            isCharging = false;
            isCasting = false;
            patternCycleIndex = 0;
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
            Debug.Log("[StampedeBoss] Phase 2 — projectile patterns.");

            if (health != null && health.HitFlash != null)
                health.HitFlash.Play(phase2FlashColor, 0.35f).Forget();
        }

        protected override void ExecuteBehaviour()
        {
            if (isCharging || isCasting) return;

            if (!isPhase2)
                ExecutePhase1Charge();
            else
                ExecutePhase2Ranged();
        }

        private void ExecutePhase1Charge()
        {
            if (chargeAttack == null || player == null || enemyData == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= enemyData.AttackRange)
            {
                if (chargeAttack.CanAttack())
                    TriggerCharge().Forget();
                else
                {
                    if (agent.enabled) agent.isStopped = true;
                    LookAtPlayer();
                }
            }
            else if (agent.enabled)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }

        private void ExecutePhase2Ranged()
        {
            if (projectileAttack == null || player == null || enemyData == null) return;

            float distance = Vector3.Distance(transform.position, player.position);
            float maxRange = enemyData.AttackRange;
            float minSafe = maxRange * phase2SafeDistanceRatio;

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
                    TriggerProjectilePattern().Forget();
            }
            else if (agent.enabled)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }

        private async UniTaskVoid TriggerCharge()
        {
            isCharging = true;

            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            LookAtPlayer();
            await chargeAttack.PerformAttack(agent);

            if (this != null && gameObject != null && agent != null)
            {
                agent.enabled = true;
                agent.nextPosition = transform.position;
            }

            isCharging = false;
        }

        private async UniTaskVoid TriggerProjectilePattern()
        {
            isCasting = true;
            LookAtPlayer();

            var pattern = (StampedeBossAttackPattern)(patternCycleIndex % 3);
            patternCycleIndex++;

            await projectileAttack.PerformPattern(pattern);

            isCasting = false;
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
