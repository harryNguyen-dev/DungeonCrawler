using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WFC
{
    public static class AStar
    {
        public static List<Tile> FindPath(Tile start, Tile end, Tile[,] grid, int gridSize)
        {
            if (start == null || end == null) return null;

            Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
            Dictionary<Tile, float> gScore = new Dictionary<Tile, float>();
            List<Tile> openList = new List<Tile>();

            gScore[start] = 0;
            cameFrom[start] = null;
            openList.Add(start);

            while (openList.Count > 0)
            {
                Tile current = GetLowestFScore(openList, gScore, end);

                if (current == end)
                    return ReconstructPath(cameFrom, end);

                openList.Remove(current);

                foreach (Tile neighbor in GetNeighbors(current, grid, gridSize))
                {
                    // 1. Tuyệt đối không đi vào ô đã chốt là ROOM (trừ khi đó là điểm đích)
                    if (neighbor.IsCollapsed && neighbor.CollapsedTile?.tileType == TileType.Room && neighbor != end)
                        continue;

                    // 2. Tính toán chi phí đi qua ô này
                    float moveCost = 1.0f;

                    if (neighbor.IsCollapsed)
                    {
                        if (neighbor.CollapsedTile.tileType == TileType.Corridor)
                        {
                            // Ưu tiên đi qua corridor đã đặt (sẽ upgrade nếu cần)
                            moveCost = 0.1f;
                        }
                        else if (neighbor.CollapsedTile.tileType == TileType.Empty)
                        {
                            // Không đi qua ô Empty
                            continue;
                        }
                    }
                    else
                    {
                        // Kiểm tra xem ô này còn khả năng làm hành lang không
                        bool canBeCorridor = neighbor.PossibleTiles.Exists(t => t.tileType == TileType.Corridor);
                        if (!canBeCorridor)
                            moveCost = 10.0f; // Vẫn cho đi qua nhưng phạt nặng (tránh đi vào nếu có đường khác)
                    }

                    float newG = gScore[current] + moveCost;

                    if (!gScore.ContainsKey(neighbor) || newG < gScore[neighbor])
                    {
                        gScore[neighbor] = newG;
                        cameFrom[neighbor] = current;
                        if (!openList.Contains(neighbor))
                            openList.Add(neighbor);
                    }
                }
            }
            return null;
        }
        private static Tile GetLowestFScore(List<Tile> openList, Dictionary<Tile, float> gScore, Tile end)
        {
            Tile best = openList[0];
            float minF = float.MaxValue;

            foreach (var t in openList)
            {
                float f = gScore[t] + Heuristic(t, end);
                if (f < minF)
                {
                    minF = f;
                    best = t;
                }
            }
            return best;
        }
        private static float CalculateCost(Tile current, Tile neighbor)
        {
            // Ưu tiên cực cao cho việc tái sử dụng hành lang cũ để tạo ngã 3, ngã 4
            if (neighbor.IsCollapsed && neighbor.CollapsedTile?.tileType == TileType.Corridor)
            {
                return 0.1f; // Chi phí rất thấp
            }

            // Nếu ô trống, chi phí bình thường
            return 1.0f;
        }

        private static bool CanBeCorridor(Tile tile)
        {
            // Kiểm tra xem trong các khả năng còn lại, có miếng nào là Corridor không
            return tile.PossibleTiles.Any(t => t != null && t.tileType == TileType.Corridor);
        }

        private static List<Tile> GetNeighbors(Tile tile, Tile[,] grid, int gridSize)
        {

            List<Tile> neighbors = new List<Tile>();
            Vector2Int[] dirs =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };


            foreach (Vector2Int dir in dirs)
            {
                Vector2Int pos = tile.GridPosition + dir;
                if (pos.x < 0 || pos.x >= gridSize || pos.y < 0 || pos.y >= gridSize) continue;
                neighbors.Add(grid[pos.x, pos.y]);
            }

            return neighbors;
        }

        private static float Heuristic(Tile a, Tile b)
        {
            return Mathf.Abs(a.GridPosition.x - b.GridPosition.x)
                 + Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
        }

        private static List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile end)
        {
            List<Tile> path = new List<Tile>();
            Tile current = end;
            while (current != null)
            {
                path.Add(current);
                cameFrom.TryGetValue(current, out current);
            }
            path.Reverse();
            return path;
        }
    }
}