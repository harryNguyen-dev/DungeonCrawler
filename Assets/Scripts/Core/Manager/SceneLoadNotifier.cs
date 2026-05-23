using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class SceneLoadNotifier : MonoBehaviour
    {
        private bool handledInitialScene;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            if (handledInitialScene) return;
            handledInitialScene = true;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"Scene loaded: {scene.name}");
            Debug.Log($"Load mode: {mode}");

            if (GameManager.Instance == null)
            {
                Debug.LogWarning("[SceneLoadNotifier] GameManager is missing.");
                return;
            }

            if (scene.name == SceneManagerCustom.DungeonSceneName)
            {
                GameManager.Instance.SetupBattleScene();
            }
            else if (scene.name == SceneManagerCustom.LobbySceneName)
            {
                GameManager.Instance.SetupLobbyScene();
            }
            else if (scene.name == SceneManagerCustom.TestBattleSceneName)
            {
                GameManager.Instance.SetupTestBattleScene();
            }
        }
    }
}
