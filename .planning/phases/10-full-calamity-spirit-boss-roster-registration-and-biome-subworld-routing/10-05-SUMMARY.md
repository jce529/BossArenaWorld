---
phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing
plan: 05
subsystem: cross-mod-integration
tags: [calamity, infernummode, weakreferences, jitwhenmodsenabled, bossregistry]

# Dependency graph
requires:
  - phase: 10-04
    provides: 12-of-12 Calamity boss registration pattern (RegisterX/IsXDowned/ApplyXDowned triplet), InfernumMode-presence gating precedent (Providence/Profaned Guardians/Astrum Deus/Astrum Aureus)
provides:
  - "InfernumMode wired as a new weak reference (build.txt, BossArenaSubWorld.csproj, Libs/InfernumMode.dll)"
  - "The Old Duke registered, gated to CalamityMod+InfernumMode both present"
  - "All 12 Calamity boss registrations this phase covers are now code-complete"
affects: [10-06, phase-10-live-verification]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "First boss requiring an AND-gate (both CalamityMod AND InfernumMode present) rather than an absent/present single-mod gate -- internal guard checks the LESS-guaranteed mod (InfernumMode), not the already-guaranteed one (CalamityMod)"

key-files:
  created: []
  modified:
    - build.txt
    - BossArenaSubWorld.csproj
    - Integrations/CalamityIntegration.cs

key-decisions:
  - "RegisterOldDuke()'s internal guard checks ModLoader.HasMod(\"InfernumMode\"), not HasMod(\"CalamityMod\") -- the latter is already guaranteed true by CalamityIntegration.PostSetupContent()'s outer guard and would be a no-op, letting the method JIT its InfernumMode type reference in the common CalamityMod-only configuration"
  - "No BossArenaRoutingRegistry call for The Old Duke -- 09-ALTAR-BIOME-REFERENCE.md Open Item 3 resolved via 10-RESEARCH.md decompile: OldDuke.cs's AI has zero Sulphurous-Sea Zone references, falls back to the plain default BossArenaSubworld"

patterns-established:
  - "Pattern: AND-gate registration (both mod A and mod B required) guards on the mod NOT already guaranteed by the enclosing PostSetupContent() check, to avoid a no-op guard that would crash via lazy JIT in the partial-install configuration"

requirements-completed: [ARENA-01]

# Metrics
duration: 5min
completed: 2026-08-14
---

# Phase 10 Plan 05: InfernumMode Weak Reference + The Old Duke Registration Summary

**Wired InfernumMode as a new weak reference and registered The Old Duke (Calamity+Infernum-only boss), completing all 12 Calamity boss registrations this phase covers.**

## Performance

- **Duration:** 5 min
- **Started:** 2026-08-14T23:16:34+09:00
- **Completed:** 2026-08-14T23:21:13+09:00
- **Tasks:** 2
- **Files modified:** 3 (build.txt, BossArenaSubWorld.csproj, Integrations/CalamityIntegration.cs)

