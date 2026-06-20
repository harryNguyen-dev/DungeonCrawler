# Audio assets — DungeonCrawler

Danh sách đầy đủ âm thanh cần thiết: **[Assets/Tasks/Audio-Requirements.md](../../Tasks/Audio-Requirements.md)**

## Thư mục

| Folder | Nội dung |
|--------|----------|
| `Music/` | Nhạc nền loop |
| `Ambient/` | Ambient loop (lobby, dungeon) |
| `UI/` | Click, back, error, loading |
| `Gameplay/Player/` | Attack, skill, hit, death |
| `Gameplay/Enemy/` | Hit, death, swing, shoot |
| `Gameplay/Boss/` | Intro, phase, special |
| `Gameplay/World/` | Room, door, gold, card |
| `Stingers/` | Win / lose fanfare |
| `SFX/` | Legacy — file cũ, có thể di chuyển dần |

## Sau khi thêm file

1. Gán clip vào `Assets/SO/Audio/GameAudioCatalog_Global.asset` (Create → Audio → Game Audio Catalog).
2. Wire catalog vào `AudioManager` / listener (bước code tiếp theo).
