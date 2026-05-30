using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        private bool isGameOver;
        private int totalRooms;
        private int clearedRooms;
        private int runGold;
        private int enemiesKilled;

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        public void SetupTestBattleScene()
        {
            ResetMatchState();
            Global.GlobalVariable.CurrentGameMode = Global.GameMode.InMatch;
            Global.GlobalVariable.PlayerSpawnPosition = Vector3.zero;

            if (Global.GlobalEntities.Instance != null)
            {
                Global.GlobalEntities.Instance.SpawnPlayer(true);
            }

            Global.GlobalEvents.RaiseGameStart();
        }
        public void SetupBattleScene()
        {
            ResetMatchState();
            Global.GlobalVariable.CurrentGameMode = Global.GameMode.InMatch;
            Global.GlobalEvents.RaiseDungeonSceneLoaded();
            Global.GlobalEvents.RaiseGameStart();
        }

        public void SetupLobbyScene()
        {
            ResetMatchState();
            Global.GlobalVariable.CurrentGameMode = Global.GameMode.Lobby;
            Global.GlobalVariable.PlayerSpawnPosition = Vector3.zero;
            Global.GlobalVariable.CurrentLevel = null;
            Global.GlobalVariable.CurrentLevelIndex = -1;

            if (Global.GlobalEntities.Instance != null)
            {
                Global.GlobalEntities.Instance.SpawnPlayer(false);
            }

            Core.Save.WeaponProgressService.SyncEquippedWeaponCache();
            Global.GlobalEvents.RaiseLobbyReady();
        }

        public void ResetMatchState()
        {
            Time.timeScale = 1f;
            isGameOver = false;
            totalRooms = 0;
            clearedRooms = 0;
            runGold = 0;
            enemiesKilled = 0;

            Global.GlobalVariable.CurrentSeed = 0;
            Global.GlobalVariable.PlayerSpawnPosition = Vector3.zero;
            Global.GlobalVariable.TotalRoomCount = 0;

            if (Global.GlobalEntities.Instance != null)
            {
                Global.GlobalEntities.Instance.ClearRuntimeSceneObjects();
            }

            Global.GlobalEvents.RaiseMatchReset();
        }

        private void OnEnable()
        {
            Global.GlobalEvents.OnPlayerEliminated += HandleLose;
            Global.GlobalEvents.OnBossDefeated += HandleBossDefeated;
            Global.GlobalEvents.OnRoomCleared += HandleRoomCleared;
            Global.GlobalEvents.OnDungeonGenerated += HandleDungeonGenerated;
            Global.GlobalEvents.OnEnemyDie += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            Global.GlobalEvents.OnPlayerEliminated -= HandleLose;
            Global.GlobalEvents.OnBossDefeated -= HandleBossDefeated;
            Global.GlobalEvents.OnRoomCleared -= HandleRoomCleared;
            Global.GlobalEvents.OnDungeonGenerated -= HandleDungeonGenerated;
            Global.GlobalEvents.OnEnemyDie -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(int goldDropped)
        {
            if (isGameOver) return;

            enemiesKilled++;
            var multiplier = Global.GlobalEntities.Instance?.PlayerStats?.runtimeStats?.DefaultGoldGainMultiplier ?? 1f;
            runGold += Mathf.RoundToInt(goldDropped * multiplier);
        }

        private void HandleBossDefeated()
        {
            HandleWin();
        }

        public void HandleWin()
        {
            if (isGameOver) return;
            isGameOver = true;
            Time.timeScale = 0f;
            Debug.Log("BOSS DEFEATED — RUN WON!");

            var catalog = Global.GlobalEntities.Instance?.Chapter1Catalog;
            var clearedIndex = ResolveClearedLevelIndex(catalog);
            if (catalog != null && clearedIndex >= 0)
            {
                var previousUnlocked = Save.LevelProgressService.GetHighestUnlockedIndex(catalog.LevelCount);
                Save.LevelProgressService.UnlockNextAfter(clearedIndex, catalog.LevelCount);
                var newUnlocked = Save.LevelProgressService.GetHighestUnlockedIndex(catalog.LevelCount);
                Debug.Log($"[GameManager] Cleared stage index {clearedIndex}. Unlocked {previousUnlocked} -> {newUnlocked}.");
            }
            else
            {
                Debug.LogWarning("[GameManager] Win but level progress not saved (missing catalog or level index).");
            }

            ShowEndScreen(true);
        }

        private void HandleLose()
        {
            if (isGameOver) return;
            isGameOver = true;
            Time.timeScale = 0f;

            Debug.Log("GAME OVER!");
            ShowEndScreen(false);
        }

        private void ShowEndScreen(bool isWin)
        {
            Time.timeScale = 0f;
            var summary = BuildRunSummary(isWin);
            Global.GlobalEvents.RaiseRequestEndGameUI(summary);
            Global.GlobalEvents.RaiseGameOver();
        }

        private RunSummary BuildRunSummary(bool isWin)
        {
            var entities = Global.GlobalEntities.Instance;
            var stats = entities?.PlayerStats;
            var health = entities?.PlayerHealth;
            var level = Global.GlobalVariable.CurrentLevel;

            var metaGain = RunEconomy.CalculateMetaGoldGain(runGold, isWin);
            var totalMeta = Save.LevelProgressService.AddMetaGold(metaGain);

            return new RunSummary
            {
                IsWin = isWin,
                LevelLabel = level != null ? level.DisplayLabel : "Run",
                PlayerLevel = stats != null ? stats.currentLevel : 1,
                CurrentHealth = health != null ? health.GetCurrentHealth() : 0,
                MaxHealth = stats != null ? stats.GetMaxHealth() : 0,
                AttackDamage = stats != null ? stats.GetAttackDamage() : 0,
                AttackCooldown = stats != null ? stats.GetAttackCooldown() : 0f,
                MoveSpeed = stats != null ? stats.GetMoveSpeed() : 0,
                Armor = stats?.runtimeStats != null ? stats.runtimeStats.Amor : 0,
                EnemiesKilled = enemiesKilled,
                RoomsCleared = clearedRooms,
                TotalRooms = totalRooms,
                RunGold = runGold,
                MetaGoldGained = metaGain,
                TotalMetaGold = totalMeta
            };
        }

        private void HandleDungeonGenerated(int seed)
        {
            totalRooms = Global.GlobalVariable.TotalRoomCount;
            clearedRooms = 0;
            isGameOver = false;

            if (Global.GlobalEntities.Instance != null)
            {
                Global.GlobalEntities.Instance.SpawnPlayer(true);
            }
        }

        private void HandleRoomCleared()
        {
            clearedRooms++;
            Debug.Log($"Room {clearedRooms} cleared (stats only; win requires boss). Total tracked: {totalRooms}");
        }

        private static int ResolveClearedLevelIndex(SO.LevelCatalogSO catalog)
        {
            var index = Global.GlobalVariable.CurrentLevelIndex;
            if (index >= 0)
                return index;

            if (catalog == null || Global.GlobalVariable.CurrentLevel == null)
                return -1;

            return catalog.IndexOf(Global.GlobalVariable.CurrentLevel);
        }
    }
}
