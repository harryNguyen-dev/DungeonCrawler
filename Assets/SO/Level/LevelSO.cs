using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "Level", menuName = "Level/Level Config")]
    public class LevelSO : ScriptableObject
    {
        [Header("Identity")]
        public int chapter = 1;
        [Min(1)] public int stageIndex = 1;
        public string levelId = "ch1_01";

        [Header("Dungeon")]
        public int wfcSeed;

        [Tooltip("0 = dùng roomsToPlace mặc định trên WFCGeneration.")]
        [Min(0)] public int roomsToPlaceOverride;

        [Header("Encounters")]
        public List<WaveConfigSO> combatWavePool = new();
        public List<WaveConfigSO> hallwayWavePool = new();
        [Range(0f, 1f)] public float hallwaySpawnChance = 0.4f;
        public BossConfigSO boss;

        [Header("Difficulty Scale")]
        [Tooltip("Nhân thêm lên HP toàn màn (sau spawn entry multiplier).")]
        [Min(0.1f)] public float enemyHealthScale = 1f;

        [Tooltip("Nhân thêm lên damage toàn màn.")]
        [Min(0.1f)] public float enemyDamageScale = 1f;

        public string DisplayLabel => $"Chap {chapter}.{stageIndex}";

        public WaveConfigSO PickCombatWave(int seedSalt) => PickFromPool(combatWavePool, seedSalt);

        public WaveConfigSO PickHallwayWave(int seedSalt) => PickFromPool(hallwayWavePool, seedSalt);

        private static WaveConfigSO PickFromPool(List<WaveConfigSO> pool, int seedSalt)
        {
            if (pool == null || pool.Count == 0)
                return null;

            var valid = new List<WaveConfigSO>();
            foreach (var w in pool)
            {
                if (w != null)
                    valid.Add(w);
            }

            if (valid.Count == 0)
                return null;

            var seed = Global.GlobalVariable.CurrentSeed ^ seedSalt;
            var index = new System.Random(seed).Next(0, valid.Count);
            return valid[index];
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(levelId))
                levelId = $"ch{chapter}_{stageIndex:D2}";
        }
    }
}
