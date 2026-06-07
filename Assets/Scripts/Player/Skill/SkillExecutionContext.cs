using SO;
using UnityEngine;

namespace PlayerController.Skill
{
    public readonly struct SkillExecutionContext
    {
        public HeroSkillSO Skill { get; }
        public PlayerStats Stats { get; }
        public Transform Caster { get; }
        public Transform FirePoint { get; }
        public Vector3 AimDirection { get; }

        public SkillExecutionContext(
            HeroSkillSO skill,
            PlayerStats stats,
            Transform caster,
            Transform firePoint,
            Vector3 aimDirection)
        {
            Skill = skill;
            Stats = stats;
            Caster = caster;
            FirePoint = firePoint;
            AimDirection = aimDirection;
        }
    }
}
