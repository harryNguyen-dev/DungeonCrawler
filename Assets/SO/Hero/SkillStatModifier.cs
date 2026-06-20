using System;

namespace SO
{
    public enum StatModifierType
    {
        AttackDamageFlat,
        AttackDamagePercent,
        AttackCooldownFlat,
        MoveSpeedFlat,
        CritChanceFlat,
        FireDamageFlat,
        ProjectileCountFlat,
        FrozenDurationFlat,
    }

    [Serializable]
    public struct StatModifier
    {
        public StatModifierType type;
        public float value;
    }
}
