using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyController
{
    public class EnemyChargeAttack : BaseAttack
    {
        [Header("Charge Specs")]
        [SerializeField] private float chargeSpeed = 18f;
        [SerializeField] private float chargeDuration = 0.4f;
        [SerializeField] private float windUpDuration = 0.5f;
        [SerializeField] private float recoveryDuration = 0.3f;
        [SerializeField] private float hitRadius = 1.2f;
        [SerializeField] private Transform center;
        [SerializeField] private bool useAnimationEvents = true;

        private System.Collections.Generic.HashSet<Transform> hitTargets;
        private bool chargeImpactReceived;

        protected override void Awake()
        {
            base.Awake();
            hitTargets = new System.Collections.Generic.HashSet<Transform>();
        }

        public override async UniTask PerformAttack(NavMeshAgent agent)
        {
            if (!canAttack || player == null || enemyData == null) return;
            canAttack = false;
            hitTargets.Clear();
            chargeImpactReceived = false;
            IsAttackInProgress = true;

            try
            {
                var healthComp = GetComponent<Health>();
                if (healthComp != null)
                    healthComp.HitFlash?.Play(Color.yellow, windUpDuration).Forget();

                if (useAnimationEvents && baseEnemyAnimation != null)
                {
                    baseEnemyAnimation.PlayChargeAttack();

                    float waited = 0f;
                    float maxWait = windUpDuration + 1.5f;
                    while (!chargeImpactReceived && waited < maxWait)
                    {
                        if (this == null) return;
                        waited += Time.deltaTime;
                        await UniTask.Yield();
                    }
                }
                else
                {
                    await UniTask.Delay(System.TimeSpan.FromSeconds(windUpDuration));
                    if (this == null) return;
                }

                if (!chargeImpactReceived && !useAnimationEvents)
                    CheckHitbox();

                Vector3 chargeDirection = transform.forward;
                float timer = 0f;

                while (timer < chargeDuration)
                {
                    if (this == null) return;
                    timer += Time.deltaTime;

                    transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
                    CheckHitbox();

                    await UniTask.Yield();
                }

                baseEnemyAnimation?.ResetAttackTrigger();
                await UniTask.Delay(System.TimeSpan.FromSeconds(recoveryDuration));
                await UniTask.Delay(System.TimeSpan.FromSeconds(enemyData.AttackCooldown));
            }
            finally
            {
                IsAttackInProgress = false;
                if (this != null) canAttack = true;
            }
        }

        public override void OnAnimationAttackEvent()
        {
            chargeImpactReceived = true;
            CheckHitbox();
        }

        private void CheckHitbox()
        {
            if (center == null) return;

            Collider[] hitColliders = Physics.OverlapSphere(center.position, hitRadius);
            foreach (var col in hitColliders)
            {
                if (!col.CompareTag("Player")) continue;

                if (!hitTargets.Contains(col.transform))
                {
                    hitTargets.Add(col.transform);

                    var playerHealth = Global.GlobalEntities.Instance?.PlayerHealth;
                    if (playerHealth != null)
                        playerHealth.TakeDamage(enemyData.Damage);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (center == null) return;
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(center.position, hitRadius);
        }
    }
}
