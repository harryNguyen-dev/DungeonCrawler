using UnityEngine;

namespace Projectile
{
    public static class ProjectileEnvironmentCollision
    {
        private const float SkinWidth = 0.05f;

        private static readonly int DefaultLayer = 0;
        private static readonly int TileLayer = LayerMask.NameToLayer("Tile");
        private static readonly int GroundLayer = LayerMask.NameToLayer("Ground");
        private static readonly LayerMask EnvironmentMask =
            (1 << DefaultLayer) | (1 << TileLayer) | (1 << GroundLayer);

        public static LayerMask BlockMask => EnvironmentMask;

        public static bool IsEnvironmentCollider(Collider collider)
        {
            if (collider == null || collider.isTrigger)
                return false;

            var layer = collider.gameObject.layer;
            return layer == DefaultLayer || layer == TileLayer || layer == GroundLayer;
        }

        public static bool TryGetBlockHit(Vector3 origin, Vector3 direction, float distance, out RaycastHit hit)
        {
            hit = default;
            if (distance <= 0f)
                return false;

            if (direction.sqrMagnitude < 0.0001f)
                return false;

            direction.Normalize();
            var rayOrigin = origin + direction * SkinWidth;

            return Physics.Raycast(
                rayOrigin,
                direction,
                out hit,
                distance + SkinWidth,
                EnvironmentMask,
                QueryTriggerInteraction.Ignore);
        }

        public static Vector3 ResolvePlanarMovement(
            Vector3 currentPosition,
            Vector3 delta,
            CapsuleCollider capsule,
            Collider ignoreCollider = null)
        {
            if (capsule == null || delta.sqrMagnitude < 0.0001f)
                return currentPosition;

            delta.y = 0f;
            var resolved = currentPosition;
            resolved = MoveAlongAxis(resolved, new Vector3(delta.x, 0f, 0f), capsule, ignoreCollider);
            resolved = MoveAlongAxis(resolved, new Vector3(0f, 0f, delta.z), capsule, ignoreCollider);
            return resolved;
        }

        private static Vector3 MoveAlongAxis(
            Vector3 currentPosition,
            Vector3 delta,
            CapsuleCollider capsule,
            Collider ignoreCollider)
        {
            if (delta.sqrMagnitude < 0.0001f)
                return currentPosition;

            GetCapsuleWorldSpace(capsule, currentPosition, out var point1, out var point2, out var radius);

            var direction = delta.normalized;
            var distance = delta.magnitude;

            if (!Physics.CapsuleCast(
                    point1,
                    point2,
                    radius,
                    direction,
                    out var hit,
                    distance + SkinWidth,
                    EnvironmentMask,
                    QueryTriggerInteraction.Ignore))
            {
                return currentPosition + delta;
            }

            if (ignoreCollider != null && hit.collider == ignoreCollider)
                return currentPosition + delta;

            var allowedDistance = Mathf.Max(0f, hit.distance - SkinWidth);
            return currentPosition + direction * allowedDistance;
        }

        private static void GetCapsuleWorldSpace(
            CapsuleCollider capsule,
            Vector3 position,
            out Vector3 point1,
            out Vector3 point2,
            out float radius)
        {
            var capsuleTransform = capsule.transform;
            var worldCenter = position + capsuleTransform.TransformVector(capsule.center);
            radius = capsule.radius * Mathf.Max(capsuleTransform.lossyScale.x, capsuleTransform.lossyScale.z);
            var height = capsule.height * capsuleTransform.lossyScale.y;

            var axis = capsule.direction switch
            {
                0 => capsuleTransform.right,
                2 => capsuleTransform.forward,
                _ => capsuleTransform.up
            };

            var halfHeight = Mathf.Max(0f, height * 0.5f - radius);
            point1 = worldCenter - axis * halfHeight;
            point2 = worldCenter + axis * halfHeight;
        }
    }
}
