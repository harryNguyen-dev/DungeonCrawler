using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Core;
using Debug = UnityEngine.Debug;
using Global;
using Unity.Cinemachine;
namespace WFC
{
    public class WFCGeneration : MonoBehaviour
    {
        [Tooltip("Delay ms giữa các bước spawn (0 = gen nhanh, dùng loading UI che view).")]
        [SerializeField] private int iterationDelayMs = 0;

        private int IterationDelayMs => iterationDelayMs;
        [SerializeField] private int gridSize = 30;

        [Tooltip("Kích thước thực tế của mỗi ô trong Unity units (VD: 14 nếu prefab là 14x14).")]
        [SerializeField] private float cellSize = 14f;

        [SerializeField] private WFCData[] allTiles;

        [Tooltip("Số ô phòng cần đặt trước khi các bước WFC khác chạy.")]
        [SerializeField] private int roomsToPlace = 5;

        [Tooltip("Khoảng cách tối thiểu từ biên grid khi đặt phòng (ô). Phải > 2 nghĩa là đặt margin ≥ 3 (không đặt phòng trong 3 hàng/cột sát mép).")]
        [SerializeField] private int roomEdgeMargin = 3;

        [SerializeField] private Transform spawnParent;

        [Header("Camera Preview")]
        [SerializeField] private Transform generationCameraTarget;
        [SerializeField] private bool followGeneratedTiles = true;
        [SerializeField] private float generationCameraTargetHeight = 50f;
        [SerializeField] private float overviewCameraDistance = 50f;
        [SerializeField] private float overviewCameraVerticalAngle = 50f;

        private readonly WFCGrid wfc = new WFCGrid();
        private RoomPlacer roomPlacer;
        private CorridorConnector corridor;
        private readonly QualityAnalyzer quality = new QualityAnalyzer();

        public Tile[,] Grid => wfc.Grid;
        public int GridSize => gridSize;
        public float CellSize => cellSize;

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

        private GenerationStats _currentStats;
        private Stopwatch _stepTimer = new Stopwatch();
        private List<int> _pathLengths = new List<int>();

        /// <summary>Random instance dùng cho toàn bộ generation, đảm bảo deterministic với cùng seed.</summary>
        private System.Random _rand;

        private void Awake()
        {
            roomPlacer = new RoomPlacer(wfc);
            InitializeGrid();
            Global.GlobalEvents.OnGameStart += HandleDungeonGenerated;
        }
        private void OnDestroy()
        {
            Global.GlobalEvents.OnGameStart -= HandleDungeonGenerated;
        }

