---
phase: 03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept
plan: 01
subsystem: gameplay-systems
tags: [tmodloader, modsystem, moditem, terraria, boss-registry, carrier-item]

# Dependency graph
requires:
  - phase: 02-summon-item-redirect-entry-registry
    provides: SummonItemRegistry ModSystem/PostSetupContent/static-Dictionary convention this plan extends
provides:
  - BossRegistry ModSystem with BossDefinition record, ApplyResult enum, Register/TryGetKeyForNpc/Apply
  - vanilla:king_slime registration via NPC.SetEventFlagCleared (flag-fidelity precedent for future mods)
  - BossCoreItem ModItem carrying BossKey instance data across Clone/SaveData/LoadData
affects: [03-02-plan (drop-side BossCoreDropRule + BossKillGlobalNPC will consume TryGetKeyForNpc/BossCoreItem.BossKey), 03-03-plan (live verification checkpoint)]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Idempotent apply via live IsDowned() getter check, no separate applied-tracking set (generalizes to future mod registrations)"
    - "BossDefinition record bundling NpcTypes[]/ApplyDowned/IsDowned per boss key, decoupling key from raw NPC.type"
    - "ModItem instance data (BossKey) persisted via CloneNewInstances=true + Clone override + SaveData/LoadData TagCompound round-trip"

key-files:
  created: [Systems/BossRegistry.cs, Items/BossCoreItem.cs, Items/BossCoreItem.png]
  modified: []

key-decisions:
  - "Fixed NpcTypes array literal from implicit short[] (NPCID.KingSlime is short) to explicit int[] to satisfy BossDefinition's int[] parameter type"
  - "Fixed CloneNewInstances override access modifier from public to protected to match the actual protected-only ModType<Item,ModItem> base member (installed tModLoader 1.4.4.9 API differs from plan's research assumption)"

patterns-established:
  - "Pattern 1: Boss registration = one BossDefinition (NpcTypes + ApplyDowned delegate replaying the source mod's real setter + IsDowned getter) registered by string key in PostSetupContent -- this is the template every future mod integration (Calamity, Spirit, etc.) will follow"
  - "Pattern 2: Carrier items follow BossCoreItem's exact shape (BossKey field + CloneNewInstances/Clone + SaveData/LoadData + UseItem 3-way switch on ApplyResult) for any future carrier-item type"

requirements-completed: [DROP-01, DROP-03, APPLY-01, APPLY-04]

# Metrics
duration: 6min
completed: 2026-08-13
---

# Phase 3 Plan 1: BossRegistry & BossCoreItem Summary

**BossRegistry ModSystem (idempotent Apply/TryGetKeyForNpc, vanilla:king_slime via NPC.SetEventFlagCleared) and BossCoreItem carrier item (BossKey persisted across Clone/SaveData/LoadData, UseItem wired to Apply) -- both compile clean against installed tModLoader 1.4.4.9**

## Performance

- **Duration:** 6 min
- **Started:** 2026-08-13T05:23:00Z
- **Completed:** 2026-08-13T05:29:03Z
- **Tasks:** 2
- **Files modified:** 3 (all created)

## Accomplishments
- BossRegistry provides the central key -> boss-definition map with idempotency logic (live IsDowned() check, no separate tracking set) that all future mod-boss registrations (Phase 4+) will extend
- vanilla:king_slime is registered as the proof-of-concept entry, replaying vanilla's own NPC.SetEventFlagCleared helper (flag + achievement notify + netcode sync + Lantern Night trigger) rather than a raw boolean assignment
- BossCoreItem carries BossKey reliably across inventory stack-splits (Clone) and world save/load (SaveData/LoadData), and its UseItem implements the exact consume-on-success-only policy (D-02): consumes on Applied/AlreadyDowned, retains with an explanatory chat message on UnknownKey

## Task Commits

Each task was committed atomically:

