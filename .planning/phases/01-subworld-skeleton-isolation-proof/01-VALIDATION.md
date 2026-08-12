---
phase: 01
slug: subworld-skeleton-isolation-proof
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-08-13
---

# Phase 01 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — no automated unit-test framework exists or is conventional for tModLoader world-generation/subworld-transition behavior. Verification in this domain is manual, in-game, and observational. |
| **Config file** | none |
| **Quick run command** | `dotnet build BossArenaSubWorld.csproj` (compile-check only — catches syntax/type errors, not gameplay correctness) |
| **Full suite command** | Manual test procedure (see below), run in-game via tModLoader with the mod loaded |
| **Estimated runtime** | ~30s (build) / ~5-10 min (manual in-game isolation-proof test) |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build BossArenaSubWorld.csproj`
- **After every plan wave:** Run the full manual in-game test procedure (isolation-proof test)
- **Before `/gsd:verify-work`:** King Slime isolation test must show `NPC.downedSlimeKing == false` in the main world after the round trip, with inventory intact
- **Max feedback latency:** ~30s for build-level feedback; manual gameplay verification happens at wave/phase boundaries, not per-task

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 01-00-01 | 00 | 0 | (setup) | build | `dotnet --version` shows 8.0.x after `global.json` pin | ❌ W0 (no `global.json` exists yet) | ⬜ pending |
| 01-01-xx | 01 | 1 | SUBW-05 | manual, visual + code-review | Load arena in-game, confirm only stone-platform tiles exist; code-review confirms `Tasks` contains exactly one `GenPass`, no vanilla passes referenced | ❌ W0 (no test harness — inherent to domain) | ⬜ pending |
| 01-02-xx | 02 | 1-2 | SUBW-06 | manual, in-game | `/bossarena-enter`, note inventory, `/bossarena-exit`, confirm inventory unchanged | ❌ W0 (same as above) | ⬜ pending |
| 01-03-xx | 03 | 2 | VERIFY-02 | manual, doc review | Guidance doc exists, covers world/player save paths + subworld file location | N/A — documentation deliverable | ⬜ pending |
| 01-04-xx | 04 | 3 | VERIFY-02 (isolation proof) | manual, in-game | Summon and kill King Slime in subworld, exit without carrier item, confirm `NPC.downedSlimeKing == false` in main world | N/A — empirical one-shot proof, not a repeatable automated test | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `global.json` — pin `.NET SDK` to an installed 8.0.x version (e.g. `8.0.424`). **Why:** `dotnet --version` in the project directory currently resolves to SDK 10.0.201 by default (no pin exists), which CLAUDE.md explicitly warns against ("avoid .NET 9.0 and .NET 10.0 — those will not work"). This must be closed before the first build.

*No test-framework Wave 0 gap applies — this domain (world generation, subworld transitions, biome-flag timing) cannot be meaningfully unit-tested outside a running tModLoader instance; that is a structural characteristic of tModLoader modding, not a gap to close with tooling in this phase.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|--------------------|
| Zero placed mod/vanilla content in generated subworld | SUBW-05 | World generation output can only be meaningfully inspected by loading it in a running tModLoader instance and visually/structurally confirming tile content; no headless world-gen test harness exists in this ecosystem | Enter subworld via `/bossarena-enter`, fly/walk across the platform, confirm no NPCs/structures/ores beyond the placed stone platform; cross-check `Tasks` list in code contains only the custom `GenPass` |
| Reliable enter/exit without inventory loss | SUBW-06 | Inventory persistence across a subworld transition is a runtime/save-state behavior only observable in a live client session | `/bossarena-enter`, note/pick up an item, `/bossarena-exit`, confirm inventory matches pre-entry state |
| Isolation-proof: downed flag does not propagate | VERIFY-02 (premise) | This is an empirical, one-time proof of a specific bug/behavior (`NPC.downedSlimeKing` scoping across world files) that requires actually summoning, fighting, and killing a live NPC — not something a unit test can simulate | Summon and kill King Slime inside subworld, `/bossarena-exit` with no carrier-item action taken, check `NPC.downedSlimeKing` in main world (e.g. via debug print or observing no King Slime statue/message trigger) — expect `false` |
| World-backup guidance followed | VERIFY-02 (doc) | Procedural/documentation compliance, not code behavior | Confirm guidance doc exists and, for this phase, confirm testing occurred on a disposable test world per D-13 (no real save at risk) |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify (build-check) or Wave 0 dependencies, or are explicitly manual-only per the table above
- [ ] Sampling continuity: no 3 consecutive tasks without a build-check or manual verification step
- [ ] Wave 0 covers all MISSING references (`global.json` SDK pin)
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s for build-level checks
- [x] `nyquist_compliant: true` set in frontmatter (manual-only domain acknowledged and covered)

**Approval:** pending