        private void HandleDungeonGenerated()
        {
            Debug.Log("[WFCGeneration] HandleDungeonGenerated");
            GenerateWithRetry(5).Forget();
        }
#if UNITY_EDITOR
        [ContextMenu("Run Batch Test 100x")]
        private async void BatchTest()
        {
            var results = new System.Text.StringBuilder();
            results.AppendLine(GenerationStats.CsvHeader);

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
            Debug.Log($"Batch test xong! Đã ghi 100 kết quả vào Assets/generation_report.csv");
        }
#endif
        public async UniTask GenerateWithRetry(int maxAttempts = 3)
        {
            GlobalEvents.RaiseDungeonGenerationStarted();
            GlobalEvents.RaiseDungeonGenerationProgress(0f);
            SetDungeonVisualsVisible(false);

            int attempts = 0;
            bool success = false;

            while (attempts < maxAttempts && !success)
            {
                attempts++;
                if (attempts > 1)
                    GlobalEvents.RaiseDungeonGenerationProgress(0.05f);

                await Generate();

                if (LastStats.generation_success && LastStats.connectivity_complete)
                {
                    success = true;
                    Debug.Log($"<color=green>Dungeon generated successfully on attempt {attempts}!</color>");
                    GlobalEvents.RaiseDungeonGenerationProgress(0.98f);
                    SetDungeonVisualsVisible(true);
                    Tile startRoomTile = GetRandomStartRoom();
                    if (startRoomTile != null)
                    {
                        // Tính toán vị trí thực tế trong không gian 3D của Unity để spawn Player hoặc Đánh dấu
                        Vector3 worldPosition = new Vector3(startRoomTile.GridPosition.x * cellSize, 0, startRoomTile.GridPosition.y * cellSize);
                        Debug.Log($"Vị trí World của Start Room: {worldPosition}");

                        // Bạn có thể lưu vị trí này lại hoặc gọi Event truyền vị trí này đi
                        GlobalVariable.PlayerSpawnPosition = worldPosition;
                        startRoomTile.SetStartRoom();
                        CenterCameraOnStartRoom(startRoomTile);
                    }

                    int actualRoomCount = CountAllRoomTiles();
                    int totalCombatRooms = Mathf.Max(0, actualRoomCount - 1);
                    DungeonEncounterTracker.Reset(totalCombatRooms);
                    Debug.Log("[WFC] Placed rooms (from RoomPlacer): " + placedRooms.Count);
                    Debug.Log("[WFC] Total room count (actual on grid): " + actualRoomCount);
                    GlobalVariable.TotalRoomCount = totalCombatRooms;
                    Debug.Log("[WFC] combat rooms (boss on last entered): " + totalCombatRooms);
                    GlobalEvents.RaiseDungeonGenerationProgress(1f);
                    GlobalEvents.RaiseDungeonGenerated(LastStats.seed);
                    GlobalVariable.CurrentSeed = LastStats.seed;
                    return;
                }
                else
                {
                    Debug.LogWarning($"Attempt {attempts} failed. Retrying...");
                    ClearSpawnedTiles();
                    InitializeGrid();
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
            if (wfc.Grid != null)
            {
                foreach (Tile tile in wfc.Grid)
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

        private void SetDungeonVisualsVisible(bool visible)
        {
            if (spawnParent == null) return;

            foreach (var renderer in spawnParent.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = visible;
            }
        }

        private void BindCameraToGenerationTarget()
        {
            if (generationCameraTarget == null)
            {
                var targetObject = new GameObject("Dungeon Generation Camera Target");
                generationCameraTarget = targetObject.transform;
            }

            generationCameraTarget.position = GetGridCenterWorldPosition();
            if (GlobalEntities.Instance != null)
            {
                GlobalEntities.Instance.BindCameraTo(generationCameraTarget);
            }
        }

        private Vector3 GetGridCenterWorldPosition()
        {
            float center = (gridSize - 1) * cellSize * 0.5f;
            return new Vector3(center, generationCameraTargetHeight, center);
        }

        private void CenterCameraOnStartRoom(Tile startRoomTile)
        {
            if (startRoomTile == null) return;

            BindCameraToGenerationTarget();

            Vector3 startRoomWorldPosition = new Vector3(
                startRoomTile.GridPosition.x * cellSize,
                generationCameraTargetHeight,
                startRoomTile.GridPosition.y * cellSize);

            generationCameraTarget.position = startRoomWorldPosition;

            if (GlobalEntities.Instance == null || GlobalEntities.Instance.CinemachineCamera == null) return;

            CinemachineCamera cinemachineCamera = GlobalEntities.Instance.CinemachineCamera;
            CinemachineOrbitalFollow orbital = cinemachineCamera.GetComponent<CinemachineOrbitalFollow>();
            if (orbital == null) return;

            orbital.Radius = Mathf.Max(overviewCameraDistance, 50f);
            orbital.VerticalAxis.Range.y = Mathf.Max(orbital.VerticalAxis.Range.y, overviewCameraVerticalAngle);
            orbital.VerticalAxis.Value = overviewCameraVerticalAngle;

            ThirdCameraController cameraController = cinemachineCamera.GetComponent<ThirdCameraController>();
            if (cameraController != null)
            {
                cameraController.SetZoom(orbital.Radius);
            }
        }

        private void MoveGenerationCameraTarget(Tile tile)
        {
            if (!followGeneratedTiles || generationCameraTarget == null || tile == null) return;
            generationCameraTarget.position = new Vector3(
                tile.GridPosition.x * cellSize,
                generationCameraTargetHeight,
                tile.GridPosition.y * cellSize);
        }

        private bool ShouldDelayAfterPrefabPlacement(Tile tile)
        {
            return tile?.CollapsedTile != null && tile.CollapsedTile.tileType != TileType.Empty;
        }

        private void ApplyGenerationRandomSeed()
        {
            if (GlobalVariable.CurrentLevel != null)
                LastGenerationSeed = GlobalVariable.CurrentLevel.wfcSeed;
            else if (useFixedSeed)
                LastGenerationSeed = randomSeed;
            else
                LastGenerationSeed = new System.Random().Next();

            _rand = new System.Random(LastGenerationSeed);
            wfc.Rand = _rand;
            Debug.Log($"WFC generation seed: {LastGenerationSeed}");
        }

        /// <summary>Lấp một vòng ô ngoài cùng bằng gạch Empty (allTiles[0]) và lan truyền ràng buộc vào nội bộ.</summary>
        private async UniTask FillEdgeCellsWithEmpty()
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

                    Tile t = wfc.Grid[x, y];
                    if (t.IsCollapsed) continue;

                    t.CollapsedTile = empty;
                    t.IsCollapsed = true;
                    t.PossibleTiles = new List<WFCData> { empty };
                    t.SpawnObject(cellSize, spawnParent);
                    MoveGenerationCameraTarget(t);
                    collapsedTiles++;
                    edgeTiles.Add(t);
                    if (ShouldDelayAfterPrefabPlacement(t))
                    {
                        await UniTask.Delay(IterationDelayMs);
                    }
                }
            }

            if (edgeTiles.Count > 0)
                wfc.PropagationFromTiles(edgeTiles);
        }

        private void InitializeGrid()
        {
            wfc.Initialize(gridSize, allTiles);
            corridor = new CorridorConnector(wfc, allTiles);
            collapsedTiles = 0;
        }

        private async UniTask OriginalGenerate()
        {
            HashSet<Vector2Int> reachableCells = quality.FindReachableCells(wfc);
            collapsedTiles += quality.CollapseUnreachableCellsToEmpty(
                wfc,
                allTiles,
                cellSize,
                spawnParent,
                reachableCells);

            foreach (Tile tile in wfc.Grid)
            {
                if (tile.IsCollapsed)
                    wfc.Propagation(tile);
            }

            while (true)
            {
                Tile nextTile = wfc.GetLowestEntropyTile();
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
                    MoveGenerationCameraTarget(nextTile);
                    wfc.Propagation(nextTile);
                    if (ShouldDelayAfterPrefabPlacement(nextTile))
                    {
                        await UniTask.Delay(IterationDelayMs);
                    }
                    continue;
                }

                wfc.CollapseTile(nextTile);
                nextTile.SpawnObject(cellSize, spawnParent);
                MoveGenerationCameraTarget(nextTile);
                wfc.Propagation(nextTile);
                if (ShouldDelayAfterPrefabPlacement(nextTile))
                {
                    await UniTask.Delay(IterationDelayMs);
                }
            }
        }

