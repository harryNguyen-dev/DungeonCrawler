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
    }
}