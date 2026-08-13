---
phase: 3
slug: bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-08-13
---

# Phase 3 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — tModLoader mod, no automated unit-test harness (confirmed: no test project/config exists anywhere in this repo; matches Phase 1/2's established `01-VALIDATION.md`/`02-VALIDATION.md` precedent) |
| **Config file** | none |
| **Quick run command** | `dotnet build BossArenaSubWorld.csproj` |
| **Full suite command** | N/A — no automated suite. "Full verification" = one live in-game playthrough of the phase's 5 Success Criteria in sequence, with a world backup taken first |
| **Estimated runtime** | ~ build: seconds; live checkpoint: manual, untimed |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build BossArenaSubWorld.csproj` (0 warnings, 0 errors expected)
- **After every plan wave:** Same build command; no separate full suite exists
- **Before `/gsd:verify-work`:** One live in-game checkpoint (world backup first) covering all 5 phase Success Criteria in sequence
- **Max feedback latency:** Build gate is near-instant; live checkpoint is manual/untimed by nature of the platform

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 03-xx-xx | TBD | TBD | DROP-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ Wave 0 (new file: `Systems/BossRegistry.cs`) | ⬜ pending |
| 03-xx-xx | TBD | TBD | DROP-02 | manual-only | live in-game kill test (in-subworld positive, main-world negative check) | ❌ Wave 0 (new file: `ItemDropRules/BossCoreDropRule.cs`) | ⬜ pending |
| 03-xx-xx | TBD | TBD | DROP-03 | manual-only | live in-game: pick up item, exit subworld, inspect `BossCoreItem.BossKey` | ❌ Wave 0 (new file: `Items/BossCoreItem.cs`) | ⬜ pending |
| 03-xx-xx | TBD | TBD | APPLY-01 | manual-only | live in-game: use item, check `NPC.downedSlimeKing` | ❌ Wave 0 (new file) | ⬜ pending |
| 03-xx-xx | TBD | TBD | APPLY-04 | manual-only | live in-game: use a second `BossCoreItem` after first successful apply, confirm no double-apply | ❌ Wave 0 (new file) | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*
*Exact Task/Plan/Wave IDs filled in by the planner once PLAN.md files exist.*

---

## Wave 0 Requirements

- [ ] `Systems/BossRegistry.cs` — new `ModSystem`, no automated test beyond compile gate
- [ ] `GlobalNPCs/BossKillGlobalNPC.cs` — new `GlobalNPC`, no automated test beyond compile gate
- [ ] `ItemDropRules/BossCoreDropRule.cs` — new custom `IItemDropRule`, no automated test beyond compile gate
- [ ] `Items/BossCoreItem.cs` — new `ModItem` with instance data, no automated test beyond compile gate

*No automated test-framework gap beyond the build gate — matches this project's established, previously-approved manual-verification model for tModLoader mods (see `01-VALIDATION.md`/`02-VALIDATION.md`).*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Killing King Slime inside the subworld drops `BossCoreItem`; killing it outside does not | DROP-02 | No harness exists (or is feasible) for simulating live NPC kill/loot inside a running tModLoader instance | World backup first. Enter subworld via Test1 portal + Slime Crown, kill King Slime, confirm `BossCoreItem` drops. Optionally verify a main-world King Slime kill does NOT drop it. |
| `BossCoreItem` carries `BossKey = "vanilla:king_slime"` across the subworld exit | DROP-03 | Requires live inventory/world-transition state, not simulatable in a unit test | Pick up dropped item, exit subworld, inspect via a temporary debug print or tooltip (planner/executor discretion) that the key survived the trip |
| Using `BossCoreItem` sets `NPC.downedSlimeKing = true` via `SetEventFlagCleared` (flag + achievement + netcode-sync-no-op path) | APPLY-01 | Requires live game state and (ideally) a tracker mod to confirm recognition | Use item in main world; check flag directly or via Boss Checklist-equivalent mod |
| Re-using a `BossCoreItem` (or a second one) after the flag is already set does not double-apply | APPLY-04 | Idempotency behavior only observable through repeated live actions | Kill King Slime a second time to obtain a second `BossCoreItem`; use it after the first successful apply; confirm no duplicate side effects/messages and the "already downed" no-op path fires |
| Full pipeline demonstrated end-to-end (subworld kill → item drop → main-world apply) | All 5 Success Criteria | Integration-level, cross-world-boundary behavior; the actual phase-closing proof | Single live playthrough exercising all 5 Success Criteria in sequence, world backup taken first |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify (build gate) or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify (build gate satisfies this for every code-writing task)
- [ ] Wave 0 covers all MISSING references (4 new files listed above)
- [ ] No watch-mode flags
- [ ] Feedback latency acceptable (build gate near-instant; live checkpoint is the accepted manual-verification model for this project)
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
