# Roadmap: BossArenaSubWorld

## Overview

The journey starts by proving the subworld itself works in isolation — an empty, content-free dimension the player can reliably enter and exit, with the founding premise (downed flags don't cross the world boundary automatically) verified empirically rather than assumed. From there, the real player-facing entry mechanism is built: intercepting an existing boss-summon item, cancelling its main-world effect, and redirecting the player into the arena where the boss auto-summons. With entry solid, the core value proposition — the BossRegistry/BossCoreItem/GlobalNPC carrier-item pipeline — is proven end-to-end against one low-risk vanilla boss before any content-mod complexity is introduced. Calamity is then integrated first (its API is best-understood and its bosses exercise the hardest side-effect categories: netcode sync and WorldGen triggers), establishing the safe cross-mod access pattern and side-effect-reproduction discipline that every subsequent mod integration reuses. Spirit follows to prove the pattern generalizes to a structurally different (raw static-field) API. The remaining three mods (Redemption, CatalystMod, ContinentOfJourney/Daybreak — identified as Homeward Journey, GabeHasWon, Steam Workshop id 2930931197) are then researched and integrated, since each is a bounded, similarly-shaped unit of work once the pattern is proven. NoxusBoss was removed from v1 scope during Phase 7 discuss-phase (2026-08-14) — most of its bosses are quest-triggered or already run in their own dedicated subworld mechanic, so they don't fit this project's carrier-item pattern. The roadmap closes with a dedicated full-pipeline verification phase confirming every registered mod's bosses work end-to-end and that applied progress is actually recognized by external tracker mods, not just internally consistent.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 1: Subworld Skeleton & Isolation Proof** - An empty, content-free boss-arena subworld exists; player can enter/exit reliably; the world-flag isolation premise is verified empirically (completed 2026-08-13)
- [x] **Phase 2: Summon-Item Redirect & Entry Registry** - Using a registered boss-summon item cancels its main-world effect and redirects the player into the subworld, where the boss auto-summons
- [x] **Phase 3: BossRegistry + BossCoreItem + GlobalNPC Pipeline (POC)** - Killing a registered boss in the subworld drops a carrier item that applies the boss's downed state, idempotently, in the main world — proven with one vanilla boss
 (completed 2026-08-13)
- [x] **Phase 4: Calamity Integration & Cross-Mod Side-Effect Reproduction** - Calamity bosses are registered with full flag + netcode + WorldGen side-effect reproduction, establishing the safe cross-mod access pattern
- [x] **Phase 5: Spirit Integration** - Spirit bosses are registered via their structurally different static-field API, proving the registry pattern generalizes (completed 2026-08-13)
- [ ] **Phase 6: Redemption & CatalystMod Integration** - Both mods' downed-progress APIs are researched and their bosses registered
- [ ] **Phase 7: ContinentOfJourney/Daybreak (Homeward Journey) Integration** - Homeward Journey's downed-progress API is researched and at least one of its bosses is registered, completing v1 mod coverage (NoxusBoss removed from scope, see Phase 7 discuss-phase decision)
- [ ] **Phase 8: Full Pipeline Verification & Tracker Confirmation** - The complete pipeline is verified end-to-end for every registered mod and confirmed recognized by external tracker mods
- [ ] **Phase 9: Biome-Dependent Subworld Coverage** - Every biome/Zone-dependent boss across all integrated mods has a matching routed subworld variant, audited systematically instead of discovered live in-game
- [ ] **Phase 10: Full Calamity/Spirit Boss Roster Registration & Biome Subworld Routing** - Every researched Calamity and Spirit boss not already registered is registered end-to-end and routed to its correct arena subworld, with Infernum-conditional gating and forced-night mechanics correctly implemented

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
- [x] 05-02-PLAN.md -- Live downed-flag/WorldGen checkpoint (D-05) + reflection-failure checkpoint + SpiritMod-disabled load-safety checkpoint (D-05)

### Phase 6: Redemption & CatalystMod Integration
**Goal**: Redemption and CatalystMod bosses' downed-progress APIs are researched and both mods' bosses are registered and applied correctly in the main world.
**Depends on**: Phase 5
**Requirements**: MOD-03, MOD-04
**Success Criteria** (what must be TRUE):
  1. Redemption's downed-progress API is identified via research, and at least one Redemption boss is registered with its downed state correctly applied in the main world.
  2. CatalystMod's downed-progress API is identified via research, and at least one CatalystMod boss is registered with its downed state correctly applied in the main world.
  3. Both integrations continue to load and run safely when their respective source mod is disabled.
**Plans**: 3 plans

Plans:
- [x] 06-01-PLAN.md — Extract Libs/Redemption.dll + Libs/CatalystMod.dll, wire build.txt/csproj weak references
- [x] 06-02-PLAN.md — Integrations/RedemptionIntegration.cs (Thorn) + Integrations/CatalystIntegration.cs (Astrageldon) registration
- [ ] 06-03-PLAN.md — Live downed-flag/WorldGen checkpoint (Thorn, Astrageldon) + Redemption/CatalystMod-disabled load-safety checkpoint

### Phase 7: ContinentOfJourney/Daybreak (Homeward Journey) Integration
**Goal**: ContinentOfJourney/Daybreak's downed-progress API is researched and at least one of its bosses is registered, completing v1 mod coverage.
**Depends on**: Phase 6
**Requirements**: MOD-06
**Success Criteria** (what must be TRUE):
  1. ContinentOfJourney/Daybreak's downed-progress API is identified via research, and at least one of its bosses is registered with its downed state correctly applied in the main world.
  2. The integration continues to load and run safely when the source mod is disabled.
**Plans**: 2 plans

Plans:
- [x] 07-01-PLAN.md — Wire ContinentOfJourney weak reference (build.txt/.csproj) + register Goblin Chariot into BossRegistry/SummonItemRegistry
- [ ] 07-02-PLAN.md — Live Goblin Chariot downed-flag/Boss Checklist verification + ContinentOfJourney-disabled load-safety checkpoint

> **Scope note (2026-08-14 discuss-phase):** "ContinentOfJourney" was identified as **Homeward Journey** by GabeHasWon (Steam Workshop id 2930931197) — two prior research passes (Phase 9 prep) could not resolve the name "ContinentOfJourney" directly; the user supplied this link during discussion, confirming the "(Homeward series)" phrase in this phase's original title was the actual pointer. "Daybreak" is confirmed to be `gold-meridian/daybreak-mod`, a boss-less library dependency of Wrath of the Gods — not a separate registration target. **NoxusBoss (Devourer of Universes) was removed from this phase's scope entirely** (was MOD-05/Success Criterion 1) — see `.planning/phases/07-*/07-CONTEXT.md` and `PROJECT.md` Key Decisions for rationale (quest-triggered/self-contained-subworld bosses don't fit the carrier-item pattern; no plan to revisit).

### Phase 8: Full Pipeline Verification & Tracker Confirmation
**Goal**: The complete subworld-kill-to-main-world-apply pipeline is verified end-to-end for every registered boss across every integrated mod — not just one representative boss per mod — and applied progress is confirmed recognized by external tracking tools, not just internally consistent.
**Depends on**: Phase 7 (and, per the expanded scope below, effectively also Phase 9/10 for their registered rosters to exist)
**Requirements**: VERIFY-01, VERIFY-03
**Success Criteria** (what must be TRUE):
  1. Every registered boss across every integrated mod (vanilla, Calamity — including Phase 10's full roster, Spirit — including Phase 10's full roster, Redemption, CatalystMod, ContinentOfJourney/Daybreak i.e. Homeward Journey) has its full subworld-kill-to-main-world-apply pipeline verified end-to-end in singleplayer, superseding the original "at least one boss per mod" scope.
  2. Applied downed flags are confirmed recognized by Boss Checklist (or an equivalent tracker mod) after application, for every boss in Success Criterion 1 — including King Slime, Hive Mind, Thorn, and Astrageldon, whose earlier phase checkpoints did not all explicitly confirm Boss Checklist recognition specifically (only Infernon/Phase 5 did; King Slime/Phase 3 allowed an alternative confirmation method, and Hive Mind/Phase 4 confirmed its own side effects but not Boss Checklist).
  3. All verification runs are performed against a backed-up world save, per the guidance established in Phase 1.
**Plans**: 4 plans

Plans:
- [ ] 08-01-PLAN.md — King Slime + Hive Mind Boss Checklist tracker-UI recognition closure, Infernon citation, Boss Checklist sanity check
- [ ] 08-02-PLAN.md — Thorn + Astrageldon live pipeline/Boss Checklist/Moon-Lord-lockout verification, Redemption/CatalystMod-disabled safety
- [ ] 08-03-PLAN.md — Goblin Chariot live pipeline/Boss Checklist verification, ContinentOfJourney-disabled safety
- [ ] 08-04-PLAN.md — Full Phase 10 Calamity/Spirit roster verification (blocked stub, gated on Phase 10 execution)

> **Scope note (2026-08-14 discuss-phase):** Originally scoped to "at least one boss per registered mod" before Phase 9 (biome routing) and Phase 10 (full Calamity/Spirit roster) were added to the roadmap. User explicitly expanded Phase 8 to cover every registered boss, reasoning that each mod-integration phase already proved its one-worked-example boss works — so Phase 8's remaining job is full-roster breadth, not re-proving the mechanism. See `.planning/phases/08-*/08-CONTEXT.md` for full rationale.

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 9 → 10

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Subworld Skeleton & Isolation Proof | 4/4 | Complete   | 2026-08-13 |
| 2. Summon-Item Redirect & Entry Registry | 3/3 | Complete | 2026-08-13 |
| 3. BossRegistry + BossCoreItem + GlobalNPC Pipeline (POC) | 3/3 | Complete   | 2026-08-13 |
| 4. Calamity Integration & Cross-Mod Side-Effect Reproduction | 2/2 | Complete | 2026-08-13 |
| 5. Spirit Integration | 2/2 | Complete | 2026-08-13 |
| 6. Redemption & CatalystMod Integration | 0/3 | Not started | - |
| 7. ContinentOfJourney/Daybreak (Homeward Journey) Integration | 0/2 | Not started | - |
| 8. Full Pipeline Verification & Tracker Confirmation | 0/TBD | Not started | - |
| 9. Biome-Dependent Subworld Coverage | 4/7 | In Progress|  |
| 10. Full Calamity/Spirit Boss Roster Registration & Biome Subworld Routing | 5/6 | In Progress|  |

### Phase 9: Biome-Dependent Subworld Coverage
**Goal**: Every v1-registered boss whose AI depends on a biome/Zone flag (the despawn-bug class found live with Calamity's Hive Mind in Phase 4, fixed there only ad-hoc via `BossArenaCorruptionSubworld`) has a matching routed biome-variant subworld, audited systematically across every integrated mod instead of being discovered live in-game per boss.
**Depends on**: Phase 8
**Requirements**: ARENA-01
**Success Criteria** (what must be TRUE):
  1. Every boss registered in Phases 4-7 (Calamity, Spirit, Redemption, CatalystMod, ContinentOfJourney/Daybreak i.e. Homeward Journey) is explicitly classified via source research as biome/Zone-dependent or not — extending the ad-hoc classification already done for Hive Mind (Phase 4, dependent) and Infernon (Phase 5, independent) to every remaining registered boss. (NoxusBoss removed from v1 scope in Phase 7 discuss-phase, 2026-08-14 — no longer applicable here. In practice, Phase 9's execution scoped its research to Calamity/Spirit/Redemption only; see `09-ALTAR-BIOME-REFERENCE.md` Scope note — Homeward Journey classification is Phase 7's own responsibility.)
  2. Every boss classified as biome/Zone-dependent has a matching `BossArenaXSubworld` variant registered via `BossArenaRoutingRegistry` (following the `BossArenaCorruptionSubworld` precedent), preventing the AI despawn/malfunction bug class found with Hive Mind. **Exception (D-07, user decision 2026-08-14):** Dungeon and Sulphurous Sea variants were built once then descoped and discarded mid-phase; Polterghast (Spirit, Dungeon) and The Old Duke (Calamity+Infernum, Sulphurous Sea) remain without a biome-safe arena until a future phase reinstates them.
  3. Classification and routing coverage is documented per boss, so future (post-v1) mod integrations can extend the same audit instead of re-discovering biome dependencies live in-game.
**Plans**: 7 plans (scope reduced from 9 to 7 biome variants mid-execution, 2026-08-14 — see 09-CONTEXT.md D-07)

Plans:
- [x] 09-01-PLAN.md — Underworld + Space biome subworlds (height-only family)
- [x] 09-02-PLAN.md — Hallow + Jungle biome subworlds (vanilla tile-weighted family)
- [x] 09-03-PLAN.md — Desert biome subworld (vanilla tile-weighted family, extra constraints) — Dungeon descoped, D-07
- [x] 09-04-PLAN.md — Astral + Briar biome subworlds (modded ModBiome family, JIT-safety discipline) — Sulphurous Sea descoped, D-07
- [x] 09-05-PLAN.md — Temporary debug entry mechanism for live verification
- [x] 09-06-PLAN.md — Live biome-flag verification checkpoints (7 subworlds)
- [x] 09-07-PLAN.md — CalamityMod/SpiritMod-disabled safety checkpoint + debug hook removal

### Phase 10: Full Calamity/Spirit Boss Roster Registration & Biome Subworld Routing

**Goal**: Every researched Calamity boss (Providence, Profaned Guardians, Ceaseless Void, The Old Duke, Signus, Storm Weaver, Astrum Deus, Astrum Aureus, Dragonfolly, Devourer of Gods, Yharon, Supreme Witch Calamitas) and Spirit boss (Ancient Avian, Scarabeus, Vinewrath Bane, Moon Jelly Wizard, Dusking, Atlas) not already registered in Phase 4/5 (Hive Mind, Infernon) is registered end-to-end in BossRegistry/SummonItemRegistry, with each boss's actual decompiled downed-progress side effects faithfully reproduced, routed to the correct Phase 9 biome-variant arena subworld where functionally required, and gated correctly across Infernum-present/absent mod combinations.
**Depends on**: Phase 9
**Requirements**: ARENA-01
**Success Criteria** (what must be TRUE):
  1. All 12 researched Calamity bosses and 6 Spirit bosses listed in the Goal above are registered end-to-end (summon item -> BossRegistry key -> BossCoreItem drop -> main-world Apply), matching each boss's actual decompiled `OnKill()` side effects (world-scoped only, per Pitfall 5 discipline).
  2. Providence, Profaned Guardians, and Ceaseless Void register ONLY when InfernumMode is absent; The Old Duke registers ONLY when InfernumMode is present -- verified live in both mod configurations.
  3. A summon item that spawns different bosses depending on the player's live Zone state (Ceaseless Void / Signus / Storm Weaver, sharing one item) resolves correctly via a new `SummonItemRegistry.RegisterPolymorphic` extension, with no silent single-item-overwrite regression.
  4. Astrum Deus/Astrum Aureus force night in their arena only when InfernumMode is loaded; Moon Jelly Wizard/Dusking force night unconditionally; forced night persists for the full fight duration via a new `ForcedTimeSystem`.
  5. Dragonfolly and Scarabeus are routed to their functionally-required biome arenas (Jungle, Desert) from Phase 9, not just thematically.
  6. The mod continues to load and run safely with CalamityMod disabled and (separately) SpiritMod disabled.
**Plans**: 6 plans

Plans:
- [x] 10-01-PLAN.md -- SummonItemRegistry polymorphic resolver + ForcedTimeSystem + Test1Tile wiring
- [x] 10-02-PLAN.md -- Calamity Tier 1: Devourer of Gods, Yharon, Supreme Witch Calamitas, Dragonfolly
- [x] 10-03-PLAN.md -- Spirit full roster: Ancient Avian, Scarabeus, Vinewrath Bane, Moon Jelly Wizard, Dusking, Atlas
- [x] 10-04-PLAN.md -- Calamity Tier 2: Providence, Profaned Guardians, Astrum Deus, Astrum Aureus, Ceaseless Void/Signus/Storm Weaver (polymorphic)
- [x] 10-05-PLAN.md -- The Old Duke: InfernumMode.dll wiring + Infernum-only registration
- [ ] 10-06-PLAN.md -- Live verification checkpoint + mod-disabled safety checkpoint
