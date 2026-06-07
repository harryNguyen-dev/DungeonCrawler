using System.Collections.Generic;
using Core;
using SO;
using UnityEngine;

namespace PlayerController.Skill
{
    public static class SkillProjectileSpawner
    {
        public static GameObject Spawn(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            int damage,
            Dictionary<WeaponEffectType, float> effects,
            float speed = 0f)
        {
            if (prefab == null)
                return null;

            PoolId poolId = PoolId.None;
            if (prefab.TryGetComponent<PooledObject>(out var poolable))
                poolId = poolable.PoolId;

            GameObject instance = null;
            if (poolId != PoolId.None && ProjectilePool.Instance != null)
                instance = ProjectilePool.Instance.Get(poolId, position, rotation);
            else
                instance = Object.Instantiate(prefab, position, rotation);

            if (instance == null)
                return null;

            var projectileController = instance.GetComponent<Projectile.ProjectileController>();
            if (projectileController != null)
            {
                projectileController.SetDamage(damage);
                projectileController.SetEffects(effects ?? new Dictionary<WeaponEffectType, float>());
                projectileController.SetProjectileActive();
            }

            var move = instance.GetComponent<Projectile.ProjectileMove>();
            if (move != null && speed > 0f)
                move.SetSpeed(speed);

            return instance;
        }
    }
}
