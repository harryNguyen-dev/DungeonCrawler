using UnityEngine;

namespace Core.Minimap
{
    public struct MinimapZoneBounds
    {
        public Vector2 MinXZ;
        public Vector2 MaxXZ;
        public Vector2Int GridPos;
        public bool IsValid;

        public static MinimapZoneBounds FromCollider(Collider collider, Vector2Int gridPos)
        {
            if (collider == null)
                return Invalid(gridPos);

            Bounds bounds = collider.bounds;
            if (bounds.size.x <= 0.01f || bounds.size.z <= 0.01f)
                return Invalid(gridPos);

            return new MinimapZoneBounds
            {
                GridPos = gridPos,
                MinXZ = new Vector2(bounds.min.x, bounds.min.z),
                MaxXZ = new Vector2(bounds.max.x, bounds.max.z),
                IsValid = true
            };
        }

        public static MinimapZoneBounds Invalid(Vector2Int gridPos)
        {
            return new MinimapZoneBounds { GridPos = gridPos, IsValid = false };
        }

        public bool ContainsWorldPosition(Vector3 worldPos)
        {
            if (!IsValid)
                return false;

            return worldPos.x >= MinXZ.x && worldPos.x <= MaxXZ.x
                && worldPos.z >= MinXZ.y && worldPos.z <= MaxXZ.y;
        }

        public Vector2 NormalizeWorldPosition(Vector3 worldPos)
        {
            if (!IsValid)
                return new Vector2(0.5f, 0.5f);

            float x = Mathf.InverseLerp(MinXZ.x, MaxXZ.x, worldPos.x);
            float y = Mathf.InverseLerp(MinXZ.y, MaxXZ.y, worldPos.z);
            return new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(y));
        }
    }
}
