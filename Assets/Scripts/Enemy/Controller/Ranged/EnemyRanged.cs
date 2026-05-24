using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EnemyController
{
    public class EnemyRanged : BaseAIController
    {
        private BaseAttack attackComponent;
        private bool isAttacking = false;

        [Header("Ranged AI Settings")]
        [Tooltip("Khoảng cách tối thiểu quái muốn duy trì. Nếu Player đến quá gần mức này, quái sẽ lùi lại.")]
        [SerializeField] private float safeDistanceRatio = 0.6f; 
        private float nextRetreatCheckTime = 0f;

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
            if (attackComponent != null)
            {
                attackComponent.SetPlayer(player);
            }
        }

        protected override void ExecuteBehaviour()
        {
            // Nếu đang trong quá trình thực hiện đòn bắn, đứng yên thực hiện chuỗi hoạt cảnh
            if (isAttacking) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            float maxAttackRange = enemyData.AttackRange;
            float minSafeDistance = maxAttackRange * safeDistanceRatio;

            // TRƯỜNG HỢP 1: Player quá gần -> Lùi lại (Kiting)
            if (distanceToPlayer < minSafeDistance)
            {
                if (agent.enabled)
                {
                    // Chỉ cho phép cập nhật Destination lùi sau mỗi 0.5 giây để tránh giật lag đường đi
                    if (Time.time >= nextRetreatCheckTime)
                    {
                        nextRetreatCheckTime = Time.time + 0.5f;

                        agent.isStopped = false;

                        // 1. Tính toán hướng chạy trốn (ngược hướng với Player)
                        Vector3 retreatDirection = (transform.position - player.position).normalized;
                        Vector3 rawRetreatPos = transform.position + retreatDirection * 4f; // Thử lùi lại 4 mét

                        // 2. Ép điểm lùi phải nằm TRÊN NavMesh (Tránh đâm đầu vào tường/vực)
                        if (UnityEngine.AI.NavMesh.SamplePosition(rawRetreatPos, out UnityEngine.AI.NavMeshHit hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            Vector3 targetRetreatPos = hit.position;
                            agent.SetDestination(targetRetreatPos);
                            Vector3 velocityDir = (hit.position - transform.position).normalized;
                            agent.velocity = velocityDir * enemyData.MoveSpeed * 1.5f;
                        }
                        else
                        {
                            agent.isStopped = true;
                        }
                    }
                }
                SoftLookAtPlayer();
            }
            // TRƯỜNG HỢP 2: Đủ tầm bắn an toàn -> Đứng yên và bắn!
            else if (distanceToPlayer <= maxAttackRange)
            {
                if (agent.enabled) agent.isStopped = true; // Đứng lại để ngắm bắn
                LookAtPlayer();

                if (attackComponent != null && attackComponent.CanAttack())
                {
                    TriggerRangedAttack().Forget();
                }
            }
            // TRƯỜNG HỢP 3: Quá xa tầm đánh -> Đuổi theo Player
            else
            {
                if (agent.enabled)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.position);
                }
            }
        }
        private void SoftLookAtPlayer()
        {
            if (player == null) return;
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                // Xoay từ từ với tốc độ vừa phải, cho phép Agent có góc mở để di chuyển
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        private async UniTaskVoid TriggerRangedAttack()
        {
            isAttacking = true;

            // Await đòn bắn kết thúc (bao gồm cả thời gian nạp đạn/cooldown)
            await attackComponent.PerformAttack(agent);

            if (this != null && gameObject != null)
            {
                isAttacking = false;
            }
        }

        private void LookAtPlayer()
        {
            if (player == null) return;
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Giữ quái thăng bằng trục Y
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (enemyData != null)
            {
                // Vẽ tầm bắn tối đa (Màu đỏ)
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, enemyData.AttackRange);

                // Vẽ vùng nguy hiểm cần giật lùi (Màu vàng)
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, enemyData.AttackRange * safeDistanceRatio);
            }
        }
    }
}