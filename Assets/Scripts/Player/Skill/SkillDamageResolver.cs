using SO;
using UnityEngine;

namespace PlayerController.Skill
{
    public static class SkillDamageResolver
    {
        public static int Resolve(HeroSkillSO skill, PlayerStats stats)
        {
            if (skill == null || stats == null)
                return 0;

            int baseDamage = skill.damageMode switch
            {
                SkillDamageMode.Fixed => skill.damage > 0
                    ? skill.damage
                    : stats.GetAttackDamage(),
                SkillDamageMode.PercentOfAttack => Mathf.RoundToInt(stats.GetAttackDamage() * skill.damagePercent),
                SkillDamageMode.RollAttackDamage => stats.RollAttackDamage(),
                _ => skill.damage > 0 ? skill.damage : stats.GetAttackDamage(),
            };

            if (skill.damageMode == SkillDamageMode.RollAttackDamage && skill.damagePercent > 0f)
                baseDamage = Mathf.RoundToInt(baseDamage * skill.damagePercent);

            return Mathf.Max(0, baseDamage);
        }
    }
}
