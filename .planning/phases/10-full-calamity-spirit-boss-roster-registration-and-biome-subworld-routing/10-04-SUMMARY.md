---
phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing
plan: 04
subsystem: boss-registration
tags: [calamity, tmodloader, jitwhenmodsenabled, polymorphic-resolver, infernum-compat]

# Dependency graph
requires:
  - phase: 10-01
    provides: "SummonItemRegistry.RegisterPolymorphic + player-aware TryGetBoss, ForcedTimeSystem.RegisterForceNight"
  - phase: 10-02
    provides: "Integrations/CalamityIntegration.cs base shape (Hive Mind + 4 Tier-1 bosses) that this plan extends"
provides:
  - "7 more Calamity boss registrations: Providence, Profaned Guardians, Astrum Deus, Astrum Aureus, Ceaseless Void, Signus, Storm Weaver"
  - "First real exercise of Plan 10-01's polymorphic resolver (MarkofProvidence -> 3 bosses by Zone) and forced-night registration (Infernum-conditional)"
  - "Integrations/CalamityIntegration.cs now registers 12 of 12 Calamity bosses this phase covers directly"
affects: [10-05, 10-06]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Infernum-absent gating: `if (ModLoader.HasMod(\"InfernumMode\")) return;` at the top of a RegisterX() method to skip registration entirely (Providence, Profaned Guardians)"
    - "Infernum-conditional forced night: `if (ModLoader.HasMod(\"InfernumMode\")) ForcedTimeSystem.RegisterForceNight(npcType);` (Astrum Deus, Astrum Aureus)"
    - "Polymorphic single-item multi-boss resolver: SummonItemRegistry.RegisterPolymorphic(itemType, namedResolverMethod) replicating the real item's UseItem() branch order exactly, returning -1 for 'no redirect'"
    - "Background-thread WorldGen dispatch replay: ThreadPool.QueueUserWorkItem(_ => ...) inside ApplyDowned, matching the real OnKill()'s own dispatch instead of simplifying to synchronous"

key-files:
  created: []
  modified:
    - Integrations/CalamityIntegration.cs

key-decisions:
  - "Providence and Profaned Guardians both route to BossArenaHallowSubworld (D-02 discretion) -- Providence has no functional Zone dependency (Hallow/Underworld choice is cosmetic-only), Hallow chosen to spread load since Underworld already hosts Signus"
  - "Astrum Deus's summon item is Starcore, not TitanHeart, since TitanHeart has unrelated armor-crafting uses (10-RESEARCH.md Discretion)"
  - "Ceaseless Void gets no BossArenaRoutingRegistry entry -- confirmed no Zone dependency and BossArenaDungeonSubworld was discarded in Phase 9 (D-07) -- falls back to the default BossArenaSubworld"
  - "MarkofProvidence resolver checks ZoneDungeon first (matching decompiled UseItem() branch order exactly), and additionally gates Ceaseless Void's branch on InfernumMode absence per D-02"

patterns-established:
  - "Infernum-conditional gating now has two proven shapes: full registration skip (Providence/Profaned Guardians) vs. partial behavior gate (Astrum Deus/Aureus forced-night only) -- future Infernum-affected bosses should pick whichever matches the actual behavioral difference"

requirements-completed: [ARENA-01]

duration: 8min
completed: 2026-08-14
---

# Phase 10 Plan 04: Infernum-Gated + Polymorphic Calamity Tier Summary

**Registered 7 more Calamity bosses in `Integrations/CalamityIntegration.cs` -- Providence and Profaned Guardians gated to InfernumMode-absent only, Astrum Deus/Astrum Aureus with Infernum-conditional forced night, and Ceaseless Void/Signus/Storm Weaver resolved from one shared MarkofProvidence item via Plan 10-01's polymorphic resolver -- bringing the file to 12 of 12 Calamity bosses this phase covers directly.**

## Performance

- **Duration:** 8 min
- **Started:** 2026-08-14T14:05:00Z
- **Completed:** 2026-08-14T14:13:36Z
- **Tasks:** 3
- **Files modified:** 1

