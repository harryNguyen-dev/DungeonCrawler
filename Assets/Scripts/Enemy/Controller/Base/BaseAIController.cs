using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using SO;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyController
{
    public abstract class BaseAIController : MonoBehaviour, IPoolable
    {
        [SerializeField] protected EnemySO enemyData;

        public EnemySO EnemyData => enemyData;
        public bool IsBossEnemy => enemyData != null && enemyData.isBoss;

        protected NavMeshAgent agent;
        protected Transform player;
        protected Rigidbody rb;
        protected CombatFeel.KnockbackAgent knockbackAgent;
        private CancellationTokenSource aiCancellation;
        private bool hasInitialized;
        private bool hasInitializedComponents = false;

        protected virtual void Awake()
        {
            EnsureComponentsInitialized();
        }

        protected virtual void Start()
        {
            // Instantiate thủ công (RoomController) không qua pool → cần khởi động AI tại đây
            if (aiCancellation == null)
                OnSpawnedFromPool();
        }

        private void EnsureComponentsInitialized()
        {
            if (hasInitializedComponents) return;

            agent = GetComponent<NavMeshAgent>();
            rb = GetComponent<Rigidbody>();
            
            // Khởi tạo knockbackAgent 1 lần duy nhất
            if (rb != null && agent != null)
            {
                knockbackAgent = new CombatFeel.KnockbackAgent(rb, agent, transform);
            }

            hasInitializedComponents = true;
        }
        private void ApplyData()
        {
            if (enemyData == null) return;
            agent.speed = enemyData.MoveSpeed;
            agent.stoppingDistance = enemyData.AttackRange;
        }

        private void InitializePlayer()
        {
            var playerObj = Global.GlobalEntities.Instance?.PlayerInstance;
            if (playerObj != null) player = playerObj.transform;
            OnPlayerInitialized();
        }

        protected virtual void OnPlayerInitialized() { }

        private void StartAILoop()
        {
            StopAILoop();
            aiCancellation = new CancellationTokenSource();
            AILoop(aiCancellation.Token).Forget();
        }
        private void StopAILoop()
        {
            if (aiCancellation != null)
            {
                if (!aiCancellation.IsCancellationRequested)
                {
                    aiCancellation.Cancel();
                }
                aiCancellation.Dispose();
                aiCancellation = null;
            }
        }

        private async UniTaskVoid AILoop(CancellationToken token)
        {
            while (this != null && gameObject != null && !token.IsCancellationRequested)
            {
                if ((knockbackAgent != null && knockbackAgent.IsActive) || agent == null || !agent.enabled)
                {
                    await UniTask.Delay(100, cancellationToken: token);
                    continue;
                }

                if (player != null)
                {
                    ExecuteBehaviour();
                }

                await UniTask.Delay(100, cancellationToken: token);
            }
        }

        // Các lớp con (Melee, Ranged, Boss) sẽ tự override hàm này để viết code AI riêng
        protected abstract void ExecuteBehaviour();

        public virtual void OnSpawnedFromPool()
        {
            // Đảm bảo components đã được cache (phòng trường hợp OnSpawned gọi trước cả Awake/Start tùy ObjectPoolingManager)
            EnsureComponentsInitialized(); 

            // Cập nhật lại chỉ số mới từ ScriptableObject phòng khi bạn đổi chỉ số lúc chạy game
            ApplyData();

            // Tìm lại Player mới nhất (Tránh việc Player cũ bị hủy hoặc đổi màn chơi)
            InitializePlayer();

            // Chạy lại vòng lặp AI cho vòng đời mới này
            StartAILoop();
        }

        public virtual void OnReturnedToPool()
        {
            aiCancellation?.Cancel();
            aiCancellation?.Dispose();
            aiCancellation = null;
        }
        public virtual void TakeKnockback(Vector3 knockbackSourcePosition)
        {
            if (knockbackAgent == null || enemyData == null) return;

            // Tính toán hướng đẩy lùi (Từ nguồn sát thương bắn ra xa quái)
            Vector3 knockbackDirection = (transform.position - knockbackSourcePosition).normalized;
            knockbackDirection.y = 0; // Giữ quái trên mặt phẳng, tránh bay lên trời

            // Kích hoạt lực đẩy lùi dựa trên chỉ số cấu hình sẵn trong EnemySO
            // Trong vòng lặp AILoop, đoạn check 'knockbackAgent.IsActive' sẽ tự động đóng băng AI khi đang bị đẩy
            knockbackAgent.Play(knockbackDirection, enemyData.KnockbackForce, enemyData.KnockbackDuration).Forget();
        }
        public virtual void UpdateAgentSpeed(float newSpeed)
        {
            if (agent == null) return;
            agent.speed = newSpeed;
        }

        public virtual void ReturnMoveSpeed()
        {
            if (agent == null || enemyData == null) return;
            agent.speed = enemyData.MoveSpeed;
        }
    }

}