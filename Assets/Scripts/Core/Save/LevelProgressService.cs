using System;
using System.Collections.Generic;
using System.IO;
using SO;
using UnityEngine;

namespace Core.Save
{
    [Serializable]
    public class LevelStarEntry
    {
        public string levelId;
        public int bestStars;
    }

    [Serializable]
    public class LevelProgressData
    {
        public int highestUnlockedIndex;
        public int metaGold;

        public List<string> unlockedHeroIds;
        public string equippedHeroId;
        public List<HeroUpgradeEntry> heroUpgrades;

        public List<LevelStarEntry> levelStars;
    }

    /// <summary>Lưu tiến độ màn đã mở khóa (local JSON).</summary>
    public static class LevelProgressService
    {
        private const string SaveFileName = "level_progress.json";
        private static LevelProgressData cachedData;

        public static int GetHighestUnlockedIndex(int catalogLevelCount)
        {
            var data = Load();
            if (catalogLevelCount <= 0)
                return 0;

            return Mathf.Clamp(data.highestUnlockedIndex, 0, catalogLevelCount - 1);
        }

        public static bool IsUnlocked(int levelIndex, int catalogLevelCount)
        {
            if (levelIndex < 0)
                return false;

            // Map 1 luôn mở (lần đầu chơi: unlock + 0 sao).
            if (levelIndex == 0)
                return true;

            if (catalogLevelCount <= 0)
                return false;

            return levelIndex <= GetHighestUnlockedIndex(catalogLevelCount);
        }

        public static void UnlockLevel(int levelIndex, int catalogLevelCount)
        {
            if (levelIndex < 0 || catalogLevelCount <= 0)
                return;

            var data = Load();
            var clamped = Mathf.Clamp(levelIndex, 0, catalogLevelCount - 1);
            if (clamped <= data.highestUnlockedIndex)
                return;

            data.highestUnlockedIndex = clamped;
            Save(data);
        }

        public static void UnlockNextAfter(int clearedLevelIndex, int catalogLevelCount)
        {
            UnlockLevel(clearedLevelIndex + 1, catalogLevelCount);
        }

        public static int GetBestStars(string levelId)
        {
            if (string.IsNullOrWhiteSpace(levelId))
                return 0;

            var data = Load();
            EnsureLevelStarsList(data);

            foreach (var entry in data.levelStars)
            {
                if (entry != null && entry.levelId == levelId)
                    return Mathf.Clamp(entry.bestStars, 0, 3);
            }

            return 0;
        }

        public static bool TryUpdateBestStars(string levelId, int earnedStars, out int newBest)
        {
            newBest = 0;
            if (string.IsNullOrWhiteSpace(levelId))
                return false;

            earnedStars = Mathf.Clamp(earnedStars, 0, 3);
            var data = Load();
            EnsureLevelStarsList(data);

            LevelStarEntry existing = null;
            foreach (var entry in data.levelStars)
            {
                if (entry != null && entry.levelId == levelId)
                {
                    existing = entry;
                    break;
                }
            }

            if (existing == null)
            {
                existing = new LevelStarEntry { levelId = levelId, bestStars = earnedStars };
                data.levelStars.Add(existing);
                newBest = earnedStars;
                Save(data);
                return earnedStars > 0;
            }

            if (earnedStars <= existing.bestStars)
            {
                newBest = existing.bestStars;
                return false;
            }

            existing.bestStars = earnedStars;
            newBest = earnedStars;
            Save(data);
            return true;
        }

        public static int GetUnlockThreshold(LevelSO level)
        {
            return level == null ? 2 : Mathf.Clamp(level.unlockNextAtStars, 2, 3);
        }

        public static bool TryUnlockFromStars(int levelIndex, int stars, LevelSO level, int catalogLevelCount)
        {
            if (levelIndex < 0 || catalogLevelCount <= 0 || level == null)
                return false;

            var threshold = GetUnlockThreshold(level);
            if (stars < threshold)
                return false;

            var previousUnlocked = GetHighestUnlockedIndex(catalogLevelCount);
            UnlockNextAfter(levelIndex, catalogLevelCount);
            return GetHighestUnlockedIndex(catalogLevelCount) > previousUnlocked;
        }

        public static int GetMetaGold() => Load().metaGold;

        public static int AddMetaGold(int amount)
        {
            if (amount <= 0)
                return GetMetaGold();

            var data = Load();
            data.metaGold += amount;
            Save(data);
            Global.GlobalEvents.RaiseMetaGoldChanged();
            return data.metaGold;
        }

        public static bool TrySpendMetaGold(int amount)
        {
            if (amount <= 0)
                return true;

            var data = Load();
            if (data.metaGold < amount)
                return false;

            data.metaGold -= amount;
            Save(data);
            return true;
        }

        public static LevelProgressData GetSaveData() => Load();

        public static void SaveData(LevelProgressData data) => Save(data);

        public static void ResetSave()
        {
            cachedData = null;

            try
            {
                if (File.Exists(SavePath))
                    File.Delete(SavePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LevelProgressService] Reset failed to delete save file: {ex.Message}");
            }

            cachedData = CreateFreshSaveData();
            HeroProgressService.EnsureHeroDefaults(cachedData);
            HeroProgressService.SyncEquippedHeroCache();
            Save(cachedData);
            Global.GlobalEvents.RaiseMetaGoldChanged();
            Global.GlobalEvents.RaiseLobbyReady();
            Debug.Log("[LevelProgressService] Save data reset.");
        }

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        private static LevelProgressData Load()
        {
            if (cachedData != null)
                return cachedData;

            cachedData = CreateFreshSaveData();
            if (!File.Exists(SavePath))
                return cachedData;

            try
            {
                var json = File.ReadAllText(SavePath);
                if (!string.IsNullOrWhiteSpace(json))
                    cachedData = JsonUtility.FromJson<LevelProgressData>(json) ?? CreateFreshSaveData();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LevelProgressService] Load failed: {ex.Message}");
                cachedData = CreateFreshSaveData();
            }

            EnsureStarterMapUnlocked(cachedData);
            EnsureLevelStarsList(cachedData);
            HeroProgressService.EnsureHeroDefaults(cachedData);
            return cachedData;
        }

        private static LevelProgressData CreateFreshSaveData()
        {
            var data = new LevelProgressData { highestUnlockedIndex = 0 };
            EnsureLevelStarsList(data);
            HeroProgressService.EnsureHeroDefaults(data);
            return data;
        }

        private static void EnsureStarterMapUnlocked(LevelProgressData data)
        {
            if (data.highestUnlockedIndex < 0)
                data.highestUnlockedIndex = 0;
        }

        private static void EnsureLevelStarsList(LevelProgressData data)
        {
            if (data.levelStars == null)
                data.levelStars = new List<LevelStarEntry>();
        }

        private static void Save(LevelProgressData data)
        {
            cachedData = data;
            try
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LevelProgressService] Save failed: {ex.Message}");
            }
        }
    }
}
