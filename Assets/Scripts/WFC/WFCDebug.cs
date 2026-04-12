using UnityEngine;
namespace WFC
{

    public class WFCDebug : MonoBehaviour
    {
        [SerializeField] private WFCGeneration wfcGeneration;

        private void OnDrawGizmos()
        {
            if (wfcGeneration.Grid == null) return;

            int gridSize = wfcGeneration.GridSize;
            float cellSize = wfcGeneration.CellSize;
            
            // Khung bao toàn bộ lưới (cạnh ngoài các ô, tâm tại nửa bù trừ)
            Vector3 gridCenter = new Vector3((gridSize - 1) * cellSize * 0.5f, 0.01f, (gridSize - 1) * cellSize * 0.5f);
            Gizmos.color = new Color(1f, 0.75f, 0.1f, 1f);
            // Gizmos.DrawWireCube(gridCenter, new Vector3(gridSize * cellSize, 0.02f, gridSize * cellSize));

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Tile tile = wfcGeneration.Grid[x, y];
                    // Vị trí trung tâm của ô Tile trên không gian thế giới (nhân với cellSize)
                    Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);

                    if (tile.IsCollapsed && tile.CollapsedTile != null)
                    {
                        // DrawTileShape(pos, tile.CollapsedTile, cellSize);
                    }
                    else
                    {
                        // Vẽ khung cho các ô chưa được chốt (collapsed)
                        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);
                        Gizmos.DrawWireCube(pos, new Vector3(cellSize, 0.1f, cellSize));
                    }
                }
            }

            // DrawMSTEdges(cellSize);
            // DrawRoomLabels(cellSize);
            // Gizmos.color = Color.yellow;
            // foreach (var room in wfcGeneration.DebugRooms)
            // {
            //     Vector3 roomCenter = new Vector3(room.x + room.width / 2f - 0.5f, 0.1f, room.y + room.height / 2f - 0.5f);
            //     Vector3 roomSize = new Vector3(room.width, 0.2f, room.height);
            //     Gizmos.DrawWireCube(roomCenter, roomSize);
            // }
        }
        private void DrawMSTEdges(float cellSize)
        {
            var edges = wfcGeneration.MSTEdges;
            if (edges == null || edges.Count == 0) return;

            foreach (var (from, to) in edges)
            {
                Vector3 fromPos = new Vector3(from.GridPosition.x * cellSize, 0.1f, from.GridPosition.y * cellSize);
                Vector3 toPos = new Vector3(to.GridPosition.x * cellSize, 0.1f, to.GridPosition.y * cellSize);

                // Edge MST — màu vàng
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(fromPos, toPos);

                // Chấm tại 2 đầu để dễ thấy
                Gizmos.color = Color.red;
                float sphereRadius = cellSize * 0.1f;
                Gizmos.DrawSphere(fromPos, sphereRadius);
                Gizmos.DrawSphere(toPos, sphereRadius);
            }
        }

#if UNITY_EDITOR
        private void DrawRoomLabels(float cellSize)
        {
            var rooms = wfcGeneration.PlacedRooms;
            if (rooms == null) return;

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;

            for (int i = 0; i < rooms.Count; i++)
            {
                Vector3 pos = new Vector3(rooms[i].GridPosition.x * cellSize, 0.2f, rooms[i].GridPosition.y * cellSize);
                UnityEditor.Handles.Label(pos, $"R{i}", style);
            }
        }
#endif
        private void DrawTileShape(Vector3 center, WFCData data, float cellSize)
        {
            float s = cellSize / 3f; // Độ rộng của đường dẫn (1/3 ô)
            float roomSize = cellSize * 0.7f; // Độ lớn của phòng (chiếm 70% ô)

            // 1. Vẽ nền gạch (Màu xám tối hơn một chút để nổi bật)
            Gizmos.color = new Color(0.2f, 0.2f, 0.2f);
            Gizmos.DrawCube(center, new Vector3(cellSize * 0.95f, 0.02f, cellSize * 0.95f));

            // 2. Vẽ các đoạn đường nối (Connectors) - Luôn màu xanh lá
            Gizmos.color = Color.green;

            if (data.north == ConnectorType.Open)
            {
                Vector3 pos = center + new Vector3(0, 0, s);
                Gizmos.DrawCube(pos, new Vector3(s, 0.05f, s));
            }
            if (data.south == ConnectorType.Open)
            {
                Vector3 pos = center + new Vector3(0, 0, -s);
                Gizmos.DrawCube(pos, new Vector3(s, 0.05f, s));
            }
            if (data.east == ConnectorType.Open)
            {
                Vector3 pos = center + new Vector3(s, 0, 0);
                Gizmos.DrawCube(pos, new Vector3(s, 0.05f, s));
            }
            if (data.west == ConnectorType.Open)
            {
                Vector3 pos = center + new Vector3(-s, 0, 0);
                Gizmos.DrawCube(pos, new Vector3(s, 0.05f, s));
            }

            // 3. Vẽ phần thân chính của Tile dựa trên TileType
            if (data.tileType == TileType.Room)
            {
                // Vẽ hình vuông màu xanh dương sáng (Cyan) cho Room
                Gizmos.color = new Color(0.3f, 0.7f, 1f); // Màu giống trong ảnh của bạn

                // Bạn có thể chọn vẽ Cube (Phòng vuông) hoặc Sphere (Phòng tròn)
                Gizmos.DrawCube(center + new Vector3(0, 0.03f, 0), new Vector3(roomSize, 0.08f, roomSize));

                // Nếu muốn vẽ hình tròn như vài ô trong ảnh:
                // Gizmos.DrawSphere(center + new Vector3(0, 0.03f, 0), roomSize / 2f);
            }
            else if (data.tileType == TileType.Corridor)
            {
                // Vẽ tâm đường hầm màu xanh lá
                Gizmos.color = Color.green;
                Gizmos.DrawCube(center + new Vector3(0, 0.03f, 0), new Vector3(s, 0.08f, s));
            }
        }
    }

}