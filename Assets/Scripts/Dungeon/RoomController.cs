using UnityEngine;

using Cysharp.Threading.Tasks;

using System.Threading;

using Core;

using Core.Minimap;

using EnemyController;

using Global;

using SO;

public enum RoomType

{

    Combat = 0,

    Start = 1,

    Boss = 2,

    Hallway = 3

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

    private Vector2Int gridPosition;

    private WaveConfigSO _roomWaveConfig;

    private int _seedSalt;



    private BossConfigSO ActiveBossConfig => GlobalVariable.CurrentLevel?.boss;

    private LevelSO ActiveLevel => GlobalVariable.CurrentLevel;



    private void Start()

    {

        var parent = gameObject.transform.parent;

        if (parent != null)

            doorsController = parent.gameObject.GetComponentsInChildren<DoorController>();

        else

            doorsController = System.Array.Empty<DoorController>();

    }



    public void SetSpawnPoints(Transform[] points)

    {

        spawnPoints = points;

    }



    public void SetSeedSalt(int seedSalt)

    {

        _seedSalt = seedSalt;

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



    public RoomType GetRoomType() => roomType;



    public void SetGridPosition(Vector2Int pos) => gridPosition = pos;

    public Vector2Int GridPosition => gridPosition;

    public bool TryGetMinimapZoneCollider(out Collider collider)
    {
        collider = GetComponent<Collider>();
        if (collider == null)
            collider = GetComponentInChildren<Collider>();
        return collider != null;
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

        var isBossRoom = false;
        if (roomType == RoomType.Combat)
            isBossRoom = DungeonEncounterTracker.RegisterCombatRoomEntered();

        GlobalEvents.RaiseRoomEntered(new RoomEnteredInfo
        {
            GridPosition = gridPosition,
            RoomType = roomType,
            IsBossRoom = isBossRoom
        });

        var ct = this.GetCancellationTokenOnDestroy();



        if (roomType == RoomType.Hallway)

        {

            HandleHallwayEnter(ct);

            return;

        }



        if (roomType != RoomType.Combat)

            return;



        CloseDoors();

        if (isBossRoom)

        {

            Debug.Log("[RoomController] final combat room -> boss encounter");

            SpawnBossEncounter(ct).Forget();

            return;

        }



        _roomWaveConfig = ResolveCombatWaveConfig();

        SpawnWave(0, ct).Forget();

    }



    private void HandleHallwayEnter(CancellationToken ct)

    {

        var chance = ActiveLevel != null ? ActiveLevel.hallwaySpawnChance : 0.4f;

        var seed = GlobalVariable.CurrentSeed ^ _seedSalt;

        var roll = new System.Random(seed).NextDouble();



        if (roll >= chance)

        {

            Debug.Log("[RoomController] hallway -> no encounter");

            return;

        }



        _roomWaveConfig = ResolveHallwayWaveConfig();

        CloseDoors();

        SpawnWave(0, ct).Forget();

    }



    private WaveConfigSO ResolveCombatWaveConfig() =>
        ActiveLevel?.PickCombatWave(_seedSalt);

    private WaveConfigSO ResolveHallwayWaveConfig() =>
        ActiveLevel?.PickHallwayWave(_seedSalt);



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

        GlobalEvents.RaiseRoomCleared(gridPosition);

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

        var config = _roomWaveConfig;

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

            GlobalEvents.RaiseRoomCleared(gridPosition);

            return;

        }



        SpawnWave(waveIndex + 1, cancellationToken).Forget();

    }



    private void SpawnEnemy()

    {

        EnemySpawnEntry entry = _roomWaveConfig?.PickRandomEntry();

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



        if (poolId != PoolId.None && EnemyPool.Instance != null)

        {

            var fromPool = EnemyPool.Instance.Get(poolId, position, rotation);

            if (fromPool != null)

                return fromPool;

        }



        var instance = Object.Instantiate(prefab, position, rotation);

        ObjectPoolBase.NotifySpawnedFromPool(instance);

        return instance;

    }



    private void CloseDoors()

    {

        if (doorsController == null) return;

        if (TryGetMinimapZoneCollider(out Collider collider))

            CombatRoomBoundary.Seal(MinimapZoneBounds.FromCollider(collider, gridPosition));

        foreach (var doorController in doorsController)

            doorController.SetClose();

    }



    private void OpenDoors()

    {

        if (doorsController == null) return;

        CombatRoomBoundary.Unseal();

        Core.GameAudio.PlayDoorOpen();

        foreach (var doorController in doorsController)

            doorController.SetOpen();

    }

}

