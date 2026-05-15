using UnityEngine;
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
        }
    }

    private void SpawnEnemy(Transform player)
    {
    }
}
