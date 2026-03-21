using System.Collections.Generic;
using UnityEngine;

namespace WFC 
{
    public class WFCCell
    {
        public int X {get; private set;}
        public int Y {get; private set;}


        public List<RoomTileData> PossibleTiles {get; private set;}

        public int Entropy => PossibleTiles.Count;
        public bool IsCollapsed => PossibleTiles.Count == 1;
        public bool IsContradiction => PossibleTiles.Count == 0;

        public RoomTileData CollapsedTile => IsCollapsed ? PossibleTiles[0] : null;
    
        public WFCCell(int x, int y, List<RoomTileData> allTiles) 
        {
            X = x;
            Y = y;
            PossibleTiles = new List<RoomTileData>(allTiles);
        }

        public void Collapse() 
        {
            if (IsCollapsed) return;

            RoomTileData chosen = PickWeighted();
            PossibleTiles.Clear();
            PossibleTiles.Add(chosen);
        }

        public void CollapseWith(RoomTileData tile) 
        {
            if (IsCollapsed) return;
            PossibleTiles.Clear();
            PossibleTiles.Add(tile);
        }

        public bool Contains(ConnectorDirection directionFromNeighbor, List<RoomTileData> neighborPossibleTiles) 
        {
            int beforeCount = PossibleTiles.Count;
            PossibleTiles.RemoveAll(myTile => 
            {
                foreach (var neighborTile in neighborPossibleTiles) 
                {
                    if(neighborTile.CanConnect(directionFromNeighbor, myTile)) return false;
                }
                // Debug: xem tile nào bị loại vì lý do gì
        Debug.Log($"Loại [{myTile.tileName}] vì không khớp với " +
                  $"neighbor theo hướng {directionFromNeighbor}. " +
                  $"Neighbor tiles: {string.Join(", ", neighborPossibleTiles.ConvertAll(t => t.tileName))}");
                return true;
            });
            return beforeCount != PossibleTiles.Count;
        }

        private RoomTileData PickWeighted() 
        {
            float total = 0f;
            foreach (var tile in PossibleTiles) 
            {
                total += tile.Weight;
            }

            if (total <= 0f)
                return PossibleTiles[Random.Range(0, PossibleTiles.Count)];

            float roll = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;
            foreach (var tile in PossibleTiles) 
            {
                cumulative += tile.Weight;
                if (roll <= cumulative)
                    return tile;
            }

            return PossibleTiles[PossibleTiles.Count - 1];
        }
    }
}