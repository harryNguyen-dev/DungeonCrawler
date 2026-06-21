using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WFC;
namespace WFC_2D_Demo
{
    public class MainCanvas : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text generateInfoText;
        [SerializeField] private TMP_InputField seedInputField;
        [SerializeField] private Button WFC_PureBtn;
        [SerializeField] private Button WFC_CustomBtn;
        [SerializeField] private Button WFC_CustomLoopBtn;
        [SerializeField] private Button WFC_PureLoopBtn;
        [SerializeField] private Button ResetBtn;
        [SerializeField] private RectTransform gridParent;

        [Header("WFC — cùng component WFCGeneration như Battle Scene")]
        [SerializeField] private WFCGeneration wfcGeneration;
        [SerializeField] private int maxRetryAttempts = 5;
        [SerializeField] private bool spawn3DForComparison = true;

        [Header("Benchmark")]
        [SerializeField] private int benchmarkLoopCount = 10000;
        [SerializeField] private int benchmarkBaseSeed = 12345;
        [SerializeField] private int benchmarkMaxAttempts = 1;
        [SerializeField] private int benchmarkYieldEvery = 50;
        [Header("Grid Render")]
        [SerializeField] private Color emptyColor = Color.black;
        [SerializeField] private Color roomFallbackColor = new Color(0.30f, 0.70f, 1f, 1f);
        [SerializeField] private Color corridorFallbackColor = new Color(0.35f, 0.85f, 0.35f, 1f);
        [SerializeField] private Color primLineColor = new Color(1f, 0.35f, 0.2f, 0.95f);
        [SerializeField] private Color primActiveLineColor = new Color(1f, 0.92f, 0.2f, 1f);
        [SerializeField] private float primLineThickness = 4f;
        [SerializeField] private float cellPixelSize = 64f;
        [SerializeField] private bool autoSizeCellsToViewport = true;

        private WFCCustom customGenerator;
        private WFCPure pureGenerator;
        private bool isGenerating;
        private int[] benchmarkSeeds;
        private void Awake()
        {
            if (wfcGeneration == null)
                wfcGeneration = GetComponent<WFCGeneration>();

            if (wfcGeneration != null)
            {
                customGenerator = new WFCCustom(
                    wfcGeneration,
                    gridParent,
                    emptyColor,
                    roomFallbackColor,
                    corridorFallbackColor,
                    primLineColor,
                    primActiveLineColor,
                    primLineThickness,
                    cellPixelSize,
                    autoSizeCellsToViewport);

                pureGenerator = new WFCPure(
                    wfcGeneration,
                    gridParent,
                    emptyColor,
                    roomFallbackColor,
                    corridorFallbackColor,
                    primLineColor,
                    primActiveLineColor,
                    primLineThickness,
                    cellPixelSize,
                    autoSizeCellsToViewport);
            }
        }

        private void OnEnable()
        {
            if (WFC_CustomBtn != null)
                WFC_CustomBtn.onClick.AddListener(OnCustomGenerateClicked);
            if (WFC_PureBtn != null)
                WFC_PureBtn.onClick.AddListener(OnPureGenerateClicked);
            if (WFC_CustomLoopBtn != null)
                WFC_CustomLoopBtn.onClick.AddListener(OnCustomLoopGenerateClicked);
            if (WFC_PureLoopBtn != null)
                WFC_PureLoopBtn.onClick.AddListener(OnPureLoopGenerateClicked);
            if (ResetBtn != null)
                ResetBtn.onClick.AddListener(OnResetClicked);
        }

        private void OnDisable()
        {
            if (WFC_CustomBtn != null)
                WFC_CustomBtn.onClick.RemoveListener(OnCustomGenerateClicked);
            if (WFC_PureBtn != null)
                WFC_PureBtn.onClick.RemoveListener(OnPureGenerateClicked);
            if (WFC_CustomLoopBtn != null)
                WFC_CustomLoopBtn.onClick.RemoveListener(OnCustomLoopGenerateClicked);
            if (WFC_PureLoopBtn != null)
                WFC_PureLoopBtn.onClick.RemoveListener(OnPureLoopGenerateClicked);
            if (ResetBtn != null)
                ResetBtn.onClick.RemoveListener(OnResetClicked);
        }

