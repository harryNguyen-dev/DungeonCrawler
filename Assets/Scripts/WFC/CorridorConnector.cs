using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace WFC
{
    public readonly struct CorridorConnectionStats
    {
        public readonly int mst_edges_total;
        public readonly int extra_edges_total;
        public readonly int mst_edges_success;
        public readonly int extra_edges_success;

        public CorridorConnectionStats(int mstTotal, int extraTotal, int mstSuccess, int extraSuccess)
        {
            mst_edges_total = mstTotal;
            extra_edges_total = extraTotal;
            mst_edges_success = mstSuccess;
            extra_edges_success = extraSuccess;
        }
    }

    /// <summary>Bước 2: MST + A* + fit corridor giữa các phòng.</summary>
    public class CorridorConnector
    {
        private readonly WFCGrid wfc;
        private readonly WFCData[] allTiles;

        public CorridorConnector(WFCGrid wfc, WFCData[] allTiles)
        {
            this.wfc = wfc;
            this.allTiles = allTiles;
        }

        public async UniTask<(List<(Tile from, Tile to)> edges, CorridorConnectionStats corridorStats)> ConnectRoomsByCorridor(
            List<Tile> placedRooms,
            System.Random rand,
            int branchingFactor,
            float cellSize,
            Transform spawnParent,
            List<int> pathLengths)
        {
            var mstOnlyEdges = Prim.BuildMST(placedRooms);
            int mstCount = mstOnlyEdges.Count;

            Prim.AddExtraEdges(mstOnlyEdges, placedRooms, rand, 0.15f);
            int extraTotal = mstOnlyEdges.Count - mstCount;

            int mstSuccess = 0;
            int extraSuccess = 0;
            int edgeIndex = 0;
            foreach (var (from, to) in mstOnlyEdges)
            {
                bool isMstEdge = edgeIndex < mstCount;
                bool success = await ConnectTwoRooms(from, to, branchingFactor, cellSize, spawnParent, pathLengths);

                if (isMstEdge && success)
                    mstSuccess++;
                else if (!isMstEdge && success)
                    extraSuccess++;

                edgeIndex++;
                await UniTask.Yield();
            }

            var corridorStats = new CorridorConnectionStats(mstCount, extraTotal, mstSuccess, extraSuccess);
            return (mstOnlyEdges, corridorStats);
        }

        private async UniTask<bool> ConnectTwoRooms(
            Tile from,
            Tile to,
            int branchingFactor,
            float cellSize,
            Transform spawnParent,
            List<int> pathLengths)
        {
            List<Direction> fromDoors = from.CollapsedTile.GetOpenConnector();
            List<Direction> toDoors = to.CollapsedTile.GetOpenConnector();

            List<Tile> bestPath = null;

            foreach (Direction dirFrom in fromDoors)
            {
                Tile startCell = wfc.GetAdjacentTile(from, dirFrom);
                if (startCell == null || (startCell.IsCollapsed && startCell.CollapsedTile?.tileType == TileType.Room))
                    continue;

                foreach (Direction dirTo in toDoors)
                {
                    Tile endCell = wfc.GetAdjacentTile(to, dirTo);
                    if (endCell == null || (endCell.IsCollapsed && endCell.CollapsedTile?.tileType == TileType.Room))
                        continue;

                    List<Tile> path = FindPathOnGrid(startCell, endCell);
                    if (path != null)
                    {
                        if (bestPath == null || path.Count < bestPath.Count)
                            bestPath = path;
                    }
                }
            }

            if (bestPath != null)
            {
                pathLengths.Add(bestPath.Count);
                await FitHallwayPath(bestPath, from, to, branchingFactor, cellSize, spawnParent);
                return true;
            }

            Debug.DrawLine(new Vector3(from.GridPosition.x, 1, from.GridPosition.y),
                new Vector3(to.GridPosition.x, 1, to.GridPosition.y), Color.red, 10f);
            Debug.LogWarning($"Không tìm được đường nối {from.GridPosition} → {to.GridPosition}");
            return false;
        }

        private async UniTask FitHallwayPath(
            List<Tile> path,
            Tile roomFrom,
            Tile roomTo,
            int branchingFactor,
            float cellSize,
            Transform spawnParent)
        {
            for (int i = 0; i < path.Count; i++)
            {
                Tile tile = path[i];
                Tile prev = (i == 0) ? roomFrom : path[i - 1];
                Tile next = (i == path.Count - 1) ? roomTo : path[i + 1];

                FitHallwayTile(tile, prev, next, branchingFactor, cellSize, spawnParent);

                wfc.LightPropagation(tile);

                await UniTask.Yield();
            }
        }

        private void FitHallwayTile(Tile tile, Tile prev, Tile next, int branchingFactor, float cellSize, Transform spawnParent)
        {
            HashSet<Direction> requiredOpen = new HashSet<Direction>();

            requiredOpen.Add(GetDirectionTo(tile, prev));
            requiredOpen.Add(GetDirectionTo(tile, next));

            if (tile.IsCollapsed && tile.CollapsedTile != null && tile.CollapsedTile.tileType == TileType.Corridor)
            {
                foreach (Direction dir in tile.CollapsedTile.GetOpenConnector())
                {
                    requiredOpen.Add(dir);
                }
            }

            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                Tile neighbor = wfc.GetAdjacentTile(tile, dir);
                if (neighbor == null || !neighbor.IsCollapsed) continue;

                if (neighbor.CollapsedTile.GetConnector(WFCGrid.GetOppositeDirection(dir)) == ConnectorType.Open)
                    requiredOpen.Add(dir);
            }

            WFCData bestFit = null;
            float bestScore = -1000f;

            foreach (WFCData data in allTiles)
            {
                if (data.tileType != TileType.Corridor) continue;

                bool isPossible = true;
                foreach (Direction dir in requiredOpen)
                {
                    if (data.GetConnector(dir) != ConnectorType.Open) { isPossible = false; break; }
                }
                if (!isPossible) continue;

                int totalOpenInTile = data.GetOpenConnector().Count;
                int extraPorts = totalOpenInTile - requiredOpen.Count;

                float score = data.weight - (extraPorts * 2.0f);
                if (extraPorts > branchingFactor) score -= 100f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestFit = data;
                }
            }

            if (bestFit != null)
            {
                tile.CollapsedTile = bestFit;
                tile.IsCollapsed = true;
                tile.PossibleTiles = new List<WFCData> { bestFit };
                tile.SpawnObject(cellSize, spawnParent);
            }
            else
            {
                Debug.LogWarning($"Không tìm được corridor phù hợp tại {tile.GridPosition}. " +
                    $"Yêu cầu mở: {string.Join(", ", requiredOpen)}");
            }
        }

        private static Direction GetDirectionTo(Tile origin, Tile target)
        {
            Vector2Int delta = target.GridPosition - origin.GridPosition;

            if (delta.y > 0) return Direction.North;
            if (delta.y < 0) return Direction.South;
            if (delta.x > 0) return Direction.East;
            return Direction.West;
        }

        private List<Tile> FindPathOnGrid(Tile start, Tile end)
        {
            return AStar.FindPath(start, end, wfc.Grid, wfc.GridSize);
        }
    }
}
