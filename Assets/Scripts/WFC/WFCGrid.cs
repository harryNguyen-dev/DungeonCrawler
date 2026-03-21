using System;
using System.Collections.Generic;
using UnityEngine;

namespace WFC 
{
    public class WFCGrid
    {
        public int Width {get; private set;}
        public int Height {get; private set;}

        private WFCCell[,] grid;
        private List<RoomTileData> allTiles;

        public System.Action<int, int, WFCCell> OnCellCollapsed;
        public System.Action<int, int, WFCCell> OnCellConstrained;

        public WFCGrid(int width, int height, List<RoomTileData> allTiles)
        {
            Width = width;
            Height = height;
            this.allTiles = allTiles;
            Initialize();
        }

        private void Initialize()
        {
            grid = new WFCCell[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    grid[x, y] = new WFCCell(x, y, allTiles);
                }
            }
        }

        public WFCCell GetCell(int x, int y) 
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return null;
            return grid[x, y];
        }

        public bool Run() 
        {
            // bool boundaryOk = ApplyBoundaryConstraints();
            // if (!boundaryOk) return false;

            while(true) 
            {
                WFCCell cell = Observe();

                if (cell == null) return true;
                
                cell.Collapse();
                OnCellCollapsed?.Invoke(cell.X, cell.Y, cell);

                bool success = Propagate(cell);
                if (!success) return false;
            }
        }

        private WFCCell Observe()
        {
            WFCCell lowest = null;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    WFCCell cell = grid[x, y];
                    if (cell.IsCollapsed) continue;

                    if (lowest == null || cell.Entropy < lowest.Entropy)
                    {
                        lowest = cell;
                    }
                }
            }
            return lowest;
        }

        private bool Propagate(WFCCell cell)
        {
            Queue<WFCCell> queue = new Queue<WFCCell>();
            queue.Enqueue(cell);

            while (queue.Count > 0)
            {
                WFCCell current = queue.Dequeue();

                foreach (ConnectorDirection dir in Enum.GetValues(typeof(ConnectorDirection))) 
                {
                    var (dx, dy) = ConnectorUtils.ToOffset(dir);
                    WFCCell neighbor = GetCell(current.X + dx, current.Y + dy);

                    if (neighbor == null || neighbor.IsCollapsed) continue;

                    // Đúng — truyền hướng ngược lại
                    bool changed = neighbor.Contains(ConnectorUtils.Opposite(dir), current.PossibleTiles);

                    if(neighbor.IsContradiction) return false;

                    if(changed) 
                    {
                        OnCellConstrained?.Invoke(neighbor.X, neighbor.Y, neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
            return true;
        }

        public bool CollapseAt(int x, int y, RoomTileData tile) 
        {
            WFCCell cell = GetCell(x, y);
            if (cell == null) return false;

            cell.CollapseWith(tile);
            OnCellCollapsed?.Invoke(x, y, cell);

            return Propagate(cell);
        }

        public RoomTileData GetCollapsedTile(int x, int y) 
        {
            WFCCell cell = GetCell(x, y);
            return cell?.CollapsedTile;
        }
        /// <summary>
        /// Áp boundary constraint — ô ở rìa grid không được có
        /// connector Open nhìn ra ngoài.
        /// Gọi trước Run().
        /// </summary>
        private bool ApplyBoundaryConstraints()
        {
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                WFCCell cell = grid[x, y];
                bool changed = false;

                if (y == 0)
                    changed |= RemoveTilesWithOpenConnector(cell, ConnectorDirection.North);
                if (y == Height - 1)
                    changed |= RemoveTilesWithOpenConnector(cell, ConnectorDirection.South);
                if (x == 0)
                    changed |= RemoveTilesWithOpenConnector(cell, ConnectorDirection.West);
                if (x == Width - 1)
                    changed |= RemoveTilesWithOpenConnector(cell, ConnectorDirection.East);

                // Propagate ngay sau khi thu hẹp ô rìa
                if (changed)
                {
                    if (cell.IsContradiction) return false;
                    bool success = Propagate(cell);
                    if (!success) return false;
                }
            }
            return true;
        }

        private bool RemoveTilesWithOpenConnector(WFCCell cell, ConnectorDirection dir)
        {
            int before = cell.PossibleTiles.Count;
            cell.PossibleTiles.RemoveAll(tile =>
                tile.GetConnector(dir) == ConnectorType.Open);
            return cell.PossibleTiles.Count < before;
        }
        public void Reset() 
        {
            Initialize();
        }
    }
}