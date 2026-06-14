using UnityEngine;

namespace Core.Minimap
{
    public struct MinimapPlayerMapPosition
    {
        public Vector2Int GridCell;
        public Vector2 LocalUv;
        public Vector2 FractionalGrid;

        public static MinimapPlayerMapPosition Invalid =>
            new MinimapPlayerMapPosition
            {
                GridCell = new Vector2Int(-1, -1),
                LocalUv = new Vector2(0.5f, 0.5f),
                FractionalGrid = Vector2.zero
            };

        public bool IsValid => GridCell.x >= 0 && GridCell.y >= 0;
    }
}
