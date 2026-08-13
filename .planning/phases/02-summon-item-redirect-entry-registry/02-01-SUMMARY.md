---
phase: 02-summon-item-redirect-entry-registry
plan: 01
subsystem: gameplay-systems
tags: [tmodloader, terraria, modplayer, modsystem, subworldlibrary]

# Dependency graph
requires:
  - phase: 01-subworld-skeleton-isolation-proof
    provides: BossArenaSubworld (Subworld subclass), SubworldSystem.IsActive<BossArenaSubworld>() guard convention (BiomeOverridePlayer.cs precedent)
provides:
  - "SummonItemRegistry: data-driven Item.type -> NPC.type lookup (SUBW-01), pre-populated with ItemID.SlimeCrown -> NPCID.KingSlime"
  - "BossSummonPlayer: generic once-per-redirect arrival auto-summon hook (SUBW-04) via NPC.SpawnOnPlayer"
affects: [02-portal-tile-redirect, 02-verification]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Static Dictionary<int,int>-backed registry populated in ModSystem.PostSetupContent, exposed via static Register()/TryGetBoss() -- no per-item branching (D-06 data-driven shape)"
    - "Nullable static field (int?) on a ModPlayer as a singleplayer-only one-shot handoff between a producer (future portal tile) and a consumer (OnEnterWorld), always nulled immediately after consumption"

key-files:
  created:
    - Systems/SummonItemRegistry.cs
    - Systems/BossSummonPlayer.cs
  modified: []

key-decisions:
  - "Generalized D-09 ('replay the item's use effect') as 'call NPC.SpawnOnPlayer with the mapped boss type' rather than literally reflecting into Player.ItemCheck_UseBossSpawners, since vanilla summon items have no isolated, externally-callable use-effect method (per 02-RESEARCH.md)"
  - "Used SubworldSystem.IsActive<BossArenaSubworld>(), not AnyActive<BossArenaSubworld>(), correcting a non-compiling example in 02-RESEARCH.md (AnyActive has a `where T : Mod` constraint and BossArenaSubworld is a Subworld, not a Mod) -- also matches the existing BiomeOverridePlayer.cs convention from Phase 1"

patterns-established:
  - "Registry ModSystem pattern: static Dictionary-backed lookup populated once in PostSetupContent, queried via static TryGetX(), for any future item-to-content mapping needs"
  - "One-shot static handoff field pattern: nullable static field set by a producer, consumed and nulled by name-matched OnEnterWorld/OnEnterWorld-equivalent hook, explicitly scoped to singleplayer-only projects"

requirements-completed: [SUBW-01, SUBW-04]

# Metrics
duration: 8min
completed: 2026-08-13
---

# Phase 2 Plan 01: Summon-Item Registry & Arrival Auto-Summon Hook Summary

**Data-driven Item.type -> NPC.type registry (SummonItemRegistry) plus a once-per-redirect arrival auto-summon hook (BossSummonPlayer) that replays a boss-summon item's effect via vanilla's own NPC.SpawnOnPlayer primitive.**

## Performance

- **Duration:** 8 min
- **Started:** 2026-08-13T03:15:00Z (approx, first read)
- **Completed:** 2026-08-13T03:23:18Z
- **Tasks:** 2
- **Files modified:** 2 (both new)

## Accomplishments
- SummonItemRegistry.cs: static, data-driven Dictionary<int,int> registry mapping summon-item `Item.type` to boss `NPC.type`, with a `Register()`/`TryGetBoss()` static API and one pre-populated entry (`ItemID.SlimeCrown -> NPCID.KingSlime`, the D-08 proof entry) added in `PostSetupContent`
- BossSummonPlayer.cs: `ModPlayer` with a nullable static `PendingBossNpcType` field and an `OnEnterWorld()` hook that, gated on `SubworldSystem.IsActive<BossArenaSubworld>()`, calls `NPC.SpawnOnPlayer` exactly once per redirect and immediately nulls the pending field to prevent re-triggering on a later unrelated subworld entry
- Both files compile cleanly (`dotnet build BossArenaSubWorld.csproj`, 0 errors / 0 warnings) as part of the full mod project

## Task Commits

Each task was committed atomically:

1. **Task 1: Create the summon-item registry** - `feca5d2` (feat)
2. **Task 2: Create the arrival auto-summon hook** - `61c1ee7` (feat)

**Plan metadata:** (this commit, docs: complete plan)

## Files Created/Modified
- `Systems/SummonItemRegistry.cs` - ModSystem holding the itemType->bossNpcType Dictionary, Register()/TryGetBoss() static methods, populates ItemID.SlimeCrown->NPCID.KingSlime in PostSetupContent
- `Systems/BossSummonPlayer.cs` - ModPlayer with static PendingBossNpcType field and OnEnterWorld hook that spawns the pending boss once, gated to the boss-arena subworld

## Decisions Made
- Generalized D-09 as "call NPC.SpawnOnPlayer with the mapped boss type," documented inline in BossSummonPlayer.cs, since vanilla boss-summon items have no isolated, externally-callable use-effect method
- Confirmed and applied the IsActive-vs-AnyActive correction from the plan's `<interfaces>` block (AnyActive<T> requires `where T : Mod`, which BossArenaSubworld does not satisfy)

## Deviations from Plan

None - plan executed exactly as written. One local build-environment fix was required but is not a code deviation: this worktree was missing the gitignored `Libs/SubworldLibrary.dll` compile-time reference (extracted from the installed Workshop copy, per Phase 1's established convention of not committing it to git); it was copied in from the main repo directory to allow `dotnet build` to succeed. No source files or build configuration were changed by this fix, and it is not part of any commit.

## Issues Encountered
- Initial `dotnet build` failed with CS0246 "SubworldLibrary type or namespace not found" across all files (not just the two new ones) because this git worktree lacked the gitignored `Libs/SubworldLibrary.dll` local reference documented in STATE.md's Phase 1 decisions. Resolved by copying the DLL from the main repository directory into this worktree's `Libs/` folder (a local, untracked file matching existing `.gitignore` behavior) -- not a code change, no commit needed.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- `Systems/SummonItemRegistry.cs` and `Systems/BossSummonPlayer.cs` are ready to be wired together by Plan 02-02's `Test1Tile.NewRightClick`: on right-click, look up the held item via `SummonItemRegistry.TryGetBoss`, set `BossSummonPlayer.PendingBossNpcType`, then call `SubworldSystem.Enter<BossArenaSubworld>()`
- No player-facing behavior exists yet from this plan alone (no tile exists to trigger the registry lookup or set the pending field) -- this is expected per the plan's own objective and will be exercised end-to-end once Plan 02-02 lands
- Isolation-premise concern from Phase 1 (STATE.md blocker: live King Slime test showed `NPC.downedSlimeKing=True` after subworld round-trip, contradicting research expectations) remains open and unrelated to this plan's scope; flagged here for visibility before Plan 02-03's live verification pass

---
*Phase: 02-summon-item-redirect-entry-registry*
*Completed: 2026-08-13*

## Self-Check: PASSED

- FOUND: Systems/SummonItemRegistry.cs
- FOUND: Systems/BossSummonPlayer.cs
- FOUND: feca5d2 (Task 1 commit)
- FOUND: 61c1ee7 (Task 2 commit)
