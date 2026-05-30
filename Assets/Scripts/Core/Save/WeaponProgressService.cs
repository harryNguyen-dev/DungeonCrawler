using System.Collections.Generic;
using SO;
using UnityEngine;

namespace Core.Save
{
    [System.Serializable]
    public class WeaponUpgradeEntry
    {
        public string weaponId;
        public int damageTier;
        public int fireRateTier;
    }

    public static class WeaponProgressService
    {
        public const string StarterWeaponId = "weapon_starter";

        public static bool IsUnlocked(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
                return false;

            var data = LevelProgressService.GetSaveData();
            EnsureWeaponDefaults(data);
            return data.unlockedWeaponIds != null && data.unlockedWeaponIds.Contains(weaponId);
        }

        public static string GetEquippedWeaponId()
        {
            var data = LevelProgressService.GetSaveData();
            EnsureWeaponDefaults(data);
            return string.IsNullOrEmpty(data.equippedWeaponId) ? StarterWeaponId : data.equippedWeaponId;
        }

        public static bool TryEquip(string weaponId)
        {
            if (!IsUnlocked(weaponId))
                return false;

            var data = LevelProgressService.GetSaveData();
            data.equippedWeaponId = weaponId;
            LevelProgressService.SaveData(data);
            Global.GlobalVariable.EquippedWeaponId = weaponId;
            return true;
        }

        public static bool TryUnlock(WeaponSO weapon)
        {
            if (weapon == null || string.IsNullOrEmpty(weapon.weaponId))
                return false;

            if (IsUnlocked(weapon.weaponId))
                return true;

            if (!LevelProgressService.TrySpendMetaGold(weapon.unlockCost))
                return false;

            var data = LevelProgressService.GetSaveData();
            EnsureWeaponDefaults(data);
            if (!data.unlockedWeaponIds.Contains(weapon.weaponId))
                data.unlockedWeaponIds.Add(weapon.weaponId);

            LevelProgressService.SaveData(data);
            Global.GlobalEvents.RaiseMetaGoldChanged();
            return true;
        }

        public static int GetDamageTier(string weaponId)
        {
            return GetOrCreateEntry(weaponId).damageTier;
        }

        public static int GetFireRateTier(string weaponId)
        {
            return GetOrCreateEntry(weaponId).fireRateTier;
        }

        public static bool TryUpgradeDamage(WeaponSO weapon)
        {
            if (weapon == null || !IsUnlocked(weapon.weaponId))
                return false;

            var entry = GetOrCreateEntry(weapon.weaponId);
            if (entry.damageTier >= weapon.maxDamageTier)
                return false;

            var cost = weapon.GetDamageUpgradeCost(entry.damageTier);
            if (!LevelProgressService.TrySpendMetaGold(cost))
                return false;

            entry.damageTier++;
            PersistEntry(entry);
            Global.GlobalEvents.RaiseMetaGoldChanged();
            return true;
        }

        public static bool TryUpgradeFireRate(WeaponSO weapon)
        {
            if (weapon == null || !IsUnlocked(weapon.weaponId))
                return false;

            var entry = GetOrCreateEntry(weapon.weaponId);
            if (entry.fireRateTier >= weapon.maxFireRateTier)
                return false;

            var cost = weapon.GetFireRateUpgradeCost(entry.fireRateTier);
            if (!LevelProgressService.TrySpendMetaGold(cost))
                return false;

            entry.fireRateTier++;
            PersistEntry(entry);
            Global.GlobalEvents.RaiseMetaGoldChanged();
            return true;
        }

        public static void SyncEquippedWeaponCache()
        {
            Global.GlobalVariable.EquippedWeaponId = GetEquippedWeaponId();
        }

        private static WeaponUpgradeEntry GetOrCreateEntry(string weaponId)
        {
            var data = LevelProgressService.GetSaveData();
            EnsureWeaponDefaults(data);

            if (data.weaponUpgrades == null)
                data.weaponUpgrades = new List<WeaponUpgradeEntry>();

            foreach (var entry in data.weaponUpgrades)
            {
                if (entry != null && entry.weaponId == weaponId)
                    return entry;
            }

            var created = new WeaponUpgradeEntry { weaponId = weaponId };
            data.weaponUpgrades.Add(created);
            LevelProgressService.SaveData(data);
            return created;
        }

        private static void PersistEntry(WeaponUpgradeEntry entry)
        {
            var data = LevelProgressService.GetSaveData();
            if (data.weaponUpgrades == null)
                data.weaponUpgrades = new List<WeaponUpgradeEntry>();

            for (var i = 0; i < data.weaponUpgrades.Count; i++)
            {
                if (data.weaponUpgrades[i]?.weaponId == entry.weaponId)
                {
                    data.weaponUpgrades[i] = entry;
                    LevelProgressService.SaveData(data);
                    return;
                }
            }

            data.weaponUpgrades.Add(entry);
            LevelProgressService.SaveData(data);
        }

        private static void EnsureWeaponDefaults(LevelProgressData data)
        {
            if (data.unlockedWeaponIds == null)
                data.unlockedWeaponIds = new List<string>();

            if (data.unlockedWeaponIds.Count == 0)
                data.unlockedWeaponIds.Add(StarterWeaponId);

            if (!data.unlockedWeaponIds.Contains(StarterWeaponId))
                data.unlockedWeaponIds.Insert(0, StarterWeaponId);

            if (string.IsNullOrEmpty(data.equippedWeaponId))
                data.equippedWeaponId = StarterWeaponId;
        }
    }
}
