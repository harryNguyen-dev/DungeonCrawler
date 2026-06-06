using Core;
using Cysharp.Threading.Tasks;
using SO;
using UnityEngine;

namespace PlayerController
{
    public class PlayerSkill : MonoBehaviour
    {
        private PlayerStats playerStats;
        private PlayerAnimation playerAnimation;
        private Rotate playerRotate;
        private HeroSkillSO activeSkill;
        private float lastSkillTime = -999f;
        private bool canUseSkill = true;
        private PlayerDash playerDash;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
            playerAnimation = GetComponent<PlayerAnimation>();
            playerRotate = GetComponent<Rotate>();
            playerDash = GetComponent<PlayerDash>();
        }

        public void SetSkillEnabled(bool enabled)
        {
            canUseSkill = enabled;
        }

        public void SetActiveSkill(HeroSkillSO skill)
        {
            activeSkill = skill;
        }

        private void Update()
        {
            if (!canUseSkill || activeSkill == null)
                return;

            if (playerDash != null && playerDash.IsDashing)
                return;

            var input = InputManager.Instance;
            if (input == null)
                return;

            if (input.IsSkillAimHeld())
            {
                Vector3 aimDirection = GetLiveAimDirection();
                if (aimDirection.sqrMagnitude > 0.0001f)
                    playerRotate?.SnapFaceDirection(aimDirection);
            }

            if (!input.WasSkillAimReleased())
                return;

            if (Time.time < lastSkillTime + activeSkill.cooldown)
                return;

            PerformSkill().Forget();
        }

        private async UniTask PerformSkill()
        {
            lastSkillTime = Time.time;

            Vector3 aimDirection = GetReleaseAimDirection();
            playerRotate?.SnapFaceDirection(aimDirection);
            playerAnimation?.SetSkill();

            await UniTask.Yield(PlayerLoopTiming.Update);
            SpawnSkillProjectile(aimDirection);
        }

        private Vector3 GetLiveAimDirection()
        {
            return ToWorldDirection(InputManager.Instance.GetSkillAimVector());
        }

        private Vector3 GetReleaseAimDirection()
        {
            var release = InputManager.Instance.GetSkillAimReleaseVector();
            var world = ToWorldDirection(release);
            if (world.sqrMagnitude > 0.0001f)
                return world;

            return transform.forward;
        }

        private static Vector3 ToWorldDirection(Vector2 input)
        {
            if (input.sqrMagnitude <= 0.01f)
                return Vector3.zero;

            return new Vector3(input.x, 0f, input.y).normalized;
        }

        private void SpawnSkillProjectile(Vector3 direction)
        {
            if (activeSkill.skillProjectilePrefab == null)
                return;

            var spawnPos = transform.position + Vector3.up * 1f;
            var rotation = Quaternion.LookRotation(direction);

            GameObject prefab = activeSkill.skillProjectilePrefab;
            PoolId poolId = PoolId.None;
            if (prefab.TryGetComponent<PooledObject>(out var poolable))
                poolId = poolable.PoolId;

            GameObject instance = null;
            if (poolId != PoolId.None && ObjectPoolingManager.Instance != null)
                instance = ObjectPoolingManager.Instance.Get(poolId, spawnPos, rotation);
            else
                instance = Instantiate(prefab, spawnPos, rotation);

            if (instance == null)
                return;

            var projectileController = instance.GetComponent<Projectile.ProjectileController>();
            if (projectileController != null)
            {
                int damage = activeSkill.damage > 0
                    ? activeSkill.damage
                    : playerStats.GetAttackDamage();

                projectileController.SetDamage(damage);
                projectileController.SetEffects(BuildSkillEffects());
                projectileController.SetProjectileActive();
            }

            var move = instance.GetComponent<Projectile.ProjectileMove>();
            if (move != null && activeSkill.projectileSpeed > 0f)
                move.SetSpeed(activeSkill.projectileSpeed);
        }

        private System.Collections.Generic.Dictionary<WeaponEffectType, float> BuildSkillEffects()
        {
            var effects = new System.Collections.Generic.Dictionary<WeaponEffectType, float>();
            if (activeSkill.skillEffects == null)
                return effects;

            foreach (var modifier in activeSkill.skillEffects)
                effects[modifier.EffectType] = modifier.Value;

            return effects;
        }
    }
}
