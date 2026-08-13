---
phase: 5
slug: spirit-integration
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-08-13
---

# Phase 5 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — tModLoader mod, no automated in-game test harness (matches Phase 1-4's established precedent, see `04-RESEARCH.md`) |
| **Config file** | none |
| **Quick run command** | `dotnet build BossArenaSubWorld.csproj` |
| **Full suite command** | N/A — "full verification" is the two live in-game checkpoints below (D-05), each requiring SpiritMod to actually be installed first |
| **Estimated runtime** | ~30 seconds (build) |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build BossArenaSubWorld.csproj`
- **After every plan wave:** Run `dotnet build BossArenaSubWorld.csproj` (0 warnings/errors expected; requires `Libs/SpiritMod.dll` present locally)
- **Before `/gsd:verify-work`:** Both live checkpoints (Infernon-downed-applies test, SpiritMod-disabled test) must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 05-01-xx | 01 | 0 | MOD-02 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 (new file: `Integrations/SpiritIntegration.cs`) | ⬜ pending |
| 05-0x-xx | TBD | TBD | MOD-02 / SC1 | manual-only | live in-game: kill Infernon in subworld, return, use `BossCoreItem`, check `MyWorld.DownedInfernon` | ❌ W0 | ⬜ pending |
| 05-0x-xx | TBD | TBD | MOD-02 / SC1 | manual-only, code-review-assisted | temporarily break the reflected field name, confirm `Mod.Logger.Warn` fires and `UseItem` doesn't throw, then revert | ❌ W0 | ⬜ pending |
| 05-0x-xx | TBD | TBD | SC3 | manual-only | disable SpiritMod in Mod Configuration, launch, confirm no crash/JIT exception in client log | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `Integrations/SpiritIntegration.cs` — new file, no automated test beyond the build gate
- [ ] `Libs/SpiritMod.dll` — copy from `ModReader/SpiritMod/SpiritMod.dll` (no `extract_tmod.py` run needed, unlike Calamity)
- [ ] `build.txt` — add `weakReferences = SpiritMod@1.5.0.44`
- [ ] `.csproj` — add the `<Reference Include="SpiritMod">` block
- [ ] SpiritMod re-subscribed/enabled in the live `Mods/` folder before the D-05 live checkpoints (blocks live verification only, not compilation)

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Downed flag applies to main world via carrier item | MOD-02 / SC1 | No in-game automated test harness; requires actual subworld round-trip and live world state read | Kill Infernon in the subworld, return to main world, use `BossCoreItem`, confirm `MyWorld.DownedInfernon` (via debug print or BossChecklist UI) becomes true |
| Reflection failure degrades gracefully | MOD-02 / SC1 | Requires deliberately breaking a field name in a debug build and observing log/crash behavior | Temporarily rename the reflected field, confirm `Mod.Logger.Warn` fires and `UseItem` does not throw, then revert |
| Mod loads safely with SpiritMod disabled | SC3 | Requires toggling mod enablement and observing client launch log, not reproducible via `dotnet build` | Disable SpiritMod in Mod Configuration, launch tModLoader, confirm no crash/JITException in the client log |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
