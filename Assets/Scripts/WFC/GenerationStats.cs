using System;

namespace WFC
{
    /// <summary>
    /// Struct chứa tất cả các metrics cho một lần generation.
    /// Dùng cho báo cáo thesis và phân tích hiệu năng.
    /// </summary>
    [Serializable]
    public struct GenerationStats
    {
        // === Rooms (Bước 1) ===
        public int rooms_placed;
        public int rooms_target;

        // === Corridors (Bước 2) ===
        public int mst_edges_total;
        public int mst_edges_success;
        public int extra_edges_total;
        public int extra_edges_success;
        public float astar_path_avg_length;
        public int astar_path_min_length;
        public int astar_path_max_length;

        // === WFC Fill (Bước 3) ===
        public int contradictions;
        public int wfc_iterations;

        // === Quality Metrics ===
        /// <summary>True nếu tất cả MST edges đều nối thành công (dungeon fully connected).</summary>
        public bool connectivity_complete;
        /// <summary>Số ô có thể tiếp cận được từ các phòng (rooms + corridors + reachable cells).</summary>
        public int reachable_cells_count;
        /// <summary>Số ô Empty sau generation.</summary>
        public int empty_cells_count;
        /// <summary>Số ô Corridor sau generation.</summary>
        public int corridor_cells_count;
        /// <summary>Số ô Room sau generation.</summary>
        public int room_cells_count;
        /// <summary>Tỷ lệ mật độ dungeon (non-empty / total).</summary>
        public float dungeon_density;
        /// <summary>Số ngõ cụt (corridor chỉ có 1 cổng mở).</summary>
        public int dead_end_count;
        /// <summary>Số ngã ba/ngã tư (corridor có >= 3 cổng mở).</summary>
        public int branch_count;
        /// <summary>True nếu generation hoàn toàn thành công (rooms đủ, MST connected, không có contradiction nghiêm trọng).</summary>
        public bool generation_success;

        // === Performance (ms) ===
        public float ms_place_rooms;
        public float ms_connect_corridors;
        public float ms_wfc_fill;
        public float ms_total;

        // === Seed ===
        public int seed;

        /// <summary>Header CSV cho báo cáo.</summary>
        public static string CsvHeader =>
            "seed," +
            "rooms_placed,rooms_target," +
            "mst_edges_total,mst_edges_success,extra_edges_total,extra_edges_success," +
            "astar_path_avg_length,astar_path_min_length,astar_path_max_length," +
            "contradictions,wfc_iterations," +
            "connectivity_complete,reachable_cells_count,empty_cells_count,corridor_cells_count,room_cells_count," +
            "dungeon_density,dead_end_count,branch_count,generation_success," +
            "ms_place_rooms,ms_connect_corridors,ms_wfc_fill,ms_total";

        /// <summary>Xuất dữ liệu dạng CSV row.</summary>
        public string ToCsvRow()
        {
            return $"{seed}," +
                   $"{rooms_placed},{rooms_target}," +
                   $"{mst_edges_total},{mst_edges_success},{extra_edges_total},{extra_edges_success}," +
                   $"{astar_path_avg_length:F2},{astar_path_min_length},{astar_path_max_length}," +
                   $"{contradictions},{wfc_iterations}," +
                   $"{(connectivity_complete ? 1 : 0)},{reachable_cells_count},{empty_cells_count},{corridor_cells_count},{room_cells_count}," +
                   $"{dungeon_density:F3},{dead_end_count},{branch_count},{(generation_success ? 1 : 0)}," +
                   $"{ms_place_rooms:F2},{ms_connect_corridors:F2},{ms_wfc_fill:F2},{ms_total:F2}";
        }
    }

    public struct TileNeighbor
    {
        public Tile tile;
        public Direction direction;
    }
}
