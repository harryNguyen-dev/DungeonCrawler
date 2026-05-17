using System.Collections.Generic;
using Core.SpawnManager;
using SO;
using Unity.Cinemachine;
using UnityEngine;

namespace Global
{
    public class GlobalEntities : MonoBehaviour
    {
        public static GlobalEntities Instance { get; private set; }

        [Header("Player Reference")]
        public GameObject PlayerPrefab;
        [HideInInspector] public PlayerController.PlayerStats PlayerStats;
        [HideInInspector] public PlayerController.Health PlayerHealth;

        [HideInInspector] public GameObject PlayerInstance;

        [Header("Manager Reference")]
        public SpawnEnemyWaveManager SpawnEnemyManager;

        [Header("Camera")]
        public CinemachineCamera CinemachineCamera;
        

        [Header("CardSO")]
        public List<CardSO> AllCards;

        // Dùng List để quản lý tất cả quái đang sống
        List<GameObject> AvaiableEnemies = new List<GameObject>();
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); 
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public void ClearPlayer()
        {
            Destroy(PlayerInstance);
            PlayerInstance = null;
            PlayerStats = null;
            PlayerHealth = null;
            CinemachineCamera.Target.TrackingTarget = null;
        }
        public void SpawnPlayer()
        {
            GameObject player = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
            PlayerInstance = player;
            PlayerStats = PlayerInstance.GetComponent<PlayerController.PlayerStats>();
            PlayerHealth = PlayerInstance.GetComponent<PlayerController.Health>();
            CinemachineCamera.Target.TrackingTarget = PlayerInstance.transform;
        }
        private void Start()
        {
            SpawnPlayer();
        }
        public void RegisterEnemy(GameObject enemy)
        {
            AvaiableEnemies.Add(enemy);
        }
        public void UnregisterEnemy(GameObject enemy)
        {
            AvaiableEnemies.Remove(enemy);
        }
        public void ClearAllEnemies()
        {
            for (int i = 0; i < AvaiableEnemies.Count; i++)
            {
                Destroy(AvaiableEnemies[i]);
            }
            AvaiableEnemies.Clear();
        }
        public List<CardSO> GetAllCards() => AllCards;
    }
}