# Balance — ScriptableObject Stats (GD Reference)

> Snapshot từ SO assets + logic runtime trong project `DungeonCrawler`.  
> Heroes: **Finn**, **Otha**, **Luna** · Enemies: 4 minion + 2 boss · Maps: 10 · Cards: 12.

---

## 0. Hằng số hệ thống (code)

| Hằng số | Giá trị | Ghi chú |
|---|---|---|
| Crit damage multiplier | **×2** | `HeroSO.CritDamageMultiplier` |
| Crit chance cap | **0–1** | `HeroSO.MaxCritChance` |
| Attack cooldown min | **0.15s** | sau mọi buff/card |
| Move speed default | **10** | trên mọi hero hiện tại |
| Level-up EXP ban đầu | **100** | mỗi level × **1.2** |
| Card pick mỗi level-up | **3 card** | weighted random |
| Dash | dùng chung | `DashConfig_Default` |

**File SO schema:** `Assets/SO/Hero/HeroSO.cs`, `HeroSkillSO.cs`, `DashConfigSO.cs`, `EnemySO.cs`, `CardSO.cs`, `LevelSO.cs`, `WaveConfigSO.cs`, `BossConfigSO.cs`, `WeaponSO.cs`

---

## 1. Hero

### Schema `HeroSO`

| Field | Ý nghĩa |
|---|---|
| maxHealth | HP tối đa |
| moveSpeed | tốc chạy |
| attackDamage | sát thương đánh thường (flat) |
| attackCooldown | thời gian giữa các đòn (giây) |
| critChance | 0–1 |
| unlockCost | giá mua hero (meta gold) |
| unlockedByDefault | mở sẵn hay không |

**Meta upgrade** (`HeroUpgradeStep`): `cost`, `damageBonus`, `healthBonus`, `cooldownReduction`, `critChanceBonus`

---

### 1.1 Finn — `Hero_Finn.asset` (starter, free)

| Stat | Giá trị |
|---|---|
| heroId | `hero_starter` |
| maxHealth | **250** |
| moveSpeed | **10** |
| attackDamage | **15** |
| attackCooldown | **0.4** |
| critChance | **0.5** |
| unlockCost | **0** |

**Gameplay:** bắn thường = mũi tên · skill = 8 mũi tên quạt phía trước.

**Meta upgrade (2 tier, custom):**

| Tier | Cost | +DMG | +HP | −CD | +Crit |
|---|---|---|---|---|---|
| 1 | 150 | 15 | 50 | 0.1 | 0.1 |
| 2 | 150 | 15 | 50 | 0.1 | 0.1 |
| **Tổng max** | 300 | +30 | +100 | −0.2 | +0.2 |

**Đánh thường** — `Weapon_Starter` (Pulse Rifle):

| Effect | Value |
|---|---|
| NumberOfProjectiles | 1 |

**Skill** — `Skill_Starter` (Pulse Burst):

| Field | Giá trị | Ghi chú |
|---|---|---|
| deliveryType | Projectile *(enum 0)* | Cone config có 8 viên nhưng SO chưa set `Cone`; `skillProjectilePrefab = null` |
| cooldown | 3s | |
| damageMode | RollAttackDamage | roll crit như đánh thường |
| damagePercent | 0.45 | mỗi viên = roll dmg × 0.45 |
| coneConfig.projectileCount | 8 | |
| coneConfig.coneAngle | 50° | |
| centerPelletPierce | true | viên giữa xuyên +1 |
| projectileSpeed | 20 | |
| range | 12 | |

---

### 1.2 Otha — `Hero_OTha.asset` (mua 250 gold)

| Stat | Giá trị |
|---|---|
| heroId | `hero_otha` |
| maxHealth | **150** |
| moveSpeed | **10** |
| attackDamage | **30** |
| attackCooldown | **0.45** |
| critChance | **0.12** |
| unlockCost | **250** |

**Gameplay:** bắn thường = đạn · skill = buff bản thân tăng sức mạnh.

**Meta upgrade (1 tier):**

