using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlayerController
{

    public class Health : MonoBehaviour
    {
        private const float ReviveInvulnerabilityDuration = 3f;

        PlayerStats playerStats;
        int currentHealth = 0;
        int maxHealth = 0;
        private PlayerEvents events;
        private bool isEliminated;
        public bool IsDead => currentHealth <= 0 || isEliminated;
        private void Awake()
        {
            events = GetComponent<PlayerEvents>();
            playerStats = GetComponent<PlayerStats>();
            maxHealth = playerStats.GetMaxHealth();
            currentHealth = maxHealth;
        }

        private void Start()
        {
            events.OnMaxHealthChanged += SetMaxHealth;
            events.OnHealHealth += SetHealHealth;
            events.InvokeChangeHealth(currentHealth, maxHealth);
        }
        private void OnDestroy()
        {
            if (events == null) return;
            events.OnMaxHealthChanged -= SetMaxHealth;
            events.OnHealHealth -= SetHealHealth;
        }
        private void SetMaxHealth(int newMaxHealth)
        {
            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            events.InvokeChangeHealth(currentHealth, maxHealth);
        }
        private void SetHealHealth(int amount)
        {
            if (amount <= 0)
                return;

            var scaledAmount = Mathf.RoundToInt(amount * playerStats.GetHealMultiplier());
            currentHealth = Mathf.Min(currentHealth + scaledAmount, maxHealth);
            Core.GameAudio.PlayPlayerHeal();
            events.InvokeChangeHealth(currentHealth, maxHealth);
        }

        private bool invulnerable;

        public void SetInvulnerable(bool value) => invulnerable = value;

        public void TakeDamage(int damage, EnemyController.Health attacker = null)
        {
            if (invulnerable || isEliminated)
                return;

            var armor = playerStats != null ? playerStats.GetArmor() : 0;
            var mitigated = Mathf.Max(1, damage - armor);
            currentHealth -= mitigated;
            Core.GameAudio.PlayPlayerHit(transform.position);
            Debug.Log("[PlayerController Health] Health: " + currentHealth);
            events.InvokeChangeHealth(currentHealth, maxHealth);
            ApplyThornReflect(mitigated, attacker);

            if (currentHealth <= 0)
                Eliminate();
        }

        private void ApplyThornReflect(int damageTaken, EnemyController.Health attacker)
        {
            if (attacker == null || attacker.IsDead || damageTaken <= 0)
                return;

            var reflectPercent = playerStats.GetThornReflectPercent();
            if (reflectPercent <= 0f)
                return;

            var reflectDamage = Mathf.Max(1, Mathf.RoundToInt(damageTaken * reflectPercent));
            attacker.TakeDamage(reflectDamage);
        }
        public void Eliminate()
        {
            if (isEliminated)
                return;

            isEliminated = true;
            currentHealth = 0;
            events.InvokeChangeHealth(currentHealth, maxHealth);
            SetInvulnerable(true);

            var movement = GetComponent<Movement>();
            movement?.SetMovementEnabled(false);
            Global.GlobalEntities.Instance?.SetPlayerCombatEnabled(false);

            Core.GameAudio.PlayPlayerDeath();
            Global.GlobalEvents.RaisePlayerEliminated();
            Debug.Log("Player died!");
            Time.timeScale = 0f;
        }

        public void Revive()
        {
            if (!isEliminated)
                return;

            isEliminated = false;
            currentHealth = Mathf.Max(1, maxHealth / 2);
            events.InvokeChangeHealth(currentHealth, maxHealth);
            Core.GameAudio.PlayPlayerHeal();

            var movement = GetComponent<Movement>();
            movement?.SetMovementEnabled(true);
            Global.GlobalEntities.Instance?.SetPlayerCombatEnabled(true);

            SetInvulnerable(true);
            ClearInvulnerabilityAfter(ReviveInvulnerabilityDuration).Forget();
        }

        private async UniTaskVoid ClearInvulnerabilityAfter(float seconds)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: false);
            if (this == null || isEliminated)
                return;

            SetInvulnerable(false);
        }

        public int GetCurrentHealth() => currentHealth;
    }

}
