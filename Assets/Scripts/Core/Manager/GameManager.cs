using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        private bool isGameOver;
        private bool bossKilled;
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

            Core.Save.HeroProgressService.SyncEquippedHeroCache();
            Global.GlobalEvents.RaiseLobbyReady();
        }

        public void ResetMatchState()
        {
            Time.timeScale = 1f;
            isGameOver = false;
            bossKilled = false;
            totalRooms = 0;
            clearedRooms = 0;
            runGold = 0;
            enemiesKilled = 0;

            Global.GlobalEvents.RaiseRunGoldChanged(runGold);

            Global.GlobalVariable.CurrentSeed = 0;
            Global.GlobalVariable.PlayerSpawnPosition = Vector3.zero;
            Global.GlobalVariable.TotalRoomCount = 0;

            if (Global.GlobalEntities.Instance != null)
            {
                Global.GlobalEntities.Instance.ClearRuntimeSceneObjects();
            }

            Global.GlobalEvents.RaiseMatchReset();
            Global.GlobalEvents.RaiseRunStarsChanged(0);
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

        private void HandleEnemyKilled(int _)
        {
            if (isGameOver) return;
            enemiesKilled++;
        }

        public void CollectGold(int baseAmount)
        {
            if (isGameOver || baseAmount <= 0) return;

            var multiplier = Global.GlobalEntities.Instance?.PlayerStats?.runtimeStats?.DefaultGoldGainMultiplier ?? 1f;
            runGold += Mathf.RoundToInt(baseAmount * multiplier);
            Core.GameAudio.PlayGoldPickup();
            Global.GlobalEvents.RaiseRunGoldChanged(runGold);
        }

        private void HandleBossDefeated()
        {
            bossKilled = true;
            RefreshRunStars();
            HandleWin();
        }

        public void HandleWin()
        {
            if (isGameOver) return;
            isGameOver = true;
            Time.timeScale = 0f;
            Debug.Log("BOSS DEFEATED — RUN WON!");
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
            var catalog = entities?.GetChapter1Catalog();
            var clearedIndex = ResolveClearedLevelIndex(catalog, level);

            var starsEarned = LevelStarCalculator.Calculate(clearedRooms, totalRooms, bossKilled, level);
            var bestStars = 0;
            var unlockedNewLevel = false;

            if (level != null)
            {
                Save.LevelProgressService.TryUpdateBestStars(level.levelId, starsEarned, out bestStars);
                TryUnlockNextMap(level, catalog, clearedIndex, starsEarned, out unlockedNewLevel);
            }

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
                TotalMetaGold = totalMeta,
                StarsEarned = starsEarned,
                BestStars = bestStars,
                UnlockedNewLevel = unlockedNewLevel
            };
        }

        private void HandleDungeonGenerated(int seed)
        {
            totalRooms = Global.GlobalVariable.TotalRoomCount;
            clearedRooms = 0;
            bossKilled = false;
            isGameOver = false;

            if (Global.GlobalEntities.Instance != null)
            {
                Global.GlobalEntities.Instance.SpawnPlayer(true);
            }

            RefreshRunStars();
        }

        private void HandleRoomCleared(Vector2Int gridPos)
        {
            clearedRooms++;
            Debug.Log($"Room {clearedRooms} cleared. Stars tracked against total: {totalRooms}");
            RefreshRunStars();
        }

        private void RefreshRunStars()
        {
            var level = Global.GlobalVariable.CurrentLevel;
            var stars = LevelStarCalculator.Calculate(clearedRooms, totalRooms, bossKilled, level);
            Global.GlobalEvents.RaiseRunStarsChanged(stars);
        }

        private static void TryUnlockNextMap(
            SO.LevelSO level,
            SO.LevelCatalogSO catalog,
            int clearedIndex,
            int starsEarned,
            out bool unlockedNewLevel)
        {
            unlockedNewLevel = false;
            var threshold = Save.LevelProgressService.GetUnlockThreshold(level);

            if (catalog == null)
            {
                Debug.LogWarning(
                    $"[GameManager] Chapter1Catalog missing — saved {starsEarned} stars for '{level.levelId}' but map unlock skipped.");
                return;
            }

            if (clearedIndex < 0)
            {
                Debug.LogWarning(
                    $"[GameManager] Could not resolve index for '{level.levelId}' — saved {starsEarned} stars but map unlock skipped.");
                return;
            }

            unlockedNewLevel = Save.LevelProgressService.TryUnlockFromStars(
                clearedIndex, starsEarned, level, catalog.LevelCount);

            if (unlockedNewLevel)
            {
                Debug.Log($"[GameManager] Stars {starsEarned} unlocked next stage after index {clearedIndex}.");
                return;
            }

            if (starsEarned >= threshold)
            {
                Debug.Log(
                    $"[GameManager] Stars {starsEarned} met threshold ({threshold}) for index {clearedIndex}; next map was already unlocked.");
            }
            else
            {
                Debug.Log(
                    $"[GameManager] Stars {starsEarned}/{threshold} on index {clearedIndex} — next map not unlocked yet.");
            }
        }

        private static int ResolveClearedLevelIndex(SO.LevelCatalogSO catalog, SO.LevelSO level)
        {
            var index = Global.GlobalVariable.CurrentLevelIndex;
            if (index >= 0)
                return index;

            if (catalog != null && level != null)
            {
                var catalogIndex = catalog.IndexOf(level);
                if (catalogIndex >= 0)
                    return catalogIndex;
            }

            if (level != null && level.stageIndex >= 1)
                return level.stageIndex - 1;

            return -1;
        }
    }
}
