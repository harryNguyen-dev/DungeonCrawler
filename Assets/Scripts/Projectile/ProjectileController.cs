using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Projectile
{
    public class ProjectileController : MonoBehaviour
    {
        private int damage;
        private int pierceCount;
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

                var move = other.GetComponent<EnemyController.Movement>();
                health.TakeDamage(damage);
                ApplyEffects(health, move);
                hitCount++;
                if (hitCount > pierceCount)
                {
                    if (isBoomerang && !hasReturned)
                    {
                        // Nếu là đạn boomerang và ĐANG BAY ĐI mà hết lượt xuyên -> Ép quay đầu ngay lập tức
                        projectileMove.StartReturnState();
                    }
                    else
                    {
                        // Đạn thường HOẶC đạn boomerang đang trên đường bay về mà hết lượt xuyên -> Hủy luôn
                        Destroy(gameObject);
                    }
                }
            }
        }

        private void ApplyEffects(EnemyController.Health health, EnemyController.Movement move)
        {
            foreach(var effect in effects)
            {
                if(effect.Key == SO.WeaponEffectType.FireDamage)
                {
                    FireDamage(Mathf.RoundToInt(effect.Value), health).Forget();
                } 
                if(effect.Key == SO.WeaponEffectType.FrozenDuration)
                {
                    FrozenDuration(Mathf.RoundToInt(effect.Value), move).Forget();
                }
            }
        }

        private async UniTaskVoid FireDamage(int damage, EnemyController.Health health)
        {
            // 3 seconds
            for (int i = 0; i < 3; i++)
            {
                await UniTask.Delay(1000);
                health?.TakeDamage(damage);
            }
        }

        private async UniTaskVoid FrozenDuration(float duration, EnemyController.Movement move)
        {
            move.UpdateAgentSpeed(0.0f);
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            move.ReturnMoveSpeed();
        }
    }
}