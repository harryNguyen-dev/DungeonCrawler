using Core;
using Global;
using PlayerController;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUI
{
    public class BattleUI : MonoBehaviour
    {
        private const string CooldownFillChildName = "CooldownFill";
        private const string CooldownTextChildName = "CooldownText";
        private const string IconChildName = "Icon";

        [Header("Hero Info")]
        [SerializeField] private Image hpBarFill;
        [SerializeField] private Image expBarFill;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text expText;

        [Header("Currency Info")]
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text monsterKilledText;

        [Header("Stars")]
        [SerializeField] private Image star1;
        [SerializeField] private Image star2;
        [SerializeField] private Image star3;

        [Header("Combat buttons")]
        [SerializeField] private Button normalAttackButton;
        [SerializeField] private Button dashButton;
        [SerializeField] private Button skillButton;

        private EventTrigger attackButtonTrigger;
        private EventTrigger.Entry attackPointerDownEntry;
        private EventTrigger.Entry attackPointerUpEntry;
        private EventTrigger.Entry attackPointerExitEntry;

        private PlayerEvents playerEvents;
        private PlayerStats playerStats;
        private Health playerHealth;
        private PlayerSkill playerSkill;
        private PlayerDash playerDash;

        private CombatButtonView normalAttackView;
        private CombatButtonView skillButtonView;
        private CombatButtonView dashButtonView;

        private int runGold;
        private int enemiesKilled;

        private void Awake()
        {
            normalAttackView = CombatButtonView.Bind(normalAttackButton, bindIcon: true);
            skillButtonView = CombatButtonView.Bind(skillButton, bindIcon: true);
            dashButtonView = CombatButtonView.Bind(dashButton, bindIcon: false);
        }

        private void OnEnable()
        {
            GlobalEvents.OnPlayerJoin += BindPlayer;
            GlobalEvents.OnMatchReset += HandleMatchReset;
            GlobalEvents.OnDungeonGenerated += HandleDungeonGenerated;
            GlobalEvents.OnRunStarsChanged += HandleRunStarsChanged;
            GlobalEvents.OnRunGoldChanged += HandleRunGoldChanged;
            GlobalEvents.OnEnemyDie += HandleEnemyKilled;

            if (GlobalEntities.Instance?.PlayerEvents != null)
            {
                BindPlayer();
            }

            RefreshCurrency();
            StarDisplayHelper.Apply(star1, star2, star3, 0);
            WireAttackButton();
            WireDashButton();
            WireSkillButton();
        }

        private void OnDisable()
        {
            GlobalEvents.OnPlayerJoin -= BindPlayer;
            GlobalEvents.OnMatchReset -= HandleMatchReset;
            GlobalEvents.OnDungeonGenerated -= HandleDungeonGenerated;
            GlobalEvents.OnRunStarsChanged -= HandleRunStarsChanged;
            GlobalEvents.OnRunGoldChanged -= HandleRunGoldChanged;
            GlobalEvents.OnEnemyDie -= HandleEnemyKilled;
            UnbindPlayerEvents();
            UnwireAttackButton();
            UnwireDashButton();
            UnwireSkillButton();
            ClearCombatButtonCooldowns();
        }

        private void Update()
        {
            RefreshCombatButtonCooldowns();
        }

        private void BindPlayer()
        {
            UnbindPlayerEvents();

            var entities = GlobalEntities.Instance;
            if (entities == null) return;

            playerStats = entities.PlayerStats;
            playerHealth = entities.PlayerHealth;
            playerEvents = entities.PlayerEvents;
            if (playerStats == null || playerEvents == null) return;

            var playerInstance = entities.PlayerInstance;
            if (playerInstance != null)
            {
                playerSkill = playerInstance.GetComponent<PlayerSkill>();
                playerDash = playerInstance.GetComponent<PlayerDash>();
            }

            playerEvents.OnHealthChanged += HandleHealthChanged;
            playerEvents.OnMaxHealthChanged += HandleMaxHealthChanged;
            playerEvents.OnExpChanged += HandleExpChanged;

            RefreshAll();
            RefreshCombatButtonIcons();
        }

        private void UnbindPlayerEvents()
        {
            if (playerEvents == null) return;

            playerEvents.OnHealthChanged -= HandleHealthChanged;
            playerEvents.OnMaxHealthChanged -= HandleMaxHealthChanged;
            playerEvents.OnExpChanged -= HandleExpChanged;
            playerEvents = null;
        }

        private void HandleMatchReset()
        {
            UnbindPlayerEvents();
            playerStats = null;
            playerHealth = null;
            playerSkill = null;
            playerDash = null;
            runGold = 0;
            enemiesKilled = 0;
            RefreshCurrency();
            StarDisplayHelper.Apply(star1, star2, star3, 0);
            ClearCombatButtonCooldowns();
        }

        private void HandleDungeonGenerated(int _)
        {
            StarDisplayHelper.Apply(star1, star2, star3, 0);
        }

        private void HandleRunStarsChanged(int stars)
        {
            StarDisplayHelper.Apply(star1, star2, star3, stars);
        }

        private void HandleHealthChanged(int current, int max)
        {
            SetBar(hpBarFill, hpText, current, max);
        }

        private void HandleMaxHealthChanged(int maxHealth)
        {
            if (playerHealth == null) return;
            HandleHealthChanged(playerHealth.GetCurrentHealth(), maxHealth);
        }

        private void HandleExpChanged(int current, int required)
        {
            SetBar(expBarFill, expText, current, required);
        }

        private void HandleRunGoldChanged(int totalRunGold)
        {
            runGold = totalRunGold;
            RefreshCurrency();
        }

        private void HandleEnemyKilled(int _)
        {
            enemiesKilled++;
            RefreshCurrency();
        }

        private void RefreshAll()
        {
            if (playerStats == null) return;

            if (playerHealth != null)
            {
                HandleHealthChanged(playerHealth.GetCurrentHealth(), playerStats.GetMaxHealth());
            }
            else
            {
                SetBar(hpBarFill, hpText, playerStats.GetMaxHealth(), playerStats.GetMaxHealth());
            }

            RefreshExp();
        }

        private void RefreshExp()
        {
            if (playerStats == null) return;
            HandleExpChanged(playerStats.currentExp, playerStats.expToNextLevel);
        }

        private void RefreshCurrency()
        {
            if (goldText != null)
                goldText.text = runGold.ToString();

            if (monsterKilledText != null)
                monsterKilledText.text = enemiesKilled.ToString();
        }

        private static void SetBar(Image fill, TMP_Text valueLabel, int current, int max)
        {
            if (fill != null)
            {
                fill.fillAmount = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            }

            if (valueLabel != null)
            {
                valueLabel.text = max > 0 ? $"{current}/{max}" : "0/0";
            }
        }

        private void WireAttackButton()
        {
            if (normalAttackButton == null)
                return;

            attackButtonTrigger = normalAttackButton.gameObject.GetComponent<EventTrigger>();
            if (attackButtonTrigger == null)
                attackButtonTrigger = normalAttackButton.gameObject.AddComponent<EventTrigger>();

            attackPointerDownEntry = CreateTriggerEntry(EventTriggerType.PointerDown, OnAttackButtonPressed);
            attackPointerUpEntry = CreateTriggerEntry(EventTriggerType.PointerUp, OnAttackButtonReleased);
            attackPointerExitEntry = CreateTriggerEntry(EventTriggerType.PointerExit, OnAttackButtonReleased);

            attackButtonTrigger.triggers.Add(attackPointerDownEntry);
            attackButtonTrigger.triggers.Add(attackPointerUpEntry);
            attackButtonTrigger.triggers.Add(attackPointerExitEntry);
        }

        private void UnwireAttackButton()
        {
            if (attackButtonTrigger != null)
            {
                if (attackPointerDownEntry != null)
                    attackButtonTrigger.triggers.Remove(attackPointerDownEntry);
                if (attackPointerUpEntry != null)
                    attackButtonTrigger.triggers.Remove(attackPointerUpEntry);
                if (attackPointerExitEntry != null)
                    attackButtonTrigger.triggers.Remove(attackPointerExitEntry);
            }

            attackButtonTrigger = null;
            attackPointerDownEntry = null;
            attackPointerUpEntry = null;
            attackPointerExitEntry = null;

            InputManager.Instance?.SetUiAttackHeld(false);
        }

        private static EventTrigger.Entry CreateTriggerEntry(
            EventTriggerType type,
            UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(callback);
            return entry;
        }

        private static void OnAttackButtonPressed(BaseEventData _)
        {
            InputManager.Instance?.SetUiAttackHeld(true);
        }

        private static void OnAttackButtonReleased(BaseEventData _)
        {
            InputManager.Instance?.SetUiAttackHeld(false);
        }

        private void WireDashButton()
        {
            if (dashButton == null)
                return;

            dashButton.onClick.RemoveAllListeners();
            dashButton.onClick.AddListener(OnDashButtonClicked);
        }

        private void UnwireDashButton()
        {
            if (dashButton != null)
                dashButton.onClick.RemoveAllListeners();
        }

        private static void OnDashButtonClicked()
        {
            InputManager.Instance?.SetUiDashPressed();
        }

        private void WireSkillButton()
        {
            if (skillButton == null)
                return;

            skillButton.onClick.RemoveAllListeners();
            skillButton.onClick.AddListener(OnSkillButtonClicked);
        }

        private void UnwireSkillButton()
        {
            if (skillButton != null)
                skillButton.onClick.RemoveAllListeners();
        }

        private static void OnSkillButtonClicked()
        {
            InputManager.Instance?.SetUiSkillPressed();
        }

        private void RefreshCombatButtonIcons()
        {
            normalAttackView.SetIcon(playerStats?.EquippedWeapon?.icon);
            skillButtonView.SetIcon(playerStats?.ActiveSkill?.icon);
        }

        private void RefreshCombatButtonCooldowns()
        {
            normalAttackView.ClearCooldown();

            if (playerSkill != null && playerSkill.TryGetCooldown(out var skillRemaining, out var skillDuration))
                skillButtonView.ApplyCooldown(skillRemaining, skillDuration);
            else
                skillButtonView.ClearCooldown();

            if (playerDash != null && playerDash.TryGetCooldown(out var dashRemaining, out var dashDuration))
                dashButtonView.ApplyCooldown(dashRemaining, dashDuration);
            else
                dashButtonView.ClearCooldown();
        }

        private void ClearCombatButtonCooldowns()
        {
            normalAttackView.ClearCooldown();
            skillButtonView.ClearCooldown();
            dashButtonView.ClearCooldown();
        }

        private readonly struct CombatButtonView
        {
            private readonly Image cooldownFill;
            private readonly TMP_Text cooldownText;
            private readonly Image icon;

            private CombatButtonView(Image cooldownFill, TMP_Text cooldownText, Image icon)
            {
                this.cooldownFill = cooldownFill;
                this.cooldownText = cooldownText;
                this.icon = icon;
            }

            public static CombatButtonView Bind(Button button, bool bindIcon)
            {
                if (button == null)
                    return default;

                var root = button.transform;
                var fillTransform = root.Find(CooldownFillChildName);
                var textTransform = root.Find(CooldownTextChildName);
                Transform iconTransform = bindIcon ? root.Find(IconChildName) : null;

                Image fillImage = null;
                TMP_Text text = null;
                Image iconImage = null;

                if (fillTransform != null)
                    fillTransform.TryGetComponent(out fillImage);
                if (textTransform != null)
                    textTransform.TryGetComponent(out text);
                if (iconTransform != null)
                    iconTransform.TryGetComponent(out iconImage);

                return new CombatButtonView(fillImage, text, iconImage);
            }

            public void SetIcon(Sprite sprite)
            {
                if (icon == null)
                    return;

                icon.sprite = sprite;
                icon.enabled = sprite != null;
            }

            public void ApplyCooldown(float remaining, float duration)
            {
                if (duration <= 0f || remaining <= 0f)
                {
                    ClearCooldown();
                    return;
                }

                var fillAmount = Mathf.Clamp01(remaining / duration);

                if (cooldownFill != null)
                {
                    cooldownFill.gameObject.SetActive(true);
                    cooldownFill.fillAmount = fillAmount;
                }

                if (cooldownText != null)
                {
                    cooldownText.gameObject.SetActive(true);
                    cooldownText.text = remaining >= 1f
                        ? Mathf.CeilToInt(remaining).ToString()
                        : remaining.ToString("0.#");
                }
            }

            public void ClearCooldown()
            {
                if (cooldownFill != null)
                {
                    cooldownFill.fillAmount = 0f;
                    cooldownFill.gameObject.SetActive(false);
                }

                if (cooldownText != null)
                {
                    cooldownText.text = string.Empty;
                    cooldownText.gameObject.SetActive(false);
                }
            }
        }
    }
}
