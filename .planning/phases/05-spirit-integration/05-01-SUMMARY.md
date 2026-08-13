---
phase: 05-spirit-integration
plan: 01
subsystem: mod-integration
tags: [tmodloader, reflection, weakReferences, JITWhenModsEnabled, spiritmod, BossRegistry]

# Dependency graph
requires:
  - phase: 04-calamity-integration-cross-mod-side-effect-reproduction
    provides: "BossRegistry/SummonItemRegistry pipeline, weakReferences + [JITWhenModsEnabled] isolation pattern, named-delegate-not-lambda JIT-safety rule"
provides:
  - "Integrations/SpiritIntegration.cs registering spirit:infernon into BossRegistry/SummonItemRegistry"
  - "Reflection-based write pattern for internal static Dictionary<string,bool> fields (BossDownedTracker.Downed), reusable for future Spirit bosses"
  - "SpiritMod weak reference wired into build.txt/BossArenaSubWorld.csproj"
affects: [05-02-live-verification, 06-redemption-integration, 07-catalystmod-integration]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Cached reflection FieldInfo pattern for internal mod fields with no public setter (System.Reflection.FieldInfo, BindingFlags.NonPublic|Static, cached once at PostSetupContent, GetValue(null) at apply time)"
    - "Multiple weakReferences entries in build.txt use comma separation, not space separation (corrected from plan's illustrative syntax)"

key-files:
  created: [Integrations/SpiritIntegration.cs]
  modified: [build.txt, BossArenaSubWorld.csproj]

key-decisions:
  - "weakReferences multi-entry syntax is comma-separated (ModA@1.0, ModB@2.0), not space-separated -- confirmed live via tModLoader's own BuildProperties.ModReference.Parse throwing 'Invalid mod reference' on the plan's space-separated illustrative syntax"
  - "Both Infernon and InfernoSkull NPC types registered under one BossDefinition since either's OnKill can be the actual downed-trigger depending on Normal vs Expert Mode difficulty"
  - "Reflection write path targets BossDownedTracker's internal Downed dictionary directly (only path available -- no public setter exists); read path uses the public MyWorld.DownedInfernon wrapper property with zero reflection"

requirements-completed: [MOD-02]

# Metrics
duration: 7min
completed: 2026-08-13
---

# Phase 5 Plan 1: Spirit Infernon Registration Summary

**Registered Infernon into BossRegistry using cached reflection against SpiritMod's internal `BossDownedTracker.Downed` dictionary (no public setter exists) paired with a zero-reflection public read via `MyWorld.DownedInfernon`, proving the generic pattern generalizes to a dictionary-based tracking API distinct from Calamity's wrapper-property shape.**

## Performance

- **Duration:** 7 min
- **Started:** 2026-08-13T21:33
- **Completed:** 2026-08-13T21:39
- **Tasks:** 2 completed
- **Files modified:** 3 (Integrations/SpiritIntegration.cs created; build.txt, BossArenaSubWorld.csproj modified)

## Accomplishments
- SpiritMod wired in as a weak, JIT-isolated dependency (`Libs/SpiritMod.dll` copied from the already-decompiled source, `build.txt`/`.csproj` updated, project builds clean)
- `Integrations/SpiritIntegration.cs` registers `spirit:infernon` into the existing, unmodified `BossRegistry`/`SummonItemRegistry` pipeline
- First genuine use of runtime reflection in this project: cached `FieldInfo` write into `BossDownedTracker`'s `internal static Dictionary<string, bool> Downed` field, since no public setter exists
- Both `Infernon` and `InfernoSkull` NPC types registered under one `BossDefinition` (Pitfall B: either can be the entity whose `OnKill` actually fires, depending on Normal vs Expert Mode)
- Infernon/InfernoSkull's `HellstoneBrick` tile-ring WorldGen side effect faithfully replayed, anchored on the player's position (no live NPC exists at `BossCoreItem`-use time)
- Reflection failures caught and logged via `Mod.Logger.Warn`, never thrown out of `UseItem`
- D-03 classification (fully world-scoped, no player-scoped side effect) documented explicitly in-code per Phase 5 Success Criterion 2

## Task Commits

Each task was committed atomically:

1. **Task 1: Copy Libs/SpiritMod.dll and wire the weakReferences build chain** - `48bc73e` (chore)
2. **Task 2: Register Infernon via Integrations/SpiritIntegration.cs** - `0e96c4e` (feat)

**Plan metadata:** (this commit, pending)

