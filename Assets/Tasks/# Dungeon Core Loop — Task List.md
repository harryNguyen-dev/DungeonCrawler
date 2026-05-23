# Dungeon Core Loop — Task List

## Mục tiêu
Start → Generate → Spawn player tại start room → Enter room → Spawn wave → Clear → Mở cửa → Clear hết → Win

---

## Tasks

### [ ] 1. `GlobalEvents.cs`
Thêm 2 events:
```csharp
public static event Action OnRoomCleared;
public static event Action OnAllRoomsCleared;
public static void RaiseRoomCleared() => OnRoomCleared?.Invoke();
public static void RaiseAllRoomsCleared() => OnAllRoomsCleared?.Invoke();
```

---

### [ ] 2. `GlobalVariable.cs`
Thêm 1 property:
```csharp
public static int TotalRoomCount { get; set; }
```

---

### [ ] 3. `WFCGeneration.cs`
Sau khi set `PlayerSpawnPosition`, trước `RaiseDungeonGenerated`:
```csharp
GlobalVariable.TotalRoomCount = placedRooms.Count - 1; // trừ start room
```

---

### [ ] 4. `GameManager.cs`
Subscribe events, track tiến độ clear room:

```csharp
// OnEnable
GlobalEvents.OnRoomCleared += HandleRoomCleared;
GlobalEvents.OnDungeonGenerated += HandleDungeonGenerated;

// OnDisable — unsubscribe tương tự

void HandleDungeonGenerated(int seed) {
    _totalRooms = GlobalVariable.TotalRoomCount;
    _clearedRooms = 0;
    isGameOver = false;
}

void HandleRoomCleared() {
    if (++_clearedRooms >= _totalRooms) HandleWin();
}

void HandleWin() {
    if (isGameOver) return;
    isGameOver = true;
    Time.timeScale = 0f;
    GlobalEvents.RaiseAllRoomsCleared();
}
```

---

### [ ] 5. `RoomController.cs`
Implement wave system hoàn chỉnh. Thêm serialized fields:
```csharp
[SerializeField] private List<GameObject> enemyPrefabs;
[SerializeField] private int waveCount = 1;
[SerializeField] private int enemiesPerWave = 3;
```

Flow:
```
OnTriggerEnter(Player)
  → isStartRoom || isPlayerReached → return
  → isPlayerReached = true
  → CloseDoor()
  → StartCoroutine(SpawnWave(1))

SpawnWave(waveIndex):
  → Delay 0.5s
  → Spawn enemiesPerWave kẻ địch tại random spawnPoints
  → livingEnemies = enemiesPerWave
  → Subscribe GlobalEvents.OnEnemyDie += HandleEnemyDie

HandleEnemyDie():
  → livingEnemies--
  → if > 0: return
  → Unsubscribe OnEnemyDie
  → if currentWave < waveCount:
        currentWave++; StartCoroutine(SpawnWave) // delay 1.5s
    else:
        ClearRoom()

ClearRoom():
  → isCleared = true
  → OpenDoor() (foreach doorController.SetOpen())
  → GlobalEvents.RaiseRoomCleared()
```

---

### [ ] 6. `EndGameUI.cs`
Tách win/lose, fix scene load:

```csharp
// Subscribe thêm
GlobalEvents.OnAllRoomsCleared += ShowWinPanel;
GlobalEvents.OnPlayerEliminated += ShowLosePanel;

void ShowWinPanel()  { endGamePanel.SetActive(true); Title.text = "YOU WIN!"; }
void ShowLosePanel() { endGamePanel.SetActive(true); Title.text = "YOU LOSE"; }

void ReturnHome()  { Time.timeScale = 1f; SceneManager.LoadScene("Main"); }
void RestartGame() { Time.timeScale = 1f; SceneManager.LoadScene("BattleScene"); }
// Xóa mọi lời gọi SpawnEnemyManager.ResetGame()
```

---

### [ ] 7. Scene — Disable `SpawnEnemyWaveManager`
Disable (không xóa script) GameObject chứa `SpawnEnemyWaveManager` trong `BattleScene`.

---

## Thứ tự thực hiện
1 → 2 → 3 → 4 → 6 → 7 → **5** (làm RoomController cuối để test end-to-end ngay)

---

## Checklist verify
- [ ] Enter start room → **không** spawn quái
- [ ] Enter room khác → cửa đóng → quái spawn
- [ ] Kill hết wave 1 → (nếu waveCount=2) delay 1.5s → spawn wave 2
- [ ] Kill hết wave cuối → cửa mở
- [ ] Clear hết tất cả room → màn hình **YOU WIN!**
- [ ] Click Return Home → load scene `Main`
- [ ] Player chết → màn hình **YOU LOSE** như cũ

---

## Pitfalls cần chú ý
- `OnEnemyDie` phải đã tồn tại trong `GlobalEvents` trước khi làm Step 5
- Tên method của `DoorController` (`SetOpen`?) — verify trước khi gọi
- `HandleDungeonGenerated` signature phải match delegate (có nhận `int seed` không?)
- `enemyPrefabs` list trong Inspector không được để trống