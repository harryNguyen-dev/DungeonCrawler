using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using WFC;
using Player;
namespace Core
{
    public class SpawnPlayerManager : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private WFCGeneration _dungeon;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private CinemachineCamera _camera;

        private GameObject _spawnedPlayer;

        private void OnEnable()
        {
            GlobalEvents.OnDungeonGeneratedSuccess += OnDungeonGeneratedSuccess;
        }
        private void OnDisable()
        {
            GlobalEvents.OnDungeonGeneratedSuccess -= OnDungeonGeneratedSuccess;
        }

        private void OnDungeonGeneratedSuccess(int seed)
        {
            if (_playerPrefab == null || _dungeon == null)
                return;

            Tile[,] grid = _dungeon.Grid;
            if (grid == null)
                return;

            var rooms = new List<Tile>();
            int w = grid.GetLength(0);
            int h = grid.GetLength(1);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Tile t = grid[x, y];
                    if (t.IsCollapsed && t.CollapsedTile != null && t.CollapsedTile.tileType == TileType.Room)
                        rooms.Add(t);
                }
            }

            if (rooms.Count == 0)
            {
                Debug.LogWarning("SpawnPlayerManager: Không tìm thấy tile Room nào trên grid.");
                return;
            }

            float cell = _dungeon.CellSize;
            Tile pick = rooms[new System.Random(seed).Next(rooms.Count)];
            Vector3 center = GetRoomSpawnWorldPosition(pick, cell);

            Debug.Log($"SpawnPlayerManager: Spawn player at {center}");
            if (_spawnedPlayer != null)
                Destroy(_spawnedPlayer);

            _spawnedPlayer = Instantiate(_playerPrefab, center, Quaternion.identity);

            _inputManager.SetPlayer(_spawnedPlayer);
            _camera.Follow = _spawnedPlayer.transform;
            _spawnedPlayer.GetComponent<PlayerController>().cameraTransform = _camera.transform;
        }

        /// <summary>
        /// Room prefab được đặt tại góc lưới như <see cref="WFC.Tile.SpawnObject"/>; pivot thường trùng tâm ô
        /// nên không cộng nửa cell. Lấy tâm XZ từ bounds renderer của instance đã spawn.
        /// </summary>
        private static Vector3 GetRoomSpawnWorldPosition(Tile pick, float cell)
        {
            const float yFallback = 1f;

            if (pick.SpawnedObject != null)
            {
                Renderer[] renderers = pick.SpawnedObject.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                        b.Encapsulate(renderers[i].bounds);
                    return new Vector3(b.center.x, yFallback, b.center.z);
                }

                Vector3 p = pick.SpawnedObject.transform.position;
                return new Vector3(p.x, yFallback, p.z);
            }

            return new Vector3(
                pick.GridPosition.x * cell,
                yFallback,
                pick.GridPosition.y * cell);
        }
    }
}
