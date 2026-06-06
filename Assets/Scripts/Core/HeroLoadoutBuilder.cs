using Core.Save;
using Global;
using SO;
using UnityEngine;

namespace Core
{
    public struct HeroLoadoutResult
    {
        public HeroSO Hero;
        public WeaponSO Weapon;
        public PlayerSO Stats;
        public HeroSkillSO Skill;
    }

    public static class HeroLoadoutBuilder
    {
        public static HeroLoadoutResult BuildForEquippedHero()
        {
            var catalog = GlobalEntities.Instance?.HeroCatalog;
            var heroId = HeroProgressService.GetEquippedHeroId();
            var hero = catalog != null ? catalog.GetById(heroId) : null;

            if (hero == null)
                hero = catalog?.GetDefaultHero();

            if (hero == null || hero.boundWeapon == null)
            {
                Debug.LogWarning("[HeroLoadoutBuilder] Missing hero or bound weapon.");
                return default;
            }

            var stats = BuildEffectivePlayerSO(hero);
            return new HeroLoadoutResult
            {
                Hero = hero,
                Weapon = hero.boundWeapon,
                Stats = stats,
                Skill = hero.skill
            };
        }

        public static PlayerSO BuildEffectivePlayerSO(HeroSO hero)
        {
            if (hero?.boundWeapon == null)
            {
                Debug.LogWarning("[HeroLoadoutBuilder] Missing bound weapon.");
                return null;
            }

            var weapon = hero.boundWeapon;
            var result = ScriptableObject.CreateInstance<PlayerSO>();
            result.MaxHealth = hero.maxHealth;
            result.MoveSpeed = hero.moveSpeed;
            result.AttackDamage = hero.attackDamage;
            result.AttackCooldown = hero.attackCooldown;
            result.CritChance = hero.critChance;
            result.Amor = 0;
            result.DefaultExpGainMultiplier = 1f;
            result.DefaultGoldGainMultiplier = 1f;
            result.WeaponEffectsSetup = new System.Collections.Generic.List<WeaponEffectModifier>();

            foreach (var effect in weapon.intrinsicEffects)
                result.WeaponEffectsSetup.Add(effect);

            var damageTier = HeroProgressService.GetDamageTier(hero.heroId);
            var fireRateTier = HeroProgressService.GetFireRateTier(hero.heroId);
            var healthTier = HeroProgressService.GetHealthTier(hero.heroId);
            var critTier = HeroProgressService.GetCritTier(hero.heroId);

            result.AttackDamage += damageTier * hero.damagePerTier;
            result.MaxHealth += healthTier * hero.healthPerTier;
            result.CritChance = Mathf.Min(
                HeroSO.MaxCritChance,
                result.CritChance + critTier * hero.critChancePerTier);
            result.AttackCooldown = Mathf.Max(
                HeroSO.MinAttackCooldown,
                result.AttackCooldown - fireRateTier * hero.cooldownReductionPerTier);

            result.InitializeRuntimeDictionary();
            return result;
        }
    }
}
