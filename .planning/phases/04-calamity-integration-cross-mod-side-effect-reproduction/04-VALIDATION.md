---
phase: 04
slug: calamity-integration-cross-mod-side-effect-reproduction
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-08-13
---

# Phase 04 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — tModLoader mod, no automated in-game test harness (matches Phase 1-3's established precedent) |
| **Config file** | none |
| **Quick run command** | `dotnet build BossArenaSubWorld.csproj` |
| **Full suite command** | N/A — full verification is the two live in-game checkpoints below (D-04 WorldGen test, D-05 Calamity-disabled load test) |
| **Estimated runtime** | ~5s build; live checkpoints are manual/unbounded |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build BossArenaSubWorld.csproj`
- **After every plan wave:** Run `dotnet build BossArenaSubWorld.csproj`
- **Before `/gsd:verify-work`:** Both live checkpoints (D-04, D-05) must be green
- **Max feedback latency:** ~5 seconds (build); live checkpoints are user-paced

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 04-01-* | 01 | 1 | MOD-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 (`Integrations/CalamityIntegration.cs`) | ⬜ pending |
| 04-01-* | 01 | 1 | APPLY-02 | manual-only | live in-game: use carrier item, observe `CalamityUtils.BroadcastLocalizedText` chat message | ❌ W0 | ⬜ pending |
| 04-02-* | 02 | 2 | APPLY-03 | manual-only, dedicated test world (D-04) | live in-game: fresh CalamityMod-enabled world, use carrier item, confirm Aerialite ore tile conversion | ❌ W0 | ⬜ pending |
| 04-02-* | 02 | 2 | Success Criterion 4 | manual-only, real checkpoint (D-05) | disable CalamityMod, launch, confirm no JIT crash | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `Integrations/CalamityIntegration.cs` — new file, no automated test beyond the build gate
- [ ] `Libs/CalamityMod.dll` — must be extracted from `Mods/2026.6CalamityMod.tmod` into the project's `Libs/` folder before `dotnet build` resolves the new compile-time `<Reference>` (per-worktree manual step, same pattern as the existing `Libs/SubworldLibrary.dll` requirement)
- [ ] `build.txt` — add `weakReferences = CalamityMod@2.2.4`
- [ ] `.csproj` — add `<Reference Include="CalamityMod">` block (Private=false, matching the `SubworldLibrary` reference shape)

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Netcode/messaging side effect on apply | APPLY-02 | `CalamityNetcode.SyncWorld()` is a singleplayer no-op (gated on `Main.dedServ`) — not independently observable live; correctness is a code-review/decompile-match check | Use carrier item in main world; confirm the `CalamityUtils.BroadcastLocalizedText` chat message fires. `SyncWorld()` call itself verified by code review against decompiled `CalamityNetcode.cs`, not live observation |
| WorldGen side effect on apply | APPLY-03 | Ore-tile conversion is a real, permanent terrain mutation — needs a live world, not simulatable in a build step | Per D-04: create a fresh dedicated test world with CalamityMod enabled, locate a placed Aerialite ore tile, use carrier item, confirm the tile visually converts from disenchanted to enchanted variant |
| Mod loads safely with CalamityMod disabled | Success Criterion 4 | JIT crashes on mod-disabled load are a runtime-only failure mode; not reliably caught by code review alone | Per D-05: disable CalamityMod in Mod Configuration, launch tModLoader, confirm no crash/JIT exception appears in the client log |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify (build gate) or are flagged Wave 0 / manual-only above
- [ ] Sampling continuity: no 3 consecutive tasks without the automated build-gate check
- [ ] Wave 0 covers all missing references (`Libs/CalamityMod.dll`, `build.txt`, `.csproj`)
- [ ] No watch-mode flags
- [ ] Feedback latency < 10s (build gate)
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
