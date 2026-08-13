---
phase: 02
slug: summon-item-redirect-entry-registry
status: draft
nyquist_compliant: true
wave_0_complete: true
created: 2026-08-13
---

# Phase 02 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — tModLoader mod, no automated unit-test harness. Verification is manual, live, in-game, matching Phase 1's established pattern (`01-VERIFICATION.md`). |
| **Config file** | none |
| **Quick run command** | `dotnet build BossArenaSubWorld.csproj` |
| **Full suite command** | N/A — no automated suite exists. "Full verification" = one live in-game playthrough of the phase's success criteria. |
| **Estimated runtime** | ~10-20s per build |

---

## Sampling Rate

- **After every task commit:** `dotnet build BossArenaSubWorld.csproj` (0 warnings, 0 errors expected)
- **After every plan wave:** Same build command; no separate full suite exists
- **Before `/gsd:verify-work`:** One live in-game checkpoint covering all four success criteria in sequence
- **Max feedback latency:** ~20 seconds (build time)

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 02-01-* | 01 | 1 | SUBW-01 | build | `dotnet build BossArenaSubWorld.csproj` | ✅ | ⬜ pending |
| 02-02-* | 02 | 1/2 | SUBW-02 | build | `dotnet build BossArenaSubWorld.csproj` | ✅ | ⬜ pending |
| 02-03-* | 03 | 2 | SUBW-03 | build | `dotnet build BossArenaSubWorld.csproj` | ✅ | ⬜ pending |
| 02-04-* | 04 | 2/3 | SUBW-04 | build | `dotnet build BossArenaSubWorld.csproj` | ✅ | ⬜ pending |
| 02-FINAL | final | last | SUBW-01..04 | manual live checkpoint | N/A — see Manual-Only Verifications below | ❌ no harness | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

*Exact task IDs are assigned by the planner; this map will be reconciled against actual PLAN.md task IDs during planning.*

---

## Wave 0 Requirements

*Existing infrastructure covers all phase requirements — no test framework applicable to this project (tModLoader has no in-process unit-test harness). `dotnet build` is the only automated gate available.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|--------------------|
| Registry recognizes Slime Crown as a registered summon item | SUBW-01 | Requires live held-item detection via `Player.HeldItem` at runtime — no harness to simulate this | Hold Slime Crown, right-click placed `Test1` tile, confirm redirect fires |
| `Test1` tile places, renders, and `NewRightClick` fires only for registered items | SUBW-02 | Requires live world placement + tile interaction — no in-process world/tile simulation exists | Place `Test1` via Creative menu; right-click while holding an unregistered item (confirm no redirect); right-click while holding Slime Crown (confirm redirect) |
| Right-click with Slime Crown sends player into `BossArenaSubworld`, main-world summon is cancelled | SUBW-03 | Requires live `SubworldSystem.Enter<T>()` transition — no harness for subworld transitions | Confirm King Slime does NOT spawn in main world at the moment of right-click; confirm player lands in `BossArenaSubworld` |
| King Slime auto-spawns on arrival; Slime Crown remains in inventory afterward | SUBW-04 | Requires live `ModPlayer.OnEnterWorld` firing + live NPC spawn + live inventory check | After subworld arrival, confirm King Slime NPC exists and is aggroed on the player; confirm Slime Crown still occupies its inventory slot |

**Checkpoint sequence (single live pass covering all four rows):** place tile → hold Slime Crown → right-click → confirm main-world summon did NOT happen → confirm subworld entry → confirm King Slime auto-spawn → confirm Slime Crown not consumed → exit → confirm no leftover unexpected state. Mirrors Phase 1's `01-04-PLAN.md` checkpoint structure.

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify (build-gate) or are covered by the manual live checkpoint
- [x] Sampling continuity: no 3 consecutive tasks without automated verify (every task gets a build check)
- [x] Wave 0 covers all MISSING references (N/A — no framework applicable)
- [x] No watch-mode flags
- [x] Feedback latency < 20s (build-time only; live checkpoint is the phase-gate, not per-task)
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** approved 2026-08-13 (per research findings — manual-verification model already established and validated in Phase 1)
