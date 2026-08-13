---
phase: 04-calamity-integration-cross-mod-side-effect-reproduction
verified: 2026-08-13T00:00:00Z
status: passed
score: 4/4 must-haves verified
---

# Phase 04: Calamity Integration — Cross-Mod Side-Effect Reproduction Verification Report

**Phase Goal:** Calamity bosses' full downed state (flag, netcode sync, and WorldGen side effects) is faithfully reproduced in the main world, and the safe cross-mod access pattern used by every later integration is established here.
**Verified:** 2026-08-13
**Status:** passed
**Re-verification:** No — initial verification

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Calamity bosses are registered via the `DownedBossSystem` wrapper-property pattern, and downed flags are set correctly by `BossRegistry.Apply` | ✓ VERIFIED | `Integrations/CalamityIntegration.cs:37-40` registers `calamity:hive_mind` into `BossRegistry.Register(...)`; `IsHiveMindDowned()` reads `CalamityMod.DownedBossSystem.downedHiveMind` and `ApplyHiveMindDowned()` sets it via the wrapper setter (`DownedBossSystem.downedHiveMind = true`, which internally calls `NPC.SetEventFlagCleared`), not a raw static field write |
| 2 | Applying a Calamity boss's progress reproduces its netcode/messaging side effects (`CalamityNetcode.SyncWorld()`, Sky Ore broadcast), not just the flag | ✓ VERIFIED | `ApplyHiveMindDowned()` calls `CalamityMod.CalamityNetcode.SyncWorld()` and `CalamityMod.CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.SkyOreText", Color.Cyan)`. Live-confirmed in 04-02-SUMMARY.md: both the "Boss credential applied" message and the real Calamity Sky Ore chat broadcast fired in-game |
| 3 | Applying a world-altering Calamity boss's progress also triggers its WorldGen side effects (ore generation) | ✓ VERIFIED | `ApplyHiveMindDowned()` calls `CalamityMod.World.AerialiteOreGen.Enchant()` inside the faithful two-flag guard (`!downedHiveMind && !downedPerforator`), matching Calamity's own `HiveMind.OnKill()` source exactly. Live-confirmed: a real, visible Aerialite Ore (Disenchanted) → Aerialite Ore tile conversion occurred on a throwaway test world (04-02-SUMMARY.md) |
| 4 | The mod continues to load and run safely with CalamityMod disabled — no JIT crash from the Calamity-specific integration code | ✓ VERIFIED | `PostSetupContent()` guards all Calamity-type access behind `ModLoader.HasMod("CalamityMod")`; all three Calamity-touching methods (`RegisterHiveMind`, `IsHiveMindDowned`, `ApplyHiveMindDowned`) carry `[JITWhenModsEnabled("CalamityMod")]`. A real JITException was found live during first-attempt testing (inline lambda hoisted to an untagged `<>c` cache-class method) and fixed in commit `0e19600` by extracting it into the named, tagged `IsHiveMindDowned()` method. Retest confirmed "load-safety verified" — clean load, clean `client.log`, Phase 2/3 features unaffected |

