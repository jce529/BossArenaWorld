---
phase: 09-biome-dependent-subworld-coverage
plan: 07
subsystem: infra
tags: [tmodloader, jit-safety, weakreferences, calamitymod, spiritmod, subworldlibrary]

# Dependency graph
requires:
  - phase: 09-04
    provides: Astral (CalamityMod) and Briar (SpiritMod) Subworld/GenPass pairs
  - phase: 09-05
    provides: Debug/BiomeArenaDebugCommands.cs temporary entry mechanism
  - phase: 09-06
    provides: live Zone/Biome flag verification for all 7 kept biome subworlds
provides:
  - Empirical, live-confirmed proof that Astral/Briar Subworld/GenPass pairs are JIT-safe with CalamityMod/SpiritMod independently disabled
  - Fixed the actual JIT-safety gap the checkpoint was designed to catch (missing [JITWhenModsEnabled] attributes)
  - D-02 restored (Debug/BiomeArenaDebugCommands.cs deleted, no permanent new player-facing entry point remains)
affects: [phase-6, phase-7, phase-8]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "A non-ModType class (e.g. a GenPass) being constructed only lazily inside a ModType's Tasks getter is NOT sufficient JIT protection on its own -- every method touching a weak-referenced mod's types must still carry its own [JITWhenModsEnabled(\"ModName\")] attribute, because tModLoader's AssemblyManager.JITAssembliesAsync JIT-prefilters every method in the assembly regardless of call-reachability."

key-files:
  created: []
  modified:
    - Subworlds/AstralPlatformPass.cs
    - Subworlds/BriarPlatformPass.cs
  deleted:
    - Debug/BiomeArenaDebugCommands.cs

key-decisions:
  - "AstralPlatformPass.ApplyPass and BriarPlatformPass.ApplyPass both required explicit [JITWhenModsEnabled] attributes -- lazy-construction-only reasoning (documented in the original Plan 04 class comments) was empirically disproven by a live JITException"

patterns-established:
  - "Per-method [JITWhenModsEnabled] tagging is required on every method (not just ModSystem registration methods) that touches a weak-referenced mod's types, regardless of what class contains it or how lazily that class is constructed"

requirements-completed: [ARENA-01]

# Metrics
duration: ~25min (excluding live in-game test wait time)
completed: 2026-08-14
---

# Phase 09 Plan 07: CalamityMod/SpiritMod-Disabled Safety Checkpoint + Debug Cleanup Summary

**Live JIT-safety checkpoint for Astral/Briar caught and fixed a real crash (missing `[JITWhenModsEnabled]` on both `GenPass.ApplyPass` overrides), then re-verified clean before deleting the temporary debug entry mechanism to restore D-02 compliance.**

## Performance

- **Duration:** ~25 min of active execution (fix, rebuild, cleanup, docs) -- excludes time spent by the user running the two live in-game checkpoint rounds
- **Started:** 2026-08-14 (continuation of Wave 4, immediately after 09-06)
- **Completed:** 2026-08-14T13:37:46+09:00
- **Tasks:** 2 (1 checkpoint + 1 auto)
- **Files modified:** 3 (2 fixed, 1 deleted)

## Accomplishments

- Live-confirmed the mod loads and runs safely with CalamityMod disabled AND with SpiritMod disabled, with no JIT crash and no exceptions in `Logs/client.log` naming the Astral/Briar Subworld or PlatformPass classes -- on the second attempt, after fixing a real bug the first attempt caught
- Found and fixed a genuine JIT-safety defect in the Plan 04-built Astral/Briar `GenPass` pairs: `AstralPlatformPass.ApplyPass` and `BriarPlatformPass.ApplyPass` referenced Calamity/Spirit types but had no `[JITWhenModsEnabled]` attribute, relying instead on incorrect reasoning (being a non-`ModType` lazily constructed inside the paired `Subworld.Tasks` getter) that a live CalamityMod-disabled test empirically disproved
- Restored D-02 ("no new permanent player-facing entry point") by deleting `Debug/BiomeArenaDebugCommands.cs` now that all Phase 9 live verification (09-06's 3 checkpoints + 09-07's 2 checkpoints) is complete
- Confirmed `dotnet build BossArenaSubWorld.csproj` exits 0 after the deletion, with all 18 expected biome-related Subworld/GenPass files intact (7 kept biome pairs from Plans 01-04 = 14 files, plus the 2 pre-existing Phase 1/4 pairs = 4 files)
- ARENA-01 is now confirmed satisfied end-to-end for the arena-construction/JIT-safety half of the claim (7 biome-variant subworlds exist, generate correctly, satisfy their target Zone/Biome flag, and load safely with their source mod disabled); boss-classification-and-routing across Phases 6-8 remains the outstanding half

## Task Commits

Each task was committed atomically:

1. **Task 1: Live CalamityMod-disabled and SpiritMod-disabled safety checkpoint** -- mid-checkpoint fix committed as `ed8eba3` (fix); the checkpoint itself is a live verification, not a repo-modifying task
2. **Task 2: Remove the temporary debug entry mechanism** -- `73e8299` (chore)

