---
phase: 06
slug: redemption-catalystmod-integration
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-08-14
---

# Phase 06 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — tModLoader mod, no automated in-game test harness (matches Phase 1-5/9's established precedent) |
| **Config file** | none |
| **Quick run command** | `dotnet build BossArenaSubWorld.csproj` |
| **Full suite command** | N/A — "full verification" is the live in-game checkpoints below, each requiring Redemption/CatalystMod to actually be installed+enabled first |
| **Estimated runtime** | ~10-20 seconds (build only; live checkpoints are manual) |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build BossArenaSubWorld.csproj` (requires `Libs/Redemption.dll` and `Libs/CatalystMod.dll` present locally)
- **After every plan wave:** Same build command
- **Before `/gsd:verify-work`:** All three live checkpoints (Thorn-downed-applies, Astrageldon-downed-applies, both mod-disabled checkpoints) must be green — blocked until Redemption/CatalystMod are re-enabled locally
- **Max feedback latency:** ~20 seconds (build gate only; live checkpoints are manual and unbounded)

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 06-01 | 01 | 0 | MOD-03/MOD-04 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 | ⬜ pending |
| 06-0x | TBD | 1 | MOD-03 | build (compile-time type check) | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 | ⬜ pending |
| 06-0x | TBD | 1 | MOD-03 / SC1 | manual-only, dedicated throwaway world | live in-game: kill Thorn in subworld, return, use `BossCoreItem`, confirm `RedeBossDowned.downedThorn` + chat message + Alignment change | ❌ W0 | ⬜ pending |
| 06-0x | TBD | 1 | MOD-04 | build (compile-time type check) | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 | ⬜ pending |
| 06-0x | TBD | 1 | MOD-04 / SC2 | manual-only, dedicated throwaway world | live in-game: kill Astrageldon in subworld, return, use `BossCoreItem`, confirm `WorldDefeats.downedAstrageldon` + `MetanovaGenerator.Generate()` ore-vein generation | ❌ W0 | ⬜ pending |
| 06-0x | TBD | final | SC3 | manual-only, real checkpoint | disable Redemption and (separately) CatalystMod in Mod Configuration, launch, confirm no `JITException` in client log | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `Libs/Redemption.dll` — copy from `ModReader\Redemption\Redemption.dll` (already extracted, no `.tmod` extraction needed)
- [ ] `Libs/CatalystMod.dll` — extract via `scripts/extract_tmod.py` against `D:\SteamLibrary\steamapps\workshop\content\1281930\2838015851\2026.6\CatalystMod.tmod`
- [ ] `build.txt` — add `weakReferences = ..., Redemption@0.8.0.4501, CatalystMod@1.1.8`
- [ ] `BossArenaSubWorld.csproj` — add the two `<Reference Include>` blocks for the new `Libs/*.dll` files
- [ ] Redemption and CatalystMod re-subscribed/enabled in the live `Mods\` folder before any live checkpoint (blocks live verification only, not compilation)

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|--------------------|
| Thorn downed-flag application in main world | MOD-03 / SC1 | No automated tModLoader in-game test harness exists in this project (established precedent since Phase 1) | Kill Thorn in the boss-arena subworld on a dedicated throwaway world, return to main world, use the dropped `BossCoreItem`, confirm `RedeBossDowned.downedThorn == true`, the chat broadcast fires, and the `Alignment` wrapper-property change + netcode sync replay correctly |
| Astrageldon downed-flag + WorldGen application | MOD-04 / SC2 | Same — no automated harness; also involves real WorldGen tile mutation that can only be observed live | Kill Astrageldon in the subworld on a dedicated throwaway world, return, use `BossCoreItem`, confirm `WorldDefeats.downedAstrageldon == true` and `MetanovaGenerator.Generate()` produces real ore-vein tiles, and that `NPC.SetEventFlagCleared` was called with the correct non-`-1` `gameEventId` |
| Redemption-disabled / CatalystMod-disabled load safety | SC3 | JIT-crash behavior can only be observed by actually disabling each mod and relaunching tModLoader — no static check can substitute (per Phase 4/5/9 precedent, including the real JITException caught live in Phase 09-07) | Disable Redemption only (leave CatalystMod/others as-is), relaunch, confirm no `JITException` naming `Integrations/RedemptionIntegration.cs` in `Logs/client.log`; re-enable; repeat for CatalystMod only |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references (`Libs/Redemption.dll`, `Libs/CatalystMod.dll`, `build.txt`, `.csproj`)
- [ ] No watch-mode flags
- [ ] Feedback latency < 20s (build gate)
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