| Cost | +DMG | +HP | −CD | +Crit |
|---|---|---|---|---|
| 350 | 30 | 50 | 0.3 | 0 |

**Đánh thường** — `Weapon_Otha` (Pyro Cannon):

| Effect | Value | Runtime |
|---|---|---|
| NumberOfProjectiles | 5 | 5 đạn/volley |
| FireDamage | 8 | DOT: 3 tick × 8 dmg, mỗi tick cách 1s |

**Skill** — `Skill_Otha_Overdrive` (Inferno Overdrive):

| Field | Giá trị |
|---|---|
| deliveryType | SelfBuff |
| cooldown | 10s |
| duration | 5s (refreshOnReuse) |
| AttackCooldownFlat | −0.17 |
| AttackDamagePercent | +40% (0.4) |
| FireDamageFlat | +4 |

---

### 1.3 Luna — `Hero_Luna.asset` (mua 250 gold)

| Stat | Giá trị |
|---|---|
| heroId | `hero_luna` |
| maxHealth | **150** |
| moveSpeed | **10** |
| attackDamage | **24** |
| attackCooldown | **0.5** |
| critChance | **0.1** |
| unlockCost | **250** |

**Gameplay:** bắn thường = đạn · skill = buff để đạn thường đóng băng kẻ địch.

**Meta upgrade:** `upgrades: []` → dùng **default 11 tier** trong `HeroSO.DefaultUpgrades`:

| # | Cost | Bonus |
|---|---|---|
| 1–3 | 75 / 120 / 180 | +5 dmg mỗi tier |
| 4–5 | 80 / 140 | −0.05 cd mỗi tier |
| 6–8 | 70 / 110 / 160 | +15 hp mỗi tier |
| 9–11 | 85 / 130 / 190 | +0.02 crit mỗi tier |
| **Tổng** | **1340** | **+15 dmg, +45 hp, −0.10 cd, +0.06 crit** |

**Đánh thường** — `Weapon_Luna` (Moonbow):

| Effect | Value |
|---|---|
| NumberOfProjectiles | 1 |

**Skill** — `Skill_Luna_FrostVeil` (Frost Veil):

| Field | Giá trị |
|---|---|
| deliveryType | SelfBuff |
| cooldown | 8s |
| duration | 6s (refreshOnReuse) |
| FrozenDurationFlat | +2.5s |

---

### 1.4 Dash dùng chung — `DashConfig_Default`

| Field | Giá trị |
|---|---|
| distance | 4 |
| duration | 0.2s |
| cooldown | 2s |
| iFrameDuration | 0.18s |

---

### 1.5 Hero Skills — tóm tắt

Mỗi hero: **dash (chung)** + **đánh thường (weapon)** + **1 skill chủ động (HeroSkillSO)**.

| Hero | Đánh thường | Skill | CD skill | Dash CD |
|---|---|---|---|---|
| **Finn** | 1 mũi tên | Pulse Burst — 8 viên quạt 50° | 3s | 2s |
| **Otha** | 5 đạn + fire DOT (3×8) | Inferno Overdrive — buff dmg/as/fire | 10s | 2s |
| **Luna** | 1 đạn | Frost Veil — đạn thường đóng băng 2.5s | 8s | 2s |

#### Bảng chi tiết skill (`HeroSkillSO`)

| Hero | Skill ID | Tên | Type | CD | Damage mode | Dmg / % | Hiệu ứng chính |
|---|---|---|---|---:|---|---|---|
| Finn | `skill_starter` | Pulse Burst | Cone *(SO ghi Projectile)* | 3s | RollAttackDamage | ×0.45 mỗi viên | 8 viên, góc 50°, viên giữa pierce +1, speed 20, range 12 |
| Otha | `skill_otha_overdrive` | Inferno Overdrive | SelfBuff | 10s | — | — | 5s buff: −0.17 cd, +40% dmg, +4 fire dmg |
| Luna | `skill_luna_frost_veil` | Frost Veil | SelfBuff | 8s | — | — | 6s buff: +2.5s freeze trên đạn thường |

#### Bảng chi tiết đánh thường (`WeaponSO.intrinsicEffects`)

