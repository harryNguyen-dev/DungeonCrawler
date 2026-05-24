using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlayerController
{
    public class Attack : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        private PlayerStats playerStats;
        private PlayerEvents playerEvents;
        private float attackCooldown;
        private int numberOfProjectiles = 1;
        private float lastAttackTime;
        private bool canAttack = true;

        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            playerEvents = GetComponent<PlayerEvents>();
            attackCooldown = playerStats.GetAttackCooldown();
            if (playerStats.runtimeStats.TryGetEffect(SO.WeaponEffectType.NumberOfProjectiles, out var value))
            {
                numberOfProjectiles = Mathf.RoundToInt(value);
            }

            playerEvents.OnAttackSpeedChanged += OnAttackChanged;
            playerEvents.OnNumberOfProjectileChanged += OnNumberOfProjectileChanged;
        }

        private void OnDestroy()
        {
            if (playerEvents == null) return;
            playerEvents.OnAttackSpeedChanged -= OnAttackChanged;
            playerEvents.OnNumberOfProjectileChanged -= OnNumberOfProjectileChanged;
        }

        public void SetAttackEnabled(bool enabled)
        {
            canAttack = enabled;
        }

        private void OnAttackChanged(float attackSpeed) => attackCooldown = attackSpeed;

        private void OnNumberOfProjectileChanged(int num)
        {
            Debug.Log($"[PlayerController] OnNumberOfProjectileChanged {num}");
            numberOfProjectiles = Mathf.RoundToInt(num);
        }

        private void Update()
        {
            if (!canAttack) return;

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
            if (projectilePrefab == null || firePoint == null) return;

            Debug.Log($"[PlayerController] Spawn {numberOfProjectiles} projectiles");
            for (int i = 0; i < numberOfProjectiles; i++)
            {
                // var projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                GameObject projectile = projectilePrefab;
                PoolId poolId = PoolId.None;
                if(projectile != null && projectile.TryGetComponent<Core.PooledObject>(out var poolable))
                {
                    poolId = poolable.PoolId;
                }
                GameObject projectileInstance = null;
                if(poolId != PoolId.None && ObjectPoolingManager.Instance != null)
                {
                    projectileInstance = ObjectPoolingManager.Instance.Get(poolId, firePoint.position, firePoint.rotation);
                }
                var projectileController = projectileInstance.GetComponent<Projectile.ProjectileController>();
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
