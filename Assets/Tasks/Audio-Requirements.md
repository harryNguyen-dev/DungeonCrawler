# DungeonCrawler — Danh sách âm thanh cần thiết

> Bạn tìm file âm thanh và đặt vào `Assets/ThirdPackage/Audio/` theo cấu trúc bên dưới.  
> Sau đó kéo thả vào `Assets/SO/Audio/GameAudioCatalog_Global.asset` (tạo từ menu **Create → Audio → Game Audio Catalog**).

## Cấu trúc thư mục đề xuất

```
Assets/ThirdPackage/Audio/
├── Music/          # Nhạc nền loop (lobby, battle, boss)
├── Ambient/        # Tiếng môi trường loop nhẹ (tùy chọn)
├── UI/             # Click, back, error, mua hàng, popup
├── Gameplay/
│   ├── Player/     # Tấn công, skill, dash, nhận damage, heal, level up
│   ├── Enemy/      # Chết, melee, ranged, charge
│   ├── Boss/       # Phase, đòn đặc biệt, intro
│   └── World/      # Mở cửa phòng, clear room, nhặt gold/exp
└── Stingers/       # Fanfare ngắn (win, lose) — không loop
```

## Quy ước đặt tên file

| Pattern | Ví dụ | Ghi chú |
|---------|-------|---------|
| `SFX_<Nhóm>_<Hành động>.wav` | `SFX_UI_Click_Confirm.wav` | SFX one-shot |
| `Music_<Scene>_<Mood>.wav` | `Music_Battle_Main.wav` | Loop, 1–3 phút |
| `Stinger_<Sự kiện>.wav` | `Stinger_Win.wav` | 2–8 giây, không loop |

**Format khuyến nghị:** WAV 44.1 kHz, mono cho SFX ngắn; stereo cho nhạc nền.

---

## P0 — Bắt buộc cho demo đồ án

### UI (tag `ui` trong `AudioManager`)

| ID | Tên file đề xuất | Khi nào phát | Đã có? |
|----|------------------|--------------|--------|
| `UI_Click_Confirm` | `UI/Click.wav` hoặc `SFX_UI_Click_Confirm.wav` | Nút xác nhận: Battle, Continue, chọn card, mua hero | ✅ `Click.wav`, `SFX/AudioClip_Click-Medium.wav` |
| `UI_Click_Back` | `UI/ClickBack.wav` | Back, Close, Exit hub, thoát shop/map select | ✅ `BackCancelSound.wav` |
| `UI_Click_Tab` | `SFX_UI_Click_Switch.wav` | Đổi tab shop (Characters / Frames / Icons) | ✅ `SFX/AudioClip_Click-Switch.wav` |
| `UI_Error` | `SFX_UI_Error.wav` | Không đủ gold, nút disabled, thao tác sai | ✅ `SFX/AudioClip_Error.wav` |
| `UI_Purchase` | `SFX_UI_Purchase.wav` | Mua hero thành công trong shop | ✅ `AudioClip_Purchase-Cha-Ching.wav` |

### Nhạc & môi trường (tag `ambient` / `master`)

| ID | Tên file đề xuất | Khi nào phát | Loop | Đã có? |
|----|------------------|--------------|------|--------|
| `Music_Lobby` | `Music/Music_Lobby.wav` | `GameManager.SetupLobbyScene` / `RaiseLobbyReady` | ✅ | ⚠️ dùng tạm `Ambient/at-the-park-afternoon- XtremeFreddy.wav` |
| `Music_Battle` | `Music/Music_Battle_Main.wav` | `RaiseGameStart` khi vào dungeon | ✅ | ✅ `Music/AudioClip_Music_MorganaRides.wav` |
| `Music_Boss` | `Music/Music_Boss.wav` | Vào phòng boss (`RoomType.Boss`) | ✅ | ⚠️ có thể dùng `Music/AudioClip_Music_Darkling.wav` |
| `Stinger_Win` | `Stingers/Stinger_Win.wav` | `RaiseBossDefeated` / màn win | ❌ | ✅ `Music/AudioClip_Fanfares_Victory.wav` |
| `Stinger_Lose` | `Stingers/Stinger_Lose.wav` | `RaisePlayerEliminated` / màn lose | ❌ | ✅ `Music/AudioClip_Fanfares_Defeat.wav` |
| `SFX_Loading` | `UI/Loading.wav` | `LoadingManager.Open` | ❌ | ✅ `Launch.wav` |

