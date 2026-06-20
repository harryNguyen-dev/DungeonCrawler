using Core;
using Core.Save;
using CustomUI.Lobby;
using Global;
using SO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomUI
{
    /// <summary>Main lobby menu (uGUI).</summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button battleBtn;
        [SerializeField] private Button levelBtn;
        [SerializeField] private Button shopBtn;
        [SerializeField] private Button creditBtn;
        [SerializeField] private Button settingsBtn;
        [SerializeField] private Button exitBtn;
        [SerializeField] private Button heroBtn;

        [Header("Panels & References")]
        [SerializeField] private SelectMapsUI selectMapsUI;
        [SerializeField] private GameObject creditPanel;
        [SerializeField] private CharacterSelectionUI characterSelectionUI;
        [SerializeField] private RawImage lobbyHeroPreview;
        [SerializeField] private ShopUI shopUI;

        [Header("Other")]
        [SerializeField] private TMP_Text metaGoldText;
        [SerializeField] private Button metaGoldAddButton;

        private void Awake()
        {
            battleBtn?.onClick.AddListener(OnBattleClicked);
            levelBtn?.onClick.AddListener(OnLevelClicked);
            shopBtn?.onClick.AddListener(OnShopClicked);
            creditBtn?.onClick.AddListener(OnCreditClicked);
            settingsBtn?.onClick.AddListener(OnSettingsClicked);
            exitBtn?.onClick.AddListener(OnExitClicked);
            heroBtn?.onClick.AddListener(OnHeroClicked);
            metaGoldAddButton?.onClick.AddListener(OnMetaGoldAddClicked);

            characterSelectionUI?.SetMainMenuHost(this);
            shopUI?.SetCharacterSelectionUI(characterSelectionUI);
        }

        private void OnEnable()
        {
            selectMapsUI?.Close();
            HideCreditPanel();
            RefreshBattleButton();
            RefreshMetaGold();
            GlobalEvents.OnLobbyReady += RefreshBattleButton;
            GlobalEvents.OnMetaGoldChanged += RefreshMetaGold;
            GlobalEvents.OnSaveReset += HandleSaveReset;

            if (!IsCharacterFlowOpen())
                ShowLobbyHeroPreview();
        }

        private void Start()
        {
            if (!IsCharacterFlowOpen())
                ShowLobbyHeroPreview();
        }

        private void OnDisable()
        {
            GlobalEvents.OnLobbyReady -= RefreshBattleButton;
            GlobalEvents.OnMetaGoldChanged -= RefreshMetaGold;
            GlobalEvents.OnSaveReset -= HandleSaveReset;
        }

        private void OnDestroy()
        {
            battleBtn?.onClick.RemoveListener(OnBattleClicked);
            levelBtn?.onClick.RemoveListener(OnLevelClicked);
            shopBtn?.onClick.AddListener(OnShopClicked);
            creditBtn?.onClick.RemoveListener(OnCreditClicked);
            settingsBtn?.onClick.RemoveListener(OnSettingsClicked);
            exitBtn?.onClick.RemoveListener(OnExitClicked);
            heroBtn?.onClick.RemoveListener(OnHeroClicked);
            metaGoldAddButton?.onClick.RemoveListener(OnMetaGoldAddClicked);
        }
        public void OnShopClicked()
        {
            GameAudio.PlayUiConfirm();
            shopUI?.Open();
        }
        public void ShowLobbyHeroPreview()
        {
            if (lobbyHeroPreview == null)
                return;

            var preview = HeroPreviewController.Instance;
            if (preview == null)
                return;

            preview.RegisterPreview(lobbyHeroPreview);
            preview.ShowPreview();
        }

        public void HideLobbyHeroPreview()
        {
            var preview = HeroPreviewController.Instance;
            if (preview == null)
                return;

            preview.HidePreview();
            preview.UnregisterPreview();
        }

        private bool IsCharacterFlowOpen()
        {
            return characterSelectionUI != null && characterSelectionUI.IsOpen;
        }

        private void HandleSaveReset()
        {
            shopUI?.Close();
            characterSelectionUI?.Close();
            HideCreditPanel();
            selectMapsUI?.Close();
            RefreshMetaGold();
            RefreshBattleButton();

            if (!IsCharacterFlowOpen())
                ShowLobbyHeroPreview();
        }

        private void RefreshBattleButton()
        {
            if (battleBtn == null)
                return;

            var catalog = GlobalEntities.Instance?.Chapter1Catalog;
            battleBtn.interactable = catalog != null && catalog.LevelCount > 0;
        }

        private void RefreshMetaGold()
        {
            if (metaGoldText != null)
                metaGoldText.text = LevelProgressService.GetMetaGold().ToString();
        }

        private void OnMetaGoldAddClicked()
        {
#if UNITY_EDITOR
            LevelProgressService.AddMetaGold(1000);
#else
            // TODO: meta gold purchase / ad reward flow
#endif
        }

        private void OnBattleClicked()
        {
            GameAudio.PlayUiConfirm();
            var catalog = GlobalEntities.Instance?.Chapter1Catalog;
            if (catalog == null || catalog.LevelCount == 0)
            {
                Debug.LogWarning("[MainMenu] Chapter1Catalog is not assigned on GlobalEntities.");
                return;
            }

            var index = LevelProgressService.GetHighestUnlockedIndex(catalog.LevelCount);
            var level = catalog.GetLevel(index);
            if (level == null)
            {
                Debug.LogWarning("[MainMenu] LevelSO not found for battle action.");
                return;
            }

            HideLobbyHeroPreview();
            StartLevel(level, index);
        }

        private void OnHeroClicked()
        {
            GameAudio.PlayUiConfirm();
            HideLobbyHeroPreview();
            characterSelectionUI?.Open();
        }

        private void OnLevelClicked()
        {
            GameAudio.PlayUiConfirm();
            if (selectMapsUI != null)
            {
                HideLobbyHeroPreview();
                selectMapsUI.Open();
                return;
            }

            Debug.LogWarning("[MainMenu] SelectMapsUI is not assigned.");
        }

        private void OnCreditClicked()
        {
            if (creditPanel != null)
            {
                creditPanel.SetActive(true);
                return;
            }

            Debug.Log("[MainMenu] Credit — coming soon.");
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenu] Settings — coming soon.");
        }

        private void OnExitClicked()
        {
            GameAudio.PlayUiBack();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void HideCreditPanel()
        {
            if (creditPanel != null)
                creditPanel.SetActive(false);
        }

        private static void StartLevel(LevelSO level, int index)
        {
            GlobalVariable.CurrentLevel = level;
            GlobalVariable.CurrentLevelIndex = index;
            SceneManagerCustom.LoadDungeon();
        }
    }
}
