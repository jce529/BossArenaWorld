# Roadmap: BossArenaSubWorld

## Overview

The journey starts by proving the subworld itself works in isolation — an empty, content-free dimension the player can reliably enter and exit, with the founding premise (downed flags don't cross the world boundary automatically) verified empirically rather than assumed. From there, the real player-facing entry mechanism is built: intercepting an existing boss-summon item, cancelling its main-world effect, and redirecting the player into the arena where the boss auto-summons. With entry solid, the core value proposition — the BossRegistry/BossCoreItem/GlobalNPC carrier-item pipeline — is proven end-to-end against one low-risk vanilla boss before any content-mod complexity is introduced. Calamity is then integrated first (its API is best-understood and its bosses exercise the hardest side-effect categories: netcode sync and WorldGen triggers), establishing the safe cross-mod access pattern and side-effect-reproduction discipline that every subsequent mod integration reuses. Spirit follows to prove the pattern generalizes to a structurally different (raw static-field) API. The remaining four mods (Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak) are then researched and integrated in two paired phases, since each is a bounded, similarly-shaped unit of work once the pattern is proven. The roadmap closes with a dedicated full-pipeline verification phase confirming every registered mod's bosses work end-to-end and that applied progress is actually recognized by external tracker mods, not just internally consistent.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 1: Subworld Skeleton & Isolation Proof** - An empty, content-free boss-arena subworld exists; player can enter/exit reliably; the world-flag isolation premise is verified empirically (completed 2026-08-13)
- [ ] **Phase 2: Summon-Item Redirect & Entry Registry** - Using a registered boss-summon item cancels its main-world effect and redirects the player into the subworld, where the boss auto-summons
- [x] **Phase 3: BossRegistry + BossCoreItem + GlobalNPC Pipeline (POC)** - Killing a registered boss in the subworld drops a carrier item that applies the boss's downed state, idempotently, in the main world — proven with one vanilla boss
 (completed 2026-08-13)
- [ ] **Phase 4: Calamity Integration & Cross-Mod Side-Effect Reproduction** - Calamity bosses are registered with full flag + netcode + WorldGen side-effect reproduction, establishing the safe cross-mod access pattern
- [ ] **Phase 5: Spirit Integration** - Spirit bosses are registered via their structurally different static-field API, proving the registry pattern generalizes
- [ ] **Phase 6: Redemption & CatalystMod Integration** - Both mods' downed-progress APIs are researched and their bosses registered
- [ ] **Phase 7: NoxusBoss & ContinentOfJourney/Daybreak Integration** - Both mods' downed-progress APIs are researched and their bosses registered, completing v1 mod coverage
- [ ] **Phase 8: Full Pipeline Verification & Tracker Confirmation** - The complete pipeline is verified end-to-end for every registered mod and confirmed recognized by external tracker mods

## Phase Details

### Phase 1: Subworld Skeleton & Isolation Proof
**Goal**: A dedicated boss-arena subworld exists that has never had any mod content placed in it, and the player can reliably enter and exit it, with the founding "flags don't cross worlds" premise proven rather than assumed.
**Depends on**: Nothing (first phase)
**Requirements**: SUBW-05, SUBW-06, VERIFY-02
**Success Criteria** (what must be TRUE):
  1. The boss-arena subworld generates with zero placed mod/vanilla content (custom `GenPass` list only) — no NPCs, structures, or ores from any installed mod are present.
  2. Player can enter the subworld and reliably exit/return to the main world without losing inventory or carried items.
  3. World-backup guidance is documented and followed before any live testing begins against a real save.
  4. Empirical test confirms a boss's downed flag does NOT propagate from the subworld back to the main world without an explicit carrier-item action — validates the premise the entire carrier-item architecture depends on.
**Plans**: 4 plans

Plans:
- [x] 01-01-PLAN.md — SDK pin (global.json), SubworldLibrary modReference, VERIFY-02 world-backup guidance doc
- [x] 01-02-PLAN.md — BossArenaSubworld Subworld subclass + FlatStonePlatformPass GenPass (SUBW-05)
- [x] 01-03-PLAN.md — Debug enter/exit/checkflag chat commands + generic biome-zone override hook (SUBW-06, D-09)
- [x] 01-04-PLAN.md — King Slime isolation-proof checkpoint test (SUBW-05, SUBW-06, VERIFY-02)

### Phase 2: Summon-Item Redirect & Entry Registry
**Goal**: Using an existing, registered boss-summon item redirects the player into the boss-arena subworld instead of summoning the boss in the main world, with the boss auto-summoning on arrival and the item preserved.
**Depends on**: Phase 1
**Requirements**: SUBW-01, SUBW-02, SUBW-03, SUBW-04
**Success Criteria** (what must be TRUE):
  1. A central registry maps existing summon items (vanilla or modded) to their target boss, keyed for redirect purposes.
  2. Using a registered summon item never spawns the boss in the main world — the normal summon effect is cancelled before anything else happens.
  3. After cancellation, the player is sent into the boss-arena subworld as the redirect's next step, with no separate portal item required.
  4. The target boss automatically summons inside the subworld once the player arrives, and the summon item itself is not consumed by the redirect.
**Plans**: 3 plans

Plans:
- [x] 02-01-PLAN.md — Summon-item registry (SUBW-01) + generic arrival auto-summon hook (SUBW-04 backend)
- [x] 02-02-PLAN.md — Test1 portal tile + placing item, right-click redirect trigger (SUBW-02, SUBW-03)
- [x] 02-03-PLAN.md — Live redirect verification checkpoint + Phase 1/2 debug tooling removal (SUBW-01..04)

### Phase 3: BossRegistry + BossCoreItem + GlobalNPC Pipeline (Proof of Concept)
**Goal**: Killing a registered boss inside the subworld reliably carries a boss-kill credential back to the main world and applies it exactly once, proven end-to-end with one low-risk vanilla boss before content-mod complexity is introduced.
**Depends on**: Phase 1, Phase 2
**Requirements**: DROP-01, DROP-02, DROP-03, APPLY-01, APPLY-04
**Success Criteria** (what must be TRUE):
  1. A central NPC.type → bossKey mapping registers trackable bosses, and killing a registered boss inside the boss-arena subworld drops a `BossCoreItem` tagged with that boss's key (via a conditional `ItemDropRule` gated to the subworld).
  2. `BossCoreItem` correctly carries its boss key as instance data across the subworld-to-main-world trip.
  3. Using `BossCoreItem` in the main world calls `BossRegistry.Apply(key)` and sets the corresponding boss's downed flag.
  4. Re-using a `BossCoreItem`, or using it again after a partial failure, does not double-apply rewards or duplicate side effects.
  5. The full pipeline (subworld kill → item drop → main-world apply) is demonstrated end-to-end in singleplayer with one vanilla boss, with a world backup taken first.
**Plans**: 3 plans

Plans:
- [x] 03-01-PLAN.md — BossRegistry (key -> BossDefinition, Apply/idempotency) + BossCoreItem (carrier item, UseItem -> Apply)
- [x] 03-02-PLAN.md — BossCoreDropRule (subworld-gated custom IItemDropRule) + BossKillGlobalNPC (ModifyNPCLoot wiring)
- [x] 03-03-PLAN.md — Live pipeline verification checkpoint (DROP-02, DROP-03, APPLY-01, APPLY-04 end-to-end)

### Phase 4: Calamity Integration & Cross-Mod Side-Effect Reproduction
**Goal**: Calamity bosses' full downed state (flag, netcode sync, and WorldGen side effects) is faithfully reproduced in the main world, and the safe cross-mod access pattern used by every later integration is established here.
**Depends on**: Phase 3
**Requirements**: MOD-01, APPLY-02, APPLY-03
**Success Criteria** (what must be TRUE):
  1. Calamity bosses are registered via the `DownedBossSystem` wrapper-property pattern, and their downed flags are set correctly by `BossRegistry.Apply`.
  2. Applying a Calamity boss's progress reproduces its netcode/messaging side effects (e.g. `CalamityNetcode.SyncWorld()`, `SetNewBossJustDowned()`), not just the flag.
  3. Applying a world-altering Calamity boss's progress (e.g. a mechanical boss) also triggers its WorldGen side effects (ore generation, dungeon activation, etc.).
  4. The mod continues to load and run safely with CalamityMod disabled — no JIT crash from the Calamity-specific integration code.
**Plans**: 2 plans

Plans:
- [x] 04-01-PLAN.md — weakReferences=CalamityMod build wiring + Integrations/CalamityIntegration.cs registering Hive Mind (MOD-01, APPLY-02, APPLY-03)
- [x] 04-02-PLAN.md — Live WorldGen/netcode checkpoint (D-04) + Calamity-disabled load-safety checkpoint (D-05)

### Phase 5: Spirit Integration
**Goal**: Spirit bosses' downed state is registered and applied correctly using Spirit's actual downed-progress API (BossDownedTracker's internal Dictionary<string,bool>, reached via reflection for writes and the public MyWorld property for reads -- corrected from the stale "MyWorld static-field" assumption per 05-CONTEXT.md D-01), proving the registry pattern generalizes across API shapes rather than being Calamity-specific.
**Depends on**: Phase 4
**Requirements**: MOD-02
**Success Criteria** (what must be TRUE):
  1. Spirit bosses are registered via SpiritMod's actual `BossDownedTracker` mechanism (public `MyWorld.DownedInfernon` read + reflection write into the internal `Downed` dictionary, since no public setter exists), and their downed flags are set correctly by `BossRegistry.Apply`.
  2. Player-scoped vs. world-scoped side effects for Spirit bosses are classified explicitly, so applying progress never double-grants anything the player already carries across the subworld boundary.
  3. The mod continues to load and run safely with Spirit disabled.
**Plans**: 2 plans

Plans:
- [x] 05-01-PLAN.md -- SpiritMod weakReferences build wiring + Integrations/SpiritIntegration.cs registering Infernon (MOD-02)
- [ ] 05-02-PLAN.md -- Live downed-flag/WorldGen checkpoint (D-05) + reflection-failure checkpoint + SpiritMod-disabled load-safety checkpoint (D-05)

### Phase 6: Redemption & CatalystMod Integration
**Goal**: Redemption and CatalystMod bosses' downed-progress APIs are researched and both mods' bosses are registered and applied correctly in the main world.
**Depends on**: Phase 5
**Requirements**: MOD-03, MOD-04
**Success Criteria** (what must be TRUE):
  1. Redemption's downed-progress API is identified via research, and at least one Redemption boss is registered with its downed state correctly applied in the main world.
  2. CatalystMod's downed-progress API is identified via research, and at least one CatalystMod boss is registered with its downed state correctly applied in the main world.
  3. Both integrations continue to load and run safely when their respective source mod is disabled.
**Plans**: TBD

### Phase 7: NoxusBoss & ContinentOfJourney/Daybreak Integration
**Goal**: NoxusBoss (Devourer of Universes) and ContinentOfJourney/Daybreak (Homeward series) bosses' downed-progress APIs are researched and registered, completing v1 mod coverage.
**Depends on**: Phase 6
**Requirements**: MOD-05, MOD-06
**Success Criteria** (what must be TRUE):
  1. NoxusBoss's downed-progress API is identified via research, and the Devourer of Universes is registered with its downed state correctly applied in the main world.
  2. ContinentOfJourney/Daybreak's downed-progress API is identified via research, and at least one of its bosses is registered with its downed state correctly applied in the main world.
  3. Both integrations continue to load and run safely when their respective source mod is disabled.
**Plans**: TBD

### Phase 8: Full Pipeline Verification & Tracker Confirmation
**Goal**: The complete subworld-kill-to-main-world-apply pipeline is verified end-to-end for every registered mod, and applied progress is confirmed recognized by external tracking tools, not just internally consistent.
**Depends on**: Phase 7
**Requirements**: VERIFY-01, VERIFY-03
**Success Criteria** (what must be TRUE):
  1. For at least one boss per registered mod (vanilla, Calamity, Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak), the full pipeline is verified end-to-end in singleplayer.
  2. Applied downed flags are confirmed recognized by Boss Checklist (or an equivalent tracker mod) after application.
  3. All verification runs are performed against a backed-up world save, per the guidance established in Phase 1.
**Plans**: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Subworld Skeleton & Isolation Proof | 4/4 | Complete   | 2026-08-13 |
| 2. Summon-Item Redirect & Entry Registry | 0/3 | Not started | - |
| 3. BossRegistry + BossCoreItem + GlobalNPC Pipeline (POC) | 3/3 | Complete   | 2026-08-13 |
| 4. Calamity Integration & Cross-Mod Side-Effect Reproduction | 1/2 | In Progress|  |
| 5. Spirit Integration | 0/2 | Not started | - |
| 6. Redemption & CatalystMod Integration | 0/TBD | Not started | - |
| 7. NoxusBoss & ContinentOfJourney/Daybreak Integration | 0/TBD | Not started | - |
| 8. Full Pipeline Verification & Tracker Confirmation | 0/TBD | Not started | - |
</content>
