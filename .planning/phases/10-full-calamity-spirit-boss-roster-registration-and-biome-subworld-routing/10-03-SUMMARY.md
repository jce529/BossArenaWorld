---
phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing
plan: 03
subsystem: boss-registration
tags: [tmodloader, reflection, spiritmod, boss-registry, forced-time, biome-routing]

# Dependency graph
requires:
  - phase: 10-01
    provides: "ForcedTimeSystem.RegisterForceNight, BossArenaRoutingRegistry.Register<T>() shared foundation"
  - phase: 05
    provides: "SpiritIntegration.cs Infernon precedent -- cached _downedField reflection, dual-type BossDefinition pattern"
  - phase: 09
    provides: "BossArenaSpaceSubworld, BossArenaDesertSubworld, BossArenaBriarSubworld biome subworlds"
provides:
  - "6 new Spirit boss registrations: Ancient Avian, Scarabeus, Vinewrath Bane, Moon Jelly Wizard, Dusking, Atlas"
  - "Shared ApplyGenericSpiritDowned<T>() generic write helper reused by all 6"
  - "Completes Spirit's full 7-boss roster (Infernon + these 6)"
affects: [10-06 live verification, ARENA-01 requirement closure]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Generic reflection write helper (ApplyGenericSpiritDowned<T>()) generalizing a per-boss reflection pattern once 6+ bosses share the identical write path"
    - "Dual-type BossDefinition for two-phase fights (ReachBoss/ReachBoss1), second precedent after Infernon/InfernoSkull"

key-files:
  created: []
  modified:
    - "Integrations/SpiritIntegration.cs"

key-decisions:
  - "Vinewrath Bane registers both ReachBoss and ReachBoss1 NPC types under one BossDefinition since SpiritMod.MyWorld.DownedVinewrath reads ReachBoss1 specifically (phase-2 transform), not the real summon target ReachBoss"
  - "Scarabeus routed to BossArenaDesertSubworld for functional reasons (1/3 damage-scaling penalty outside Desert), not just theme"
  - "Ancient Avian and Vinewrath Bane routed to Space/Briar subworlds for thematic reasons only (no despawn/damage dependency found)"
  - "Moon Jelly Wizard and Dusking both call ForcedTimeSystem.RegisterForceNight since their AI despawns on Main.dayTime == true"
  - "Atlas and Moon Jelly Wizard/Dusking use the plain default BossArenaSubworld -- no biome dependency found"

patterns-established:
  - "When 3+ bosses from the same mod share an identical downed-write mechanism, generalize into one shared generic helper (ApplyGenericSpiritDowned<T>()) instead of repeating per-boss reflection boilerplate"

requirements-completed: []  # ARENA-01 listed in this plan's frontmatter but NOT marked complete in REQUIREMENTS.md -- it remains a multi-plan requirement; Phase 7's live-verification checkpoint and Phase 10 plans 10-02/10-04/10-05/10-06 must also land first. This plan contributes Spirit's 6-boss remainder toward it.

# Metrics
duration: 4min
completed: 2026-08-14
---

# Phase 10 Plan 03: Spirit Full Roster Registration (6 Bosses) Summary

**Registered Ancient Avian, Scarabeus, Vinewrath Bane, Moon Jelly Wizard, Dusking, and Atlas in `Integrations/SpiritIntegration.cs`, generalizing Infernon's cached-reflection write path into a shared `ApplyGenericSpiritDowned<T>()` helper, completing Spirit's full 7-boss roster.**

## Performance

- **Duration:** 4 min (build/commit time; excludes file-read/context-load time)
- **Started:** 2026-08-14T14:01:00Z (approx, first commit 23:01:29+09:00)
- **Completed:** 2026-08-14T14:02:24Z (last commit 23:02:24+09:00)
- **Tasks:** 3 completed
- **Files modified:** 1

