using System.Threading;
using UnityEngine;
using Core;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

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
        private int baseMaxHealth;
        private int poolLifeId;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        [SerializeField] private SO.EnemySO enemyData;
        private bool isBossInstance;
        private bool bossSetupPending;
        private float bossHpMultiplier = 3f;
        private bool isDead = false;
        public bool IsDead => isDead;
        private EnemyEvents events;
        private BaseEnemyAnimation baseEnemyAnimation;

        /// <summary>Gán boss cho encounter phòng boss (HP scale khi prefab chưa có isBoss trên SO).</summary>
        public void ConfigureAsBoss(float hpMultiplier = 3f)
        {
            isBossInstance = true;
            bossHpMultiplier = hpMultiplier;
            bossSetupPending = true;
            if (events != null)
                TryApplyBossSetup();
        }

        public void ApplyRuntimeHealthScale(float healthMultiplier)
        {
            if (healthMultiplier <= 0f || Mathf.Approximately(healthMultiplier, 1f))
                return;

            CacheBaseMaxHealth();
            maxHealth = Mathf.Max(1, Mathf.RoundToInt(baseMaxHealth * healthMultiplier));
            currentHealth = maxHealth;
            events?.ChangeHealth(currentHealth);
        }

        private void Awake()
        {
            CacheBaseMaxHealth();
        }

        private void Start()
        {
            EnsureInitialized();
            ResetStatusEffectCancellation();
            ResetHealth();
            TryApplyBossSetup();
        }

        private void TryApplyBossSetup()
        {
            if (!bossSetupPending) return;
            bossSetupPending = false;

            bool dataIsBoss = enemyData != null && enemyData.isBoss;
            if (!dataIsBoss && bossHpMultiplier > 1f)
            {
                int scaled = Mathf.RoundToInt(maxHealth * bossHpMultiplier);
                maxHealth = scaled;
                currentHealth = scaled;
                events?.ChangeHealth(currentHealth);
            }
        }

        public void OnSpawnedFromPool()
        {
            poolLifeId++;
            EnsureInitialized();
            isBossInstance = false;
            bossSetupPending = false;
            isDead = false;
            ResetStatusEffectCancellation();
            ResetHealth();
        }

        public void OnReturnedToPool()
        {
            CancelStatusEffects();
            isDead = false;
            ResetHealth();
        }

        private void EnsureInitialized()
        {
            if (events == null)
                events = GetComponent<EnemyEvents>();
            if (baseEnemyAnimation == null)
                baseEnemyAnimation = GetComponent<BaseEnemyAnimation>();
            if (hitFlash == null)
                hitFlash = new CombatFeel.HitFlash(gameObject);
        }

        private void CacheBaseMaxHealth()
        {
            if (enemyData != null)
                baseMaxHealth = enemyData.MaxHealth;
            else if (baseMaxHealth <= 0)
                baseMaxHealth = maxHealth;
        }

        private void ResetHealth()
        {
            CacheBaseMaxHealth();
            maxHealth = baseMaxHealth;
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
            baseEnemyAnimation?.SetHitTrigger();
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
            var goldDrop = enemyData != null ? enemyData.GoldDrop : 5;
            var expDrop = enemyData != null ? enemyData.ExpDrop : 20;
            baseEnemyAnimation?.SetDieTrigger();
            CancelStatusEffects();

            DropPool.Instance?.SpawnFromEnemy(transform.position, goldDrop, expDrop);

            Debug.Log("Enemy died!");
            Global.GlobalEvents.RaiseEnemyDie(0);
            ReturnToPoolAsync().Forget();
        }
        private async UniTaskVoid ReturnToPoolAsync()
        {
            int returnLifeId = poolLifeId;
            await UniTask.Delay(1300);

            if (this == null || gameObject == null || poolLifeId != returnLifeId)
                return;

            if (!gameObject.activeInHierarchy)
                return;

            if (isBossInstance || (enemyData != null && enemyData.isBoss))
            {
                Global.GlobalEvents.RaiseBossDefeated();
            }
            Global.GlobalEntities.Instance?.UnregisterEnemy(gameObject);

            if (Core.EnemyPool.Instance != null)
            {
                Core.EnemyPool.Instance.Return(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
