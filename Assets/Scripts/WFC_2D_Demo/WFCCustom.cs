using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WFC;

namespace WFC_2D_Demo
{
    public struct WFCCustomResult
    {
        public Tile[,] Grid;
        public int GridSize;
        public GenerationStats Stats;
        public List<Tile> PlacedRooms;
        public List<(Tile from, Tile to)> MSTEdges;
        public int Attempts;
        public bool Success;
    }

    internal class WFC2DGridRenderer
    {
        private const string CellContainerName = "CellContainer";
        private const string PrimContainerName = "PrimLineContainer";

        private readonly RectTransform viewport;
        private readonly Color emptyColor;
        private readonly Color roomFallbackColor;
        private readonly Color corridorFallbackColor;
        private readonly Color primLineColor;
        private readonly Color primActiveLineColor;
        private readonly float primLineThickness;
        private readonly float defaultCellPixelSize;
        private readonly bool autoSizeCellsToViewport;

        private readonly List<Image> cellPool = new List<Image>();
        private readonly List<Image> primLinePool = new List<Image>();
        private readonly List<(Tile from, Tile to, Image image)> primLineEntries = new List<(Tile from, Tile to, Image image)>();
        private RectTransform cellContainer;
        private RectTransform primLineContainer;
        private Sprite solidSprite;
        private int currentGridSize;
        private float currentCellSize;
        private float currentOffsetX;
        private float currentOffsetY;

        public WFC2DGridRenderer(
            RectTransform viewport,
            Color emptyColor,
            Color roomFallbackColor,
            Color corridorFallbackColor,
            Color primLineColor,
            Color primActiveLineColor,
            float primLineThickness,
            float defaultCellPixelSize = 64f,
            bool autoSizeCellsToViewport = true)
        {
            this.viewport = viewport;
            this.emptyColor = emptyColor;
            this.roomFallbackColor = roomFallbackColor;
            this.corridorFallbackColor = corridorFallbackColor;
            this.primLineColor = primLineColor;
            this.primActiveLineColor = primActiveLineColor;
            this.primLineThickness = primLineThickness;
            this.defaultCellPixelSize = defaultCellPixelSize;
            this.autoSizeCellsToViewport = autoSizeCellsToViewport;
        }

        public void EnsureGrid(int gridSize)
        {
            if (viewport == null || gridSize <= 0)
                return;

            EnsureSolidSprite();
            EnsureCellContainer();
            EnsurePrimLineContainer();

            if (currentGridSize != gridSize || cellPool.Count != gridSize * gridSize)
                RebuildPool(gridSize);

            LayoutCells();
        }

        public void ResetAllEmpty()
        {
            for (int i = 0; i < cellPool.Count; i++)
                SetCellToEmpty(cellPool[i]);
            ClearPrimLines();
        }

        public void UpdateCell(Tile tile)
        {
            if (tile == null || currentGridSize <= 0)
                return;

            int x = tile.GridPosition.x;
            int y = tile.GridPosition.y;
            if (x < 0 || y < 0 || x >= currentGridSize || y >= currentGridSize)
                return;

            int index = y * currentGridSize + x;
            if (index < 0 || index >= cellPool.Count)
                return;

            ApplyTileVisual(cellPool[index], tile);
        }

