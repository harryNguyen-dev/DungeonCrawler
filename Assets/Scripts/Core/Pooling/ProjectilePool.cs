using UnityEngine;

namespace Core
{
    public class ProjectilePool : ObjectPoolBase
    {
        public static ProjectilePool Instance { get; private set; }

        protected override string LogName => nameof(ProjectilePool);

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
