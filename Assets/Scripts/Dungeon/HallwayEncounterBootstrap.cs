using UnityEngine;

/// <summary>
/// Ensures hallway tile instances have a trigger + spawn points for RoomController.
/// Called after corridor spawn when prefab has no encounter zone yet.
/// </summary>
public static class HallwayEncounterBootstrap
{
        private const float TriggerHeight = 4f;
        private const float TriggerYCenter = 2f;

        public static RoomController EnsureEncounterZone(GameObject hallwayRoot)
        {
            if (hallwayRoot == null)
                return null;

            var existing = hallwayRoot.GetComponentInChildren<RoomController>();
            if (existing != null)
                return existing;

            var zoneGo = new GameObject("RoomController");
            zoneGo.transform.SetParent(hallwayRoot.transform, false);
            zoneGo.transform.localPosition = Vector3.zero;

            var collider = zoneGo.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            FitTriggerToHallway(hallwayRoot, collider);

            var spawnA = new GameObject("SpawnPoint (1)");
            spawnA.transform.SetParent(zoneGo.transform, false);
            spawnA.transform.localPosition = new Vector3(0f, 0.18f, 0f);

            var spawnB = new GameObject("SpawnPoint (2)");
            spawnB.transform.SetParent(zoneGo.transform, false);
            spawnB.transform.localPosition = new Vector3(0f, 0.18f, 3f);

            var controller = zoneGo.AddComponent<RoomController>();
            controller.SetSpawnPoints(new[] { spawnA.transform, spawnB.transform });

            return controller;
        }

        private static void FitTriggerToHallway(GameObject hallwayRoot, BoxCollider collider)
        {
            var renderers = hallwayRoot.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                collider.size = new Vector3(8f, TriggerHeight, 14f);
                collider.center = new Vector3(0f, TriggerYCenter, 0f);
                return;
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            var localCenter = hallwayRoot.transform.InverseTransformPoint(bounds.center);
            var localSize = bounds.size;
            var scale = hallwayRoot.transform.lossyScale;
            localSize = new Vector3(
                localSize.x / Mathf.Max(scale.x, 0.01f),
                TriggerHeight,
                localSize.z / Mathf.Max(scale.z, 0.01f));

            collider.center = new Vector3(localCenter.x, TriggerYCenter, localCenter.z);
            collider.size = new Vector3(
                Mathf.Max(localSize.x, 4f),
                TriggerHeight,
                Mathf.Max(localSize.z, 4f));
        }
}
