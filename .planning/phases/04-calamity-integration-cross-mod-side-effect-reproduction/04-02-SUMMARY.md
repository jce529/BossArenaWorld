---
phase: 04-calamity-integration-cross-mod-side-effect-reproduction
plan: 02
subsystem: cross-mod-integration
tags: [tmodloader, calamity, JITWhenModsEnabled, jit-compilation, live-verification, checkpoint]

# Dependency graph
requires:
  - phase: 04-calamity-integration-cross-mod-side-effect-reproduction
    plan: 01
    provides: "Integrations/CalamityIntegration.cs registering calamity:hive_mind into BossRegistry/SummonItemRegistry, isolated behind [JITWhenModsEnabled(\"CalamityMod\")]"
provides:
  - "Empirical live confirmation that APPLY-02 (netcode/messaging side effect) and APPLY-03 (WorldGen side effect) fire correctly for Hive Mind on a real throwaway Corruption-evil-type world"
  - "Empirical live confirmation that the weakReferences + [JITWhenModsEnabled] isolation boundary holds at runtime with CalamityMod disabled -- AFTER fixing a real inline-lambda JIT-crash bug this checkpoint discovered"
  - "Documented gotcha: delegates passed into [JITWhenModsEnabled]-guarded registration calls must be named, separately-tagged methods, never inline lambdas -- applies to every future per-boss registration (Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak)"
affects: [phase-05-spirit-integration, phase-06-redemption-catalystmod, phase-07-noxusboss-continentofjourney-daybreak]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Named-method-only rule for [JITWhenModsEnabled] delegate registration: inline lambdas referencing a weak-referenced mod's types get hoisted by the C# compiler into a <>c cache-class method that does NOT inherit the enclosing method's [JITWhenModsEnabled] attribute, so tModLoader's JIT prefilter can still eagerly touch it and crash even when the enclosing method is never called"

key-files:
  created: []
  modified:
    - Integrations/CalamityIntegration.cs

key-decisions:
  - "Extracted the IsDowned inline lambda (`() => CalamityMod.DownedBossSystem.downedHiveMind`) into a named, separately-tagged `IsHiveMindDowned()` method after a live CalamityMod-disabled load produced a real JITException naming the compiler-generated <RegisterHiveMind>b__1_0 method -- confirms [JITWhenModsEnabled] isolation must be applied per-generated-method, not just per-enclosing-method, whenever a delegate/lambda referencing weak-referenced-mod types is passed as an argument"
  - "Task 1's live in-game verification (WorldGen ore-conversion + Sky Ore broadcast) was satisfied by evidence already gathered during the immediately-preceding debug session (hivemind-zonecorrupt-despawn-corruption-subworld), which used a fresh throwaway Corruption-evil-type world (not the real save) -- user explicitly confirmed this satisfies Task 1's acceptance criteria rather than requiring a duplicate live test"

requirements-completed: [MOD-01, APPLY-02, APPLY-03]

# Metrics
duration: 25min
completed: 2026-08-13
---

# Phase 04 Plan 02: Live Verification Checkpoints (WorldGen/Netcode Side Effects + Calamity-Disabled Load Safety) Summary

**Two live in-game checkpoints closing out Phase 4: confirmed Hive Mind's real WorldGen/netcode/messaging side effects fire correctly via the carrier-item pipeline, and found + fixed a real JIT-crash bug in the CalamityMod isolation boundary during the Calamity-disabled load-safety test.**

## Performance

- **Duration:** ~25 min (across the checkpoint dialogue: evidence review, clarifying question, live retest cycle after the fix)
- **Tasks:** 2 completed (both checkpoint tasks), plus 1 deviation-driven code fix task
- **Files modified:** 1 (Integrations/CalamityIntegration.cs)

## Accomplishments

