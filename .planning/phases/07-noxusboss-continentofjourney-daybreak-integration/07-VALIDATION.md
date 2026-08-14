---
phase: 07
slug: noxusboss-continentofjourney-daybreak-integration
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-08-14
---

# Phase 07 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — tModLoader mod, no automated in-game test harness (matches Phase 1-6/9's established precedent) |
| **Config file** | none |
| **Quick run command** | `dotnet build BossArenaSubWorld.csproj` |
| **Full suite command** | N/A — "full verification" is the live in-game checkpoint below, requiring Homeward Journey (`ContinentOfJourney`) to actually be installed+enabled locally first |
| **Estimated runtime** | ~10-20 seconds (build only; live checkpoints are manual) |

---

## Sampling Rate

- **After every task commit:** `dotnet build BossArenaSubWorld.csproj` (requires `Libs/ContinentOfJourney.dll` present locally)
- **After every plan wave:** Same build command
- **Before `/gsd:verify-work`:** Both live checkpoints (Goblin-Chariot-downed-applies, mod-disabled load-safety) must be green — blocked until Homeward Journey is re-enabled locally
- **Max feedback latency:** ~20 seconds (build gate only; live checkpoints are manual and unbounded)

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 07-01 | 01 | 0 | MOD-06 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 | ⬜ pending |
| 07-0x | TBD | 1 | MOD-06 | build (compile-time type check) | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 | ⬜ pending |
| 07-0x | TBD | 1 | MOD-06 / SC1 | manual-only, dedicated throwaway world | live in-game: kill Goblin Chariot in subworld, return, use `BossCoreItem`, confirm `DownedBossSystem.downedGoblinChariot == true` + Boss Checklist recognition (07-RESEARCH.md Open Question 1) | ❌ W0 | ⬜ pending |
| 07-0x | TBD | final | SC2 | manual-only, real checkpoint | disable Homeward Journey (`ContinentOfJourney`) in Mod Configuration, launch, confirm no `JITException` naming `Integrations/HomewardJourneyIntegration.cs` in client log | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `Libs/ContinentOfJourney.dll` (+ `.pdb`) — already extracted this session via `scripts/extract_tmod.py` against `D:\SteamLibrary\steamapps\workshop\content\1281930\2930931197\2026.3\ContinentOfJourney.tmod` (v0.8.70.88); ready for the `.csproj` reference
- [ ] `build.txt` — add `weakReferences = ..., ContinentOfJourney@0.8.70.88`
- [ ] `BossArenaSubWorld.csproj` — add the one new `<Reference Include>` block for `Libs/ContinentOfJourney.dll`
- [ ] Homeward Journey re-subscribed/enabled in the live `Mods\` folder before any live checkpoint (blocks live verification only, not compilation)

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|--------------------|
| Goblin Chariot downed-flag application in main world | MOD-06 / SC1 | No automated tModLoader in-game test harness exists in this project (established precedent since Phase 1) | Kill Goblin Chariot in the boss-arena subworld on a dedicated throwaway world, return to main world, use the dropped `BossCoreItem`, confirm `ContinentOfJourney.DownedBossSystem.downedGoblinChariot == true` and that Homeward Journey's own bundled `CoJ_BossChecklist.cs` integration correctly surfaces the boss as downed in Boss Checklist's tracker UI (this closes 07-RESEARCH.md's Open Question 1 and doubles as this phase's contribution to Phase 8's VERIFY-03) |
| Homeward Journey (`ContinentOfJourney`)-disabled load safety | SC2 | JIT-crash behavior can only be observed by actually disabling the mod and relaunching tModLoader — no static check can substitute (per Phase 4/5/9 precedent, including the real JITException caught live in Phase 09-07) | Disable Homeward Journey only (leave others as-is), relaunch, confirm no `JITException` naming `Integrations/HomewardJourneyIntegration.cs` in `Logs/client.log`; re-enable |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references (`Libs/ContinentOfJourney.dll`, `build.txt`, `.csproj`)
- [ ] No watch-mode flags
- [ ] Feedback latency < 20s (build gate)
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
