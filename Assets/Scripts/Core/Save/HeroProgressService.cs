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
        public int upgradeTier;
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

        public static int GetUpgradeTier(string heroId) =>
            GetOrCreateEntry(heroId).upgradeTier;

        public static bool CanUpgrade(HeroSO hero)
        {
            if (hero == null || !IsUnlocked(hero.heroId))
                return false;

            var tier = GetUpgradeTier(hero.heroId);
            if (tier >= hero.MaxUpgradeTier)
                return false;

            return LevelProgressService.GetMetaGold() >= hero.GetUpgradeCost(tier);
        }

        public static bool TryUpgrade(HeroSO hero)
        {
            if (hero == null || !IsUnlocked(hero.heroId))
                return false;

            var entry = GetOrCreateEntry(hero.heroId);
            if (entry.upgradeTier >= hero.MaxUpgradeTier)
                return false;

            var cost = hero.GetUpgradeCost(entry.upgradeTier);
            if (!LevelProgressService.TrySpendMetaGold(cost))
                return false;

            entry.upgradeTier++;
            PersistEntry(entry);
            GlobalEvents.RaiseMetaGoldChanged();
            return true;
        }

        public static void SyncEquippedHeroCache()
        {
            GlobalVariable.EquippedHeroId = GetEquippedHeroId();
        }

        public static void ApplyFreshDefaults(LevelProgressData data)
        {
            if (data == null)
                return;

            data.heroUpgrades = new List<HeroUpgradeEntry>();
            data.unlockedHeroIds = new List<string> { StarterHeroId };

            var catalog = Global.GlobalEntities.Instance?.HeroCatalog;
            if (catalog?.heroes != null)
            {
                foreach (var hero in catalog.heroes)
                {
                    if (hero == null || string.IsNullOrEmpty(hero.heroId))
                        continue;

                    if (hero.unlockedByDefault && !data.unlockedHeroIds.Contains(hero.heroId))
                        data.unlockedHeroIds.Add(hero.heroId);
                }
            }

            var defaultHero = catalog?.GetDefaultHero();
            data.equippedHeroId = defaultHero != null ? defaultHero.heroId : StarterHeroId;
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
