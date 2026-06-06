using Core;
using Core.Save;
using CustomUI.Lobby;
using Global;
using SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUI
{
    public class CharacterDetailUI : MonoBehaviour
    {
        private enum SkillPanelMode
        {
            None,
            NormalAttack,
            SpecialSkill,
            Dash
        }

        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button upgradeButton;

        [Header("Skill Buttons")]
        [SerializeField] private Button normalSkillButton;
        [SerializeField] private Button specialSkillButton;
        [SerializeField] private Button dashButton;

        [Header("Hero Info")]
        [SerializeField] private TMP_Text currentHealthText;
        [SerializeField] private TMP_Text currentDamageText;
        [SerializeField] private TMP_Text currentCritChanceText;
        [SerializeField] private RawImage characterRenderTexture;
        [SerializeField] private TMP_Text heroNameText;

        [Header("Skill Info")]
        [SerializeField] private GameObject detailSkillPanel;
        [SerializeField] private Image detailSkillIcon;
        [SerializeField] private TMP_Text detailSkillNameText;
        [SerializeField] private TMP_Text detailSkillDescriptionText;

        [Header("Main Panel")]
        [SerializeField] private GameObject characterSelectionUIPanel;
        [SerializeField] private GameObject characterDetailUIPanel;

        private HeroSO selectedHero;
        private SkillPanelMode skillPanelMode = SkillPanelMode.None;
        private CharacterSelectionUI selectionHost;

        public void SetSelectionHost(CharacterSelectionUI host) => selectionHost = host;

        private void Awake()
        {
            backButton?.onClick.AddListener(OnBackClicked);
            equipButton?.onClick.AddListener(OnEquipClicked);
            upgradeButton?.onClick.AddListener(OnUpgradeClicked);
            normalSkillButton?.onClick.AddListener(OnNormalSkillClicked);
            specialSkillButton?.onClick.AddListener(OnSpecialSkillClicked);
            dashButton?.onClick.AddListener(OnDashClicked);
        }

        private void OnEnable()
        {
            GlobalEvents.OnMetaGoldChanged += RefreshIfVisible;
            GlobalEvents.OnSaveReset += RefreshIfVisible;
        }

        private void OnDisable()
        {
            GlobalEvents.OnMetaGoldChanged -= RefreshIfVisible;
            GlobalEvents.OnSaveReset -= RefreshIfVisible;
        }

        private void OnDestroy()
        {
            backButton?.onClick.RemoveListener(OnBackClicked);
            equipButton?.onClick.RemoveListener(OnEquipClicked);
            upgradeButton?.onClick.RemoveListener(OnUpgradeClicked);
            normalSkillButton?.onClick.RemoveListener(OnNormalSkillClicked);
            specialSkillButton?.onClick.RemoveListener(OnSpecialSkillClicked);
            dashButton?.onClick.RemoveListener(OnDashClicked);
        }

        public void Show(HeroSO hero)
        {
            selectedHero = hero;

            if (characterSelectionUIPanel != null)
                characterSelectionUIPanel.SetActive(false);

            if (characterDetailUIPanel != null)
                characterDetailUIPanel.SetActive(true);

            BindData();
        }

        public void ReturnToList()
        {
            if (characterSelectionUIPanel != null)
                characterSelectionUIPanel.SetActive(true);
        }

        public void HidePreview()
        {
            HeroPreviewController.Instance?.HidePreview();
            HeroPreviewController.Instance?.UnregisterPreview();
        }

        public void BindData()
        {
            if (selectedHero == null)
                return;

            BindHeroStats();
            BindPreview();
            BindEquipState();
            BindUpgradeState();
            ShowSkillPanel(SkillPanelMode.None);
        }

        private void BindHeroStats()
        {
            var stats = HeroLoadoutBuilder.BuildEffectivePlayerSO(selectedHero);
            if (stats == null)
                return;

            if (currentHealthText != null)
                currentHealthText.text = stats.MaxHealth.ToString();

            if (currentDamageText != null)
                currentDamageText.text = stats.AttackDamage.ToString();

            if (currentCritChanceText != null)
                currentCritChanceText.text = $"{Mathf.RoundToInt(stats.CritChance * 100f)}%";

            if (heroNameText != null)
                heroNameText.text = selectedHero.displayName;
        }

        private void BindPreview()
        {
            if (characterRenderTexture == null)
                return;

            var preview = HeroPreviewController.Instance;
            if (preview == null)
                return;

            preview.RegisterPreview(characterRenderTexture);
            preview.ShowHero(selectedHero);
        }

        private void BindEquipState()
        {
            if (equipButton == null)
                return;

            var unlocked = HeroProgressService.IsUnlocked(selectedHero.heroId);
            var equipped = HeroProgressService.GetEquippedHeroId() == selectedHero.heroId;

            equipButton.gameObject.SetActive(unlocked);
            equipButton.interactable = unlocked && !equipped;
        }

        private void BindUpgradeState()
        {
            if (upgradeButton == null)
                return;

            var unlocked = HeroProgressService.IsUnlocked(selectedHero.heroId);
            upgradeButton.gameObject.SetActive(unlocked);
            upgradeButton.interactable = unlocked && HeroProgressService.CanUpgrade(selectedHero);
        }

        private void OnEquipClicked()
        {
            if (selectedHero == null)
                return;

            if (HeroProgressService.TryEquip(selectedHero.heroId))
            {
                BindEquipState();
                selectionHost?.BindData();
            }
        }

        private void OnUpgradeClicked()
        {
            if (selectedHero == null)
                return;

            if (HeroProgressService.TryUpgrade(selectedHero))
                BindData();
        }

        private void OnNormalSkillClicked() => ToggleSkillPanel(SkillPanelMode.NormalAttack);

        private void OnSpecialSkillClicked() => ToggleSkillPanel(SkillPanelMode.SpecialSkill);

        private void OnDashClicked() => ToggleSkillPanel(SkillPanelMode.Dash);

        private void ToggleSkillPanel(SkillPanelMode mode)
        {
            if (skillPanelMode == mode)
                ShowSkillPanel(SkillPanelMode.None);
            else
                ShowSkillPanel(mode);
        }

        private void ShowSkillPanel(SkillPanelMode mode)
        {
            skillPanelMode = mode;

            if (detailSkillPanel != null)
                detailSkillPanel.SetActive(mode != SkillPanelMode.None);

            if (mode == SkillPanelMode.None)
                return;

            switch (mode)
            {
                case SkillPanelMode.NormalAttack:
                    BindWeaponSkillPanel();
                    break;
                case SkillPanelMode.SpecialSkill:
                    BindHeroSkillPanel();
                    break;
                case SkillPanelMode.Dash:
                    BindDashSkillPanel();
                    break;
            }
        }

        private void BindWeaponSkillPanel()
        {
            var weapon = selectedHero?.boundWeapon;
            if (weapon == null)
            {
                SetSkillPanelContent(null, "Normal Attack", "No weapon assigned.");
                return;
            }

            SetSkillPanelContent(
                weapon.icon,
                weapon.displayName,
                string.IsNullOrWhiteSpace(weapon.description) ? weapon.displayName : weapon.description);
        }

        private void BindHeroSkillPanel()
        {
            var skill = selectedHero?.skill;
            if (skill == null)
            {
                SetSkillPanelContent(null, "Skill", "No skill assigned.");
                return;
            }

            var description = string.IsNullOrWhiteSpace(skill.description)
                ? $"{skill.displayName}\nCooldown: {skill.cooldown:0.#}s • Damage: {skill.damage}"
                : $"{skill.description}\nCooldown: {skill.cooldown:0.#}s • Damage: {skill.damage}";

            SetSkillPanelContent(skill.icon, skill.displayName, description);
        }

        private void BindDashSkillPanel()
        {
            var dash = GlobalEntities.Instance?.DefaultDashConfig;
            if (dash == null)
            {
                SetSkillPanelContent(null, "Dash", "Shared mobility ability for all heroes.");
                return;
            }

            var description =
                $"Distance: {dash.distance:0.#}m\n" +
                $"Duration: {dash.duration:0.##}s\n" +
                $"Cooldown: {dash.cooldown:0.#}s\n" +
                $"I-frames: {dash.iFrameDuration:0.##}s";

            SetSkillPanelContent(null, "Dash", description);
        }

        private void SetSkillPanelContent(Sprite icon, string title, string description)
        {
            if (detailSkillIcon != null)
            {
                detailSkillIcon.sprite = icon;
                detailSkillIcon.enabled = icon != null;
            }

            if (detailSkillNameText != null)
                detailSkillNameText.text = title ?? string.Empty;

            if (detailSkillDescriptionText != null)
                detailSkillDescriptionText.text = description ?? string.Empty;
        }

        private void RefreshIfVisible()
        {
            if (characterDetailUIPanel != null && characterDetailUIPanel.activeInHierarchy)
                BindData();
        }

        private void OnBackClicked()
        {
            Debug.Log("[OnBackClicked]");
            HidePreview();

            if (characterDetailUIPanel != null)
                Debug.Log("[OnBackClicked] SetActive(false)");
                characterDetailUIPanel.SetActive(false);

            Debug.Log("[OnBackClicked] ReturnToList");
            ReturnToList();
            Debug.Log("[OnBackClicked] BindData");
            selectionHost?.BindData();
        }
    }
}
