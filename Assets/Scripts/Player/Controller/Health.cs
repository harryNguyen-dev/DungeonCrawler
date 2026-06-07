using UnityEngine;

namespace PlayerController
{

    public class Health : MonoBehaviour
    {
        PlayerStats playerStats;
        int currentHealth = 0;
        int maxHealth = 0;
        private PlayerEvents events;
        public bool IsDead => currentHealth <= 0;
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
            var scaledAmount = Mathf.RoundToInt(amount * playerStats.GetHealMultiplier());
            currentHealth = Mathf.Min(currentHealth + scaledAmount, maxHealth);
            events.InvokeChangeHealth(currentHealth, maxHealth);
        }

        private bool invulnerable;

        public void SetInvulnerable(bool value) => invulnerable = value;

        public void TakeDamage(int damage, EnemyController.Health attacker = null)
        {
            if (invulnerable)
                return;

            currentHealth -= damage;
            Debug.Log("[PlayerController Health] Health: " + currentHealth);
            events.InvokeChangeHealth(currentHealth, maxHealth);
            ApplyThornReflect(damage, attacker);

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
            Global.GlobalEvents.RaisePlayerEliminated();
            Debug.Log("Player died!");
            Time.timeScale = 0f;
            Global.GlobalEntities.Instance.ClearPlayer();
        }
        public int GetCurrentHealth() => currentHealth;
    }

}