        private void OnCustomGenerateClicked()
        {
            if (isGenerating) return;
            RunCustomGenerate().Forget();
        }

        private void OnPureGenerateClicked()
        {
            if (isGenerating) return;
            RunPureGenerate().Forget();
        }

        private void OnCustomLoopGenerateClicked()
        {
            if (isGenerating) return;
            RunBenchmarkLoop(isCustom: true).Forget();
        }

        private void OnPureLoopGenerateClicked()
        {
            if (isGenerating) return;
            RunBenchmarkLoop(isCustom: false).Forget();
        }
        private void OnResetClicked()
        {
            customGenerator?.Clear();
            pureGenerator?.Clear();
            if (generateInfoText != null)
                generateInfoText.text = string.Empty;
            if (seedInputField != null)
                seedInputField.text = string.Empty;
        }

        private async UniTask RunCustomGenerate()
        {
            if (customGenerator == null || wfcGeneration == null)
            {
                SetInfoText("Thiếu WFCGeneration — gán component giống Battle Scene.");
                return;
            }

            isGenerating = true;
            SetButtonsInteractable(false);

            try
            {
                int seed = ResolveSeed();
                if (seedInputField != null && string.IsNullOrWhiteSpace(seedInputField.text))
                    seedInputField.SetTextWithoutNotify(seed.ToString());

                WFCCustomResult result = await customGenerator.GenerateAndRender(
                    seed,
                    maxRetryAttempts,
                    spawnPrefabs: spawn3DForComparison);

                SetInfoText(FormatCustomInfo(result, spawn3DForComparison));
            }
            finally
            {
                isGenerating = false;
                SetButtonsInteractable(true);
            }
        }

        private async UniTask RunPureGenerate()
        {
            if (pureGenerator == null || wfcGeneration == null)
            {
                SetInfoText("Thiếu WFCGeneration — gán component giống Battle Scene.");
                return;
            }

            if (wfcGeneration.AllTiles == null || wfcGeneration.AllTiles.Length == 0)
            {
                SetInfoText("Thiếu allTiles — gán WFCData trong WFCGeneration.");
                return;
            }

            isGenerating = true;
            SetButtonsInteractable(false);

            try
            {
                int seed = ResolveSeed();
                if (seedInputField != null && string.IsNullOrWhiteSpace(seedInputField.text))
                    seedInputField.SetTextWithoutNotify(seed.ToString());

                WFCPureResult result = await pureGenerator.GenerateAndRender(seed, maxRetryAttempts);
                SetInfoText(FormatPureInfo(result));
            }
            finally
            {
                isGenerating = false;
                SetButtonsInteractable(true);
            }
        }