- **Task 1 (D-04, APPLY-02, APPLY-03) -- confirmed via evidence from the immediately-preceding resolved debug session**, not a duplicate live test: the user explicitly confirmed the debug session's live kill->carry->apply cycle used a freshly-created throwaway world (not the real save "HiPo's_Terrarium") with Evil Type = Corruption, satisfying Task 1's acceptance criteria exactly. That session confirmed:
  - BossCoreItem dropped from Hive Mind killed inside `BossArenaCorruptionSubworld`
  - Using the BossCoreItem in the main world produced BOTH the "Boss credential applied: calamity:hive_mind" message AND the Calamity Sky Ore broadcast ("The sky is glittering with cyan light.")
  - A real, visible Aerialite Ore (Disenchanted) -> Aerialite Ore tile conversion occurred in the main world
  - `downedHiveMind` persists across save/reload, and no duplicate message/conversion occurs on repeat BossCoreItem use or arena re-entry (idempotency holds)
- **Task 2 (D-05, Success Criterion 4) -- first attempt crashed, second attempt (after a code fix) passed.**
  - First attempt: disabling CalamityMod and loading tModLoader produced a real `Terraria.ModLoader.Exceptions.JITException` naming `BossArenaSubWorld.Integrations.CalamityIntegration+<>c.<RegisterHiveMind>b__1_0` -- CalamityMod could not be resolved.
  - Root cause: the inline lambda `IsDowned: () => CalamityMod.DownedBossSystem.downedHiveMind` inside `RegisterHiveMind()` (a `[JITWhenModsEnabled("CalamityMod")]`-tagged method) is compiled by the C# compiler into a separate method on a compiler-generated `<>c` cache class. That generated method does **not** inherit the `[JITWhenModsEnabled]` attribute from its enclosing method, so tModLoader's JIT prefilter still eagerly attempted to JIT it even though `RegisterHiveMind()` itself is never invoked when CalamityMod is absent (the `PostSetupContent()` guard prevents the call, but not the assembly-wide prefilter scan).
  - Fix: extracted the lambda into a named, separately `[JITWhenModsEnabled("CalamityMod")]`-tagged static method `IsHiveMindDowned()`, and passed the method group instead of an inline lambda. Scanned the rest of `Integrations/CalamityIntegration.cs` (the only file in `Integrations/`) for other inline lambdas referencing Calamity types -- none found; `ApplyHiveMindDowned` was already a plain tagged method, not a lambda.
  - `dotnet build BossArenaSubWorld.csproj` passed with 0 warnings/0 errors after the fix.
  - Second attempt (retest): user confirmed **"load-safety verified"** -- tModLoader loaded cleanly with CalamityMod disabled, no JIT crash/exception dialog, Test1 tile and King Slime pipeline unaffected, `Logs/client.log` clean of CalamityMod/TypeLoadException/MissingMethodException entries. CalamityMod re-enabled afterward.

## Task Commits

This plan's tasks are both live checkpoints (no repo files modified by design), except for one deviation-driven fix commit made mid-plan when Task 2's first attempt uncovered a real bug:

