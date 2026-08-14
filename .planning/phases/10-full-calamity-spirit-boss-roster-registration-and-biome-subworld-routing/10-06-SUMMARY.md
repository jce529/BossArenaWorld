---
phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing
plan: 06
subsystem: boss-registry
tags: [calamity, spiritmod, infernummode, biome-routing, live-verification, boss-checklist]

# Dependency graph
requires:
  - phase: 10-01..10-05
    provides: SummonItemRegistry.RegisterPolymorphic, ForcedTimeSystem, all 17 in-scope Calamity/Spirit boss registrations, InfernumMode weak reference
provides:
  - Live-verified full 17-boss Phase 10 roster (11 Calamity + 6 Spirit) pipeline correctness
  - Live-verified Infernum-conditional registration matrix in both mod configurations
  - Live-verified forced-night full-duration persistence (Pitfall 6 confirmed non-issue)
  - Live-verified CalamityMod-disabled / SpiritMod-disabled JIT safety
affects: [phase-08-04, milestone-wrapup]

tech-stack:
  added: []
  patterns: []

key-files:
  created: []
  modified: []

key-decisions:
  - "The Old Duke removed from v1 scope entirely (quick task 260815-024) after its despawn bug was root-caused and a general fix (ForceInfernumModeActiveInArena) was implemented and kept, but Old Duke's own registration was still descoped by user decision"

patterns-established: []

requirements-completed: [ARENA-01]

# Metrics
duration: n/a (live verification results carried over from this session's earlier checkpoint round; closed via quick task 260815-024)
completed: 2026-08-15
---

# Phase 10 Plan 06: Live Verification Checkpoint + Mod-Disabled Safety Checkpoint Summary

**17 of the originally-planned 18 Calamity/Spirit bosses live-confirmed end-to-end (pipeline, Infernum-gating matrix, forced-night persistence, mod-disabled JIT safety); The Old Duke descoped from v1 after its despawn bug was root-caused but not re-verified.**

## Performance

- **Duration:** n/a -- this plan's live-verification content was gathered during this session's earlier consolidated live-testing round (see `.planning/check.md` section ④) and formally closed via quick task 260815-024 once The Old Duke's status was resolved (descoped).
- **Completed:** 2026-08-15
- **Tasks:** 3 checkpoint tasks (all live-verification, no repo files modified by the checkpoints themselves)
- **Files modified:** 0 (this plan's own scope) -- The Old Duke's removal from the roster was a separate code change, made by quick task 260815-024 in `Integrations/CalamityIntegration.cs`/`Systems/BossSummonPlayer.cs`

## Accomplishments

- **Task 1 (registration-matrix + representative live-fight verification):** All items passed. Dragonfolly (Jungle) and Scarabeus (Desert) both confirmed no despawn/enrage and normal damage output over a full fight; both downed flags flip true after `BossCoreItem` use. Ancient Avian (Space, Zone-thematic-only sample) fights normally and its downed flag flips true. `MarkofProvidence` polymorphic resolution confirmed correct across all 3 reachable Zones (Dungeon -> Ceaseless Void, Underworld -> Signus, Space -> Storm Weaver), item never consumed across any of the 3 uses. The Infernum-conditional registration matrix confirmed correct in BOTH mod configurations: with InfernumMode disabled, Providence/Profaned Guardians/Ceaseless Void all redirect normally; with InfernumMode enabled, all three produce no redirect (fall through to Infernum's own structure-gated flow). Astrum Deus/Astrum Aureus confirmed to force night only when InfernumMode is enabled. Re-use idempotency spot-check confirmed no duplicate side effects.
- **Task 2 (forced-night persistence, Pitfall 6):** Confirmed Moon Jelly Wizard/Dusking's forced night persists for the full natural fight duration with no mid-fight daytime despawn -- `ForcedTimeSystem`'s per-tick re-assertion works as designed.
- **Task 3 (CalamityMod-disabled / SpiritMod-disabled JIT safety):** Both mod-disabled load tests completed cleanly with no crash dialog and no `JITException` naming `CalamityIntegration`/`SpiritIntegration` in `Logs/client.log`. Both mods confirmed re-enabled afterward.
- **The Old Duke -- Descoped, Not Verified:** The Old Duke was excluded from this checkpoint's live-verification scope entirely, not carried forward as a failure. During this session's testing, The Old Duke despawned immediately after spawning in the default plain-stone arena. A follow-up `/gsd:debug` session (`old-duke-immediate-despawn-plain-arena`) root-caused the issue: NoxusBoss (an installed-but-out-of-scope mod) hijacks Old Duke's AI into a scripted, harmless "Avatar of Emptiness" cutscene whenever InfernumMode's own per-world "Infernum Mode" toggle reads false -- which it always does inside the throwaway `BossArenaSubworld` (same "world-scoped modded flag does not survive the subworld round-trip" category as the Phase 4 Hive Mind/ZoneCorrupt precedent). A general fix (`ForceInfernumModeActiveInArena()`, forcing the toggle active on every arena entry via InfernumMode's own sanctioned `Mod.Call("SetInfernumActive", true)` API) was implemented and build-verified. Despite the fix existing, the user decided to remove The Old Duke's own registration from BossArenaSubWorld's v1 roster entirely (quick task 260815-024) rather than live re-verify and keep it in scope -- a deliberate scope-closing decision, not a "no fix exists" outcome. See `.planning/debug/old-duke-immediate-despawn-plain-arena.md` Resolution section for full detail. The general fix itself was kept in the codebase since it also benefits Providence/Profaned Guardians/Astrum Deus/Astrum Aureus's Infernum-conditional gating correctness.

## Task Commits

This plan is entirely live-verification checkpoints (no repo files modified directly by 10-06 itself). The Old Duke's subsequent removal from the roster was committed separately under quick task 260815-024:

1. `86956e3` - fix(quick-260815-024): remove The Old Duke from v1 roster, keep InfernumMode arena-activation fix
2. `12a5d88` - docs(quick-260815-024): close Old Duke debug session, update STATE/REQUIREMENTS for 17-boss v1 roster

## Files Created/Modified

- None directly by this plan -- see quick task 260815-024's commits above for the code change that finalized the 17-boss roster this summary documents.

## Decisions Made

- The Old Duke removed from v1 scope entirely (2026-08-15, quick task 260815-024), despite its despawn bug being root-caused and a general fix implemented and kept. Deliberate scope-closing decision, mirroring the Sulphurous Sea exclusion already made for the same boss under D-07 (Phase 9).

## Deviations from Plan

None beyond The Old Duke's descope, which is the explicit subject of this plan's closure -- see quick task `260815-024-the-old-duke-v1-despawn`.

## Next Phase Readiness

Phase 10 is COMPLETE (6/6 plans). `.planning/phases/08-full-pipeline-verification-tracker-confirmation/08-04-PLAN.md` can now close, citing this file's results for the 17-boss roster's pipeline correctness and Boss Checklist recognition.

## Self-Check: PASSED

- FOUND: `.planning/debug/old-duke-immediate-despawn-plain-arena.md` (status: wontfix, Resolution filled in)
- FOUND: commit `86956e3` (git log)
- FOUND: commit `12a5d88` (git log)