        private async UniTask RunBenchmarkLoop(bool isCustom)
        {
            string version = isCustom ? "Custom" : "Pure";

            if (wfcGeneration == null)
            {
                SetInfoText("Thiếu WFCGeneration — gán component giống Battle Scene.");
                return;
            }

            if (!isCustom && (wfcGeneration.AllTiles == null || wfcGeneration.AllTiles.Length == 0))
            {
                SetInfoText("Thiếu allTiles — gán WFCData trong WFCGeneration.");
                return;
            }

            if (isCustom && customGenerator == null)
            {
                SetInfoText("Thiếu WFCCustom generator.");
                return;
            }

            if (!isCustom && pureGenerator == null)
            {
                SetInfoText("Thiếu WFCPure generator.");
                return;
            }

            isGenerating = true;
            SetButtonsInteractable(false);

            try
            {
                int[] seeds = EnsureBenchmarkSeeds();
                string outputDir = GetBenchmarkOutputDirectory();
                Directory.CreateDirectory(outputDir);
                ExportBenchmarkSeedsCsv(seeds, outputDir);

                var csv = new StringBuilder();
                csv.AppendLine(BenchmarkCsvHeader);

                var totalTimer = System.Diagnostics.Stopwatch.StartNew();
                int successCount = 0;

                for (int i = 0; i < seeds.Length; i++)
                {
                    int seed = seeds[i];
                    GenerationStats stats;
                    int attempts;
                    bool success;

                    if (isCustom)
                    {
                        var (customStats, customAttempts) = await wfcGeneration.GenerateDemoWithRetry(
                            seed,
                            benchmarkMaxAttempts,
                            spawnPrefabs: false);
                        stats = customStats;
                        attempts = customAttempts;
                        success = customStats.generation_success && customStats.connectivity_complete;
                    }
                    else
                    {
                        WFCPureResult result = await pureGenerator.GenerateBenchmark(seed, benchmarkMaxAttempts);
                        stats = result.Stats;
                        attempts = result.Attempts;
                        success = result.Success;
                    }

                    if (success)
                        successCount++;

                    csv.AppendLine(FormatBenchmarkRow(version, attempts, success, stats));

                    if (i % benchmarkYieldEvery == 0)
                    {
                        SetInfoText(
                            $"Benchmark {version}… {i + 1}/{seeds.Length}\n" +
                            $"Success: {successCount}/{i + 1} ({(float)successCount / (i + 1):P1})");
                        await UniTask.Yield();
                    }
                }

                totalTimer.Stop();

                string resultFile = Path.Combine(outputDir, isCustom
                    ? "wfc_custom_benchmark.csv"
                    : "wfc_pure_benchmark.csv");
                File.WriteAllText(resultFile, csv.ToString(), Encoding.UTF8);

                TryWriteComparisonCsv(outputDir);

                float successRate = seeds.Length > 0 ? (float)successCount / seeds.Length : 0f;
                SetInfoText(
                    $"Benchmark {version} xong!\n" +
                    $"Loops: {seeds.Length} | Success: {successCount} ({successRate:P1})\n" +
                    $"Total: {totalTimer.Elapsed.TotalSeconds:F1}s | Avg: {totalTimer.Elapsed.TotalMilliseconds / seeds.Length:F2}ms/seed\n" +
                    $"CSV: {resultFile}");
            }
            finally
            {
                isGenerating = false;
                SetButtonsInteractable(true);
            }
        }

        private int[] EnsureBenchmarkSeeds()
        {
            if (benchmarkSeeds != null && benchmarkSeeds.Length == benchmarkLoopCount)
                return benchmarkSeeds;

            benchmarkSeeds = new int[benchmarkLoopCount];
            for (int i = 0; i < benchmarkLoopCount; i++)
                benchmarkSeeds[i] = DeriveBenchmarkSeed(benchmarkBaseSeed, i);

            return benchmarkSeeds;
        }

        /// <summary>Hash deterministic — cùng baseSeed + index → cùng seed cho Custom và Pure.</summary>
        private static int DeriveBenchmarkSeed(int baseSeed, int index)
        {
            unchecked
            {
                uint h = (uint)baseSeed;
                h ^= (uint)index * 0x9E3779B9u;
                h = (h ^ (h >> 16)) * 0x85EBCA6Bu;
                h = (h ^ (h >> 13)) * 0xC2B2AE35u;
                h ^= h >> 16;
                return (int)(h & 0x7FFFFFFF);
            }
        }

        private static string BenchmarkCsvHeader =>
            "version,attempts,success," + GenerationStats.CsvHeader;

        private static string FormatBenchmarkRow(string version, int attempts, bool success, GenerationStats stats)
        {
            return $"{version},{attempts},{(success ? 1 : 0)},{stats.ToCsvRow()}";
        }

