using UnityEngine;

namespace EnemyController
{
    public class BaseReciverAnimationEvent : MonoBehaviour
    {
        private BaseAttack attacks;
        private BaseAIController baseAIController;

        private void Start()
        {
            attacks = GetComponentInParent<BaseAttack>();
            baseAIController = GetComponentInParent<BaseAIController>();
        }

        public void AE_OnAttack()
        {
            attacks?.OnAnimationAttackEvent();
        }

        public void AE_OnSpawnFinish()
        {
            baseAIController?.SpawnFinish();
        }
    }
}
