using Core;
using Global;
using SO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlayerController
{
    public class PlayerAnimation : MonoBehaviour
    {
        private Animator animator;
        private Attack attack;
        private readonly int speedHash = Animator.StringToHash("Speed");
        private readonly int isAttackHash = Animator.StringToHash("Attack");
        private readonly int skillHash = Animator.StringToHash("Skill");
        private readonly int dashHash = Animator.StringToHash("Dash");

        private void Start()
        {
            attack = GetComponent<Attack>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        public void RebindAnimator(Animator newAnimator)
        {
            animator = newAnimator;
        }

        public void SetSpeed(float speed)
        {
            if (animator == null) return;
            animator.SetFloat(speedHash, speed);
        }

        public void SetAttack()
        {
            if (animator == null) return;
            animator.SetTrigger(isAttackHash);
        }

        public void SetSkill()
        {
            if (animator == null) return;
            if (HasParameter(skillHash))
                animator.SetTrigger(skillHash);
        }

        public void SetDash()
        {
            if (animator == null) return;
            if (HasParameter(dashHash))
                animator.SetTrigger(dashHash);
        }

        private bool HasParameter(int hash)
        {
            if (animator == null) return false;

            foreach (var param in animator.parameters)
            {
                if (param.nameHash == hash)
                    return true;
            }

            return false;
        }

        public void AE_Attack()
        {
            attack?.SpawnProjectile().Forget();
        }
    }
}
