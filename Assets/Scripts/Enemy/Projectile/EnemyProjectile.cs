using System.Collections.Generic;
using Core;
using Projectile;
using UnityEngine;

namespace EnemyController
{
    public class EnemyProjectile : MonoBehaviour, IPoolable
    {
        [Header("Projectile Movement")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private float maxLifetime = 5f;

        [Header("VFX")]
        [SerializeField] private GameObject muzzlePrefab;
        [SerializeField] private GameObject hitPrefab;
        [SerializeField] private bool rotate;
        [SerializeField] private float rotateAmount = 45f;
        [SerializeField] private List<GameObject> trails = new();

        private int damage;
        private float currentLifetime;
        private bool isInitialized;
        private bool isDespawning;

        public void Setup(int damageValue)
        {
            damage = damageValue;
            currentLifetime = 0f;
            isInitialized = true;

            ProjectileVfxHelper.PlayMuzzle(muzzlePrefab, transform.position, transform.forward);
            ProjectileVfxHelper.RestartProjectileVisuals(gameObject, trails);
        }

        private void Update()
        {
            if (!isInitialized || isDespawning) return;

            if (rotate)
                transform.Rotate(0f, 0f, rotateAmount, Space.Self);

            var step = speed * Time.deltaTime;
            if (ProjectileEnvironmentCollision.TryGetBlockHit(transform.position, transform.forward, step, out var hit))
            {
                ReturnToPool(hit.point, hit.normal);
                return;
            }

            transform.Translate(Vector3.forward * step);

            currentLifetime += Time.deltaTime;
            if (currentLifetime >= maxLifetime)
                ReturnToPool();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDespawning || !isInitialized) return;

            if (ProjectileEnvironmentCollision.IsEnvironmentCollider(other))
            {
                ReturnToPool(other.ClosestPoint(transform.position), -transform.forward);
                return;
            }

            if (!IsPlayerHit(other)) return;
            if (damage <= 0) return;

            var playerHealth = other.GetComponentInParent<PlayerController.Health>()
                ?? Global.GlobalEntities.Instance?.PlayerHealth;
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

            var hitPoint = other.ClosestPoint(transform.position);
            ReturnToPool(hitPoint, -transform.forward);
        }

        private static bool IsPlayerHit(Collider other)
        {
            if (other.CompareTag("Player")) return true;
            return other.gameObject.layer == LayerMask.NameToLayer("Player");
        }

        private void ReturnToPool(Vector3? hitPoint = null, Vector3? hitNormal = null)
        {
            if (isDespawning) return;
            isDespawning = true;

            if (hitPoint.HasValue)
                ProjectileVfxHelper.SpawnHit(hitPrefab, hitPoint.Value, hitNormal ?? -transform.forward);

            isInitialized = false;
            ProjectileVfxHelper.ResetTrails(trails);
            PoolReturn.SafeReturn(gameObject);
        }

        public void OnSpawnedFromPool()
        {
            currentLifetime = 0f;
            isInitialized = false;
            isDespawning = false;
        }

        public void OnReturnedToPool()
        {
            isInitialized = false;
            isDespawning = false;
            ProjectileVfxHelper.ResetTrails(trails);
        }
    }
}