        public async UniTask Generate()
        {
            ApplyGenerationRandomSeed();
            var totalTimer = Stopwatch.StartNew();

            _currentStats = new GenerationStats();
            var targetRooms = GlobalVariable.CurrentLevel != null && GlobalVariable.CurrentLevel.roomsToPlaceOverride > 0
                ? GlobalVariable.CurrentLevel.roomsToPlaceOverride
                : roomsToPlace;
            _currentStats.rooms_target = targetRooms;
            _pathLengths.Clear();

            _currentStats.seed = LastGenerationSeed;
            GlobalEvents.RaiseDungeonGenerationProgress(0.08f);

            await FillEdgeCellsWithEmpty();
            GlobalEvents.RaiseDungeonGenerationProgress(0.18f);

            _stepTimer.Restart();
            var placeOutcome = await roomPlacer.PlaceRoomMustHaveTiles(
                _rand, targetRooms, roomEdgeMargin, cellSize, spawnParent, IterationDelayMs);
            placedRooms = placeOutcome.placedRooms;
            collapsedTiles += placeOutcome.collapsedDelta;
            _stepTimer.Stop();
            _currentStats.ms_place_rooms = (float)_stepTimer.Elapsed.TotalMilliseconds;
            _currentStats.rooms_placed = placedRooms.Count;
            GlobalEvents.RaiseDungeonGenerationProgress(0.45f);

            _stepTimer.Restart();
            var (edges, corridorStats) = await corridor.ConnectRoomsByCorridor(
                placedRooms, _rand, branchingFactor, cellSize, spawnParent, _pathLengths, IterationDelayMs);
            MSTEdges = edges;
            GlobalEvents.RaiseDungeonGenerationProgress(0.72f);
            _currentStats.mst_edges_total = corridorStats.mst_edges_total;
            _currentStats.extra_edges_total = corridorStats.extra_edges_total;
            _currentStats.mst_edges_success = corridorStats.mst_edges_success;
            _currentStats.extra_edges_success = corridorStats.extra_edges_success;
            _stepTimer.Stop();
            _currentStats.ms_connect_corridors = (float)_stepTimer.Elapsed.TotalMilliseconds;

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

            _stepTimer.Restart();
            await OriginalGenerate();
            _stepTimer.Stop();
            _currentStats.ms_wfc_fill = (float)_stepTimer.Elapsed.TotalMilliseconds;
            GlobalEvents.RaiseDungeonGenerationProgress(0.9f);

            totalTimer.Stop();
            _currentStats.ms_total = (float)totalTimer.Elapsed.TotalMilliseconds;

            quality.CalculateQualityMetrics(wfc, ref _currentStats);

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
        }

