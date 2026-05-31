using UnityEngine;
using Cysharp.Threading.Tasks;

namespace PlayerController
{
    public class PlayerAnimation : MonoBehaviour
    {
        private Animator animator;
        private Attack attack;
        private int speedHash = Animator.StringToHash("Speed");
        private int isAttackHash = Animator.StringToHash("Attack");
        private void Start()
        {
            animator = GetComponentInChildren<Animator>();
            attack = GetComponent<Attack>();
        }

        public void SetSpeed(float speed) 
        {
            animator.SetFloat(speedHash, speed);
        }
        public void SetAttack() 
        {
            animator.SetTrigger(isAttackHash);
        }



        public void AE_Attack() 
        {
            attack.SpawnProjectile().Forget();
        }
    }
}
