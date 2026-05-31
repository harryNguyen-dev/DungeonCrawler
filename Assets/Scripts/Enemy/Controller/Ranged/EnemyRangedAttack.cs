using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyController
{
    public class EnemyRangedAttack : BaseAttack
    {
        [Header("Ranged Attack Setup")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private bool useAnimationEvents = false;

        public override async UniTask PerformAttack(NavMeshAgent agent)
        {
            if (!canAttack || player == null || enemyData == null) return;
            canAttack = false;

            try
            {
                if (useAnimationEvents && baseEnemyAnimation != null)
                {
                    baseEnemyAnimation.SetAttackTrigger(); 

                    // Chờ theo thời gian hồi chiêu cấu hình trong ScriptableObject
                    await UniTask.Delay(System.TimeSpan.FromSeconds(enemyData.AttackCooldown));
                }
                else
                {
                    // --- CHẠY BẰNG CODE CHO PROTOTYPE CUBE ---

                    // 1. Thời gian gồng bắn (ngắm bắn) mất 0.3 giây
                    await UniTask.Delay(300);
                    if (this == null || player == null) return;

                    // 2. Kích hoạt sinh đạn
                    SpawnProjectile();

                    // 3. Chờ hồi chiêu tiếp đợt sau
                    await UniTask.Delay(System.TimeSpan.FromSeconds(enemyData.AttackCooldown));
                }
            }
            finally
            {
                if (this != null) canAttack = true;
            }
        }
        public override void OnAnimationAttackEvent()
        {
            SpawnProjectile();
        }
        public void SpawnProjectile()
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Chưa gán Projectile Prefab cho EnemyRangedAttack!");
                return;
            }

            // Tính toán vị trí xuất phát của đạn
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + transform.forward;

            // Tính toán hướng bay thẳng tới tầm ngực của Player (tránh đạn cắm xuống đất)
            Vector3 targetPosition = player.position + Vector3.up * 1f;
            Vector3 shootDirection = (targetPosition - spawnPos).normalized;

            var poolId = projectilePrefab.GetComponent<Core.PooledObject>().PoolId;
            // Lấy đạn từ Pool thay vì Instantiate trực tiếp để tối ưu hiệu năng
            GameObject bulletObj = null;
            if (Core.ObjectPoolingManager.Instance != null)
            {
                bulletObj = Core.ObjectPoolingManager.Instance.Get(poolId, spawnPos, Quaternion.LookRotation(shootDirection));
            }
            else
            {
                bulletObj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(shootDirection));
            }

            // TRUYỀN CHỈ SỐ: Giao sát thương của Quái cho viên đạn xử lý
            if (bulletObj != null)
            {
                var projectileScript = bulletObj.GetComponent<EnemyProjectile>();
                if (projectileScript != null)
                {
                    projectileScript.Setup(enemyData.Damage);
                }
                else
                {
                    Debug.LogError($"Prefab đạn của [{gameObject.name}] thiếu component EnemyProjectile!");
                }
            }
        }
    }
}