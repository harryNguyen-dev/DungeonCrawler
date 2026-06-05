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
        public int MoveSpeed;
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
        public const int WinBonusGold = 25;

        public static int CalculateMetaGoldGain(int runGold, bool isWin)
        {
            if (isWin)
                return runGold + WinBonusGold;

            return UnityEngine.Mathf.RoundToInt(runGold * LoseMetaRate);
        }
    }
}
