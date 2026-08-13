---
phase: 01-subworld-skeleton-isolation-proof
plan: 04
subsystem: testing
tags: [subworldlibrary, terraria, downed-flags, isolation-test, live-verification]

# Dependency graph
requires:
  - phase: 01-subworld-skeleton-isolation-proof (plans 01-03)
    provides: BossArenaSubworld GenPass skeleton, debug enter/exit/checkflag commands, biome-override hook
provides:
  - Empirical (live, in-game) test result for the subworld isolation premise
  - Confirmation that BossArenaSubworld generates a walkable, content-free stone platform in practice
  - CRITICAL: evidence that NPC.downedSlimeKing DID propagate from the subworld back to the main world (True, not the expected False) after a real King Slime kill with no carrier-item action taken
affects: [phase-02-summon-item-redirect, phase-03-boss-registry-pipeline, roadmap, PROJECT.md-Key-Decisions]

# Tech tracking
tech-stack:
  added: []
  patterns: []

key-files:
  created: []
  modified: []

key-decisions:
  - "Isolation premise (the founding assumption of the entire carrier-item architecture) is NOT empirically confirmed as of this test -- observed result contradicts 01-RESEARCH.md's source-traced expectation (Pitfall 3) that NPC.downedSlimeKing would revert to False on return to the main world"
  - "Per the plan's own built-in guidance (01-04-PLAN.md Task 2, step 9), Phase 2 planning must NOT proceed until this finding is re-investigated"

patterns-established: []

requirements-completed: [SUBW-05, SUBW-06, VERIFY-02]

# Metrics
duration: ~20min (interactive checkpoint session)
completed: 2026-08-13
---

# Phase 1 Plan 04: Subworld Skeleton & Isolation Proof (Live King Slime Test) Summary

**Live King Slime kill test shows `NPC.downedSlimeKing = True` after the subworld round-trip -- the opposite of the expected/required `False`, contradicting the isolation premise the entire Phase 3+ carrier-item architecture depends on.**

## Performance

