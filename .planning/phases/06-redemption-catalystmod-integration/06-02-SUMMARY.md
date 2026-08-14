---
phase: 06-redemption-catalystmod-integration
plan: 02
subsystem: infra
tags: [tmodloader, redemption, catalystmod, boss-registry, jitwhenmodsenabled, weak-references]

# Dependency graph
requires:
  - phase: 06-01
    provides: "Redemption@0.8.0.4501 and CatalystMod@1.1.8 wired as weak references in build.txt/csproj, Libs/Redemption.dll and Libs/CatalystMod.dll available for compile-time reference"
provides:
  - "Integrations/RedemptionIntegration.cs registering redemption:thorn into BossRegistry via Redemption's direct public-static-field API"
  - "Integrations/CatalystIntegration.cs registering catalyst:astrageldon into BossRegistry via CatalystMod's direct public-static-field API"
  - "SummonItemRegistry optional per-item eligibility delegate (canSummon), preserving source-mod CanUseItem() lockout semantics that the portal-redirect pipeline would otherwise bypass"
affects: [06-03, boss-registration-generalization]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Direct public-static-field write (Redemption.Globals.RedeBossDowned.downedThorn, CatalystMod.WorldDefeats.downedAstrageldon) as a third generalization of BossRegistry, distinct from Calamity's wrapper-property and Spirit's reflection-into-internal-dictionary shapes"
    - "SummonItemRegistry.Register(itemType, npcType, canSummon) optional eligibility delegate, mirroring BossDefinition's named-delegate (never lambda) convention, for source-mod items with a genuine CanUseItem() lockout condition"

key-files:
  created:
    - Integrations/RedemptionIntegration.cs
    - Integrations/CatalystIntegration.cs
  modified:
    - Systems/SummonItemRegistry.cs
    - Tiles/Test1Tile.cs

key-decisions:
  - "CatalystMod.MetanovaGenerator's real namespace is CatalystMod.Common.World.MetanovaGenerator, not CatalystMod.MetanovaGenerator as the plan's illustrative code assumed -- fixed via ilspycmd decompile (Rule 3)"
  - "User-approved scope addition: SummonItemRegistry gained an optional canSummon eligibility delegate to replicate AstralCommunicator's real Moon-Lord-lockout CanUseItem() behavior, which the portal-redirect pipeline would otherwise silently ignore"
  - "RegisterThorn() (Redemption) deliberately kept as the 2-arg Register call -- no equivalent lockout confirmed in research for Thorn"

patterns-established:
  - "Optional eligibility delegate on SummonItemRegistry, defaulting to true/allowed when unregistered, for future bosses whose source item has a genuine use-lockout condition"

requirements-completed: [MOD-03, MOD-04]

# Metrics
duration: 10min
completed: 2026-08-14
---

# Phase 06 Plan 02: Redemption & CatalystMod Boss Registration Summary

**Registered Redemption's Thorn and CatalystMod's Astrageldon into BossRegistry using each mod's direct public-static-field API (a third API shape beyond Calamity's wrapper properties and Spirit's reflection-only dictionary), plus a user-requested SummonItemRegistry eligibility-delegate extension that preserves Astrageldon's real Moon-Lord-lockout behavior across the portal-redirect pipeline.**

## Performance

- **Duration:** ~10 min
- **Completed:** 2026-08-14
- **Tasks:** 3 (2 planned + 1 user-approved scope addition)
- **Files modified:** 4 (2 created, 2 modified)

## Accomplishments

- `Integrations/RedemptionIntegration.cs` registers `redemption:thorn`, replaying `Thorn.OnKill()`'s real side effects: net-mode-aware chat broadcast, `RedeWorld.Alignment += 2`, and `ChaliceAlignmentUI.BroadcastDialogue(...)`
- `Integrations/CatalystIntegration.cs` registers `catalyst:astrageldon`, replaying `Astrageldon.OnKill()`'s real WorldGen side effect (`MetanovaGenerator.Generate()`) and its non-standard `-Type` `gameEventId` (not `-1`, per the project's established anti-simplification discipline)
- `SummonItemRegistry` extended with an optional named eligibility delegate (`canSummon`), and `Tiles/Test1Tile.cs` gated on it, so Astrageldon's summon can be silently refused once Moon Lord is downed but Astrageldon isn't -- matching the real item's own `CanUseItem()` lockout, which the project's portal-redirect design (Phase 2 D-09) otherwise bypasses entirely
- Both integrations follow the locked `[JITWhenModsEnabled]` + named-method (never inline lambda) discipline; `dotnet build BossArenaSubWorld.csproj` exits 0

## Task Commits

Each task was committed atomically:

1. **Task 1: Register Thorn via Integrations/RedemptionIntegration.cs** - `229b8bc` (feat)
2. **Task 2: Register Astrageldon via Integrations/CatalystIntegration.cs** - `35ac617` (feat)
3. **Task 3 (user-approved scope addition): SummonItemRegistry eligibility delegate + Test1Tile gate + CanSummonAstrageldon** - `1110c70` (feat)

## Files Created/Modified

