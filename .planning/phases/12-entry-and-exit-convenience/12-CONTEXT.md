# Phase 12 Context: Entry & Exit Convenience

**Milestone:** v1.1 아레나 서브월드 디자인 개선
**Phase Goal:** Players control their own prep timing before a boss fight starts, and have a clear, safe way back to the main world via a return portal that routes through `SubworldSystem.Exit()`.
**Requirements Covered:** ENTRY-01, ENTRY-02, ENTRY-03

---

## 1. Scope & Intent

1. **Player-Controlled Prep Timing (ENTRY-01)**:
   - Discontinue immediate boss auto-summon on entering the arena.
   - The player prepares at their own pace and uses their summon item directly inside the subworld.
   - Infernum mode activation is preserved for bosses flagged `RequiresInfernumToggle`.
2. **In-World Return Portal (ENTRY-02)**:
   - A visible portal tile (`ReturnPortalTile`) generated near spawn in the arena.
   - Right-clicking invokes `SubworldSystem.Exit()`, safely executing the `OnExit` downed-flag restore guard.
3. **Additive UI Convenience (ENTRY-03)**:
   - SubworldLibrary's default Return button remains fully functional and untouched.

---

## 2. Key Decisions

- **D-01 (Infernum Mode Activation on Entry)**: `CalamityIntegration.ForceInfernumModeActiveInArena()` is triggered during `OnEnterWorld()` keyed off `ForcedTimeSystem.ActiveArenaBossNpcType`, ensuring Infernum boss AI is primed when the player uses the summon item.
- **D-02 (ReturnPortalTile Interaction)**: Direct call to `SubworldSystem.Exit()` inside `RightClick(int i, int j)`, outputting `"메인 월드로 귀환합니다."`.
- **D-03 (Placement near Spawn)**: Placed via `ArenaPolishPass` at `(Main.maxTilesX / 2) + 4` on the base platform row `surfaceY - 1`.

---

## 3. Wave Breakdown

- **Wave 1**: Systems Refactor (`BossSummonPlayer.cs`, `Test1Tile.cs`, `BiomeOverridePlayer.cs`).
- **Wave 2**: Portal Tile & Placement (`ReturnPortalTile.cs`, `ArenaBuilder.cs`, `ArenaPolishPass.cs`).
