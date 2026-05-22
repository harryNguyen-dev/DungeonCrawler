using UnityEngine;
public class RoomController : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int _enemyCount;
    private bool isPlayerReached = false;
    public bool IsPlayerReached { get => isPlayerReached; set => isPlayerReached = value; }
    
    bool isStartRoom = false;
    public void SetIsStartRoom(bool isStartRoom)
    {
        this.isStartRoom = isStartRoom;
    }
    
    private void OnTriggerEnter(Collider other)
    {
    }

    private void SpawnEnemy(Transform player)
    {
        if(isStartRoom)
        {
            return;
        }
        
    }
}
