using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Core;

namespace Projectile
{
    public class ProjectileController : MonoBehaviour, IPoolable
    {
        private int damage;
        private int pierceCount;
        private float explosiveRadius;
        private float explosiveSplashMultiplier = 0.6f;
        private int hitCount = 0;
        private bool isBoomerang = false;
        private bool hasReturned = false;
        Dictionary<SO.WeaponEffectType, float> effects;

        HashSet<EnemyController.Health> enemiesHit = new HashSet<EnemyController.Health>();
        private ProjectileMove projectileMove;

        private void Awake()
        {
            projectileMove = GetComponent<ProjectileMove>();
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
            hitCount = 0; // Reset số lần xuyên thấu để đường về có thể xuyên tiếp
            enemiesHit.Clear(); // CHÌA KHÓA: Xóa danh sách quái đã trúng để đạn có thể gây sát thương lượt về
        }
        private void OnTriggerEnter(Collider other)
        {
            // Kiểm tra nếu chạm vào Quái
            if (other.CompareTag("Enemy"))
            {
                var health = other.GetComponent<EnemyController.Health>();
                if (health == null || enemiesHit.Contains(health)) return;
                
                enemiesHit.Add(health);

                var aiController = other.GetComponent<EnemyController.BaseAIController>();
                var hitEffect = Global.GlobalEntities.Instance?.playerHitEffect;

                Vector3 bulletPos = transform.position;
                Vector3 exactHitPoint = other.ClosestPoint(bulletPos);

                hitEffect?.PlayHitEffect(exactHitPoint, Quaternion.LookRotation(transform.forward));


                if (aiController != null)
                {
                    aiController.TakeKnockback(transform.position);
                }
                health.TakeDamage(damage);
                ApplyEffects(health, aiController);

                if (explosiveRadius > 0f)
                {
                    ApplyExplosiveSplash(exactHitPoint, health);
                    ObjectPoolingManager.SafeReturn(gameObject);
                    return;
                }

                hitCount++;
                if (hitCount > pierceCount)
                {
                    if (isBoomerang && !hasReturned)
                    {
                        projectileMove.StartReturnState();
                    }
                    else
                    {
                        ObjectPoolingManager.SafeReturn(gameObject);
                    }
                }
            }
        }

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
            enemiesHit.Clear();
        }

        public void OnReturnedToPool()
        {
            hitCount = 0;
            hasReturned = false;
            enemiesHit.Clear();
        }

        private void ApplyEffects(EnemyController.Health health, EnemyController.BaseAIController move)
        {
            foreach(var effect in effects)
            {
                if(effect.Key == SO.WeaponEffectType.FireDamage)
                {
                    FireDamage(Mathf.RoundToInt(effect.Value), health).Forget();
                } 
                if(effect.Key == SO.WeaponEffectType.FrozenDuration)
                {
                    FrozenDuration(Mathf.RoundToInt(effect.Value), move, health).Forget();
                }
            }
        }

        private async UniTaskVoid FireDamage(int damage, EnemyController.Health health)
        {
            if (health == null) return;

            // Lấy token hủy từ chính con quái để hủy DOT khi quái bị trả về pool hoặc die
            var cancellationToken = health.GetStatusEffectCancellationToken();

            try
            {
                health.gameObject.GetComponent<EnemyController.EnemyEffects>().ShowFireEffect();
                // Ví dụ vòng lặp đốt sát thương lửa 3 lần, mỗi lần cách nhau 1 giây
                for (int i = 0; i < 3; i++)
                {
                    await UniTask.Delay(System.TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);

                    if (health != null && !cancellationToken.IsCancellationRequested)
                    {
                        health.TakeDamage(damage);
                    }
                    else
                    {
                        break;
                    }
                }
                health.gameObject.GetComponent<EnemyController.EnemyEffects>().HideFireEffect();
            }
            catch (System.OperationCanceledException)
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
