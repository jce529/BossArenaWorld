---
phase: 9
slug: biome-dependent-subworld-coverage
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-08-14
---

# Phase 9 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | None — tModLoader mod, no automated in-game test harness (matches Phases 1-5's established precedent) |
| **Config file** | none |
| **Quick run command** | `dotnet build BossArenaSubWorld.csproj` |
| **Full suite command** | N/A — full verification is live in-game per-biome checkpoints (see Per-Task Verification Map / Manual-Only Verifications) |
| **Estimated runtime** | ~10-20s per build; live checkpoints not timed |

---

## Sampling Rate

- **After every task commit:** Run `dotnet build BossArenaSubWorld.csproj` (0 warnings/errors expected)
- **After every plan wave:** Run `dotnet build BossArenaSubWorld.csproj` again
- **Before `/gsd:verify-work`:** All 9 subworlds individually entered and Zone-flag-confirmed live, plus the CalamityMod/SpiritMod-disabled safety checkpoint, must be green
- **Max feedback latency:** ~20s (build-only signal); live checkpoints are manual and untimed

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 09-01-01 | 01 | 1 | ARENA-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 (new `Subworlds/BossArenaHallowSubworld.cs` + `HallowPlatformPass.cs`) | ⬜ pending |
| 09-01-02 | 01 | 1 | ARENA-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 (new `Subworlds/BossArenaUnderworldSubworld.cs` + `UnderworldPlatformPass.cs`) | ⬜ pending |
| 09-01-03 | 01 | 1 | ARENA-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 (new `Subworlds/BossArenaJunglePlatformSubworld.cs` + `JunglePlatformPass.cs`) | ⬜ pending |
| 09-01-04 | 01 | 1 | ARENA-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 (new `Subworlds/BossArenaSpaceSubworld.cs` + `SpacePlatformPass.cs`) | ⬜ pending |
| 09-01-05 | 01 | 1 | ARENA-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 (new `Subworlds/BossArenaDesertSubworld.cs` + `DesertPlatformPass.cs`) | ⬜ pending |
| 09-01-06 | 01 | 1 | ARENA-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 (new `Subworlds/BossArenaDungeonSubworld.cs` + `DungeonPlatformPass.cs`) | ⬜ pending |
| 09-02-01 | 02 | 2 | ARENA-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 (new `Subworlds/BossArenaAstralSubworld.cs` + `AstralPlatformPass.cs` — modded biome, JIT-safety discipline per Pitfall 4) | ⬜ pending |
| 09-02-02 | 02 | 2 | ARENA-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 (new `Subworlds/BossArenaSulphurousSubworld.cs` + `SulphurousPlatformPass.cs` — modded biome) | ⬜ pending |
| 09-02-03 | 02 | 2 | ARENA-01 | build | `dotnet build BossArenaSubWorld.csproj` | ❌ W0 (new `Subworlds/BossArenaBriarSubworld.cs` + `BriarPlatformPass.cs` — modded biome) | ⬜ pending |
| 09-03-01 | 03 | 3 | ARENA-01 | manual-only | live in-game: temporary debug hook enters each of the 9 subworlds in turn, confirms generation completes and player spawns correctly | ❌ W0 | ⬜ pending |
| 09-03-02 | 03 | 3 | ARENA-01 | manual-only | live in-game: confirm correct Zone/Biome flag reads `true` on each platform (vanilla flags via debug print; Calamity via `player.Calamity().ZoneAstral`/`ZoneSulphur`; Spirit via `BiomeTileCounts.InBriar`) | ❌ W0 | ⬜ pending |
| 09-03-03 | 03 | 3 | ARENA-01 | manual-only, real checkpoint (mirrors Phase 4/5's D-05) | disable CalamityMod (and separately SpiritMod) in Mod Configuration, launch, confirm no JITException naming any of the 3 modded-biome subworld/platformpass classes | ❌ W0 | ⬜ pending |

*Task IDs above are illustrative groupings pending the planner's actual wave/task breakdown — the planner may combine or split these differently; this table's rows should be reconciled against the final PLAN.md files during Wave 0.*

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `Subworlds/BossArenaHallowSubworld.cs` + `Subworlds/HallowPlatformPass.cs` — new, no automated test beyond build gate
- [ ] `Subworlds/BossArenaUnderworldSubworld.cs` + `Subworlds/UnderworldPlatformPass.cs` — new (height-only, no tile-weight fill per research)
- [ ] `Subworlds/BossArenaJungleSubworld.cs` + `Subworlds/JunglePlatformPass.cs` — new (full-thickness JungleGrass, not thin veneer)
- [ ] `Subworlds/BossArenaSpaceSubworld.cs` + `Subworlds/SpacePlatformPass.cs` — new (height-only)
- [ ] `Subworlds/BossArenaDesertSubworld.cs` + `Subworlds/DesertPlatformPass.cs` — new
- [ ] `Subworlds/BossArenaDungeonSubworld.cs` + `Subworlds/DungeonPlatformPass.cs` — new (requires Unsafe Dungeon wall variant, not just tiles)
- [ ] `Subworlds/BossArenaAstralSubworld.cs` + `Subworlds/AstralPlatformPass.cs` — new, modded biome (Calamity `BiomeTileCounterSystem`), JIT-safety discipline required (Pitfall 4: no Calamity type refs outside lazy `Tasks`/`ApplyPass()`)
- [ ] `Subworlds/BossArenaSulphurousSubworld.cs` + `Subworlds/SulphurousPlatformPass.cs` — new, modded biome, same JIT-safety discipline
- [ ] `Subworlds/BossArenaBriarSubworld.cs` + `Subworlds/BriarPlatformPass.cs` — new, modded biome (Spirit `BiomeTileCounts`), same JIT-safety discipline
- [ ] A temporary debug entry mechanism to individually reach each new subworld for live verification (D-02 forbids any new permanent player-facing entry point) — short-lived debug chat command, removed after this phase's checkpoints pass, mirroring Phase 1/2's now-deleted `Debug/SubworldDebugCommands.cs` precedent

*18 new source files total (9 Subworld + 9 GenPass), plus one temporary debug-only entry mechanism not intended to survive past this phase's live verification.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Each of the 9 subworlds generates and is enterable without crashing | ARENA-01 | No in-game automated test harness exists for this project (established precedent, Phases 1-5) | Use the temporary debug hook to enter each subworld in turn; confirm no crash, player spawns above the platform surface |
| Correct Zone/Biome flag reads true while standing on each platform | ARENA-01 | Requires live game state inspection, not just code review | Temporary debug print of the relevant flag per biome (see Per-Task Verification Map row 09-03-02 for the exact flag names) while standing on the platform |
| Mod loads safely with CalamityMod/SpiritMod disabled | ARENA-01 | JIT-safety can only be confirmed by actually disabling the mod and observing load behavior, not by static analysis alone | Disable CalamityMod (then separately SpiritMod) in Mod Configuration, relaunch, confirm no JITException in the client log naming any of the 3 modded-biome classes |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify (build gate) or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references (18 new files + temporary debug hook, listed above)
- [ ] No watch-mode flags
- [ ] Feedback latency < 20s (build gate)
- [ ] `nyquist_compliant: true` set in frontmatter once planner confirms task/wave breakdown matches this map

**Approval:** pending
