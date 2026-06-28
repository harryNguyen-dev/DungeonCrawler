using System.Collections.Generic;
using SO;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Global
{
    public class GlobalEntities : MonoBehaviour
    {
        public static GlobalEntities Instance { get; private set; }

        [Header("Player Reference")]
        public GameObject PlayerPrefab;
        [HideInInspector] public PlayerController.PlayerStats PlayerStats;
        [HideInInspector] public PlayerController.PlayerEffect PlayerEffect;
        [HideInInspector] public PlayerController.Health PlayerHealth;
        [HideInInspector] public PlayerController.PlayerEvents PlayerEvents;

        [HideInInspector] public GameObject PlayerInstance;

        [Header("Camera")]
        public CinemachineCamera CinemachineCamera;

        [Header("CardSO")]
        public List<CardSO> AllCards;

        [Header("Levels")]
        public LevelCatalogSO Chapter1Catalog;

        [Header("Heroes")]
        public HeroCatalogSO HeroCatalog;
        public DashConfigSO DefaultDashConfig;

        [Header("Enemies")]
        public List<GameObject> EnemyPrefabs;
        [Tooltip("Prefab boss riêng (optional). Nếu null, dùng enemy ngẫu nhiên + HP scale trong boss room.")]
        public GameObject BossPrefab;
        public List<GameObject> AvailableEnemies = new List<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            EnsureChapterCatalog();
            EnsureHeroCatalog();
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            Instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ClearRuntimeSceneObjects();
            EnsureChapterCatalog();
            EnsureHeroCatalog();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            ClearRuntimeSceneObjects();
        }

        public LevelCatalogSO GetChapter1Catalog()
        {
            EnsureChapterCatalog();
            return Chapter1Catalog;
        }

        private void EnsureChapterCatalog()
        {
            if (Chapter1Catalog != null)
                return;

#if UNITY_EDITOR
            Chapter1Catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelCatalogSO>(
                "Assets/SO/Level/LevelCatalog_Chapter1.asset");
#endif
            if (Chapter1Catalog == null)
                Debug.LogWarning("[GlobalEntities] Chapter1Catalog is not assigned.");
        }

        private void EnsureHeroCatalog()
        {
            if (HeroCatalog != null)
                return;

#if UNITY_EDITOR
            HeroCatalog = UnityEditor.AssetDatabase.LoadAssetAtPath<HeroCatalogSO>(
                "Assets/SO/Hero/HeroCatalog_Global.asset");
            if (DefaultDashConfig == null)
                DefaultDashConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<DashConfigSO>(
                    "Assets/SO/Hero/DashConfig_Default.asset");
#endif
            if (HeroCatalog == null)
                Debug.LogWarning("[GlobalEntities] HeroCatalog is not assigned.");
        }

        public HeroSO GetHero(string heroId) => HeroCatalog?.GetById(heroId);

        public void ClearRuntimeSceneObjects()
        {
            ClearProjectiles();
            ClearAllEnemies();
            ClearPlayer();
        }

        public void ClearPlayer()
        {
            if (PlayerInstance != null)
            {
                Destroy(PlayerInstance);
            }

            PlayerInstance = null;
            PlayerStats = null;
            PlayerHealth = null;
            PlayerEffect = null;
            PlayerEvents = null;
            if (CinemachineCamera != null)
            {
                CinemachineCamera.Target.TrackingTarget = null;
            }
        }

        public void SpawnPlayer(bool canAttack = true)
        {
            ClearPlayer();

            GameObject player = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
            PlayerInstance = player;
            PlayerStats = PlayerInstance.GetComponent<PlayerController.PlayerStats>();
            PlayerHealth = PlayerInstance.GetComponent<PlayerController.Health>();
            PlayerEffect = PlayerInstance.GetComponent<PlayerController.PlayerEffect>();
            PlayerEvents = PlayerInstance.GetComponent<PlayerController.PlayerEvents>();
            SetPlayerCombatEnabled(canAttack);
            BindCameraToPlayer();

            PlayerInstance.transform.position = GlobalVariable.PlayerSpawnPosition;
            GlobalEvents.RaisePlayerJoin();
        }

        public void SetPlayerAttackEnabled(bool enabled) => SetPlayerCombatEnabled(enabled);

        public void SetPlayerCombatEnabled(bool enabled)
        {
            if (PlayerInstance == null) return;

            var attack = PlayerInstance.GetComponent<PlayerController.Attack>();
            if (attack != null)
                attack.SetAttackEnabled(enabled);

            var skill = PlayerInstance.GetComponent<PlayerController.PlayerSkill>();
            if (skill != null)
                skill.SetSkillEnabled(enabled);

            var dash = PlayerInstance.GetComponent<PlayerController.PlayerDash>();
            if (dash != null)
                dash.SetDashEnabled(enabled);
        }

        public void BindCameraToPlayer()
        {
            if (PlayerInstance == null) return;
            BindCameraTo(PlayerInstance.transform);
        }

        public void BindCameraTo(Transform target)
        {
            if (CinemachineCamera == null)
            {
                CinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            }

            if (CinemachineCamera == null) return;
            CinemachineCamera.Target.TrackingTarget = target;
        }

        public void RegisterEnemy(GameObject enemy)
        {
            if (enemy == null || AvailableEnemies.Contains(enemy)) return;
            AvailableEnemies.Add(enemy);
        }

        public void UnregisterEnemy(GameObject enemy)
        {
            AvailableEnemies.Remove(enemy);
        }

        public void ClearAllEnemies()
        {
            for (int i = AvailableEnemies.Count - 1; i >= 0; i--)
            {
                if (AvailableEnemies[i] != null)
                    Core.PoolReturn.SafeReturn(AvailableEnemies[i]);
            }

            AvailableEnemies.Clear();
        }

        private void ClearProjectiles()
        {
            ClearActiveProjectiles<Projectile.ProjectileController>();
            ClearActiveProjectiles<EnemyController.EnemyProjectile>();
        }

        private static void ClearActiveProjectiles<T>() where T : MonoBehaviour
        {
            var projectiles = FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var projectile in projectiles)
            {
                if (projectile != null)
                    Core.PoolReturn.SafeReturn(projectile.gameObject);
            }
        }

        public List<CardSO> GetAllCards() => AllCards ?? new List<CardSO>();
    }
}
