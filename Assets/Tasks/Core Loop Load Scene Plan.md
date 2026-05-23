# Core Loop Load/Reload/Return Lobby Plan

## Goal
Hoan thien loop chinh:

`Lobby -> spawn player tai (0,0,0), chi di chuyen -> Portal -> BattleScene -> generate dungeon -> spawn player tai Start Room -> play match -> win/lose/restart/return Lobby -> reset in-match data`

## Implementation Checklist
- Scene lifecycle
  - `SceneManagerCustom` co API ro rang: `LoadLobby()`, `LoadDungeon()`, `ReloadDungeon()`.
  - `SceneLoadNotifier` xu ly scene hien tai luc Start va scene moi qua `SceneManager.sceneLoaded`.
  - Khong dung delay co dinh de setup scene.

- Persistent systems
  - `GameManager`, `GlobalEntities`, `InputManager`, `SceneLoadNotifier` tiep tuc la `DontDestroyOnLoad`.
  - `GlobalEntities` clear player, enemy, projectile runtime khi scene moi load.
  - Camera duoc bind lai moi lan spawn player.

- Lobby flow
  - `SetupLobbyScene()` reset match state.
  - Set spawn position ve `Vector3.zero`.
  - Spawn player lobby voi attack disabled.
  - `PortalController` goi `SceneManagerCustom.LoadDungeon()`.

- Dungeon flow
  - `SetupBattleScene()` reset match state, set game mode `InMatch`, raise dungeon scene loaded, roi raise game start.
  - `WFCGeneration` generate dungeon khi nhan `OnGameStart`.
  - Sau khi dungeon generate thanh cong, `OnDungeonGenerated(seed)` cap nhat room count va spawn player tai Start Room.
  - Player trong dungeon duoc enable attack.

- Return/restart flow
  - `EndGameUI.RestartGame()` chi goi `SceneManagerCustom.ReloadDungeon()`.
  - `EndGameUI.ReturnHome()` chi goi `SceneManagerCustom.LoadLobby()`.
  - Reset du lieu in-match tap trung trong `GameManager.ResetMatchState()`.

## Public Interfaces
- `Global.GameMode`
  - `Lobby`
  - `InMatch`
- `GlobalVariable.CurrentGameMode`
- `GlobalEvents`
  - `OnLobbyReady`
  - `OnDungeonSceneLoaded`
  - `OnMatchReset`
- `PlayerController.Attack.SetAttackEnabled(bool enabled)`
- `GlobalEntities.SpawnPlayer(bool canAttack = true)`

## Verify
- Play tu `Lobby`: player spawn tai `(0,0,0)` va khong attack duoc.
- Vao Portal: load `BattleScene`, dungeon generate xong moi spawn player tai Start Room.
- Trong dungeon: camera follow player moi va attack hoat dong.
- Win/lose: UI hien dung.
- Restart: reload dungeon sach, khong giu player/enemy/projectile cu.
- Return Home: load `Lobby`, reset match, spawn lobby player, attack disabled.
- Lap lai `Lobby -> Dungeon -> Lobby` nhieu lan khong tao duplicate singleton/player.

## Notes
- Core scenes hien dung: `Lobby` va `BattleScene`.
- Reset in-match khong dung den meta progression/save data dai han.
