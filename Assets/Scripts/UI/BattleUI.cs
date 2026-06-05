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

        [Header("Skill buttons")]
        [SerializeField] private Button normalAttackButton;

        private EventTrigger attackButtonTrigger;
        private EventTrigger.Entry attackPointerDownEntry;
        private EventTrigger.Entry attackPointerUpEntry;
        private EventTrigger.Entry attackPointerExitEntry;

        private PlayerEvents playerEvents;
        private PlayerStats playerStats;
        private Health playerHealth;

        private int runGold;
        private int enemiesKilled;

        private void OnEnable()
        {
            GlobalEvents.OnPlayerJoin += BindPlayer;
            GlobalEvents.OnMatchReset += HandleMatchReset;
            GlobalEvents.OnDungeonGenerated += HandleDungeonGenerated;
            GlobalEvents.OnRunStarsChanged += HandleRunStarsChanged;
            GlobalEvents.OnEnemyDie += HandleEnemyKilled;

            if (GlobalEntities.Instance?.PlayerEvents != null)
            {
                BindPlayer();
            }

            RefreshCurrency();
            StarDisplayHelper.Apply(star1, star2, star3, 0);
            WireAttackButton();
        }

        private void OnDisable()
        {
            GlobalEvents.OnPlayerJoin -= BindPlayer;
            GlobalEvents.OnMatchReset -= HandleMatchReset;
            GlobalEvents.OnDungeonGenerated -= HandleDungeonGenerated;
            GlobalEvents.OnRunStarsChanged -= HandleRunStarsChanged;
            GlobalEvents.OnEnemyDie -= HandleEnemyKilled;
            UnbindPlayerEvents();
            UnwireAttackButton();
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
            playerEvents.OnMaxHealthChanged += HandleMaxHealthChanged;
            playerEvents.OnExpChanged += HandleExpChanged;

            RefreshAll();
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
            runGold = 0;
            enemiesKilled = 0;
            RefreshCurrency();
            StarDisplayHelper.Apply(star1, star2, star3, 0);
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

        private void HandleEnemyKilled(int goldDropped)
        {
            enemiesKilled++;

            var multiplier = GlobalEntities.Instance?.PlayerStats?.runtimeStats?.DefaultGoldGainMultiplier ?? 1f;
            runGold += Mathf.RoundToInt(goldDropped * multiplier);
            RefreshCurrency();

            RefreshExp();
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
    }
}
