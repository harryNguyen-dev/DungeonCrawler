using Components;
using Core;
using Core.Save;
using Global;
using SO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CustomUI
{
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject contentPanel;

        [SerializeField] private GameObject itemShopUIPrefab;
        [SerializeField] private Transform itemShopListRoot;

        [SerializeField] private Button[] categoryButtons; // 0: Characters, 1: Frames (TODO), 2: Icons (TODO)

        [Header("Confirm Session")]
        [SerializeField] private GameObject confirmSessionPanel;
        [SerializeField] private Button confirmSessionButton;
        [SerializeField] private Button cancelSessionButton;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text itemDescriptionText;
        [SerializeField] private Image itemIcon;

        [Header("Refresh Targets")]
        [SerializeField] private CharacterSelectionUI characterSelectionUI;

        private int activeCategoryIndex;
        private HeroSO pendingHero;
        private UnityAction[] categoryListeners;
        private UnityAction cancelConfirmListener;
        private Image[] categoryButtonImages;

        private static readonly Color ActiveCategoryColor = ColorUtils.Yellow;
        private static readonly Color InactiveCategoryColor = ColorUtils.Gray;

        public void SetCharacterSelectionUI(CharacterSelectionUI selectionUI) =>
            characterSelectionUI = selectionUI;

        private void Awake()
        {
            backButton?.onClick.AddListener(OnBackClicked);
            confirmSessionButton?.onClick.AddListener(OnConfirmPurchase);
            cancelConfirmListener = () => HideConfirmPanel(true);
            cancelSessionButton?.onClick.AddListener(cancelConfirmListener);

            if (categoryButtons != null)
            {
                categoryListeners = new UnityAction[categoryButtons.Length];
                categoryButtonImages = new Image[categoryButtons.Length];

                for (var i = 0; i < categoryButtons.Length; i++)
                {
                    var index = i;
                    categoryListeners[i] = () => SelectCategory(index);
                    categoryButtons[i]?.onClick.AddListener(categoryListeners[i]);

                    if (categoryButtons[i] != null)
                        categoryButtonImages[i] = categoryButtons[i].GetComponent<Image>();
                }
            }

            HideConfirmPanel();
        }

        private void OnEnable()
        {
            GlobalEvents.OnMetaGoldChanged += OnMetaGoldChanged;
            GlobalEvents.OnSaveReset += HandleSaveReset;
        }

        private void OnDisable()
        {
            GlobalEvents.OnMetaGoldChanged -= OnMetaGoldChanged;
            GlobalEvents.OnSaveReset -= HandleSaveReset;
        }

        private void HandleSaveReset()
        {
            HideConfirmPanel();

            if (!gameObject.activeInHierarchy)
                return;

            if (activeCategoryIndex == 0)
                BindCharactersTab();

            characterSelectionUI?.BindData();
        }

        private void OnDestroy()
        {
            backButton?.onClick.RemoveListener(OnBackClicked);
            confirmSessionButton?.onClick.RemoveListener(OnConfirmPurchase);
            cancelSessionButton?.onClick.RemoveListener(cancelConfirmListener);

            if (categoryButtons != null && categoryListeners != null)
            {
                for (var i = 0; i < categoryButtons.Length; i++)
                    categoryButtons[i]?.onClick.RemoveListener(categoryListeners[i]);
            }
        }

        public void Close()
        {
            HideConfirmPanel();
            gameObject.SetActive(false);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            HideConfirmPanel();
            SelectCategory(0);
        }

        private void OnBackClicked()
        {
            GameAudio.PlayUiBack();
            Close();
        }

        private void SelectCategory(int index)
        {
            GameAudio.PlayUiTab();
            activeCategoryIndex = index;
            RefreshCategoryButtonVisuals();

            if (index == 0)
                BindCharactersTab();
            else
                ClearItemList();
        }

        private void RefreshCategoryButtonVisuals()
        {
            if (categoryButtonImages == null)
                return;

            for (var i = 0; i < categoryButtonImages.Length; i++)
            {
                if (categoryButtonImages[i] == null)
                    continue;

                categoryButtonImages[i].color = i == activeCategoryIndex
                    ? ActiveCategoryColor
                    : InactiveCategoryColor;
            }
        }

        private void BindCharactersTab()
        {
            ClearItemList();

            if (itemShopListRoot == null || itemShopUIPrefab == null)
                return;

            var catalog = GlobalEntities.Instance?.HeroCatalog;
            if (catalog?.heroes == null)
                return;

            foreach (var hero in catalog.heroes)
            {
                if (hero == null || HeroProgressService.IsUnlocked(hero.heroId))
                    continue;

                var entry = Instantiate(itemShopUIPrefab, itemShopListRoot);
                var entryView = entry.GetComponent<ItemShopUIPrefab>();
                if (entryView != null)
                    entryView.Bind(hero, ShowConfirmPanel);
            }
        }

        private void ClearItemList()
        {
            if (itemShopListRoot == null)
                return;

            for (var i = itemShopListRoot.childCount - 1; i >= 0; i--)
                Destroy(itemShopListRoot.GetChild(i).gameObject);
        }

        private void ShowConfirmPanel(HeroSO hero)
        {
            if (hero == null)
                return;

            pendingHero = hero;

            if (confirmSessionPanel != null)
                confirmSessionPanel.SetActive(true);

            if (itemNameText != null)
                itemNameText.text = hero.displayName;

            if (itemDescriptionText != null)
                itemDescriptionText.text = hero.description;

            if (itemIcon != null)
            {
                itemIcon.sprite = hero.icon;
                itemIcon.enabled = hero.icon != null;
            }

            RefreshConfirmButton();
        }

        private void HideConfirmPanel(bool playBackSound = false)
        {
            if (playBackSound)
                GameAudio.PlayUiBack();

            pendingHero = null;

            if (confirmSessionPanel != null)
                confirmSessionPanel.SetActive(false);
        }

        private void OnConfirmPurchase()
        {
            if (pendingHero == null)
                return;

            if (!HeroProgressService.TryUnlock(pendingHero))
            {
                GameAudio.PlayUiError();
                return;
            }

            GameAudio.PlayUiPurchase();

            HideConfirmPanel();

            if (activeCategoryIndex == 0)
                BindCharactersTab();

            characterSelectionUI?.BindData();
        }

        private void OnMetaGoldChanged()
        {
            RefreshCurrentTab();
        }

        private void RefreshCurrentTab()
        {
            if (activeCategoryIndex == 0)
            {
                if (confirmSessionPanel != null && confirmSessionPanel.activeSelf)
                    RefreshConfirmButton();

                RefreshBuyStates();
                return;
            }

            if (confirmSessionPanel != null && confirmSessionPanel.activeSelf)
                HideConfirmPanel();
        }

        private void RefreshBuyStates()
        {
            if (itemShopListRoot == null)
                return;

            for (var i = itemShopListRoot.childCount - 1; i >= 0; i--)
            {
                var entryView = itemShopListRoot.GetChild(i).GetComponent<ItemShopUIPrefab>();
                entryView?.RefreshBuyState();
            }
        }

        private void RefreshConfirmButton()
        {
            if (confirmSessionButton == null || pendingHero == null)
                return;

            confirmSessionButton.interactable =
                LevelProgressService.GetMetaGold() >= pendingHero.unlockCost;
        }
    }
}
