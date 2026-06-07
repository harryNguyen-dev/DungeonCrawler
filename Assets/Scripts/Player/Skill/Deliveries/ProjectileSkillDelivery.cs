using UnityEngine;

namespace PlayerController.Skill.Deliveries
{
    public sealed class ProjectileSkillDelivery : ISkillDelivery
    {
        public void Execute(in SkillExecutionContext context)
        {
            var skill = context.Skill;
            if (skill.skillProjectilePrefab == null)
                return;

            var spawnPos = GetSpawnPosition(context);
            var rotation = Quaternion.LookRotation(context.AimDirection);
            var damage = SkillDamageResolver.Resolve(skill, context.Stats);
            var effects = SkillEffectBuilder.FromModifiers(skill.skillEffects);

            SkillProjectileSpawner.Spawn(
                skill.skillProjectilePrefab,
                spawnPos,
                rotation,
                damage,
                effects,
                skill.projectileSpeed);
        }

        private static Vector3 GetSpawnPosition(in SkillExecutionContext context)
        {
            if (context.FirePoint != null)
                return context.FirePoint.position;

            return context.Caster.position + Vector3.up;
        }
    }
}