## Accomplishments
- Providence and Profaned Guardians register only when InfernumMode is absent (D-02), both routed to BossArenaHallowSubworld, with faithful `ApplyDowned` replays of their real `OnKill()` side effects (ore generation, chat broadcasts, netcode sync)
- Astrum Deus and Astrum Aureus route unconditionally to BossArenaAstralSubworld and force night in that arena only when InfernumMode is loaded (D-02/D-04); Astrum Aureus's meteor placement replicates the real `ThreadPool.QueueUserWorkItem` background-thread dispatch exactly
- MarkofProvidence (one shared summon item) is registered polymorphically via `SummonItemRegistry.RegisterPolymorphic`, resolving to Ceaseless Void (Dungeon, InfernumMode-absent only), Signus (Underworld -> BossArenaUnderworldSubworld), or Storm Weaver (Sky -> BossArenaSpaceSubworld) -- first real exercise of both Plan 10-01 capabilities

## Task Commits

Each task was committed atomically:

1. **Task 1: Register Providence + Profaned Guardians (Infernum-off gate, Hallow altar)** - `95567b9` (feat)
2. **Task 2: Register Astrum Deus + Astrum Aureus (Astral altar, Infernum-conditional night)** - `cfc0342` (feat)
3. **Task 3: Register the MarkofProvidence polymorphic resolver (Ceaseless Void + Signus + Storm Weaver)** - `add81c6` (feat)

**Plan metadata:** (this commit) `docs(10-04): complete Infernum-gated + polymorphic Calamity tier plan`

## Files Created/Modified
- `Integrations/CalamityIntegration.cs` - Extended `PostSetupContent()` with 5 new `RegisterX()` calls (12 total for this phase's Calamity roster); added `RegisterProvidence`/`RegisterProfanedGuardians`/`RegisterAstrumDeus`/`RegisterAstrumAureus`/`RegisterMarkOfProvidenceBosses` plus their `IsXDowned`/`ApplyXDowned` pairs and the `ResolveMarkOfProvidenceBoss` named resolver method; added `using System.Threading;`

## Decisions Made
- Confirmed live via `dotnet build`: all 5 new `RegisterX()` methods compile against the installed `Libs/CalamityMod.dll` v2.2.4 fully-qualified type paths exactly as specified in the plan's decompiled interface section -- no namespace corrections needed this time (unlike Plan 10-02's `CalamityGlobalTownNPC` fix)
- Split the plan's single combined edit into 3 separate task commits (revert-and-reapply pattern) to preserve per-task atomic commit history despite all 3 tasks targeting the same file

## Deviations from Plan

None - plan executed exactly as written. All illustrative code from the plan compiled and passed acceptance criteria without modification.

**Worktree setup note (not a plan deviation):** This worktree's `Libs/*.dll` compile-time references were missing at session start (known per-worktree gap, documented in STATE.md since Phase 02/06) -- copied from the main working tree before the first build. They remain gitignored, not committed.

## Issues Encountered
None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- `Integrations/CalamityIntegration.cs` now registers all 12 Calamity bosses this phase covers directly (Hive Mind + Devourer of Gods/Yharon/Supreme Witch Calamitas/Dragonfolly from 10-02 + this plan's 7). Only The Old Duke (Plan 10-05, needs a new external DLL) remains to finish Calamity's full roster.
- Live in-game verification of the Infernum-conditional matrix (InfernumMode present vs. absent for Providence/Profaned Guardians/Astrum Deus/Astrum Aureus/Ceaseless Void) and polymorphic Zone-based resolution across all 3 reachable zones (Underworld, Sky, Dungeon) is explicitly deferred to Plan 10-06, per this plan's own scope.
- ARENA-01 stays open in REQUIREMENTS.md until 10-05/10-06 land and Plan 10-06's live verification confirms behavior.

---
*Phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing*
*Completed: 2026-08-14*

## Self-Check: PASSED

- FOUND: Integrations/CalamityIntegration.cs
- FOUND: commit 95567b9 (Task 1)
- FOUND: commit cfc0342 (Task 2)
- FOUND: commit add81c6 (Task 3)
- `dotnet build BossArenaSubWorld.csproj` exits 0 (0 warnings, 0 errors) after each task and after final state
