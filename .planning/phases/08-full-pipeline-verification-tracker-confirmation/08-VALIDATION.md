---
phase: 08
slug: full-pipeline-verification-tracker-confirmation
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-08-14
---

# Phase 08 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — tModLoader mod, no automated in-game test harness exists or is planned (established since Phase 1) |
| **Config file** | none |
| **Quick run command** | `dotnet build BossArenaSubWorld.csproj` — smoke-test gate only; Phase 8 writes no new code, so this simply re-confirms nothing was accidentally broken by whatever Phase 6/7/10 work landed most recently before each wave |
| **Full suite command** | N/A — "full verification" IS the manual live in-game checklist per wave |
| **Estimated runtime** | ~10-20 seconds (build smoke gate only; live checkpoints are manual and unbounded) |

---

## Sampling Rate

- **Per wave kickoff:** `dotnet build BossArenaSubWorld.csproj` (confirms the codebase the wave depends on — e.g. Phase 6/7/10's registration code — actually compiles before spending live-test time on it)
- **Per boss checkpoint:** manual live in-game steps, following the `check.md` numbered-checklist / explicit resume-signal format established in Phase 6
- **Phase gate:** all 24 in-scope bosses' rows in 08-RESEARCH.md's Master Boss Roster show "Complete" execution status AND "Confirmed" Boss-Checklist status before `/gsd:verify-work` — cannot close until Phase 6 (`06-03`), Phase 7, and Phase 10 have all executed

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 08-0x | TBD | 1 | VERIFY-01/03 | manual-only, live in-game | King Slime + Hive Mind Boss Checklist recognition closure (already registered, only tracker-recognition unconfirmed) | ❌ W0 | ⬜ pending |
| 08-0x | TBD | 2 | VERIFY-01/03 | manual-only, live in-game, blocked on Phase 6 `06-03` executing first | Thorn + Astrageldon full pipeline + Boss Checklist | ❌ W0 | ⬜ pending |
| 08-0x | TBD | 3 | VERIFY-01/03 | manual-only, live in-game, blocked on Phase 7 executing first | Goblin Chariot full pipeline + Boss Checklist (folds in 07-RESEARCH.md's own open question) | ❌ W0 | ⬜ pending |
| 08-0x | TBD | 4 | VERIFY-01/03 | manual-only, live in-game, blocked on Phase 10 executing first | Full Calamity/Spirit roster (~18 bosses) full pipeline + Boss Checklist | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

*None in the traditional "missing test file" sense — this project has never had automated tests. The actual Wave 0 gap for Phase 8 is sequencing: Phase 6 (`06-03`), Phase 7, and Phase 10 must execute before their respective waves' live checkpoints become runnable. The planner should structure later waves as explicitly phase-gated rather than attempting to force premature coverage.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|--------------------|
| King Slime / Hive Mind Boss Checklist recognition | VERIFY-01/03 | Already live-verified for their own side effects, but Boss Checklist tracker-UI recognition specifically was never confirmed (King Slime used an alternative method; Hive Mind confirmed its own side effects only) — closing this requires a live Boss Checklist UI check, not a re-run of the whole pipeline | Kill King Slime / Hive Mind again (or check current save state if already downed), confirm Boss Checklist's tracker UI shows each as defeated |
| Thorn / Astrageldon full pipeline + Boss Checklist | VERIFY-01/03 | No automated harness; blocked until Phase 6's `06-03-PLAN.md` checkpoint actually executes | Follow `06-03-PLAN.md`/`check.md`'s existing steps once run, additionally confirming Boss Checklist recognition |
| Goblin Chariot full pipeline + Boss Checklist | VERIFY-01/03 | No automated harness; blocked until Phase 7 executes | Follow `07-02-PLAN.md`'s steps once run, additionally confirming Homeward Journey's own `CoJ_BossChecklist.cs` integration surfaces it correctly |
| Full Calamity/Spirit roster (Phase 10) | VERIFY-01/03 | No automated harness; blocked until Phase 10 executes | Per-boss live checkpoints following Phase 10's own execution/validation output, plus Boss Checklist recognition for each |
| Boss Checklist `.tmod` physical presence | N/A (pre-flight sanity check) | `Mods\enabled.json` lists it enabled but this session's directory listing did not show its `.tmod` file — genuine unresolved discrepancy | First Wave 1 task should be a trivial sanity check: confirm Boss Checklist is actually loaded and functional in-game before relying on it for every other checkpoint in this phase |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references (none — sequencing gap only, not a test-infra gap)
- [ ] No watch-mode flags
- [ ] Feedback latency < 20s (build gate)
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