- **Duration:** ~20 min (interactive, human-in-the-loop checkpoint session; no wall-clock timer started at plan launch since both tasks are pure checkpoints with no autonomous work preceding them)
- **Completed:** 2026-08-13
- **Tasks:** 2/2 executed (both `checkpoint:human-action` / `checkpoint:human-verify`)
- **Files modified:** 0 (this plan is pure manual/live verification of Plans 01-01–01-03's already-committed code; no repo files were changed)

## Accomplishments

- Disposable test world (`BossArenaTest`, distinct from the real save `HiPo's_Terrarium`) created and confirmed loading with only SubworldLibrary + BossArenaSubWorld enabled (Task 1).
- `/bossarena-enter` confirmed to transition into the subworld and land the player on a walkable stone platform -- not falling indefinitely, not spawned entombed (Pitfall 2 from 01-RESEARCH.md did not occur).
- Visual inspection (after equipping a torch, since the platform sits at `Main.maxTilesY / 2`, well below the subworld's sky-light range, so it is dark without a carried light source -- an expected side effect of the mid-height placement, not a bug) showed only stone-platform tiles. A small number of naturally-spawned mob-like sprites were visible near the player; these are runtime mob spawns (a function of darkness/underground tile conditions), not placed generation content, so they do not violate SUBW-05.
- A real King Slime was summoned via a Slime Crown and defeated inside the subworld (satisfies D-11 -- a genuine kill, not a debug flag toggle).
- No carrier item was used and no debug command manually touched any flag (D-12 respected).
- `/bossarena-exit` returned the player to the main world without a crash or stuck transition.
- **`/bossarena-checkflag` in the main world after the round trip printed `NPC.downedSlimeKing = True`** -- this is the critical, unexpected empirical finding this plan exists to surface.

## Task Commits

No code/doc commits were made for this plan's task execution -- both tasks are pure in-game verification with zero repo files modified (per plan frontmatter `files_modified: []`). This SUMMARY and the STATE/ROADMAP/REQUIREMENTS metadata updates are the only artifacts committed, via the final docs commit below.

1. **Task 1: Prepare a disposable test world** -- no commit (environment/world setup only, confirmed via user report "world ready")
2. **Task 2: Run the King Slime isolation-proof test** -- no commit (live verification only, confirmed via user report)

**Plan metadata:** committed separately (see `docs(01-04): complete isolation-proof plan` in repo history after this SUMMARY is written)

## Files Created/Modified

None -- this plan verifies Plans 01-02/01-03's already-committed code in a live game session; no source files were touched.

## Decisions Made

- **Isolation premise is NOT confirmed.** 01-RESEARCH.md (Pitfall 3, SUBW-06 entry) source-traced the expected mechanism as: exiting the subworld triggers the main world's own file reload, which restores whatever `NPC.downedSlimeKing` value was already saved on disk (False for a fresh world), discarding the in-memory `True` set during the subworld kill. The live test contradicts this: the flag read `True` after the full round trip. Per the plan's own explicit instruction (01-04-PLAN.md Task 2, step 9), **Phase 2 planning must not proceed until this is re-investigated.**
- Two working hypotheses for the re-investigation (not verified, flagged for a future research pass):
  1. **In-memory-only propagation:** `SubworldSystem.Exit()` may not actually re-invoke a full `WorldFile.LoadWorld()`-style reload of boss-flag statics for the main world (even though it swaps tile/world-size data back) -- so the `True` set by vanilla `OnKill` code during the subworld visit simply persists in the shared static `NPC.downedSlimeKing` field across the transition, because nothing ever resets it. If true, this is an in-memory-only leak that would NOT survive a full game/process restart (would need to be tested: kill in subworld, exit, then fully quit and relaunch tModLoader, then recheck the flag in the main world).
  2. **Genuine on-disk persistence:** Some autosave or shared-file-identity mechanism during the subworld visit could be writing the flag into the actual main-world `.wld` file, making the propagation permanent, not just an in-memory artifact. This would be a more serious finding requiring a `.wld` file inspection (e.g., via TEdit or a hex/tag reader) to confirm whether `downedSlimeKing` is actually persisted to the disposable world's save file after the round trip.
  3. **NOT a vanilla-vs-modded distinction.** During this checkpoint the tester asked whether this result could be explained by vanilla bosses being a special case ("I recall vanilla bosses syncing correctly in subworlds"). Checked directly against this project's own prior research and found no support for that read: `.planning/research/PITFALLS.md` line 27 explicitly chose a vanilla boss as the isolation-proof test precisely *because* "vanilla `NPC.downedBoss*` fields are the simplest case" of the same general bug, not an exception to it; line 15 states SubworldLibrary treats "`NPC` downed booleans" as part of the per-subworld-isolated state it does NOT reliably copy back out to the main world. 01-RESEARCH.md's Pitfall 3 makes the identical prediction for vanilla flags specifically. One real distinct mechanism does exist in the ecosystem -- SubworldLibrary's `CopyMainWorldData()` copies a hand-picked set of vanilla state **into** the subworld on entry (one-directional, main → subworld) -- which may be what the tester was recalling, but that is a different data-flow direction from what this test measures (subworld kill → main world, i.e. subworld → main). It does not explain or predict the observed `True` result. This hypothesis is listed for completeness but is not supported by the project's own research and should not be assumed as the explanation without further evidence.
- This finding does not invalidate SUBW-05 (zero placed content -- confirmed via visual inspection, only stone tiles seen) or the structural, source-verified parts of SUBW-06 (transition completed without crash/hang). It specifically contradicts the separate "isolation premise" truth stated in 01-04-PLAN.md's frontmatter (`must_haves.truths`), which is not itself a REQUIREMENTS.md ID but is the load-bearing assumption behind Phase 3's planned `BossCoreItem`/`BossRegistry` design.

## Deviations from Plan

None in the auto-fix sense (Rules 1-3) -- no code was written or changed by this plan, so there was nothing to auto-fix. The deviation here is empirical/architectural: the live test's actual result diverged from the plan's expected/hypothesized outcome. Per Rule 4 territory (architectural implications) and the plan's own built-in "unexpected result" branch, this was not silently resolved -- it is flagged here and in STATE.md for explicit human/future-agent decision before Phase 2 proceeds.

## Issues Encountered

- **Initial subworld entry appeared as a fully black screen.** Root cause (confirmed via code read of `Subworlds/FlatStonePlatformPass.cs`): the platform is generated at `Main.maxTilesY / 2` (mid-height of an 800-tile-tall subworld), which is below the subworld's sky-light range, so without a held light source the screen is black. This is expected behavior given the current Y-placement choice, not a bug -- resolved by the user equipping a torch. Not code-fixed since it doesn't violate any stated acceptance criterion (player was not falling or entombed), but noted here in case a future plan wants to move the platform closer to a lit/sky-exposed height for easier manual QA.
- **Inventory-intact check (SUBW-06, step 7 of the test procedure) was not actually performed.** The user skipped picking up/comparing inventory items after `/bossarena-exit`, reasoning that since the subworld transition worked normally, item preservation "probably" also worked. This is an assumption, not an empirical confirmation. SUBW-06's structural/source-verified claim (`NoPlayerSaving = false` in 01-RESEARCH.md Pitfall 1) still stands, but the live-test-specific inventory check for this run is unconfirmed and should be re-run (trivially) alongside the isolation-premise re-investigation.

## User Setup Required

None -- no external service configuration required.

## Next Phase Readiness

**Phase 1's structural/build-time deliverables (Plans 01-01–01-03) are confirmed working in a live game session:** the subworld generates a walkable, content-free platform, debug enter/exit/checkflag commands work, and the disposable-test-world workflow (D-13) is validated as a safe way to test without touching the real save.

**However, Phase 1's core empirical purpose -- proving the isolation premise -- did NOT succeed.** `NPC.downedSlimeKing` read `True` in the main world after a real King Slime kill in the subworld with no carrier-item action taken, contradicting the source-traced expectation in 01-RESEARCH.md.

**Blocker for Phase 2:** Per 01-04-PLAN.md's own explicit instruction, do not proceed to Phase 2 (summon-item redirect) or Phase 3 (BossRegistry/BossCoreItem/GlobalNPC pipeline) planning until this finding is re-investigated. Recommended next step: a targeted `/gsd:research-phase` or `/gsd:debug` pass to determine which of the two hypotheses above (in-memory-only leak vs. genuine on-disk persistence) explains the observed `True` result, since the two have very different implications for whether the carrier-item architecture (PROJECT.md's core value) is even necessary in its currently planned form.

---
*Phase: 01-subworld-skeleton-isolation-proof*
*Completed: 2026-08-13*
