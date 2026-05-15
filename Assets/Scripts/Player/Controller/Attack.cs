using UnityEngine;

namespace PlayerController
{

    public class Attack : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint; // Điểm xuất hiện sóng kiếm (gắn ở tay/kiếm)
    
        private PlayerStats playerStats;
        private float attackCooldown;
        private float lastAttackTime;

        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            attackCooldown = playerStats.GetAttackCooldown();
            playerStats.OnAttackSpeedChanged += (attackSpeed) => attackCooldown = attackSpeed;
        }

        private void Update()
        {
            // Nếu người chơi đang nhấn giữ nút tấn công
            if (InputManager.Instance.IsAttacking())
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    PerformAttack();
                    lastAttackTime = Time.time;
                }
            }
        }
        private void PerformAttack()
        {
            Debug.Log("[PlayerController] Perform Attack");
            SpawnProjectile();
        }
        public void SpawnProjectile()
        {
            if (projectilePrefab != null && firePoint != null)
            {
                var projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                var projectileMove = projectile.GetComponent<Projectile.ProjectileMove>();
                if (projectileMove != null)
                {
                    projectileMove.SetDamage(playerStats.GetAttackDamage());
                }
            }
        }
    }

}