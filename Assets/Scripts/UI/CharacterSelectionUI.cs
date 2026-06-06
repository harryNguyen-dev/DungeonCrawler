using Components;
using Core;
using Core.Save;
using Global;
using SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class CharacterSelectionUI : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Transform characterListRoot;
        [SerializeField] private GameObject characterSelectionUIPrefab;
        [SerializeField] private CharacterDetailUI characterDetailUI;
        [Header("Main Panel")]
        [SerializeField] private GameObject characterSelectionUIPanel;
        [SerializeField] private GameObject characterDetailUIPanel;

        private MainMenuUI mainMenuHost;

        public bool IsOpen =>
            characterSelectionUIPanel != null && characterSelectionUIPanel.activeInHierarchy;

        public void SetMainMenuHost(MainMenuUI host) => mainMenuHost = host;

        private void Awake()
        {
            backButton?.onClick.AddListener(OnBackClicked);

            if (characterDetailUI == null && characterDetailUIPanel != null)
                characterDetailUI = characterDetailUIPanel.GetComponent<CharacterDetailUI>();

            characterDetailUI?.SetSelectionHost(this);
        }

        private void OnDestroy()
        {
            backButton?.onClick.RemoveListener(OnBackClicked);
        }

        private void OnEnable()
        {
            GlobalEvents.OnSaveReset += HandleSaveReset;
        }

        private void OnDisable()
        {
            GlobalEvents.OnSaveReset -= HandleSaveReset;
        }

        private void HandleSaveReset()
        {
            if (!IsOpen)
                return;

            ShowListPanel();
            BindData();
        }

        public void Close()
        {
            if (characterSelectionUIPanel != null)
                characterSelectionUIPanel.SetActive(false);

            if (characterDetailUIPanel != null)
                characterDetailUIPanel.SetActive(false);

            characterDetailUI?.HidePreview();
            mainMenuHost?.ShowLobbyHeroPreview();
        }

        public void Open()
        {
            mainMenuHost?.HideLobbyHeroPreview();

            if (characterSelectionUIPanel != null)
                characterSelectionUIPanel.SetActive(true);

            ShowListPanel();
            BindData();
        }

        public void BindData()
        {
            if (characterListRoot == null || characterSelectionUIPrefab == null)
                return;

            for (var i = characterListRoot.childCount - 1; i >= 0; i--)
                Destroy(characterListRoot.GetChild(i).gameObject);

            var catalog = GlobalEntities.Instance?.HeroCatalog;
            if (catalog?.heroes == null)
                return;

            foreach (var hero in catalog.heroes)
            {
                if (hero == null || !HeroProgressService.IsUnlocked(hero.heroId))
                    continue;

                var entry = Instantiate(characterSelectionUIPrefab, characterListRoot);
                var entryView = entry.GetComponent<CharacterSelectionUIPrefab>();
                if (entryView != null)
                    entryView.Bind(hero, OnHeroChosen);
            }
        }

        private void OnHeroChosen(HeroSO hero)
        {
            if (hero == null || characterDetailUI == null)
                return;

            if (characterDetailUIPanel != null)
                characterDetailUIPanel.SetActive(true);

            characterDetailUI.Show(hero);
        }

        private void ShowListPanel()
        {
            if (characterDetailUIPanel != null)
                characterDetailUIPanel.SetActive(false);

            characterDetailUI?.ReturnToList();
        }

        private void OnBackClicked()
        {
            Close();
        }
    }
}