## Files Created/Modified
- `Integrations/SpiritIntegration.cs` - New file: registers `spirit:infernon` into `BossRegistry`/`SummonItemRegistry`, isolated behind `[JITWhenModsEnabled("SpiritMod")]`
- `build.txt` - Added `SpiritMod@1.5.0.44` to `weakReferences` (comma-separated)
- `BossArenaSubWorld.csproj` - Added compile-time-only `Reference` block for `Libs\SpiritMod.dll`, mirroring the existing `CalamityMod` block
- `Libs/SpiritMod.dll` - Copied locally from `ModReader/SpiritMod/SpiritMod.dll` (gitignored, not committed)

## Decisions Made
- Confirmed live that tModLoader's `build.txt` parser (`BuildProperties.ModReference.Parse`) requires comma-separated entries for multiple `weakReferences` on one line (`CalamityMod@2.2.4, SpiritMod@1.5.0.44`), not the space-separated syntax the plan's action block illustrated -- fixed inline, see Deviations below
- Kept the plan's exact reflection design: cache `FieldInfo` once in `RegisterInfernon()`, reuse at apply time via `GetValue(null)`, avoiding a fresh reflection lookup on every `BossCoreItem` use

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed `weakReferences` multi-entry separator syntax**
- **Found during:** Task 1 (build verification)
- **Issue:** The plan's action block specified `weakReferences = CalamityMod@2.2.4 SpiritMod@1.5.0.44` (space-separated). Building with this syntax failed with `System.Exception: Invalid mod reference: CalamityMod@2.2.4 SpiritMod@1.5.0.44` thrown by tModLoader's own `BuildProperties.ModReference.Parse` -- the installed tModLoader binary parses the whole line as one reference when space-separated.
- **Fix:** Changed to comma-separated: `weakReferences = CalamityMod@2.2.4, SpiritMod@1.5.0.44`.
- **Files modified:** `build.txt`
- **Verification:** `dotnet build BossArenaSubWorld.csproj` exits 0 after the fix (it had failed with exit 1 / `TML002` before).
- **Committed in:** `48bc73e` (Task 1 commit)

**2. [Rule 1 - Bug] Removed `--` from an XML comment in the csproj edit**
- **Found during:** Task 1 (build verification)
- **Issue:** The plan's illustrative `<Reference>` XML comment block for SpiritMod contained a literal `--` ("...to extract fresh -- see 05-RESEARCH.md..."), which is invalid inside an XML comment (`An XML comment cannot contain '--'`) and made the `.csproj` fail to load entirely (`MSB4025`).
- **Fix:** Replaced `-- see` with `, see` in the comment text; no semantic change to the documentation.
- **Files modified:** `BossArenaSubWorld.csproj`
- **Verification:** `dotnet build BossArenaSubWorld.csproj` loads and builds successfully after the fix.
- **Committed in:** `48bc73e` (Task 1 commit)

---

**Total deviations:** 2 auto-fixed (both Rule 1 - blocking build bugs in the plan's illustrative snippets, not design changes)
**Impact on plan:** Both fixes were pure syntax corrections required to make the plan's own specified content actually compile/build; no change to the registered behavior, design, or file scope.

## Issues Encountered

**Plan/acceptance-criteria self-inconsistency (not fixed, documented only):** Task 2's acceptance criteria include "Does NOT contain the string: HandleBossSyncing", but the plan's own verbatim action code block for `Integrations/SpiritIntegration.cs` includes the comment `// Pitfall C: NEVER call BossDownedTrackingIO.HandleBossSyncing(BitsByte) as a shortcut here...`, which necessarily contains that literal string. The functional intent of the criterion ("this call is deliberately never made") is satisfied -- `HandleBossSyncing` is never invoked anywhere in the file, only named in an explanatory warning comment documenting Pitfall C (a comment the plan's own `read_first`/`action` guidance explicitly required). Kept the comment as specified by the plan's action block since removing it would remove mandated pitfall documentation for a purely literal (not functional) string-match technicality. Flagging here for visibility rather than silently deviating from the plan's own verbatim code.

## User Setup Required

None - no external service configuration required. `Libs/SpiritMod.dll` is a local, gitignored copy already sourced from the pre-existing decompiled `ModReader/SpiritMod/` tree; no download or account setup needed.

## Next Phase Readiness

- Code-level registration of `spirit:infernon` is complete and builds cleanly (`dotnet build BossArenaSubWorld.csproj` exits 0).
- Live in-game verification (fresh test world with SpiritMod + SubworldLibrary + BossArenaSubWorld enabled, plus a SpiritMod-disabled load-safety checkpoint) is explicitly deferred to Plan 02 (D-05), consistent with this plan's scope boundary.
- No blockers identified for Plan 02.

---
*Phase: 05-spirit-integration*
*Completed: 2026-08-13*

## Self-Check: PASSED

- FOUND: Integrations/SpiritIntegration.cs
- FOUND: build.txt
- FOUND: BossArenaSubWorld.csproj
- FOUND commit: 48bc73e
- FOUND commit: 0e96c4e
