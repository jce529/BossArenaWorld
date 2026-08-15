# Phase 11 Context: Shared Arena-Polish Foundation (Plain Arena)

**Milestone:** v1.1 아레나 서브월드 디자인 개선
**Phase Goal:** Build and prove the mod-agnostic boundary, torch lighting, and multi-tier platform foundation on the plain arena first, with zero biome or JIT risk.
**Requirements Covered:** BOUND-03, LIGHT-01, LIGHT-02, TIER-01

---

## 1. Background & Scope

In v1.0, the mod established 9 minimalist arena subworlds (1 plain + 8 biome variants). The plain arena (`BossArenaSubworld`) generated only a single thin stone platform (`FlatStonePlatformPass`).
Phase 11 establishes the reusable, mod-agnostic foundation for arena polish:
1. **Solid Boundary Containment (`BoundaryTile`)**: Invisible solid boundary preventing players from falling into the void or being knocked out of the arena.
2. **Shared Static Helper (`ArenaBuilder`)**: Primitive-only methods (`ushort`/`int`) for building boundary walls, multi-tier platforms, and torch lighting.
3. **Shared GenPass (`ArenaPolishPass`)**: A reusable `GenPass` parameterized by `surfaceY`, `thickness`, `tierCount`, `tierSpacing`, and `torchStyle`, appended to `Tasks`.
4. **Integration on Plain Arena**: Validates the whole pipeline on `BossArenaSubworld` first before extending to the 8 biome variants in Phase 13.

---

## 2. Load-Bearing Decisions & Constraints

- **D-01 (Primitive-Only Public API in `ArenaBuilder` / `ArenaPolishPass`)**: Must never reference any `CalamityMod.*` or `SpiritMod.*` types. All tile types are passed as `ushort`/`int` primitives so JIT prefiltering on autoloaded classes never triggers `JITException` when optional mods are absent.
- **D-02 (Invisible Boundary Tile `BoundaryTile`)**: A custom `ModTile` with `Main.tileSolid[Type] = true`, `Main.tileBlockLight[Type] = false`, and `PreDraw => false` so players are contained without visual clutter or shadow artifacts.
- **D-03 (Multi-Tier Platform Geometry)**: 3 platform tiers (base stone + 2 wooden platform tiers above it), spaced at 28 tiles (standard vanilla jump height without double-jump accessories).
- **D-04 (Torch Placement)**: Placed along tiers at regular intervals (~30 tiles) using `WorldGen.PlaceTile` with `TileID.Torches` and style parameters. Explicitly documented as visual-only (LIGHT-02; light level does not affect mob spawns in Terraria).

---

## 3. Interfaces & Components

### `Tiles/BoundaryTile.cs`
Custom invisible solid `ModTile`.

### `Subworlds/ArenaBuilder.cs`
Static methods:
- `PlaceBoundaryWalls(int startX, int endX, int surfaceY, int thickness, int boundaryMargin, ushort boundaryTileType)`
- `BuildTierPlatforms(int startX, int endX, int baseSurfaceY, int tierCount, int tierSpacing, ushort platformTileType, int platformStyle = 0)`
- `PlaceTorchInterval(int startX, int endX, int y, int interval, int torchStyle = 0)`

### `Subworlds/ArenaPolishPass.cs`
`GenPass` subclass executing `ArenaBuilder` methods parameterized per subworld.

### `Subworlds/BossArenaSubworld.cs`
Appends `ArenaPolishPass` to `Tasks`.
