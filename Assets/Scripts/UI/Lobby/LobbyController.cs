using System.Globalization;
using Core;
using Core.Save;
using Global;
using SO;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomUI.Lobby
{
    /// <summary>Màn hình Lobby chính (UI Toolkit) — thay tương tác Portal/Cube.</summary>
    [RequireComponent(typeof(UIDocument))]
    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private string playerDisplayName = "PILOT";

        private UIDocument uiDocument;

        private Button quickAccessButton;
        private Button levelsButton;
        private Button heroButton;
        private Button creditButton;
        private Button exitButton;

        private Label quickAccessTitleLabel;
        private Label quickAccessSubtitleLabel;
        private Label playerNameLabel;
        private Label metaGoldValueLabel;
        private Label playerAchievementLabel;

        // Giữ reference callback để hủy đăng ký trong OnDisable (tránh leak)
        private EventCallback<ClickEvent> onQuickAccessClick;
        private EventCallback<ClickEvent> onLevelsClick;
        private EventCallback<ClickEvent> onExitClick;
        private EventCallback<ClickEvent> onHeroClick;

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

        /// <summary>Query các phần tử UI theo name đã đặt trong LobbyUI.uxml.</summary>
        private void CacheElements()
        {
            var root = uiDocument?.rootVisualElement;
            if (root == null)
                return;

            quickAccessButton = root.Q<Button>("quick-access-button");
            levelsButton = root.Q<Button>("levels-button");
            heroButton = root.Q<Button>("hero-button");
            creditButton = root.Q<Button>("credit-button");
            exitButton = root.Q<Button>("exit-button");

            quickAccessTitleLabel = root.Q<Label>("quick-access-title");
            quickAccessSubtitleLabel = root.Q<Label>("quick-access-subtitle");
            playerNameLabel = root.Q<Label>("player-name-label");
            metaGoldValueLabel = root.Q<Label>("meta-gold-value-label");
            playerAchievementLabel = root.Q<Label>("player-achievement-label");
        }

        private void BindCallbacks()
        {
            onQuickAccessClick = _ => OnQuickAccessClicked();
            onLevelsClick = _ => OpenLevelSelectPanel();
            onExitClick = _ => ExitGame();

            quickAccessButton?.RegisterCallback(onQuickAccessClick);
            levelsButton?.RegisterCallback(onLevelsClick);
            exitButton?.RegisterCallback(onExitClick);

            onHeroClick = _ => GlobalEvents.RaiseRequestHeroLoadoutUI();
            heroButton?.RegisterCallback(onHeroClick);
        }

        private void UnbindCallbacks()
        {
            quickAccessButton?.UnregisterCallback(onQuickAccessClick);
            levelsButton?.UnregisterCallback(onLevelsClick);
            exitButton?.UnregisterCallback(onExitClick);
            heroButton?.UnregisterCallback(onHeroClick);
        }

        private void HandleLobbyReady()
        {
            RefreshLobbyDisplay();
        }

        /// <summary>Cập nhật tên người chơi, tài nguyên meta, tiến độ màn và nhãn Quick Access.</summary>
        private void RefreshLobbyDisplay()
        {
            if (playerNameLabel != null)
                playerNameLabel.text = playerDisplayName;

            if (metaGoldValueLabel != null)
                metaGoldValueLabel.text = FormatResourceAmount(LevelProgressService.GetMetaGold());

            var catalog = GlobalEntities.Instance?.Chapter1Catalog;
            if (catalog == null || catalog.LevelCount == 0)
            {
                if (playerAchievementLabel != null)
                    playerAchievementLabel.text = "—";
                if (quickAccessSubtitleLabel != null)
                    quickAccessSubtitleLabel.text = "Chưa có dữ liệu màn";
                return;
            }

            var unlockedIndex = LevelProgressService.GetHighestUnlockedIndex(catalog.LevelCount);
            var unlockedCount = unlockedIndex + 1;

            if (playerAchievementLabel != null)
                playerAchievementLabel.text = $"{unlockedCount}/{catalog.LevelCount}";

            var level = catalog.GetLevel(unlockedIndex);
            if (quickAccessSubtitleLabel != null)
                quickAccessSubtitleLabel.text = level != null ? level.DisplayLabel : $"Màn {unlockedIndex + 1}";

            // Placeholder: giữ nút sẵn sàng cho panel tương lai
            heroButton?.SetEnabled(true);
            creditButton?.SetEnabled(true);
        }

        private static string FormatResourceAmount(int amount)
        {
            return amount.ToString("N0", CultureInfo.InvariantCulture);
        }

        /// <summary>Vào màn tiến độ hiện tại (màn mở khóa cao nhất).</summary>
        private void OnQuickAccessClicked()
        {
            var catalog = GlobalEntities.Instance?.Chapter1Catalog;
            if (catalog == null || catalog.LevelCount == 0)
            {
                Debug.LogWarning("[Lobby] Chapter1Catalog chưa gán trên GlobalEntities.");
                return;
            }

            var index = LevelProgressService.GetHighestUnlockedIndex(catalog.LevelCount);
            var level = catalog.GetLevel(index);
            if (level == null)
            {
                Debug.LogWarning("[Lobby] Không tìm thấy LevelSO cho màn quick access.");
                return;
            }

            StartLevel(level, index);
        }

        /// <summary>Mở panel chọn màn (LevelSelectController lắng nghe event).</summary>
        private void OpenLevelSelectPanel()
        {
            GlobalEvents.RaiseRequestLevelSelectUI();
        }

        private void StartLevel(LevelSO level, int index)
        {
            GlobalVariable.CurrentLevel = level;
            GlobalVariable.CurrentLevelIndex = index;
            SceneManagerCustom.LoadDungeon();
        }

        /// <summary>Thoát game — Editor dừng Play Mode, bản build gọi Application.Quit().</summary>
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
