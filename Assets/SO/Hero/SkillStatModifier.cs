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
    }

    [Serializable]
    public struct StatModifier
    {
        public StatModifierType type;
        public float value;
    }
}
