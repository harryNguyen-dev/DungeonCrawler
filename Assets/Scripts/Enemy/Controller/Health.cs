using UnityEngine;

namespace EnemyController
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private float currentHealth;
        
        private void Start()
        {
            currentHealth = maxHealth;
        }
        
        public void TakeDamage(float damage)
        {
            Debug.Log($"[EnemyTakeDamage] {damage}");
            currentHealth -= damage;
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