| Hero | Weapon | Projectiles | Effect khác |
|---|---|---:|---|
| Finn | Pulse Rifle | 1 | — |
| Otha | Pyro Cannon | 5 | FireDamage 8 (DOT 3 tick/s) |
| Luna | Moonbow | 1 | — *(freeze chỉ khi buff skill active)* |

> Skill Finn: cần fix SO (`deliveryType = Cone` + gán `skillProjectilePrefab`) để khớp design và chạy in-game.

---

## 2. Enemy

### Schema `EnemySO`

`MaxHealth`, `MoveSpeed`, `Damage`, `AttackRange`, `AttackCooldown`, `KnockbackForce`, `KnockbackDuration`, `GoldDrop`, `ExpDrop` (default **20**), `isBoss`

### 2.1 Minion (4 loại dùng trong design)

| Enemy | Type | HP | Speed | Dmg | Range | CD | KB Force | Gold | Exp |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Cyclops Bat | Range | 60 | 4 | 20 | 12 | 1.0 | 5 | 5 | 20 |
| Imp Mischief | Melee | 100 | 6 | 30 | 2 | 1.5 | 3 | 5 | 20 |
| Imp Mischief Ranger | Range | 60 | 3 | 20 | 30 | 1.5 | 5 | 5 | 20 |
| Treant Minion Evergreen | Melee (tank) | 150 | 5 | 2 | 2 | 1.5 | 3 | 5 | 20 |
| Worm Baby | Charge/Melee | 50 | 5 | 20 | 12 | 1.0 | 6 | 5 | 20 |

> **Cyclops Bat** có SO + prefab nhưng **chưa nằm trong wave pool** combat/hallway hiện tại.

### 2.2 Boss

| Boss | HP | Speed | Dmg | Range | CD | isBoss | Gold | Exp |
|---|---:|---:|---:|---:|---:|---|---:|---:|
| Worm Junior | 800 | 3 | 50 | 12 | 1.0 | ✓ | 5 | 20 |
| Imp Mischief Junior | 700 | 3.5 | 35 | 14 | 1.2 | ✓ | 15 | 80 |

### Công thức HP runtime

- **Minion:** `EnemySO.MaxHealth × spawnEntry.healthMultiplier × level.enemyHealthScale`
- **Boss** (prefab có `isBoss=true`): `EnemySO.MaxHealth × level.enemyHealthScale`  
  *( `BossConfig.healthMultiplier` không nhân thêm )*

**Ví dụ map 10** (enemyHealthScale = 1.72):

- Worm Junior: 800 × 1.72 ≈ **1376 HP**
- Imp Junior: 700 × 1.72 ≈ **1204 HP**

> Kỹ năng / pattern boss: xem **§7 Boss Abilities** (config trên prefab, không có SO riêng).

---

## 3. Map — 10 level (Chapter 1)

### Schema `LevelSO`

| Field | Ý nghĩa |
|---|---|
| roomsToPlaceOverride | số phòng dungeon |
| enemyHealthScale | nhân HP toàn map |
| enemyDamageScale | nhân dmg toàn map |
| hallwaySpawnChance | xác suất spawn hallway |
| oneStarMinRooms / twoStarRoomRatio | điều kiện sao |
| unlockNextAtStars | sao tối thiểu mở map kế (default 2) |

| Map | Rooms | HP Scale | Dmg Scale | Hallway % | Boss |
|---:|---:|---:|---:|---:|---|
| 1.1 | 5 | 1.00 | 1 | 0.40 | Imp Mischief Junior |
| 1.2 | 5 | 1.08 | 1 | 0.50 | Worm Junior |
| 1.3 | 5 | 1.16 | 1 | 0.40 | *(ref broken)* |
| 1.4 | 6 | 1.24 | 1 | 0.40 | *(ref broken)* |
| 1.5 | 6 | 1.32 | 1 | 0.40 | *(ref broken)* |
| 1.6 | 6 | 1.40 | 1 | 0.40 | *(ref broken)* |
| 1.7 | 7 | 1.48 | 1 | 0.40 | *(ref broken)* |
| 1.8 | 7 | 1.56 | 1 | 0.40 | *(ref broken)* |
| 1.9 | 7 | 1.64 | 1 | 0.40 | *(ref broken)* |
| 1.10 | 7 | 1.72 | 1 | 0.40 | *(ref broken)* |

