using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyController
{
    public class EnemyMeleeAttack : BaseAttack
    {
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
                    // Chờ theo thời gian hồi gốc của cấu hình SO
                    await UniTask.Delay(System.TimeSpan.FromSeconds(enemyData.AttackCooldown));
                }
                else
                {
                    // Prototype bằng Cube: Khựng lại vung tay mất 0.2s rồi gây damage
                    await UniTask.Delay(200);
                    if (this == null || player == null) return;

                    ApplyMeleeDamage();

                    // Chờ hồi chiêu tiếp
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
            ApplyMeleeDamage();
        }
        private void ApplyMeleeDamage()
        {
            Debug.Log("[EnemyMeleeAttack] Apply Melee Damage");
            float distance = Vector3.Distance(transform.position, player.position);
            // Check xem player có lướt ra ngoài tầm đánh (AttackRange trong SO) chưa
            if (distance <= enemyData.AttackRange + 0.5f) 
            {
                Global.GlobalEntities.Instance?.PlayerHealth?.TakeDamage(GetEffectiveDamage(), GetComponent<Health>());
            }
        }
    }
}