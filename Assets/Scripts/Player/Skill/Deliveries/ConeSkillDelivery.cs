using Cysharp.Threading.Tasks;
using SO;
using UnityEngine;

namespace PlayerController.Skill.Deliveries
{
    public sealed class ConeSkillDelivery : ISkillDelivery
    {
        public void Execute(in SkillExecutionContext context)
        {
            ExecuteAsync(context).Forget();
        }

        private async UniTask ExecuteAsync(SkillExecutionContext context)
        {
            var skill = context.Skill;
            if (skill.skillProjectilePrefab == null)
                return;

            var config = skill.coneConfig;
            var count = config.projectileCount > 0 ? config.projectileCount : 8;
            var angle = config.coneAngle > 0f ? config.coneAngle : skill.coneAngle;
            if (angle <= 0f)
                angle = 45f;

            var spawnPos = context.FirePoint != null
                ? context.FirePoint.position
                : context.Caster.position + Vector3.up;

            var baseEffects = SkillEffectBuilder.FromModifiers(skill.skillEffects);
            var halfAngle = angle * 0.5f;
            var centerIndex = (count - 1) * 0.5f;

            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0.5f : i / (float)(count - 1);
                var yaw = Mathf.Lerp(-halfAngle, halfAngle, t);
                var direction = Quaternion.Euler(0f, yaw, 0f) * context.AimDirection;
                var rotation = Quaternion.LookRotation(direction);
                var damage = SkillDamageResolver.Resolve(skill, context.Stats);

                var effects = baseEffects;
                if (config.centerPelletPierce && Mathf.Abs(i - centerIndex) < 0.01f)
                    effects = SkillEffectBuilder.WithExtraPierce(baseEffects, 1);

                SkillProjectileSpawner.Spawn(
                    skill.skillProjectilePrefab,
                    spawnPos,
                    rotation,
                    damage,
                    effects,
                    skill.projectileSpeed);

                if (config.spawnDelayMs > 0f && i < count - 1)
                    await UniTask.Delay((int)config.spawnDelayMs);
            }
        }
    }
}
