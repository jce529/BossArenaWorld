---
phase: 03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept
plan: 02
subsystem: gameplay-systems
tags: [tmodloader, itemdroprule, globalnpc, terraria, loot-pipeline, boss-registry]

# Dependency graph
requires:
  - phase: 03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept
    provides: BossRegistry.TryGetKeyForNpc/Apply and BossCoreItem.BossKey contracts from Plan 03-01
provides:
  - BossCoreDropRule custom IItemDropRule with per-kill dynamic subworld gate in CanDrop
  - BossKillGlobalNPC.ModifyNPCLoot wiring every BossRegistry-recognized NPC type to a BossCoreDropRule
affects: [03-03-plan (live in-game verification checkpoint of the full kill -> drop -> apply pipeline)]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Dynamic per-kill gating lives inside IItemDropRule.CanDrop, never inside GlobalNPC.ModifyNPCLoot (which only runs once per NPC type at mod load) -- template for any future conditional drop rule in this mod"
    - "NPC.GetSource_Loot requires a non-empty string context argument on this installed tModLoader version, contrary to some wiki paraphrases"

key-files:
  created: [ItemDropRules/BossCoreDropRule.cs, GlobalNPCs/BossKillGlobalNPC.cs]
  modified: []

key-decisions: []

patterns-established:
  - "Pattern 3: Conditional loot drops = custom IItemDropRule whose CanDrop re-evaluates the dynamic condition every kill, attached unconditionally via GlobalNPC.ModifyNPCLoot at mod-load time -- the gate never lives in the hook itself"

requirements-completed: [DROP-02, DROP-03]

# Metrics
duration: 4min
completed: 2026-08-13
---

# Phase 3 Plan 2: BossCoreDropRule & BossKillGlobalNPC Summary

**Custom IItemDropRule (BossCoreDropRule) gating BossCoreItem drops per-kill on SubworldSystem.IsActive<BossArenaSubworld>(), wired to every BossRegistry-registered NPC type via BossKillGlobalNPC.ModifyNPCLoot -- completes the compile-time kill-to-carrier-item pipeline**

## Performance

- **Duration:** 4 min
- **Started:** 2026-08-13T05:29:53Z
- **Completed:** 2026-08-13T05:32:04Z
- **Tasks:** 2
- **Files modified:** 2 (both created)

## Accomplishments
- BossCoreDropRule implements the full IItemDropRule interface, gating dynamically per-kill (not frozen at mod-load) via `SubworldSystem.IsActive<BossArenaSubworld>()` inside CanDrop
- BossCoreDropRule spawns a BossCoreItem via `Item.NewItem` and immediately tags the spawned instance's `BossKey` field with the constructor-supplied boss key, using the correct `GetSource_Loot(string context)` overload confirmed by 03-RESEARCH.md's reflection findings
- BossKillGlobalNPC.ModifyNPCLoot attaches a BossCoreDropRule to every NPC type BossRegistry.TryGetKeyForNpc recognizes, with zero dynamic gating logic of its own -- correctly delegating that responsibility entirely to the drop rule
- Full compile-time pipeline (BossRegistry -> BossCoreDropRule -> BossKillGlobalNPC -> BossCoreItem) now builds clean against installed tModLoader 1.4.4.9

## Task Commits

Each task was committed atomically:

1. **Task 1: Create BossCoreDropRule (ItemDropRules/BossCoreDropRule.cs)** - `ce2a598` (feat)
2. **Task 2: Create BossKillGlobalNPC (GlobalNPCs/BossKillGlobalNPC.cs)** - `f7529a8` (feat)

**Plan metadata:** (pending) docs: complete plan

## Files Created/Modified
- `ItemDropRules/BossCoreDropRule.cs` - Custom IItemDropRule: CanDrop gates on SubworldSystem.IsActive<BossArenaSubworld>() per kill; TryDroppingItem spawns BossCoreItem via Item.NewItem and sets BossKey on the spawned instance
- `GlobalNPCs/BossKillGlobalNPC.cs` - GlobalNPC.ModifyNPCLoot override that looks up BossRegistry.TryGetKeyForNpc(npc.type, ...) and adds a BossCoreDropRule(key) to npcLoot for every registered boss NPC type

## Decisions Made
None - plan's reference implementation (from 03-RESEARCH.md Pattern 2, already API-verified against the installed tModLoader.dll) was followed exactly with no compile errors or ambiguity.

## Deviations from Plan

None - plan executed exactly as written. Both files matched the plan's illustrative code verbatim and compiled successfully on the first attempt for both tasks.

## Issues Encountered
None.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Full compile-time pipeline complete: `Systems/BossRegistry.cs`, `Items/BossCoreItem.cs`, `ItemDropRules/BossCoreDropRule.cs`, `GlobalNPCs/BossKillGlobalNPC.cs` all exist and the project builds as a whole
- The subworld gate is confirmed dynamic (lives in `BossCoreDropRule.CanDrop`), not baked in at mod-load time -- `GlobalNPCs/BossKillGlobalNPC.cs` contains no `SubworldSystem.IsActive` reference
- Plan 03-03's live in-game verification checkpoint (kill King Slime inside the subworld -> confirm exactly one BossCoreItem tagged `vanilla:king_slime` drops; kill outside the subworld -> confirm no drop; use the item in the main world -> confirm `NPC.downedSlimeKing` flips true) is now unblocked
- No known stubs: both files are fully wired, non-placeholder implementations consuming Plan 03-01's real contracts

---
*Phase: 03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept*
*Completed: 2026-08-13*

## Self-Check: PASSED

- FOUND: ItemDropRules/BossCoreDropRule.cs
- FOUND: GlobalNPCs/BossKillGlobalNPC.cs
- FOUND: ce2a598 (Task 1 commit)
- FOUND: f7529a8 (Task 2 commit)
