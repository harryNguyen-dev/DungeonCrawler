namespace Core
{
    /// <summary>Snapshot stats shown on the run end screen.</summary>
    public class RunSummary
    {
        public bool IsWin;
        public string LevelLabel;

        public int PlayerLevel;
        public int CurrentHealth;
        public int MaxHealth;
        public int AttackDamage;
        public float AttackCooldown;
        public float MoveSpeed;
        public int Armor;

        public int EnemiesKilled;
        public int RoomsCleared;
        public int TotalRooms;

        public int RunGold;
        public int MetaGoldGained;
        public int TotalMetaGold;

        public int StarsEarned;
        public int BestStars;
        public bool UnlockedNewLevel;
    }

    public static class RunEconomy
    {
        public const float LoseMetaRate = 0.3f;

        public static int GetWinClearBonus(int starsEarned)
        {
            return starsEarned switch
            {
                >= 3 => 100,
                2 => 60,
                1 => 30,
                _ => 0
            };
        }

        public static int CalculateMetaGoldGain(int runGold, bool isWin, int starsEarned = 0)
        {
            if (isWin)
                return runGold + GetWinClearBonus(starsEarned);

            return UnityEngine.Mathf.RoundToInt(runGold * LoseMetaRate);
        }
    }
}
