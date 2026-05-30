namespace Core
{
    public static class SceneManagerCustom
    {
        public const string LobbySceneName = "Lobby";
        public const string DungeonSceneName = "BattleScene";
        public const string TestBattleSceneName = "Test";

        public static void LoadScene(string sceneName)
        {
            if (Global.GlobalEntities.Instance != null)
                Global.GlobalEntities.Instance.ClearRuntimeSceneObjects();

            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        public static void LoadLobby()
        {
            LoadScene(LobbySceneName);
        }

        public static void LoadDungeon()
        {
            LoadScene(DungeonSceneName);
        }

        public static void LoadTestBattle()
        {
            LoadScene(TestBattleSceneName);
        }

        public static void ReloadDungeon()
        {
            LoadDungeon();
        }
    }
}
