using System;
using Global;
using UnityEngine;
using WFC;

namespace Core.Minimap
{
    public sealed class MinimapService : MonoBehaviour
    {
        public static MinimapService Instance { get; private set; }

        private static readonly Vector2Int[] NeighborDeltas =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        private static readonly Direction[] NeighborDirections =
        {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        };

        private sealed class CellData
        {
            public MinimapCellState State = MinimapCellState.Hidden;
            public WFCData TileData;
            public MinimapZoneBounds Bounds;
        }

        public event Action<Vector2Int, MinimapCellState> OnCellStateChanged;
        public event Action<MinimapPlayerMapPosition> OnPlayerMapPositionChanged;

        private CellData[,] _cells;
        private Tile[,] _gridCache;
        private int _gridSize;
        private float _cellSize;
        private bool _isReady;
        private Vector2Int _previewCenter = new(-1, -1);
        private MinimapPlayerMapPosition _playerMapPosition = MinimapPlayerMapPosition.Invalid;

        public bool IsReady => _isReady;
        public int GridSize => _gridSize;
        public float CellSize => _cellSize;
        public MinimapPlayerMapPosition PlayerMapPosition => _playerMapPosition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            GlobalEvents.OnDungeonGenerationStarted += HandleGenerationStarted;
            GlobalEvents.OnDungeonGenerated += HandleDungeonGenerated;
            GlobalEvents.OnRoomEntered += HandleRoomEntered;
            GlobalEvents.OnMatchReset += HandleMatchReset;
        }

        private void OnDisable()
        {
            GlobalEvents.OnDungeonGenerationStarted -= HandleGenerationStarted;
            GlobalEvents.OnDungeonGenerated -= HandleDungeonGenerated;
            GlobalEvents.OnRoomEntered -= HandleRoomEntered;
            GlobalEvents.OnMatchReset -= HandleMatchReset;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Initialize(Tile[,] grid, int gridSize, float cellSize)
        {
            ResetInternal();

            _gridCache = grid;
            _gridSize = gridSize;
            _cellSize = cellSize;
            _cells = new CellData[gridSize, gridSize];

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    Tile tile = grid[x, y];
                    _cells[x, y] = new CellData
                    {
                        TileData = tile != null && tile.IsCollapsed ? tile.CollapsedTile : null
                    };
                }
            }