> Map 3–10 trỏ GUID boss **missing** — cần fix trước khi balance late game.

### Wave pool combat

| Wave config | Waves × enemies/wave | Enemy pool |
|---|---|---|
| basic_1 | 2 × 7 | Imp Mischief (w50) + Imp Ranger (w50, hp×1.2, dmg×0.9) |
| basic_2 | 2 × 10 | Imp Mischief only |
| basic_3 | 2 × 10 | Treant + Worm Baby — **chưa gán level pool** |
| basic_4 | 2 × 10 | Imp Mischief only — **chưa gán level pool** |

### Hallway waves

| Config | Waves × count | Pool |
|---|---|---|
| Hallway_1 | 1 × 2 | Imp + Imp Ranger |
| Hallway_2 | 1 × 3 | Imp only |

**Timing:** spawnDelay 500ms · giữa wave 500ms · vào phòng 500ms (hallway enter 300ms)

---

## 4. Card in-run (12 card)

Mỗi card: `CardID`, `CardName`, `Effect`, `CardTier`, `CardTierWeight`, `Value`

**Tier weight:** Common 60 · Rare 30 · Epic 15 · Legendary 5

| ID | Tên | Tier | Weight | Effect | Value | Áp dụng |
|---|---|---|---|---|---:|---|
| C01 | Sharp Edge | Common | 60 | IncreaseDamage | +15 | +attackDamage |
| C02 | Juggernaut | Common | 60 | IncreaseMaxHealth | +25 | +maxHP |
| C03 | Rations | Common | 60 | HealHealth | +30 | heal ngay (× heal mult) |
| C04 | March | Common | 60 | IncreaseRunSpeed | 0.15 | +moveSpeed *(RoundToInt → +0 — bug)* |
| R01 | Flurry | Rare | 30 | IncreaseAttackSpeed | 0.08 | −attackCooldown |
| R02 | Ironclad | Rare | 30 | IncreaseAmor | +5 | +Amor *(chưa giảm dmg nhận)* |
| R03 | Rejuvenate | Rare | 30 | InceaseHealSpeed | +0.3 | +DefaultHealMultiplier |
| R04 | Scholar | Rare | 30 | IncreaseExpGain | +0.2 | +DefaultExpGainMultiplier |
| R05 | Greed | Rare | 30 | IncreaseGoldGain | +0.25 | +DefaultGoldGainMultiplier |
| E01 | Spiked Armor | Epic | 15 | ThornArmor | 0.3 | reflect 30% dmg |
| L01 | Twin Split | Legendary | 5 | AddOneProjectile | +1 | +1 projectile/volley |
| L02 | Boomerang | Legendary | 5 | ProjectileBoomerang | 1 | bật boomerang mode |

**Player runtime stats card có thể chỉnh:**

AttackDamage, AttackCooldown, MaxHealth, MoveSpeed, CritChance, Amor, ThornReflectPercent, DefaultExpGainMultiplier, DefaultGoldGainMultiplier, DefaultHealMultiplier, weapon effects (NumberOfProjectiles, PierceCount, FireDamage, FrozenDuration, ExplosiveRadius, BoomerangMode)

---

## 5. Enum reference

### SkillDeliveryType

`Projectile` · `Cone` · `GroundAoE` · `Beam` · `SelfBuff`

### SkillDamageMode

`Fixed` · `PercentOfAttack` · `RollAttackDamage`

### StatModifierType (skill buff)

| Enum | Ý nghĩa |
|---|---|
| AttackDamageFlat | +dmg flat |
| AttackDamagePercent | +% dmg (tính trên atk hiện tại) |
| AttackCooldownFlat | −/+ cooldown |
| MoveSpeedFlat | +speed |
| CritChanceFlat | +crit |
| FireDamageFlat | +fire DOT trên weapon |
| ProjectileCountFlat | +projectile |
| FrozenDurationFlat | +thời gian đóng băng |

