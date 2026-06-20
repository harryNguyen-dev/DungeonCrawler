using System;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyController
{
    /// <summary>Phase 2 Stampede Boss — 3 kiểu bắn đạn.</summary>
    public class StampedeBossProjectileAttack : BaseAttack
    {
        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private bool useAnimationEvents = true;

        [Header("Pattern 1 — 3 tia góc")]
        [SerializeField] private float spreadHalfAngle = 22.5f;
        [SerializeField] private int windUpMsSpread = 300;

        [Header("Pattern 2 — 5 viên liên tiếp")]
        [SerializeField] private int rapidShotCount = 5;
        [SerializeField] private int rapidShotIntervalMs = 120;
        [SerializeField] private int windUpMsRapid = 250;

        [Header("Pattern 3 — vòng tròn 2 đợt")]
        [SerializeField] private int circleBulletCount = 12;
        [SerializeField] private float circleWave2AngleOffset = 15f;
        [SerializeField] private int circleWaveDelayMs = 450;
        [SerializeField] private int windUpMsCircle = 400;

        private StampedeBossAttackPattern pendingPattern;
        private bool awaitingAnimationHit;

        public async UniTask PerformPattern(StampedeBossAttackPattern pattern)
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
                    baseEnemyAnimation.PlayProjectileAttack();

                    float waited = 0f;
                    const float maxWait = 2.5f;
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
            return PerformPattern(StampedeBossAttackPattern.TripleSpread);
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
                case StampedeBossAttackPattern.TripleSpread:
                    FireTripleSpread();
                    break;
                case StampedeBossAttackPattern.RapidFive:
                    FireRapidFive().Forget();
                    break;
                case StampedeBossAttackPattern.CircleDoubleWave:
                    FireCircleDoubleWave().Forget();
                    break;
            }
        }

        private async UniTask PerformPatternTimed(StampedeBossAttackPattern pattern)
        {
            switch (pattern)
            {
                case StampedeBossAttackPattern.TripleSpread:
                    await UniTask.Delay(windUpMsSpread);
                    if (this == null) return;
                    FireTripleSpread();
                    break;
                case StampedeBossAttackPattern.RapidFive:
                    await UniTask.Delay(windUpMsRapid);
                    if (this == null) return;
                    await FireRapidFive();
                    break;
                case StampedeBossAttackPattern.CircleDoubleWave:
                    await UniTask.Delay(windUpMsCircle);
                    if (this == null) return;
                    await FireCircleDoubleWave();
                    break;
            }
        }

        private void FireTripleSpread()
        {
            Vector3 baseDir = GetAimDirection();
            SpawnProjectile(baseDir);
            SpawnProjectile(Quaternion.Euler(0f, -spreadHalfAngle, 0f) * baseDir);
            SpawnProjectile(Quaternion.Euler(0f, spreadHalfAngle, 0f) * baseDir);
        }

        private async UniTask FireRapidFive()
        {
            for (int i = 0; i < rapidShotCount; i++)
            {
                if (this == null || player == null) return;
                SpawnProjectile(GetAimDirection());
                if (i < rapidShotCount - 1)
                    await UniTask.Delay(rapidShotIntervalMs);
            }
        }

        private async UniTask FireCircleDoubleWave()
        {
            SpawnCircleWave(0f);
            await UniTask.Delay(circleWaveDelayMs);
            if (this == null) return;
            SpawnCircleWave(circleWave2AngleOffset);
        }

        private void SpawnCircleWave(float angleOffsetDegrees)
        {
            if (circleBulletCount <= 0) return;

            float step = 360f / circleBulletCount;
            float yaw = transform.eulerAngles.y + angleOffsetDegrees;

            for (int i = 0; i < circleBulletCount; i++)
            {
                float angle = yaw + i * step;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                SpawnProjectile(dir);
            }
        }

        private Vector3 GetAimDirection()
        {
            Vector3 spawnPos = GetSpawnPosition();
            if (player == null)
                return transform.forward;

            Vector3 target = player.position + Vector3.up;
            Vector3 dir = (target - spawnPos).normalized;
            dir.y = 0f;
            return dir.sqrMagnitude > 0.001f ? dir.normalized : transform.forward;
        }

        private Vector3 GetSpawnPosition()
        {
            return firePoint != null
                ? firePoint.position
                : transform.position + Vector3.up * 1f + transform.forward * 0.5f;
        }

        private void SpawnProjectile(Vector3 direction)
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning($"[{gameObject.name}] StampedeBossProjectileAttack: chưa gán projectile prefab.");
                return;
            }

            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
                direction = transform.forward;
            direction.Normalize();

            Vector3 spawnPos = GetSpawnPosition();
            Quaternion rot = Quaternion.LookRotation(direction);

            GameObject bulletObj = null;
            if (projectilePrefab.TryGetComponent<PooledObject>(out var pooled) &&
                pooled.PoolId != PoolId.None &&
                ProjectilePool.Instance != null)
            {
                bulletObj = ProjectilePool.Instance.Get(pooled.PoolId, spawnPos, rot);
            }
            else
            {
                bulletObj = Instantiate(projectilePrefab, spawnPos, rot);
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
