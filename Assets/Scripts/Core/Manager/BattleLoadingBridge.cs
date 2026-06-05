using Global;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Drives the persistent LoadingManager during battle scene load and WFC dungeon generation.
    /// </summary>
    public sealed class BattleLoadingBridge : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] float _sceneLoadProgressMax = 0.25f;

        private void OnEnable()
        {
            GlobalEvents.OnDungeonSceneLoaded += HandleDungeonSceneLoaded;
            GlobalEvents.OnDungeonGenerationProgress += HandleGenerationProgress;
            GlobalEvents.OnDungeonGenerated += HandleDungeonGenerated;
            GlobalEvents.OnMatchReset += HandleMatchReset;
        }

        private void OnDisable()
        {
            GlobalEvents.OnDungeonSceneLoaded -= HandleDungeonSceneLoaded;
            GlobalEvents.OnDungeonGenerationProgress -= HandleGenerationProgress;
            GlobalEvents.OnDungeonGenerated -= HandleDungeonGenerated;
            GlobalEvents.OnMatchReset -= HandleMatchReset;
        }

        private void HandleDungeonSceneLoaded()
        {
            var loading = LoadingManager.Singleton;
            if (loading == null) return;

            loading.SetStatus("Preparing dungeon");
            loading.SetProgress(_sceneLoadProgressMax);
        }

        private void HandleGenerationProgress(float progress)
        {
            var loading = LoadingManager.Singleton;
            if (loading == null || !loading.isLoading) return;

            var scaled = _sceneLoadProgressMax + (1f - _sceneLoadProgressMax) * Mathf.Clamp01(progress);
            loading.SetProgress(scaled);
            loading.SetStatus("Generating dungeon");
        }

        private void HandleDungeonGenerated(int _)
        {
            var loading = LoadingManager.Singleton;
            if (loading == null) return;

            loading.SetProgress(1f);
            loading.Close();
        }

        private void HandleMatchReset()
        {
            LoadingManager.Singleton?.Close();
        }
    }
}