## Accomplishments
- Added `ApplyGenericSpiritDowned<T>()` shared generic write helper reusing the cached `_downedField` `FieldInfo` established by `RegisterInfernon()`, eliminating per-boss reflection boilerplate for the 6 new bosses
- Registered all 6 remaining Spirit bosses (`spirit:ancient_avian`, `spirit:scarabeus`, `spirit:vinewrath_bane`, `spirit:atlas`, `spirit:moon_jelly_wizard`, `spirit:dusking`) end-to-end (summon item, BossDefinition, biome/time routing)
- Applied the Vinewrath Bane dual-type correction (ReachBoss + ReachBoss1) confirmed via decompile this session, mirroring the existing Infernon/InfernoSkull precedent
- Wired Moon Jelly Wizard and Dusking to `ForcedTimeSystem.RegisterForceNight` for their confirmed daytime-despawn AI check
- Routed Scarabeus to `BossArenaDesertSubworld` (functional damage-scaling reasons), Ancient Avian to `BossArenaSpaceSubworld` and Vinewrath Bane to `BossArenaBriarSubworld` (thematic)

## Task Commits

Each task was committed atomically:

1. **Task 1: Add shared generic write helper + register Ancient Avian + Scarabeus** - `baeb553` (feat)
2. **Task 2: Register Vinewrath Bane (dual-type) + Atlas** - `7e2c7cb` (feat)
3. **Task 3: Register Moon Jelly Wizard + Dusking (forced night)** - `5f8ea8b` (feat)

_Note: parallel-executor plan run alongside sibling plan 10-02 (CalamityIntegration.cs) in an isolated worktree; `--no-verify` used on all commits per orchestrator instructions to avoid pre-commit hook contention._

## Files Created/Modified
- `Integrations/SpiritIntegration.cs` - Extended from 1-boss (Infernon) to full 7-boss Spirit roster; added `ApplyGenericSpiritDowned<T>()` and 6 `RegisterX`/`IsXDowned`/`ApplyXDowned` triplets, all `[JITWhenModsEnabled("SpiritMod")]`-tagged

## Decisions Made
- Shared `ApplyGenericSpiritDowned<T>()` generic helper introduced (Architecture Pattern 3 per 10-RESEARCH.md) since none of the 6 new bosses override `OnKill()` themselves or have any WorldGen/player-scoped side effect beyond the flag write -- unlike Infernon's Hellstone tile-ring replay, which remains Infernon-specific and untouched by this plan
- Vinewrath Bane's `BossDefinition` registers both `ReachBoss` and `ReachBoss1` NPC types; `ApplyVinewrathBaneDowned` only writes `ReachBoss1`'s dictionary key since that's the one `SpiritMod.MyWorld.DownedVinewrath` actually reads
- Scarabeus's Desert routing is functional (real 1/3 damage-scaling penalty when `!player.ZoneDesert`), documented distinctly from Ancient Avian/Vinewrath Bane's purely thematic routing in code comments

## Deviations from Plan

None - plan executed exactly as written. All illustrative code blocks from `10-03-PLAN.md` compiled and built successfully without modification.

One environment-only adjustment (not a deviation from plan content): this worktree's `Libs/*.dll` compile-time references (`CalamityMod.dll`, `SpiritMod.dll`, `SubworldLibrary.dll`, etc.) were missing at session start -- a known per-worktree setup gap documented in STATE.md since Phase 02/06. Copied from the main working tree before the first build; these remain gitignored, not committed.

## Issues Encountered
None beyond the known per-worktree `Libs/*.dll` setup gap noted above, which is not a code issue.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Spirit's full 7-boss roster (Infernon + these 6) is now code-complete and builds with 0 warnings/0 errors
- Live in-game verification (real summon items, kills, BossCoreItem drops/use, forced-night persistence across a full fight) is explicitly deferred to Plan 10-06 per this plan's `<verification>` section, not this plan
- `Integrations/SpiritIntegration.cs` is ready for the parallel sibling plan 10-02's `Integrations/CalamityIntegration.cs` work to merge without conflict (disjoint files)
- This plan's `requirements` frontmatter lists ARENA-01; deliberately NOT marked complete in `REQUIREMENTS.md` -- it is a multi-plan requirement and Phase 7's pending live-verification checkpoint plus Phase 10 plans 10-02/10-04/10-05/10-06 must also land first. This plan contributes Spirit's 6-boss remainder toward it.

---
*Phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing*
*Completed: 2026-08-14*

## Self-Check: PASSED

- FOUND: `.planning/phases/10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing/10-03-SUMMARY.md`
- FOUND: `Integrations/SpiritIntegration.cs`
- FOUND: commit `baeb553` (Task 1)
- FOUND: commit `7e2c7cb` (Task 2)
- FOUND: commit `5f8ea8b` (Task 3)
