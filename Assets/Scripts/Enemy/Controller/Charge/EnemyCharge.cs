using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EnemyController
{
    public class EnemyCharge : BaseAIController
    {
        private BaseAttack attackComponent;
        private bool isCharging = false;

        protected override void Awake()
        {
            base.Awake();
            attackComponent = GetComponent<BaseAttack>();
        }

        public override void OnSpawnedFromPool()
        {
            base.OnSpawnedFromPool();
            isCharging = false;
            if (attackComponent != null && player != null)
            {
                attackComponent.SetPlayer(player);
            }
        }

        public override void OnReturnedToPool()
        {
            base.OnReturnedToPool();
            isCharging = false;
        }

        protected override void OnPlayerInitialized()
        {
            if (attackComponent != null)
            {
                attackComponent.SetPlayer(player);
            }
        }

        protected override void ExecuteBehaviour()
        {
            // Nếu đang trong trạng thái lao húc, đóng băng toàn bộ di chuyển mặc định của NavMesh
            if (isCharging) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // 1. Nếu lọt vào tầm kích hoạt lao húc
            if (distanceToPlayer <= enemyData.AttackRange)
            {
                if (attackComponent != null && attackComponent.CanAttack())
                {
                    TriggerChargeAttack().Forget();
                }
                else
                {
                    // Trong thời gian chờ Cooldown chiêu húc, đứng im nhìn về phía Player
                    if (agent.enabled) agent.isStopped = true;
                    LookAtPlayer();
                }
            }
            // 2. Nếu ở quá xa -> Bật NavMesh để chạy bộ đuổi theo Player
            else
            {
                if (agent.enabled)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }
            }
        }

        private async UniTaskVoid TriggerChargeAttack()
        {
            isCharging = true;

            // TẮT NavMeshAgent trước khi húc để tránh việc quái bị kéo ghì giữ lại bởi tính toán đường đi của Unity
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            // Xoay mặt nhìn thẳng vào Player để chốt hướng húc cố định
            LookAtPlayer();
            // Kích hoạt đòn lao húc từ Attack Component
            await attackComponent.PerformAttack(agent);

            // Sau khi lao húc xong (bao gồm cả khựng phục hồi) -> Bật lại NavMeshAgent
            if (this != null && gameObject != null)
            {
                if (agent != null)
                {
                    agent.enabled = true;
                    agent.nextPosition = transform.position; // Đồng bộ vị trí thực tế sau khi lao vào NavMesh
                }
                isCharging = false;
            }
        }

        private void LookAtPlayer()
        {
            if (player == null) return;
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Tránh quái bị nghiêng ngửa chúc đầu xuống đất
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}