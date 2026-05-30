using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapon/Weapon")]
    public class WeaponSO : ScriptableObject
    {
        [Header("Identity")]
        public string weaponId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;
        public int sortOrder;

        [Header("Combat base")]
        public PlayerSO baseStats;
        public List<WeaponEffectModifier> intrinsicEffects = new();

        [Header("Unlock")]
        public bool unlockedByDefault;
        public int unlockCost = 200;

        [Header("Meta upgrades (player-wide)")]
        public int damagePerTier = 5;
        public int maxDamageTier = 3;
        public int[] damageUpgradeCosts = { 75, 120, 180 };

        public float cooldownReductionPerTier = 0.05f;
        public int maxFireRateTier = 2;
        public int[] fireRateUpgradeCosts = { 80, 140 };

        public const float MinAttackCooldown = 0.15f;

        public int GetDamageUpgradeCost(int nextTierIndex)
        {
            if (damageUpgradeCosts == null || nextTierIndex < 0 || nextTierIndex >= damageUpgradeCosts.Length)
                return 0;
            return damageUpgradeCosts[nextTierIndex];
        }

        public int GetFireRateUpgradeCost(int nextTierIndex)
        {
            if (fireRateUpgradeCosts == null || nextTierIndex < 0 || nextTierIndex >= fireRateUpgradeCosts.Length)
                return 0;
            return fireRateUpgradeCosts[nextTierIndex];
        }
    }
}
