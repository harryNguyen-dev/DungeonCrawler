using Core;
using Cysharp.Threading.Tasks;
using PlayerController.Skill;
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
        private PlayerDash playerDash;
        private bool canAttack = true;

        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            playerEvents = GetComponent<PlayerEvents>();
            playerAnimation = GetComponent<PlayerAnimation>();
            playerRotate = GetComponent<Rotate>();
            playerDash = GetComponent<PlayerDash>();
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

        public void SetFirePoint(Transform point)
        {
            firePoint = point;
        }

        public Transform GetFirePoint() => firePoint;

        public void ApplyWeapon(SO.WeaponSO weapon)
        {
            if (weapon?.projectilePrefab != null)
                projectilePrefab = weapon.projectilePrefab;
        }

        public void SetAttackEnabled(bool enabled)
        {
            canAttack = enabled;
        }

        public bool TryGetCooldown(out float remaining, out float duration)
        {
            duration = attackCooldown;
            remaining = duration > 0f
                ? Mathf.Max(0f, lastAttackTime + duration - Time.time)
                : 0f;
            return remaining > 0f;
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
            if (playerDash != null && playerDash.IsDashing) return;

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
            if (projectilePrefab == null)
            {
                Debug.LogWarning("[PlayerController] SpawnProjectile aborted: projectilePrefab is null.");
                return;
            }

            if (firePoint == null)
            {
                Debug.LogWarning("[PlayerController] SpawnProjectile aborted: firePoint is null.");
                return;
            }

            var shootRotation = firePoint.rotation;
            Debug.Log($"[PlayerController] Spawn {numberOfProjectiles} projectiles");
            for (int i = 0; i < numberOfProjectiles; i++)
            {
                SkillProjectileSpawner.Spawn(
                    projectilePrefab,
                    firePoint.position,
                    shootRotation,
                    playerStats.RollAttackDamage(),
                    playerStats.runtimeStats.RuntimeEffects);

                await UniTask.Delay(100);
            }
        }
    }
}