### WeaponEffectType

`NumberOfProjectiles` · `PierceCount` · `FireDamage` · `FrozenDuration` · `ExplosiveRadius` · `BoomerangMode`

### CardEffect

`IncreaseDamage` · `IncreaseAttackSpeed` · `IncreaseMaxHealth` · `HealHealth` · `IncreaseAmor` · `ThornArmor` · `IncreaseRunSpeed` · `InceaseHealSpeed` · `IncreaseExpGain` · `IncreaseGoldGain` · `AddOneProjectile` · `ProjectileBoomerang`

---

## 7. Boss Abilities

> Nguồn: `[SerializeField]` trên prefab + controller scripts. **Không có BossSkillSO** — dmg đạn/charge lấy từ `EnemySO.Damage`; sau mỗi pattern chờ `EnemySO.AttackCooldown`.

### 7.1 Worm Junior — `WormJunior.prefab`

**Script:** `StampedeBossController` + `StampedeBossProjectileAttack` + `EnemyChargeAttack`

| Stat base (EnemySO) | Giá trị |
|---|---|
| Damage (charge + đạn) | 50 |
| AttackRange | 12 |
| AttackCooldown | 1.0s |

#### Phase

| Phase | Điều kiện | Hành vi |
|---|---|---|
| **Phase 1** | HP > 50% | **Charge** về phía player (trong AttackRange) |
| **Phase 2** | HP ≤ 50% | **Ranged** — luân phiên 3 pattern đạn; lùi nếu player quá gần (< 55% AttackRange) |

#### Phase 1 — Charge (`EnemyChargeAttack` trên prefab)

| Param | Giá trị |
|---|---|
| chargeSpeed | 30 |
| chargeDuration | 0.4s |
| windUpDuration | 0.5s |
| recoveryDuration | 0.3s |
| hitRadius | 1.11 |

#### Phase 2 — Projectile patterns (luân phiên 0→1→2)

| Pattern | Mô tả | Param (prefab) |
|---|---|---|
| **TripleSpread** | 3 tia ngắm player (giữa ±22.5°) | spreadHalfAngle **22.5°**, windUp **300ms** |
| **RapidFive** | 5 viên liên tiếp ngắm player | count **5**, interval **120ms**, windUp **250ms** |
| **CircleDoubleWave** | 2 vòng tròn 360° (vòng 2 lệch 15°) | bullets/wave **12**, offset **15°**, delay giữa wave **450ms**, windUp **400ms** |

---

### 7.2 Imp Mischief Junior — `ImpMischiefJunior.prefab`

**Script:** `ImpMischiefJuniorBossController` + `ImpMischiefJuniorBossProjectileAttack`

| Stat base (EnemySO) | Giá trị |
|---|---|
| Damage (đạn) | 35 |
| AttackRange | 14 |
| AttackCooldown | 1.2s |

#### Phase

| Phase | Điều kiện | Hành vi |
|---|---|---|
| **Phase 1** | HP > 55% | Projectile patterns + summon |
| **Phase 2** | HP ≤ 55% | Giống phase 1; summon **2** minion/lần (thay vì 1) |

**AI ranged:** giữ khoảng cách an toàn ≥ **55%** AttackRange; lùi nếu player quá gần.

#### Pattern cycle (luân phiên 5 kiểu, lặp vô hạn)

| # | Pattern | Mô tả | Param (prefab) |
|---|---|---|---|
| 0 | **AimedSpread** | 1 tia giữa + 2 cặp hai bên (tổng **5** viên) | spreadPairCount **2**, góc bước **18°**, windUp **350ms** |
| 1 | **RapidBurst** | 6 viên liên tiếp ngắm player | count **6**, interval **100ms**, windUp **250ms** |
| 2 | **CircleBurst** | 10 viên vòng tròn 360° (heavy prefab nếu có) | count **10**, windUp **400ms** |
| 3 | **SpiralVolley** | 8 viên xoắn ốc | count **8**, góc bước **22.5°**, interval **90ms**, windUp **300ms** |
| 4 | **SummonMinions** | Triệu hồi minion quanh boss | xem bảng summon |

