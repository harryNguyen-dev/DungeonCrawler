using UnityEngine;
using System.Collections.Generic;

namespace SO {
    public enum WeaponEffectType
    {
        PierceCount,
        FireDamage,
        FrozenDuration,
        ExplosiveRadius,
        AdditionalProjectile,
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
        public int MoveSpeed;
        public int MaxHealth;
        public int Amor;
        public float DefaultExpGainMultiplier = 1f;
        public float DefaultGoldGainMultiplier = 1f;

        [Header("Weapon Dynamic Stats")]
        public List<WeaponEffectModifier> WeaponEffectsSetup = new List<WeaponEffectModifier>();

        public Dictionary<WeaponEffectType, float> RuntimeEffects { get; private set; } = new Dictionary<WeaponEffectType, float>();

        public void InitializeRuntimeDictionary()
        {
            RuntimeEffects.Clear();
            foreach (var effect in WeaponEffectsSetup)
            {
                if (!RuntimeEffects.ContainsKey(effect.EffectType))
                {
                    RuntimeEffects.Add(effect.EffectType, effect.Value);
                }
            }
        }
        public bool TryGetEffect(WeaponEffectType type, out float value)
        {
            return RuntimeEffects.TryGetValue(type, out value);
        }
    }
}
