using Core.Minimap;
using Global;
using UnityEngine;
using UnityEngine.UI;
using WFC;

namespace CustomUI.Minimap
{
    public class MinimapView : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform cellContainer;
        [SerializeField] private RectTransform playerDot;
        [SerializeField] private MinimapCellView cellPrefab;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Zoom")]
        [SerializeField] private int visibleCellRadius = 2;
        [SerializeField] private float cellPixelSize = 64f;
        [SerializeField] private bool autoSizeCellsToViewport = true;

        private float _effectiveCellSize;

        [Header("Colors")]
        [SerializeField] private Color hiddenColor = Color.black;
        [SerializeField] private Color visitedColor = Color.white;

        [Header("Update")]
        [SerializeField] private float playerUpdateInterval = 0.05f;

        private MinimapCellView[] _cellPool;
        private int _poolViewCells;
        private Vector2Int _viewMin;
        private int _viewCellCount;
        private float _playerUpdateTimer;
        private Transform _playerTransform;
        private bool _isBound;

        private void Awake()
        {
            if (viewport == null)
                viewport = transform as RectTransform;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            EnsureCellPool();
            EnsureLayout();
        }

        private void EnsureLayout()
        {
            if (viewport == null)
                return;

            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.pivot = Vector2.zero;

            if (cellContainer == null)
                return;

            cellContainer.anchorMin = Vector2.zero;
            cellContainer.anchorMax = Vector2.one;
            cellContainer.offsetMin = Vector2.zero;
            cellContainer.offsetMax = Vector2.zero;
            cellContainer.pivot = Vector2.zero;
        }

        private float GetEffectiveCellSize()
        {
            if (!autoSizeCellsToViewport || viewport == null || _viewCellCount <= 0)
                return cellPixelSize;

            float viewWidth = viewport.rect.width;
            float viewHeight = viewport.rect.height;

            if (viewWidth <= 0f || viewHeight <= 0f)
            {
                var root = transform as RectTransform;
                if (root != null)
                {
                    viewWidth = root.rect.width;
                    viewHeight = root.rect.height;
                }
            }

            if (viewWidth <= 0f || viewHeight <= 0f)
                return cellPixelSize;

            return Mathf.Min(viewWidth, viewHeight) / _viewCellCount;
        }

        private void OnEnable()
        {
            BindService();
            GlobalEvents.OnPlayerJoin += HandlePlayerJoin;
            GlobalEvents.OnDungeonGenerationStarted += HandleGenerationStarted;
            GlobalEvents.OnDungeonGenerated += HandleDungeonGenerated;
            GlobalEvents.OnMatchReset += HandleMatchReset;

            if (MinimapService.Instance != null && MinimapService.Instance.IsReady)
                RefreshFullView();
            else
                SetVisible(false);
        }

        private void OnDisable()
        {
            UnbindService();
            GlobalEvents.OnPlayerJoin -= HandlePlayerJoin;
            GlobalEvents.OnDungeonGenerationStarted -= HandleGenerationStarted;
            GlobalEvents.OnDungeonGenerated -= HandleDungeonGenerated;
            GlobalEvents.OnMatchReset -= HandleMatchReset;
        }

        private void Update()
        {
            if (!MinimapService.Instance || !MinimapService.Instance.IsReady)
                return;

            if (_playerTransform == null)
                HandlePlayerJoin();

            if (_playerTransform == null)
                return;

            _playerUpdateTimer += Time.unscaledDeltaTime;
            if (_playerUpdateTimer < playerUpdateInterval)
                return;

            _playerUpdateTimer = 0f;
            MinimapService.Instance.UpdatePlayerWorldPosition(_playerTransform.position);
        }

        private void BindService()
        {
            if (_isBound || MinimapService.Instance == null)
                return;

            MinimapService.Instance.OnCellStateChanged += HandleCellStateChanged;
            MinimapService.Instance.OnPlayerMapPositionChanged += HandlePlayerMapPositionChanged;
            _isBound = true;
        }

        private void UnbindService()
        {
            if (!_isBound || MinimapService.Instance == null)
                return;

            MinimapService.Instance.OnCellStateChanged -= HandleCellStateChanged;
            MinimapService.Instance.OnPlayerMapPositionChanged -= HandlePlayerMapPositionChanged;
            _isBound = false;
        }

        private void HandlePlayerJoin()
        {
            var entities = GlobalEntities.Instance;
            if (entities != null && entities.PlayerInstance != null)
                _playerTransform = entities.PlayerInstance.transform;
        }

        private void HandleGenerationStarted()
        {
            SetVisible(false);
        }

        private void HandleDungeonGenerated(int seed)
        {
            BindService();
            HandlePlayerJoin();
            if (_playerTransform != null)
                MinimapService.Instance?.UpdatePlayerWorldPosition(_playerTransform.position);
            RefreshFullView();
            SetVisible(true);
        }

        private void HandleMatchReset()
        {
            SetVisible(false);
        }

        private void HandleCellStateChanged(Vector2Int gridPos, MinimapCellState state)
        {
            if (!IsInsideCurrentView(gridPos))
                return;

            int viewX = gridPos.x - _viewMin.x;
            int viewY = gridPos.y - _viewMin.y;
            int index = viewY * _viewCellCount + viewX;
            if (index < 0 || index >= _cellPool.Length)
                return;

            ApplyCellState(_cellPool[index], gridPos, state);
        }

