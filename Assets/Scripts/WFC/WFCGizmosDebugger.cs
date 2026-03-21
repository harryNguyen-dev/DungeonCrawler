using UnityEngine;
using System.Collections.Generic;
namespace WFC 
{
     public class WFCGizmosDebugger : MonoBehaviour
    {
        [SerializeField] private WFCDungeonGenerator generator;
        [SerializeField] private float cellSize = 2f;
        [SerializeField] private bool showConnectors = true;
        [SerializeField] private bool showTileNames = true;

        private void OnDrawGizmos()
        {
            if (generator == null || generator.Grid == null) return;

            int w = generator.Grid.Width;
            int h = generator.Grid.Height;

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                RoomTileData tile = generator.Grid.GetCollapsedTile(x, y);
                Vector3 center = new Vector3(x * cellSize, 0, y * cellSize);

                // Vẽ ô
                Gizmos.color = GetTileColor(tile);
                Gizmos.DrawCube(center, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));

                // Vẽ connector
                if (showConnectors && tile != null)
                    DrawConnectors(tile, center);

#if UNITY_EDITOR
                // Vẽ tên tile
                if (showTileNames && tile != null)
                    UnityEditor.Handles.Label(
                        center + Vector3.up * 0.5f,
                        tile.tileName,
                        new GUIStyle { fontSize = 8,
                            normal = { textColor = Color.white } });
#endif
            }
        }

        private void DrawConnectors(RoomTileData tile, Vector3 center)
        {
            float half = cellSize * 0.5f;

            DrawConnectorLine(tile, center,
                ConnectorDirection.North, center + new Vector3(0, 0.2f, -half));
            DrawConnectorLine(tile, center,
                ConnectorDirection.South, center + new Vector3(0, 0.2f,  half));
            DrawConnectorLine(tile, center,
                ConnectorDirection.East,  center + new Vector3( half, 0.2f, 0));
            DrawConnectorLine(tile, center,
                ConnectorDirection.West,  center + new Vector3(-half, 0.2f, 0));
        }

        private void DrawConnectorLine(RoomTileData tile, Vector3 from,
            ConnectorDirection dir, Vector3 to)
        {
            bool isOpen = tile.GetConnector(dir) == ConnectorType.Open;
            Gizmos.color = isOpen ? Color.green : Color.red;
            Gizmos.DrawLine(from + Vector3.up * 0.2f, to);

            // Vẽ hình tròn nhỏ tại đầu connector
            Gizmos.DrawSphere(to, cellSize * 0.08f);
        }

        private Color GetTileColor(RoomTileData tile)
        {
            if (tile == null) return Color.gray;

            return tile.tileName switch
            {
                var n when n.Contains("Wall")       => new Color(0.3f, 0.3f, 0.3f),
                var n when n.Contains("Start")      => Color.green,
                var n when n.Contains("Normal")     => new Color(0.2f, 0.5f, 1f),
                var n when n.Contains("Hallway_NS") => new Color(1f, 0.8f, 0.2f),
                var n when n.Contains("Hallway_EW") => new Color(1f, 0.6f, 0.1f),
                var n when n.Contains("Boss")       => Color.red,
                var n when n.Contains("SuperChest") => new Color(0.8f, 0.2f, 0.8f),
                var n when n.Contains("Exit")       => new Color(0f, 1f, 0.8f),
                _                                   => Color.white,
            };
        }
    }
}