1. **Task 1: Create BossRegistry (Systems/BossRegistry.cs)** - `f20a9b2` (feat)
2. **Task 2: Create BossCoreItem (Items/BossCoreItem.cs) and its placeholder texture** - `70f328e` (feat)

**Plan metadata:** (pending) docs: complete plan

## Files Created/Modified
- `Systems/BossRegistry.cs` - BossDefinition record, ApplyResult enum, BossRegistry ModSystem (Register/TryGetKeyForNpc/Apply), registers vanilla:king_slime in PostSetupContent
- `Items/BossCoreItem.cs` - ModItem carrying BossKey instance data; Clone/SaveData/LoadData persistence; UseItem wired to BossRegistry.Apply
- `Items/BossCoreItem.png` - Placeholder texture copied from Test1Item.png precedent, avoids missing-texture rendering in-game

## Decisions Made
- Cast NpcTypes array literal to explicit `int[]` (NPCID.KingSlime resolves to `short`, causing a CS1503 compile error against the plan's literal `new[] { NPCID.KingSlime }` example) -- Rule 1 auto-fix, bug in the plan's illustrative code
- Changed `CloneNewInstances` override to `protected` instead of `public` (installed tModLoader 1.4.4.9's `ModType<Item, ModItem>.CloneNewInstances` is declared `protected`, not `public` as the plan's research assumed; overriding with a wider access modifier is a CS0507 compile error) -- Rule 1 auto-fix, research inaccuracy corrected against the real installed binary

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed short-to-int[] type mismatch in BossRegistry's NpcTypes literal**
- **Found during:** Task 1 (Create BossRegistry)
- **Issue:** `new[] { NPCID.KingSlime }` infers `short[]` because `NPCID.KingSlime` is a `short` constant, but `BossDefinition.NpcTypes` is typed `int[]` -- CS1503 compile error
- **Fix:** Changed to explicit `new int[] { NPCID.KingSlime }`
- **Files modified:** Systems/BossRegistry.cs
- **Verification:** `dotnet build BossArenaSubWorld.csproj` exits 0
- **Committed in:** f20a9b2 (Task 1 commit)

**2. [Rule 1 - Bug] Fixed CloneNewInstances access modifier mismatch in BossCoreItem**
- **Found during:** Task 2 (Create BossCoreItem)
- **Issue:** Plan's reference code declared `public override bool CloneNewInstances => true;`, but the installed tModLoader 1.4.4.9's `ModType<Item, ModItem>.CloneNewInstances` base member is `protected`, not `public` -- overriding with a wider access modifier is CS0507
- **Fix:** Changed to `protected override bool CloneNewInstances => true;`
- **Files modified:** Items/BossCoreItem.cs
- **Verification:** `dotnet build BossArenaSubWorld.csproj` exits 0
- **Committed in:** 70f328e (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (both Rule 1 - compile-blocking bugs in the plan's illustrative code, corrected against the real installed tModLoader.dll API)
**Impact on plan:** Both fixes were mechanical type/access-modifier corrections with zero behavioral change from the plan's intent. No scope creep.

## Issues Encountered
None beyond the two auto-fixed compile errors documented above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- `BossRegistry.TryGetKeyForNpc` and `BossRegistry.Apply` are available with the exact signatures Plan 03-02's `BossCoreDropRule`/`BossKillGlobalNPC` will consume
- `BossCoreItem` exposes the public `BossKey` field Plan 03-02's drop rule will set at spawn time
- No live-testable behavior yet -- BossCoreItem cannot be obtained in-game until Plan 03-02 adds the drop rule; live verification is deferred to Plan 03-03's checkpoint, consistent with this plan's own verification note

---
*Phase: 03-bossregistry-bosscoreitem-globalnpc-pipeline-proof-of-concept*
*Completed: 2026-08-13*

## Self-Check: PASSED

- FOUND: Systems/BossRegistry.cs
- FOUND: Items/BossCoreItem.cs
- FOUND: Items/BossCoreItem.png
- FOUND: f20a9b2 (Task 1 commit)
- FOUND: 70f328e (Task 2 commit)