        private void HandlePlayerMapPositionChanged(MinimapPlayerMapPosition mapPos)
        {
            if (!mapPos.IsValid)
                return;

            SetVisible(true);
            UpdateViewport(mapPos);
            UpdatePlayerDot(mapPos);
        }

        private void RefreshFullView()
        {
            BindService();
            SetVisible(true);

            if (MinimapService.Instance == null)
                return;

            MinimapPlayerMapPosition mapPos = MinimapService.Instance.PlayerMapPosition;
            if (!mapPos.IsValid && _playerTransform != null)
            {
                MinimapService.Instance.UpdatePlayerWorldPosition(_playerTransform.position);
                mapPos = MinimapService.Instance.PlayerMapPosition;
            }

            if (mapPos.IsValid)
            {
                UpdateViewport(mapPos);
                UpdatePlayerDot(mapPos);
            }
        }

        private void UpdateViewport(MinimapPlayerMapPosition mapPos)
        {
            EnsureCellPool();
            EnsureLayout();

            MinimapService service = MinimapService.Instance;
            if (service == null || !service.IsReady)
                return;

            _viewCellCount = visibleCellRadius * 2 + 1;
            if (_viewCellCount >= service.GridSize)
            {
                _viewMin = Vector2Int.zero;
                _viewCellCount = service.GridSize;
            }
            else
            {
                int half = visibleCellRadius;
                int minX = mapPos.GridCell.x - half;
                int minY = mapPos.GridCell.y - half;
                minX = Mathf.Clamp(minX, 0, service.GridSize - _viewCellCount);
                minY = Mathf.Clamp(minY, 0, service.GridSize - _viewCellCount);
                _viewMin = new Vector2Int(minX, minY);
            }

            _effectiveCellSize = GetEffectiveCellSize();

            for (int y = 0; y < _viewCellCount; y++)
            {
                for (int x = 0; x < _viewCellCount; x++)
                {
                    int index = y * _viewCellCount + x;
                    Vector2Int gridPos = new(_viewMin.x + x, _viewMin.y + y);
                    MinimapCellView cellView = _cellPool[index];
                    cellView.gameObject.SetActive(true);
                    cellView.SetGridPosition(x, y, _effectiveCellSize);
                    ApplyCellState(cellView, gridPos, service.GetCellState(gridPos));
                }
            }

            for (int i = _viewCellCount * _viewCellCount; i < _cellPool.Length; i++)
                _cellPool[i].gameObject.SetActive(false);
        }

        private void UpdatePlayerDot(MinimapPlayerMapPosition mapPos)
        {
            if (playerDot == null || viewport == null)
                return;

            if (_effectiveCellSize <= 0f)
                _effectiveCellSize = GetEffectiveCellSize();

            Vector2 localPos = new Vector2(
                (mapPos.FractionalGrid.x - _viewMin.x) * _effectiveCellSize,
                (mapPos.FractionalGrid.y - _viewMin.y) * _effectiveCellSize);

            playerDot.SetParent(viewport, false);
            playerDot.anchorMin = Vector2.zero;
            playerDot.anchorMax = Vector2.zero;
            playerDot.pivot = new Vector2(0.5f, 0.5f);
            playerDot.anchoredPosition = localPos;
            playerDot.SetAsLastSibling();
        }

        private void ApplyCellState(MinimapCellView cellView, Vector2Int gridPos, MinimapCellState state)
        {
            WFCData tileData = MinimapService.Instance?.GetTileData(gridPos);
            cellView.ApplyState(state, tileData, hiddenColor, visitedColor);
        }

        private bool IsInsideCurrentView(Vector2Int gridPos)
        {
            return gridPos.x >= _viewMin.x
                && gridPos.y >= _viewMin.y
                && gridPos.x < _viewMin.x + _viewCellCount
                && gridPos.y < _viewMin.y + _viewCellCount;
        }

        private void EnsureCellPool()
        {
            int requiredViewCells = visibleCellRadius * 2 + 1;
            int maxGrid = MinimapService.Instance != null && MinimapService.Instance.IsReady
                ? MinimapService.Instance.GridSize
                : 10;
            int viewCells = Mathf.Min(requiredViewCells, maxGrid);
            int requiredCount = viewCells * viewCells;

            if (_cellPool != null && _poolViewCells == viewCells && _cellPool.Length == requiredCount)
                return;

            if (_cellPool != null)
            {
                for (int i = 0; i < _cellPool.Length; i++)
                {
                    if (_cellPool[i] != null)
                        Destroy(_cellPool[i].gameObject);
                }
            }

            if (cellPrefab == null || cellContainer == null)
                return;

            _poolViewCells = viewCells;
            _cellPool = new MinimapCellView[requiredCount];

            for (int i = 0; i < requiredCount; i++)
            {
                MinimapCellView instance = Instantiate(cellPrefab, cellContainer);
                instance.gameObject.SetActive(false);
                _cellPool[i] = instance;
            }
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }
    }
}
