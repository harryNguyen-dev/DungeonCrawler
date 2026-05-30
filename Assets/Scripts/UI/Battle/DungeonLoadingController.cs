using Global;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUI.Battle
{
    [RequireComponent(typeof(UIDocument))]
    public class DungeonLoadingController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement loadingRoot;
        private VisualElement progressFill;
        private Label statusLabel;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null)
                uiDocument.sortingOrder = 200;
        }

        private void OnEnable()
        {
            GlobalEvents.OnDungeonSceneLoaded += ShowLoading;
            GlobalEvents.OnDungeonGenerationStarted += ShowLoading;
            GlobalEvents.OnDungeonGenerationProgress += UpdateProgress;
            GlobalEvents.OnDungeonGenerated += HandleDungeonGenerated;
            GlobalEvents.OnMatchReset += HideLoading;

            CacheElements();
            HideLoading();
        }

        private void OnDisable()
        {
            GlobalEvents.OnDungeonSceneLoaded -= ShowLoading;
            GlobalEvents.OnDungeonGenerationStarted -= ShowLoading;
            GlobalEvents.OnDungeonGenerationProgress -= UpdateProgress;
            GlobalEvents.OnDungeonGenerated -= HandleDungeonGenerated;
            GlobalEvents.OnMatchReset -= HideLoading;
        }

        private void CacheElements()
        {
            var root = uiDocument?.rootVisualElement;
            if (root == null) return;

            loadingRoot = root.Q<VisualElement>("loading-root");
            progressFill = root.Q<VisualElement>("loading-fill");
            statusLabel = root.Q<Label>("loading-status");
        }

        private void ShowLoading()
        {
            if (loadingRoot == null)
                CacheElements();

            if (loadingRoot != null)
                loadingRoot.style.display = DisplayStyle.Flex;

            UpdateProgress(0f);
        }

        private void UpdateProgress(float progress)
        {
            if (loadingRoot == null)
                CacheElements();

            if (progressFill != null)
                progressFill.style.width = Length.Percent(progress * 100f);

            if (statusLabel != null)
                statusLabel.text = $"Generating dungeon... {Mathf.RoundToInt(progress * 100f)}%";
        }

        private void HandleDungeonGenerated(int _) => HideLoading();

        private void HideLoading()
        {
            if (loadingRoot == null)
                CacheElements();

            if (loadingRoot != null)
                loadingRoot.style.display = DisplayStyle.None;
        }
    }
}
