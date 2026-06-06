using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "Hero", menuName = "Hero/Hero")]
    public class HeroSO : ScriptableObject
    {
        public const float MinAttackCooldown = 0.15f;
        public const float CritDamageMultiplier = 2f;
        public const float MaxCritChance = 1f;

        private static readonly HeroUpgradeStep[] DefaultUpgrades =
        {
            new() { cost = 75, damageBonus = 5 },
            new() { cost = 120, damageBonus = 5 },
            new() { cost = 180, damageBonus = 5 },
            new() { cost = 80, cooldownReduction = 0.05f },
            new() { cost = 140, cooldownReduction = 0.05f },
            new() { cost = 70, healthBonus = 15 },
            new() { cost = 110, healthBonus = 15 },
            new() { cost = 160, healthBonus = 15 },
            new() { cost = 85, critChanceBonus = 0.02f },
            new() { cost = 130, critChanceBonus = 0.02f },
            new() { cost = 190, critChanceBonus = 0.02f },
        };

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

        [Header("Meta upgrades")]
        public HeroUpgradeStep[] upgrades;

        public int MaxUpgradeTier => GetUpgrades().Length;

        public HeroUpgradeStep[] GetUpgrades() =>
            upgrades != null && upgrades.Length > 0 ? upgrades : DefaultUpgrades;

        public int GetUpgradeCost(int tierIndex)
        {
            var steps = GetUpgrades();
            if (tierIndex < 0 || tierIndex >= steps.Length)
                return 0;
            return steps[tierIndex].cost;
        }

        public void GetAccumulatedBonuses(int tier, out int damageBonus, out int healthBonus,
            out float cooldownReduction, out float critChanceBonus)
        {
            damageBonus = 0;
            healthBonus = 0;
            cooldownReduction = 0f;
            critChanceBonus = 0f;

            var steps = GetUpgrades();
            var count = Mathf.Clamp(tier, 0, steps.Length);
            for (var i = 0; i < count; i++)
            {
                damageBonus += steps[i].damageBonus;
                healthBonus += steps[i].healthBonus;
                cooldownReduction += steps[i].cooldownReduction;
                critChanceBonus += steps[i].critChanceBonus;
            }
        }
    }
}
