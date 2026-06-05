using EnemyController;
using Global;
using UnityEngine;

namespace PlayerController
{
    public class Rotate : MonoBehaviour
    {
        [SerializeField] private float moveRotateSpeed = 15f;

        /// <summary>Hướng ngắm tới enemy sống gần nhất trên mặt phẳng ngang.</summary>
        public bool TryGetAimDirection(out Vector3 aimDirection)
        {
            aimDirection = Vector3.zero;

            var entities = GlobalEntities.Instance;
            if (entities == null || entities.AvailableEnemies.Count == 0)
                return false;

            Transform nearest = null;
            float nearestSqrDist = float.MaxValue;
            Vector3 origin = transform.position;

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

            if (nearest == null)
                return false;

            aimDirection = nearest.position - origin;
            aimDirection.y = 0f;
            return aimDirection.sqrMagnitude > 0.0001f;
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

        /// <summary>Xoay tức thì về enemy gần nhất trước khi đánh.</summary>
        public void SnapFaceAimDirection()
        {
            if (!TryGetAimDirection(out Vector3 aimDirection))
                return;

            transform.rotation = Quaternion.LookRotation(aimDirection.normalized);
        }
    }
}
