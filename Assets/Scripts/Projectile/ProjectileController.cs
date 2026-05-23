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
                var hitEffect = Global.GlobalEntities.Instance.playerHitEffect;

                Vector3 bulletPos = transform.position;
                Vector3 exactHitPoint = other.ClosestPoint(bulletPos);

                hitEffect.PlayHitEffect(exactHitPoint, Quaternion.LookRotation(transform.forward));


                move?.TakeKnockback(transform.forward);
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
            if (health == null) return;

            // Lấy token hủy từ chính con quái
            var cancellationToken = health.GetCancellationTokenOnDestroy();

            try
            {
                health.gameObject.GetComponent<EnemyController.EnemyEffects>().ShowFireEffect();
                // Ví dụ vòng lặp đốt sát thương lửa 3 lần, mỗi lần cách nhau 1 giây
                for (int i = 0; i < 3; i++)
                {
                    // Chờ 1 giây, nếu trong 1 giây này quái bị Destroy, UniTask sẽ ném ra ngoại lệ tự hủy và dừng hàm tại đây luôn
                    await UniTask.Delay(System.TimeSpan.FromSeconds(1f), cancellationToken: cancellationToken);

                    // Kiểm tra an toàn trước khi trừ máu
                    if (health != null)
                    {
                        health.TakeDamage(damage);
                    }
                }
                health.gameObject.GetComponent<EnemyController.EnemyEffects>().HideFireEffect();
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("[FireDamage] Tác vụ đốt lửa đã tự động hủy vì quái die trước.");
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
