---
phase: 03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept
verified: 2026-08-13T06:19:10Z
status: passed
score: 5/5 phase success criteria verified (all backed by live in-game test evidence)
---

# Phase 3: BossRegistry + BossCoreItem + GlobalNPC Pipeline (POC) Verification Report

**Phase Goal:** Killing a registered boss inside the subworld reliably carries a boss-kill credential back to the main world and applies it exactly once, proven end-to-end with one low-risk vanilla boss before content-mod complexity is introduced.
**Verified:** 2026-08-13T06:19:10Z
**Status:** passed
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths (ROADMAP Success Criteria)

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | A central NPC.type → bossKey mapping registers trackable bosses, and killing a registered boss inside the subworld drops a `BossCoreItem` tagged with that boss's key, via a conditional `ItemDropRule` gated to the subworld | ✓ VERIFIED | `Systems/BossRegistry.cs` registers `"vanilla:king_slime" -> NPCID.KingSlime` in `PostSetupContent`, exposes `TryGetKeyForNpc`. `ItemDropRules/BossCoreDropRule.cs.CanDrop` gates dynamically on `SubworldSystem.IsActive<BossArenaSubworld>()` (evaluated per-kill, not baked in at load). `GlobalNPCs/BossKillGlobalNPC.cs.ModifyNPCLoot` attaches the rule to every registered NPC type. Live test Steps 1–2 (03-03-SUMMARY.md) confirm no drop outside the subworld and a correctly-tagged drop inside it. |
| 2 | `BossCoreItem` correctly carries its boss key as instance data across the subworld-to-main-world trip | ✓ VERIFIED | `Items/BossCoreItem.cs`: `BossKey` field + `CloneNewInstances=>true` + `Clone()` override + `SaveData`/`LoadData` TagCompound round-trip. Live test Steps 3–4 confirm the item survives inventory pickup and the SubworldLibrary exit trip with `BossKey` intact. |
| 3 | Using `BossCoreItem` in the main world calls `BossRegistry.Apply(key)` and sets the corresponding boss's downed flag | ✓ VERIFIED | `BossCoreItem.UseItem` calls `BossRegistry.Apply(BossKey)`; `BossRegistry.Apply` calls `NPC.SetEventFlagCleared(ref NPC.downedSlimeKing, -1)` (not a raw assignment) on the `Applied` path. Live test Step 5 confirms a success chat message, item consumption, and `NPC.downedSlimeKing` flip. |
| 4 | Re-using a `BossCoreItem`, or using it again after a partial failure, does not double-apply rewards or duplicate side effects | ✓ VERIFIED | `BossRegistry.Apply` checks `def.IsDowned()` before calling `ApplyDowned()` (live-flag idempotency, no separate tracking set) and returns `AlreadyDowned` without re-invoking the delegate. `BossCoreItem.UseItem` still consumes the item on `AlreadyDowned` with distinct chat feedback. Live test Step 6 confirms a second use produces a different "already defeated" message, still consumes the item, and produces no duplicate side effects. |
| 5 | The full pipeline (subworld kill → item drop → main-world apply) is demonstrated end-to-end in singleplayer with one vanilla boss, with a world backup taken first | ✓ VERIFIED | 03-03-SUMMARY.md documents a world/player backup at `Worlds\_backups\2026-08-13_pre-phase3-verify\` (VERIFY-02) taken before all 6 live test steps, all of which passed per the user's own resume-signal ("전부 통과했어" / all 6 steps confirmed passing). |

**Score:** 5/5 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `Systems/BossRegistry.cs` | `BossDefinition` record, `ApplyResult` enum, `BossRegistry` ModSystem (Register/TryGetKeyForNpc/Apply), registers `vanilla:king_slime` | ✓ VERIFIED | Read in full. Matches plan spec exactly, including the `int[]` fix and idempotency-via-`IsDowned()` design. `min_lines: 35` — file is 56 lines. |
| `Items/BossCoreItem.cs` | ModItem carrying `BossKey` instance data, `UseItem` wired to `BossRegistry.Apply` | ✓ VERIFIED | Read in full. `BossKey`, `Clone`, `SaveData`/`LoadData`, and 3-way `UseItem` switch all present and correct. `min_lines: 30` — file is 61 lines. |
| `Items/BossCoreItem.png` | Placeholder texture so item doesn't render as missing-texture | ✓ VERIFIED | File exists on disk, 136 bytes — byte-identical to `Items/Test1Item.png` (the copy precedent), non-zero size. |
| `ItemDropRules/BossCoreDropRule.cs` | Custom `IItemDropRule`: `CanDrop` gates on subworld-active, `TryDroppingItem` spawns + tags `BossCoreItem` | ✓ VERIFIED | Read in full. Implements all 4 `IItemDropRule` members; gate lives in `CanDrop`, tagging happens immediately after `Item.NewItem`. `min_lines: 25` — file is 45 lines. |
| `GlobalNPCs/BossKillGlobalNPC.cs` | `GlobalNPC.ModifyNPCLoot` looks up `BossRegistry.TryGetKeyForNpc` and adds `BossCoreDropRule` | ✓ VERIFIED | Read in full. Contains guard + `npcLoot.Add(new BossCoreDropRule(key))`; correctly contains no `SubworldSystem.IsActive` reference (gate lives exclusively in the drop rule). `min_lines: 12` — file is 20 lines. |

All 5 artifacts exist, are substantive, and pass Level 1-2 checks. `dotnet build BossArenaSubWorld.csproj` re-run during this verification succeeded with **0 warnings, 0 errors**.

### Key Link Verification

| From | To | Via | Status | Details |
|------|-----|-----|--------|---------|
| `Items/BossCoreItem.cs` (`UseItem`) | `Systems/BossRegistry.cs` (`Apply`) | `BossRegistry.Apply(BossKey)` | ✓ WIRED | Confirmed at line 45 of `BossCoreItem.cs`, inside a `switch` covering all 3 `ApplyResult` cases. |
| `Systems/BossRegistry.cs` (`Apply`) | `Terraria.NPC.downedSlimeKing` | `NPC.SetEventFlagCleared(ref NPC.downedSlimeKing, -1)` | ✓ WIRED | Confirmed at line 29 of `BossRegistry.cs`. No raw `NPC.downedSlimeKing = true` assignment anywhere in the file. |
| `GlobalNPCs/BossKillGlobalNPC.cs` (`ModifyNPCLoot`) | `Systems/BossRegistry.cs` (`TryGetKeyForNpc`) | `BossRegistry.TryGetKeyForNpc(npc.type, out string key)` | ✓ WIRED | Confirmed at line 16 of `BossKillGlobalNPC.cs`. |
| `GlobalNPCs/BossKillGlobalNPC.cs` (`ModifyNPCLoot`) | `ItemDropRules/BossCoreDropRule.cs` | `npcLoot.Add(new BossCoreDropRule(key))` | ✓ WIRED | Confirmed at line 17. |
| `ItemDropRules/BossCoreDropRule.cs` (`CanDrop`) | `SubworldLibrary.SubworldSystem.IsActive<BossArenaSubworld>()` | dynamic per-kill gate check | ✓ WIRED | Confirmed at line 24, inside `CanDrop` (not `ModifyNPCLoot`). |
| `ItemDropRules/BossCoreDropRule.cs` (`TryDroppingItem`) | `Items/BossCoreItem.cs` (`BossKey`) | `Main.item[index].ModItem is BossCoreItem coreItem -> coreItem.BossKey = _bossKey` | ✓ WIRED | Confirmed at lines 36-37. |

All 6 declared key links across Plans 03-01/03-02 are wired correctly.

### Data-Flow Trace (Level 4)

Not applicable in the standard React/API sense — this is a game-logic pipeline, not a UI data-render chain. The equivalent trace (kill event → drop rule → carrier item → apply call → vanilla flag) is exactly what the live in-game test (03-03) empirically exercised end-to-end; static analysis alone could not have confirmed this (no automated test framework exists for tModLoader runtime behavior, per 03-VALIDATION.md). The live test is treated as this phase's Level 4 equivalent and is fully documented in 03-03-SUMMARY.md's 6 numbered steps.

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
|----------|---------|--------|--------|
| Full project builds cleanly against pinned SDK, including Phase 3's 4 new files | `dotnet build BossArenaSubWorld.csproj` | "빌드했습니다. 경고 0개 오류 0개" (Build succeeded, 0 warnings, 0 errors) | ✓ PASS |
| No TODO/FIXME/placeholder/stub markers in any Phase 3 file | grep scan of all 4 `.cs` files | No matches | ✓ PASS |
| All 5 phase Task commits present in git history | `git log --oneline` | `f20a9b2`, `70f328e`, `ce2a598`, `f7529a8` (feat commits) + `b89bfd1`/`2e71cb8`/`ffbc336` (docs commits) all present | ✓ PASS |

In-game runtime behavior (drop gating, cross-world survival, flag application, idempotency) cannot be re-run via automated spot-check in this environment — this is exactly what Plan 03-03's live human checkpoint already covered, and its 6-step result is treated as authoritative per this project's stated lack of an automated tModLoader test framework.

### Requirements Coverage

| Requirement | Source Plan(s) | Description | Status | Evidence |
|-------------|----------------|--------------|--------|----------|
| DROP-01 | 03-01 | Central NPC.type → bossKey mapping registers trackable bosses | ✓ SATISFIED | `BossRegistry._npcTypeToKey` + `TryGetKeyForNpc`, populated in `PostSetupContent`. |
| DROP-02 | 03-02, 03-03 | Registered bosses drop `BossCoreItem` via conditional `ItemDropRule`, gated to subworld kills | ✓ SATISFIED | `BossCoreDropRule.CanDrop` + live test Steps 1-2 (negative/positive gate confirmation). |
| DROP-03 | 03-01, 03-02, 03-03 | `BossCoreItem` stores `BossKey` as instance data, set at spawn time inside the drop rule | ✓ SATISFIED | `BossCoreItem.BossKey` + `Clone`/`SaveData`/`LoadData` + `BossCoreDropRule.TryDroppingItem` tagging + live test Steps 3-4 (cross-world survival). |
| APPLY-01 | 03-01, 03-03 | Using `BossCoreItem` calls `BossRegistry.Apply(key)`, which sets the downed flag | ✓ SATISFIED | `BossCoreItem.UseItem` → `BossRegistry.Apply` → `NPC.SetEventFlagCleared` + live test Step 5. |
| APPLY-04 | 03-01, 03-03 | Applying progress is idempotent — no double-apply / duplicate netcode on re-use | ✓ SATISFIED | `BossRegistry.Apply`'s `IsDowned()` pre-check + `BossCoreItem.UseItem`'s 3-way switch + live test Step 6 (distinct feedback, still consumed, no duplicate side effects). |

**Orphaned requirements check:** REQUIREMENTS.md's Traceability table maps exactly DROP-01, DROP-02, DROP-03, APPLY-01, APPLY-04 to "Phase 3 / Complete" — matching the 5 IDs declared across the three plans' frontmatter exactly. No orphaned requirements found.

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| — | — | No TODO/FIXME/placeholder/stub/empty-implementation patterns found in any of Phase 3's 4 owned files (`Systems/BossRegistry.cs`, `Items/BossCoreItem.cs`, `ItemDropRules/BossCoreDropRule.cs`, `GlobalNPCs/BossKillGlobalNPC.cs`) | — | — |
| `Subworlds/BossArenaSubworld.cs` | 47-160 (working-tree diff, not yet committed) | **Uncommitted dependency**: `OnEnter()`/`OnExit()` snapshot-restore fix for a confirmed SubworldLibrary v2.2.3.2 bidirectional vanilla-downed-flag leak (`CopyDowned()`/`ReadCopiedDowned()`) exists **only in the working directory** — `git log -- Subworlds/BossArenaSubworld.cs` shows a single commit (`3916f57`, Phase 1) with no fix. See "Critical Cross-Phase Finding" below. | 🛑 Blocker (reproducibility risk, not a Phase 3 task-completion gap) | If this file's uncommitted changes are lost (git reset, fresh clone, worktree cleanup), the vanilla `NPC.downedSlimeKing` (and ~33 other vanilla flags) leak back into the main world on every subworld exit **independent of the `BossCoreItem` mechanism**, silently defeating Phase 3's proven "applies exactly once, only via carrier item" guarantee for King Slime and any future vanilla boss. |

### Critical Cross-Phase Finding (Not a Phase 3 Gap — Urgent Commit Action Recommended)

During this verification, `git status` showed uncommitted changes to `Subworlds/BossArenaSubworld.cs` (a Phase 1-owned file, not in Phase 3's `files_modified` list). Investigation traced this to `.planning/debug/isolation-premise-flag-persistence.md` (status: resolved):

- Phase 1's Plan 04 live test (commit `79e2642`, "CRITICAL: live test contradicts isolation premise") found `NPC.downedSlimeKing` reads `True` in the main world after a subworld King Slime kill **with no carrier item used** — directly contradicting the isolation premise the entire Phase 3 carrier-item architecture depends on.
- A subsequent debug investigation (root cause confirmed via direct source read of SubworldLibrary's `SubworldSystem.cs`) found `CopyMainWorldData()`/`ReadCopiedMainWorldData()` unconditionally, bidirectionally sync a hardcoded whitelist of ~34 vanilla `NPC`/`DD2Event` "downed" flags (including `downedSlimeKing`) between the main world and any subworld, on every entry/exit, independent of `ShouldSave`/`NoPlayerSaving` and independent of any carrier-item mechanism.
- A defensive fix (`OnEnter()` snapshots the true pre-visit values of this exact flag whitelist; `OnExit()` force-restores them before SubworldLibrary's own `CopyMainWorldData()` can capture the corrupted in-subworld values) was written into `Subworlds/BossArenaSubworld.cs`, built clean (0 warnings/errors), and **live-verified by the user** to correctly restore isolation.
- **This fix was never `git commit`-ed.** `git log --all -- Subworlds/BossArenaSubworld.cs` shows only the original Phase 1 commit; the current working-tree diff (134 insertions) is the entire OnEnter/OnExit fix, sitting uncommitted.

**Why this matters for Phase 3's verified status:** Phase 3's live pipeline test (03-03-SUMMARY.md) was run chronologically *after* this fix was written to disk (per git commit ordering: Phase 1 fix session precedes Phase 2's 11:00 KST start, Phase 3 runs 13:27-15:13 KST), so the fix was almost certainly present in the working tree during Phase 3's actual live test and its `dotnet build` runs — meaning 03-03-SUMMARY.md's reported results are very likely a faithful, uncorrupted measurement of the real `BossRegistry`/`BossCoreItem` mechanism, not an artifact of the SubworldLibrary leak. This verification's own `dotnet build` (run just now, against the current working tree including this uncommitted fix) also succeeded clean, confirming the fix is still present and compiling correctly today.

However, **the committed git history (HEAD) does not contain this fix.** Anyone checking out this repository fresh — a teammate, CI, or a future `git reset`/worktree cleanup — would get a codebase where the vanilla flag leak is still active, silently breaking the exact "applies exactly once, only via explicit carrier-item action" guarantee Phase 3 exists to prove, specifically for the vanilla King Slime case this phase used as its worked example.

**Recommendation:** Commit `Subworlds/BossArenaSubworld.cs` as a standalone Phase 1 fix-up commit (e.g. `fix(01): restore vanilla downed-flag isolation across subworld round-trip`) before starting Phase 4 work, so the proven pipeline is actually reproducible from git history. This does not block Phase 3's own status (its own 4 files are complete, correct, and the live test evidence is credible) but is flagged here with Blocker severity because it threatens the reproducibility of everything Phase 3 (and by extension Phase 4+) is built on.

### Human Verification Required

None — Phase 3's live checkpoint (03-03) already satisfied the human-verification requirement for this phase's runtime behavior, and its 6-step result is accepted as authoritative per this project's documented lack of an automated tModLoader test framework.

One optional follow-up recommended to the user (not blocking): re-run the King Slime pipeline test once more after committing the `Subworlds/BossArenaSubworld.cs` fix, purely as a sanity check that committing the file (a no-op for the running game, since the working-tree bytes don't change) doesn't alter behavior — low risk, but cheap to confirm.

### Gaps Summary

No gaps found against Phase 3's own must-haves. All 4 Phase 3-owned artifacts (`Systems/BossRegistry.cs`, `Items/BossCoreItem.cs`, `ItemDropRules/BossCoreDropRule.cs`, `GlobalNPCs/BossKillGlobalNPC.cs`) exist, are substantive, and are correctly wired end-to-end. All 5 ROADMAP Success Criteria and all 5 requirement IDs (DROP-01, DROP-02, DROP-03, APPLY-01, APPLY-04) are backed by both static code evidence and a genuine, detailed live in-game test (03-03-SUMMARY.md) that a human operator ran and confirmed passing across all 6 numbered steps.

The one significant finding from this verification is cross-phase, not Phase-3-owned: a critical, currently-uncommitted fix in `Subworlds/BossArenaSubworld.cs` (Phase 1's file) is what makes the vanilla-boss isolation premise Phase 3 depends on actually hold. See "Critical Cross-Phase Finding" above. This is a git-hygiene/reproducibility risk, not a code-correctness or wiring gap in Phase 3's own deliverables, so it does not change this phase's `passed` status — but it should be committed promptly.

---

*Verified: 2026-08-13T06:19:10Z*
*Verifier: Claude (gsd-verifier)*
