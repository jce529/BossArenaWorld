---
phase: 07-noxusboss-continentofjourney-daybreak-integration
plan: 01
subsystem: infra
tags: [tmodloader, continentofjourney, homeward-journey, weakreferences, jitwhenmodsenabled, bossregistry]

# Dependency graph
requires:
  - phase: 06-redemption-catalystmod-integration
    provides: The direct public-static-field-write BossDefinition/JITWhenModsEnabled/named-method registration pattern (RedemptionIntegration.cs/CatalystIntegration.cs) mirrored exactly here
provides:
  - Libs/ContinentOfJourney.dll wired as a compile-time-only weak reference (build.txt + csproj)
  - Integrations/HomewardJourneyIntegration.cs registering continentofjourney:goblin_chariot into BossRegistry/SummonItemRegistry
affects: [07-02-PLAN.md live verification, Phase 8 full-pipeline verification]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Sixth weakReferences/.csproj Reference block mirrors the existing five (CalamityMod/SpiritMod/Redemption/CatalystMod pattern), zero shape changes"
    - "Direct public-static-field write (ContinentOfJourney.DownedBossSystem.downedGoblinChariot via NPC.SetEventFlagCleared), identical shape to Redemption/CatalystMod (Phase 6), no reflection needed"

key-files:
  created: [Integrations/HomewardJourneyIntegration.cs]
  modified: [build.txt, BossArenaSubWorld.csproj]

key-decisions:
  - "Fixed a compile-blocking XML comment double-dash (Rule 3) in the new csproj Reference block's doc comment -- \"Homeward Journey\" -- the internal... contained a literal '--' which XML comments forbid, causing MSB4025 project-load failure; changed to a semicolon."

patterns-established:
  - "Fifth and final v1 mod integration follows the exact same PostSetupContent-guard + named-static-method + per-method [JITWhenModsEnabled] discipline as Phase 4/5/6, closing MOD-06 at the code level with zero new architectural work."

requirements-completed: [MOD-06]

# Metrics
duration: 12min
completed: 2026-08-14
---

# Phase 07 Plan 01: ContinentOfJourney (Homeward Journey) Weak Reference Wiring + Goblin Chariot Registration Summary

**ContinentOfJourney.DownedBossSystem.downedGoblinChariot registered into BossRegistry via direct public-static-field write, closing MOD-06 as the fifth and final v1 mod integration**

## Performance

- **Duration:** 12 min
- **Started:** 2026-08-14T07:53:00Z
- **Completed:** 2026-08-14T07:55:10Z
- **Tasks:** 2
- **Files modified:** 3 (2 modified, 1 created)

## Accomplishments
- `Libs/ContinentOfJourney.dll` wired as a compile-time-only weak reference in `build.txt` (`ContinentOfJourney@0.8.70.88`) and `BossArenaSubWorld.csproj` (sixth `<Reference>` block, mirroring the existing five)
- `Integrations/HomewardJourneyIntegration.cs` created, registering `continentofjourney:goblin_chariot` into `BossRegistry` using `ContinentOfJourney.DownedBossSystem.downedGoblinChariot` (a public static bool field, written directly via `NPC.SetEventFlagCleared(ref ..., -1)` -- no reflection)
- `SummonItemRegistry.Register` maps `PurpleFlareGun` to `GoblinChariot`, no `canSummon` eligibility delegate needed (confirmed no biome/location gate in `CanUseItem()`)
- No `BossArenaRoutingRegistry.Register<T>()` call -- confirmed no `Zone*`/`CheckActive` dependency, falls back to default `BossArenaSubworld`
- `dotnet build BossArenaSubWorld.csproj` exits 0 after both tasks

## Task Commits

Each task was committed atomically:

1. **Task 1: Wire ContinentOfJourney weak reference in build.txt and BossArenaSubWorld.csproj** - `7755dfe` (chore)
2. **Task 2: Register Goblin Chariot via Integrations/HomewardJourneyIntegration.cs** - `6f17a07` (feat)

**Plan metadata:** (pending, see final commit below)

## Files Created/Modified
- `build.txt` - Added `ContinentOfJourney@0.8.70.88` to the comma-separated `weakReferences` line
- `BossArenaSubWorld.csproj` - Added a sixth `<Reference Include="ContinentOfJourney">` block (compile-time-only, `Private=false`, `Condition="Exists('Libs\ContinentOfJourney.dll')"`)
- `Integrations/HomewardJourneyIntegration.cs` - New `ModSystem`: `PostSetupContent()` guards on `ModLoader.HasMod("ContinentOfJourney")`, then `RegisterGoblinChariot()` (JIT-guarded) registers the summon item and the `BossDefinition` with named `ApplyGoblinChariotDowned`/`IsGoblinChariotDowned` static methods (no inline lambdas)

## Decisions Made
- Used the mod's confirmed internal name `"ContinentOfJourney"` (not the Workshop display name `"HomewardJourney"`) in every `ModLoader.HasMod`/`[JITWhenModsEnabled]` call, per 07-RESEARCH.md Pitfall 1
- No `canSummon` eligibility delegate for `PurpleFlareGun` (unlike CatalystMod's Astrageldon in Phase 6) -- research confirmed no lockout condition exists in the real `CanUseItem()`
- No `BossArenaRoutingRegistry` registration -- Goblin Chariot has no biome dependency, falls back to the default subworld automatically

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed invalid XML comment (double-dash) causing MSB4025 project-load failure**
- **Found during:** Task 1 (`dotnet restore` after editing `BossArenaSubWorld.csproj`)
- **Issue:** The new `<Reference Include="ContinentOfJourney">` doc comment used `"Homeward Journey" -- the internal mod name...`, and XML comments forbid a literal `--` anywhere in their body. `dotnet restore` failed with `MSB4025: An XML comment cannot contain '--', and '-' cannot be the last character.`
- **Fix:** Changed the em-dash-style `--` separator to a semicolon (`"Homeward Journey"; the internal mod name...`)
- **Files modified:** `BossArenaSubWorld.csproj`
- **Verification:** `dotnet restore` and `dotnet build BossArenaSubWorld.csproj` both succeeded (exit 0) after the fix
- **Committed in:** `7755dfe` (Task 1 commit, fix folded in before commit since the file was not yet staged/committed)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Necessary to unblock the build; no scope creep, purely a comment-syntax fix caused by my own edit.

## Issues Encountered
None beyond the auto-fixed blocking issue above.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Code-level MOD-06 registration is complete and builds clean with `ContinentOfJourney` present as a compile-time reference
- Live in-game verification (killing Goblin Chariot in the subworld, using the carrier item in the main world, confirming `downedGoblinChariot` + BossChecklist recognition) is explicitly deferred to Plan 02 per this plan's objective
- ContinentOfJourney must be re-subscribed/enabled in the local `Mods\` folder before Plan 02's live verification checkpoint can run (per 07-RESEARCH.md Environment Availability note) -- this does not block Plan 02's code, only its live test

---
*Phase: 07-noxusboss-continentofjourney-daybreak-integration*
*Completed: 2026-08-14*

## Self-Check: PASSED

All created files and commit hashes verified to exist:
- FOUND: Integrations/HomewardJourneyIntegration.cs
- FOUND: 7755dfe (Task 1 commit)
- FOUND: 6f17a07 (Task 2 commit)
- FOUND: build.txt contains "ContinentOfJourney@0.8.70.88"
- FOUND: BossArenaSubWorld.csproj contains 'Reference Include="ContinentOfJourney"'
