using UnityEngine;

namespace PlayerController
{

    public class Health : MonoBehaviour
    {
        PlayerStats playerStats;
        float currentHealth = 0;
        private void Start()
        {
            playerStats = GetComponent<PlayerStats>();
            currentHealth = playerStats.GetHealth();
        }

        public void TakeDamage(float damage)
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
            Time.timeScale = 0f;
            Global.GlobalEntities.Instance.ClearPlayer();
        }
    }

}