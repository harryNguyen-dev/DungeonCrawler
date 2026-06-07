using SO;
using UnityEngine;

namespace PlayerController.Skill
{
    public readonly struct AppliedStatDelta
    {
        public StatModifierType Type { get; }
        public float FloatDelta { get; }
        public int IntDelta { get; }

        public AppliedStatDelta(StatModifierType type, float floatDelta, int intDelta)
        {
            Type = type;
            FloatDelta = floatDelta;
            IntDelta = intDelta;
        }
    }

    public static class StatModifierApplier
    {
        public static AppliedStatDelta Apply(PlayerStats stats, StatModifier modifier)
        {
            switch (modifier.type)
            {
                case StatModifierType.AttackDamageFlat:
                    var dmgFlat = Mathf.RoundToInt(modifier.value);
                    stats.UpgradeAttackDamage(dmgFlat);
                    return new AppliedStatDelta(modifier.type, 0f, dmgFlat);

                case StatModifierType.AttackDamagePercent:
                    var dmgPct = Mathf.RoundToInt(stats.GetAttackDamage() * modifier.value);
                    stats.UpgradeAttackDamage(dmgPct);
                    return new AppliedStatDelta(modifier.type, modifier.value, dmgPct);

                case StatModifierType.AttackCooldownFlat:
                    stats.ModifyAttackCooldown(modifier.value);
                    return new AppliedStatDelta(modifier.type, modifier.value, 0);

                case StatModifierType.MoveSpeedFlat:
                    var moveDelta = Mathf.RoundToInt(modifier.value);
                    stats.UpgradeIncreaseRunSpeed(moveDelta);
                    return new AppliedStatDelta(modifier.type, 0f, moveDelta);

                case StatModifierType.CritChanceFlat:
                    stats.ModifyCritChance(modifier.value);
                    return new AppliedStatDelta(modifier.type, modifier.value, 0);

                case StatModifierType.FireDamageFlat:
                    stats.ModifyWeaponEffect(WeaponEffectType.FireDamage, modifier.value);
                    return new AppliedStatDelta(modifier.type, modifier.value, 0);

                case StatModifierType.ProjectileCountFlat:
                    var projDelta = Mathf.RoundToInt(modifier.value);
                    stats.ModifyProjectileCount(projDelta);
                    return new AppliedStatDelta(modifier.type, 0f, projDelta);

                default:
                    Debug.LogWarning($"[StatModifierApplier] Unsupported modifier type {modifier.type}");
                    return new AppliedStatDelta(modifier.type, 0f, 0);
            }
        }

        public static void Revert(PlayerStats stats, AppliedStatDelta delta)
        {
            switch (delta.Type)
            {
                case StatModifierType.AttackDamageFlat:
                case StatModifierType.AttackDamagePercent:
                    stats.UpgradeAttackDamage(-delta.IntDelta);
                    break;

                case StatModifierType.AttackCooldownFlat:
                    stats.ModifyAttackCooldown(-delta.FloatDelta);
                    break;

                case StatModifierType.MoveSpeedFlat:
                    stats.UpgradeIncreaseRunSpeed(-delta.IntDelta);
                    break;

                case StatModifierType.CritChanceFlat:
                    stats.ModifyCritChance(-delta.FloatDelta);
                    break;

                case StatModifierType.FireDamageFlat:
                    stats.ModifyWeaponEffect(WeaponEffectType.FireDamage, -delta.FloatDelta);
                    break;

                case StatModifierType.ProjectileCountFlat:
                    stats.ModifyProjectileCount(-delta.IntDelta);
                    break;
            }
        }
    }
}