**Plan metadata:** (this commit, docs: complete plan)

## Files Created/Modified

- `Subworlds/AstralPlatformPass.cs` - Added `[JITWhenModsEnabled("CalamityMod")]` to `ApplyPass`, with an in-code note correcting the prior lazy-construction-only JIT-safety claim
- `Subworlds/BriarPlatformPass.cs` - Added `[JITWhenModsEnabled("SpiritMod")]` to `ApplyPass` proactively (same defect pattern, fixed before it could crash Part B)
- `Debug/BiomeArenaDebugCommands.cs` - Deleted entirely (temporary Plan 05 tooling, no longer needed now that all live verification is complete)

## Decisions Made

- **Mid-checkpoint architectural correction, not a Rule 4 escalation:** the missing `[JITWhenModsEnabled]` attributes were a straightforward Rule 1 bug fix (existing pattern already established in `Integrations/CalamityIntegration.cs`/`Integrations/SpiritIntegration.cs`; applying the same known-correct pattern to two more files, no new architecture). Fixed inline per Rule 1, not escalated.
- Applied the fix to `BriarPlatformPass.cs` proactively even though only `AstralPlatformPass.cs` had actually crashed in Part A -- both files shared byte-for-byte the same flawed class-level reasoning from Plan 04, so waiting for Part B to also crash before fixing it would have been redundant risk.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Missing `[JITWhenModsEnabled]` attributes caused a real JITException**
- **Found during:** Task 1 (live CalamityMod-disabled checkpoint, Part A)
- **Issue:** `AstralPlatformPass.ApplyPass` referenced `CalamityMod.Tiles.Astral.AstralStone`/`AstralGrass` types with no `[JITWhenModsEnabled("CalamityMod")]` guard. tModLoader's `AssemblyManager.JITAssembliesAsync` JIT-prefilters every method in the assembly regardless of call-reachability, so the class-level comment's "lazy construction only" reasoning was insufficient -- CalamityMod-disabled load crashed with a real `Terraria.ModLoader.Exceptions.JITException` naming `AstralPlatformPass.ApplyPass`.
- **Fix:** Added `[JITWhenModsEnabled("CalamityMod")]` to `AstralPlatformPass.ApplyPass` and `[JITWhenModsEnabled("SpiritMod")]` to `BriarPlatformPass.ApplyPass` (proactive, same root cause), mirroring the established per-method tagging discipline already used in `Integrations/CalamityIntegration.cs`/`Integrations/SpiritIntegration.cs`.
- **Files modified:** `Subworlds/AstralPlatformPass.cs`, `Subworlds/BriarPlatformPass.cs`
- **Verification:** `dotnet build BossArenaSubWorld.csproj` compiled cleanly (0 errors); user re-ran both Part A and Part B live and confirmed "mod-disabled safety verified" -- no JIT crash, no relevant exceptions in `Logs/client.log`, with both mods re-enabled afterward
- **Committed in:** `ed8eba3`

---

**Total deviations:** 1 auto-fixed (Rule 1 - bug)
**Impact on plan:** This is exactly the kind of defect this checkpoint exists to catch -- code that looked correctly isolated on review but was not actually JIT-safe at runtime. The plan's own text called this out explicitly ("a JIT crash from a leaked Calamity/Spirit type reference... would only surface with the mod actually disabled, not via dotnet build alone"). No scope creep; fix is a direct application of an already-established codebase pattern.

## Issues Encountered

- Two intermediate `dotnet build` attempts failed with `TML003: Please close tModLoader or disable the mod in-game to build mods directly` -- a file lock on `BossArenaSubWorld.tmod` because tModLoader (running as a `dotnet.exe` host process, not a distinctly-named executable) was still open from the live test session. Not a code defect both times; resolved once tModLoader was closed, after which builds succeeded with exit code 0.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- ARENA-01's arena-construction/JIT-safety half is fully closed: all 7 kept biome-variant subworlds (Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) generate correctly, satisfy their target Zone/Biome flag, and are now empirically confirmed JIT-safe with their respective source mod disabled (Calamity for Astral, Spirit for Briar; the other 5 have no weak-mod-reference exposure to test).
- No permanent player-facing debug/dev-only entry points remain in the codebase (`Debug/` is now empty) -- D-02 restored.
- Phase 9 is now fully complete (all 7 plans: 01-07). Phases 6-8 (boss-classification-and-routing for Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak) remain the next unplanned work, per ROADMAP.md's existing ordering; Dungeon and Sulphurous Sea biome coverage remains explicitly deferred (D-07) until a future phase reinstates it.

---
*Phase: 09-biome-dependent-subworld-coverage*
*Completed: 2026-08-14*

## Self-Check: PASSED

- FOUND: Subworlds/AstralPlatformPass.cs
- FOUND: Subworlds/BriarPlatformPass.cs
- CONFIRMED DELETED: Debug/BiomeArenaDebugCommands.cs
- FOUND commit: ed8eba3
- FOUND commit: 73e8299
- FOUND: 09-07-SUMMARY.md (this file)
