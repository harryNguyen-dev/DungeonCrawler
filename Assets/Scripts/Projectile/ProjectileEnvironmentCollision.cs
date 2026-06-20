using UnityEngine;

namespace Projectile
{
    public static class ProjectileEnvironmentCollision
    {
        private const float SkinWidth = 0.05f;

        private static readonly int TileLayer = LayerMask.NameToLayer("Tile");
        private static readonly int GroundLayer = LayerMask.NameToLayer("Ground");
        private static readonly LayerMask EnvironmentMask = (1 << TileLayer) | (1 << GroundLayer);

        public static bool IsEnvironmentCollider(Collider collider)
        {
            if (collider == null || collider.isTrigger)
                return false;

            var layer = collider.gameObject.layer;
            return layer == TileLayer || layer == GroundLayer;
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
    }
}
