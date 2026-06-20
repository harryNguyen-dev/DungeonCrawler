using System;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyController
{
    /// <summary>Imp Mischief Junior Boss — bắn nhiều pattern với nhiều loại projectile.</summary>
    public class ImpMischiefJuniorBossProjectileAttack : BaseAttack
    {
        [Header("Projectile Types")]
        [SerializeField] private GameObject standardProjectilePrefab;
        [SerializeField] private GameObject heavyProjectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private bool useAnimationEvents = true;

        [Header("Pattern 1 — quạt ngắm player")]
        [SerializeField] private int spreadPairCount = 2;
        [SerializeField] private float spreadAngleStep = 18f;
        [SerializeField] private int windUpMsSpread = 350;

        [Header("Pattern 2 — bắn liên tiếp")]
        [SerializeField] private int rapidShotCount = 6;
        [SerializeField] private int rapidShotIntervalMs = 100;
        [SerializeField] private int windUpMsRapid = 250;

        [Header("Pattern 3 — vòng tròn (heavy projectile)")]
        [SerializeField] private int circleBulletCount = 10;
        [SerializeField] private int windUpMsCircle = 400;

        [Header("Pattern 4 — xoắn ốc")]
        [SerializeField] private int spiralShotCount = 8;
        [SerializeField] private float spiralAngleStep = 22.5f;
        [SerializeField] private int spiralShotIntervalMs = 90;
        [SerializeField] private int windUpMsSpiral = 300;

        private ImpMischiefJuniorBossAttackPattern pendingPattern;
        private bool awaitingAnimationHit;

        public async UniTask PerformPattern(ImpMischiefJuniorBossAttackPattern pattern)
        {
            if (!canAttack || enemyData == null) return;
            canAttack = false;
            pendingPattern = pattern;
            IsAttackInProgress = true;

            try
            {
                if (useAnimationEvents && baseEnemyAnimation != null)
                {
                    awaitingAnimationHit = true;
                    // AC_Enemy_Base dùng Attack 1/2 + AttackTrigger, không có state "Projectile Attack".
                    baseEnemyAnimation.SetAttackTrigger();

                    float waited = 0f;
                    const float maxWait = 1.5f;
                    while (awaitingAnimationHit && waited < maxWait)
                    {
                        if (this == null) return;
                        waited += Time.deltaTime;
                        await UniTask.Yield();
                    }

                    if (awaitingAnimationHit)
                    {
                        awaitingAnimationHit = false;
                        FirePendingPattern();
                    }
                }
                else
                {
                    await PerformPatternTimed(pattern);
                }

                if (enemyData != null)
                    await UniTask.Delay(TimeSpan.FromSeconds(enemyData.AttackCooldown));
            }
            finally
            {
                IsAttackInProgress = false;
                if (this != null) canAttack = true;
            }
        }

        public override UniTask PerformAttack(NavMeshAgent agent)
        {
            return PerformPattern(ImpMischiefJuniorBossAttackPattern.AimedSpread);
        }

        public override void OnAnimationAttackEvent()
        {
            if (!awaitingAnimationHit) return;
            awaitingAnimationHit = false;
            FirePendingPattern();
        }

        private void FirePendingPattern()
        {
            switch (pendingPattern)
            {
                case ImpMischiefJuniorBossAttackPattern.AimedSpread:
                    FireAimedSpread();
                    break;
                case ImpMischiefJuniorBossAttackPattern.RapidBurst:
                    FireRapidBurst().Forget();
                    break;
                case ImpMischiefJuniorBossAttackPattern.CircleBurst:
                    FireCircleBurst();
                    break;
                case ImpMischiefJuniorBossAttackPattern.SpiralVolley:
                    FireSpiralVolley().Forget();
                    break;
            }
        }

        private async UniTask PerformPatternTimed(ImpMischiefJuniorBossAttackPattern pattern)
        {
            switch (pattern)
            {
                case ImpMischiefJuniorBossAttackPattern.AimedSpread:
                    await UniTask.Delay(windUpMsSpread);
                    if (this == null) return;
                    FireAimedSpread();
                    break;
                case ImpMischiefJuniorBossAttackPattern.RapidBurst:
                    await UniTask.Delay(windUpMsRapid);
                    if (this == null) return;
                    await FireRapidBurst();
                    break;
                case ImpMischiefJuniorBossAttackPattern.CircleBurst:
                    await UniTask.Delay(windUpMsCircle);
                    if (this == null) return;
                    FireCircleBurst();
                    break;
                case ImpMischiefJuniorBossAttackPattern.SpiralVolley:
                    await UniTask.Delay(windUpMsSpiral);
                    if (this == null) return;
                    await FireSpiralVolley();
                    break;
            }
        }

        private void FireAimedSpread()
        {
            Vector3 baseDir = GetAimDirection();
            SpawnProjectile(baseDir, standardProjectilePrefab);

            for (int i = 1; i <= spreadPairCount; i++)
            {
                float angle = spreadAngleStep * i;
                SpawnProjectile(Quaternion.Euler(0f, -angle, 0f) * baseDir, standardProjectilePrefab);
                SpawnProjectile(Quaternion.Euler(0f, angle, 0f) * baseDir, standardProjectilePrefab);
            }
        }

        private async UniTask FireRapidBurst()
        {
            for (int i = 0; i < rapidShotCount; i++)
            {
                if (this == null || player == null) return;
                SpawnProjectile(GetAimDirection(), standardProjectilePrefab);
                if (i < rapidShotCount - 1)
                    await UniTask.Delay(rapidShotIntervalMs);
            }
        }

        private void FireCircleBurst()
        {
            if (circleBulletCount <= 0) return;

            var prefab = heavyProjectilePrefab != null ? heavyProjectilePrefab : standardProjectilePrefab;
            float step = 360f / circleBulletCount;
            float yaw = transform.eulerAngles.y;

            for (int i = 0; i < circleBulletCount; i++)
            {
                float angle = yaw + i * step;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                SpawnProjectile(dir, prefab, flattenToHorizontal: true);
            }
        }

        private async UniTask FireSpiralVolley()
        {
            float yaw = transform.eulerAngles.y;
            for (int i = 0; i < spiralShotCount; i++)
            {
                if (this == null) return;
                float angle = yaw + i * spiralAngleStep;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                SpawnProjectile(dir, standardProjectilePrefab);
                if (i < spiralShotCount - 1)
                    await UniTask.Delay(spiralShotIntervalMs);
            }
        }

        private Vector3 GetAimDirection()
        {
            Vector3 spawnPos = GetSpawnPosition();
            if (player == null)
                return transform.forward;

            // Giống EnemyRangedAttack — ngắm ngực player, không ép phẳng Y (tránh bay xẹt dưới chân).
            Vector3 target = player.position + Vector3.up * 1f;
            Vector3 dir = (target - spawnPos).normalized;
            return dir.sqrMagnitude > 0.001f ? dir : transform.forward;
        }

        private Vector3 GetSpawnPosition()
        {
            return firePoint != null
                ? firePoint.position
                : transform.position + Vector3.up * 1f + transform.forward * 0.5f;
        }

        private void SpawnProjectile(Vector3 direction, GameObject prefab, bool flattenToHorizontal = false)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[{gameObject.name}] ImpMischiefJuniorBossProjectileAttack: chưa gán projectile prefab.");
                return;
            }

            if (flattenToHorizontal)
            {
                direction.y = 0f;
                if (direction.sqrMagnitude < 0.001f)
                    direction = transform.forward;
                direction.Normalize();
            }
            else if (direction.sqrMagnitude < 0.001f)
            {
                direction = transform.forward;
            }
            else
            {
                direction.Normalize();
            }

            Vector3 spawnPos = GetSpawnPosition();
            Quaternion rot = Quaternion.LookRotation(direction);

            GameObject bulletObj = null;
            if (prefab.TryGetComponent<PooledObject>(out var pooled) &&
                pooled.PoolId != PoolId.None &&
                ProjectilePool.Instance != null)
            {
                bulletObj = ProjectilePool.Instance.Get(pooled.PoolId, spawnPos, rot);
            }
            else
            {
                bulletObj = Instantiate(prefab, spawnPos, rot);
                ObjectPoolBase.NotifySpawnedFromPool(bulletObj);
            }

            if (bulletObj == null) return;

            var projectile = bulletObj.GetComponent<EnemyProjectile>();
            if (projectile != null)
                projectile.Setup(GetEffectiveDamage());
            else
                Debug.LogError($"[{gameObject.name}] Projectile thiếu EnemyProjectile.");
        }
    }
}
