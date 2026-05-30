using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Core;
using EnemyController;
using Global;
using SO;

public enum RoomType
{
    Combat = 0,
    Start = 1,
    Boss = 2
}

public class RoomController : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    [Header("Fallback (khi không có LevelSO)")]
    [SerializeField] private int waveCount = 1;
    [SerializeField] private int enemiesPerWave = 3;
    [SerializeField] private float bossHealthMultiplier = 3f;

    private DoorController[] doorsController;
    public bool isPlayerReached = false;
    public bool IsPlayerReached { get => isPlayerReached; set => isPlayerReached = value; }
    public bool IsCleared = false;

    private RoomType roomType = RoomType.Combat;

    private WaveConfigSO ActiveWaveConfig => GlobalVariable.CurrentLevel?.combatWaves;
    private BossConfigSO ActiveBossConfig => GlobalVariable.CurrentLevel?.boss;
    private LevelSO ActiveLevel => GlobalVariable.CurrentLevel;

    private void Start()
    {
        var parent = gameObject.transform.parent;
        doorsController = parent.gameObject.GetComponentsInChildren<DoorController>();
    }

    public void SetIsStartRoom(bool isStartRoom)
    {
        if (isStartRoom)
            roomType = RoomType.Start;
    }

    public void SetRoomType(RoomType type)
    {
        roomType = type;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (roomType == RoomType.Start)
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

        var ct = this.GetCancellationTokenOnDestroy();
        if (roomType == RoomType.Boss)
            SpawnBossEncounter(ct).Forget();
        else
            SpawnWave(0, ct).Forget();
    }

    private async UniTask SpawnBossEncounter(CancellationToken cancellationToken)
    {
        var delay = ActiveBossConfig != null ? ActiveBossConfig.spawnDelayMs : 500;
        await UniTask.Delay(delay, cancellationToken: cancellationToken);
        SpawnBossEnemy();
        Debug.Log("[RoomController] boss spawned — waiting for defeat");

        await UniTask.WaitUntil(
            () => GlobalEntities.Instance.AvailableEnemies.Count == 0,
            cancellationToken: cancellationToken);

        await UniTask.Delay(500, cancellationToken: cancellationToken);
        OpenDoors();
        IsCleared = true;
        GlobalEvents.RaiseRoomCleared();
        Debug.Log("[RoomController] boss room cleared (win via OnBossDefeated)");
    }

    private void SpawnBossEnemy()
    {
        var entities = GlobalEntities.Instance;
        if (entities == null || spawnPoints == null || spawnPoints.Length == 0) return;

        GameObject prefab = ResolveBossPrefab(entities);
        var spawnPoint = spawnPoints[0];
        var e = SpawnEnemyInstance(prefab, spawnPoint.position, spawnPoint.rotation);
        entities.RegisterEnemy(e);

        var health = e.GetComponent<Health>();
        if (health == null) return;

        var ai = e.GetComponent<BaseAIController>();
        bool dataIsBoss = ai != null && ai.IsBossEnemy;
        var bossHpMult = ActiveBossConfig != null ? ActiveBossConfig.healthMultiplier : bossHealthMultiplier;
        if (!dataIsBoss)
            health.ConfigureAsBoss(bossHpMult);
        else
            health.ConfigureAsBoss(1f);

        ApplyLevelHealthScale(health);
    }

    private GameObject ResolveBossPrefab(GlobalEntities entities)
    {
        if (ActiveBossConfig?.bossPrefab != null)
            return ActiveBossConfig.bossPrefab;

        if (entities.BossPrefab != null)
            return entities.BossPrefab;

        foreach (var p in entities.EnemyPrefabs)
        {
            if (p == null) continue;
            var ai = p.GetComponentInChildren<BaseAIController>(true);
            if (ai != null && ai.IsBossEnemy)
                return p;
        }

        var list = entities.EnemyPrefabs;
        return list[Random.Range(0, list.Count)];
    }

    private async UniTask SpawnWave(int waveIndex, CancellationToken cancellationToken)
    {
        var config = ActiveWaveConfig;
        var totalWaves = config != null ? config.waveCount : waveCount;
        var perWave = config != null ? config.enemiesPerWave : enemiesPerWave;
        var spawnDelay = config != null ? config.spawnDelayMs : 500;
        var betweenWaves = config != null ? config.delayBetweenWavesMs : 500;
        var enterDelay = config != null ? config.roomEnterDelayMs : 500;

        if (waveIndex > totalWaves)
            return;

        await UniTask.Delay(enterDelay, cancellationToken: cancellationToken);
        for (int i = 0; i < perWave; i++)
        {
            await UniTask.Delay(spawnDelay, cancellationToken: cancellationToken);
            SpawnEnemy();
        }

        Debug.Log("[RoomController] wave " + waveIndex + " spawned");
        await UniTask.WaitUntil(
            () => GlobalEntities.Instance.AvailableEnemies.Count == 0,
            cancellationToken: cancellationToken);

        await UniTask.Delay(betweenWaves, cancellationToken: cancellationToken);
        if (waveIndex + 1 >= totalWaves)
        {
            OpenDoors();
            IsCleared = true;
            GlobalEvents.RaiseRoomCleared();
            return;
        }

        SpawnWave(waveIndex + 1, cancellationToken).Forget();
    }

    private void SpawnEnemy()
    {
        EnemySpawnEntry entry = ActiveWaveConfig?.PickRandomEntry();
        GameObject prefab = entry?.prefab;
        if (prefab == null)
        {
            var listPrefab = GlobalEntities.Instance.EnemyPrefabs;
            if (listPrefab == null || listPrefab.Count == 0) return;
            prefab = listPrefab[Random.Range(0, listPrefab.Count)];
        }

        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var e = SpawnEnemyInstance(prefab, spawnPoint.position, spawnPoint.rotation);
        ApplySpawnEntryScale(e.GetComponent<Health>(), entry);
        GlobalEntities.Instance.RegisterEnemy(e);
    }

    private void ApplyLevelHealthScale(Health health)
    {
        if (health == null || ActiveLevel == null)
            return;

        if (!Mathf.Approximately(ActiveLevel.enemyHealthScale, 1f))
            health.ApplyRuntimeHealthScale(ActiveLevel.enemyHealthScale);
    }

    private void ApplySpawnEntryScale(Health health, EnemySpawnEntry entry)
    {
        if (health == null || entry == null)
            return;

        var scale = entry.healthMultiplier;
        if (ActiveLevel != null)
            scale *= ActiveLevel.enemyHealthScale;

        health.ApplyRuntimeHealthScale(scale);
    }

    private static GameObject SpawnEnemyInstance(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        PoolId poolId = PoolId.None;
        if (prefab != null && prefab.TryGetComponent<PooledObject>(out var pooledObject))
            poolId = pooledObject.PoolId;

        if (poolId != PoolId.None && ObjectPoolingManager.Instance != null)
        {
            var fromPool = ObjectPoolingManager.Instance.Get(poolId, position, rotation);
            if (fromPool != null)
                return fromPool;
        }

        var instance = Object.Instantiate(prefab, position, rotation);
        ObjectPoolingManager.NotifySpawnedFromPool(instance);
        return instance;
    }

    private void CloseDoors()
    {
        foreach (var doorController in doorsController)
            doorController.SetClose();
    }

    private void OpenDoors()
    {
        foreach (var doorController in doorsController)
            doorController.SetOpen();
    }
}
