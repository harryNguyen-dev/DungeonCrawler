using Cysharp.Threading.Tasks;
using UnityEngine;

namespace EnemyController
{
    public class Attack : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private int damage = 30;
        [SerializeField] private float attackCooldown = 1.5f;

        private bool canAttack = true;
        private Transform player;

        public void SetPlayer(Transform p)
        {
            player = p;
        }

        public bool CanAttack() => canAttack;

        public async UniTaskVoid PerformAttack(UnityEngine.AI.NavMeshAgent agent)
        {
            if (!canAttack || player == null) return;

            canAttack = false;

            try
            {
                Vector3 startPos = transform.position;
                Vector3 dirToPlayer = (player.position - transform.position).normalized;
                Vector3 retreatPos = startPos - dirToPlayer * 0.5f;

                float t = 0;
                while (t < 0.2f)
                {
                    if (this == null || player == null) return;
                    t += Time.deltaTime;
                    transform.position = Vector3.Lerp(startPos, retreatPos, t / 0.2f);
                    await UniTask.Yield();
                }

                if (player == null) return;
                Vector3 targetAttackPos = player.position;
                t = 0;
                while (t < 0.1f)
                {
                    if (this == null) return;
                    t += Time.deltaTime;
                    transform.position = Vector3.Lerp(retreatPos, targetAttackPos, t / 0.1f);
                    await UniTask.Yield();
                }

                var playerHealth = Global.GlobalEntities.Instance != null
                    ? Global.GlobalEntities.Instance.PlayerHealth
                    : null;
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }

                t = 0;
                while (t < 0.15f)
                {
                    if (this == null) return;
                    t += Time.deltaTime;
                    transform.position = Vector3.Lerp(targetAttackPos, startPos, t / 0.15f);
                    await UniTask.Yield();
                }

                if (agent != null)
                {
                    agent.nextPosition = transform.position;
                }

                await UniTask.Delay(System.TimeSpan.FromSeconds(attackCooldown));
            }
            finally
            {
                if (this != null)
                {
                    canAttack = true;
                }
            }
        }
    }
}
