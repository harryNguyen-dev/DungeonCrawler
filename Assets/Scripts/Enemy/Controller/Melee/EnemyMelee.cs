using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EnemyController
{
    public class EnemyMelee : BaseAIController
    {
        private BaseAttack attackComponent;
        private bool isAttacking = false;

        protected override void Awake()
        {
            base.Awake();
            attackComponent = GetComponent<BaseAttack>();
        }

        public override void OnSpawnedFromPool()
        {
            base.OnSpawnedFromPool();
            isAttacking = false;
            
            if (attackComponent != null && player != null)
            {
                attackComponent.SetPlayer(player);
            }
        }

        public override void OnReturnedToPool()
        {
            base.OnReturnedToPool();
            isAttacking = false;
        }

        protected override void OnPlayerInitialized()
        {
            // Đảm bảo truyền Player reference vào component Attack ngay khi tìm thấy Player
            if (attackComponent != null)
            {
                attackComponent.SetPlayer(player);
            }
        }

        protected override void ExecuteBehaviour()
        {
            // Nếu đang trong quá trình thực hiện đòn đánh (UniTask của Attack đang chạy), đóng băng AI loop
            if (isAttacking) return;
            
            // Tính khoảng cách đến Player trên không gian 3D
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Trường hợp 1: Đã áp sát trong tầm đánh (Attack Range lấy từ EnemySO)
            if (distanceToPlayer <= enemyData.AttackRange)
            {
                if (attackComponent != null && attackComponent.CanAttack())
                {
                    TriggerMeleeAttack().Forget();
                }
                else
                {
                    // Nếu đang chờ Cooldown của đòn đánh, đứng yên nhìn về phía Player
                    LookAtPlayer();
                    if (agent.enabled) agent.isStopped = true;
                }
            }
            // Trường hợp 2: Ở xa -> Bật NavMesh và Chase (Lao thẳng tới Player)
            else
            {
                if (agent.enabled)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }
            }
        }

        private async UniTaskVoid TriggerMeleeAttack()
        {
            isAttacking = true;

            // Tắt NavMeshAgent tạm thời để tránh xung đột vị trí với Logic Lerp/Move trong Attack.cs
            if (agent.enabled)
            {
                agent.isStopped = true;
                agent.enabled = false; 
            }

            // Luôn quay mặt về phía Player trước khi vung đòn
            LookAtPlayer();

            // Kích hoạt chuỗi hành vi tấn công (Telegraph -> Impact -> Recovery) từ Attack.cs
            // Hàm PerformAttack của bạn yêu cầu truyền NavMeshAgent vào (để update agent.nextPosition cuối chuỗi)
            // Vì ta đã tạm tắt agent.enabled, ta vẫn truyền component agent vào bình thường.
            await attackComponent.PerformAttack(agent);

            // Sau khi thực hiện xong đòn đánh (bao gồm cả khoảng hoãn/recovery trong Attack)
            // Bật lại NavMeshAgent để tiếp tục vòng lặp đuổi theo ở Frame tiếp theo
            if (this != null && gameObject != null)
            {
                if (agent != null)
                {
                    agent.enabled = true;
                    // Đồng bộ lại vị trí thực tế của Object vào NavMesh để không bị giật lùi (Snap) vị trí
                    agent.nextPosition = transform.position; 
                }
                isAttacking = false;
            }
        }

        private void LookAtPlayer()
        {
            if (player == null) return;
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Giữ quái không bị nghiêng trục X/Z nếu Player nhảy cao lên
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Vẽ tầm đánh trên Editor để dễ tinh chỉnh chỉ số trong ScriptableObject
            if (enemyData != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, enemyData.AttackRange);
            }
        }
    }
}