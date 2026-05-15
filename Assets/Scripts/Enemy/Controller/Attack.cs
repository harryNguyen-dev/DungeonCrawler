using UnityEngine;
using Cysharp.Threading.Tasks;
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
           canAttack = false;
    
            // Lưu lại các vị trí cần thiết
            Vector3 startPos = transform.position;
            // Hướng từ Quái đến Player
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            
            // 1. LÙI LẠI (Anticipation)
            float backwardDist = 0.5f; 
            Vector3 retreatPos = startPos - dirToPlayer * backwardDist;
            
            float t = 0;
            while (t < 0.2f) // Lùi lại trong 0.2s
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, retreatPos, t / 0.2f);
                await UniTask.Yield();
            }

            // 2. LAO TỚI (The Strike)
            // Lấy vị trí Player mới nhất (phòng trường hợp Player đã di chuyển)
            Vector3 targetAttackPos = player.position;
            t = 0;
            while (t < 0.1f) // Lao tới cực nhanh trong 0.1s
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(retreatPos, targetAttackPos, t / 0.1f);
                await UniTask.Yield();
            }

            Global.GlobalEntities.Instance.PlayerHealth.TakeDamage(damage);

            // 3. VỀ LẠI VỊ TRÍ CŨ (Recovery)
            t = 0;
            while (t < 0.15f) // Thu người về vị trí ban đầu trong 0.15s
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(targetAttackPos, startPos, t / 0.15f);
                await UniTask.Yield();
            }

            // Đảm bảo NavMeshAgent hoạt động lại sau khi di chuyển bằng transform
            if(agent != null) agent.nextPosition = transform.position;

            // Hồi chiêu
            await UniTask.Delay(System.TimeSpan.FromSeconds(attackCooldown));
            canAttack = true;
        }
    }

}