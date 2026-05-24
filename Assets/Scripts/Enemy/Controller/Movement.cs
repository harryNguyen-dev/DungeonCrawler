using Cysharp.Threading.Tasks;
using CombatFeel;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyController
{
    public class Movement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField]
        private float attackRange = 2f;

        [SerializeField]
        private float moveSpeed = 3.5f;

        [Header("Knockback Settings")]
        [SerializeField]
        private float knockbackForce = 6f;

        [SerializeField]
        private float knockbackDuration = 0.12f;

        private NavMeshAgent agent;
        private Transform player;
        private Attack attackComponent;
        private Rigidbody rb;
        private KnockbackAgent knockbackAgent;
        private CancellationTokenSource updateAICancellation;
        private bool hasInitialized;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            knockbackAgent = new KnockbackAgent(rb, agent, transform);

            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;

            attackComponent = GetComponent<Attack>();
            hasInitialized = true;
            InitializePlayerReference();
            StartAIBehaviour();
        }

        private void OnEnable()
        {
            if (hasInitialized)
            {
                InitializePlayerReference();
                StartAIBehaviour();
            }
        }

        private void OnDisable()
        {
            updateAICancellation?.Cancel();
            updateAICancellation?.Dispose();
            updateAICancellation = null;
        }

        private void InitializePlayerReference()
        {
            var playerObj = Global.GlobalEntities.Instance?.PlayerInstance;
            if (playerObj != null)
            {
                player = playerObj.transform;
            }

            attackComponent?.SetPlayer(player);
        }

        private void StartAIBehaviour()
        {
            if (updateAICancellation != null) return;
            updateAICancellation = new CancellationTokenSource();
            UpdateAIBehaviour(updateAICancellation.Token).Forget();
        }

        public void UpdateAgentSpeed(float speed)
        {
            if (agent != null)
            {
                agent.speed = speed;
            }
        }

        public void ReturnMoveSpeed()
        {
            if (agent != null)
            {
                agent.speed = moveSpeed;
            }
        }

        public UniTaskVoid TakeKnockback(Vector3 direction, float force = -1f, float recoveryTime = -1f)
        {
            if (knockbackAgent == null)
            {
                return default;
            }

            float appliedForce = force > 0f ? force : knockbackForce;
            float appliedDuration = recoveryTime > 0f ? recoveryTime : knockbackDuration;
            knockbackAgent.Play(direction, appliedForce, appliedDuration, player).Forget();
            return default;
        }

        private async UniTaskVoid UpdateAIBehaviour(CancellationToken cancellationToken)
        {
            while (this != null && gameObject != null && !cancellationToken.IsCancellationRequested)
            {
                if ((knockbackAgent != null && knockbackAgent.IsActive) || agent == null || !agent.enabled)
                {
                    await UniTask.Delay(100, cancellationToken: cancellationToken);
                    continue;
                }

                if (player != null)
                {
                    float distance = Vector3.Distance(transform.position, player.position);

                    if (distance <= attackRange)
                    {
                        agent.isStopped = true;
                        if (attackComponent != null && attackComponent.CanAttack())
                        {
                            attackComponent.PerformAttack(agent).Forget();
                        }
                    }
                    else
                    {
                        agent.isStopped = false;
                        agent.SetDestination(player.position);
                    }
                }

                await UniTask.Delay(100, cancellationToken: cancellationToken);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