- `Integrations/RedemptionIntegration.cs` - Registers `redemption:thorn`; `ApplyThornDowned` replays chat broadcast + Alignment change + dialogue broadcast; `IsThornDowned` reads `RedeBossDowned.downedThorn` directly
- `Integrations/CatalystIntegration.cs` - Registers `catalyst:astrageldon`; `ApplyAstrageldonDowned` replays `MetanovaGenerator.Generate()` and the `-Type` `SetEventFlagCleared` call; adds `CanSummonAstrageldon` eligibility delegate
- `Systems/SummonItemRegistry.cs` - Added optional `Func<bool> canSummon` parameter to `Register`, a second `_eligibility` dictionary, and `CanSummon(itemType)` (defaults to true when unregistered)
- `Tiles/Test1Tile.cs` - `RightClick` now checks `SummonItemRegistry.CanSummon(...)` after the existing `TryGetBoss` check, returning `false` (silent no-op) when ineligible

## Decisions Made

- Fixed `CatalystMod.MetanovaGenerator`'s namespace (Rule 3 - blocking compile error): the plan flagged this namespace as unverified in its Interfaces block; `dotnet build` produced CS0234 as anticipated, and `ilspycmd -l c Libs/CatalystMod.dll` confirmed the real fully-qualified path is `CatalystMod.Common.World.MetanovaGenerator` (public static `Generate(object state = null)`), not the bare `CatalystMod.MetanovaGenerator` in the plan's illustrative code
- User-approved scope addition (confirmed directly with the user this session, not a deviation): CatalystMod's real `AstralCommunicator.CanUseItem()` (decompiled and confirmed against `Libs/CatalystMod.dll` via `ilspycmd`) contains a Moon-Lord-lockout branch — if `NPC.downedMoonlord` is true and `WorldDefeats.downedAstrageldon` is still false, the real item becomes permanently unusable. Since this project's `SummonItemRegistry`/`Test1Tile` redirect pipeline bypasses `CanUseItem()`/`UseItem()` entirely by design (Phase 2 D-09), this would otherwise be silently ignored, letting a player summon Astrageldon via the arena portal in a state the source mod itself blocks. Implemented as a named, `[JITWhenModsEnabled("CatalystMod")]`-tagged static delegate (`CanSummonAstrageldon`), following the project's locked no-inline-lambda discipline, replicating only the Moon-Lord-lockout branch (not the biome/location/anti-duplicate-spawn checks, which have no equivalent in a portal-redirect flow). `RegisterThorn()` (Redemption) deliberately left as the 2-arg `Register` call — no equivalent lockout confirmed for Thorn during Phase 6 research.
- Neither integration calls `BossArenaRoutingRegistry.Register<T>()` — both Thorn and Astrageldon were confirmed via full decompiled-source read to have no `player.Zone*`/`CheckActive`-override biome dependency

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed CatalystMod.MetanovaGenerator's real namespace**
- **Found during:** Task 2 (`Integrations/CatalystIntegration.cs`)
- **Issue:** The plan's illustrative code referenced `CatalystMod.MetanovaGenerator`, but the plan's own Interfaces block flagged this namespace as unverified from 06-RESEARCH.md's prose-only research. `dotnet build` failed with CS0234 as anticipated.
- **Fix:** Ran `ilspycmd -l c Libs/CatalystMod.dll` to list all classes, found `CatalystMod.Common.World.MetanovaGenerator`, then `ilspycmd -t CatalystMod.Common.World.MetanovaGenerator Libs/CatalystMod.dll` to confirm `public static void Generate(object state = null)`. Updated the fully-qualified reference.
- **Files modified:** `Integrations/CatalystIntegration.cs`
- **Verification:** `dotnet build BossArenaSubWorld.csproj` exits 0
- **Committed in:** `35ac617` (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking namespace fix)
**Impact on plan:** Necessary for compilation; no scope creep. The user-requested `SummonItemRegistry` eligibility-delegate addition (Task 3) is documented separately above as an explicitly user-approved scope addition, not a deviation.

## Issues Encountered

- Fresh worktree was missing `Libs/Redemption.dll`, `Libs/CatalystMod.dll`, `Libs/SubworldLibrary.dll`, `Libs/CalamityMod.dll`, and `Libs/SpiritMod.dll` (known, previously-documented per-worktree gitignored-DLL setup gap — see 06-01-SUMMARY.md and STATE.md Phase 02 decision). Resolved by copying all five DLLs from the main working tree's `Libs/` folder before running `dotnet build`, per the same precedent as Plan 06-01.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Both Redemption's Thorn and CatalystMod's Astrageldon are registered end-to-end in `BossRegistry`/`SummonItemRegistry`, following the project's locked `[JITWhenModsEnabled]` + named-method discipline, and the project builds cleanly (`dotnet build` exit 0)
- Live in-game verification of both bosses (redirect, kill, carrier-item apply, side effects, and the new Astrageldon Moon-Lord-lockout gate) is deferred to Plan 06-03, per this plan's stated scope ("Live verification of runtime behavior happens in Plan 03")
- No blockers identified

---
*Phase: 06-redemption-catalystmod-integration*
*Completed: 2026-08-14*

## Self-Check: PASSED

- FOUND: Integrations/RedemptionIntegration.cs
- FOUND: Integrations/CatalystIntegration.cs
- FOUND: Systems/SummonItemRegistry.cs
- FOUND: Tiles/Test1Tile.cs
- FOUND commit: 229b8bc (Task 1)
- FOUND commit: 35ac617 (Task 2)
- FOUND commit: 1110c70 (Task 3)