#### Summon (`SummonMinions`)

| Param | Phase 1 | Phase 2 |
|---|---|---|
| Số minion/lần | 1 | 2 |
| summonWindUpMs | 600 | 600 |
| minionSpawnRadius | 3.5 | 3.5 |
| maxActiveMinions | 6 (cap) | 6 |
| rangerSpawnWeight | 0.5 (50% Imp Ranger, còn lại Imp Mischief) | 0.5 |

Minion spawn dùng prefab **ImpMischief** / **Imp Mischief Ranger** — stats theo `EnemySO` minion tương ứng (§2.1).

#### Projectile types

| Loại | Dùng cho |
|---|---|
| standardProjectilePrefab | Spread, Rapid, Spiral |
| heavyProjectilePrefab | CircleBurst *(fallback standard nếu null)* |

---

### 7.3 Ghi chú balance boss

- Dmg mỗi viên đạn = `EnemySO.Damage` (Worm **50**, Imp Junior **35**).
- Sau mỗi pattern: delay **`AttackCooldown`** giây trước pattern tiếp theo.
- `BossConfigSO.healthMultiplier` **không** áp cho 2 boss này (prefab đã `isBoss=true` trên EnemySO).

**Script paths:**

```
Assets/Scripts/Enemy/Controller/Boss/StampedeBossController.cs
Assets/Scripts/Enemy/Controller/Boss/StampedeBossProjectileAttack.cs
Assets/Scripts/Enemy/Controller/Boss/ImpMischiefJuniorBossController.cs
Assets/Scripts/Enemy/Controller/Boss/ImpMischiefJuniorBossProjectileAttack.cs
Assets/Scripts/Enemy/Controller/Charge/EnemyChargeAttack.cs
Assets/Prefabs/Characters/Enemy/WormJunior.prefab
Assets/Prefabs/Characters/Enemy/ImpMischiefJunior.prefab
```

---

## 8. Known issues (balance / data)

1. **HeroCatalog_Global** list Starter, Pierce, Explosive, Luna, Fire — chưa trỏ `Hero_Finn` / `Hero_OTha`.
2. Hai bản Finn: `Hero_Finn` (stats mới) vs `Hero_Starter` (cũ) — catalog vẫn dùng Starter.
3. **Armor card** tăng stat nhưng `TakeDamage` chưa trừ armor.
4. **March card** (+0.15 speed) không hiệu lực do `RoundToInt`.
5. **enemyDamageScale = 1** cả chapter — difficulty chủ yếu từ HP scale.
6. **Finn skill SO:** cần `deliveryType = Cone` + gán projectile prefab.

---

## Asset paths

```
Assets/SO/Hero/Heroes/Hero_Finn.asset
Assets/SO/Hero/Heroes/Hero_OTha.asset
Assets/SO/Hero/Heroes/Hero_Luna.asset
Assets/SO/Hero/DashConfig_Default.asset
Assets/SO/Hero/Skills/Skill_Starter.asset
Assets/SO/Hero/Skills/Skill_Otha_Overdrive.asset
Assets/SO/Hero/Skills/Skill_Luna_FrostVeil.asset
Assets/SO/Weapon/Weapons/Weapon_Starter.asset
Assets/SO/Weapon/Weapons/Weapon_Otha.asset
Assets/SO/Weapon/Weapons/Weapon_Luna.asset
Assets/SO/Enemy/*.asset
Assets/SO/Card/*.asset
Assets/SO/Level/Levels/Level_Ch1_01.asset … Level_Ch1_10.asset
Assets/SO/Level/Waves/*.asset
Assets/SO/Level/Boss/*.asset
Assets/Scripts/Enemy/Controller/Boss/*.cs
Assets/Prefabs/Characters/Enemy/WormJunior.prefab
Assets/Prefabs/Characters/Enemy/ImpMischiefJunior.prefab
```
