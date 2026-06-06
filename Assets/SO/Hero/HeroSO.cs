using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "Hero", menuName = "Hero/Hero")]
    public class HeroSO : ScriptableObject
    {
        public const float MinAttackCooldown = 0.15f;
        public const float CritDamageMultiplier = 2f;
        public const float MaxCritChance = 1f;

        [Header("Identity")]
        public string heroId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;
        public int sortOrder;

        [Header("Base stats")]
        public int maxHealth = 150;
        public int moveSpeed = 10;
        public int attackDamage = 30;
        public float attackCooldown = 0.5f;
        [Range(0f, 1f)]
        public float critChance = 0.1f;

        [Header("Loadout")]
        public WeaponSO boundWeapon;
        public HeroSkillSO skill;

        [Header("Visual")]
        public GameObject visualPrefab;
        public Vector3 visualLocalPosition;
        public Vector3 visualLocalEulerAngles;
        public Vector3 visualLocalScale = Vector3.one;

        [Header("Unlock")]
        public bool unlockedByDefault;
        public int unlockCost = 200;

        [Header("Meta upgrades — Damage")]
        public int damagePerTier = 5;
        public int maxDamageTier = 3;
        public int[] damageUpgradeCosts = { 75, 120, 180 };

        [Header("Meta upgrades — Fire rate")]
        public float cooldownReductionPerTier = 0.05f;
        public int maxFireRateTier = 2;
        public int[] fireRateUpgradeCosts = { 80, 140 };

        [Header("Meta upgrades — Health")]
        public int healthPerTier = 15;
        public int maxHealthTier = 3;
        public int[] healthUpgradeCosts = { 70, 110, 160 };

        [Header("Meta upgrades — Crit chance")]
        public float critChancePerTier = 0.02f;
        public int maxCritTier = 3;
        public int[] critUpgradeCosts = { 85, 130, 190 };

        public int GetDamageUpgradeCost(int nextTierIndex) =>
            GetUpgradeCost(damageUpgradeCosts, nextTierIndex);

        public int GetFireRateUpgradeCost(int nextTierIndex) =>
            GetUpgradeCost(fireRateUpgradeCosts, nextTierIndex);

        public int GetHealthUpgradeCost(int nextTierIndex) =>
            GetUpgradeCost(healthUpgradeCosts, nextTierIndex);

        public int GetCritUpgradeCost(int nextTierIndex) =>
            GetUpgradeCost(critUpgradeCosts, nextTierIndex);

        private static int GetUpgradeCost(int[] costs, int nextTierIndex)
        {
            if (costs == null || nextTierIndex < 0 || nextTierIndex >= costs.Length)
                return 0;
            return costs[nextTierIndex];
        }
    }
}
