using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public static class SceneManagerCustom
    {
        public const string BoostSceneName = "Boost";
        public const string LobbySceneName = "Lobby";
        public const string DungeonSceneName = "BattleScene";
        public const string TestBattleSceneName = "Test";
        public const string DungeonGenTestSceneName = "DungeonGenTest";

        const float BattleSceneLoadProgressMax = 0.25f;

        public static void LoadScene(string sceneName)
        {
            if (Global.GlobalEntities.Instance != null)
                Global.GlobalEntities.Instance.ClearRuntimeSceneObjects();

            SceneManager.LoadScene(sceneName);
        }

        public static void LoadLobby()
        {
            LoadScene(LobbySceneName);
        }

        public static void LoadDungeon()
        {
            LoadBattleScene(DungeonSceneName, "Loading battle");
        }

        public static void LoadTestBattle()
        {
            LoadBattleScene(TestBattleSceneName, "Loading test battle");
        }

        public static void ReloadDungeon()
        {
            LoadDungeon();
        }

        static void LoadBattleScene(string sceneName, string loadingStatus)
        {
            if (GameManager.Instance != null && LoadingManager.Singleton != null)
                GameManager.Instance.StartCoroutine(LoadBattleSceneRoutine(sceneName, loadingStatus));
            else
                LoadScene(sceneName);
        }

        static IEnumerator LoadBattleSceneRoutine(string sceneName, string loadingStatus)
        {
            var loading = LoadingManager.Singleton;
            loading.OpenManual(loadingStatus);

            if (Global.GlobalEntities.Instance != null)
                Global.GlobalEntities.Instance.ClearRuntimeSceneObjects();

            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogError($"[SceneManagerCustom] Failed to load scene '{sceneName}'.");
                loading.Close();
                yield break;
            }

            op.allowSceneActivation = false;
            while (op.progress < 0.9f)
            {
                loading.SetProgress(Mathf.Clamp01(op.progress / 0.9f) * BattleSceneLoadProgressMax);
                yield return null;
            }

            loading.SetProgress(BattleSceneLoadProgressMax);
            op.allowSceneActivation = true;

            while (!op.isDone)
                yield return null;
        }
    }
}
