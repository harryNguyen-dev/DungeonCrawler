using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "BossConfig", menuName = "Level/Boss Config")]
    public class BossConfigSO : ScriptableObject
    {
        [Header("Boss")]
        public GameObject bossPrefab;

        [Tooltip("Nhân HP khi prefab chưa có EnemySO.isBoss.")]
        [Min(0.1f)] public float healthMultiplier = 1f;

        [Tooltip("Delay trước khi spawn boss (ms).")]
        [Min(0)] public int spawnDelayMs = 500;
    }
}
