---
phase: quick-260815-u7g-bossdefinition-requiresinfernumtoggle-ca
plan: 01
subsystem: infra
tags: [calamity, infernummode, boss-registry, refactor, reflection-free-flag]

# Dependency graph
requires:
  - phase: quick-260815-to6
    provides: "ForceInfernumModeActiveInArena() gated to Calamity-sourced bosses via bossKey.StartsWith(\"calamity:\") string-prefix heuristic"
provides:
  - "BossDefinition.RequiresInfernumToggle explicit per-boss flag (default false)"
  - "BossRegistry.TryGetDefinitionForNpc(int, out BossDefinition) accessor"
  - "Providence/Profaned Guardians/Astrum Deus/Astrum Aureus flagged RequiresInfernumToggle: true"
  - "catalyst:astrageldon explicitly confirmed and documented as NOT requiring the Infernum toggle (decompile-verified)"
affects: [calamity-integration, catalyst-integration, boss-summon-flow]

# Tech tracking
tech-stack:
  added: []
  patterns: ["Explicit per-entity boolean flag on a data record replacing a string-prefix/naming-convention inference at the call site"]

key-files:
  created: []
  modified:
    - Systems/BossRegistry.cs
    - Integrations/CalamityIntegration.cs
    - Integrations/CatalystIntegration.cs
    - Systems/BossSummonPlayer.cs

key-decisions:
  - "RequiresInfernumToggle added as a 4th optional named record parameter (default false) to BossDefinition -- source-compatible with all other mods' existing new BossDefinition(...) call sites (Spirit/HomewardJourney/Redemption), zero edits required there"
  - "catalyst:astrageldon deliberately left at RequiresInfernumToggle=false, confirmed correct via ilspycmd decompile of Libs/InfernumMode.dll (2097 types, zero references to Astrageldon or CatalystMod) -- documented in-code as a closed judgment call, not a deferred one"

patterns-established:
  - "Prefer an explicit boolean flag on the relevant data record over inferring behavior from a naming convention (string-prefix) at the call site -- closes silent-exclusion gaps for future entries that don't follow the convention (e.g. catalyst:astrageldon)"

requirements-completed: [ARENA-01]

# Metrics
duration: 2min
completed: 2026-08-15
---

# Quick Task 260815-u7g: BossDefinition RequiresInfernumToggle Flag Summary

**Replaced the `bossKey.StartsWith("calamity:")` string-prefix heuristic gating InfernumMode's forced-active toggle with an explicit `BossDefinition.RequiresInfernumToggle` flag, closing the `catalyst:astrageldon` silent-exclusion gap a code review flagged.**

## Performance

- **Duration:** 2 min
- **Started:** 2026-08-15T21:51:00+09:00
- **Completed:** 2026-08-15T21:52:48+09:00
- **Tasks:** 3
- **Files modified:** 4

## Accomplishments
- `BossDefinition` now carries a self-documenting `RequiresInfernumToggle` field (default `false`) instead of callers inferring the InfernumMode dependency from a boss key's mod-source prefix string
- `BossRegistry.TryGetDefinitionForNpc(int, out BossDefinition)` added alongside the unchanged `TryGetKeyForNpc`, giving callers direct access to per-boss flags without duplicating the key lookup
- Providence, Profaned Guardians, Astrum Deus, and Astrum Aureus (the four Calamity bosses that genuinely depend on InfernumMode's toggle) now set `RequiresInfernumToggle: true` explicitly
- `catalyst:astrageldon`'s lack of an InfernumMode dependency is now a documented, decompile-confirmed judgment call in-code rather than an implicit, silent gap
- `BossSummonPlayer.OnEnterWorld()` gates `ForceInfernumModeActiveInArena()` on the explicit flag, with zero behavioral change for any currently-registered boss

## Task Commits

Each task was committed atomically:

1. **Task 1: Add RequiresInfernumToggle flag and TryGetDefinitionForNpc accessor to BossRegistry** - `01f9547` (feat)
2. **Task 2: Set RequiresInfernumToggle on the four Calamity bosses; document Astrageldon's judgment call** - `4305c25` (feat)
3. **Task 3: Wire BossSummonPlayer to the explicit flag and update the doc comment; final build verification** - `954ee88` (refactor)

_No plan-metadata commit needed for quick tasks -- STATE.md update below is committed as part of the final commit for this task._

## Files Created/Modified
- `Systems/BossRegistry.cs` - `BossDefinition` record gains `RequiresInfernumToggle` (default `false`); new `TryGetDefinitionForNpc(int, out BossDefinition)` static accessor added after `TryGetKeyForNpc`
- `Integrations/CalamityIntegration.cs` - `calamity:providence`, `calamity:profaned_guardians`, `calamity:astrum_deus`, `calamity:astrum_aureus` registrations now pass `RequiresInfernumToggle: true`
- `Integrations/CatalystIntegration.cs` - `catalyst:astrageldon` registration unchanged (stays default `false`), preceded by an in-code comment documenting the `ilspycmd`-decompile-based judgment call
- `Systems/BossSummonPlayer.cs` - `OnEnterWorld()`'s InfernumMode-force-active guard now checks `BossRegistry.TryGetDefinitionForNpc(...)` + `def.RequiresInfernumToggle` instead of `BossRegistry.TryGetKeyForNpc(...)` + `bossKey.StartsWith("calamity:")`

## Decisions Made
- `RequiresInfernumToggle` added as a 4th optional named record parameter (default `false`) so all other mods' existing `new BossDefinition(...)` call sites (Spirit, HomewardJourney, Redemption) remain source-compatible with zero edits, as anticipated by the plan's `<interfaces>` section
- Astrageldon's exclusion is treated as a closed, confirmed judgment call (not a deferred question) per the plan's investigation: `ilspycmd` decompile of `Libs/InfernumMode.dll` (2097 types) found zero references to "Astrageldon" or "Catalyst" anywhere in the assembly

## Deviations from Plan

None - plan executed exactly as written. All three tasks matched their `<action>` blocks verbatim; each `dotnet build BossArenaSubWorld.csproj` verification passed with 0 warnings/0 errors on the first attempt.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

This is a pure refactor of an already-verified gating condition (Providence/Profaned Guardians/Astrum Deus/Astrum Aureus's Infernum-conditional gating, live-verified in Phase 10's `10-06` checkpoint) to an equivalent, more explicit representation. Behavioral equivalence confirmed by design: `ForceInfernumModeActiveInArena()` fires for the exact same four bosses before and after this change. No live in-game re-verification required per the plan's own `<verification>` section. No blockers for v1 -- this closes a code-review-flagged correctness gap on an already-complete, already-shipped mechanism.

---
*Quick task: 260815-u7g*
*Completed: 2026-08-15*

## Self-Check: PASSED

All modified files and all three task commits verified present via `git log --oneline --all` and filesystem check.
