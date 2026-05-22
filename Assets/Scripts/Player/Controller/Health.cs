using UnityEngine;

namespace PlayerController
{

    public class Health : MonoBehaviour
    {
        PlayerStats playerStats;
        int currentHealth = 0;
        int maxHealth = 0;
        private PlayerEvents events;
        private void Start()
        {
            events = GetComponent<PlayerEvents>();
            playerStats = GetComponent<PlayerStats>();
            maxHealth = playerStats.GetMaxHealth();
            currentHealth = maxHealth;
            events.OnMaxHealthChanged += SetMaxHealth;
            events.OnHealHealth += SetHealHealth;
        }
        private void SetMaxHealth(int maxHealth) => currentHealth = maxHealth;
        private void SetHealHealth(int amount) => currentHealth += amount;

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
            
            events.OnMaxHealthChanged -= SetMaxHealth;
            events.OnHealHealth -= SetHealHealth;
            Time.timeScale = 0f;
            Global.GlobalEntities.Instance.ClearPlayer();
        }
        public int GetCurrentHealth() => currentHealth;
    }

}