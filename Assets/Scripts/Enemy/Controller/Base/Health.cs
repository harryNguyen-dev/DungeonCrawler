using System.Threading;
using UnityEngine;
using Core;

namespace EnemyController
{
    public class Health : MonoBehaviour, IPoolable
    {

        [Header("Flash Settings")]
        [SerializeField] private Color defaultFlashColor = Color.white; // Mặc định nháy màu trắng xóa
        [SerializeField] private Color? customFlashColor = null;
        [SerializeField] private float flashDuration = 0.1f; // Nháy trong 0.1 giây là vừa đẹp

        private CombatFeel.HitFlash hitFlash;
        public CombatFeel.HitFlash HitFlash => hitFlash;
        private CancellationTokenSource statusEffectCancellation;

        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        [SerializeField] private SO.EnemySO enemyData;
        private bool isDead = false;
        private EnemyEvents events;
        private void Start()
        {
            events = GetComponent<EnemyEvents>();
            hitFlash = new CombatFeel.HitFlash(gameObject);
            ResetStatusEffectCancellation();
            ResetHealth();
        }

        public void OnSpawnedFromPool()
        {
            ResetStatusEffectCancellation();
            ResetHealth();
        }

        public void OnReturnedToPool()
        {
            CancelStatusEffects();
        }

        private void ResetHealth()
        {
            if (enemyData != null) maxHealth = enemyData.MaxHealth; // Gán từ SO
            currentHealth = maxHealth;
            isDead = false;
            events?.ChangeHealth(currentHealth);
        }

        private void ResetStatusEffectCancellation()
        {
            CancelStatusEffects();
            statusEffectCancellation = new CancellationTokenSource();
        }

        private void CancelStatusEffects()
        {
            if (statusEffectCancellation != null)
            {
                if (!statusEffectCancellation.IsCancellationRequested)
                    statusEffectCancellation.Cancel();
                statusEffectCancellation.Dispose();
                statusEffectCancellation = null;
            }
        }

        public CancellationToken GetStatusEffectCancellationToken()
        {
            return statusEffectCancellation?.Token ?? CancellationToken.None;
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

            CancelStatusEffects();

            Debug.Log("Enemy died!");
            Global.GlobalEvents.RaiseEnemyDie();
            Global.GlobalEntities.Instance?.UnregisterEnemy(gameObject);

            if (Core.ObjectPoolingManager.Instance != null)
            {
                Core.ObjectPoolingManager.Instance.Return(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
