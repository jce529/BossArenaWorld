# Phase 14 Research: Per-Biome Decorative Theming

**Milestone:** v1.1 아레나 서브월드 디자인 개선
**Researched:** 2026-08-15
**Domain:** Procedural decorative theming, biome background walls, additive qualifying tiles, and JIT safety.
**Requirements:** DECOR-01, DECOR-02, DECOR-03
**Confidence:** HIGH

---

## 1. Architectural & Aesthetic Strategy

### 1.1 Additive Biome Decoration Strategy (`DECOR-01`, `DECOR-02`)
To ensure that decorative polish makes each arena feel immersive and visually distinct without dropping below any Zone flag qualifying threshold (`DECOR-02`):
1. **Background Backing Walls (`FillWall`)**:
   - Placing themed background walls behind the 3-tier platform band (`y in [surfaceY - 60, surfaceY + thickness]`) gives each arena rich atmospheric depth.
   - Walls do not affect tile collision, gravity, or Zone-flag calculations.
2. **Platform & Column Biome Theming**:
   - **Plain**: Stone Slab / Gray Brick accents + Stone Wall (`WallID.Stone`). Campfire at center.
   - **Corruption**: Ebonstone brick accents + Corrupt Wall (`WallID.EbonstoneUnsafe`).
   - **Hallow**: Pearlstone brick accents + Hallow Wall (`WallID.HallowedGrassUnsafe`).
   - **Underworld**: Obsidian Brick / Ash detailing + Obsidian Brick Wall (`WallID.ObsidianBrick`).
   - **Space**: Cloud & Sunplate accents (`TileID.Cloud`, `TileID.Sunplate`) + Glass/Sky Wall (`WallID.Glass`).
   - **Jungle**: Rich Mahogany & Jungle Grass accents + Jungle Wall (`WallID.JungleUnsafe`).
   - **Desert**: Smooth Sandstone / Hardened Sand + Sandstone Wall (`WallID.SandstoneBrick`).
   - **Astral (Calamity)**: Modded Astral Stone / Astral Monolith / Astral Wall placed within `AstralPlatformPass.ApplyPass()` (`[JITWhenModsEnabled("CalamityMod")]`).
   - **Briar (Spirit)**: Modded Briar Grass / Briar Wall placed within `BriarPlatformPass.ApplyPass()` (`[JITWhenModsEnabled("SpiritMod")]`).

### 1.2 JIT-Safety Discipline for Modded Theming (`DECOR-03`)
- All modded decoration logic for Astral and Briar remains strictly isolated inside `AstralPlatformPass.cs` and `BriarPlatformPass.cs`.
- `ArenaBuilder` and `ArenaPolishPass` remain 100% mod-agnostic and use pure primitive types (`ushort`/`int`).

---

## 2. Decorative Palette Matrix

| Arena | Primary Platform Tiles | Background Wall | Campfire / Accents |
| :--- | :--- | :--- | :--- |
| **Plain** | Stone / Gray Brick | `WallID.Stone` (1) | Campfire at spawn |
| **Corruption** | Ebonstone / Corrupt Grass | `WallID.EbonstoneUnsafe` (3) | Corrupt Campfire |
| **Hallow** | Pearlstone / Hallowed Grass | `WallID.HallowedGrassUnsafe` (70) | Hallowed Campfire |
| **Underworld** | Hellstone / Ash / Obsidian | `WallID.ObsidianBrick` (20) | Demon Campfire |
| **Space** | Sunplate / Cloud / Stone | `WallID.Glass` (21) / Cloud (153) | Ultrabright Campfire |
| **Jungle** | Jungle Grass / Rich Mahogany | `WallID.JungleUnsafe` (63) | Jungle Campfire |
| **Desert** | Sandstone / Smooth Sand | `WallID.SandstoneBrick` (21) | Desert Campfire |
| **Astral** | Astral Stone / Astral Grass | Astral Wall (Calamity) | Astral Palette |
| **Briar** | Briar Grass / Briar Foliage | Briar Wall (Spirit) | Briar Palette |

---

## 3. Wave Execution Strategy

- **Wave 1: Plain & 6 Vanilla Biomes Theming (`DECOR-01`, `DECOR-02`)**
  - Add `FillWall` helper to `ArenaBuilder.cs`.
  - Enhance `FlatStonePlatformPass`, `CorruptionPlatformPass`, `HallowPlatformPass`, `UnderworldPlatformPass`, `SpacePlatformPass`, `JunglePlatformPass`, `DesertPlatformPass`.
- **Wave 2: 2 Modded Biomes Theming & JIT Safety (`DECOR-03`)**
  - Enhance `AstralPlatformPass.cs` (Calamity) and `BriarPlatformPass.cs` (Spirit).
  - Verify JIT safety with mods disabled.
