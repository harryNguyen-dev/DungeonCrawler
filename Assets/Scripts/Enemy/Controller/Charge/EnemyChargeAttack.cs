using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyController
{
    public class EnemyChargeAttack : BaseAttack
    {
        [Header("Charge Specs")]
        [SerializeField] private float chargeSpeed = 18f;      // Tốc độ lao cực nhanh
        [SerializeField] private float chargeDuration = 0.4f;   // Thời gian lao (0.4s ứng với tầm ~7 mét)
        [SerializeField] private float windUpDuration = 0.5f;   // Thời gian đứng im gồng báo trước
        [SerializeField] private float recoveryDuration = 0.3f; // Thời gian đứng im khựng sau khi húc xong
        [SerializeField] private float hitRadius = 1.2f;        // Bán kính hộp hitbox quét sát thương dọc đường lao
        [SerializeField] private Transform center;

        private System.Collections.Generic.HashSet<Transform> hitTargets;

        protected override void Awake()
        {
            base.Awake();
            hitTargets = new System.Collections.Generic.HashSet<Transform>();
        }

        public override async UniTask PerformAttack(NavMeshAgent agent)
        {
            if (!canAttack || player == null || enemyData == null) return;
            canAttack = false;
            hitTargets.Clear(); // Xóa danh sách mục tiêu đã trúng đòn của lượt trước

            try
            {
                // GIAI ĐOẠN 1: Đứng im gồng chiêu (Telegraph / Wind-up)
                // Bạn có thể kích hoạt Flash đỏ của quái ở đây để tăng hiệu ứng Game Feel cảnh báo player
                var healthComp = GetComponent<Health>();
                if (healthComp != null) healthComp.HitFlash?.Play(Color.yellow, windUpDuration).Forget();
                
                await UniTask.Delay(System.TimeSpan.FromSeconds(windUpDuration));
                if (this == null) return;

                // GIAI ĐOẠN 2: Lao thẳng (Khai hỏa đòn húc)
                // Lấy hướng cố định ngay tại thời điểm bắt đầu lao (Player chạy né ra hướng đó vẫn húc thẳng tiếp)
                Vector3 chargeDirection = transform.forward; 
                float timer = 0f;

                while (timer < chargeDuration)
                {
                    if (this == null) return;
                    timer += Time.deltaTime;

                    // Di chuyển tịnh tiến quái bằng Code tự thân với vận tốc lớn
                    transform.position += chargeDirection * chargeSpeed * Time.deltaTime;

                    // Quét hitbox hình cầu dọc đường đi xem có trúng Player không
                    CheckHitbox();

                    await UniTask.Yield();
                }

                // GIAI ĐOẠN 3: Khựng lại sau khi húc (Recovery / Stun tự thân)
                await UniTask.Delay(System.TimeSpan.FromSeconds(recoveryDuration));
                
                // Chờ nạp lại chiêu (Cooldown tổng)
                await UniTask.Delay(System.TimeSpan.FromSeconds(enemyData.AttackCooldown));
            }
            finally
            {
                if (this != null) canAttack = true;
            }
        }

        private void CheckHitbox()
        {
            // Quét tất cả các Collider trong bán kính hitRadius quanh tâm con quái
            Collider[] hitColliders = Physics.OverlapSphere(center.position, hitRadius);
            foreach (var col in hitColliders)
            {
                if (col.CompareTag("Player"))
                {
                    // Tránh việc 1 lượt lao húc gây damage liên tục nhiều lần trên 1 Player (Gây chết tức tưởi)
                    if (!hitTargets.Contains(col.transform))
                    {
                        hitTargets.Add(col.transform);
                        
                        var playerHealth = Global.GlobalEntities.Instance?.PlayerHealth;
                        if (playerHealth != null)
                        {
                            playerHealth.TakeDamage(enemyData.Damage);
                            
                            // Bạn có thể gọi thêm hàm đẩy lùi Player (nếu Player có nhận lực knockback) tại đây!
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Vẽ hitbox quét sát thương trên Editor để bạn dễ căn chỉnh độ to của con quái húc
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(center.position, hitRadius);
        }
    }
}