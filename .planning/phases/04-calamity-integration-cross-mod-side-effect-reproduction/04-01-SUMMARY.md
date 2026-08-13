---
phase: 04-calamity-integration-cross-mod-side-effect-reproduction
plan: 01
subsystem: cross-mod-integration
tags: [tmodloader, calamity, weakReferences, JITWhenModsEnabled, jit-compilation, tmod-format]

# Dependency graph
requires:
  - phase: 03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept
    provides: "Generic BossRegistry (BossDefinition/Register/Apply) and SummonItemRegistry (Register/TryGetBoss), proven end-to-end with vanilla King Slime"
provides:
  - "scripts/extract_tmod.py, a reusable .tmod-format extractor (magic TMOD -> version string -> hash/signature -> file table -> deflate blobs)"
  - "Libs/CalamityMod.dll (gitignored, local), compile-time-only reference resolved via BossArenaSubWorld.csproj"
  - "build.txt weakReferences = CalamityMod@2.2.4 declaration"
  - "Integrations/CalamityIntegration.cs registering calamity:hive_mind into BossRegistry/SummonItemRegistry, isolated behind [JITWhenModsEnabled(\"CalamityMod\")]"
affects: [phase-05-spirit-integration, phase-06-redemption-catalystmod, phase-07-noxusboss-continentofjourney-daybreak, phase-04-plan-02-live-verification]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "weakReferences (build.txt) + [JITWhenModsEnabled(\"ModName\")] (per-method attribute) for safe cross-mod type access without hard-requiring the target mod be installed"
    - ".tmod extraction via scripts/extract_tmod.py to produce a local, gitignored compile-time Reference DLL (mirrors the existing SubworldLibrary.dll pattern from Phase 1) -- reusable for all future content-mod integrations"
    - "Faithful OnKill() side-effect replay (multi-flag guard conditionals, WorldGen calls, netcode sync, chat broadcast) instead of raw boolean flag assignment"

key-files:
  created:
    - scripts/extract_tmod.py
    - Integrations/CalamityIntegration.cs
  modified:
    - build.txt
    - BossArenaSubWorld.csproj

key-decisions:
  - "Confirmed installed CalamityMod version via extract_tmod.py header read: 2.2.4, matching build.txt's weakReferences = CalamityMod@2.2.4 declaration"
  - "Deliberately did NOT call CalamityGlobalNPC.SetNewBossJustDowned() in ApplyHiveMindDowned() per 04-RESEARCH.md's correction -- it is player-scoped speedrun-timer bookkeeping that already applied live during the actual subworld kill; replaying it from Apply() would double-apply"

patterns-established:
  - "Pattern: weakReferences + [JITWhenModsEnabled] cross-mod integration -- PostSetupContent() only ever calls ModLoader.HasMod() and a JIT-gated private method; every line touching a content-mod type lives inside a method carrying [JITWhenModsEnabled(\"ModName\")], so the JIT never compiles that method's IL (and therefore never resolves the mod's types) when the mod is absent"
  - "Pattern: .tmod extraction workflow -- run scripts/extract_tmod.py against the installed .tmod under %AppData%/../LocalLow/.../tModLoader/Mods (or wherever installed), move <ModName>.dll to Libs/<ModName>.dll, add a Condition=\"Exists(...)\" Reference block to the .csproj mirroring the existing SubworldLibrary block"

requirements-completed: [MOD-01, APPLY-02, APPLY-03]

# Metrics
duration: 5min
completed: 2026-08-13
---

# Phase 04 Plan 01: CalamityMod Weak Reference Wiring + Hive Mind Registration Summary

**Wired CalamityMod as a weakReferences/[JITWhenModsEnabled]-isolated dependency and registered Calamity's Hive Mind into the existing BossRegistry/SummonItemRegistry pipeline, faithfully replaying its real OnKill() netcode sync, chat broadcast, and WorldGen ore-enchant side effects.**

## Performance

- **Duration:** 5 min
- **Started:** 2026-08-13T16:32:00+09:00 (approx, first commit 16:32:45)
- **Completed:** 2026-08-13T16:33:29+09:00
- **Tasks:** 2 completed
- **Files modified:** 4 (2 created, 2 modified)