### Gameplay — Player (tag `vfx`)

| ID | Tên file đề xuất | Khi nào phát | Đã có? |
|----|------------------|--------------|--------|
| `Player_Attack` | `Gameplay/Player/SFX_Player_Attack.wav` | `Attack.PerformAttack` — bắn projectile | ❌ |
| `Player_Skill` | `Gameplay/Player/SFX_Player_Skill.wav` | `PlayerSkill` kích hoạt skill | ❌ |
| `Player_Dash` | `Gameplay/Player/SFX_Player_Dash.wav` | `PlayerDash` bắt đầu dash | ❌ |
| `Player_Hit` | `Gameplay/Player/SFX_Player_Hit.wav` | `Health.TakeDamage` (player) | ✅ `gameplay/AudioClip_generic-gethit.wav` |
| `Player_Death` | `Gameplay/Player/SFX_Player_Death.wav` | `Health.Eliminate` | ❌ |
| `Player_LevelUp` | `Gameplay/Player/SFX_Player_LevelUp.wav` | `RaiseLevelUp` | ❌ |
| `Player_Heal` | `Gameplay/Player/SFX_Player_Heal.wav` | Nhận heal từ card/skill | ❌ |

### Gameplay — Enemy & Combat (tag `vfx`)

| ID | Tên file đề xuất | Khi nào phát | Đã có? |
|----|------------------|--------------|--------|
| `Enemy_Hit` | `Gameplay/Enemy/SFX_Enemy_Hit.wav` | `EnemyController.Health.TakeDamage` | ⚠️ dùng chung `generic-gethit` |
| `Enemy_Death` | `Gameplay/Enemy/SFX_Enemy_Death.wav` | `RaiseEnemyDie` | ❌ |
| `Enemy_Melee_Swing` | `Gameplay/Enemy/SFX_Enemy_Melee_Swing.wav` | `EnemyMeleeAttack` | ❌ |
| `Enemy_Ranged_Shoot` | `Gameplay/Enemy/SFX_Enemy_Ranged_Shoot.wav` | `EnemyRangedAttack` / projectile spawn | ❌ |
| `Projectile_Hit` | `Gameplay/Enemy/SFX_Projectile_Hit.wav` | `EnemyProjectile` / `ProjectileController` trúng | ❌ |

### Gameplay — Dungeon & Meta (tag `vfx`)

| ID | Tên file đề xuất | Khi nào phát | Đã có? |
|----|------------------|--------------|--------|
| `Room_Enter` | `Gameplay/World/SFX_Room_Enter.wav` | Player bước vào phòng combat mới | ❌ |
| `Room_Clear` | `Gameplay/World/SFX_Room_Clear.wav` | `RaiseRoomCleared` | ❌ |
| `Door_Open` | `Gameplay/World/SFX_Door_Open.wav` | Cửa mở sau khi clear wave | ❌ |
| `Gold_Pickup` | `Gameplay/World/SFX_Gold_Pickup.wav` | `GameManager.CollectGold` | ❌ |
| `Card_Reveal` | `Gameplay/World/SFX_Card_Reveal.wav` | `RaiseRequestBattleCardUI` — hiện 3 card | ❌ |
| `Card_Select` | `Gameplay/World/SFX_Card_Select.wav` | `CardBattleUI.OnCardSelected` | ❌ |

---

## P1 — Nên có (polish demo)

