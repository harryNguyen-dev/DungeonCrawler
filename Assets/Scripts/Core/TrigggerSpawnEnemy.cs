using UnityEngine;
using Enemy;

public class TrigggerSpawnEnemy : MonoBehaviour
{
    [SerializeField] GameObject _enemyPrefab;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // SpawnEnemy(other.transform);
        }
    }
    public void SpawnEnemy(Transform player)
    {
        // Vector3 spawnPosition = player.position + new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));

        // GameObject enemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);

        // // Truyền player instance thật vào skeleton
        // SkeletonAI ai = enemy.GetComponent<SkeletonAI>();
        // if (ai != null)
        //     ai.SetTarget(player);
    }
}
