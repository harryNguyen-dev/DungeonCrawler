using SO;
using UnityEngine;

namespace Core
{
    public static class LevelStarCalculator
    {
        public static int Calculate(int clearedRooms, int totalRooms, bool bossKilled, LevelSO level)
        {
            if (clearedRooms < 1)
                return 0;

            var oneStarMin = level != null ? Mathf.Max(1, level.oneStarMinRooms) : 1;
            var twoStarRatio = level != null ? Mathf.Clamp(level.twoStarRoomRatio, 0.05f, 1f) : 0.5f;

            if (clearedRooms < oneStarMin)
                return 0;

            var stars = 1;
            var twoStarTarget = Mathf.CeilToInt(totalRooms * twoStarRatio);
            if (clearedRooms >= twoStarTarget)
                stars = 2;

            if (bossKilled)
                stars = 3;

            return stars;
        }
    }
}
