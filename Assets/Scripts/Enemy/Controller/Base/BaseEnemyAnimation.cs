using UnityEngine;

namespace EnemyController
{
    public class BaseEnemyAnimation : MonoBehaviour
    {
        private Animator animator;
        
        // BASE ANIMATION HASH
        private int speedHash = Animator.StringToHash("Speed");
        private static readonly int AttackTrigger1Hash = Animator.StringToHash("AttackTrigger1");
        private static readonly int AttackTrigger2Hash = Animator.StringToHash("AttackTrigger2");
        private int spawnTriggerHash = Animator.StringToHash("SpawnTrigger");

        /// <summary>0 = AttackTrigger1, 1 = AttackTrigger2 — luân phiên 1 → 2 → 1 …</summary>
        private int nextAttackComboIndex;
        private int HitTriggerHash = Animator.StringToHash("HitTrigger");
        private int DieTriggerHash = Animator.StringToHash("DieTrigger");

        private static readonly int SpawnStateHash = Animator.StringToHash("Spawn");
        private static readonly int ChargeAttackStateHash = Animator.StringToHash("Roll Attack In Place Start");
        private static readonly int ProjectileAttackStateHash = Animator.StringToHash("Projectile Attack");
        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        public void SetSpeed(float speed)
        {
            animator.SetFloat(speedHash, speed);
        }
        public void ResetAttackCombo()
        {
            nextAttackComboIndex = 0;
            ResetAttackTrigger();
        }

        public void ResetAttackTrigger()
        {
            if (animator == null) return;
            animator.ResetTrigger(AttackTrigger1Hash);
            animator.ResetTrigger(AttackTrigger2Hash);
        }

        public void SetAttackTrigger()
        {
            if (animator == null) return;

            if (nextAttackComboIndex == 0)
                animator.SetTrigger(AttackTrigger1Hash);
            else
                animator.SetTrigger(AttackTrigger2Hash);

            nextAttackComboIndex = 1 - nextAttackComboIndex;
        }
        public void SetSpawnTrigger()
        {
            if (animator == null) return;
            animator.CrossFade(SpawnStateHash, 0.1f, 0);
        }

        public void PlayChargeAttack()
        {
            if (animator == null) return;
            animator.CrossFade(ChargeAttackStateHash, 0.1f, 0);
        }

        public void PlayProjectileAttack()
        {
            if (animator == null) return;
            animator.CrossFade(ProjectileAttackStateHash, 0.1f, 0);
        }
        public void SetHitTrigger()
        {
            animator.SetTrigger(HitTriggerHash);
        }
        public void SetDieTrigger()
        {
            animator.SetTrigger(DieTriggerHash);
        }
    }
}
