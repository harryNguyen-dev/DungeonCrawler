using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Level/Level Catalog")]
    public class LevelCatalogSO : ScriptableObject
    {
        public int chapter = 1;
        public List<LevelSO> levels = new();

        public int LevelCount => levels?.Count ?? 0;

        public LevelSO GetLevel(int index)
        {
            if (levels == null || index < 0 || index >= levels.Count)
                return null;

            return levels[index];
        }

        public int IndexOf(LevelSO level)
        {
            if (levels == null || level == null)
                return -1;

            return levels.IndexOf(level);
        }
    }
}
