# Phase 12 Research: Entry & Exit Convenience

**Milestone:** v1.1 아레나 서브월드 디자인 개선
**Researched:** 2026-08-15
**Domain:** tModLoader runtime entry hooks, player-driven boss summon, SubworldLibrary exit routing, and return portal tile placement.
**Requirements:** ENTRY-01, ENTRY-02, ENTRY-03
**Confidence:** HIGH

---

## 1. Architectural Analysis

### 1.1 Entry Flow & Player Prep Control (ENTRY-01)
- **Current Behavior**:
  - `Test1Tile.RightClick()` sets `BossSummonPlayer.PendingBossNpcType` and enters the subworld.
  - `BossSummonPlayer.OnEnterWorld()` immediately executes `NPC.SpawnOnPlayer(Player.whoAmI, PendingBossNpcType.Value)`.
  - The player is thrust directly into the boss battle with zero orientation or preparation time.
- **Target Behavior**:
  - The player enters the subworld with their held boss-summon item.
  - `BossSummonPlayer.OnEnterWorld()` does NOT automatically call `NPC.SpawnOnPlayer`.
  - If the active boss requires Infernum Mode active (`def.RequiresInfernumToggle`), `CalamityIntegration.ForceInfernumModeActiveInArena()` is still invoked on `OnEnterWorld()` using `ForcedTimeSystem.ActiveArenaBossNpcType`.
  - The player can use potions, adjust buffs, check positions, and use the held summon item whenever ready.
  - When the player uses their summon item in the arena subworld, the normal vanilla/modded summon pipeline executes smoothly because `ForcedTimeSystem` holds night (if needed), `BiomeOverridePlayer` / arena biome tiles satisfy biome prerequisites, and Infernum mode is active.

### 1.2 Return Portal Tile & Subworld Exit (ENTRY-02, ENTRY-03)
- **Exit Mechanism**:
  - SubworldLibrary provides `SubworldSystem.Exit()`.
  - `BossArenaSubworld.OnExit()` (and each biome subworld subclass) executes a critical vanilla downed-flag restore guard before SubworldLibrary reloads the main world.
  - **Crucial Invariant**: Custom teleportation (`Player.Teleport`) MUST NOT be used for crossing subworld boundaries. The return portal must call `SubworldSystem.Exit()` directly.
- **`ReturnPortalTile` Design**:
  - Custom `ModTile` (`Style1x1` or `Style2x2`) with `Main.tileFrameImportant = true`, `Main.tileNoAttach = true`.
  - Right-click interaction (`RightClick(int i, int j) -> bool`) displays feedback text (`"메인 월드로 귀환합니다."`) and invokes `SubworldSystem.Exit()`.
  - Automatically placed near the player's spawn point (`Main.spawnTileX + 4`, on top of the base platform) during `ArenaPolishPass`.
- **Preserving Built-in Return Button (ENTRY-03)**:
  - `ReturnPortalTile` is an additive in-world convenience. It routes through the exact same `SubworldSystem.Exit()` API without modifying or suppressing SubworldLibrary's built-in UI Return button.

---

## 2. Codebase Touchpoints

1. `Systems/BossSummonPlayer.cs`:
   - Remove `NPC.SpawnOnPlayer` auto-summon.
   - Refactor `OnEnterWorld` to use `ForcedTimeSystem.ActiveArenaBossNpcType` for InfernumMode toggle activation.
2. `Tiles/Test1Tile.cs`:
   - Update entry message: `"보스 아레나로 입장합니다. 준비가 되면 소환 아이템을 사용하세요."`.
   - Remove `BossSummonPlayer.PendingBossNpcType` assignment.
3. `Systems/BiomeOverridePlayer.cs`:
   - Replace legacy `SubworldSystem.IsActive<BossArenaSubworld>()` check with `BossArenaRoutingRegistry.IsAnyArenaActive()`.
4. `Tiles/ReturnPortalTile.cs` (New):
   - Custom `ModTile` calling `SubworldSystem.Exit()`.
5. `Subworlds/ArenaBuilder.cs` & `Subworlds/ArenaPolishPass.cs`:
   - Add `PlaceReturnPortal(int x, int y, ushort tileType)` and call it in `ArenaPolishPass.ApplyPass()` near spawn (`spawnTileX + 4, surfaceY - 1`).

---

## 3. Wave Structure Recommendation

- **Wave 1: Entry Flow & System Gate Refactoring (`ENTRY-01`)**
  - Refactor `BossSummonPlayer.cs` and `Test1Tile.cs` to eliminate auto-summon.
  - Update `BiomeOverridePlayer.cs` gate.
- **Wave 2: Return Portal Implementation & Arena Placement (`ENTRY-02`, `ENTRY-03`)**
  - Implement `Tiles/ReturnPortalTile.cs` and asset.
  - Update `ArenaBuilder.cs` and `ArenaPolishPass.cs` to place the return portal near spawn.
  - Validate with `dotnet build`.
