using System;
using UnityEngine;

namespace Global
{
    public struct RoomEnteredInfo
    {
        public Vector2Int GridPosition;
        public RoomType RoomType;
        public bool IsBossRoom;
    }

    public static class GlobalEvents
    {
        public static event Action<int> OnDungeonGenerated;
        public static event Action OnGameStart;
        public static event Action OnGameOver;
        public static event Action OnPlayerJoin;
        public static event Action OnLobbyReady;
        public static event Action OnDungeonSceneLoaded;
        public static event Action OnMatchReset;

        public static event Action<int> OnEnemyDie;
        public static event Action OnPlayerEliminated;
        public static event Action<int> OnExperienceGained;
        public static event Action<int> OnLevelUp;
        public static event Action<Vector2Int> OnRoomCleared;
        public static event Action<RoomEnteredInfo> OnRoomEntered;
        public static event Action OnAllRoomsCleared;
        public static event Action OnBossDefeated;

        public static event Action OnRequestBattleCardUI;
        public static event Action OnBattleCardUIDismissed;
        public static event Action<Core.RunSummary> OnRequestEndGameUI;
        public static event Action OnRequestLevelSelectUI;
        public static event Action OnRequestHeroLoadoutUI;
        public static event Action OnMetaGoldChanged;
        public static event Action OnSaveReset;
        public static event Action<int> OnRunGoldChanged;
        public static event Action<int> OnRunStarsChanged;

        public static event Action OnDungeonGenerationStarted;
        public static event Action<float> OnDungeonGenerationProgress;

        public static void RaiseRequestEndGameUI(Core.RunSummary summary) => OnRequestEndGameUI?.Invoke(summary);
        public static void RaiseRequestLevelSelectUI() => OnRequestLevelSelectUI?.Invoke();
        public static void RaiseRequestHeroLoadoutUI() => OnRequestHeroLoadoutUI?.Invoke();
        public static void RaiseMetaGoldChanged() => OnMetaGoldChanged?.Invoke();
        public static void RaiseSaveReset() => OnSaveReset?.Invoke();
        public static void RaiseRunGoldChanged(int totalRunGold) => OnRunGoldChanged?.Invoke(totalRunGold);
        public static void RaiseRunStarsChanged(int stars) => OnRunStarsChanged?.Invoke(stars);
        public static void RaiseDungeonGenerationStarted() => OnDungeonGenerationStarted?.Invoke();
        public static void RaiseDungeonGenerationProgress(float progress)
        {
            if (progress < 0f) progress = 0f;
            else if (progress > 1f) progress = 1f;
            OnDungeonGenerationProgress?.Invoke(progress);
        }
        public static void RaisePlayerEliminated() => OnPlayerEliminated?.Invoke();
        public static void RaiseRequestBattleCard() => OnRequestBattleCardUI?.Invoke();
        public static void RaiseBattleCardUIDismissed() => OnBattleCardUIDismissed?.Invoke();
        public static void RaiseEnemyDie(int goldDropped = 0) => OnEnemyDie?.Invoke(goldDropped);
        public static void RaiseGameStart() => OnGameStart?.Invoke();
        public static void RaiseLevelUp(int level) => OnLevelUp?.Invoke(level);
        public static void RaiseDungeonGenerated(int seed) => OnDungeonGenerated?.Invoke(seed);
        public static void RaiseGameOver() => OnGameOver?.Invoke();
        public static void RaisePlayerJoin() => OnPlayerJoin?.Invoke();
        public static void RaiseLobbyReady() => OnLobbyReady?.Invoke();
        public static void RaiseDungeonSceneLoaded() => OnDungeonSceneLoaded?.Invoke();
        public static void RaiseMatchReset() => OnMatchReset?.Invoke();
        public static void RaiseRoomCleared(Vector2Int gridPos) => OnRoomCleared?.Invoke(gridPos);
        public static void RaiseRoomEntered(RoomEnteredInfo info) => OnRoomEntered?.Invoke(info);
        public static void RaiseAllRoomsCleared() => OnAllRoomsCleared?.Invoke();
        public static void RaiseBossDefeated() => OnBossDefeated?.Invoke();
    }
}
