using Core.Minimap;
using UnityEngine;
using UnityEngine.AI;

namespace PlayerController
{
    public static class DashTargetResolver
    {
        private const float NavMeshSampleRadius = 1.5f;
        private const float NavMeshTargetSampleRadius = 0.75f;
        private const float RoomBoundsMargin = 0.35f;
        private const float MinDashDistance = 0.25f;

        public static bool TryResolve(
            Vector3 start,
            Vector3 direction,
            float maxDistance,
            MinimapZoneBounds? roomBounds,
            out Vector3 target)
        {
            target = start;

            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return false;

            direction.Normalize();
            start.y = 0f;

            if (!NavMesh.SamplePosition(start, out NavMeshHit startHit, NavMeshSampleRadius, NavMesh.AllAreas))
                return false;

            Vector3 navStart = startHit.position;
            float allowedDistance = maxDistance;

            Vector3 navEnd = navStart + direction * maxDistance;
            if (NavMesh.Raycast(navStart, navEnd, out NavMeshHit navHit, NavMesh.AllAreas))
                allowedDistance = Mathf.Min(allowedDistance, navHit.distance);

            if (roomBounds.HasValue && roomBounds.Value.IsValid)
            {
                float boundsDistance = GetRayAabbExitDistance(
                    navStart, direction, roomBounds.Value, maxDistance, RoomBoundsMargin);
                allowedDistance = Mathf.Min(allowedDistance, boundsDistance);
            }

            if (allowedDistance < MinDashDistance)
                return false;

            target = navStart + direction * allowedDistance;
            target.y = 0f;

            if (NavMesh.SamplePosition(target, out NavMeshHit endHit, NavMeshTargetSampleRadius, NavMesh.AllAreas))
                target = endHit.position;

            return Vector3.Distance(navStart, target) >= MinDashDistance;
        }

        private static float GetRayAabbExitDistance(
            Vector3 origin,
            Vector3 dir,
            MinimapZoneBounds bounds,
            float maxDist,
            float margin)
        {
            float minX = bounds.MinXZ.x + margin;
            float maxX = bounds.MaxXZ.x - margin;
            float minZ = bounds.MinXZ.y + margin;
            float maxZ = bounds.MaxXZ.y - margin;

            float tMin = float.MaxValue;

            if (dir.x > 0.0001f)
                tMin = Mathf.Min(tMin, (maxX - origin.x) / dir.x);
            else if (dir.x < -0.0001f)
                tMin = Mathf.Min(tMin, (minX - origin.x) / dir.x);

            if (dir.z > 0.0001f)
                tMin = Mathf.Min(tMin, (maxZ - origin.z) / dir.z);
            else if (dir.z < -0.0001f)
                tMin = Mathf.Min(tMin, (minZ - origin.z) / dir.z);

            if (tMin == float.MaxValue)
                return 0f;

            return Mathf.Clamp(tMin, 0f, maxDist);
        }
    }
}
