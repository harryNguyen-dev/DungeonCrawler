using System;

namespace Global
{
    public static class GlobalEvents
    {
        public static event Action<int> OnDungeonGenerated;
        public static event Action OnGameStart;
        public static event Action OnGameOver;
        public static event Action OnPlayerJoin;
        public static event Action OnLobbyReady;
        public static event Action OnDungeonSceneLoaded;
        public static event Action OnMatchReset;

        public static event Action OnEnemyDie;
        public static event Action OnPlayerEliminated;
        public static event Action<int> OnExperienceGained;
        public static event Action<int> OnLevelUp;
        public static event Action OnRoomCleared;
        public static event Action OnAllRoomsCleared;

        public static event Action OnRequestBattleCardUI;
        public static event Action OnRequestEndGameUI;

        public static void RaiseRequestEndGameUI() => OnRequestEndGameUI?.Invoke();
        public static void RaisePlayerEliminated() => OnPlayerEliminated?.Invoke();
        public static void RaiseRequestBattleCard() => OnRequestBattleCardUI?.Invoke();
        public static void RaiseEnemyDie() => OnEnemyDie?.Invoke();
        public static void RaiseGameStart() => OnGameStart?.Invoke();
        public static void RaiseLevelUp(int level) => OnLevelUp?.Invoke(level);
        public static void RaiseDungeonGenerated(int seed) => OnDungeonGenerated?.Invoke(seed);
        public static void RaiseGameOver() => OnGameOver?.Invoke();
        public static void RaisePlayerJoin() => OnPlayerJoin?.Invoke();
        public static void RaiseLobbyReady() => OnLobbyReady?.Invoke();
        public static void RaiseDungeonSceneLoaded() => OnDungeonSceneLoaded?.Invoke();
        public static void RaiseMatchReset() => OnMatchReset?.Invoke();
        public static void RaiseRoomCleared() => OnRoomCleared?.Invoke();
        public static void RaiseAllRoomsCleared() => OnAllRoomsCleared?.Invoke();
    }
}
