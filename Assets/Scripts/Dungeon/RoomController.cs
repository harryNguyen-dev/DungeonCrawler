using UnityEngine;
using Global;
using Enemy;
public class RoomController : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int _enemyCount;
    private bool isPlayerReached = false;
    public bool IsPlayerReached { get => isPlayerReached; set => isPlayerReached = value; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SpawnEnemy(other.transform);
            isPlayerReached = true;
        }
    }

    private void SpawnEnemy(Transform player)
    {
        if (isPlayerReached)
        {
            return;
        }
        var random = new System.Random(GlobalVariable.CurrentSeed);
        _enemyCount += random.Next(-1, 4);
        for (int i = 0; i < _enemyCount; i++)
        {
            // Vector3 spawnPosition = System.Random.Range(0, GlobalVariable.CurrentSeed);
            var spawnPoint = spawnPoints[random.Next(0, spawnPoints.Length)];
            var enemy = Instantiate(_enemyPrefab, spawnPoint.position, Quaternion.identity);
            SkeletonAI ai = enemy.GetComponent<SkeletonAI>();
            if (ai != null)
                ai.SetTarget(player);
            
        }
    }
}
