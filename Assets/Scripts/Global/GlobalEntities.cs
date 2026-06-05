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

        [Header("Weapons")]
        public WeaponCatalogSO WeaponCatalog;

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
            EnsureWeaponCatalog();
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
            EnsureWeaponCatalog();
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

        private void EnsureWeaponCatalog()
        {
            if (WeaponCatalog != null)
                return;

#if UNITY_EDITOR
            WeaponCatalog = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponCatalogSO>(
                "Assets/SO/Weapon/WeaponCatalog_Global.asset");
#endif
            if (WeaponCatalog == null)
                Debug.LogWarning("[GlobalEntities] WeaponCatalog is not assigned.");
        }

        public WeaponSO GetWeapon(string weaponId) => WeaponCatalog?.GetById(weaponId);

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
            SetPlayerAttackEnabled(canAttack);
            BindCameraToPlayer();

            Vector3 offset = Vector3.up * 2.5f;
            Vector3 spawnPoint = GlobalVariable.PlayerSpawnPosition;
            PlayerInstance.transform.position = spawnPoint + offset;
            GlobalEvents.RaisePlayerJoin();
        }

        public void SetPlayerAttackEnabled(bool enabled)
        {
            if (PlayerInstance == null) return;
            var attack = PlayerInstance.GetComponent<PlayerController.Attack>();
            if (attack != null)
            {
                attack.SetAttackEnabled(enabled);
            }
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
                    Core.ObjectPoolingManager.SafeReturn(AvailableEnemies[i]);
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
                    Core.ObjectPoolingManager.SafeReturn(projectile.gameObject);
            }
        }

        public List<CardSO> GetAllCards() => AllCards ?? new List<CardSO>();
    }
}
