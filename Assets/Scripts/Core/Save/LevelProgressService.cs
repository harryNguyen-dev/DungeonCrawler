using System;
using System.IO;
using UnityEngine;

namespace Core.Save
{
    [Serializable]
    public class LevelProgressData
    {
        public int highestUnlockedIndex;
        public int metaGold;

        public System.Collections.Generic.List<string> unlockedWeaponIds;
        public string equippedWeaponId;
        public System.Collections.Generic.List<WeaponUpgradeEntry> weaponUpgrades;
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
            if (levelIndex < 0 || catalogLevelCount <= 0)
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

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        private static LevelProgressData Load()
        {
            if (cachedData != null)
                return cachedData;

            cachedData = new LevelProgressData();
            if (!File.Exists(SavePath))
                return cachedData;

            try
            {
                var json = File.ReadAllText(SavePath);
                if (!string.IsNullOrWhiteSpace(json))
                    cachedData = JsonUtility.FromJson<LevelProgressData>(json) ?? new LevelProgressData();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LevelProgressService] Load failed: {ex.Message}");
                cachedData = new LevelProgressData();
            }

            return cachedData;
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
