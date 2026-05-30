using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "Level/Wave Config")]
    public class WaveConfigSO : ScriptableObject
    {
        [Header("Wave")]
        [Min(1)] public int waveCount = 2;
        [Min(1)] public int enemiesPerWave = 3;

        [Header("Timing (ms)")]
        [Min(0)] public int spawnDelayMs = 500;
        [Min(0)] public int delayBetweenWavesMs = 500;
        [Min(0)] public int roomEnterDelayMs = 500;

        [Header("Enemy Pool")]
        public List<EnemySpawnEntry> enemyPool = new();

        public EnemySpawnEntry PickRandomEntry()
        {
            if (enemyPool == null || enemyPool.Count == 0)
                return null;

            var totalWeight = 0;
            foreach (var entry in enemyPool)
            {
                if (entry?.prefab != null)
                    totalWeight += entry.weight;
            }

            if (totalWeight <= 0)
                return null;

            var roll = Random.Range(0, totalWeight);
            var accumulated = 0;
            foreach (var entry in enemyPool)
            {
                if (entry?.prefab == null) continue;

                accumulated += entry.weight;
                if (roll < accumulated)
                    return entry;
            }

            return enemyPool[0];
        }
    }
}
