using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WFC
{
    /// <summary>Bước 1: đặt phòng lên lưới WFC.</summary>
    public class RoomPlacer
    {
        private readonly WFCGrid wfc;

        public RoomPlacer(WFCGrid wfc)
        {
            this.wfc = wfc;
        }

        public async UniTask<(List<Tile> placedRooms, int collapsedDelta)> PlaceRoomMustHaveTiles(
            System.Random rand,
            int roomsToPlace,
            int roomEdgeMargin,
            float cellSize,
            Transform spawnParent)
        {
            int roomsPlaced = 0;
            int collapsedDelta = 0;
            List<Tile> placedRooms = new List<Tile>();
            for (int attempt = 0; roomsPlaced < roomsToPlace && attempt < roomsToPlace * 10; attempt++)
            {
                Tile tile = GetRandomTileWithRoomPossibility(rand, roomEdgeMargin);
                if (tile == null) break;

                if (TryCollapseTileToRoom(tile, rand, cellSize, spawnParent))
                {
                    collapsedDelta++;
                    roomsPlaced++;

                    wfc.LightPropagation(tile);

                    placedRooms.Add(tile);
                    await UniTask.Delay(20);
                }
            }

            return (placedRooms, collapsedDelta);
        }

        private Tile GetRandomTileWithRoomPossibility(System.Random rand, int roomEdgeMargin)
        {
            List<Tile> candidates = new List<Tile>();
            foreach (Tile t in wfc.Grid)
            {
                if (t.IsCollapsed) continue;
                if (!IsValidRoomPlacementCell(t, roomEdgeMargin)) continue;
                if (!HasRoomPossibility(t)) continue;
                candidates.Add(t);
            }
            if (candidates.Count == 0) return null;
            return candidates[rand.Next(0, candidates.Count)];
        }

        private bool IsValidRoomPlacementCell(Tile tile, int roomEdgeMargin)
        {
            int m = Mathf.Max(0, roomEdgeMargin);
            int size = wfc.GridSize;
            if (size <= 2 * m)
                return false;
            Vector2Int p = tile.GridPosition;
            int innerMax = size - 1 - m;
            return p.x >= m && p.x <= innerMax && p.y >= m && p.y <= innerMax;
        }

        private static bool HasRoomPossibility(Tile tile)
        {
            foreach (WFCData p in tile.PossibleTiles)
            {
                if (p != null && p.tileType == TileType.Room)
                    return true;
            }
            return false;
        }

        private bool TryCollapseTileToRoom(Tile tile, System.Random rand, float cellSize, Transform spawnParent)
        {
            List<WFCData> roomOptions = new List<WFCData>();
            foreach (WFCData p in tile.PossibleTiles)
            {
                if (p != null && p.tileType == TileType.Room)
                    roomOptions.Add(p);
            }
            if (roomOptions.Count == 0) return false;

            float totalWeight = 0f;
            foreach (WFCData p in roomOptions)
                totalWeight += Mathf.Max(0f, p.weight);

            WFCData chosen;
            if (totalWeight <= 0f)
            {
                chosen = roomOptions[rand.Next(0, roomOptions.Count)];
            }
            else
            {
                float randomWeight = (float)(rand.NextDouble() * totalWeight);
                chosen = roomOptions[0];
                foreach (WFCData p in roomOptions)
                {
                    randomWeight -= Mathf.Max(0f, p.weight);
                    if (randomWeight <= 0f)
                    {
                        chosen = p;
                        break;
                    }
                }
            }

            tile.CollapsedTile = chosen;
            tile.IsCollapsed = true;
            tile.PossibleTiles = new List<WFCData> { chosen };
            tile.SpawnObject(cellSize, spawnParent);
            return true;
        }
    }
}
