using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Global;
namespace WFC
{
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
        }

        private void Start()
        {
            // GenerateWithRetry(5).Forget();
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
            int attempts = 0;
            bool success = false;

            while (attempts < maxAttempts && !success)
            {
                attempts++;
                await Generate();

                if (LastStats.generation_success && LastStats.connectivity_complete)
                {
                    success = true;
                    Debug.Log($"<color=green>Dungeon generated successfully on attempt {attempts}!</color>");
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

        private void ApplyGenerationRandomSeed()
        {
            if (useFixedSeed)
                LastGenerationSeed = randomSeed;
            else
                LastGenerationSeed = new System.Random().Next();

            _rand = new System.Random(LastGenerationSeed);
            wfc.Rand = _rand;
            Debug.Log($"WFC generation seed: {LastGenerationSeed}");
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

                    Tile t = wfc.Grid[x, y];
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
            quality.CollapseUnreachableCellsToEmpty(wfc, allTiles, cellSize, spawnParent, reachableCells, ref collapsedTiles);

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
                    wfc.Propagation(nextTile);
                    continue;
                }

                wfc.CollapseTile(nextTile);
                nextTile.SpawnObject(cellSize, spawnParent);
                wfc.Propagation(nextTile);
                await UniTask.Delay(10);
            }
        }

        public async UniTask Generate()
        {
            ApplyGenerationRandomSeed();
            var totalTimer = Stopwatch.StartNew();

            _currentStats = new GenerationStats();
            _currentStats.rooms_target = roomsToPlace;
            _pathLengths.Clear();

            _currentStats.seed = LastGenerationSeed;

            FillEdgeCellsWithEmpty();

            _stepTimer.Restart();
            var placeOutcome = await roomPlacer.PlaceRoomMustHaveTiles(
                _rand, roomsToPlace, roomEdgeMargin, cellSize, spawnParent);
            placedRooms = placeOutcome.placedRooms;
            collapsedTiles += placeOutcome.collapsedDelta;
            _stepTimer.Stop();
            _currentStats.ms_place_rooms = (float)_stepTimer.Elapsed.TotalMilliseconds;
            _currentStats.rooms_placed = placedRooms.Count;

            _stepTimer.Restart();
            var (edges, corridorStats) = await corridor.ConnectRoomsByCorridor(
                placedRooms, _rand, branchingFactor, cellSize, spawnParent, _pathLengths);
            MSTEdges = edges;
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
    }
}
