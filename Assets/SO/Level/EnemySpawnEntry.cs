using System;
using UnityEngine;

namespace SO
{
    [Serializable]
    public class EnemySpawnEntry
    {
        public GameObject prefab;
        [Min(1)] public int weight = 1;

        [Tooltip("Nhân HP runtime (1 = giữ nguyên EnemySO trên prefab).")]
        [Min(0.1f)] public float healthMultiplier = 1f;

        [Tooltip("Nhân damage runtime (1 = giữ nguyên EnemySO trên prefab).")]
        [Min(0.1f)] public float damageMultiplier = 1f;
    }
}
