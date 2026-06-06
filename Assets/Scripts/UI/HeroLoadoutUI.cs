using Core.Save;
using Global;
using SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class HeroLoadoutUI : MonoBehaviour
    {
        [SerializeField] private GameObject loadoutPanel;
        [SerializeField] private Transform heroListRoot;
        [SerializeField] private GameObject heroEntryPrefab;
        [SerializeField] private TMP_Text heroNameText;
        [SerializeField] private TMP_Text heroDescriptionText;
        [SerializeField] private TMP_Text metaGoldText;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button unlockButton;
        [SerializeField] private Button closeButton;

        private HeroSO selectedHero;

        private void OnEnable()
        {
            GlobalEvents.OnRequestHeroLoadoutUI += Open;
            GlobalEvents.OnMetaGoldChanged += RefreshMetaGold;
            GlobalEvents.OnSaveReset += HandleSaveReset;

            if (equipButton != null)
                equipButton.onClick.AddListener(OnEquipClicked);
            if (unlockButton != null)
                unlockButton.onClick.AddListener(OnUnlockClicked);
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            HidePanel();
        }

        private void OnDisable()
        {
            GlobalEvents.OnRequestHeroLoadoutUI -= Open;
            GlobalEvents.OnMetaGoldChanged -= RefreshMetaGold;
            GlobalEvents.OnSaveReset -= HandleSaveReset;

            if (equipButton != null)
                equipButton.onClick.RemoveListener(OnEquipClicked);
            if (unlockButton != null)
                unlockButton.onClick.RemoveListener(OnUnlockClicked);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);
        }

        public void Open()
        {
            if (loadoutPanel != null)
                loadoutPanel.SetActive(true);

            BuildHeroList();
            RefreshMetaGold();
        }

        public void Close()
        {
            HidePanel();
        }

        private void BuildHeroList()
        {
            if (heroListRoot == null || heroEntryPrefab == null)
            {
                SelectDefaultHero();
                RefreshSelectionUI();
                return;
            }

            for (var i = heroListRoot.childCount - 1; i >= 0; i--)
                Destroy(heroListRoot.GetChild(i).gameObject);

            var catalog = GlobalEntities.Instance?.HeroCatalog;
            if (catalog?.heroes == null)
                return;

            foreach (var hero in catalog.heroes)
            {
                if (hero == null) continue;

                var entry = Instantiate(heroEntryPrefab, heroListRoot);
                var label = entry.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    var locked = !HeroProgressService.IsUnlocked(hero.heroId);
                    label.text = locked ? $"{hero.displayName} (Locked)" : hero.displayName;
                }

                var button = entry.GetComponent<Button>();
                if (button != null)
                {
                    var captured = hero;
                    button.onClick.AddListener(() => SelectHero(captured));
                }
            }

            SelectDefaultHero();
            RefreshSelectionUI();
        }

        private void SelectDefaultHero()
        {
            var equippedId = HeroProgressService.GetEquippedHeroId();
            selectedHero = GlobalEntities.Instance?.GetHero(equippedId);
            if (selectedHero == null)
                selectedHero = GlobalEntities.Instance?.HeroCatalog?.GetDefaultHero();
        }

        private void SelectHero(HeroSO hero)
        {
            selectedHero = hero;
            RefreshSelectionUI();
            Lobby.HeroPreviewController.Instance?.ShowHero(hero);
        }

        private void RefreshSelectionUI()
        {
            if (selectedHero == null)
                return;

            if (heroNameText != null)
                heroNameText.text = selectedHero.displayName;
            if (heroDescriptionText != null)
                heroDescriptionText.text = selectedHero.description;

            bool unlocked = HeroProgressService.IsUnlocked(selectedHero.heroId);
            bool equipped = HeroProgressService.GetEquippedHeroId() == selectedHero.heroId;

            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(unlocked);
                equipButton.interactable = unlocked && !equipped;
            }

            if (unlockButton != null)
            {
                unlockButton.gameObject.SetActive(!unlocked);
                unlockButton.interactable = !unlocked;
            }
        }

        private void OnEquipClicked()
        {
            if (selectedHero == null)
                return;

            if (HeroProgressService.TryEquip(selectedHero.heroId))
                RefreshSelectionUI();
        }

        private void OnUnlockClicked()
        {
            if (selectedHero == null)
                return;

            if (HeroProgressService.TryUnlock(selectedHero))
            {
                RefreshMetaGold();
                RefreshSelectionUI();
            }
        }

        private void RefreshMetaGold()
        {
            if (metaGoldText != null)
                metaGoldText.text = LevelProgressService.GetMetaGold().ToString();
        }

        private void HandleSaveReset()
        {
            if (loadoutPanel == null || !loadoutPanel.activeSelf)
                return;

            BuildHeroList();
            RefreshMetaGold();
        }

        private void HidePanel()
        {
            if (loadoutPanel != null)
                loadoutPanel.SetActive(false);
        }
    }
}
