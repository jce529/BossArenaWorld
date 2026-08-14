---
phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing
plan: 01
subsystem: infra
tags: [tmodloader, csharp, summon-item-registry, forced-time, boss-arena]

# Dependency graph
requires:
  - phase: 09-biome-dependent-subworld-coverage
    provides: BossArenaRoutingRegistry.IsAnyArenaActive() and per-boss arena routing
provides:
  - "SummonItemRegistry.RegisterPolymorphic(itemType, resolveBossNpcType, canSummon) for
     single-item-multiple-boss summon items (e.g. Calamity's MarkofProvidence)"
  - "SummonItemRegistry.TryGetBoss(Player, int, out int) player-aware overload"
  - "ForcedTimeSystem ModSystem: RegisterForceNight(bossNpcType) + per-tick forced-night
     re-assertion for the whole arena visit"
  - "Tiles/Test1Tile.cs wired to both new capabilities, zero regression to existing bosses"
affects: [10-02, 10-03, 10-04, 10-05, 10-06]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Polymorphic summon-item resolution: Func<Player, int> delegate returning -1 for
       'no valid boss for current state', mirroring the source item's own CanUseItem()==false
       outcome, kept in a separate dictionary from the single-boss _itemToBoss map so an item
       is either single-boss or polymorphic, never both"
    - "Forced-time-of-day-for-arena: a persistent (not consume-once) static field
       (ForcedTimeSystem.ActiveArenaBossNpcType) re-checked every PreUpdateWorld tick, guarded
       by BossArenaRoutingRegistry.IsAnyArenaActive() so it never leaks into the real main-world
       day/night cycle even though the field itself is never cleared on exit"

key-files:
  created: [Systems/ForcedTimeSystem.cs]
  modified: [Systems/SummonItemRegistry.cs, Tiles/Test1Tile.cs]

key-decisions:
  - "Kept the existing single-item TryGetBoss(int, out int) overload completely untouched --
     every other registered boss call site in the project continues to compile and behave
     identically"
  - "ForcedTimeSystem.ActiveArenaBossNpcType is intentionally never nulled on arena exit
     (unlike BossSummonPlayer.PendingBossNpcType's consume-once pattern) because
     PreUpdateWorld's own IsAnyArenaActive() guard makes leaving it set harmless"

patterns-established:
  - "Pattern: RegisterPolymorphic/TryGetBoss(Player,...) is the shared contract every later
     Phase 10 plan (10-02..10-06) uses to register multi-boss summon items"
  - "Pattern: ForcedTimeSystem.RegisterForceNight(bossNpcType) is the shared contract for any
     boss whose AI despawns outside night (Moon Jelly Wizard, Dusking, conditionally
     Astrum Deus/Astrum Aureus under InfernumMode)"

requirements-completed: [ARENA-01]

# Metrics
duration: 12min
completed: 2026-08-14
---

# Phase 10 Plan 01: Polymorphic Summon Items + Forced-Night Arena Foundation Summary

**Extended SummonItemRegistry with a player-aware polymorphic-item resolver and added a new ForcedTimeSystem that re-asserts forced night every tick for arena bosses that need it -- both wired into Test1Tile.RightClick with zero behavior change to any existing boss.**

## Performance

- **Duration:** 12 min
- **Started:** 2026-08-14T00:00:00Z (approx, not recorded at session start)
- **Completed:** 2026-08-14
- **Tasks:** 3
- **Files modified:** 3 (2 modified, 1 created)

## Accomplishments
- `SummonItemRegistry.RegisterPolymorphic` + `TryGetBoss(Player, int, out int)` overload let a single `Item.type` resolve to one of several bosses based on the player's live Zone state at click-time (needed for Calamity's `MarkofProvidence`: Ceaseless Void / Signus / Storm Weaver)
- New `Systems/ForcedTimeSystem.cs` provides a per-boss forced-night mechanism that re-asserts `Main.dayTime = false` / `Main.time = 0.0` every `PreUpdateWorld` tick for the whole arena visit, guarded so it never touches the real main-world day/night cycle
- `Tiles/Test1Tile.cs` wired to both new capabilities: calls the player-aware `TryGetBoss` overload and sets `ForcedTimeSystem.ActiveArenaBossNpcType` before entering the arena
- Every existing single-boss registration (King Slime, Hive Mind, Infernon, Thorn, Astrageldon) continues to redirect correctly -- confirmed by keeping the original `TryGetBoss(int, out int)` overload byte-for-byte unchanged and by both new mechanisms being pure no-ops until Plans 10-02..10-06 register real callers

## Task Commits

Each task was committed atomically:

1. **Task 1: Add polymorphic resolver support to SummonItemRegistry** - `942d067` (feat)
2. **Task 2: Create Systems/ForcedTimeSystem.cs** - `05819dd` (feat)
3. **Task 3: Wire Test1Tile.RightClick to the polymorphic lookup and forced-night tracking** - `37b3c34` (feat)

**Plan metadata:** (this commit) `docs(10-01): complete polymorphic summon item + forced-night foundation plan`

## Files Created/Modified
- `Systems/SummonItemRegistry.cs` - Added `_polymorphicResolvers` dictionary, `RegisterPolymorphic()`, and the player-aware `TryGetBoss(Player, int, out int)` overload; original `TryGetBoss(int, out int)` unchanged
- `Systems/ForcedTimeSystem.cs` (new) - `ModSystem` with `ActiveArenaBossNpcType` static field, `RegisterForceNight(bossNpcType)`, and `PreUpdateWorld()` re-asserting forced night every tick while inside a registered arena for a force-night boss
- `Tiles/Test1Tile.cs` - `RightClick` now calls `SummonItemRegistry.TryGetBoss(player, player.HeldItem.type, out int bossNpcType)` and sets `ForcedTimeSystem.ActiveArenaBossNpcType = bossNpcType` before `BossArenaRoutingRegistry.Enter(bossNpcType)`

## Decisions Made
- Kept the existing single-item `TryGetBoss(int, out int)` overload completely untouched so every other registered boss call site continues to compile and behave identically -- no regression risk introduced by this plan.
- `ForcedTimeSystem.ActiveArenaBossNpcType` is intentionally never nulled on arena exit (unlike `BossSummonPlayer.PendingBossNpcType`'s consume-once pattern), because `PreUpdateWorld`'s own `BossArenaRoutingRegistry.IsAnyArenaActive()` guard makes leaving it set harmless between visits.

## Deviations from Plan

None - plan executed exactly as written. One environment-setup step was required but is not a plan deviation: this worktree's gitignored `Libs/*.dll` compile-time references (SubworldLibrary, CalamityMod, SpiritMod, Redemption, CatalystMod, ContinentOfJourney) were missing (known per-worktree setup gap documented in STATE.md Phase 02/06 notes) and were copied from the main working tree before the first `dotnet build` could succeed.

## Issues Encountered
None - all three `dotnet build BossArenaSubWorld.csproj` runs (one per task) exited with 0 errors, 0 warnings.

## User Setup Required

None - no external service configuration required. No live in-game verification in this plan by design (per plan `<verification>`: "these are inert extensions with zero real callers until Plans 10-02 through 10-06 register actual bosses against them").

## Next Phase Readiness

- `SummonItemRegistry.RegisterPolymorphic`/`TryGetBoss(Player,...)` and `ForcedTimeSystem.RegisterForceNight`/`ActiveArenaBossNpcType` are now available as the shared contracts Plans 10-02 through 10-06 will register real bosses against (Calamity's MarkofProvidence-summoned trio; Spirit's Moon Jelly Wizard, Dusking; Calamity's InfernumMode-only Astrum Deus/Astrum Aureus).
- No blockers. The final live checkpoint in Plan 10-06 will exercise both capabilities end-to-end once real bosses are registered.

---
*Phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing*
*Completed: 2026-08-14*

## Self-Check: PASSED

- FOUND: Systems/SummonItemRegistry.cs
- FOUND: Systems/ForcedTimeSystem.cs
- FOUND: Tiles/Test1Tile.cs
- FOUND: 942d067 (Task 1 commit)
- FOUND: 05819dd (Task 2 commit)
- FOUND: 37b3c34 (Task 3 commit)
