using Components;
using EnemyController;
using Global;
using UnityEngine;

namespace PlayerController
{
    public class Rotate : MonoBehaviour
    {
        [SerializeField] private float moveRotateSpeed = 15f;

        /// <summary>Hướng ngắm tới enemy sống gần nhất; nếu không có thì rương gần nhất.</summary>
        public bool TryGetAimDirection(out Vector3 aimDirection)
        {
            aimDirection = Vector3.zero;
            Vector3 origin = transform.position;

            Transform nearest = TryFindNearestEnemy(origin, out float nearestSqrDist);
            if (nearest == null)
                nearest = TryFindNearestChest(origin, nearestSqrDist);

            if (nearest == null)
                return false;

            aimDirection = nearest.position - origin;
            aimDirection.y = 0f;
            return aimDirection.sqrMagnitude > 0.0001f;
        }

        private static Transform TryFindNearestEnemy(Vector3 origin, out float nearestSqrDist)
        {
            nearestSqrDist = float.MaxValue;
            Transform nearest = null;

            var entities = GlobalEntities.Instance;
            if (entities == null)
                return null;

            foreach (var enemy in entities.AvailableEnemies)
            {
                if (enemy == null) continue;

                var health = enemy.GetComponent<Health>();
                if (health != null && health.IsDead) continue;

                Vector3 offset = enemy.transform.position - origin;
                offset.y = 0f;
                float sqrDist = offset.sqrMagnitude;
                if (sqrDist >= nearestSqrDist) continue;

                nearestSqrDist = sqrDist;
                nearest = enemy.transform;
            }

            return nearest;
        }

        private static Transform TryFindNearestChest(Vector3 origin, float nearestSqrDist)
        {
            Transform nearest = null;
            var chests = Object.FindObjectsByType<Chest>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            foreach (var chest in chests)
            {
                if (chest == null || chest.IsDestroyed) continue;

                Vector3 offset = chest.transform.position - origin;
                offset.y = 0f;
                float sqrDist = offset.sqrMagnitude;
                if (sqrDist >= nearestSqrDist) continue;

                nearestSqrDist = sqrDist;
                nearest = chest.transform;
            }

            return nearest;
        }

        public void FaceTowards(Vector3 worldDirection, float rotateSpeed)
        {
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(worldDirection.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotateSpeed);
        }

        public void FaceMovementDirection(Vector3 moveDirection)
        {
            FaceTowards(moveDirection, moveRotateSpeed);
        }

        /// <summary>Xoay tức thì về hướng chỉ định (skill aim).</summary>
        public void SnapFaceDirection(Vector3 worldDirection)
        {
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude < 0.0001f)
                return;

            transform.rotation = Quaternion.LookRotation(worldDirection.normalized);
        }

        /// <summary>Xoay tức thì về enemy gần nhất trước khi đánh.</summary>
        public void SnapFaceAimDirection()
        {
            if (!TryGetAimDirection(out Vector3 aimDirection))
                return;

            transform.rotation = Quaternion.LookRotation(aimDirection.normalized);
        }
    }
}
