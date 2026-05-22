using UnityEngine;

namespace EnemyController
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        
        private EnemyEvents events;
        private void Start()
        {
            events = GetComponent<EnemyEvents>();
            currentHealth = maxHealth;
        }
        
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            events.ChangeHealth((int)currentHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void Die()
        {
            Debug.Log("Enemy died!");
            Global.GlobalEvents.RaiseEnemyDie();
            Global.GlobalEntities.Instance.UnregisterEnemy(gameObject);
            Destroy(gameObject);
        }
    }
}
