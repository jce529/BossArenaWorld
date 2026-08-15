# Phase 13 Research: Boundary & Tier Extension to All Biome Variants

**Milestone:** v1.1 아레나 서브월드 디자인 개선
**Researched:** 2026-08-15
**Domain:** Procedural arena generation, Zone-flag Y-windows, tile-weight thresholds, and JIT safety across 8 biome arenas.
**Requirements:** BOUND-01, BOUND-02, BOUND-04, TIER-02, TIER-03
**Confidence:** HIGH

---

## 1. Architectural & Domain Findings

### 1.1 Pure-Y Zone Biomes (Space & Underworld) — BOUND-02
Decompiled `Terraria.Player.UpdateBiomes()` establishes strict Y-bounds for Space and Underworld:
1. **Space (`ZoneSkyHeight`)**:
   - Condition: `player.Y <= Main.worldSurface * 0.35` -> At `WorldHeight = 800`, `worldSurface = 240`, threshold is `y <= 84`.
   - Layout Design:
     - `surfaceY = 70`, `thickness = 10`.
     - Tiers: Tier 1 at `y = 52`, Tier 2 at `y = 34`.
     - Bottom boundary floor: placed at `y = 80` (`surfaceY + thickness`), strictly above row 84.
     - Top ceiling barrier: placed at `y = 10`.
     - Result: Entire arena space is contained strictly in `y in [10, 80]`, guaranteeing `ZoneSkyHeight` NEVER drops false.
2. **Underworld (`ZoneUnderworldHeight`)**:
   - Condition: `player.Y > Main.UnderworldLayer` -> At `WorldHeight = 800`, `UnderworldLayer = 600`, threshold is `y > 600`.
   - Layout Design:
     - `surfaceY = 670`, `thickness = 10`.
     - Tiers: Tier 1 at `y = 642`, Tier 2 at `y = 614`.
     - Top ceiling barrier: placed at `y = 605` (with `boundaryMargin = 65`), so players jumping from Tier 2 cannot rise above `y = 605`.
     - Bottom boundary floor: placed at `y = 681` to `687`.
     - Result: Entire arena space is contained strictly in `y in [605, 687]`, guaranteeing `ZoneUnderworldHeight` NEVER drops false.

### 1.2 Tile-Weighted Biomes (Corruption, Hallow, Jungle, Desert) — TIER-02, TIER-03
- SceneMetrics scan window is ~200 tiles wide by ~140 tiles high centered on the player.
- **Corruption**: 15-thick Ebonstone (`threshold = 300`). At tier 2 (`y = 344`), distance to base platform (`y = 400`) is 56 tiles, well within the 70-tile vertical radius. Count remains ~3000 >> 300.
- **Hallow**: 15-thick Pearlstone (`threshold = 125`). Count remains ~3000 >> 125.
- **Jungle**: 15-thick JungleGrass (`threshold = 140`). Count remains ~3000 >> 140.
- **Desert**: 20-thick Sandstone/Sand (`threshold = 1500`). Count remains ~4000 >> 1500.

### 1.3 Modded Biomes (Astral & Briar) & JIT Safety — BOUND-04
- **Astral**: `BiomeTileCounterSystem.AstralTiles > 950`. 15-thick Astral tiles provide ~3000 tiles >> 950.
- **Briar**: `BiomeTileCounts.briarCount > 80` and `player.Y <= 240`. `surfaceY = 150` with 15-thick BriarGrass provides ~3000 tiles >> 80.
- **JIT Safety Discipline**:
  - `ArenaPolishPass` public API is strictly primitive (`ushort`/`int`), referencing zero modded types.
  - In `BossArenaAstralSubworld.cs` and `BossArenaBriarSubworld.cs`, `Tasks` list constructs `ArenaPolishPass` with pure integers.
  - Modded tile references remain strictly confined to `AstralPlatformPass.ApplyPass()` and `BriarPlatformPass.ApplyPass()` (which are protected with `[JITWhenModsEnabled]`).

---

## 2. Per-Arena Parameter Matrix

| Arena Subworld | `surfaceY` | `thickness` | `tierCount` | `tierSpacing` | `boundaryMargin` | `torchStyle` | Notes |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| **Plain** | 400 | 15 | 3 | 28 | 120 | `TorchID.Torch` (0) | Already proven in Phase 11 |
| **Corruption** | 400 | 15 | 3 | 28 | 120 | `TorchID.Purple` (4) | Purple Corrupt torches |
| **Hallow** | 400 | 15 | 3 | 28 | 120 | `TorchID.Hallowed` (20) | Hallowed torches |
| **Underworld** | 670 | 10 | 3 | 28 | 65 | `TorchID.Demon` (7) | Strictly `y > 600` |
| **Space** | 70 | 10 | 3 | 18 | 60 | `TorchID.White` (5) | Strictly `y <= 84` |
| **Jungle** | 400 | 15 | 3 | 28 | 120 | `TorchID.Jungle` (21) | Jungle torches |
| **Desert** | 400 | 20 | 3 | 28 | 120 | `TorchID.Desert` (16) | Desert torches |
| **Astral** | 400 | 15 | 3 | 28 | 120 | `TorchID.Purple` (4) | JIT-safe pure int |
| **Briar** | 150 | 15 | 3 | 28 | 80 | `TorchID.Green` (3) | JIT-safe pure int |

---

## 3. Wave Execution Strategy

- **Wave 1**: Extend to the 6 Vanilla Biome Arenas (Corruption, Hallow, Underworld, Space, Jungle, Desert), adjusting `UnderworldPlatformPass` (`surfaceY = 670`) and `SpacePlatformPass` (`surfaceY = 70`).
- **Wave 2**: Extend to the 2 Modded Biome Arenas (Astral, Briar) and verify JIT-safety discipline.
