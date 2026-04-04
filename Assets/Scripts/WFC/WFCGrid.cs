using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    /// <summary>
    /// Thuật toán WFC thuần: lưới, collapse, lan truyền ràng buộc. Không biết phòng/hành lang.
    /// </summary>
    public class WFCGrid
    {
        /// <summary>Thứ tự khớp enum Direction: North, East, South, West.</summary>
        private static readonly Vector2Int[] DirectionDeltas =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        private static readonly Direction[] NeighborDirections =
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };

        private Tile[,] grid;
        private int gridSize;
        private WFCData[] allTiles;

        public Tile[,] Grid => grid;
        public int GridSize => gridSize;
        public System.Random Rand { get; set; }

        public void Initialize(int size, WFCData[] tiles)
        {
            gridSize = size;
            allTiles = tiles;
            grid = new Tile[gridSize, gridSize];

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    grid[x, y] = new Tile();
                    grid[x, y].GridPosition = new Vector2Int(x, y);
                    grid[x, y].PossibleTiles = new List<WFCData>(allTiles);
                }
            }
        }

        public Tile GetAdjacentTile(Tile tile, Direction dir)
        {
            Vector2Int pos = tile.GridPosition + DirectionDeltas[(int)dir];
            if (pos.x < 0 || pos.x >= gridSize || pos.y < 0 || pos.y >= gridSize) return null;
            return grid[pos.x, pos.y];
        }

        public Tile GetLowestEntropyTile()
        {
            int lowestEntropy = int.MaxValue;
            List<Tile> lowestEntropyTiles = new List<Tile>();

            foreach (var tile in grid)
            {
                if (tile.IsCollapsed) continue;

                if (tile.Entropy < lowestEntropy)
                {
                    lowestEntropy = tile.Entropy;
                    lowestEntropyTiles.Clear();
                    lowestEntropyTiles.Add(tile);
                }
                else if (tile.Entropy == lowestEntropy)
                {
                    lowestEntropyTiles.Add(tile);
                }
            }

            if (lowestEntropyTiles.Count == 0) return null;
            return lowestEntropyTiles[Rand.Next(0, lowestEntropyTiles.Count)];
        }

        public void CollapseTile(Tile tile)
        {
            if (tile.PossibleTiles.Count == 0)
            {
                Debug.LogError("No possible tiles for tile at " + tile.GridPosition);
                return;
            }

            float totalWeight = 0f;
            foreach (var possibleTile in tile.PossibleTiles)
            {
                totalWeight += possibleTile.weight;
            }
            float randomWeight = (float)(Rand.NextDouble() * totalWeight);
            foreach (var possibleTile in tile.PossibleTiles)
            {
                randomWeight -= possibleTile.weight;
                if (randomWeight <= 0f)
                {
                    tile.CollapsedTile = possibleTile;
                    tile.IsCollapsed = true;
                    tile.PossibleTiles = new List<WFCData> { possibleTile };
                    break;
                }
            }
        }

        public void ReCollapseTile(Tile tile)
        {
            tile.ResetTile(allTiles);
        }

        public void Propagation(Tile tile)
        {
            PropagationFromTiles(new List<Tile> { tile });
        }

        public void PropagationFromTiles(List<Tile> seeds)
        {
            if (seeds == null || seeds.Count == 0) return;

            Queue<Tile> queue = new Queue<Tile>();
            foreach (Tile s in seeds)
                queue.Enqueue(s);

            while (queue.Count > 0)
            {
                Tile currentTile = queue.Dequeue();
                List<TileNeighbor> neighbors = GetNeighbors(currentTile);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor.tile.IsCollapsed) continue;

                    if (Constraint(currentTile, neighbor, neighbor.direction))
                    {
                        if (!queue.Contains(neighbor.tile))
                            queue.Enqueue(neighbor.tile);
                    }
                }
            }
        }

        public void LightPropagation(Tile tile)
        {
            List<TileNeighbor> neighbors = GetNeighbors(tile);

            foreach (var neighbor in neighbors)
            {
                if (!neighbor.tile.IsCollapsed)
                    Constraint(tile, neighbor, neighbor.direction);
            }
        }

        private bool Constraint(Tile currentTile, TileNeighbor neighbor, Direction directionToNeighbor)
        {
            bool isChanged = false;
            List<WFCData> newPossibleTiles = new List<WFCData>();

            Direction oppositeDirection = GetOppositeDirection(directionToNeighbor);

            foreach (var neighborPossible in neighbor.tile.PossibleTiles)
            {
                bool canFit = false;
                foreach (var currentPossible in currentTile.PossibleTiles)
                {
                    if (currentPossible.GetConnector(directionToNeighbor) == neighborPossible.GetConnector(oppositeDirection))
                    {
                        canFit = true;
                        break;
                    }
                }

                if (canFit)
                {
                    newPossibleTiles.Add(neighborPossible);
                }
                else
                {
                    isChanged = true;
                }
            }

            neighbor.tile.PossibleTiles = newPossibleTiles;
            return isChanged;
        }

        public List<TileNeighbor> GetNeighbors(Tile tile)
        {
            List<TileNeighbor> neighbors = new List<TileNeighbor>();
            for (int i = 0; i < DirectionDeltas.Length; i++)
            {
                Vector2Int neighborPosition = tile.GridPosition + DirectionDeltas[i];
                if (neighborPosition.x < 0 || neighborPosition.x >= gridSize || neighborPosition.y < 0 || neighborPosition.y >= gridSize) continue;

                TileNeighbor tileNeighbor = new TileNeighbor();
                tileNeighbor.tile = grid[neighborPosition.x, neighborPosition.y];
                tileNeighbor.direction = NeighborDirections[i];
                neighbors.Add(tileNeighbor);
            }
            return neighbors;
        }

        public static Direction GetOppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Direction.South;
                case Direction.East:
                    return Direction.West;
                case Direction.South:
                    return Direction.North;
                case Direction.West:
                    return Direction.East;
                default:
                    return Direction.West;
            }
        }

        public ConnectorType GetConnector(Tile tile, Direction direction)
        {
            return tile.CollapsedTile?.GetConnector(direction) ?? ConnectorType.None;
        }
    }
}
