using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.SpawnManager
{
    public class SpawnEnemyWaveManager : MonoBehaviour
    {
        public List<Transform> spawnPoints;
        public List<GameObject> enemyPrefabs;

        public float spawnInterval = 5f;

        public int wave = 0;
        public int numberMonsterEachWave = 5;
        public int NumberCurrentLeft = 5;

        private void OnEnable()
        {
            Global.GlobalEvents.OnGameStart += HandleGameStart;
            Global.GlobalEvents.OnEnemyDie += HandleEnemyDie;
        }

        private void OnDisable()
        {
            Global.GlobalEvents.OnGameStart -= HandleGameStart;
            Global.GlobalEvents.OnEnemyDie -= HandleEnemyDie;
        }

        private void HandleGameStart()
        {
            ResetGame();
        }

        private void SpawnEnemy()
        {
            if (enemyPrefabs == null || enemyPrefabs.Count == 0 || spawnPoints == null || spawnPoints.Count == 0)
            {
                Debug.LogWarning("[SpawnEnemyWaveManager] Missing enemy prefabs or spawn points.");
                return;
            }

            int randomIndex = Random.Range(0, enemyPrefabs.Count);
            GameObject enemy = enemyPrefabs[randomIndex];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject e = Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);
            Global.GlobalEntities.Instance.RegisterEnemy(e);
        }
        public void ResetGame()
        {
            wave = 0;
            numberMonsterEachWave = 5;
            NumberCurrentLeft = numberMonsterEachWave;
            NextWave().Forget();
        }
        public UniTask NextWave()
        {
            for (int i = 0; i < numberMonsterEachWave; i++)
            {
                SpawnEnemy();
            }
            wave++;
            return UniTask.CompletedTask;
        }
        private void HandleEnemyDie()
        {
            NumberCurrentLeft--;
            if (NumberCurrentLeft <= 0)
            {
                StartNextWave().Forget();
            }
        }
        private async UniTaskVoid StartNextWave()
        {
            Debug.Log($"[Wave] Wave {wave} cleared! Prepare for next wave...");

            await UniTask.Delay(1500);

            wave++;
            numberMonsterEachWave += 2;
            NumberCurrentLeft = numberMonsterEachWave;

            await NextWave();
        }
    }
}