**Score:** 4/4 truths verified

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `Integrations/CalamityIntegration.cs` | ModSystem registering `calamity:hive_mind` into BossRegistry/SummonItemRegistry, isolated behind `[JITWhenModsEnabled("CalamityMod")]` | ✓ VERIFIED | Exists, 120 lines, contains `BossRegistry.Register("calamity:hive_mind"`, `SummonItemRegistry.Register(`, three `[JITWhenModsEnabled("CalamityMod")]` tags (one more than the original plan's two — the extra covers the extracted `IsHiveMindDowned()` fix), zero inline-lambda Calamity-type references remaining |
| `build.txt` | Declares `weakReferences = CalamityMod@2.2.4` | ✓ VERIFIED | Line present exactly as specified, version confirmed against the installed `.tmod`'s own header during extraction |
| `BossArenaSubWorld.csproj` | Compile-time-only `Reference` to `Libs/CalamityMod.dll`, mirroring the existing SubworldLibrary block | ✓ VERIFIED | `<Reference Include="CalamityMod" Condition="Exists('Libs\CalamityMod.dll')">` block present, `Private=false` |
| `scripts/extract_tmod.py` | Reusable `.tmod` extractor for future phases | ✓ VERIFIED | Exists, contains `TMOD` magic-header parsing logic |
| `Libs/CalamityMod.dll` | Local, gitignored compile-time reference | ✓ VERIFIED | Present on disk, non-zero size, correctly gitignored (not tracked) |

### Key Link Verification

| From | To | Via | Status | Details |
|------|-----|-----|--------|---------|
| `CalamityIntegration.cs` | `Systems/BossRegistry.cs` | `BossRegistry.Register("calamity:hive_mind", new BossDefinition(...))` | ✓ WIRED | Confirmed at line 37; `BossRegistry` itself is unmodified (boss-agnostic, Phase 3 code) |
| `CalamityIntegration.cs` | `Systems/SummonItemRegistry.cs` | `SummonItemRegistry.Register(itemType, npcType)` | ✓ WIRED | Confirmed at line 28 |
| `CalamityIntegration.cs` | `CalamityMod.DownedBossSystem.downedHiveMind` | Wrapper-property setter (`NPC.SetEventFlagCleared` internally) | ✓ WIRED | Confirmed at line 110; getter separately wired via `IsHiveMindDowned()` |
| `CalamityIntegration.cs` | `Systems/BossArenaRoutingRegistry.cs` | `BossArenaRoutingRegistry.Register<BossArenaCorruptionSubworld>(npcType)` | ✓ WIRED | Post-04-02 fix (commit `c275209`) routes Hive Mind to a dedicated Corruption-biome arena; consumed by `Test1Tile.RightClick`, `BossSummonPlayer.OnEnterWorld`, and `BossCoreDropRule.CanDrop`, all confirmed calling into the registry rather than a hardcoded arena type |

### Data-Flow Trace (Level 4)

Not applicable in the standard sense (no UI/render layer in this phase) — the equivalent trace is "does `BossRegistry.Apply("calamity:hive_mind")` actually reach real Calamity state rather than a stub." Traced: `BossCoreItem.UseItem` (Phase 3, unmodified) → `BossRegistry.Apply(key)` → `def.ApplyDowned()` → `ApplyHiveMindDowned()` → real `CalamityMod.DownedBossSystem`/`AerialiteOreGen`/`CalamityNetcode`/`CalamityUtils` calls. No static/empty fallback found; live-tested end-to-end per 04-02-SUMMARY.md.

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
|----------|---------|--------|--------|
| Project builds cleanly with CalamityMod reference wired in | `dotnet build BossArenaSubWorld.csproj` | 0 warnings, 0 errors | ✓ PASS |
| No stray TODO/placeholder markers in phase-04 files | grep for TODO/FIXME/placeholder across `Integrations/CalamityIntegration.cs`, `Systems/BossArenaRoutingRegistry.cs`, `Subworlds/BossArenaCorruptionSubworld.cs`, `Subworlds/CorruptionPlatformPass.cs` | No matches | ✓ PASS |
| In-game runtime behavior (Sky Ore broadcast, ore conversion, JIT-safety) | N/A — no automated test framework for in-game tModLoader behavior exists (per 04-VALIDATION.md) | Live checkpoints, both PASSED per 04-02-SUMMARY.md and conversation record | ? SKIP (covered by human checkpoint evidence, not re-flagged as a gap per task instructions) |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|-------------|-------------|--------|----------|
| MOD-01 | 04-01, 04-02 | Calamity bosses registered via the `DownedBossSystem` wrapper-property pattern | ✓ SATISFIED | `Integrations/CalamityIntegration.cs` registers Hive Mind via wrapper property, not raw field; REQUIREMENTS.md marks `[x]` and Traceability table shows `MOD-01 | Phase 4 | Complete` |
| APPLY-02 | 04-01, 04-02 | `BossRegistry.Apply` reproduces netcode/messaging side effects | ✓ SATISFIED | `CalamityNetcode.SyncWorld()` + `CalamityUtils.BroadcastLocalizedText(...)` calls present and live-confirmed firing; REQUIREMENTS.md `[x]`, Traceability `Complete` |
| APPLY-03 | 04-01, 04-02 | `BossRegistry.Apply` reproduces WorldGen side effects | ✓ SATISFIED | `AerialiteOreGen.Enchant()` call present and live-confirmed converting real tiles; REQUIREMENTS.md `[x]`, Traceability `Complete` |

No orphaned requirements found — REQUIREMENTS.md's Traceability table maps exactly MOD-01, APPLY-02, APPLY-03 to Phase 4, matching both plans' declared `requirements` frontmatter exactly.

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| — | — | None found | — | Scanned `Integrations/CalamityIntegration.cs`, `Systems/BossArenaRoutingRegistry.cs`, `Subworlds/BossArenaCorruptionSubworld.cs`, `Subworlds/CorruptionPlatformPass.cs` for TODO/FIXME/placeholder/stub markers and empty-implementation patterns — none present. All code paths are fully implemented, not stubbed. |

Note: a documentation-only discrepancy was flagged in 04-01-SUMMARY.md's own self-check — the file's explanatory comment text contains the literal string "SetNewBossJustDowned" (explaining why the call is deliberately omitted), which technically fails a plan acceptance-criteria string-negation check that was written without anticipating an explanatory comment. The actual code correctly omits the call itself. This is a cosmetic plan-wording mismatch, not a functional gap, and does not affect goal achievement.

### Human Verification Required

None outstanding. Both live in-game checkpoints required by 04-02-PLAN.md (Task 1: WorldGen/netcode/messaging side-effect verification; Task 2: Calamity-disabled load-safety verification) were already executed and confirmed passed by the user directly in conversation, including a real bug found and fixed mid-checkpoint (commit `0e19600`, retested and re-confirmed). Per this project's stated constraints (no automated test framework exists for in-game tModLoader behavior, per 04-VALIDATION.md), this live-checkpoint evidence recorded in 04-02-SUMMARY.md and the conversation record constitutes this phase's verification evidence for those criteria — not re-flagged here as pending.

### Gaps Summary

No gaps found. All four Success Criteria are met:

1. Hive Mind is registered via the `DownedBossSystem` wrapper-property pattern, code-verified.
2. Netcode/messaging side effects (`SyncWorld()`, Sky Ore broadcast) are faithfully replayed, code-verified and live-confirmed.
3. WorldGen side effects (`AerialiteOreGen.Enchant()`) are faithfully replayed, code-verified and live-confirmed.
4. The mod loads and runs safely with CalamityMod disabled — confirmed live after a real JIT-crash bug (inline lambda hoisting) was found and fixed (commit `0e19600`).

Additionally, a real behavioral bug (Hive Mind despawning in the plain arena due to missing `player.ZoneCorrupt`) was discovered during Plan 02's live testing and resolved via a follow-up debug session (commit `c275209`), which added `Subworlds/BossArenaCorruptionSubworld.cs`, `Subworlds/CorruptionPlatformPass.cs`, and `Systems/BossArenaRoutingRegistry.cs`, and generalized `Test1Tile`, `BossSummonPlayer`, and `BossCoreDropRule` to route/gate against any registered arena type rather than a hardcoded one. This work is fully committed, builds cleanly, and is consistent with — not contradictory to — this phase's must-haves; it was necessary to make the phase's own live verification checkpoint (Task 1) pass at all, and it establishes routing infrastructure that will be reused by future per-mod integrations exactly as the `weakReferences`/`[JITWhenModsEnabled]` pattern will be.

The project builds successfully (`dotnet build BossArenaSubWorld.csproj`: 0 warnings, 0 errors) with the full current state of all Phase 4 code, including the post-checkpoint fix commits.

---

*Verified: 2026-08-13*
*Verifier: Claude (gsd-verifier)*
