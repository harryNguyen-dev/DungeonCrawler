using UnityEngine;

namespace EnemyController
{
    public class BaseReciverAnimationEvent : MonoBehaviour
    {
        private BaseAttack[] attacks;
        private BaseAIController baseAIController;

        private void Start()
        {
            attacks = GetComponentsInParent<BaseAttack>(true);
            baseAIController = GetComponentInParent<BaseAIController>();
        }

        public void AE_OnAttack()
        {
            if (attacks == null || attacks.Length == 0)
                return;

            foreach (var attack in attacks)
            {
                if (attack != null && attack.IsAttackInProgress)
                {
                    attack.OnAnimationAttackEvent();
                    return;
                }
            }

            foreach (var attack in attacks)
            {
                if (attack != null && attack.enabled)
                {
                    attack.OnAnimationAttackEvent();
                    return;
                }
            }
        }

        public void AE_OnSpawnFinish()
        {
            baseAIController?.SpawnFinish();
        }
    }
}
