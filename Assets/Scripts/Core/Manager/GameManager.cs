using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        private bool isGameOver;
        private int totalRooms;
        private int clearedRooms;

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

            if (Global.GlobalEntities.Instance != null)
            {
                Global.GlobalEntities.Instance.SpawnPlayer(false);
            }

            Global.GlobalEvents.RaiseLobbyReady();
        }

        public void ResetMatchState()
        {
            Time.timeScale = 1f;
            isGameOver = false;
            totalRooms = 0;
            clearedRooms = 0;

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
            Global.GlobalEvents.OnRoomCleared += HandleRoomCleared;
            Global.GlobalEvents.OnDungeonGenerated += HandleDungeonGenerated;
        }

        private void OnDisable()
        {
            Global.GlobalEvents.OnPlayerEliminated -= HandleLose;
            Global.GlobalEvents.OnRoomCleared -= HandleRoomCleared;
            Global.GlobalEvents.OnDungeonGenerated -= HandleDungeonGenerated;
        }

        public void HandleWin()
        {
            if (isGameOver) return;
            isGameOver = true;
            Time.timeScale = 0f;
            Debug.Log("LEVEL CLEAR!");
            Global.GlobalEvents.RaiseAllRoomsCleared();
            ShowEndScreen(true);
        }

        private void HandleLose()
        {
            if (isGameOver) return;
            isGameOver = true;

            Debug.Log("GAME OVER!");
            ShowEndScreen(false);
        }

        private void ShowEndScreen(bool isWin)
        {
            Time.timeScale = 0f;
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
            Debug.Log($"Room {clearedRooms + 1} cleared! {totalRooms - clearedRooms} rooms left. And total room is {totalRooms}");
            if (++clearedRooms >= totalRooms)
            {
                HandleWin();
            }
        }
    }
}