        public void RenderFullGrid(Tile[,] grid, int gridSize)
        {
            if (grid == null || gridSize <= 0)
                return;

            EnsureGrid(gridSize);

            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    int index = y * gridSize + x;
                    if (index < 0 || index >= cellPool.Count)
                        continue;

                    ApplyTileVisual(cellPool[index], grid[x, y]);
                }
            }
        }

        public void DrawPrimLines(List<(Tile from, Tile to)> edges)
        {
            ClearPrimLines();
            if (edges == null || edges.Count == 0 || currentGridSize <= 0)
                return;

            EnsurePrimLineContainer();

            for (int i = 0; i < edges.Count; i++)
            {
                Tile from = edges[i].from;
                Tile to = edges[i].to;
                if (from == null || to == null)
                    continue;

                Image image = CreatePrimLineImage(i, from.GridPosition, to.GridPosition, primLineColor);
                if (image != null)
                    primLineEntries.Add((from, to, image));
            }
        }

        public void HighlightPrimEdge(Tile from, Tile to, bool isMstEdge)
        {
            if (from == null || to == null || primLineEntries.Count == 0)
                return;

            Color activeColor = isMstEdge ? primActiveLineColor : primLineColor;
            float activeThickness = isMstEdge ? primLineThickness * 1.75f : primLineThickness;

            for (int i = 0; i < primLineEntries.Count; i++)
            {
                (Tile edgeFrom, Tile edgeTo, Image image) = primLineEntries[i];
                if (image == null)
                    continue;

                bool isActive = (edgeFrom == from && edgeTo == to) || (edgeFrom == to && edgeTo == from);
                image.color = isActive ? activeColor : primLineColor;
                RectTransform rect = image.rectTransform;
                Vector2 size = rect.sizeDelta;
                rect.sizeDelta = new Vector2(size.x, isActive ? activeThickness : primLineThickness);
            }
        }

        private Image CreatePrimLineImage(int index, Vector2Int fromPos, Vector2Int toPos, Color color)
        {
            Vector2 start = GridToCanvasPoint(fromPos);
            Vector2 end = GridToCanvasPoint(toPos);
            Vector2 delta = end - start;
            float length = delta.magnitude;
            if (length <= 0.01f)
                return null;

            var lineObject = new GameObject($"PrimLine_{index:D2}", typeof(RectTransform), typeof(Image));
            var image = lineObject.GetComponent<Image>();
            image.raycastTarget = false;
            image.sprite = solidSprite;
            image.type = Image.Type.Simple;
            image.color = color;

            RectTransform rect = lineObject.GetComponent<RectTransform>();
            rect.SetParent(primLineContainer, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(length, primLineThickness);
            rect.anchoredPosition = start;
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

            primLinePool.Add(image);
            return image;
        }

        private void EnsureSolidSprite()
        {
            if (solidSprite != null)
                return;

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            solidSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        private void EnsureCellContainer()
        {
            if (cellContainer != null)
                return;

            var containerObj = new GameObject(CellContainerName, typeof(RectTransform));
            cellContainer = containerObj.GetComponent<RectTransform>();
            cellContainer.SetParent(viewport, false);
            cellContainer.anchorMin = Vector2.zero;
            cellContainer.anchorMax = Vector2.zero;
            cellContainer.pivot = Vector2.zero;
            cellContainer.anchoredPosition = Vector2.zero;
            cellContainer.localScale = Vector3.one;
        }

        private void EnsurePrimLineContainer()
        {
            if (primLineContainer != null)
                return;

            var containerObj = new GameObject(PrimContainerName, typeof(RectTransform));
            primLineContainer = containerObj.GetComponent<RectTransform>();
            primLineContainer.SetParent(viewport, false);
            primLineContainer.anchorMin = Vector2.zero;
            primLineContainer.anchorMax = Vector2.zero;
            primLineContainer.pivot = Vector2.zero;
            primLineContainer.anchoredPosition = Vector2.zero;
            primLineContainer.localScale = Vector3.one;
            primLineContainer.SetAsLastSibling();
        }

        private void RebuildPool(int gridSize)
        {
            for (int i = 0; i < cellPool.Count; i++)
            {
                if (cellPool[i] != null)
                    Object.Destroy(cellPool[i].gameObject);
            }

            cellPool.Clear();
            currentGridSize = gridSize;

            int total = gridSize * gridSize;
            for (int i = 0; i < total; i++)
            {
                var cellObject = new GameObject($"Cell_{i:D3}", typeof(RectTransform), typeof(Image));
                var image = cellObject.GetComponent<Image>();
                image.raycastTarget = false;
                image.sprite = solidSprite;
                image.preserveAspect = false;
                image.type = Image.Type.Simple;

                RectTransform rect = cellObject.GetComponent<RectTransform>();
                rect.SetParent(cellContainer, false);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);

                cellPool.Add(image);
                SetCellToEmpty(image);
            }
        }

        private void LayoutCells()
        {
            if (viewport == null || cellContainer == null || currentGridSize <= 0)
                return;

            float viewportWidth = viewport.rect.width;
            float viewportHeight = viewport.rect.height;
            if (viewportWidth <= 0f || viewportHeight <= 0f)
                return;

            float cellSize = autoSizeCellsToViewport
                ? Mathf.Min(viewportWidth, viewportHeight) / currentGridSize
                : defaultCellPixelSize;
            float gridWidth = cellSize * currentGridSize;
            float gridHeight = cellSize * currentGridSize;
            float offsetX = (viewportWidth - gridWidth) * 0.5f;
            float offsetY = (viewportHeight - gridHeight) * 0.5f;
            currentCellSize = cellSize;
            currentOffsetX = offsetX;
            currentOffsetY = offsetY;

            cellContainer.sizeDelta = new Vector2(viewportWidth, viewportHeight);
            if (primLineContainer != null)
                primLineContainer.sizeDelta = new Vector2(viewportWidth, viewportHeight);

            for (int y = 0; y < currentGridSize; y++)
            {
                for (int x = 0; x < currentGridSize; x++)
                {
                    int index = y * currentGridSize + x;
                    if (index < 0 || index >= cellPool.Count)
                        continue;

                    RectTransform rect = cellPool[index].rectTransform;
                    rect.sizeDelta = new Vector2(cellSize, cellSize);
                    rect.anchoredPosition = new Vector2(
                        offsetX + (x + 0.5f) * cellSize,
                        offsetY + (y + 0.5f) * cellSize);
                }
            }
        }

        private void ApplyTileVisual(Image image, Tile tile)
        {
            if (image == null || tile == null || !tile.IsCollapsed || tile.CollapsedTile == null)
            {
                SetCellToEmpty(image);
                return;
            }

            WFCData data = tile.CollapsedTile;
            if (data.tileType == TileType.Empty)
            {
                SetCellToEmpty(image);
                return;
            }

            image.sprite = data.minimapSprite != null ? data.minimapSprite : solidSprite;
            image.color = ResolveFallbackColor(data.tileType);
        }

        private Color ResolveFallbackColor(TileType type)
        {
            if (type == TileType.Room)
                return roomFallbackColor;
            if (type == TileType.Corridor)
                return corridorFallbackColor;
            return emptyColor;
        }

        private void SetCellToEmpty(Image image)
        {
            if (image == null)
                return;

            image.sprite = solidSprite;
            image.color = emptyColor;
        }

        private void ClearPrimLines()
        {
            for (int i = 0; i < primLinePool.Count; i++)
            {
                if (primLinePool[i] != null)
                    Object.Destroy(primLinePool[i].gameObject);
            }
            primLinePool.Clear();
            primLineEntries.Clear();
        }

        private Vector2 GridToCanvasPoint(Vector2Int gridPosition)
        {
            return new Vector2(
                currentOffsetX + (gridPosition.x + 0.5f) * currentCellSize,
                currentOffsetY + (gridPosition.y + 0.5f) * currentCellSize);
        }
    }

    public class WFCCustom
    {
        private readonly WFCGeneration generation;
        private readonly WFC2DGridRenderer renderer;

        public WFCCustom(
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
            this.generation = generation;
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

        public async UniTask<WFCCustomResult> GenerateAndRender(
            int seed,
            int maxAttempts = 5,
            bool spawnPrefabs = false)
        {
            renderer.EnsureGrid(generation.GridSize);
            renderer.ResetAllEmpty();

            var (stats, attempts) = await generation.GenerateDemoWithRetry(
                seed,
                maxAttempts,
                spawnPrefabs: spawnPrefabs,
                onTileCollapsed: renderer.UpdateCell,
                onPrimBuilt: renderer.DrawPrimLines,
                onCorridorEdgeStarting: renderer.HighlightPrimEdge);

            renderer.RenderFullGrid(generation.Grid, generation.GridSize);

            return new WFCCustomResult
            {
                Grid = generation.Grid,
                GridSize = generation.GridSize,
                Stats = stats,
                PlacedRooms = new List<Tile>(generation.PlacedRooms),
                MSTEdges = new List<(Tile from, Tile to)>(generation.MSTEdges),
                Attempts = attempts,
                Success = stats.generation_success && stats.connectivity_complete,
            };
        }

        public void Clear()
        {
            generation.ClearDemoDungeon();
            renderer.ResetAllEmpty();
        }
    }
}
