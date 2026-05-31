using Cysharp.Threading.Tasks;
using Core;
using SO;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyController
{
    public abstract class BaseAttack : MonoBehaviour, IPoolable
    {
        [Header("Base Attack Setup")]
        [SerializeField] protected EnemySO enemyData;

        protected bool canAttack = true;
        /// <summary>True while PerformAttack / pattern đang chạy — dùng để route AE_OnAttack khi có nhiều BaseAttack.</summary>
        public bool IsAttackInProgress { get; protected set; }
        protected Transform player;
        protected Animator animator;
        protected BaseEnemyAnimation baseEnemyAnimation;

        protected virtual void Awake()
        {
            // Cache animator nếu có (nằm ở model con)
            animator = GetComponentInChildren<Animator>();
            baseEnemyAnimation = GetComponentInParent<BaseEnemyAnimation>();
        }

        public void SetPlayer(Transform p) => player = p;
        public bool CanAttack() => canAttack;

        // Hàm Reset khi lôi từ Pool ra
        public virtual void OnSpawnedFromPool() => canAttack = true;
        public virtual void OnReturnedToPool() { }

        /// <summary>
        /// Hàm trừu tượng bắt buộc các lớp con (Melee, Ranged, Charge...) phải tự định nghĩa logic vung đòn riêng.
        /// </summary>
        public virtual UniTask PerformAttack(NavMeshAgent agent) 
        {
            return UniTask.CompletedTask;
        }
        public virtual void OnAnimationAttackEvent() { }
    }
}