using UnityEngine;

namespace Core
{
    public class EnemyPool : ObjectPoolBase
    {
        public static EnemyPool Instance { get; private set; }

        protected override string LogName => nameof(EnemyPool);

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildPools();
        }

        void Start() => PrewarmAll();

#if UNITY_EDITOR
        void OnValidate() => ValidateEntries();
#endif

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
