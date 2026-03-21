using UnityEngine;
using System.Collections.Generic;


namespace WFC 
{
    public class WFCDungeonGenerator : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float roomSize = 20f;

        [Header("Tile Data")]
        [SerializeField] private List<RoomTileData> allTiles = new List<RoomTileData>();

        [SerializeField] private RoomTileData startRoomTile;
        [SerializeField] private RoomTileData bossRoomTile;
        [SerializeField] private RoomTileData treasureRoomTile;
        [SerializeField] private RoomTileData exitTile;
    

        [Header("Generation Settings")]
        [SerializeField] private int maxRestartAttempts = 10;
        [SerializeField] private int randomSeed = 0;
        [SerializeField] private bool useRandomSeed = true;

        [Header("References")]
        [SerializeField] private Transform dungeonHolder;

        private Dictionary<GameObject, Queue<GameObject>> pool
            = new Dictionary<GameObject, Queue<GameObject>>();

        private WFCGrid grid;
        private List<GameObject> spawnedRooms = new List<GameObject>();

        public System.Action OnGenerated;    

        private void Start() 
        {
            Generate();
        }

        public void Generate()
        {
            if(useRandomSeed)
                randomSeed = UnityEngine.Random.Range(0, int.MaxValue);

            Random.InitState(randomSeed);
            Debug.Log($"WFC seed: {randomSeed}");

            for (int attempt = 0; attempt < maxRestartAttempts; attempt++) 
            {
                grid = new WFCGrid(gridWidth, gridHeight, allTiles);
                
                int startX = gridWidth  / 2;
                int startY = gridHeight / 2;
                bool ok = grid.CollapseAt(startX, startY, startRoomTile);

                if(!ok) 
                {
                    Debug.LogWarning($"Attempt {attempt + 1}: startroom make contradiction");
                    continue;
                }

                ok = grid.Run();

                if(!ok) 
                {
                    Debug.LogWarning($"Attempt {attempt + 1}: WFC contradiction failed");
                    Random.InitState(++randomSeed);
                    continue;
                }

                PlaceSpecialRooms();
                MapTo3D();
                OnGenerated?.Invoke();
                Debug.Log($"WFC thành công sau {attempt + 1} lần thử.");
                return;
            }
            Debug.LogError("WFC thất bại sau tất cả các lần thử!");
        }


        private void PlaceSpecialRooms() 
        {
            TryPlaceSpecial(bossRoomTile,    FindFarthestCell);
            TryPlaceSpecial(treasureRoomTile,  FindRandomSuitableCell);
            TryPlaceSpecial(exitTile,        FindRandomSuitableCell);
        }
        private void TryPlaceSpecial(RoomTileData tile,
            System.Func<RoomTileData, (int x, int y)> findCell)
        {
            if (tile == null) return;
            var (x, y) = findCell(tile);
            if (x < 0) return; // không tìm được ô phù hợp

            grid.CollapseAt(x, y, tile);
        }

        /// <summary> Tìm ô Normal/Hallway xa StartRoom nhất — dùng cho BossRoom. </summary>
        private (int x, int y) FindFarthestCell(RoomTileData tile)
        {
            int startX = gridWidth  / 2;
            int startY = gridHeight / 2;
            int bestX = -1, bestY = -1;
            int bestDist = -1;

            for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            {
                var collapsed = grid.GetCollapsedTile(x, y);
                if (collapsed == null) continue;
                if (collapsed == startRoomTile) continue;
                if (collapsed.Weight <= 0f) continue; // bỏ Wall

                int dist = Mathf.Abs(x - startX) + Mathf.Abs(y - startY);
                if (dist > bestDist)
                {
                    bestDist = dist;
                    bestX = x;
                    bestY = y;
                }
            }

            return (bestX, bestY);
        }

         /// <summary> Tìm ô Normal/Hallway ngẫu nhiên — dùng cho SuperChest, Exit. </summary>
        private (int x, int y) FindRandomSuitableCell(RoomTileData tile)
        {
            List<(int x, int y)> candidates = new List<(int, int)>();

            for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            {
                var collapsed = grid.GetCollapsedTile(x, y);
                if (collapsed == null) continue;
                if (collapsed == startRoomTile ||
                    collapsed == bossRoomTile  ||
                    collapsed == treasureRoomTile||
                    collapsed == exitTile) continue;
                if (collapsed.Weight <= 0f) continue;

                candidates.Add((x, y));
            }

            if (candidates.Count == 0) return (-1, -1);
            return candidates[Random.Range(0, candidates.Count)];
        }

        /// <summary>
        /// Duyệt grid đã collapsed, Instantiate prefab tại đúng vị trí.
        /// </summary>
        private void MapTo3D()
        {
            ClearSpawnedRooms();

            for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            {
                RoomTileData tile = grid.GetCollapsedTile(x, y);
                if (tile == null || tile.prefab == null) continue;

                Vector3 pos = new Vector3(x * roomSize, 0f, y * roomSize);
                GameObject room = GetOrCreateFromPool(tile.prefab, pos, Quaternion.identity);
                spawnedRooms.Add(room);
            }
        }

        private void ClearSpawnedRooms()
        {
            foreach (var room in spawnedRooms)
                ReturnToPool(room);
            spawnedRooms.Clear();
        }

        // ── Pool system (giữ nguyên logic từ project cũ) ──────────────────

        private GameObject GetOrCreateFromPool(GameObject prefab,
            Vector3 position, Quaternion rotation)
        {
            if (pool.TryGetValue(prefab, out var queue) && queue.Count > 0)
            {
                var instance = queue.Dequeue();
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.transform.SetParent(dungeonHolder);
                instance.SetActive(true);
                return instance;
            }
            else
            {
                var instance = Instantiate(prefab, position, rotation);
                instance.transform.SetParent(dungeonHolder);
                return instance;
            }
        }

        private void ReturnToPool(GameObject instance)
        {
            if (instance == null) return;
            var prefab = GetSourcePrefab(instance);
            if (prefab == null) { Destroy(instance); return; }

            instance.SetActive(false);
            instance.transform.SetParent(dungeonHolder);
            if (!pool.ContainsKey(prefab))
                pool[prefab] = new Queue<GameObject>();
            pool[prefab].Enqueue(instance);
        }

        // Lưu prefab gốc vào instance để pool biết trả về đâu
        private Dictionary<GameObject, GameObject> _instanceToPrefab
            = new Dictionary<GameObject, GameObject>();

        private GameObject GetSourcePrefab(GameObject instance)
        {
            _instanceToPrefab.TryGetValue(instance, out var prefab);
            return prefab;
        }

        public WFCGrid Grid => grid;
    }
}