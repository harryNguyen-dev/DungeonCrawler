using Global;
using PlayerController;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUI.Battle
{
    [RequireComponent(typeof(UIDocument))]
    public class BattleHUDController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement hpFill;
        private VisualElement expFill;
        private Label levelLabel;
        private Label hpValueLabel;
        private Label expValueLabel;
        private Label atkLabel;
        private Label asLabel;
        private Label msLabel;

        private PlayerEvents playerEvents;
        private PlayerStats playerStats;
        private Health playerHealth;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null)
            {
                // Below modal UI Toolkit card pick. Full-screen root must not eat clicks.
                uiDocument.sortingOrder = 0;
            }
        }

        private void ConfigureInputPassthrough(VisualElement root)
        {
            if (root == null) return;
            root.pickingMode = PickingMode.Ignore;
        }

        private void OnEnable()
        {
            GlobalEvents.OnPlayerJoin += BindPlayer;
            GlobalEvents.OnMatchReset += UnbindPlayer;
            GlobalEvents.OnLevelUp += HandleLevelUp;
            GlobalEvents.OnEnemyDie += HandleExpChanged;

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                ConfigureInputPassthrough(root);
                CacheElements(root);
            }

            if (GlobalEntities.Instance?.PlayerEvents != null)
            {
                BindPlayer();
            }
        }

        private void OnDisable()
        {
            GlobalEvents.OnPlayerJoin -= BindPlayer;
            GlobalEvents.OnMatchReset -= UnbindPlayer;
            GlobalEvents.OnLevelUp -= HandleLevelUp;
            GlobalEvents.OnEnemyDie -= HandleExpChanged;
            UnbindPlayerEvents();
        }

        private void CacheElements(VisualElement root)
        {
            if (root == null) return;

            hpFill = root.Q<VisualElement>("hp-fill");
            expFill = root.Q<VisualElement>("exp-fill");
            levelLabel = root.Q<Label>("level-label");
            hpValueLabel = root.Q<Label>("hp-value");
            expValueLabel = root.Q<Label>("exp-value");
            atkLabel = root.Q<Label>("atk-label");
            asLabel = root.Q<Label>("as-label");
            msLabel = root.Q<Label>("ms-label");
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

            playerEvents.OnHealthChanged += HandleHealthChanged;
            playerEvents.OnAttackDamageChanged += HandleAttackDamageChanged;
            playerEvents.OnAttackSpeedChanged += HandleAttackSpeedChanged;
            playerEvents.OnIncreaseMoveSpeed += HandleMoveSpeedChanged;
            playerEvents.OnMaxHealthChanged += HandleMaxHealthChanged;
            playerEvents.OnExpChanged += HandleExpChanged;

            RefreshAll();
        }

        private void UnbindPlayer()
        {
            UnbindPlayerEvents();
            playerStats = null;
            playerHealth = null;
        }

        private void UnbindPlayerEvents()
        {
            if (playerEvents == null) return;

            playerEvents.OnHealthChanged -= HandleHealthChanged;
            playerEvents.OnAttackDamageChanged -= HandleAttackDamageChanged;
            playerEvents.OnAttackSpeedChanged -= HandleAttackSpeedChanged;
            playerEvents.OnIncreaseMoveSpeed -= HandleMoveSpeedChanged;
            playerEvents.OnMaxHealthChanged -= HandleMaxHealthChanged;
            playerEvents.OnExpChanged -= HandleExpChanged;
            playerEvents = null;
        }

        private void HandleLevelUp(int level)
        {
            if (levelLabel != null)
            {
                levelLabel.text = $"Lv.{level}";
            }

            RefreshExp();
        }

        private void HandleHealthChanged(int current, int max)
        {
            SetBar(hpFill, hpValueLabel, current, max);
        }

        private void HandleMaxHealthChanged(int maxHealth)
        {
            if (playerHealth == null) return;
            HandleHealthChanged(playerHealth.GetCurrentHealth(), maxHealth);
        }

        private void HandleAttackDamageChanged(int attack) => SetStat(atkLabel, "ATK", attack);

        private void HandleAttackSpeedChanged(float cooldown) => SetStat(asLabel, "AS", cooldown, 2);

        private void HandleMoveSpeedChanged(int moveSpeed) => SetStat(msLabel, "MS", moveSpeed);

        private void HandleExpChanged(int current, int required) => SetBar(expFill, expValueLabel, current, required);

        private void HandleExpChanged(int _) => RefreshExp();

        private void RefreshAll()
        {
            if (playerStats == null) return;

            if (levelLabel != null)
            {
                levelLabel.text = $"Lv.{playerStats.currentLevel}";
            }

            if (playerHealth != null)
            {
                HandleHealthChanged(playerHealth.GetCurrentHealth(), playerStats.GetMaxHealth());
            }
            else
            {
                SetBar(hpFill, hpValueLabel, playerStats.GetMaxHealth(), playerStats.GetMaxHealth());
            }

            RefreshExp();
            SetStat(atkLabel, "ATK", playerStats.GetAttackDamage());
            SetStat(asLabel, "AS", playerStats.GetAttackCooldown(), 2);
            SetStat(msLabel, "MS", playerStats.GetMoveSpeed());
        }

        private void RefreshExp()
        {
            if (playerStats == null) return;
            HandleExpChanged(playerStats.currentExp, playerStats.expToNextLevel);
        }

        private static void SetBar(VisualElement fill, Label valueLabel, int current, int max)
        {
            if (fill != null)
            {
                var ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
                fill.style.width = Length.Percent(ratio * 100f);
            }

            if (valueLabel != null)
            {
                valueLabel.text = max > 0 ? $"{current}/{max}" : "0/0";
            }
        }

        private static void SetStat(Label label, string prefix, float value, int decimals = 0)
        {
            if (label == null) return;
            label.text = decimals > 0
                ? $"{prefix}: {value.ToString($"F{decimals}")}"
                : $"{prefix}: {Mathf.RoundToInt(value)}";
        }

        private static void SetStat(Label label, string prefix, int value) => SetStat(label, prefix, value, 0);
    }
}