            _isReady = true;
            RegisterAllZonesFromGrid();
            RevealStartRoom();
            SyncVisitedFromScene();
            EnsureInitialPreview();
        }

        public void RegisterZone(Vector2Int gridPos, MinimapZoneBounds bounds)
        {
            if (!_isReady || !IsInsideGrid(gridPos))
                return;

            _cells[gridPos.x, gridPos.y].Bounds = bounds;
        }

        public void RegisterZoneFromRoom(GameObject spawnedRoot, Vector2Int gridPos)
        {
            if (spawnedRoot == null)
                return;

            var room = spawnedRoot.GetComponentInChildren<RoomController>();
            if (room != null && room.TryGetMinimapZoneCollider(out Collider collider))
            {
                RegisterZone(gridPos, MinimapZoneBounds.FromCollider(collider, gridPos));
                return;
            }

            RegisterZone(gridPos, MinimapZoneBounds.Invalid(gridPos));
        }

        public void MarkVisited(Vector2Int gridPos)
        {
            if (!_isReady || !IsInsideGrid(gridPos))
                return;

            SetCellState(gridPos, MinimapCellState.Visited);
        }

        public MinimapCellState GetCellState(Vector2Int pos)
        {
            if (!_isReady || !IsInsideGrid(pos))
                return MinimapCellState.Hidden;

            return _cells[pos.x, pos.y].State;
        }

        public WFCData GetTileData(Vector2Int pos)
        {
            if (!_isReady || !IsInsideGrid(pos))
                return null;

            return _cells[pos.x, pos.y].TileData;
        }

        public void UpdatePlayerWorldPosition(Vector3 worldPos)
        {
            if (!_isReady || _cellSize <= 0f)
                return;

            Vector2Int gridCell = ResolveGridCell(worldPos);
            Vector2 localUv = ResolveLocalUv(worldPos, gridCell);
            var mapPos = new MinimapPlayerMapPosition
            {
                GridCell = gridCell,
                LocalUv = localUv,
                FractionalGrid = new Vector2(gridCell.x + localUv.x, gridCell.y + localUv.y)
            };

            if (mapPos.GridCell != _playerMapPosition.GridCell
                || Vector2.Distance(mapPos.LocalUv, _playerMapPosition.LocalUv) > 0.001f
                || Vector2.Distance(mapPos.FractionalGrid, _playerMapPosition.FractionalGrid) > 0.001f)
            {
                _playerMapPosition = mapPos;
                OnPlayerMapPositionChanged?.Invoke(_playerMapPosition);
            }

            if (gridCell != _previewCenter)
            {
                _previewCenter = gridCell;
                RefreshAdjacentPreviews(gridCell);
            }
        }

        private void RegisterAllZonesFromGrid()
        {
            if (_gridCache == null)
                return;

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    Tile tile = _gridCache[x, y];
                    if (tile?.SpawnedObject == null)
                        continue;

                    RegisterZoneFromRoom(tile.SpawnedObject, tile.GridPosition);
                }
            }
        }

        private void HandleGenerationStarted()
        {
            ResetInternal();
        }

        private void HandleDungeonGenerated(int seed)
        {
            var wfc = FindFirstObjectByType<WFCGeneration>();
            if (wfc == null || wfc.Grid == null)
                return;

            Initialize(wfc.Grid, wfc.GridSize, wfc.CellSize);
        }

        private void HandleRoomEntered(Vector2Int gridPos)
        {
            MarkVisited(gridPos);
        }

        private void HandleMatchReset()
        {
            ResetInternal();
        }

        private void ResetInternal()
        {
            _isReady = false;
            _gridCache = null;
            _cells = null;
            _gridSize = 0;
            _cellSize = 0f;
            _previewCenter = new(-1, -1);
            _playerMapPosition = MinimapPlayerMapPosition.Invalid;
        }

        private void RevealStartRoom()
        {
            if (_gridCache == null)
                return;

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    Tile tile = _gridCache[x, y];
                    if (tile?.SpawnedObject == null || tile.CollapsedTile?.tileType != TileType.Room)
                        continue;

                    var room = tile.SpawnedObject.GetComponentInChildren<RoomController>();
                    if (room != null && room.GetRoomType() == RoomType.Start)
                    {
                        MarkVisited(tile.GridPosition);
                        return;
                    }
                }
            }
        }

        private void SyncVisitedFromScene()
        {
            if (_gridCache == null)
                return;

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    Tile tile = _gridCache[x, y];
                    if (tile?.SpawnedObject == null)
                        continue;

                    var room = tile.SpawnedObject.GetComponentInChildren<RoomController>();
                    if (room != null && room.IsPlayerReached)
                        SetCellState(tile.GridPosition, MinimapCellState.Visited, refreshPreview: false);
                }
            }

            if (_playerMapPosition.IsValid)
                RefreshAdjacentPreviews(_playerMapPosition.GridCell);
        }

        private void EnsureInitialPreview()
        {
            if (_previewCenter.x >= 0)
                return;

            if (_playerMapPosition.IsValid)
            {
                RefreshAdjacentPreviews(_playerMapPosition.GridCell);
                return;
            }

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    if (_cells[x, y].State != MinimapCellState.Visited)
                        continue;

                    RefreshAdjacentPreviews(new Vector2Int(x, y));
                    return;
                }
            }
        }

        private void RefreshAdjacentPreviews(Vector2Int center)
        {
            if (_cells == null || _gridCache == null)
                return;

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    if (_cells[x, y].State != MinimapCellState.Preview)
                        continue;

                    SetCellState(new Vector2Int(x, y), MinimapCellState.Hidden, refreshPreview: false);
                }
            }

            if (!IsInsideGrid(center))
                return;

            Tile currentTile = _gridCache[center.x, center.y];
            if (currentTile == null || !currentTile.IsCollapsed || currentTile.CollapsedTile == null)
                return;

            for (int i = 0; i < NeighborDeltas.Length; i++)
            {
                Vector2Int neighborPos = center + NeighborDeltas[i];
                if (!IsInsideGrid(neighborPos))
                    continue;

                if (_cells[neighborPos.x, neighborPos.y].State == MinimapCellState.Visited)
                    continue;

                Tile neighborTile = _gridCache[neighborPos.x, neighborPos.y];
                if (!IsPreviewableTile(neighborTile))
                    continue;

                if (!AreTilesConnected(currentTile, neighborTile, NeighborDirections[i]))
                    continue;

                SetCellState(neighborPos, MinimapCellState.Preview, refreshPreview: false);
            }
        }

        private void SetCellState(Vector2Int pos, MinimapCellState state, bool refreshPreview = true)
        {
            if (!IsInsideGrid(pos))
                return;

            CellData cell = _cells[pos.x, pos.y];
            if (cell.State == state)
                return;

            if (state == MinimapCellState.Hidden && !IsDrawableTile(cell.TileData))
                return;

            if (state is MinimapCellState.Preview or MinimapCellState.Visited
                && !IsDrawableTile(cell.TileData))
                return;

            cell.State = state;
            OnCellStateChanged?.Invoke(pos, state);

            if (refreshPreview && state == MinimapCellState.Visited)
                RefreshAdjacentPreviews(pos);
        }

        private Vector2Int ResolveGridCell(Vector3 worldPos)
        {
            Vector2Int floorCell = WorldToGrid(worldPos);

            if (IsInsideGrid(floorCell))
            {
                MinimapZoneBounds bounds = _cells[floorCell.x, floorCell.y].Bounds;
                if (bounds.IsValid && bounds.ContainsWorldPosition(worldPos))
                    return floorCell;
            }

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    MinimapZoneBounds bounds = _cells[x, y].Bounds;
                    if (bounds.IsValid && bounds.ContainsWorldPosition(worldPos))
                        return bounds.GridPos;
                }
            }

            return floorCell;
        }

        private Vector2 ResolveLocalUv(Vector3 worldPos, Vector2Int gridCell)
        {
            if (IsInsideGrid(gridCell))
            {
                MinimapZoneBounds bounds = _cells[gridCell.x, gridCell.y].Bounds;
                if (bounds.IsValid)
                    return bounds.NormalizeWorldPosition(worldPos);
            }

            float fx = worldPos.x / _cellSize - gridCell.x;
            float fy = worldPos.z / _cellSize - gridCell.y;
            return new Vector2(Mathf.Clamp01(fx), Mathf.Clamp01(fy));
        }

        private Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.Clamp(Mathf.FloorToInt(worldPos.x / _cellSize), 0, _gridSize - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(worldPos.z / _cellSize), 0, _gridSize - 1);
            return new Vector2Int(x, y);
        }

        private bool IsInsideGrid(Vector2Int pos)
        {
            return pos.x >= 0 && pos.y >= 0 && pos.x < _gridSize && pos.y < _gridSize;
        }

        private static bool IsDrawableTile(WFCData data)
        {
            return data != null && data.tileType != TileType.Empty;
        }

        private static bool IsPreviewableTile(Tile tile)
        {
            return tile != null
                && tile.IsCollapsed
                && tile.CollapsedTile != null
                && tile.CollapsedTile.tileType != TileType.Empty;
        }

        private static bool AreTilesConnected(Tile from, Tile to, Direction direction)
        {
            if (from?.CollapsedTile == null || to?.CollapsedTile == null)
                return false;

            return from.CollapsedTile.GetConnector(direction) == ConnectorType.Open
                && to.CollapsedTile.GetConnector(GetOpposite(direction)) == ConnectorType.Open;
        }

        private static Direction GetOpposite(Direction direction)
        {
            return direction switch
            {
                Direction.North => Direction.South,
                Direction.East => Direction.West,
                Direction.South => Direction.North,
                Direction.West => Direction.East,
                _ => Direction.North
            };
        }
    }
}
