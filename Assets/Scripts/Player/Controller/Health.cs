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
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            events.InvokeChangeHealth(currentHealth, maxHealth);
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            Debug.Log("[PlayerController Health] Health: " + currentHealth);
            events.InvokeChangeHealth(currentHealth, maxHealth);
            if (currentHealth <= 0)
            {
                Eliminate();
            }
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
