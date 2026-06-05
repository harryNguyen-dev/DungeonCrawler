using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Boot scene entry point: show loading UI, load save data, then transition to Lobby.
    /// </summary>
    public sealed class BootController : MonoBehaviour
    {
        [SerializeField] float _minimumLoadingSeconds = 2f;
        [SerializeField] string _nextSceneName = SceneManagerCustom.LobbySceneName;

        private async void Start()
        {
            await RunAsync();
        }

        private async UniTask RunAsync()
        {
            var loading = LoadingManager.Singleton;
            if (loading == null)
            {
                Debug.LogError("[BootController] LoadingManager is missing from the Boost scene.");
            }
            else
            {
                loading.Open(_minimumLoadingSeconds);
            }

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

            GameBootstrap.LoadPersistentData();

            var op = SceneManager.LoadSceneAsync(_nextSceneName);
            if (op != null)
            {
                op.allowSceneActivation = false;
                while (op.progress < 0.9f)
                    await UniTask.Yield();

                op.allowSceneActivation = true;
                await op;
            }
            else
            {
                Debug.LogError($"[BootController] Failed to load scene '{_nextSceneName}'. Add it to Build Settings.");
            }

            loading?.Close();
        }
    }
}
