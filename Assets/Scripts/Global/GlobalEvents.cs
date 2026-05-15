using System;
namespace Global
{
    public static class GlobalEvents
    {
        public static event Action<int> OnDungeonGenerated; // Trả về Seed
        public static event Action OnGameStart;
        public static event Action OnGameOver;

        // --- COMBAT & PROGRESSION ---
        public static event Action OnEnemyDie; // Không cần bool, gọi là biết quái chết
        public static event Action OnPlayerEliminated;
        public static event Action<int> OnExperienceGained; // Trả về lượng Exp
        public static event Action<int> OnLevelUp; // Trả về Level hiện tại

        // --- UI & INTERACTION ---
        public static event Action OnRequestBattleCardUI; // Khi cần hiện bảng chọn thẻ
        public static event Action OnRequestEndGameUI;
        // --- TRIGGER METHODS ---
        // Sử dụng phương thức rút gọn để gọi Event an toàn
        public static void RaiseRequestEndGameUI() => OnRequestEndGameUI?.Invoke();
        public static void RaisePlayerEliminated() => OnPlayerEliminated?.Invoke();
        public static void RaiseRequestBattleCard() => OnRequestBattleCardUI?.Invoke();
        public static void RaiseEnemyDie() => OnEnemyDie?.Invoke();
        public static void RaiseGameStart() => OnGameStart?.Invoke();
        public static void RaiseLevelUp(int level) => OnLevelUp?.Invoke(level);
        public static void RaiseDungeonGenerated(int seed) => OnDungeonGenerated?.Invoke(seed);
    }
}
