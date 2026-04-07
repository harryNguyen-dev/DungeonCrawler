using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public interface IPoolable
    {
        void OnSpawnedFromPool();
        void OnReturnedToPool();
    }

    [Serializable]
    public sealed class PoolConfig
    {
        [SerializeField] PoolId _id = PoolId.None;
        [SerializeField] GameObject _prefab;
        [SerializeField] int _prewarmCount = 8;
        [Tooltip("0 = unlimited growth when the pool is empty.")]
        [SerializeField] int _maxSize;

        public PoolId Id => _id;
        public GameObject Prefab => _prefab;
        public int PrewarmCount => Mathf.Max(0, _prewarmCount);
        public int MaxSize => Mathf.Max(0, _maxSize);
    }

    public class ObjectPoolingManager : MonoBehaviour
    {
        public static ObjectPoolingManager Instance { get; private set; }

        [SerializeField] List<PoolConfig> _entries = new();
        [SerializeField] Transform _poolRoot;

        readonly Dictionary<PoolId, Pool> _pools = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (_poolRoot == null)
            {
                var go = new GameObject("PooledObjects");
                go.transform.SetParent(transform, false);
                _poolRoot = go.transform;
            }

            foreach (var config in _entries)
            {
                if (config == null || config.Id == PoolId.None || config.Prefab == null)
                    continue;

                if (_pools.ContainsKey(config.Id))
                {
                    Debug.LogError($"ObjectPoolingManager: duplicate PoolId '{config.Id}' — skipping duplicate entry.");
                    continue;
                }

                var holder = new GameObject($"Pool_{config.Id}").transform;
                holder.SetParent(_poolRoot, false);
                _pools[config.Id] = new Pool(config, holder);
            }
        }

        void Start()
        {
            foreach (var pool in _pools.Values)
                pool.Prewarm();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (_entries == null) return;
            var seen = new HashSet<PoolId>();
            foreach (var e in _entries)
            {
                if (e == null || e.Id == PoolId.None) continue;
                if (!seen.Add(e.Id))
                    Debug.LogWarning($"ObjectPoolingManager: duplicate PoolId '{e.Id}' in serialized list.");
            }
        }
#endif

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Activate an instance from the pool (or create one if allowed).</summary>
        public GameObject Get(PoolId id, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!_pools.TryGetValue(id, out var pool))
            {
                Debug.LogError($"ObjectPoolingManager: no pool for '{id}'.");
                return null;
            }

            var instance = pool.Get(position, rotation, parent);
            if (instance != null)
                NotifySpawned(instance);
            return instance;
        }

        public GameObject Get(PoolId id) => Get(id, Vector3.zero, Quaternion.identity, null);

        /// <summary>Return an instance spawned by this manager (must have <see cref="PooledObject"/>).</summary>
        public void Return(GameObject instance)
        {
            if (instance == null) return;

            if (!instance.TryGetComponent<PooledObject>(out var tag) || tag.PoolId == PoolId.None)
            {
                Debug.LogWarning("ObjectPoolingManager.Return: object has no PooledObject / PoolId — destroying.");
                Destroy(instance);
                return;
            }

            if (!_pools.TryGetValue(tag.PoolId, out var pool))
            {
                Debug.LogWarning($"ObjectPoolingManager.Return: unknown pool '{tag.PoolId}' — destroying.");
                Destroy(instance);
                return;
            }

            NotifyReturned(instance);
            pool.Return(instance);
        }

        public void Return(PoolId id, GameObject instance)
        {
            if (instance == null) return;
            if (!instance.TryGetComponent<PooledObject>(out var tag))
                tag = instance.AddComponent<PooledObject>();
            tag.SetPoolId(id);
            Return(instance);
        }

        static void NotifySpawned(GameObject instance)
        {
            var poolables = instance.GetComponentsInChildren<IPoolable>(true);
            for (var i = 0; i < poolables.Length; i++)
                poolables[i].OnSpawnedFromPool();
        }

        static void NotifyReturned(GameObject instance)
        {
            var poolables = instance.GetComponentsInChildren<IPoolable>(true);
            for (var i = 0; i < poolables.Length; i++)
                poolables[i].OnReturnedToPool();
        }

        sealed class Pool
        {
            readonly PoolConfig _config;
            readonly Transform _holder;
            readonly Stack<GameObject> _inactive = new();
            int _created;

            public Pool(PoolConfig config, Transform holder)
            {
                _config = config;
                _holder = holder;
            }

            public void Prewarm()
            {
                int n = _config.PrewarmCount;
                for (var i = 0; i < n; i++)
                {
                    if (_config.MaxSize > 0 && _created >= _config.MaxSize)
                        break;
                    _inactive.Push(CreateInstance());
                }
            }

            public GameObject Get(Vector3 position, Quaternion rotation, Transform parent)
            {
                GameObject instance;
                if (_inactive.Count > 0)
                {
                    instance = _inactive.Pop();
                }
                else
                {
                    if (_config.MaxSize > 0 && _created >= _config.MaxSize)
                    {
                        Debug.LogWarning($"ObjectPoolingManager: pool '{_config.Id}' is at max size ({_config.MaxSize}).");
                        return null;
                    }

                    instance = CreateInstance();
                }

                var t = instance.transform;
                t.SetPositionAndRotation(position, rotation);
                t.SetParent(parent, true);
                instance.SetActive(true);
                return instance;
            }

            public void Return(GameObject instance)
            {
                instance.SetActive(false);
                instance.transform.SetParent(_holder, false);
                _inactive.Push(instance);
            }

            GameObject CreateInstance()
            {
                var go = Instantiate(_config.Prefab, _holder);
                go.name = _config.Prefab.name;
                _created++;

                var tag = go.GetComponent<PooledObject>();
                if (tag == null)
                    tag = go.AddComponent<PooledObject>();
                tag.SetPoolId(_config.Id);

                go.SetActive(false);
                return go;
            }
        }
    }
}
