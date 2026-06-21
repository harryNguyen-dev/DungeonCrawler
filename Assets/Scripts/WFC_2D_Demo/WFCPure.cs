using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using UnityEngine;
using WFC;
using Debug = UnityEngine.Debug;

namespace WFC_2D_Demo
{
    public struct WFCPureResult
    {
        public Tile[,] Grid;
        public int GridSize;
        public GenerationStats Stats;
        public int Attempts;
        public bool Success;
    }

    /// <summary>Classic WFC only (edge fill + collapse loop) — no room placement or Prim/A*.</summary>
    public class WFCPure
    {
        private readonly int gridSize;
        private readonly int iterationDelayMs;
        private readonly WFCData[] allTiles;
        private readonly WFC2DGridRenderer renderer;

        private readonly WFCGrid wfc = new WFCGrid();
        private readonly QualityAnalyzer quality = new QualityAnalyzer();

        public WFCPure(
            WFCGeneration generation,
            RectTransform gridParent,
            Color emptyColor,
            Color roomFallbackColor,
            Color corridorFallbackColor,
            Color primLineColor,
            Color primActiveLineColor,
            float primLineThickness,
            float cellPixelSize = 64f,
            bool autoSizeCellsToViewport = true)
        {
            gridSize = generation.GridSize;
            iterationDelayMs = generation.IterationDelayMs;
            allTiles = generation.AllTiles;
            renderer = new WFC2DGridRenderer(
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

        public async UniTask<WFCPureResult> GenerateAndRender(int seed, int maxAttempts = 5)
        {
            renderer.EnsureGrid(gridSize);
            renderer.ResetAllEmpty();

            int attempts = 0;
            WFCPureResult lastResult = default;

            while (attempts < maxAttempts)
            {
                attempts++;
                int attemptSeed = attempts == 1 ? seed : Random.Range(0, int.MaxValue);
                lastResult = await GenerateOnce(attemptSeed, render: true);
                lastResult.Attempts = attempts;

                if (lastResult.Success)
                    return lastResult;

                Debug.LogWarning($"[WFCPure] Attempt {attempts} failed (seed={attemptSeed}). Retrying...");
            }

            lastResult.Attempts = attempts;
            return lastResult;
        }

        /// <summary>Headless benchmark — không render UI, không delay iteration.</summary>
        public async UniTask<WFCPureResult> GenerateBenchmark(int seed, int maxAttempts = 1)
        {
            int attempts = 0;
            WFCPureResult lastResult = default;

            while (attempts < maxAttempts)
            {
                attempts++;
                int attemptSeed = attempts == 1 ? seed : Random.Range(0, int.MaxValue);
                lastResult = await GenerateOnce(attemptSeed, render: false);
                lastResult.Attempts = attempts;

                if (lastResult.Success)
                    return lastResult;
            }

            lastResult.Attempts = attempts;
            return lastResult;
        }

        public void Clear()
        {
            renderer.ResetAllEmpty();
        }

        private async UniTask<WFCPureResult> GenerateOnce(int seed, bool render)
        {
            wfc.Initialize(gridSize, allTiles);
            var rand = new System.Random(seed);
            wfc.Rand = rand;

            var stats = new GenerationStats { seed = seed };
            var totalTimer = Stopwatch.StartNew();

            if (render)
            {
                renderer.EnsureGrid(gridSize);
                renderer.ResetAllEmpty();
            }

            FillEdgeCellsWithEmpty(render);

            var wfcTimer = Stopwatch.StartNew();
            (stats.wfc_iterations, stats.contradictions) = await RunPureCollapseLoop(render);
            wfcTimer.Stop();
            stats.ms_wfc_fill = (float)wfcTimer.Elapsed.TotalMilliseconds;

            totalTimer.Stop();
            stats.ms_total = (float)totalTimer.Elapsed.TotalMilliseconds;

            quality.CalculateQualityMetrics(wfc, ref stats);

            if (render)
                renderer.RenderFullGrid(wfc.Grid, gridSize);

            return new WFCPureResult
            {
                Grid = wfc.Grid,
                GridSize = gridSize,
                Stats = stats,
                Success = stats.generation_success,
            };
        }

        private void FillEdgeCellsWithEmpty(bool render)
        {
            if (allTiles == null || allTiles.Length == 0 || allTiles[0] == null)
            {
                Debug.LogWarning("[WFCPure] allTiles[0] thiếu; bỏ qua lấp ô rìa.");
                return;
            }

            WFCData empty = allTiles[0];
            var edgeTiles = new List<Tile>();

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
                    edgeTiles.Add(t);
                    if (render)
                        renderer.UpdateCell(t);
                }
            }

            if (edgeTiles.Count > 0)
                wfc.PropagationFromTiles(edgeTiles);
        }

        private static bool ShouldDelayAfterCollapse(Tile tile)
        {
            return tile?.CollapsedTile != null && tile.CollapsedTile.tileType != TileType.Empty;
        }

        private async UniTask<(int wfcIterations, int contradictions)> RunPureCollapseLoop(bool render)
        {
            int wfcIterations = 0;
            int contradictions = 0;
            bool useDelay = render && iterationDelayMs > 0;

            while (true)
            {
                Tile nextTile = wfc.GetLowestEntropyTile();
                if (nextTile == null) break;

                wfcIterations++;

                if (nextTile.PossibleTiles.Count == 0)
                {
                    contradictions++;
                    WFCData fallback = allTiles[0];
                    nextTile.CollapsedTile = fallback;
                    nextTile.IsCollapsed = true;
                    nextTile.PossibleTiles = new List<WFCData> { fallback };
                    if (render)
                        renderer.UpdateCell(nextTile);
                    wfc.Propagation(nextTile);
                    continue;
                }

                if (render)
                    wfc.CollapseTile(nextTile, renderer.UpdateCell);
                else
                    wfc.CollapseTile(nextTile, null);
                wfc.Propagation(nextTile);
                if (useDelay && ShouldDelayAfterCollapse(nextTile))
                    await UniTask.Delay(iterationDelayMs);
            }

            return (wfcIterations, contradictions);
        }
    }
}
