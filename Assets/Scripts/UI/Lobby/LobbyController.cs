using System.Globalization;
using Core;
using Core.Save;
using CustomUI.SciFi;
using Global;
using SO;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomUI.Lobby
{
    /// <summary>Main lobby screen (UI Toolkit).</summary>
    [RequireComponent(typeof(UIDocument))]
    public class LobbyController : MonoBehaviour
    {
        private UIDocument uiDocument;

        private Button continueButton;
        private Button levelsButton;
        private Button heroButton;
        private Button creditButton;
        private Button settingsButton;
        private Button exitButton;

        private Label continueLabel;
        private Label metaGoldValueLabel;

        private EventCallback<ClickEvent> onContinueClick;
        private EventCallback<ClickEvent> onLevelsClick;
        private EventCallback<ClickEvent> onExitClick;
        private EventCallback<ClickEvent> onHeroClick;
        private EventCallback<ClickEvent> onSettingsClick;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null)
                uiDocument.sortingOrder = 10;
        }

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            CacheElements();
            SciFiUiHelper.StyleLobbyUi(uiDocument?.rootVisualElement);
            BindCallbacks();
            RefreshLobbyDisplay();

            GlobalEvents.OnLobbyReady += HandleLobbyReady;
            GlobalEvents.OnMetaGoldChanged += HandleLobbyReady;
        }

        private void OnDisable()
        {
            UnbindCallbacks();
            GlobalEvents.OnLobbyReady -= HandleLobbyReady;
            GlobalEvents.OnMetaGoldChanged -= HandleLobbyReady;
        }

        private void CacheElements()
        {
            var root = uiDocument?.rootVisualElement;
            if (root == null)
                return;

            continueButton = root.Q<Button>("continue-button");
            levelsButton = root.Q<Button>("levels-button");
            heroButton = root.Q<Button>("hero-button");
            creditButton = root.Q<Button>("credit-button");
            settingsButton = root.Q<Button>("settings-button");
            exitButton = root.Q<Button>("exit-button");

            continueLabel = root.Q<Label>("continue-label");
            metaGoldValueLabel = root.Q<Label>("meta-gold-value-label");
        }

        private void BindCallbacks()
        {
            onContinueClick = _ => OnContinueClicked();
            onLevelsClick = _ => GlobalEvents.RaiseRequestLevelSelectUI();
            onExitClick = _ => ExitGame();
            onHeroClick = _ => GlobalEvents.RaiseRequestHeroLoadoutUI();
            onSettingsClick = _ => Debug.Log("[Lobby] Settings — coming soon.");

            continueButton?.RegisterCallback(onContinueClick);
            levelsButton?.RegisterCallback(onLevelsClick);
            exitButton?.RegisterCallback(onExitClick);
            heroButton?.RegisterCallback(onHeroClick);
            settingsButton?.RegisterCallback(onSettingsClick);
        }

        private void UnbindCallbacks()
        {
            continueButton?.UnregisterCallback(onContinueClick);
            levelsButton?.UnregisterCallback(onLevelsClick);
            exitButton?.UnregisterCallback(onExitClick);
            heroButton?.UnregisterCallback(onHeroClick);
            settingsButton?.UnregisterCallback(onSettingsClick);
        }

        private void HandleLobbyReady()
        {
            RefreshLobbyDisplay();
        }

        private void RefreshLobbyDisplay()
        {
            if (metaGoldValueLabel != null)
                metaGoldValueLabel.text = FormatResourceAmount(LevelProgressService.GetMetaGold());

            var catalog = GlobalEntities.Instance?.Chapter1Catalog;
            if (catalog == null || catalog.LevelCount == 0)
            {
                if (continueLabel != null)
                    continueLabel.text = "CONTINUE (LEVEL —)";
                continueButton?.SetEnabled(false);
                return;
            }

            continueButton?.SetEnabled(true);

            var unlockedIndex = LevelProgressService.GetHighestUnlockedIndex(catalog.LevelCount);
            var level = catalog.GetLevel(unlockedIndex);
            if (continueLabel != null)
            {
                var levelName = level != null ? level.DisplayLabel : $"Level {unlockedIndex + 1}";
                continueLabel.text = $"CONTINUE ({levelName.ToUpperInvariant()})";
            }

            heroButton?.SetEnabled(true);
            levelsButton?.SetEnabled(true);
            creditButton?.SetEnabled(true);
            settingsButton?.SetEnabled(true);
        }

        private static string FormatResourceAmount(int amount)
        {
            return amount.ToString("N0", CultureInfo.InvariantCulture);
        }

        private void OnContinueClicked()
        {
            var catalog = GlobalEntities.Instance?.Chapter1Catalog;
            if (catalog == null || catalog.LevelCount == 0)
            {
                Debug.LogWarning("[Lobby] Chapter1Catalog is not assigned on GlobalEntities.");
                return;
            }

            var index = LevelProgressService.GetHighestUnlockedIndex(catalog.LevelCount);
            var level = catalog.GetLevel(index);
            if (level == null)
            {
                Debug.LogWarning("[Lobby] LevelSO not found for continue action.");
                return;
            }

            StartLevel(level, index);
        }

        private static void StartLevel(LevelSO level, int index)
        {
            GlobalVariable.CurrentLevel = level;
            GlobalVariable.CurrentLevelIndex = index;
            SceneManagerCustom.LoadDungeon();
        }

        private static void ExitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
