using UnityEngine;
using Cysharp.Threading.Tasks;
using EnemyController;

namespace EnemyController
{
    public class Movement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField]
        private float attackRange = 2f;

        [SerializeField]
        private float moveSpeed = 3.5f;

        private UnityEngine.AI.NavMeshAgent agent;
        private Transform player;
        private EnemyController.Attack attackComponent;
        private void Start()
        {
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            attackComponent = GetComponent<EnemyController.Attack>();
            attackComponent.SetPlayer(player);
            UpdateAIBehaviour().Forget();
        }
        public void UpdateAgentSpeed(float speed)
        {
            agent.speed = speed;
        }
        public void ReturnMoveSpeed()
        {
            agent.speed = moveSpeed;
        }
        private async UniTaskVoid UpdateAIBehaviour()
        {
            while (this != null && gameObject != null)
            {
                if (player != null)
                {
                    float distance = Vector3.Distance(transform.position, player.position);

                    if (distance <= attackRange)
                    {
                        // Dừng lại và tấn công
                        agent.isStopped = true;
                        if (attackComponent != null)
                        {
                            if (attackComponent.CanAttack())
                            {
                                attackComponent.PerformAttack(agent).Forget();
                            }
                        }
                    }
                    else
                    {
                        // Đuổi theo
                        agent.isStopped = false;
                        agent.SetDestination(player.position);
                    }
                }

                // Delay nhẹ để tránh tốn tài nguyên (AI không cần check mỗi frame)
                await UniTask.Delay(100);
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
