using UnityEngine;

namespace PlayerController
{

    public class Health : MonoBehaviour
    {
        PlayerStats playerStats;
        int currentHealth = 0;
        int maxHealth = 0;
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            maxHealth = playerStats.GetMaxHealth();
            currentHealth = maxHealth;
            playerStats.OnMaxHealthChanged += SetMaxHealth;
            playerStats.OnHealHealth += SetHealHealth;
        }
        private void SetMaxHealth(int maxHealth) => currentHealth = maxHealth;
        private void SetHealHealth(int amount) => currentHealth += amount;

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Eliminate();
            }
        }
        public void Eliminate()
        {
            Global.GlobalEvents.RaisePlayerEliminated();
            Debug.Log("Player died!");
            
            playerStats.OnMaxHealthChanged -= SetMaxHealth;
            playerStats.OnHealHealth -= SetHealHealth;
            Time.timeScale = 0f;
            Global.GlobalEntities.Instance.ClearPlayer();
        }
    }

}