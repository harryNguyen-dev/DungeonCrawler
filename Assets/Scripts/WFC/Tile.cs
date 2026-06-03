using System.Collections.Generic;
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

        public void SpawnObject(float cellSize, Transform parent)
        {
            DespawnObject();

            if (CollapsedTile == null || CollapsedTile.prefab == null) return;

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
        }

        public void SetCombatRoom()
        {
            var room = SpawnedObject?.GetComponentInChildren<RoomController>();
            if (room == null) return;
            room.SetSeedSalt(SeedSaltFromGrid());
            room.SetRoomType(RoomType.Combat);
        }

        public void ConfigureAsHallwayRoom()
        {
            if (SpawnedObject == null) return;

            var room = HallwayEncounterBootstrap.EnsureEncounterZone(SpawnedObject);
            if (room == null) return;
            room.SetSeedSalt(SeedSaltFromGrid());
            room.SetRoomType(RoomType.Hallway);
        }

        private int SeedSaltFromGrid() => GridPosition.x * 73856093 ^ GridPosition.y;
    }
}