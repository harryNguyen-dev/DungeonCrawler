using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlayerController
{
    [RequireComponent(typeof(Rotate))]
    public class Attack : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        private PlayerStats playerStats;
        private PlayerEvents playerEvents;
        private PlayerAnimation playerAnimation;
        private Rotate playerRotate;
        private float attackCooldown;
        private int numberOfProjectiles = 1;
        private float lastAttackTime;
        private bool canAttack = true;

        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            playerEvents = GetComponent<PlayerEvents>();
            playerAnimation = GetComponent<PlayerAnimation>();
            playerRotate = GetComponent<Rotate>();
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

            // Cũ: đánh ngay, model đã xoay theo chuột liên tục (Rotate.LateUpdate)
            // if (InputManager.Instance.IsAttacking())
            // {
            //     if (Time.time >= lastAttackTime + attackCooldown)
            //     {
            //         PerformAttack();
            //         lastAttackTime = Time.time;
            //     }
            // }

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
            // Xoay mặt về enemy gần nhất rồi mới đánh
            playerRotate?.SnapFaceAimDirection();

            Debug.Log("[PlayerController] Perform Attack");
            playerAnimation.SetAttack();
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
                // TODO-check null reference
                
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
