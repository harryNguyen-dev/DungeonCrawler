using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace WFC
{
    /// <summary>Metrics và cleanup sau generation (reachable / empty).</summary>
    public class QualityAnalyzer
    {
        public void CalculateQualityMetrics(WFCGrid wfc, ref GenerationStats stats)
        {
            int emptyCount = 0;
            int corridorCount = 0;
            int roomCount = 0;
            int deadEndCount = 0;
            int branchCount = 0;

            HashSet<Vector2Int> reachableCells = FindReachableCells(wfc);
            int reachableCount = reachableCells.Count;

            foreach (Tile tile in wfc.Grid)
            {
                if (!tile.IsCollapsed || tile.CollapsedTile == null) continue;

                TileType type = tile.CollapsedTile.tileType;
                switch (type)
                {
                    case TileType.Empty:
                        emptyCount++;
                        break;
                    case TileType.Corridor:
                        corridorCount++;
                        int openPorts = tile.CollapsedTile.GetOpenConnector().Count;
                        if (openPorts == 1) deadEndCount++;
                        else if (openPorts >= 3) branchCount++;
                        break;
                    case TileType.Room:
                        roomCount++;
                        break;
                }
            }

            stats.empty_cells_count = emptyCount;
            stats.corridor_cells_count = corridorCount;
            stats.room_cells_count = roomCount;
            stats.dead_end_count = deadEndCount;
            stats.branch_count = branchCount;
            stats.reachable_cells_count = reachableCount;

            int totalCells = wfc.GridSize * wfc.GridSize;
            int nonEmptyCells = totalCells - emptyCount;
            stats.dungeon_density = totalCells > 0 ? (float)nonEmptyCells / totalCells : 0f;

            stats.connectivity_complete = (stats.mst_edges_success == stats.mst_edges_total);
            stats.generation_success = stats.connectivity_complete
                                       && stats.rooms_placed == stats.rooms_target
                                       && stats.contradictions <= 5;
        }

        public HashSet<Vector2Int> FindReachableCells(WFCGrid wfc)
        {
            HashSet<Vector2Int> reachable = new HashSet<Vector2Int>();
            Queue<Tile> queue = new Queue<Tile>();

            foreach (Tile tile in wfc.Grid)
            {
                if (tile.IsCollapsed && tile.CollapsedTile != null)
                {
                    reachable.Add(tile.GridPosition);
                    queue.Enqueue(tile);
                }
            }

            while (queue.Count > 0)
            {
                Tile current = queue.Dequeue();

                foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
                {
                    if (current.CollapsedTile.GetConnector(dir) == ConnectorType.Open)
                    {
                        Tile neighbor = wfc.GetAdjacentTile(current, dir);
                        if (neighbor == null) continue;

                        if (!reachable.Contains(neighbor.GridPosition))
                        {
                            reachable.Add(neighbor.GridPosition);

                            if (neighbor.IsCollapsed && neighbor.CollapsedTile != null)
                            {
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }

            return reachable;
        }

        public void CollapseUnreachableCellsToEmpty(
            WFCGrid wfc,
            WFCData[] allTiles,
            float cellSize,
            Transform spawnParent,
            HashSet<Vector2Int> reachableCells,
            ref int collapsedTiles)
        {
            if (allTiles == null || allTiles.Length == 0) return;

            WFCData emptyTile = null;
            foreach (var tile in allTiles)
            {
                if (tile.tileType == TileType.Empty)
                {
                    emptyTile = tile;
                    break;
                }
            }

            if (emptyTile == null)
            {
                Debug.LogWarning("Không tìm thấy tile Empty trong allTiles!");
                return;
            }

            int collapsedCount = 0;
            foreach (Tile tile in wfc.Grid)
            {
                if (tile.IsCollapsed) continue;

                if (!reachableCells.Contains(tile.GridPosition))
                {
                    tile.CollapsedTile = emptyTile;
                    tile.IsCollapsed = true;
                    tile.PossibleTiles = new List<WFCData> { emptyTile };
                    tile.SpawnObject(cellSize, spawnParent);
                    collapsedTiles++;
                    collapsedCount++;
                }
            }

            Debug.Log($"Đã ép {collapsedCount} ô không thể tiếp cận thành Empty.");
        }
    }
}
