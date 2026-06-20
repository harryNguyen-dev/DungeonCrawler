using System.Collections.Generic;
using Core.Minimap;
using UnityEngine;

namespace WFC {
    public class Tile {
        public List<WFCData> PossibleTiles;
        public WFCData CollapsedTile; // null if not collapsed
        
        public bool IsCollapsed;

        public Vector2Int GridPosition;
        public int Entropy => PossibleTiles.Count;

        public GameObject SpawnedObject { get; private set; }

        public void ClearPossibleTiles()
        {
            PossibleTiles.Clear();
        }

        public void ResetTile(WFCData[] allTiles)
        {
            DespawnObject();
            IsCollapsed = false;
            CollapsedTile = null;
            PossibleTiles = new List<WFCData>(allTiles);
        }

        public void SpawnObject(float cellSize, Transform parent, bool spawnPrefab = true)
        {
            DespawnObject();

            if (!spawnPrefab || CollapsedTile == null || CollapsedTile.prefab == null) return;

            Vector3 position = new Vector3(GridPosition.x * cellSize, 0, GridPosition.y * cellSize);
            SpawnedObject = Object.Instantiate(CollapsedTile.prefab, position, Quaternion.identity, parent);
            ApplyEncounterRoleFromTileType();
        }

        private void ApplyEncounterRoleFromTileType()
        {
            if (CollapsedTile == null || SpawnedObject == null)
                return;

            if (CollapsedTile.tileType == TileType.Corridor)
            {
                ConfigureAsHallwayRoom();
                return;
            }

            if (CollapsedTile.tileType != TileType.Room)
                return;

            var room = SpawnedObject.GetComponentInChildren<RoomController>();
            if (room == null)
                return;

            room.SetSeedSalt(SeedSaltFromGrid());
            room.SetRoomType(RoomType.Combat);
            room.SetGridPosition(GridPosition);
            RegisterMinimapZone();
        }

        private void RegisterMinimapZone()
        {
            if (SpawnedObject == null)
                return;

            MinimapService.Instance?.RegisterZoneFromRoom(SpawnedObject, GridPosition);
        }

        public void DespawnObject()
        {
            if (SpawnedObject == null) return;

            if (Application.isPlaying)
                Object.Destroy(SpawnedObject);
            else
                Object.DestroyImmediate(SpawnedObject);

            SpawnedObject = null;
        }
        public void SetStartRoom()
        {
            var room = SpawnedObject?.GetComponentInChildren<RoomController>();
            if (room == null) return;
            room.SetSeedSalt(SeedSaltFromGrid());
            room.SetRoomType(RoomType.Start);
            room.SetGridPosition(GridPosition);
            RegisterMinimapZone();
        }

        public void SetCombatRoom()
        {
            var room = SpawnedObject?.GetComponentInChildren<RoomController>();
            if (room == null) return;
            room.SetSeedSalt(SeedSaltFromGrid());
            room.SetRoomType(RoomType.Combat);
            room.SetGridPosition(GridPosition);
            RegisterMinimapZone();
        }

        public void ConfigureAsHallwayRoom()
        {
            if (SpawnedObject == null) return;

            var room = HallwayEncounterBootstrap.EnsureEncounterZone(SpawnedObject);
            if (room == null) return;
            room.SetSeedSalt(SeedSaltFromGrid());
            room.SetRoomType(RoomType.Hallway);
            room.SetGridPosition(GridPosition);
            RegisterMinimapZone();
        }

        private int SeedSaltFromGrid() => GridPosition.x * 73856093 ^ GridPosition.y;
    }
}