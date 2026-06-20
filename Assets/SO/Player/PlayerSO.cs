using UnityEngine;
using System.Collections.Generic;

namespace SO {
    public enum WeaponEffectType
    {
        NumberOfProjectiles,
        PierceCount,
        FireDamage,
        FrozenDuration,
        ExplosiveRadius,
        BoomerangMode // Hiệu ứng này nếu kích hoạt thì điền Value = 1
    }

    [System.Serializable]
    public struct WeaponEffectModifier
    {
        public WeaponEffectType EffectType;
        public float Value;
    }

    [CreateAssetMenu(fileName = "Player", menuName = "Player", order = 0)]
    public class PlayerSO : ScriptableObject
    {
        [Header("Hero stats")]
        public float AttackCooldown;
        public int AttackDamage;
        public float MoveSpeed;
        public int MaxHealth;
        public int Amor;
        [Range(0f, 1f)]
        public float CritChance;
        public float DefaultExpGainMultiplier = 1f;
        public float DefaultGoldGainMultiplier = 1f;
        public float DefaultHealMultiplier = 1f;
        [Range(0f, 1f)]
        public float ThornReflectPercent;

        [Header("Weapon Stats")]
        public List<WeaponEffectModifier> WeaponEffectsSetup = new List<WeaponEffectModifier>();

        public Dictionary<WeaponEffectType, float> RuntimeEffects { get; private set; } = new Dictionary<WeaponEffectType, float>();

        public void AddpendWeaponModifier(WeaponEffectModifier modifier)
        {
            WeaponEffectsSetup.Add(modifier);
            if(!RuntimeEffects.ContainsKey(modifier.EffectType))
            {
                RuntimeEffects.Add(modifier.EffectType, modifier.Value);
            } else
            {
                RuntimeEffects[modifier.EffectType] += modifier.Value;
            }
        }
        public void InitializeRuntimeDictionary()
        {
            RuntimeEffects.Clear();
            foreach (var effect in WeaponEffectsSetup)
            {
                if (!RuntimeEffects.ContainsKey(effect.EffectType))
                    RuntimeEffects.Add(effect.EffectType, effect.Value);
                else
                    RuntimeEffects[effect.EffectType] += effect.Value;
            }
        }
        public bool TryGetEffect(WeaponEffectType type, out float value)
        {
            return RuntimeEffects.TryGetValue(type, out value);
        }
    }
}
