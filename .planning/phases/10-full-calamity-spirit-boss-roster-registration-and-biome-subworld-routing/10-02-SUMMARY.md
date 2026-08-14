---
phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing
plan: 02
subsystem: cross-mod-integration
tags: [tmodloader, calamity, boss-registry, JITWhenModsEnabled, biome-routing]

# Dependency graph
requires:
  - phase: 10-01
    provides: SummonItemRegistry polymorphic resolver, ForcedTimeSystem, Test1Tile wiring (shared contracts this plan registers real bosses against)
  - phase: 04
    provides: Integrations/CalamityIntegration.cs's RegisterHiveMind()/IsHiveMindDowned()/ApplyHiveMindDowned() named-method template, BossRegistry/BossArenaRoutingRegistry APIs
  - phase: 09
    provides: BossArenaJungleSubworld (Jungle-biome arena, routing target for Dragonfolly)
provides:
  - "Integrations/CalamityIntegration.cs registers 4 more Calamity bosses: Devourer of Gods, Yharon, Supreme Witch Calamitas (all plain-arena), Dragonfolly (routed to BossArenaJungleSubworld)"
  - "calamity:devourer_of_gods, calamity:yharon, calamity:supreme_calamitas, calamity:dragonfolly BossRegistry keys"
affects: [10-03, 10-04, 10-05, 10-06]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Growing-single-file convention for Integrations/CalamityIntegration.cs: one RegisterX()/IsXDowned()/ApplyXDowned() named-method triplet per boss, all [JITWhenModsEnabled(\"CalamityMod\")]-tagged, called from PostSetupContent()"
    - "Plain-arena bosses (no BossArenaRoutingRegistry call) fall back to default BossArenaSubworld automatically; only genuinely biome-dependent bosses (Dragonfolly -> Jungle) get an explicit Register<T> call"

key-files:
  created: []
  modified:
    - Integrations/CalamityIntegration.cs

key-decisions:
  - "Fixed compile-blocking namespace bug in plan's illustrative code (Rule 3): CalamityMod.NPCs.CalamityGlobalTownNPC, not CalamityMod.CalamityGlobalTownNPC -- confirmed via ilspycmd decompile of the installed CalamityMod.dll"
  - "Copied gitignored Libs/*.dll compile-time references (CalamityMod.dll, SpiritMod.dll, SubworldLibrary.dll, etc.) from the main working tree into this fresh worktree before the first build succeeded -- known per-worktree setup gap documented since Phase 2/STATE.md, not a code defect"

patterns-established: []

requirements-completed: [ARENA-01]

# Metrics
duration: 15min
completed: 2026-08-14
---

# Phase 10 Plan 02: Calamity Tier-1 Boss Registration (Devourer of Gods, Yharon, Supreme Calamitas, Dragonfolly) Summary

**4 new Calamity bosses registered end-to-end in `Integrations/CalamityIntegration.cs` (3 plain-arena, 1 Jungle-routed), following Hive Mind's proven named-method/`[JITWhenModsEnabled]` template**

## Performance

- **Duration:** 15 min
- **Started:** 2026-08-14T (session continuation, Phase 10 Plan 02)
- **Completed:** 2026-08-14
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- Devourer of Gods (`calamity:devourer_of_gods`) and Yharon (`calamity:yharon`) registered, both plain-arena, replaying decompiled `OnKill()` shop-variable/ore-gen/broadcast side effects
- Supreme Witch Calamitas (`calamity:supreme_calamitas`) registered, plain-arena, `CeremonialUrn` summon item (Test1Tile bypasses the real hold-item/right-click-altar flow entirely)
- Dragonfolly (`calamity:dragonfolly`) registered and routed to `BossArenaJungleSubworld` -- a genuine functional dependency (AI's leave-Jungle enrage timer), not a thematic choice
- `dotnet build BossArenaSubWorld.csproj` passes with 0 warnings / 0 errors after both tasks
- Hive Mind's existing registration left unchanged

## Task Commits

Each task was committed atomically:

1. **Task 1: Register Devourer of Gods + Yharon** - `56bc0bc` (feat)
2. **Task 2: Register Supreme Witch, Calamitas + Dragonfolly** - `21d145d` (feat)

**Plan metadata:** (this commit) `docs(10-02): complete Calamity Tier-1 boss registration plan`

## Files Created/Modified
- `Integrations/CalamityIntegration.cs` - Added `RegisterDevourerOfGods`/`IsDevourerOfGodsDowned`/`ApplyDevourerOfGodsDowned`, `RegisterYharon`/`IsYharonDowned`/`ApplyYharonDowned`, `RegisterSupremeCalamitas`/`IsSupremeCalamitasDowned`/`ApplySupremeCalamitasDowned`, `RegisterDragonfolly`/`IsDragonfollyDowned`/`ApplyDragonfollyDowned`; extended `PostSetupContent()` to call all four `RegisterX()` methods after the existing `RegisterHiveMind()` call

## Decisions Made
- Namespace fix (Rule 3, blocking bug in plan's illustrative code): `CalamityGlobalTownNPC` is declared in `CalamityMod.NPCs`, not directly under `CalamityMod` -- corrected both call sites (Devourer of Gods, Yharon) before the build would compile. Confirmed via `ilspycmd -l c Libs/CalamityMod.dll` against the actually-installed DLL, consistent with this project's established "verify a plan's illustrative mod-API code against the real installed DLL" precedent (see STATE.md Phase 09 P05 decision).
- Per-worktree setup gap (not a plan/code defect): this fresh worktree had no `Libs/*.dll` compile-time references at session start (`CalamityMod.dll`, `SpiritMod.dll`, `SubworldLibrary.dll`, etc. are gitignored). Copied them from the main working tree before the first `dotnet build` could even parse the project's existing files, consistent with the documented Phase 2/09/06 precedent for this exact gap.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking compile error] Wrong fully-qualified namespace for CalamityGlobalTownNPC**
- **Found during:** Task 1 (Register Devourer of Gods + Yharon)
- **Issue:** Plan's illustrative code called `CalamityMod.CalamityGlobalTownNPC.SetNewShopVariable(...)`. The real type is `CalamityMod.NPCs.CalamityGlobalTownNPC` (confirmed via `ilspycmd -l c` decompile of the installed `Libs/CalamityMod.dll`), causing a CS0234 compile error that blocked both Task 1's Devourer of Gods and Yharon registrations from building.
- **Fix:** Corrected both call sites to `CalamityMod.NPCs.CalamityGlobalTownNPC.SetNewShopVariable(...)`, verified the method signature (`public static void SetNewShopVariable(int[] types, bool alreadySet)`) matches the call exactly.
- **Files modified:** `Integrations/CalamityIntegration.cs`
- **Verification:** `dotnet build BossArenaSubWorld.csproj` — 0 warnings, 0 errors after the fix
- **Committed in:** `56bc0bc` (part of Task 1 commit)

---

**Total deviations:** 1 auto-fixed (Rule 3)
**Impact on plan:** Necessary compile-correctness fix only, no scope creep. All four boss registrations match the plan's intended behavior and decompiled `OnKill()` fidelity exactly.

## Issues Encountered
This worktree was missing its gitignored `Libs/*.dll` compile-time references at session start (`CalamityMod.dll`, `CatalystMod.dll`, `ContinentOfJourney.dll`, `Redemption.dll`, `SpiritMod.dll`, `SubworldLibrary.dll`) — a known per-worktree setup gap already documented in STATE.md since Phase 02/05/06/09. Copied all six DLLs from the main working tree's `Libs/` directory before the first build attempt; not a code or plan defect, no commit needed (gitignored, runtime-resolved via `weakReferences` in production but required locally as compile-time `Reference` entries per Phase 01's decision).

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
`Integrations/CalamityIntegration.cs` now registers 5 Calamity bosses total (Hive Mind + this plan's 4). Ready for Plan 10-03 (parallel sibling, `Integrations/SpiritIntegration.cs`) and Plan 10-04 (Infernum-gated/polymorphic Calamity tier) to continue independently. ARENA-01 stays open in REQUIREMENTS.md until the full roster (Plans 10-02..10-06) lands and live in-game verification (Plan 10-06) confirms behavior — this plan's own acceptance criteria (build success, correct BossRegistry keys, correct routing) are code-level only, per plan scope.

---
*Phase: 10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing*
*Completed: 2026-08-14*

## Self-Check: PASSED

- FOUND: Integrations/CalamityIntegration.cs
- FOUND: .planning/phases/10-full-calamity-spirit-boss-roster-registration-and-biome-subworld-routing/10-02-SUMMARY.md
- FOUND: 56bc0bc (Task 1 commit)
- FOUND: 21d145d (Task 2 commit)
