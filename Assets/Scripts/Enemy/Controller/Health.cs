using UnityEngine;

namespace EnemyController
{
    public class Health : MonoBehaviour
    {

        [Header("Flash Settings")]
        [SerializeField] private Color defaultFlashColor = Color.white; // Mặc định nháy màu trắng xóa
        [SerializeField] private Color? customFlashColor = null;
        [SerializeField] private float flashDuration = 0.1f; // Nháy trong 0.1 giây là vừa đẹp

        private CombatFeel.HitFlash hitFlash;

        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        private bool isDead = false;
        private EnemyEvents events;
        private void Start()
        {
            events = GetComponent<EnemyEvents>();
            currentHealth = maxHealth;
            // Khởi tạo bộ nháy màu cho chính con quái này
            hitFlash = new CombatFeel.HitFlash(gameObject);
        }
        
        public void TakeDamage(int damage)
        {
            if (this == null || gameObject == null || isDead) return;

            currentHealth -= damage;
            events.ChangeHealth((int)currentHealth);
            Color colorToFlash = customFlashColor ?? defaultFlashColor;
            hitFlash?.Play(colorToFlash, flashDuration).Forget();
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void Die()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log("Enemy died!");
            Global.GlobalEvents.RaiseEnemyDie();
            Global.GlobalEntities.Instance.UnregisterEnemy(gameObject);
            Destroy(gameObject);
        }
    }
}