## Accomplishments
- `Libs/InfernumMode.dll` v2.0.1.35 wired as a new gitignored compile-time-only weak reference, copied from tModLoader's local `ModAssemblies` build cache (not extracted from a `.tmod`) -- `build.txt`/`BossArenaSubWorld.csproj` updated following the established per-mod pattern, all pre-existing `weakReferences` entries preserved (including `ContinentOfJourney@0.8.70.88`)
- The Old Duke registered via `RegisterOldDuke()`, gated to CalamityMod+InfernumMode both present -- the internal guard correctly checks `HasMod("InfernumMode")` (this plan's revision already fixed the earlier `HasMod("CalamityMod")` bug that a plan-checker pass caught pre-execution, commit `0c90ffa`)
- `ApplyOldDukeDowned` faithfully replays `OldDuke.OnKill()`'s exact side-effect order: Sea King shop-unlock (fired BEFORE the flag flips, passing the OLD value) + `downedBoomerDuke = true` (NOT the non-existent `downedOldDuke`) + `AcidRainEvent.OldDukeHasBeenEncountered = true` + `CalamityNetcode.SyncWorld()`
- All 12 Calamity bosses this phase covers are now registered: Hive Mind (Phase 4) + Devourer of Gods, Yharon, Supreme Witch Calamitas, Dragonfolly (10-02) + Providence, Profaned Guardians, Astrum Deus, Astrum Aureus, Ceaseless Void, Signus, Storm Weaver (10-04) + The Old Duke (this plan)

## Task Commits

Each task was committed atomically:

1. **Task 1: Wire InfernumMode as a new weak reference** - `05e8786` (feat)
2. **Task 2: Register The Old Duke (Infernum-only gate)** - `00d2daa` (feat)

**Plan metadata:** (pending) - docs: complete plan

## Files Created/Modified
- `build.txt` - appended `InfernumMode@2.0.1.35` to `weakReferences`
- `BossArenaSubWorld.csproj` - added `InfernumMode` `<Reference>` block mirroring `CatalystMod`'s pattern
- `Libs/InfernumMode.dll` - copied from `../ModAssemblies/InfernumMode_v2.0.1.35.dll` (gitignored, not committed)
- `Integrations/CalamityIntegration.cs` - added `RegisterOldDuke`/`IsOldDukeDowned`/`ApplyOldDukeDowned`, wired `RegisterOldDuke();` into `PostSetupContent()`

## Decisions Made
- `RegisterOldDuke()`'s internal guard checks `HasMod("InfernumMode")`, not `HasMod("CalamityMod")` -- this was already corrected in the plan file by a prior plan-checker revision pass (commit `0c90ffa`) before this execution began; followed the plan's current text exactly.
- No `BossArenaRoutingRegistry` registration for The Old Duke -- confirmed via 10-RESEARCH.md decompile that `OldDuke.cs`'s AI has zero Sulphurous-Sea Zone flag references; falls back to the plain default `BossArenaSubworld`.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Removed literal `--` from new XML doc comment in BossArenaSubWorld.csproj**
- **Found during:** Task 1 (`dotnet build` verification)
- **Issue:** `dotnet build` failed with `MSB4025: An XML comment cannot contain '--'`. The plan's illustrative doc-comment text for the new `InfernumMode` `<Reference>` block contained `.tmod --` and `build.txt) --` -- literal double-dashes are invalid inside an XML `<!-- -->` comment. This is the same class of bug STATE.md documents being auto-fixed in Phase 7 for `ContinentOfJourney`'s Reference block.
- **Fix:** Replaced the two `--` occurrences with `,`/`;` phrasing that preserves the original meaning without using XML-illegal double-dashes.
- **Files modified:** `BossArenaSubWorld.csproj`
- **Verification:** `dotnet build BossArenaSubWorld.csproj` exits 0 with 0 warnings/0 errors after the fix.
- **Committed in:** `05e8786` (Task 1 commit)

**2. [Setup, not a plan deviation] Copied all `Libs/*.dll` weak-reference DLLs from the main working tree into this worktree**
- **Found during:** Task 1 (before first build attempt)
- **Issue:** This worktree's `Libs/` directory only had `InfernumMode.dll` after Task 1's explicit copy step; `SubworldLibrary.dll`/`CalamityMod.dll`/`SpiritMod.dll`/`Redemption.dll`/`CatalystMod.dll`/`ContinentOfJourney.dll` were missing (known per-worktree setup gap, documented in STATE.md Phase 02/07 notes -- `Libs/*.dll` is gitignored, each worktree needs its own copy).
- **Fix:** Copied all pre-existing `Libs/*.dll` files from the main `BossArenaSubWorld` working tree into this worktree's `Libs/` directory before building.
- **Files modified:** none tracked (gitignored `Libs/` directory only)
- **Verification:** `dotnet build` resolved all references successfully.
- **Committed in:** N/A (gitignored, not committed)

---

**Total deviations:** 1 auto-fixed (1 blocking XML fix) + 1 per-worktree setup step (not a code deviation)
**Impact on plan:** The XML fix was necessary for the build to succeed at all; no scope creep, matches an already-documented precedent from Phase 7. The Libs/ copy is routine per-worktree setup, not a plan deviation.

## Issues Encountered
None beyond the two items documented above.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- All 12 Calamity boss registrations this phase covers are code-complete (`Integrations/CalamityIntegration.cs`).
- `dotnet build BossArenaSubWorld.csproj` passes with 0 warnings/0 errors after both tasks.
- ARENA-01 stays open until Plan 10-06's live in-game verification confirms behavior for the full roster (including The Old Duke's Infernum-only gate: no InfernumMode = no registration; InfernumMode present = registration works) -- this plan only proves code-level correctness, per the plan's own `<verification>` section.
- Next entry point: `10-06-PLAN.md`.

---
*Phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing*
*Completed: 2026-08-14*

## Self-Check: PASSED

- FOUND: Libs/InfernumMode.dll
- FOUND: Integrations/CalamityIntegration.cs
- FOUND: build.txt
- FOUND: BossArenaSubWorld.csproj
- FOUND commit: 05e8786
- FOUND commit: 00d2daa