        /// <summary>
        /// Chọn phòng xa start nhất trên graph MST/corridor (BFS). Dùng cho boss room MVP.
        /// </summary>
        private static Tile FindFarthestRoomFromStart(
            Tile startRoom,
            List<Tile> rooms,
            List<(Tile from, Tile to)> edges)
        {
            if (startRoom == null || rooms == null || rooms.Count == 0)
                return null;

            var adjacency = new Dictionary<Tile, List<Tile>>();
            foreach (Tile room in rooms)
                adjacency[room] = new List<Tile>();

            if (edges != null)
            {
                foreach (var (from, to) in edges)
                {
                    if (from == null || to == null) continue;
                    if (!adjacency.ContainsKey(from) || !adjacency.ContainsKey(to)) continue;
                    adjacency[from].Add(to);
                    adjacency[to].Add(from);
                }
            }

            var distance = new Dictionary<Tile, int>();
            var queue = new Queue<Tile>();
            distance[startRoom] = 0;
            queue.Enqueue(startRoom);

            while (queue.Count > 0)
            {
                Tile current = queue.Dequeue();
                if (!adjacency.TryGetValue(current, out List<Tile> neighbors)) continue;

                foreach (Tile neighbor in neighbors)
                {
                    if (distance.ContainsKey(neighbor)) continue;
                    distance[neighbor] = distance[current] + 1;
                    queue.Enqueue(neighbor);
                }
            }

            Tile farthest = null;
            int maxDistance = -1;

            foreach (Tile room in rooms)
            {
                if (room == startRoom) continue;
                if (!distance.TryGetValue(room, out int d)) continue;

                if (d > maxDistance || (d == maxDistance && IsTieBreakBossRoom(room, farthest)))
                {
                    maxDistance = d;
                    farthest = room;
                }
            }

            if (farthest != null) return farthest;

            foreach (Tile room in rooms)
            {
                if (room != startRoom)
                    return room;
            }

            return null;
        }

        private static bool IsTieBreakBossRoom(Tile candidate, Tile current)
        {
            if (current == null) return true;
            if (candidate.GridPosition.x != current.GridPosition.x)
                return candidate.GridPosition.x < current.GridPosition.x;
            return candidate.GridPosition.y < current.GridPosition.y;
        }

