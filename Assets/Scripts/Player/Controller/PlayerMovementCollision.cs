using Projectile;
using UnityEngine;

namespace PlayerController
{
    public static class PlayerMovementCollision
    {
        private const float SkinWidth = 0.05f;
        private const float MovementRadiusScale = 0.72f;
        private const float MaxFloorNormalY = 0.35f;

        public static Vector3 ResolveMovement(
            Vector3 currentPosition,
            Vector3 delta,
            CapsuleCollider capsule,
            Collider bodyCollider)
        {
            if (capsule == null || delta.sqrMagnitude < 0.0001f)
                return currentPosition;

            delta.y = 0f;
            var resolved = currentPosition;
            resolved = MoveAlongAxis(resolved, new Vector3(delta.x, 0f, 0f), capsule, bodyCollider);
            resolved = MoveAlongAxis(resolved, new Vector3(0f, 0f, delta.z), capsule, bodyCollider);
            return resolved;
        }

        private static Vector3 MoveAlongAxis(
            Vector3 currentPosition,
            Vector3 delta,
            CapsuleCollider capsule,
            Collider bodyCollider)
        {
            if (delta.sqrMagnitude < 0.0001f)
                return currentPosition;

            GetCapsuleWorldSpace(
                capsule,
                currentPosition,
                out var point1,
                out var point2,
                out var radius);

            var direction = delta.normalized;
            var distance = delta.magnitude;

            if (!TryGetBlockingHit(point1, point2, radius, direction, distance, bodyCollider, out var hit))
                return currentPosition + delta;

            var allowedDistance = Mathf.Max(0f, hit.distance - SkinWidth);
            return currentPosition + direction * allowedDistance;
        }

        private static bool TryGetBlockingHit(
            Vector3 point1,
            Vector3 point2,
            float radius,
            Vector3 direction,
            float distance,
            Collider bodyCollider,
            out RaycastHit blockingHit)
        {
            blockingHit = default;
            var hits = Physics.CapsuleCastAll(
                point1,
                point2,
                radius,
                direction,
                distance + SkinWidth,
                ProjectileEnvironmentCollision.BlockMask,
                QueryTriggerInteraction.Ignore);

            if (hits == null || hits.Length == 0)
                return false;

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (bodyCollider != null && hit.collider == bodyCollider)
                    continue;

                if (!ShouldBlockHorizontalMovement(hit))
                    continue;

                blockingHit = hit;
                return true;
            }

            return false;
        }

        private static bool ShouldBlockHorizontalMovement(RaycastHit hit)
        {
            if (hit.collider == null)
                return false;

            if (Mathf.Abs(hit.normal.y) > MaxFloorNormalY)
                return false;

            if (IsFloorCollider(hit.collider))
                return false;

            return true;
        }

        private static bool IsFloorCollider(Collider collider)
        {
            var go = collider.gameObject;
            if (go.name == "Floor" || go.name.StartsWith("Floor "))
                return true;

            if (collider is BoxCollider box)
            {
                var size = Vector3.Scale(box.size, go.transform.lossyScale);
                if (size.y <= 0.5f)
                    return true;
            }

            return false;
        }

        private static void GetCapsuleWorldSpace(
            CapsuleCollider capsule,
            Vector3 position,
            out Vector3 point1,
            out Vector3 point2,
            out float worldRadius)
        {
            var capsuleTransform = capsule.transform;
            var worldCenter = position + capsuleTransform.TransformVector(capsule.center);
            var horizontalScale = Mathf.Max(capsuleTransform.lossyScale.x, capsuleTransform.lossyScale.z);
            worldRadius = capsule.radius * MovementRadiusScale * horizontalScale;
            var height = capsule.height * capsuleTransform.lossyScale.y;
            var verticalRadius = capsule.radius * MovementRadiusScale * capsuleTransform.lossyScale.y;

            var axis = capsule.direction switch
            {
                0 => capsuleTransform.right,
                2 => capsuleTransform.forward,
                _ => capsuleTransform.up
            };

            var halfHeight = Mathf.Max(0f, height * 0.5f - verticalRadius);
            point1 = worldCenter - axis * halfHeight;
            point2 = worldCenter + axis * halfHeight;
        }
    }
}