1. **Deviation fix (found during Task 2's first live attempt): extract Hive Mind IsDowned lambda to avoid JIT crash** - `0e19600` (fix)

## Files Created/Modified

- `Integrations/CalamityIntegration.cs` - Extracted the inline `IsDowned` lambda into a named, `[JITWhenModsEnabled("CalamityMod")]`-tagged `IsHiveMindDowned()` method; added an explanatory comment block documenting the compiler-generated `<>c` cache-class JIT-inheritance gotcha for future per-boss registrations.

## Decisions Made

- Delegates/lambdas passed as arguments into any `[JITWhenModsEnabled]`-guarded registration call (e.g. `BossDefinition`'s `ApplyDowned`/`IsDowned` `Func`/`Action` fields) must always be named, separately-tagged methods -- never inline lambdas -- because the C# compiler hoists inline lambdas into compiler-generated cache-class methods that do not inherit the enclosing method's JIT-guard attribute. This is now a documented pattern for Phase 5 (Spirit), Phase 6 (Redemption/CatalystMod), and Phase 7 (NoxusBoss/ContinentOfJourney/Daybreak), all of which will register similar `BossDefinition`s.
- Task 1's live verification requirement was satisfied by evidence gathered during the immediately-preceding debug session rather than a duplicate live test, after explicit user confirmation that the debug session's test world was a fresh, throwaway, Corruption-evil-type world matching Task 1's acceptance criteria exactly (not the real save).

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug, discovered live during Task 2] Inline lambda caused a real JITException when CalamityMod is disabled**
- **Found during:** Task 2's first live attempt (CalamityMod-disabled load test)
- **Issue:** `RegisterHiveMind()`'s `IsDowned: () => CalamityMod.DownedBossSystem.downedHiveMind` inline lambda was compiled into a `<>c` cache-class method that does not inherit `[JITWhenModsEnabled("CalamityMod")]` from its enclosing method, so tModLoader's JIT prefilter eagerly JIT'd it and crashed trying to resolve `CalamityMod` types with the mod disabled.
- **Fix:** Extracted the lambda into a named `[JITWhenModsEnabled("CalamityMod")]`-tagged static method `IsHiveMindDowned()`, passed as a method group instead of an inline lambda. Scanned the whole file for other occurrences of the same pattern -- none found.
- **Files modified:** `Integrations/CalamityIntegration.cs`
- **Commit:** `0e19600`

This is a plan-scope deviation from the plan's stated `files_modified: []` (the plan expected this to be a pure verification-only checkpoint with zero code changes) -- justified because Task 2's own live verification step is exactly what surfaced the bug, and fixing it was required to actually pass Task 2's acceptance criteria.

## Issues Encountered

None beyond the JIT-crash deviation documented above, which was fully resolved and re-verified live.

## User Setup Required

None -- CalamityMod was disabled and re-enabled by the user as part of the Task 2 checkpoint procedure itself; no persistent environment change remains.

## Checkpoint Results

### Task 1: Live WorldGen + netcode/messaging side-effect verification (D-04, APPLY-02, APPLY-03)

**Result: PASSED** (satisfied by evidence from the resolved debug session `hivemind-zonecorrupt-despawn-corruption-subworld.md`, user-confirmed to meet this task's exact acceptance criteria -- fresh throwaway world, Evil Type = Corruption, full kill->carry->apply cycle, both chat messages, real Aerialite conversion, all confirmed live.)

### Task 2: Live Calamity-disabled load-safety checkpoint (D-05, Success Criterion 4)

**Result: PASSED on retest**, after fixing the inline-lambda JIT-crash bug (commit `0e19600`). First attempt failed with a real JITException; user confirmed "load-safety verified" on the second attempt following the fix.

## Next Phase Readiness

- Phase 4's three owned requirements (MOD-01, APPLY-02, APPLY-03) are now all empirically confirmed end-to-end via real in-game testing, not just code review -- closing out Phase 4 per REQUIREMENTS.md's Traceability table.
- The named-method-only rule for `[JITWhenModsEnabled]`-guarded delegate registration is now a documented, must-follow pattern for every future content-mod integration (Phase 5 Spirit, Phase 6 Redemption/CatalystMod, Phase 7 NoxusBoss/ContinentOfJourney/Daybreak) -- each of those plans should explicitly avoid inline lambdas in their `BossDefinition` registration calls.
- No blockers identified for Phase 5.

---
*Phase: 04-calamity-integration-cross-mod-side-effect-reproduction*
*Completed: 2026-08-13*

## Self-Check: PASSED

- FOUND: Integrations/CalamityIntegration.cs
- FOUND: commit 0e19600
- FOUND: .planning/phases/04-calamity-integration-cross-mod-side-effect-reproduction/04-02-SUMMARY.md
