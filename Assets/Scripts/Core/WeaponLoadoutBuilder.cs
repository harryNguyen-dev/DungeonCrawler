using Core.Save;
using SO;
using UnityEngine;

namespace Core
{
    public static class WeaponLoadoutBuilder
    {
        public static PlayerSO BuildEffectivePlayerSO(WeaponSO weapon)
        {
            if (weapon == null || weapon.baseStats == null)
            {
                Debug.LogWarning("[WeaponLoadoutBuilder] Missing weapon or baseStats.");
                return null;
            }

            var result = Object.Instantiate(weapon.baseStats);
            result.WeaponEffectsSetup = new System.Collections.Generic.List<WeaponEffectModifier>();

            foreach (var effect in weapon.intrinsicEffects)
                result.WeaponEffectsSetup.Add(effect);

            var damageTier = WeaponProgressService.GetDamageTier(weapon.weaponId);
            var fireRateTier = WeaponProgressService.GetFireRateTier(weapon.weaponId);

            result.AttackDamage += damageTier * weapon.damagePerTier;
            result.AttackCooldown = Mathf.Max(
                WeaponSO.MinAttackCooldown,
                result.AttackCooldown - fireRateTier * weapon.cooldownReductionPerTier);

            result.InitializeRuntimeDictionary();
            return result;
        }

        public static PlayerSO BuildForEquippedWeapon()
        {
            var catalog = Global.GlobalEntities.Instance?.WeaponCatalog;
            var weaponId = WeaponProgressService.GetEquippedWeaponId();
            var weapon = catalog != null ? catalog.GetById(weaponId) : null;

            if (weapon == null)
                weapon = catalog?.GetDefaultWeapon();

            return BuildEffectivePlayerSO(weapon);
        }
    }
}