## Accomplishments
- Built `scripts/extract_tmod.py`, a reusable `.tmod`-format extractor, and ran it against the locally installed `2026.6CalamityMod.tmod` to produce `Libs/CalamityMod.dll` (gitignored, mirroring the existing `Libs/SubworldLibrary.dll` pattern from Phase 1)
- Declared `weakReferences = CalamityMod@2.2.4` in `build.txt` (version confirmed by reading the `.tmod`'s own header string during extraction, not guessed)
- Added a compile-time-only `<Reference Include="CalamityMod" Condition="Exists('Libs\CalamityMod.dll')">` block to `BossArenaSubWorld.csproj`, mirroring the existing SubworldLibrary block exactly (`Private=false`)
- Created `Integrations/CalamityIntegration.cs`, registering `calamity:hive_mind` into the unmodified, boss-agnostic `BossRegistry`/`SummonItemRegistry` pipeline from Phase 3, isolated entirely behind `ModLoader.HasMod("CalamityMod")` + `[JITWhenModsEnabled("CalamityMod")]`
- The registered `ApplyDowned` delegate faithfully replays Hive Mind's real `OnKill()` behavior: the exact two-flag guard (`!downedHiveMind && !downedPerforator`) before calling `AerialiteOreGen.Enchant()` and broadcasting the Sky Ore chat message, then sets `DownedBossSystem.downedHiveMind = true` (wrapper setter, internally calls `NPC.SetEventFlagCleared`) and `CalamityNetcode.SyncWorld()`
- `dotnet build BossArenaSubWorld.csproj` succeeds with exit code 0, 0 warnings, 0 errors, with the real CalamityMod types resolved

## Task Commits

Each task was committed atomically:

1. **Task 1: Extract Libs/CalamityMod.dll and wire the weakReferences build chain** - `2f51cba` (chore)
2. **Task 2: Register Hive Mind via Integrations/CalamityIntegration.cs** - `19eb2fe` (feat)

**Plan metadata:** (pending — final commit created after this SUMMARY)

## Files Created/Modified
- `scripts/extract_tmod.py` - Reusable `.tmod`-format extractor (magic `TMOD` -> version string -> SHA1 hash -> signature -> file table -> deflate blobs); reusable for all future content-mod integrations (Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak)
- `build.txt` - Added `weakReferences = CalamityMod@2.2.4` line after the existing `modReferences = SubworldLibrary` line
- `BossArenaSubWorld.csproj` - Added a second compile-time-only `<Reference>` block for CalamityMod, gated on `Exists('Libs\CalamityMod.dll')`
- `Integrations/CalamityIntegration.cs` - New `ModSystem` registering `calamity:hive_mind` into `BossRegistry`/`SummonItemRegistry`, isolated behind `[JITWhenModsEnabled("CalamityMod")]`

## Decisions Made
- Confirmed the installed CalamityMod's actual version (2.2.4) by reading the `.tmod` container's own version-string field during extraction, rather than trusting the plan's stated value blindly — it matched exactly, so `build.txt`'s `weakReferences = CalamityMod@2.2.4` line is empirically verified correct, not just copied from the plan.
- Followed 04-RESEARCH.md's explicit correction: `CalamityGlobalNPC.SetNewBossJustDowned()` is deliberately NOT called in `ApplyHiveMindDowned()` because it is player-scoped speedrun-timer bookkeeping that already ran for real during the live subworld kill (survives the exit since `Subworld.NoPlayerSaving = false`); replaying it here would double-apply.

## Deviations from Plan

None — plan executed exactly as written for both tasks. One documentation-level note (not a code deviation): the plan's own exact code content for `Integrations/CalamityIntegration.cs` includes an explanatory comment mentioning `CalamityGlobalNPC.SetNewBossJustDowned()` by name (explaining why it's deliberately *not* called), which technically means the acceptance-criteria check "Does NOT contain the string: SetNewBossJustDowned" does not hold for the comment text, even though the code itself correctly omits the actual call. The file was created with the plan's exact specified content as instructed; the intent of that acceptance criterion (no call to `SetNewBossJustDowned()`) is fully satisfied — this is a check-vs-code wording mismatch internal to the plan document, not a functional gap.

## Issues Encountered
None — `dotnet restore` and `dotnet build BossArenaSubWorld.csproj` succeeded on the first attempt after both tasks, with the known per-worktree `Libs/SubworldLibrary.dll` gap resolved by copying it in from the main working directory before starting (per this plan's `<known_worktree_gap>` briefing).

## User Setup Required

None - no external service configuration required. `Libs/CalamityMod.dll` and `Libs/SubworldLibrary.dll` are local, gitignored, developer-machine-specific compile-time references extracted from already-installed Workshop mods; each fresh worktree/clone needs to re-run the extraction (or copy the DLL) locally, as already documented for SubworldLibrary in Phase 1.

## Next Phase Readiness
- The weakReferences + `[JITWhenModsEnabled]` cross-mod integration pattern is now proven end-to-end at the code/build level and ready to be reused unchanged for Phase 5 (Spirit), Phase 6 (Redemption/CatalystMod), and Phase 7 (NoxusBoss/ContinentOfJourney/Daybreak).
- `scripts/extract_tmod.py` is a generic, reusable extractor — later phases can run it directly against their target mod's `.tmod` without rewriting extraction logic.
- Live in-game verification of `calamity:hive_mind`'s actual behavior (killing Hive Mind in the subworld, carrying the BossCoreItem back, applying it, confirming Sky Ore chat message + world ore conversion + no double-application) is explicitly out of scope for this plan and deferred to Plan 02 (D-04, D-05), which is the next plan in this phase.
- No blockers identified for Plan 02.

---
*Phase: 04-calamity-integration-cross-mod-side-effect-reproduction*
*Completed: 2026-08-13*

## Self-Check: PASSED

- FOUND: scripts/extract_tmod.py
- FOUND: Integrations/CalamityIntegration.cs
- FOUND: Libs/CalamityMod.dll (gitignored, local extraction — not committed)
- FOUND: commit 2f51cba (Task 1)
- FOUND: commit 19eb2fe (Task 2)
