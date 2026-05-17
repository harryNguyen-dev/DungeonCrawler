using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace PlayerController
{

    public class Attack : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint; // Điểm xuất hiện sóng kiếm (gắn ở tay/kiếm)

        private PlayerStats playerStats;
        private float attackCooldown;
        private int numberOfProjectiles = 1;
        private float lastAttackTime;

        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            attackCooldown = playerStats.GetAttackCooldown();
            if (playerStats.runtimeStats.TryGetEffect(SO.WeaponEffectType.NumberOfProjectiles, out var value))
            {
                numberOfProjectiles = Mathf.RoundToInt(value);
            }
            playerStats.OnAttackSpeedChanged += OnAttackChanged;
            playerStats.OnNumberOfProjectileChanged += OnNumberOfProjectileChanged;

        }
        private void OnAttackChanged(float attackSpeed) => attackCooldown = attackSpeed;
        private void OnNumberOfProjectileChanged(int num) {Debug.Log($"[PlayerController] OnNumberOfProjectileChanged {num}"); numberOfProjectiles = Mathf.RoundToInt(num);}

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
            SpawnProjectile().Forget();
        }
        public async UniTask SpawnProjectile()
        {
            if (projectilePrefab != null && firePoint != null)
            {
                Debug.Log($"[PlayerController] Spawn {numberOfProjectiles} projectiles");
                for (int i = 0; i < numberOfProjectiles; i++)
                {
                    var projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                    var projectileController = projectile.GetComponent<Projectile.ProjectileController>();
                    if (projectileController != null)
                    {
                        projectileController.SetDamage(playerStats.GetAttackDamage());
                        projectileController.SetEffects(playerStats.runtimeStats.RuntimeEffects);
                        projectileController.SetProjectileActive();
                    }
                    await UniTask.Delay(100);
                }
            }
        }
    }

}