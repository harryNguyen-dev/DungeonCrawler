using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace WFC
{
    /// <summary>
    /// Struct chứa tất cả các metrics cho một lần generation.
    /// Dùng cho báo cáo thesis và phân tích hiệu năng.
    /// </summary>
    [System.Serializable]
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

    public class WFCGeneration : MonoBehaviour
    {
        [SerializeField] private int gridSize = 30;

        [Tooltip("Kích thước thực tế của mỗi ô trong Unity units (VD: 14 nếu prefab là 14x14).")]
        [SerializeField] private float cellSize = 14f;

        [SerializeField] private WFCData[] allTiles;

        [Tooltip("Số ô phòng cần đặt trước khi các bước WFC khác chạy.")]
        [SerializeField] private int roomsToPlace = 5;

        [Tooltip("Khoảng cách tối thiểu từ biên grid khi đặt phòng (ô). Phải > 2 nghĩa là đặt margin ≥ 3 (không đặt phòng trong 3 hàng/cột sát mép).")]
        [SerializeField] private int roomEdgeMargin = 3;

        [SerializeField] private Transform spawnParent;

        private Tile[,] grid;
        public Tile[,] Grid => grid;
        public int GridSize => gridSize;
        public float CellSize => cellSize;

        private int totalTiles;
        private int collapsedTiles;
        private List<Tile> placedRooms = new List<Tile>();
        public List<Tile> PlacedRooms => placedRooms;
        public List<(Tile from, Tile to)> MSTEdges { get; private set; } = new();

        [Range(0, 2)]
        [SerializeField] private int branchingFactor = 1;

        [Tooltip("Bật để mỗi lần Generate() dùng đúng Random Seed bên dưới (có thể tái lập dungeon).")]
        [SerializeField] private bool useFixedSeed = true;

        [Tooltip("Seed cho UnityEngine.Random khi Use Fixed Seed bật. Khi tắt, mỗi lần Generate() chọn seed ngẫu nhiên và ghi log.")]
        [SerializeField] private int randomSeed = 12345;

        /// <summary>Seed thực tế đã dùng cho lần Generate() gần nhất (sau khi InitState).</summary>
        public int LastGenerationSeed { get; private set; }

        /// <summary>Stats của lần generate gần nhất. Public để debug/UI có thể đọc.</summary>
        public GenerationStats LastStats { get; private set; }

        // Tracking variables cho stats
        private GenerationStats _currentStats;
        private Stopwatch _stepTimer = new Stopwatch();
        private List<int> _pathLengths = new List<int>();

        /// <summary>Random instance dùng cho toàn bộ generation, đảm bảo deterministic với cùng seed.</summary>
        private System.Random _rand;

        private void Start()
        {
            InitializeGrid();
            // Generate().Forget();
            GenerateWithRetry(5).Forget();
            // BatchTest();
        }
#if UNITY_EDITOR
        [ContextMenu("Run Batch Test 100x")]
        private async void BatchTest()
        {
            // useFixedSeed = false;
            var results = new System.Text.StringBuilder();
            results.AppendLine(GenerationStats.CsvHeader);

            // Chạy nháp 1 lần để JIT compile mọi thứ, tránh lần đầu chậm
            ClearSpawnedTiles();
            InitializeGrid();
            await Generate();

            for (int i = 0; i < 100; i++)
            {
                ClearSpawnedTiles();
                InitializeGrid();
                await Generate();
                results.AppendLine(LastStats.ToCsvRow());
            }

            System.IO.File.WriteAllText("Assets/generation_report.csv", results.ToString());
            Debug.Log($"Batch test xong! Đã ghi {100} kết quả vào Assets/generation_report.csv");
        }
#endif 
        public async UniTask GenerateWithRetry(int maxAttempts = 3)
        {
            int attempts = 0;
            bool success = false;

            while (attempts < maxAttempts && !success)
            {
                attempts++;
                await Generate(); // Gọi hàm Generate hiện tại của bạn

                // Kiểm tra tiêu chí thành công dựa trên stats bạn đã định nghĩa
                if (LastStats.generation_success && LastStats.connectivity_complete)
                {
                    success = true;
                    Debug.Log($"<color=green>Dungeon generated successfully on attempt {attempts}!</color>");
                }
                else
                {
                    Debug.LogWarning($"Attempt {attempts} failed. Retrying...");
                    ClearSpawnedTiles();
                    InitializeGrid();
                    // Đổi seed cho lần thử tiếp theo nếu không dùng Fixed Seed
                    if (!useFixedSeed) randomSeed = UnityEngine.Random.Range(0, 1000000);
                }
            }

            if (!success)
            {
                Debug.LogError("Failed to generate a valid dungeon after max attempts. Check your constraints/tileset.");
            }
        }

        private void ClearSpawnedTiles()
        {
            if (grid != null)
            {
                foreach (Tile tile in grid)
                {
                    tile.DespawnObject();
                }
            }

            if (spawnParent == null) return;
            for (int i = spawnParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(spawnParent.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Tính toán các quality metrics sau khi generation hoàn tất.
        /// </summary>
        private void CalculateQualityMetrics()
        {
            int emptyCount = 0;
            int corridorCount = 0;
            int roomCount = 0;
            int deadEndCount = 0;
            int branchCount = 0;
            int reachableCount = 0;

            HashSet<Vector2Int> reachableCells = FindReachableCells();
            reachableCount = reachableCells.Count;

            foreach (Tile tile in grid)
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

            _currentStats.empty_cells_count = emptyCount;
            _currentStats.corridor_cells_count = corridorCount;
            _currentStats.room_cells_count = roomCount;
            _currentStats.dead_end_count = deadEndCount;
            _currentStats.branch_count = branchCount;
            _currentStats.reachable_cells_count = reachableCount;

            int totalCells = gridSize * gridSize;
            int nonEmptyCells = totalCells - emptyCount;
            _currentStats.dungeon_density = totalCells > 0 ? (float)nonEmptyCells / totalCells : 0f;

            _currentStats.connectivity_complete = (_currentStats.mst_edges_success == _currentStats.mst_edges_total);
            _currentStats.generation_success = _currentStats.connectivity_complete
                                               && _currentStats.rooms_placed == _currentStats.rooms_target
                                               && _currentStats.contradictions <= 5;
        }
        private void ApplyGenerationRandomSeed()
        {
            if (useFixedSeed)
                LastGenerationSeed = randomSeed;
            else
                LastGenerationSeed = new System.Random().Next();

            _rand = new System.Random(LastGenerationSeed);
            Debug.Log($"WFC generation seed: {LastGenerationSeed}");
        }
        private async UniTask PlaceRoomMustHaveTiles()
        {
            int roomsPlaced = 0;
            placedRooms.Clear();
            for (int attempt = 0; roomsPlaced < roomsToPlace && attempt < roomsToPlace * 10; attempt++)
            {
                Tile tile = GetRandomTileWithRoomPossibility();
                if (tile == null) break;

                if (TryCollapseTileToRoom(tile))
                {
                    collapsedTiles++;
                    roomsPlaced++;

                    LightPropagation(tile);

                    placedRooms.Add(tile);
                    await UniTask.Yield();
                }
            }
        }

        private Tile GetRandomTileWithRoomPossibility()
        {
            List<Tile> candidates = new List<Tile>();
            foreach (Tile t in grid)
            {
                if (t.IsCollapsed) continue;
                if (!IsValidRoomPlacementCell(t)) continue;
                if (!HasRoomPossibility(t)) continue;
                candidates.Add(t);
            }
            if (candidates.Count == 0) return null;
            return candidates[_rand.Next(0, candidates.Count)];
        }

        /// <summary>Ô có nằm trong vùng cho phép đặt phòng (trừ lề biên grid) hay không.</summary>
        private bool IsValidRoomPlacementCell(Tile tile)
        {
            int m = Mathf.Max(0, roomEdgeMargin);
            if (gridSize <= 2 * m)
                return false;
            Vector2Int p = tile.GridPosition;
            int innerMax = gridSize - 1 - m;
            return p.x >= m && p.x <= innerMax && p.y >= m && p.y <= innerMax;
        }

        private static bool HasRoomPossibility(Tile tile)
        {
            foreach (WFCData p in tile.PossibleTiles)
            {
                if (p != null && p.tileType == TileType.Room)
                    return true;
            }
            return false;
        }

        private bool TryCollapseTileToRoom(Tile tile)
        {
            List<WFCData> roomOptions = new List<WFCData>();
            foreach (WFCData p in tile.PossibleTiles)
            {
                if (p != null && p.tileType == TileType.Room)
                    roomOptions.Add(p);
            }
            if (roomOptions.Count == 0) return false;

            float totalWeight = 0f;
            foreach (WFCData p in roomOptions)
                totalWeight += Mathf.Max(0f, p.weight);

            WFCData chosen;
            if (totalWeight <= 0f)
            {
                chosen = roomOptions[_rand.Next(0, roomOptions.Count)];
            }
            else
            {
                float randomWeight = (float)(_rand.NextDouble() * totalWeight);
                chosen = roomOptions[0];
                foreach (WFCData p in roomOptions)
                {
                    randomWeight -= Mathf.Max(0f, p.weight);
                    if (randomWeight <= 0f)
                    {
                        chosen = p;
                        break;
                    }
                }
            }

            tile.CollapsedTile = chosen;
            tile.IsCollapsed = true;
            tile.PossibleTiles = new List<WFCData> { chosen };
            tile.SpawnObject(cellSize, spawnParent);
            return true;
        }
        private async UniTask ConnectRoomsByCorridor()
        {
            // Bước 2a: Xây MST
            var mstOnlyEdges = Prim.BuildMST(placedRooms);
            _currentStats.mst_edges_total = mstOnlyEdges.Count;

            // Bước 2b: Thêm extra edges
            int mstCount = mstOnlyEdges.Count;
            Prim.AddExtraEdges(mstOnlyEdges, placedRooms, _rand, 0.15f);
            _currentStats.extra_edges_total = mstOnlyEdges.Count - mstCount;

            MSTEdges = mstOnlyEdges;

            // Nối từng cạnh và đếm success
            int edgeIndex = 0;
            foreach (var (from, to) in MSTEdges)
            {
                bool isMstEdge = edgeIndex < mstCount;
                bool success = await ConnectTwoRooms(from, to);

                if (isMstEdge && success)
                    _currentStats.mst_edges_success++;
                else if (!isMstEdge && success)
                    _currentStats.extra_edges_success++;

                edgeIndex++;
                await UniTask.Yield();
            }
        }
        /// <summary>
        /// Nối 2 phòng bằng A* + Fit corridor.
        /// </summary>
        /// <returns>True nếu tìm được đường và fit thành công.</returns>
        private async UniTask<bool> ConnectTwoRooms(Tile from, Tile to)
        {
            List<Direction> fromDoors = from.CollapsedTile.GetOpenConnector();
            List<Direction> toDoors = to.CollapsedTile.GetOpenConnector();

            List<Tile> bestPath = null;

            foreach (Direction dirFrom in fromDoors)
            {
                Tile startCell = GetAdjacentTile(from, dirFrom);
                if (startCell == null || (startCell.IsCollapsed && startCell.CollapsedTile?.tileType == TileType.Room))
                    continue;

                foreach (Direction dirTo in toDoors)
                {
                    Tile endCell = GetAdjacentTile(to, dirTo);
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
                _pathLengths.Add(bestPath.Count);
                await FitHallwayPath(bestPath, from, to);
                return true;
            }
            else
            {
                Debug.DrawLine(new Vector3(from.GridPosition.x, 1, from.GridPosition.y),
                               new Vector3(to.GridPosition.x, 1, to.GridPosition.y), Color.red, 10f);
                Debug.LogWarning($"Không tìm được đường nối {from.GridPosition} → {to.GridPosition}");
                return false;
            }
        }
        private async UniTask FitHallwayPath(List<Tile> path, Tile roomFrom, Tile roomTo)
        {
            for (int i = 0; i < path.Count; i++)
            {
                Tile tile = path[i];

                // Ô trước đó: nếu là ô đầu tiên của path thì prev là roomFrom, ngược lại là path[i-1]
                Tile prev = (i == 0) ? roomFrom : path[i - 1];

                // Ô tiếp theo: nếu là ô cuối cùng của path thì next là roomTo, ngược lại là path[i+1]
                Tile next = (i == path.Count - 1) ? roomTo : path[i + 1];

                FitHallwayTile(tile, prev, next);

                LightPropagation(tile);

                await UniTask.Yield();
            }
        }
        private void FitHallwayTile(Tile tile, Tile prev, Tile next)
        {
            HashSet<Direction> requiredOpen = new HashSet<Direction>();

            // 1. Xác định hướng từ đường đi A*
            requiredOpen.Add(GetDirectionTo(tile, prev));
            requiredOpen.Add(GetDirectionTo(tile, next));

            // 2. Nếu ô đã là corridor, giữ lại tất cả các cổng đang mở (upgrade mode)
            if (tile.IsCollapsed && tile.CollapsedTile != null && tile.CollapsedTile.tileType == TileType.Corridor)
            {
                foreach (Direction dir in tile.CollapsedTile.GetOpenConnector())
                {
                    requiredOpen.Add(dir);
                }
            }

            // 3. Kiểm tra 4 hướng xung quanh để khớp với các ô ĐÃ CHỐT (Collapsed)
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                Tile neighbor = GetAdjacentTile(tile, dir);
                if (neighbor == null || !neighbor.IsCollapsed) continue;

                // Nếu hàng xóm có cổng hướng về mình, ta cũng phải mở cổng hướng đó
                if (neighbor.CollapsedTile.GetConnector(GetOppositeDirection(dir)) == ConnectorType.Open)
                    requiredOpen.Add(dir);
            }

            // 4. Tìm Tile Corridor phù hợp nhất
            WFCData bestFit = null;
            float bestScore = -1000f;

            foreach (WFCData data in allTiles)
            {
                if (data.tileType != TileType.Corridor) continue;

                // Kiểm tra: corridor phải có TẤT CẢ các cổng yêu cầu mở
                bool isPossible = true;
                foreach (Direction dir in requiredOpen)
                {
                    if (data.GetConnector(dir) != ConnectorType.Open) { isPossible = false; break; }
                }
                if (!isPossible) continue;

                // Tính điểm: Ưu tiên gạch khớp vừa khít (ít cổng dư thừa nhất)
                int totalOpenInTile = data.GetOpenConnector().Count;
                int extraPorts = totalOpenInTile - requiredOpen.Count;

                // Điểm phạt nếu thừa cổng, nhưng cho phép nếu nằm trong branchingFactor
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

        // Hướng từ origin nhìn về target
        private Direction GetDirectionTo(Tile origin, Tile target)
        {
            Vector2Int delta = target.GridPosition - origin.GridPosition;

            if (delta.y > 0) return Direction.North;
            if (delta.y < 0) return Direction.South;
            if (delta.x > 0) return Direction.East;
            return Direction.West;
        }
        private Tile GetAdjacentTile(Tile tile, Direction dir)
        {
            Dictionary<Direction, Vector2Int> dirMap = new Dictionary<Direction, Vector2Int>
            {
                { Direction.North, Vector2Int.up    },
                { Direction.South, Vector2Int.down  },
                { Direction.East,  Vector2Int.right },
                { Direction.West,  Vector2Int.left  }
            };

            Vector2Int pos = tile.GridPosition + dirMap[dir];
            if (pos.x < 0 || pos.x >= gridSize || pos.y < 0 || pos.y >= gridSize) return null;
            return grid[pos.x, pos.y];
        }

        private List<Tile> FindPathOnGrid(Tile start, Tile end)
        {
            return AStar.FindPath(start, end, grid, gridSize);
        }

        public async UniTask Generate()
        {
            ApplyGenerationRandomSeed();
            var totalTimer = Stopwatch.StartNew();

            // Reset stats cho lần generate mới
            _currentStats = new GenerationStats();
            _currentStats.rooms_target = roomsToPlace;
            _pathLengths.Clear();

            _currentStats.seed = LastGenerationSeed;

            FillEdgeCellsWithEmpty();

            // Bước 1: Đặt các phòng chính
            _stepTimer.Restart();
            await PlaceRoomMustHaveTiles();
            _stepTimer.Stop();
            _currentStats.ms_place_rooms = (float)_stepTimer.Elapsed.TotalMilliseconds;
            _currentStats.rooms_placed = placedRooms.Count;

            // Bước 2: Nối các phòng bằng thuật toán Prim + A* + Fit gạch Corridor
            _stepTimer.Restart();
            await ConnectRoomsByCorridor();
            _stepTimer.Stop();
            _currentStats.ms_connect_corridors = (float)_stepTimer.Elapsed.TotalMilliseconds;

            // Tính độ dài path trung bình, min, max
            if (_pathLengths.Count > 0)
            {
                float sum = 0;
                int minLen = int.MaxValue;
                int maxLen = int.MinValue;
                foreach (int len in _pathLengths)
                {
                    sum += len;
                    if (len < minLen) minLen = len;
                    if (len > maxLen) maxLen = len;
                }
                _currentStats.astar_path_avg_length = sum / _pathLengths.Count;
                _currentStats.astar_path_min_length = minLen;
                _currentStats.astar_path_max_length = maxLen;
            }

            // Bước 3: Chạy WFC truyền thống cho tất cả các ô còn lại
            _stepTimer.Restart();
            await OriginalGenerate();
            _stepTimer.Stop();
            _currentStats.ms_wfc_fill = (float)_stepTimer.Elapsed.TotalMilliseconds;

            totalTimer.Stop();
            _currentStats.ms_total = (float)totalTimer.Elapsed.TotalMilliseconds;

            // Tính quality metrics sau khi generation hoàn tất
            CalculateQualityMetrics();

            // Lưu stats
            LastStats = _currentStats;

            Debug.Log($"Generation Completed! Seed={LastStats.seed}, " +
                      $"Rooms={LastStats.rooms_placed}/{LastStats.rooms_target}, " +
                      $"MST={LastStats.mst_edges_success}/{LastStats.mst_edges_total}, " +
                      $"Extra={LastStats.extra_edges_success}/{LastStats.extra_edges_total}, " +
                      $"Contradictions={LastStats.contradictions}, " +
                      $"WFC_Iter={LastStats.wfc_iterations}, " +
                      $"Density={LastStats.dungeon_density:P1}, " +
                      $"DeadEnds={LastStats.dead_end_count}, Branches={LastStats.branch_count}, " +
                      $"Success={LastStats.generation_success}, " +
                      $"Time={LastStats.ms_total:F1}ms (R:{LastStats.ms_place_rooms:F1} C:{LastStats.ms_connect_corridors:F1} W:{LastStats.ms_wfc_fill:F1})");
        
            // SpawnTiles();
        }
        /// <summary>
        /// Tìm tất cả các ô có thể tiếp cận được từ đường đi chính (rooms + corridors).
        /// Một ô được coi là reachable nếu nó kề với một ô đã collapsed có cổng Open hướng về phía nó.
        /// </summary>
        private HashSet<Vector2Int> FindReachableCells()
        {
            HashSet<Vector2Int> reachable = new HashSet<Vector2Int>();
            Queue<Tile> queue = new Queue<Tile>();

            // Bắt đầu từ tất cả các ô đã collapsed (rooms + corridors)
            foreach (Tile tile in grid)
            {
                if (tile.IsCollapsed && tile.CollapsedTile != null)
                {
                    reachable.Add(tile.GridPosition);
                    queue.Enqueue(tile);
                }
            }

            // Flood-fill: Mở rộng từ các ô có cổng Open
            while (queue.Count > 0)
            {
                Tile current = queue.Dequeue();

                // Kiểm tra 4 hướng
                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    // Nếu ô hiện tại có cổng Open hướng này
                    if (current.CollapsedTile.GetConnector(dir) == ConnectorType.Open)
                    {
                        Tile neighbor = GetAdjacentTile(current, dir);
                        if (neighbor == null) continue;

                        // Nếu neighbor chưa được đánh dấu reachable
                        if (!reachable.Contains(neighbor.GridPosition))
                        {
                            reachable.Add(neighbor.GridPosition);

                            // Nếu neighbor đã collapsed, tiếp tục flood-fill từ nó
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

        /// <summary>
        /// Ép tất cả các ô không thể tiếp cận thành Empty.
        /// </summary>
        private void CollapseUnreachableCellsToEmpty(HashSet<Vector2Int> reachableCells)
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
            foreach (Tile tile in grid)
            {
                if (tile.IsCollapsed) continue;

                // Nếu ô này không nằm trong danh sách reachable
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

        private async UniTask OriginalGenerate()
        {
            HashSet<Vector2Int> reachableCells = FindReachableCells();
            CollapseUnreachableCellsToEmpty(reachableCells);

            foreach (Tile tile in grid)
            {
                if (tile.IsCollapsed)
                    Propagation(tile);
            }

            while (true)
            {
                Tile nextTile = GetLowestEntropyTile();
                if (nextTile == null) { break; }

                _currentStats.wfc_iterations++;

                if (nextTile.PossibleTiles.Count == 0)
                {
                    _currentStats.contradictions++;
                    WFCData fallback = allTiles[0];
                    nextTile.CollapsedTile = fallback;
                    nextTile.IsCollapsed = true;
                    nextTile.PossibleTiles = new List<WFCData> { fallback };
                    nextTile.SpawnObject(cellSize, spawnParent);
                    Propagation(nextTile);
                    continue;
                }

                CollapseTile(nextTile);
                nextTile.SpawnObject(cellSize, spawnParent);
                Propagation(nextTile);
                await UniTask.Yield();
            }
        }
        /// <summary>Lấp một vòng ô ngoài cùng bằng gạch Empty (allTiles[0]) và lan truyền ràng buộc vào nội bộ.</summary>
        private void FillEdgeCellsWithEmpty()
        {
            if (allTiles == null || allTiles.Length == 0 || allTiles[0] == null)
            {
                Debug.LogWarning("WFCGeneration: allTiles[0] thiếu; bỏ qua lấp ô rìa.");
                return;
            }

            WFCData empty = allTiles[0];
            List<Tile> edgeTiles = new List<Tile>();

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    bool isEdge = x == 0 || x == gridSize - 1 || y == 0 || y == gridSize - 1;
                    if (!isEdge) continue;

                    Tile t = grid[x, y];
                    if (t.IsCollapsed) continue;

                    t.CollapsedTile = empty;
                    t.IsCollapsed = true;
                    t.PossibleTiles = new List<WFCData> { empty };
                    t.SpawnObject(cellSize, spawnParent);
                    collapsedTiles++;
                    edgeTiles.Add(t);
                }
            }

            if (edgeTiles.Count > 0)
                PropagationFromTiles(edgeTiles);
        }

        private void InitializeGrid()
        {
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
            totalTiles = gridSize * gridSize;
            collapsedTiles = 0;
        }
        private Tile GetLowestEntropyTile()
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
            return lowestEntropyTiles[_rand.Next(0, lowestEntropyTiles.Count)];
        }

        private void CollapseTile(Tile tile)
        {
            if (tile.PossibleTiles.Count == 0)
            {
                Debug.LogError("No possible tiles for tile at " + tile.GridPosition);
                return;
            }

            // pick one of the possible tiles at random
            float totalWeight = 0f;
            foreach (var possibleTile in tile.PossibleTiles)
            {
                totalWeight += possibleTile.weight;
            }
            float randomWeight = (float)(_rand.NextDouble() * totalWeight);
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
        private void ReCollapseTile(Tile tile)
        {
            tile.ResetTile(allTiles);
        }
        private void Propagation(Tile tile)
        {
            PropagationFromTiles(new List<Tile> { tile });
        }

        private void PropagationFromTiles(List<Tile> seeds)
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

        private void LightPropagation(Tile tile)
        {
            // Lấy 4 hàng xóm xung quanh
            List<TileNeighbor> neighbors = GetNeighbors(tile);

            foreach (var neighbor in neighbors)
            {
                // Chỉ cập nhật nếu hàng xóm CHƯA bị chốt (collapsed)
                if (!neighbor.tile.IsCollapsed)
                {
                    // Gọi hàm Constraint để lọc bớt PossibleTiles của hàng xóm
                    // dựa trên ô hiện tại, nhưng KHÔNG cho phép hàng xóm lan truyền tiếp
                    Constraint(tile, neighbor, neighbor.direction);
                }
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
                    // Kiểm tra xem Connector của ô hiện tại và ô hàng xóm có khớp nhau không
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
        private static readonly Vector2Int[] _neighborDirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        private static readonly Direction[] _neighborDirections = { Direction.North, Direction.East, Direction.South, Direction.West };

        private List<TileNeighbor> GetNeighbors(Tile tile)
        {
            List<TileNeighbor> neighbors = new List<TileNeighbor>();
            for (int i = 0; i < _neighborDirs.Length; i++)
            {
                Vector2Int neighborPosition = tile.GridPosition + _neighborDirs[i];
                if (neighborPosition.x < 0 || neighborPosition.x >= gridSize || neighborPosition.y < 0 || neighborPosition.y >= gridSize) continue;

                TileNeighbor tileNeighbor = new TileNeighbor();
                tileNeighbor.tile = grid[neighborPosition.x, neighborPosition.y];
                tileNeighbor.direction = _neighborDirections[i];
                neighbors.Add(tileNeighbor);
            }
            return neighbors;
        }

        private ConnectorType GetConnector(Tile tile, Direction direction)
        {
            return tile.CollapsedTile?.GetConnector(direction) ?? ConnectorType.None;
        }
        private Direction GetOppositeDirection(Direction direction)
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
    }

}