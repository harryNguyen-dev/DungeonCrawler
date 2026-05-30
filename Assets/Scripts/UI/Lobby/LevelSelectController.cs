using Core;
using Core.Save;
using Global;
using SO;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUI.Lobby
{
    [RequireComponent(typeof(UIDocument))]
    public class LevelSelectController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement selectRoot;
        private ScrollView levelScroll;
        private Button closeButton;
        private Label progressLabel;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null)
                uiDocument.sortingOrder = 50;
        }

        private void OnEnable()
        {
            GlobalEvents.OnRequestLevelSelectUI += ShowPanel;
            GlobalEvents.OnLobbyReady += HandleLobbyReady;
            CacheElements();
            HidePanel();
        }

        private void OnDisable()
        {
            GlobalEvents.OnRequestLevelSelectUI -= ShowPanel;
            GlobalEvents.OnLobbyReady -= HandleLobbyReady;
        }

        private void CacheElements()
        {
            var root = uiDocument?.rootVisualElement;
            if (root == null) return;

            selectRoot = root.Q<VisualElement>("level-select-root");
            levelScroll = root.Q<ScrollView>("level-scroll");
            closeButton = root.Q<Button>("close-button");
            progressLabel = root.Q<Label>("progress-label");
            closeButton?.RegisterCallback<ClickEvent>(_ => HidePanel());
        }

        private void HandleLobbyReady()
        {
            if (!GlobalVariable.OpenLevelSelectOnLobbyReturn)
                return;

            GlobalVariable.OpenLevelSelectOnLobbyReturn = false;
            ShowPanel();
        }

        private void ShowPanel()
        {
            if (selectRoot == null || levelScroll == null)
                CacheElements();

            if (levelScroll == null) return;

            levelScroll.Clear();
            var catalog = GlobalEntities.Instance?.Chapter1Catalog;
            if (catalog == null)
            {
                Debug.LogWarning("[LevelSelect] Chapter1Catalog chưa gán trên GlobalEntities.");
                return;
            }

            var unlockedIndex = LevelProgressService.GetHighestUnlockedIndex(catalog.LevelCount);
            var unlockedCount = unlockedIndex + 1;
            if (progressLabel != null)
                progressLabel.text = $"Unlocked {unlockedCount}/{catalog.LevelCount}";

            for (var i = 0; i < catalog.LevelCount; i++)
            {
                var level = catalog.GetLevel(i);
                if (level == null) continue;

                var isUnlocked = i <= unlockedIndex;
                var isNew = isUnlocked && i == unlockedIndex && unlockedIndex > 0;
                levelScroll.Add(BuildLevelRow(level, i, isUnlocked, isNew));
            }

            if (selectRoot != null)
                selectRoot.style.display = DisplayStyle.Flex;
        }

        private VisualElement BuildLevelRow(LevelSO level, int index, bool isUnlocked, bool isNew)
        {
            var row = new VisualElement();
            row.AddToClassList("level-row");
            if (!isUnlocked)
                row.AddToClassList("level-row--locked");
            else if (isNew)
                row.AddToClassList("level-row--next");

            var label = new Label(level.DisplayLabel);
            label.AddToClassList("level-label");

            var statusText = !isUnlocked ? "LOCKED" : isNew ? "NEW" : "PLAY";
            var status = new Label(statusText);
            status.AddToClassList("level-status");

            row.Add(label);
            row.Add(status);

            if (isUnlocked)
            {
                row.RegisterCallback<ClickEvent>(_ => OnLevelSelected(level, index));
            }

            return row;
        }

        private void OnLevelSelected(LevelSO level, int index)
        {
            GlobalVariable.CurrentLevel = level;
            GlobalVariable.CurrentLevelIndex = index;
            HidePanel();
            SceneManagerCustom.LoadDungeon();
        }

        private void HidePanel()
        {
            if (selectRoot != null)
                selectRoot.style.display = DisplayStyle.None;
        }
    }
}
