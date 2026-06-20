using System;
using System.Collections.Generic;
using Components;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Core;

namespace Projectile
{
    public class ProjectileController : MonoBehaviour, IPoolable
    {
        [Header("VFX")]
        [SerializeField] private GameObject hitPrefab;

        private int damage;
        private int pierceCount;
        private float explosiveRadius;
        private float explosiveSplashMultiplier = 0.6f;
        private int hitCount = 0;
        private bool isBoomerang = false;
        private bool hasReturned = false;
        Dictionary<SO.WeaponEffectType, float> effects;

        HashSet<EnemyController.Health> enemiesHit = new HashSet<EnemyController.Health>();
        HashSet<Chest> chestsHit = new HashSet<Chest>();
        private ProjectileMove projectileMove;
        private bool isDespawning;

        private void Awake()
        {
            projectileMove = GetComponent<ProjectileMove>();
            projectileMove.SetDespawnCallback(DespawnProjectile);
            projectileMove.SetEnvironmentHitCallback(OnEnvironmentHit);
        }

        public void SetDamage(int damage)
        {
            this.damage = damage;
        }

        public void SetEffects(Dictionary<SO.WeaponEffectType, float> effects)
        {
            this.effects = new Dictionary<SO.WeaponEffectType, float>(effects);

            this.effects.TryGetValue(SO.WeaponEffectType.PierceCount, out var pierceCount);
            this.pierceCount = Mathf.RoundToInt(pierceCount);

            this.effects.TryGetValue(SO.WeaponEffectType.BoomerangMode, out var boomerangMode);
            this.isBoomerang = boomerangMode >= 1;

            this.effects.TryGetValue(SO.WeaponEffectType.ExplosiveRadius, out explosiveRadius);
        }

        public void SetProjectileActive()
        {
            projectileMove.ActiveSelf(isBoomerang, OnBoomerangReturn);
        }

        private void OnBoomerangReturn()
        {
            hasReturned = true;
            hitCount = 0;
            enemiesHit.Clear();
            chestsHit.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDespawning) return;

            if (ProjectileEnvironmentCollision.IsEnvironmentCollider(other))
            {
                OnEnvironmentHit(other.ClosestPoint(transform.position), -transform.forward);
                return;
            }

            var chest = other.GetComponent<Chest>() ?? other.GetComponentInParent<Chest>();
            if (chest != null)
            {
                HandleChestHit(chest, other);
                return;
            }

            if (!other.CompareTag("Enemy")) return;

            var health = other.GetComponent<EnemyController.Health>();
            if (health == null || enemiesHit.Contains(health)) return;

            enemiesHit.Add(health);

            var aiController = other.GetComponent<EnemyController.BaseAIController>();
            var exactHitPoint = other.ClosestPoint(transform.position);

            SpawnHitVfx(exactHitPoint, -transform.forward);

            if (aiController != null)
                aiController.TakeKnockback(transform.position);

            health.TakeDamage(damage);
            ApplyEffects(health, aiController);

            if (explosiveRadius > 0f)
            {
                ApplyExplosiveSplash(exactHitPoint, health);
                DespawnProjectile();
                return;
            }

            hitCount++;
            if (hitCount > pierceCount)
            {
                if (isBoomerang && !hasReturned)
                    projectileMove.StartReturnState();
                else
                    DespawnProjectile();
            }
        }

        private void DespawnProjectile()
        {
            if (isDespawning) return;
            isDespawning = true;
            PoolReturn.SafeReturn(gameObject);
        }

        private void HandleChestHit(Chest chest, Collider other)
        {
            if (chest.IsDestroyed || chestsHit.Contains(chest)) return;

            chestsHit.Add(chest);

            var exactHitPoint = other.ClosestPoint(transform.position);
            SpawnHitVfx(exactHitPoint, -transform.forward);
            chest.TakeDamage(damage);

            if (explosiveRadius > 0f)
            {
                ApplyExplosiveSplash(exactHitPoint, null);
                DespawnProjectile();
                return;
            }

            hitCount++;
            if (hitCount > pierceCount)
            {
                if (isBoomerang && !hasReturned)
                    projectileMove.StartReturnState();
                else
                    DespawnProjectile();
            }
        }

        private void OnEnvironmentHit(Vector3 hitPoint, Vector3 hitNormal)
        {
            if (isDespawning) return;

            SpawnHitVfx(hitPoint, hitNormal);
            DespawnProjectile();
        }

        private void SpawnHitVfx(Vector3 position, Vector3 normal) =>
            ProjectileVfxHelper.SpawnHit(hitPrefab, position, normal);

        private void ApplyExplosiveSplash(Vector3 center, EnemyController.Health primaryTarget)
        {
            var splashDamage = Mathf.Max(1, Mathf.RoundToInt(damage * explosiveSplashMultiplier));
            var hits = Physics.OverlapSphere(center, explosiveRadius);
            foreach (var col in hits)
            {
                if (!col.CompareTag("Enemy")) continue;

                var health = col.GetComponent<EnemyController.Health>();
                if (health == null || health == primaryTarget || enemiesHit.Contains(health)) continue;

                enemiesHit.Add(health);
                var ai = col.GetComponent<EnemyController.BaseAIController>();
                if (ai != null)
                    ai.TakeKnockback(center);

                health.TakeDamage(splashDamage);
            }
        }

        public void OnSpawnedFromPool()
        {
            hitCount = 0;
            hasReturned = false;
            isDespawning = false;
            enemiesHit.Clear();
            chestsHit.Clear();
        }

        public void OnReturnedToPool()
        {
            hitCount = 0;
            hasReturned = false;
            isDespawning = false;
            enemiesHit.Clear();
            chestsHit.Clear();
        }

        private void ApplyEffects(EnemyController.Health health, EnemyController.BaseAIController move)
        {
            foreach (var effect in effects)
            {
                if (effect.Key == SO.WeaponEffectType.FireDamage)
                    FireDamage(Mathf.RoundToInt(effect.Value), health).Forget();

                if (effect.Key == SO.WeaponEffectType.FrozenDuration)
                    FrozenDuration(Mathf.RoundToInt(effect.Value), move, health).Forget();
            }
        }

        private async UniTaskVoid FireDamage(int damage, EnemyController.Health health)
        {
            if (health == null) return;

            var cancellationToken = health.GetStatusEffectCancellationToken();

            try
            {
                health.gameObject.GetComponent<EnemyController.EnemyEffects>().ShowFireEffect();
                for (int i = 0; i < 3; i++)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);

                    if (health != null && !cancellationToken.IsCancellationRequested)
                        health.TakeDamage(damage);
                    else
                        break;
                }
                health.gameObject.GetComponent<EnemyController.EnemyEffects>().HideFireEffect();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[FireDamage] Tác vụ đốt lửa đã tự động hủy vì quái die hoặc bị pool trước.");
            }
        }

        private async UniTaskVoid FrozenDuration(float duration, EnemyController.BaseAIController baseAI, EnemyController.Health health)
        {
            if (baseAI == null || health == null) return;

            baseAI.UpdateAgentSpeed(0.0f);
            health.HitFlash.HitFrozen(duration).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            if (baseAI != null)
                baseAI.ReturnMoveSpeed();
        }
    }
}
