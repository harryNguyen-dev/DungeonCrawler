using System.Collections.Generic;
using SO;

namespace PlayerController.Skill
{
    public static class SkillEffectBuilder
    {
        public static Dictionary<WeaponEffectType, float> FromModifiers(List<WeaponEffectModifier> modifiers)
        {
            var effects = new Dictionary<WeaponEffectType, float>();
            if (modifiers == null)
                return effects;

            foreach (var modifier in modifiers)
                effects[modifier.EffectType] = modifier.Value;

            return effects;
        }

        public static Dictionary<WeaponEffectType, float> WithExtraPierce(
            Dictionary<WeaponEffectType, float> baseEffects,
            int extraPierce)
        {
            if (extraPierce <= 0)
                return baseEffects;

            var effects = new Dictionary<WeaponEffectType, float>(baseEffects);
            effects.TryGetValue(WeaponEffectType.PierceCount, out var current);
            effects[WeaponEffectType.PierceCount] = current + extraPierce;
            return effects;
        }
    }
}
