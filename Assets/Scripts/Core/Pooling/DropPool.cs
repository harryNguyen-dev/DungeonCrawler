using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class DropPool : MonoBehaviour
    {
        public static DropPool Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private DropEntity goldPrefab;
        [SerializeField] private DropEntity expPrefab;

        [Header("Pool")]
        [SerializeField] private int prewarmCount = 12;
        [SerializeField] private float spawnRadius = 0.35f;
        [SerializeField] private Transform poolRoot;

        readonly Stack<DropEntity> _goldPool = new();
        readonly Stack<DropEntity> _expPool = new();
        readonly HashSet<DropEntity> _active = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
    
            if (poolRoot == null)
            {
                var root = new GameObject("DropPool_Root");
                root.transform.SetParent(transform, false);
                poolRoot = root.transform;
            }
            DontDestroyOnLoad(gameObject);
            Prewarm(goldPrefab, _goldPool);
            Prewarm(expPrefab, _expPool);
        } 
        /// <summary>Spawn gold/exp pickups at enemy death position (values from EnemySO).</summary>
        public void SpawnFromEnemy(Vector3 position, int goldValue, int expValue)
        {
            if (goldValue > 0)
                Spawn(DropType.Gold, position, goldValue);

            if (expValue > 0)
                Spawn(DropType.Exp, position, expValue);
        }

        public void Return(DropEntity entity)
        {
            if (entity == null || !_active.Remove(entity))
                return;

            entity.OnReturnedToPool();
            entity.gameObject.SetActive(false);
            entity.transform.SetParent(poolRoot, false);

            switch (entity.Type)
            {
                case DropType.Gold:
                    _goldPool.Push(entity);
                    break;
                case DropType.Exp:
                    _expPool.Push(entity);
                    break;
            }
        }

        public void ReturnAllActive()
        {
            var snapshot = new List<DropEntity>(_active);
            for (var i = 0; i < snapshot.Count; i++)
                Return(snapshot[i]);
        }

        void Spawn(DropType type, Vector3 position, int value)
        {
            var prefab = GetPrefab(type);
            if (prefab == null)
            {
                Debug.LogWarning($"DropPool: missing prefab for {type}.");
                ApplyRewardDirect(type, value);
                return;
            }

            var entity = Get(type);
            if (entity == null)
            {
                ApplyRewardDirect(type, value);
                return;
            }

            var offset = Random.insideUnitSphere;
            offset.y = 0f;
            var spawnPos = position + offset * spawnRadius;

            entity.gameObject.SetActive(true);
            entity.transform.SetParent(null, true);
            _active.Add(entity);
            entity.OnSpawnedFromPool();
            entity.Initialize(type, value, spawnPos);
        }

        DropEntity Get(DropType type)
        {
            var stack = type == DropType.Gold ? _goldPool : _expPool;
            var prefab = GetPrefab(type);

            DropEntity entity;
            if (stack.Count > 0)
            {
                entity = stack.Pop();
            }
            else
            {
                entity = Instantiate(prefab, poolRoot);
            }

            return entity;
        }

        DropEntity GetPrefab(DropType type) => type == DropType.Gold ? goldPrefab : expPrefab;

        void Prewarm(DropEntity prefab, Stack<DropEntity> stack)
        {
            if (prefab == null) return;

            for (var i = 0; i < prewarmCount; i++)
            {
                var entity = Instantiate(prefab, poolRoot);
                entity.gameObject.SetActive(false);
                stack.Push(entity);
            }
        }

        static void ApplyRewardDirect(DropType type, int value)
        {
            switch (type)
            {
                case DropType.Gold:
                    GameManager.Instance?.CollectGold(value);
                    break;
                case DropType.Exp:
                    Global.GlobalEntities.Instance?.PlayerStats?.CollectExp(value);
                    break;
            }
        }
    }
}
