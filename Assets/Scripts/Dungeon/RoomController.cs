using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
public class RoomController : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private int waveCount = 1;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private int _enemyCount;
    private DoorController[] doorsController;
    public bool isPlayerReached = false;
    public bool IsPlayerReached { get => isPlayerReached; set => isPlayerReached = value; }
    public bool IsCleared = false;
    bool isStartRoom = false;
    private void Start()
    {
        var parent = gameObject.transform.parent;
        doorsController = parent.gameObject.GetComponentsInChildren<DoorController>();
    }
    public void SetIsStartRoom(bool isStartRoom)
    {
        this.isStartRoom = isStartRoom;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isStartRoom)
            {
                Debug.Log("[RoomController] startRoom -> skip spawn");
                return;
            }
            if (isPlayerReached)
            {
                Debug.Log("[RoomController] player reached -> skip spawn");
                return;
            }
            isPlayerReached = true;
            CloseDoors();
            SpawnWave(0, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }
    private async UniTask SpawnWave(int waveIndex, CancellationToken cancellationToken)
    {
        if(waveIndex > waveCount)
        {
            return;
        }

        await UniTask.Delay(500, cancellationToken: cancellationToken);
        for (int i = 0; i < enemiesPerWave; i++)
        {
            await UniTask.Delay(500, cancellationToken: cancellationToken);
            SpawnEnemy();
        }
        Debug.Log("[RoomController] wave " + waveIndex + " spawned");
        Debug.Log("[RoomController] wait until all enemies are dead");
        await UniTask.WaitUntil(() => Global.GlobalEntities.Instance.AvailableEnemies.Count == 0, cancellationToken: cancellationToken);
        await UniTask.Delay(500, cancellationToken: cancellationToken);
        if(waveIndex + 1 >= waveCount)
        {
            // Finish this room => return and fire a event 
            Debug.Log("[RoomController] finish room");
            OpenDoors();
            IsCleared = true;
            Global.GlobalEvents.RaiseRoomCleared();
            return;
        }
        SpawnWave(waveIndex + 1, cancellationToken).Forget();
    }
    private void SpawnEnemy()
    {
        var ListPrefab = Global.GlobalEntities.Instance.EnemyPrefabs;
        var randomIndex = Random.Range(0, ListPrefab.Count);
        var enemy = ListPrefab[randomIndex];
        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var e = Instantiate(enemy, spawnPoint.position, spawnPoint.rotation);
        Global.GlobalEntities.Instance.RegisterEnemy(e);
    }
    private void CloseDoors()
    {
        foreach (var doorController in doorsController)
        {
            doorController.SetClose();
        }
    }
    private void OpenDoors()
    {
        foreach (var doorController in doorsController)
        {
            doorController.SetOpen();
        }
    }

}
