using Core;
using Core.Save;
using Global;
using SO;
using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private Button creditBtn;
        [SerializeField] private Button settingsBtn;
        [SerializeField] private Button exitBtn;

        [Header("Panels")]
        [SerializeField] private SelectMapsUI selectMapsUI;
        [SerializeField] private GameObject creditPanel;

        private void Awake()
        {
            battleBtn?.onClick.AddListener(OnBattleClicked);
            levelBtn?.onClick.AddListener(OnLevelClicked);
            creditBtn?.onClick.AddListener(OnCreditClicked);
            settingsBtn?.onClick.AddListener(OnSettingsClicked);
            exitBtn?.onClick.AddListener(OnExitClicked);
        }

        private void OnEnable()
        {
            selectMapsUI?.Close();
            HideCreditPanel();
            RefreshBattleButton();

            GlobalEvents.OnLobbyReady += RefreshBattleButton;
        }

        private void OnDisable()
        {
            GlobalEvents.OnLobbyReady -= RefreshBattleButton;
        }

        private void OnDestroy()
        {
            battleBtn?.onClick.RemoveListener(OnBattleClicked);
            levelBtn?.onClick.RemoveListener(OnLevelClicked);
            creditBtn?.onClick.RemoveListener(OnCreditClicked);
            settingsBtn?.onClick.RemoveListener(OnSettingsClicked);
            exitBtn?.onClick.RemoveListener(OnExitClicked);
        }

        private void RefreshBattleButton()
        {
            if (battleBtn == null)
                return;

            var catalog = GlobalEntities.Instance?.Chapter1Catalog;
            battleBtn.interactable = catalog != null && catalog.LevelCount > 0;
        }

        private void OnBattleClicked()
        {
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

            StartLevel(level, index);
        }

        private void OnLevelClicked()
        {
            if (selectMapsUI != null)
            {
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
