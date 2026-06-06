using UnityEngine;

namespace Core
{
    public static class PoolReturn
    {
        public static void SafeReturn(GameObject instance)
        {
            if (instance == null) return;

            if (!instance.TryGetComponent<PooledObject>(out var tag) || tag.PoolId == PoolId.None)
            {
                Object.Destroy(instance);
                return;
            }

            if (PoolCategories.IsEnemy(tag.PoolId))
            {
                if (EnemyPool.Instance != null)
                    EnemyPool.Instance.Return(instance);
                else
                    Object.Destroy(instance);
                return;
            }

            if (PoolCategories.IsProjectile(tag.PoolId))
            {
                if (ProjectilePool.Instance != null)
                    ProjectilePool.Instance.Return(instance);
                else
                    Object.Destroy(instance);
                return;
            }

            Debug.LogWarning($"PoolReturn: unhandled PoolId '{tag.PoolId}' — destroying.");
            Object.Destroy(instance);
        }
    }
}
