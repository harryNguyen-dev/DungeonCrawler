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
        public WaveConfigSO combatWaves;
        public BossConfigSO boss;

        [Header("Difficulty Scale")]
        [Tooltip("Nhân thêm lên HP toàn màn (sau spawn entry multiplier).")]
        [Min(0.1f)] public float enemyHealthScale = 1f;

        [Tooltip("Nhân thêm lên damage toàn màn.")]
        [Min(0.1f)] public float enemyDamageScale = 1f;

        public string DisplayLabel => $"Chap {chapter}.{stageIndex}";

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(levelId))
                levelId = $"ch{chapter}_{stageIndex:D2}";
        }
    }
}
