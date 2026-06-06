using System.Collections.Generic;
using Global;
using SO;
using UnityEngine;

namespace Core.Save
{
    [System.Serializable]
    public class HeroUpgradeEntry
    {
        public string heroId;
        public int damageTier;
        public int fireRateTier;
        public int healthTier;
        public int critTier;
    }

    public static class HeroProgressService
    {
        public const string StarterHeroId = "hero_starter";

        public static bool IsUnlocked(string heroId)
        {
            if (string.IsNullOrEmpty(heroId))
                return false;

            var data = LevelProgressService.GetSaveData();
            EnsureHeroDefaults(data);
            return data.unlockedHeroIds != null && data.unlockedHeroIds.Contains(heroId);
        }

        public static string GetEquippedHeroId()
        {
            var data = LevelProgressService.GetSaveData();
            EnsureHeroDefaults(data);
            return string.IsNullOrEmpty(data.equippedHeroId) ? StarterHeroId : data.equippedHeroId;
        }

        public static bool TryEquip(string heroId)
        {
            if (!IsUnlocked(heroId))
                return false;

            var data = LevelProgressService.GetSaveData();
            data.equippedHeroId = heroId;
            LevelProgressService.SaveData(data);
            GlobalVariable.EquippedHeroId = heroId;
            return true;
        }

        public static bool TryUnlock(HeroSO hero)
        {
            if (hero == null || string.IsNullOrEmpty(hero.heroId))
                return false;

            if (IsUnlocked(hero.heroId))
                return true;

            if (!LevelProgressService.TrySpendMetaGold(hero.unlockCost))
                return false;

            var data = LevelProgressService.GetSaveData();
            EnsureHeroDefaults(data);
            if (!data.unlockedHeroIds.Contains(hero.heroId))
                data.unlockedHeroIds.Add(hero.heroId);

            LevelProgressService.SaveData(data);
            GlobalEvents.RaiseMetaGoldChanged();
            return true;
        }

        public static int GetDamageTier(string heroId)
        {
            return GetOrCreateEntry(heroId).damageTier;
        }

        public static int GetFireRateTier(string heroId)
        {
            return GetOrCreateEntry(heroId).fireRateTier;
        }

        public static int GetHealthTier(string heroId)
        {
            return GetOrCreateEntry(heroId).healthTier;
        }

        public static int GetCritTier(string heroId)
        {
            return GetOrCreateEntry(heroId).critTier;
        }

        public static bool TryUpgradeDamage(HeroSO hero)
        {
            if (hero == null || !IsUnlocked(hero.heroId))
                return false;

            var entry = GetOrCreateEntry(hero.heroId);
            if (entry.damageTier >= hero.maxDamageTier)
                return false;

            var cost = hero.GetDamageUpgradeCost(entry.damageTier);
            if (!LevelProgressService.TrySpendMetaGold(cost))
                return false;

            entry.damageTier++;
            PersistEntry(entry);
            GlobalEvents.RaiseMetaGoldChanged();
            return true;
        }

        public static bool TryUpgradeFireRate(HeroSO hero)
        {
            if (hero == null || !IsUnlocked(hero.heroId))
                return false;

            var entry = GetOrCreateEntry(hero.heroId);
            if (entry.fireRateTier >= hero.maxFireRateTier)
                return false;

            var cost = hero.GetFireRateUpgradeCost(entry.fireRateTier);
            if (!LevelProgressService.TrySpendMetaGold(cost))
                return false;

            entry.fireRateTier++;
            PersistEntry(entry);
            GlobalEvents.RaiseMetaGoldChanged();
            return true;
        }

        public static bool TryUpgradeHealth(HeroSO hero)
        {
            if (hero == null || !IsUnlocked(hero.heroId))
                return false;

            var entry = GetOrCreateEntry(hero.heroId);
            if (entry.healthTier >= hero.maxHealthTier)
                return false;

            var cost = hero.GetHealthUpgradeCost(entry.healthTier);
            if (!LevelProgressService.TrySpendMetaGold(cost))
                return false;

            entry.healthTier++;
            PersistEntry(entry);
            GlobalEvents.RaiseMetaGoldChanged();
            return true;
        }

        public static bool TryUpgradeCrit(HeroSO hero)
        {
            if (hero == null || !IsUnlocked(hero.heroId))
                return false;

            var entry = GetOrCreateEntry(hero.heroId);
            if (entry.critTier >= hero.maxCritTier)
                return false;

            var cost = hero.GetCritUpgradeCost(entry.critTier);
            if (!LevelProgressService.TrySpendMetaGold(cost))
                return false;

            entry.critTier++;
            PersistEntry(entry);
            GlobalEvents.RaiseMetaGoldChanged();
            return true;
        }

        public static void SyncEquippedHeroCache()
        {
            GlobalVariable.EquippedHeroId = GetEquippedHeroId();
        }

        public static void EnsureHeroDefaults(LevelProgressData data)
        {
            if (data == null)
                return;

            if (data.unlockedHeroIds == null)
                data.unlockedHeroIds = new List<string>();

            if (data.unlockedHeroIds.Count == 0)
                data.unlockedHeroIds.Add(StarterHeroId);

            if (!data.unlockedHeroIds.Contains(StarterHeroId))
                data.unlockedHeroIds.Insert(0, StarterHeroId);

            if (string.IsNullOrEmpty(data.equippedHeroId))
                data.equippedHeroId = StarterHeroId;

            if (data.heroUpgrades == null)
                data.heroUpgrades = new List<HeroUpgradeEntry>();
        }

        private static HeroUpgradeEntry GetOrCreateEntryInData(LevelProgressData data, string heroId)
        {
            EnsureHeroDefaults(data);

            foreach (var entry in data.heroUpgrades)
            {
                if (entry != null && entry.heroId == heroId)
                    return entry;
            }

            var created = new HeroUpgradeEntry { heroId = heroId };
            data.heroUpgrades.Add(created);
            return created;
        }

        private static HeroUpgradeEntry GetOrCreateEntry(string heroId)
        {
            var data = LevelProgressService.GetSaveData();
            return GetOrCreateEntryInData(data, heroId);
        }

        private static void PersistEntry(HeroUpgradeEntry entry)
        {
            var data = LevelProgressService.GetSaveData();
            if (data.heroUpgrades == null)
                data.heroUpgrades = new List<HeroUpgradeEntry>();

            for (var i = 0; i < data.heroUpgrades.Count; i++)
            {
                if (data.heroUpgrades[i]?.heroId == entry.heroId)
                {
                    data.heroUpgrades[i] = entry;
                    LevelProgressService.SaveData(data);
                    return;
                }
            }

            data.heroUpgrades.Add(entry);
            LevelProgressService.SaveData(data);
        }
    }
}