        private static string GetBenchmarkOutputDirectory()
        {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, "WFC_Benchmark");
#else
            return Path.Combine(Application.persistentDataPath, "WFC_Benchmark");
#endif
        }

        private static void ExportBenchmarkSeedsCsv(int[] seeds, string outputDir)
        {
            var sb = new StringBuilder();
            sb.AppendLine("index,seed");
            for (int i = 0; i < seeds.Length; i++)
                sb.AppendLine($"{i},{seeds[i]}");

            File.WriteAllText(Path.Combine(outputDir, "wfc_benchmark_seeds.csv"), sb.ToString(), Encoding.UTF8);
        }

        private static void TryWriteComparisonCsv(string outputDir)
        {
            string customPath = Path.Combine(outputDir, "wfc_custom_benchmark.csv");
            string purePath = Path.Combine(outputDir, "wfc_pure_benchmark.csv");
            if (!File.Exists(customPath) || !File.Exists(purePath))
                return;

            var customBySeed = ParseBenchmarkCsvBySeed(customPath);
            var pureBySeed = ParseBenchmarkCsvBySeed(purePath);
            if (customBySeed.Count == 0 || pureBySeed.Count == 0)
                return;

            var sb = new StringBuilder();
            sb.AppendLine(ComparisonCsvHeader);

            foreach (var pair in customBySeed)
            {
                if (!pureBySeed.TryGetValue(pair.Key, out string pureRow))
                    continue;

                sb.AppendLine(BuildComparisonRow(pair.Key, pair.Value, pureRow));
            }

            File.WriteAllText(Path.Combine(outputDir, "wfc_benchmark_comparison.csv"), sb.ToString(), Encoding.UTF8);
        }

        private static string ComparisonCsvHeader =>
            "seed," +
            "custom_success,custom_attempts,custom_generation_success,custom_connectivity_complete," +
            "custom_rooms_placed,custom_rooms_target,custom_mst_edges_success,custom_mst_edges_total," +
            "custom_contradictions,custom_wfc_iterations,custom_dungeon_density,custom_dead_end_count,custom_branch_count," +
            "custom_ms_total,custom_ms_place_rooms,custom_ms_connect_corridors,custom_ms_wfc_fill," +
            "pure_success,pure_attempts,pure_generation_success,pure_connectivity_complete," +
            "pure_rooms_placed,pure_rooms_target,pure_mst_edges_success,pure_mst_edges_total," +
            "pure_contradictions,pure_wfc_iterations,pure_dungeon_density,pure_dead_end_count,pure_branch_count," +
            "pure_ms_total,pure_ms_place_rooms,pure_ms_connect_corridors,pure_ms_wfc_fill," +
            "delta_ms_total,delta_dungeon_density,delta_dead_end_count,delta_branch_count,delta_contradictions";

        private static System.Collections.Generic.Dictionary<int, string> ParseBenchmarkCsvBySeed(string path)
        {
            var map = new System.Collections.Generic.Dictionary<int, string>();
            string[] lines = File.ReadAllLines(path);
            if (lines.Length <= 1)
                return map;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] cols = line.Split(',');
                if (cols.Length < 4)
                    continue;

                if (!int.TryParse(cols[3], out int seed))
                    continue;

                map[seed] = line;
            }

            return map;
        }

        private static string BuildComparisonRow(int seed, string customLine, string pureLine)
        {
            string[] c = customLine.Split(',');
            string[] p = pureLine.Split(',');
            if (c.Length < 27 || p.Length < 27)
                return string.Empty;

            float customMsTotal = ParseFloat(c[27]);
            float pureMsTotal = ParseFloat(p[27]);
            float customDensity = ParseFloat(c[20]);
            float pureDensity = ParseFloat(p[20]);
            int customDeadEnds = ParseInt(c[21]);
            int pureDeadEnds = ParseInt(p[21]);
            int customBranches = ParseInt(c[22]);
            int pureBranches = ParseInt(p[22]);
            int customContradictions = ParseInt(c[13]);
            int pureContradictions = ParseInt(p[13]);

            return string.Join(",",
                seed,
                c[2], c[1], c[23], c[15],
                c[4], c[5], c[7], c[6],
                c[13], c[14], c[20], c[21], c[22],
                c[27], c[24], c[25], c[26],
                p[2], p[1], p[23], p[15],
                p[4], p[5], p[7], p[6],
                p[13], p[14], p[20], p[21], p[22],
                p[27], p[24], p[25], p[26],
                (pureMsTotal - customMsTotal).ToString("F2"),
                (pureDensity - customDensity).ToString("F3"),
                pureDeadEnds - customDeadEnds,
                pureBranches - customBranches,
                pureContradictions - customContradictions);
        }

        private static float ParseFloat(string value) =>
            float.TryParse(value, out float result) ? result : 0f;

        private static int ParseInt(string value) =>
            int.TryParse(value, out int result) ? result : 0;

        private int ResolveSeed()
        {
            if (seedInputField == null || string.IsNullOrWhiteSpace(seedInputField.text))
                return UnityEngine.Random.Range(0, int.MaxValue);

            string text = seedInputField.text.Trim();
            if (int.TryParse(text, out int parsed))
                return parsed;

            return UnityEngine.Random.Range(0, int.MaxValue);
        }

        private static string FormatPureInfo(WFCPureResult result)
        {
            GenerationStats s = result.Stats;
            string status = result.Success ? "OK" : "FAIL";
            return
                $"WFC Pure [{status}]\n" +
                $"Seed: {s.seed} | Retries: {result.Attempts}\n" +
                $"WFC iter: {s.wfc_iterations} | Contradictions: {s.contradictions}\n" +
                $"Rooms: {s.room_cells_count} | Corridors: {s.corridor_cells_count} | Empty: {s.empty_cells_count}\n" +
                $"Density: {s.dungeon_density:P1} | Dead ends: {s.dead_end_count} | Branches: {s.branch_count}\n" +
                $"Time: {s.ms_total:F1}ms (WFC: {s.ms_wfc_fill:F1})";
        }

        private static string FormatCustomInfo(WFCCustomResult result, bool spawn3DForComparison)
        {
            GenerationStats s = result.Stats;
            string status = result.Success ? "OK" : "FAIL";
            string mode = spawn3DForComparison ? "3D+2D" : "2D only";
            return
                $"WFC Custom [{status}]\n" +
                $"Mode: {mode}\n" +
                $"Seed: {s.seed} | Retries: {result.Attempts}\n" +
                $"Rooms: {s.rooms_placed}/{s.rooms_target}\n" +
                $"MST: {s.mst_edges_success}/{s.mst_edges_total} | Extra: {s.extra_edges_success}/{s.extra_edges_total}\n" +
                $"A* path avg: {s.astar_path_avg_length:F1} (min {s.astar_path_min_length}, max {s.astar_path_max_length})\n" +
                $"WFC iter: {s.wfc_iterations} | Contradictions: {s.contradictions}\n" +
                $"Density: {s.dungeon_density:P1} | Dead ends: {s.dead_end_count} | Branches: {s.branch_count}\n" +
                $"Time: {s.ms_total:F1}ms (R:{s.ms_place_rooms:F1} C:{s.ms_connect_corridors:F1} W:{s.ms_wfc_fill:F1})";
        }

        private void SetInfoText(string text)
        {
            if (generateInfoText != null)
                generateInfoText.text = text;
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (WFC_CustomBtn != null) WFC_CustomBtn.interactable = interactable;
            if (WFC_PureBtn != null) WFC_PureBtn.interactable = interactable;
            if (WFC_CustomLoopBtn != null) WFC_CustomLoopBtn.interactable = interactable;
            if (WFC_PureLoopBtn != null) WFC_PureLoopBtn.interactable = interactable;
            if (ResetBtn != null) ResetBtn.interactable = interactable;
        }
    }
}
