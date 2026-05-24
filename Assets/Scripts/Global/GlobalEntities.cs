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
        [HideInInspector] public CombatFeel.PlayerHitEffect playerHitEffect;

        [HideInInspector] public GameObject PlayerInstance;

        [Header("Camera")]
        public CinemachineCamera CinemachineCamera;

        [Header("CardSO")]
        public List<CardSO> AllCards;

        [Header("Enemies")]
        public List<GameObject> EnemyPrefabs;
        public List<GameObject> AvailableEnemies = new List<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ClearRuntimeSceneObjects();
        }

        public void ClearRuntimeSceneObjects()
        {
            ClearPlayer();
            ClearAllEnemies();
            ClearProjectiles();
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
            playerHitEffect = null;
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
            playerHitEffect = PlayerInstance.GetComponent<CombatFeel.PlayerHitEffect>();
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
                {
                    Core.ObjectPoolingManager.Instance.Return(AvailableEnemies[i]);
                }
            }

            AvailableEnemies.Clear();
        }

        private void ClearProjectiles()
        {
            var projectiles = FindObjectsByType<Projectile.ProjectileController>(FindObjectsSortMode.None);
            foreach (var projectile in projectiles)
            {
                Core.ObjectPoolingManager.Instance.Return(projectile.gameObject);
            }
        }

        public List<CardSO> GetAllCards() => AllCards;
    }
}
