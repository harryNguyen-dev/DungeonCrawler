# DungeonCrawler — Roadmap tới tay người dùng

> Góc nhìn Tech Lead + PM · cập nhật theo trạng thái codebase hiện tại

## Lộ trình ngắn

```
Alpha nội bộ → Playtest (10–20 người) → Soft launch (1 nền tảng) → Store release
```

| Giai đoạn | Mục tiêu | Exit criteria |
|-----------|----------|---------------|
| **Alpha** | Core loop ổn, không crash trong 30 phút chơi | P0 gameplay + P0 kỹ thuật xong |
| **Beta / Soft launch** | Retention D1 ≥ 25%, funnel rõ | P0 + P1 monetization/analytics |
| **Release** | Store listing, marketing, support | P0 hoàn tất, P1 ≥ 80% |

---

## Hiện trạng (đã có)

- Boot → Lobby → chọn map/hero → dungeon WFC → wave room → boss → win/lose + sao
- Meta: unlock map theo sao, meta gold, mua/unlock hero (`LevelProgressService`)
- In-run: chọn card, skill hero, minimap, loading scene
- Chapter 1: ~10 level, nhiều enemy/boss SO

## Còn thiếu / placeholder

- Settings, pause menu, revive/ads — chưa implement
- Shop: Frames/Icons, mua gold — TODO
- Branding store (`DefaultCompany`), splash Unity bật
- Chưa thấy analytics, crash report, tutorial, localization

---

## P0 — Bắt buộc để ship bản chơi được

**Gameplay & loop**
- [ ] Verify end-to-end: Lobby → map → dungeon → win/lose → hub (≥ 3 map liên tiếp, không duplicate player/singleton)
- [ ] Boss win condition + star/unlock map hoạt động đúng trên toàn Chapter 1
- [ ] Card pick: pause game, chọn xong resume; không soft-lock UI
- [ ] Reset match sạch khi restart / về lobby (pool, enemy, projectile)

**UX tối thiểu**
- [ ] Settings: âm lượng SFX/BGM, chất lượng đồ họa (Low/Med/High)
- [ ] Pause in-run: resume / restart / về hub
- [ ] Màn hình kết quả: hiển thị rõ sao, gold nhận, map mở khóa
- [ ] First-run hint ngắn (1 lần): di chuyển, tấn công, chọn card

**Kỹ thuật & build**
- [ ] Build Settings ổn định: `Boost → Lobby → BattleScene`
- [ ] Target platform #1 (khuyến nghị Android hoặc PC) — profile build + icon/splash
- [ ] FPS ổn trên thiết bị mục tiêu (≥ 30 fps, không leak pool sau 5 run)
- [ ] Save: load fail → tạo save mới an toàn, không crash
- [ ] Smoke test checklist tự động hoặc QA script (15–20 case)

**Store / pháp lý tối thiểu**
- [ ] Tên app, icon, mô tả ngắn, screenshot (5+)
- [ ] Privacy policy (dù chỉ local save)
- [ ] Version + changelog nội bộ

---

## P1 — Cần cho launch tốt & đo lường

**Retention & học game**
- [ ] Tutorial có bước: lobby → chọn map → combat → card → boss
- [ ] Onboarding hero: giải thích meta gold / unlock hero
- [ ] Balance pass Chapter 1 (damage, wave count, gold gain, star threshold)

**Monetization (nếu F2P)**
- [ ] Revive sau thua (rewarded ad hoặc gold) — thay placeholder `WinLoseUI`
- [ ] Meta gold: IAP pack hoặc rewarded ad — thay TODO `MainMenuUI`
- [ ] Shop hero flow hoàn chỉnh (preview, confirm, insufficient gold)

**Vận hành**
- [ ] Analytics funnel: boot, start run, win/lose, quit, unlock map
- [ ] Crash/error reporting (Firebase, Sentry, Unity Cloud…)
- [ ] Remote config hoặc hotfix số cân bằng (JSON/SO patch)

**Chất lượng**
- [ ] Audio mix + SFX combat/UI đủ set
- [ ] Polish UI lobby/battle (loading, empty state, feedback nút bấm)
- [ ] Credit panel nội dung đầy đủ

---

## P2 — Sau launch / mở rộng

**Nội dung**
- [ ] Chapter 2+ (catalog, boss, wave config mới)
- [ ] Thêm card/hero/skill — mở rộng build variety
- [ ] Shop Frames/Icons (cosmetic)

**Platform & scale**
- [ ] Port nền tảng thứ 2 (iOS / Steam / WebGL)
- [ ] Cloud save / đăng nhập tài khoản
- [ ] Localization (EN + VI tối thiểu)

**Live ops**
- [ ] Daily/weekly quest, battle pass
- [ ] Leaderboard, achievement
- [ ] Sự kiện theo mùa, banner in-game

**Tech debt**
- [ ] CI build tự động (GitHub Actions + Unity)
- [ ] Automated playmode tests cho core loop
- [ ] Addressables / bundle cho cập nhật nội dung nhẹ

---

## Ưu tiên 4 tuần đầu (gợi ý)

| Tuần | Focus |
|------|--------|
| 1 | P0 gameplay verify + bug critical |
| 2 | Settings + Pause + first-run hint |
| 3 | Build target #1 + QA smoke + store assets |
| 4 | Playtest nội bộ → fix → soft launch |

---

## KPI theo dõi sau khi có user

- Crash-free sessions ≥ 99%
- Tutorial completion ≥ 70%
- D1 retention ≥ 25% (soft launch)
- Avg session ≥ 8 phút
- Win rate map 1–3: 40–60% (điều chỉnh balance)