| ID | Mô tả |
|----|-------|
| `Boss_Intro` | Tiếng gầm / alarm khi vào phòng boss |
| `Boss_Phase` | Chuyển phase boss (Stampede, Imp Mischief) |
| `Boss_Special` | Đòn đặc biệt (stampede, barrage projectile) |
| `UI_Pause_Open` | Mở menu pause (`SettingBattleUI.Show`) |
| `UI_Pause_Close` | Đóng pause / Continue |
| `Ambient_Dungeon` | Tiếng gió/humming nhẹ trong dungeon (volume thấp, loop) |
| `Player_Projectile_Whoosh` | Bay projectile (tùy weapon) |
| `Explosion` | Skill explosive / splash damage |

---

## P2 — Có thì tốt (cắt được)

- Voice line boss, footstep player/enemy  
- Nhạc riêng từng chapter  
- SFX khác nhau theo weapon/hero (Freeze, Pierce, Explosive…)  
- Ambient lobby theo giờ trong ngày  

---

## Volume tags (`AudioManager`)

| Tag | Dùng cho |
|-----|----------|
| `master` | Loading stinger, fanfare win/lose |
| `ui` | Toàn bộ click UI (thêm vào `customTagVolumes` trên `AudioManager` trong `Boost.unity`) |
| `vfx` | Combat, skill, pickup |
| `ambient` | Nhạc nền lobby / battle / boss |

Công thức: `volume = masterVolume × categoryVolume`.

---

## Điểm gắn code (khi đã có clip)

| Sự kiện | File / API |
|---------|------------|
| Lobby sẵn sàng | `GameManager.SetupLobbyScene` → `PlayAmbientAudio(catalog.musicLobby)` |
| Bắt đầu run | `GlobalEvents.OnGameStart` → đổi `musicBattle` |
| Vào boss room | `RoomController` khi `RoomType.Boss` → `musicBoss` |
| Player chết | `Health.Eliminate` → `stingerLose` + `StopAllAudioPlay` |
| Thắng boss | `HandleBossDefeated` → `stingerWin` |
| UI button | Helper `GameAudio.PlayUI(id)` gọi từ các `Button.onClick` |
| Loading | Đã có: `LoadingManager` → `loadingAudioClip` |

---

## Checklist khi thêm file mới

- [ ] Đặt đúng thư mục + tên theo bảng trên  
- [ ] Import Unity: **Load Type = Decompress On Load** (SFX ngắn) hoặc **Streaming** (nhạc dài)  
- [ ] Gán vào `GameAudioCatalog_Global.asset`  
- [ ] Test volume trong Settings (P1 roadmap: slider Master / SFX / Music)  
- [ ] Không vượt `maxConcurrentAudio` (20) khi spam combat  

---

## File hiện có trong project (tham chiếu nhanh)

| File hiện tại | Gán tạm cho ID |
|---------------|----------------|
| `Click.wav` | `UI_Click_Confirm` |
| `BackCancelSound.wav` | `UI_Click_Back` |
| `Launch.wav` | `SFX_Loading` |
| `SFX/AudioClip_Click-*.wav` | Biến thể UI click |
| `SFX/AudioClip_Error.wav` | `UI_Error` |
| `AudioClip_Purchase-Cha-Ching.wav` | `UI_Purchase` |
| `gameplay/AudioClip_generic-gethit.wav` | `Player_Hit` / `Enemy_Hit` |
| `Music/AudioClip_Music_MorganaRides.wav` | `Music_Battle` |
| `Music/AudioClip_Music_Darkling.wav` | `Music_Boss` |
| `Music/AudioClip_Fanfares_Victory.wav` | `Stinger_Win` |
| `Music/AudioClip_Fanfares_Defeat.wav` | `Stinger_Lose` |
| `Ambient/*.wav` | `Music_Lobby` hoặc `Ambient_Dungeon` |

**Còn thiếu P0:** `Player_Attack`, `Enemy_Death`, `Room_Clear`, `Card_Reveal/Select`, `Gold_Pickup`, `Door_Open`, `Player_Death`, `Player_LevelUp`.