        /// <summary>
        /// Lọc và lấy ra một ô phòng ngẫu nhiên có thể làm Start Room.
        /// Trả về null nếu không tìm thấy phòng nào thỏa mãn điều kiện.
        /// </summary>
        public Tile GetRandomStartRoom()
        {
            if (placedRooms == null || placedRooms.Count == 0)
            {
                Debug.LogWarning("Chưa có phòng nào được đặt hoặc danh sách phòng trống!");
                return null;
            }

            List<Tile> validStartRooms = new List<Tile>();

            // Duyệt qua tất cả các ô phòng đã được đặt trong quá trình sinh dungeon
            foreach (Tile tile in placedRooms)
            {
                // Kiểm tra xem ô đó đã sập (Collapse) chưa và có dữ liệu gạch không
                if (tile.IsCollapsed && tile.CollapsedTile != null)
                {
                    // Sử dụng hàm điều kiện có sẵn trong WFCData của bạn
                    if (tile.CollapsedTile.CanBeStartRoom())
                    {
                        validStartRooms.Add(tile);
                    }
                }
            }

            // Nếu tìm thấy các ứng viên phù hợp
            if (validStartRooms.Count > 0)
            {
                // Sử dụng instance _rand đã được khởi tạo theo Seed của bạn để đảm bảo tính deterministic
                int randomIndex = _rand.Next(0, validStartRooms.Count);
                Tile startRoom = validStartRooms[randomIndex];

                Debug.Log($"<color=cyan>Đã chọn được Start Room tại vị trí: Grid({startRoom.GridPosition.x}, {startRoom.GridPosition.y})</color>");
                return startRoom;
            }

            // Trường hợp xấu: Thuật toán chạy xong nhưng không có phòng nào chỉ có 1 cổng mở
            Debug.LogWarning("Không tìm thấy phòng nào thỏa mãn điều kiện làm Start Room (Phòng Room và có đúng 1 cổng Open).");
            return null;
        }

        /// <summary>
        /// Đếm tất cả Room tiles thực tế trên grid (bao gồm cả những phòng được tạo thêm bởi WFC collapse).
        /// </summary>
        private int CountAllRoomTiles()
        {
            int count = 0;
            foreach (Tile tile in wfc.Grid)
            {
                if (tile.IsCollapsed && tile.CollapsedTile != null && tile.CollapsedTile.tileType == TileType.Room)
                {
                    count++;
                }
            }
            return count;
        }
        /// <summary>
        /// Xóa bỏ dungeon hiện tại, giải phóng bộ nhớ và đưa toàn bộ dữ liệu grid/danh sách về trạng thái trống.
        /// </summary>
        public void ResetDungeon()
        {
            Debug.Log("[WFCGeneration] Resetting Dungeon...");

            // 1. Xóa toàn bộ GameObjects (Prefabs) đã spawn trên Scene
            ClearSpawnedTiles();

            // 2. Clear các danh sách lưu trữ dữ liệu tính toán
            if (placedRooms != null)
            {
                placedRooms.Clear();
            }
            else
            {
                placedRooms = new List<Tile>();
            }

            if (MSTEdges != null)
            {
                MSTEdges.Clear();
            }
            else
            {
                MSTEdges = new List<(Tile from, Tile to)>();
            }

            if (_pathLengths != null)
            {
                _pathLengths.Clear();
            }

            // 3. Khởi tạo lại trạng thái ban đầu cho cấu trúc Grid của WFC
            InitializeGrid();

            // 4. Khôi phục các biến đếm trạng thái toàn cục (Global Variables) nếu cần
            GlobalVariable.PlayerSpawnPosition = Vector3.zero;
            GlobalVariable.TotalRoomCount = 0;
            GlobalVariable.CurrentSeed = 0;
            DungeonEncounterTracker.Reset(0);

            // 5. Đặt lại các thông số tracking nội bộ
            collapsedTiles = 0;
            _currentStats = new GenerationStats();
            LastStats = new GenerationStats();

            Debug.Log("<color=yellow>[WFCGeneration] Dungeon has been reset successfully!</color>");
        }

        /// <summary>
        /// Hàm tiện ích: Tự động Reset dungeon cũ và tiến hành tạo một Dungeon hoàn toàn mới.
        /// </summary>
        public async UniTask ResetAndGenerate(int maxAttempts = 5)
        {
            ResetDungeon();

            // Nếu không dùng Fixed Seed, ta chủ động đổi seed mới ngay trước khi tạo
            if (!useFixedSeed)
            {
                randomSeed = UnityEngine.Random.Range(0, 1000000);
            }

            await GenerateWithRetry(maxAttempts);
        }
    }
}
