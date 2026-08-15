# BossArenaSubWorld

## What This Is

A Terraria tModLoader mod that lets a player fight lag-heavy bosses (Moon Lord, Infernum/Wrath-reworked bosses, and other bosses from large content mods) inside a dedicated subworld that has never had any mod content placed in it, then carries the boss-kill progress back to the main world. Solves severe FPS crashes (40-50 → 1-2) caused by running multiple large content mods (Calamity, Spirit, Redemption, etc.) simultaneously during heavy boss fights.

## Current State (as of v1.0, shipped 2026-08-15)

v1.0 MVP is shipped: the full subworld-kill → carrier-item → main-world-apply pipeline is live-verified end-to-end for **17 bosses across 5 content mods** (Calamity, Spirit, Redemption, CatalystMod, Homeward Journey/ContinentOfJourney), including biome-dependent arena routing (7 biome variants) and InfernumMode-conditional gating. See `.planning/milestones/v1.0-ROADMAP.md` and `.planning/milestones/v1.0-REQUIREMENTS.md` for full archived detail, and `.planning/MILESTONES.md` for the shipped-accomplishments summary.

Deliberately out of v1: The Old Duke (Calamity boss, despawn bug root-caused and a general fix kept, but the boss itself descoped by user decision), NoxusBoss, Exo Mechs, Starplate Voyager, Dungeon/Sulphurous Sea biome variants, and multiplayer — see Out of Scope below.

## Next Milestone Goals

Not yet defined. Run `/gsd:new-milestone` to scope v1.1 (candidates surfaced during v1.0: Dungeon/Sulphurous Sea biome variants for Polterghast, a re-attempt at The Old Duke, multiplayer support).

## Core Value

The generic boss-kill → carrier-item → main-world-apply mechanism (BossRegistry + BossCoreItem + GlobalNPC) must reliably reproduce a boss's full "downed" state in the main world — flags, netcode sync, and any WorldGen side effects — for any registered boss. If this pipeline doesn't work end-to-end, nothing else matters.

## Requirements

### Validated

- [x] Right-clicking a new placeable portal tile (working name "Test1", benchmarked off the Corruption Altar sprite) while holding an existing, registered boss-summon item (vanilla or modded) redirects the player into a dedicated boss-arena subworld (no mod content ever placed) instead of summoning in the main world — the boss auto-summons once inside by replaying the held item's own use-effect — Validated in Phase 2: Summon-Item Redirect & Entry Registry (live in-game test confirmed, 2026-08-13)
- [x] Killing a registered boss in the subworld drops a BossCoreItem carrying that boss's key, gated dynamically per-kill (not baked in at mod load) via a custom `IItemDropRule` attached through `GlobalNPC.ModifyNPCLoot` — Validated in Phase 3: BossRegistry + BossCoreItem + GlobalNPC Pipeline (POC) (live in-game test confirmed, 2026-08-13)
- [x] Using BossCoreItem in the main world applies the boss's downed flag via `BossRegistry.Apply(key)`, replaying vanilla's own `NPC.SetEventFlagCleared` fidelity path, idempotently (re-use after already-downed is a no-op with distinct feedback, no duplicate side effects) — Validated in Phase 3: BossRegistry + BossCoreItem + GlobalNPC Pipeline (POC) (live in-game test confirmed, 2026-08-13)
- [x] Full pipeline verified end-to-end in singleplayer (subworld kill → item → main world apply), with world backup before testing — proven with one low-risk vanilla boss (King Slime) — Validated in Phase 3: BossRegistry + BossCoreItem + GlobalNPC Pipeline (POC) (live in-game test confirmed, 2026-08-13). Content-mod-specific reproduction (Calamity/Spirit/etc.) remains Active below.
- [x] Boss-specific side effects (netcode sync calls, "boss just downed" messages) are reproduced when the item is used, matching each source mod's original OnKill behavior — Validated in Phase 4: Calamity Integration & Cross-Mod Side-Effect Reproduction (live in-game test confirmed for Hive Mind: Sky Ore chat broadcast + CalamityNetcode.SyncWorld(), 2026-08-13)
- [x] World-altering bosses also trigger their WorldGen side effects (ore generation, dungeon activation, etc.) when the item is used in the main world — Validated in Phase 4: Calamity Integration & Cross-Mod Side-Effect Reproduction (live in-game test confirmed: Hive Mind's AerialiteOreGen.Enchant() converted real Aerialite Ore tiles on BossCoreItem use, 2026-08-13)
- [x] Calamity bosses registered via `DownedBossSystem` pattern — Validated in Phase 4: Calamity Integration & Cross-Mod Side-Effect Reproduction (Hive Mind registered end-to-end via `Integrations/CalamityIntegration.cs`; `[JITWhenModsEnabled("CalamityMod")]` isolation confirmed safe live with CalamityMod disabled, 2026-08-13)
- [x] Spirit bosses registered via their actual `BossDownedTracker` API (an internal `Dictionary<string,bool>`, not the plain `MyWorld` static-field pattern originally assumed) — Validated in Phase 5: Spirit Integration (Infernon/InfernoSkull registered end-to-end via `Integrations/SpiritIntegration.cs`: public `MyWorld.DownedInfernon` read + cached-reflection write into the internal `Downed` dictionary; live in-game test confirmed downed-flag application, WorldGen tile-ring replay, and BossChecklist recognition; `[JITWhenModsEnabled("SpiritMod")]` isolation confirmed safe live with SpiritMod disabled, 2026-08-13)
- [x] Redemption bosses (Thorn) and CatalystMod bosses (Astrageldon) researched and registered — Validated in Phase 6: Redemption & CatalystMod Integration (`Integrations/RedemptionIntegration.cs`/`Integrations/CatalystIntegration.cs`, direct public-static-field writes; live in-game verified end-to-end, closed via Phase 8's `08-02-SUMMARY.md`, 2026-08-15)
- [x] ContinentOfJourney / Daybreak (identified as Homeward Journey, GabeHasWon, Steam Workshop id 2930931197) bosses researched and registered — Validated in Phase 7: ContinentOfJourney/Daybreak (Homeward Journey) Integration (Goblin Chariot registered via `Integrations/HomewardJourneyIntegration.cs`, live in-game verified end-to-end including Boss Checklist recognition and mod-disabled JIT safety, 2026-08-14)
- [x] Every biome/Zone-dependent registered boss has a matching routed biome-variant arena subworld (7 variants: Hallow, Underworld, Jungle, Space, Desert, Astral, Briar), audited systematically rather than discovered live per boss — Validated in Phase 9: Biome-Dependent Subworld Coverage + Phase 10 routing (all 7 variants live-confirmed to satisfy their real per-tick Zone/Biome flag, 2026-08-14/15)
- [x] Full researched Calamity (11 bosses) and Spirit (7 bosses, incl. Infernon) rosters registered end-to-end with correct Infernum-conditional gating (presence/absence-gated redirects, forced-night persistence) — Validated in Phase 10: Full Calamity/Spirit Boss Roster Registration & Biome Subworld Routing (17 of 18 originally-scoped bosses live-confirmed 2026-08-14; The Old Duke descoped by user decision post-fix, see Out of Scope)
- [x] Full pipeline + all registered bosses recognized correctly by external tracker mods (Boss Checklist), not just internally consistent — Validated in Phase 8: Full Pipeline Verification & Tracker Confirmation (all mods' Boss Checklist recognition + Moon-Lord-lockout + per-mod-disabled JIT safety live-confirmed, 2026-08-15)

### Active

None yet — v1.0 shipped 2026-08-15. Run `/gsd:new-milestone` to define v1.1 requirements.

### Out of Scope

- Multiplayer / dedicated server support — netcode complexity deferred; v1 targets singleplayer only
- Automatic subworld entry based on game-state heuristics (e.g. auto-detecting an imminent boss fight) — v1 redirects only on explicit use of an existing boss-summon item, still a deliberate player action
- Boss priority ordering / phased rollout by "worst offender" — registration cost is uniform per boss once the BossRegistry/BossCoreItem/GlobalNPC skeleton exists, so there's no value in special-casing specific bosses first
- NoxusBoss (Devourer of Universes and its other bosses) — removed from v1 scope entirely during Phase 7 discuss-phase (2026-08-14). User's rationale: most NoxusBoss bosses are quest-triggered (Solyn's moon-event questline) or already run in their own dedicated subworld/arena mechanic, so they don't fit this project's carrier-item redirect pattern the way a plain summon-item boss does. No plan to revisit.
- Calamity's Exo Mechs (real trigger is a placeable Tile+TileEntity+UI "Codebreaker" machine, no `Item.type` exists to hook) and Spirit's Starplate Voyager (real trigger is a scripted ambient-tile `Event`, not an item) — removed from v1 scope entirely during Phase 10 planning (2026-08-14). Both require a non-item trigger mechanism (Tile-based / Event-based) that SUBW-01 explicitly excludes from v1 ("limited in v1 to simple 'use item to summon' types — not altar-thrown or bulb-break style triggers"). User confirmed exclusion applies going forward, not just this phase ("앞으로도 제외") — no plan to revisit; would require its own SummonItemRegistry-equivalent redesign for non-item triggers.
- Dungeon and Sulphurous Sea biome-variant arena subworlds — removed from Phase 9's build scope mid-execution (D-07, 2026-08-14) after both had already been built once; blocks a future biome-safe arena for Polterghast (Spirit, Dungeon) until reinstated in a future milestone. Candidate for v1.1.
- The Old Duke (Calamity, Infernum-only boss) — despawn bug fully root-caused (InfernumMode's per-world toggle resetting inside the throwaway arena subworld, weaponized by NoxusBoss's hijack AI) and a general fix (`ForceInfernumModeActiveInArena()`) implemented and kept in the codebase, but the boss's own registration was removed from v1 scope by deliberate user decision (quick task 260815-024, 2026-08-15) rather than live re-verified and kept. Candidate for v1.1 re-attempt (would also need the deferred Sulphurous Sea biome variant above).

## Context

Player runs several large Terraria content mods together (Calamity, Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak, Infernum, Wrath of the Gods) plus QoL/library mods (SubworldLibrary, StructureHelper, Luminance, etc.). Heavy boss fights (Moon Lord, Infernum/Wrath-reworked bosses) combine projectile-spam with the elevated background load these content mods introduce even when idle, causing GC stalls and collision-detection bottlenecks that crash framerate to 1-2 FPS. NoxusBoss remains installed/enabled for gameplay but is out of this mod's v1 registration scope (see Out of Scope).

Disabling content mods outright risks crashes because their content is already placed in the world. The adopted fix: keep all mods enabled, but run the heaviest boss fights in a subworld (via SubworldLibrary) that has never had any content placed in it.

Known blocker: boss "downed" flags are serialized per-world-file and unconditionally overwritten on world load, so a subworld boss kill does not propagate to the main world automatically — this is a reported SubworldLibrary-ecosystem bug (workaround mods like "Calamity Boss Resyncer" exist for it). This mod works around it with a carrier item: kill drops a `BossCoreItem` tagged with a boss key; using that item in the main world looks up and replays the registered downed-flag + side-effect logic.

Related discovery (Phase 1, confirmed live 2026-08-13): SubworldLibrary v2.2.3.2 also has the *opposite* problem for a whitelist of ~30 vanilla `NPC`/`DD2Event` downed flags (including `NPC.downedSlimeKing`) — its own `CopyDowned()`/`ReadCopiedDowned()` helpers bidirectionally sync those specific flags between main world and subworld on every entry/exit, independent of the carrier-item mechanism. `Subworlds/BossArenaSubworld.cs`'s `OnEnter()`/`OnExit()` snapshot-and-restore that whitelist so SubworldLibrary's own sync becomes a no-op. This fix must be preserved (not accidentally reverted) in all future phases — see `.planning/debug/resolved/isolation-premise-flag-persistence.md` for the full investigation.

Mod-specific research completed so far (see `DESIGN_1.md` for full detail, originally at `C:\Users\chang\Downloads\DESIGN_1.md`):
- **Calamity**: `CalamityMod.DownedBossSystem`, wrapper properties whose setters call `NPC.SetEventFlagCleared`; requires `CalamityNetcode.SyncWorld()` and `CalamityGlobalNPC.SetNewBossJustDowned()` side effects.
- **Spirit** (corrected in Phase 5, see `05-RESEARCH.md`): `SpiritMod.MyWorld.DownedInfernon` is a public read-only property backed by `SpiritMod.NPCs.BossDownedTracker` — an `internal` class wrapping a static `Dictionary<string,bool>`. No public setter exists (the original "plain public static bool fields" assumption above was wrong); writes require cached reflection into the internal `Downed` dictionary, replicating exactly what `BossDownedTracker.OnKill()` itself does.
- **Infernum / Wrath of the Gods**: rework existing boss AI only, no separate flags — covered automatically once the underlying Calamity/vanilla boss is registered (Wrath's own boss status needs a recheck — enabled.json only shows the KR localization file, base mod presence unconfirmed).
- **Redemption** (Phase 6): `Redemption.Globals.RedeBossDowned.downedThorn`, direct public-static-field write (Thorn).
- **CatalystMod** (Phase 6): `CatalystMod.WorldDefeats.downedAstrageldon`, direct public-static-field write, non-standard `-Type` `gameEventId` (Astrageldon).
- **Not yet researched**: ContinentOfJourney/Daybreak (identified in Phase 7 discuss-phase as Homeward Journey, GabeHasWon, Steam Workshop id 2930931197 — "Daybreak" itself confirmed a boss-less library dependency of Wrath of the Gods, not a separate boss-bearing mod). The HomewardSubworld bridge module (`GabeHasWon/HomewardSubworld`) remains useful as a reference for subworld data-sync patterns.
- **NoxusBoss**: out of v1 scope — not researched, no plan to research (see Out of Scope).

## Constraints

- **Tech stack**: tModLoader mod in C#, .NET 8.0 SDK, developed in VS Code with C# Dev Kit, built via `dotnet msbuild`
- **Dependency**: SubworldLibrary for subworld creation/management
- **Compatibility**: must reproduce each source mod's actual `OnKill` side effects (flag + netcode sync + any WorldGen calls) rather than just setting a boolean — under-reproducing breaks vanilla systems that key off those flags (e.g. Lantern Night event triggers)
- **API variance**: each content mod exposes downed-progress differently (Calamity: wrapper properties with hooks; Spirit: raw static fields) — registration code must be written per-mod after per-mod research, no generic shortcut

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Carrier-item pattern (BossCoreItem) instead of trying to sync subworld state directly | SubworldLibrary doesn't reliably propagate downed flags across worlds; item-use on return is a known, controllable workaround | Validated (Phase 3) |
| Research all target mods (Calamity, Spirit, Redemption, CatalystMod, Homeward Journey) before writing implementation code | User explicitly chose full-research-first over incremental research+build; NoxusBoss dropped from this list (see Phase 7 removal decision below) | ✓ Good — held for all 5 mods through Phase 10 |
| No boss priority ordering in v1 | Once the BossRegistry/BossCoreItem/GlobalNPC skeleton exists, registering any individual boss costs the same — no benefit to special-casing "worst offenders" like Moon Lord first | ✓ Good — 17-boss full-roster registration (Phase 10) confirmed uniform cost held |
| Singleplayer-only for v1 | Netcode/dedicated-server sync adds significant complexity; explicitly deferred | ✓ Good — no multiplayer need surfaced during v1.0 |
| ~~Existing boss-summon items are the subworld entry trigger, not a new dedicated portal item~~ — **SUPERSEDED in Phase 2 discuss-phase (2026-08-13)** | Original rationale: simpler for the player, less new content to maintain. Superseded because the user explicitly requested a dedicated portal tile instead — see next row | Superseded |
| New placeable portal tile ("Test1", Corruption Altar sprite reused visually only — no vanilla altar behavior) is the subworld entry trigger; right-click while holding a registered summon item | User's explicit design choice (Phase 2 discuss-phase): a physical, placeable altar-style object reads more naturally as an "arena portal" than reusing an item's own use-action, and keeps the summon item's normal main-world behavior completely untouched | Validated (Phase 2) |
| BossCoreItem drop via `GlobalNPC.ModifyNPCLoot` + conditional `ItemDropRule` (gated to boss-arena subworld) instead of imperative `OnKill()` spawn | More idiomatic tModLoader loot pipeline (bestiary/expert-mode integration); custom drop rule can set the item's BossKey instance data at spawn time in one step | Validated (Phase 3) |
| Biome-gated bosses (e.g. Hive Mind, which despawns without `player.ZoneCorrupt`) get their own dedicated per-biome arena subworld, routed via `BossArenaRoutingRegistry`, instead of forcing biome Zone flags via an every-tick `ModPlayer` override | A real tile-based biome survives vanilla's per-tick Zone-flag recompute (`Player.UpdateBiomes()`); an ever-growing pile of per-boss `Zone*` overrides in `BiomeOverridePlayer` doesn't scale across future biome-gated bosses | Validated (Phase 4, see `.planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md`) |
| Delegates passed into `[JITWhenModsEnabled]`-guarded registration calls (e.g. `BossDefinition.IsDowned`) must be named, separately-`[JITWhenModsEnabled]`-tagged methods, never inline lambdas | An inline lambda compiles into a `<>c` compiler-generated cache-class method that does NOT inherit the enclosing method's `[JITWhenModsEnabled]` attribute; tModLoader's JIT prefilter still touches it and throws a real `JITException` when the referenced mod is disabled — confirmed live in Phase 4 (commit `0e19600`) | Validated (Phase 4) — applies to every future per-mod boss registration (Phase 5 Spirit onward) |
| When a content mod's downed-progress backing field has no public setter (only a public read property), write via cached `FieldInfo` reflection into the internal field, wrapped in try/catch + `Mod.Logger.Warn` on failure, rather than skipping the write or crashing | SpiritMod's `BossDownedTracker` is declared `internal`, so even its individually-`public static` members are compile-time unreachable; no `Mod.Call` "set downed" context exists either — reflection is the only path that still replicates the mod's own `OnKill()` write exactly, per this project's "call the mod's actual setter" intent | Validated (Phase 5) |
| Player-scoped vs. world-scoped side-effect classification (to avoid double-granting across the subworld boundary) can be satisfied by explicit in-code documentation instead of exclusion logic, when research confirms no player-scoped effect actually exists for that boss | Investigated for Spirit's Infernon: `BossDownedTracker.OnKill()`/`Infernon.OnKill()`/`InfernoSkull.OnKill()` are all fully world-scoped (dictionary write, singleplayer-no-op netcode, world-scoped tile mutation) — building exclusion logic for a risk that doesn't exist would be unnecessary complexity | Validated (Phase 5, D-03) |
| Phase 9 (Biome-Dependent Subworld Coverage) added after Phase 8, generalizing the ad-hoc `BossArenaCorruptionSubworld`/`BossArenaRoutingRegistry` fix from Phase 4 into a systematic per-boss audit across all v1 mods | User requested during Phase 5 execution, after seeing Phase 4's Hive Mind biome-despawn bug get fixed ad-hoc and confirming Phase 5's Infernon needed no such fix — wanted a dedicated phase to audit the remaining Phase 6/7 bosses systematically rather than discovering dependencies live in-game each time | ✓ Good — ARENA-01 shipped (7 biome variants, Phase 9/10) |
| NoxusBoss removed from v1 scope entirely (was Phase 7 MOD-05/Success Criterion 1) | User decision during Phase 7 discuss-phase (2026-08-14): most NoxusBoss bosses are quest-triggered (Solyn's moon-event questline) or already run in their own dedicated subworld/arena mechanic, so they don't fit this project's plain-summon-item carrier-item redirect pattern. No plan to revisit — moved to PROJECT.md Out of Scope, not backlogged | Locked (Phase 7 discuss-phase) |
| ContinentOfJourney identified as Homeward Journey (GabeHasWon, Steam Workshop id 2930931197); Daybreak confirmed as a boss-less library dependency of Wrath of the Gods, not a separate target | Two prior research passes (Phase 9 prep) could not identify "ContinentOfJourney" by that literal name; user supplied the Workshop link directly during Phase 7 discuss-phase, confirming the "(Homeward series)" parenthetical in ROADMAP.md Phase 7's title was the actual pointer | Locked (Phase 7 discuss-phase) |
| Exo Mechs (Calamity) and Starplate Voyager (Spirit) removed from v1 scope entirely | Phase 10 research found both use a non-item trigger (Exo Mechs: placeable Tile+TileEntity+UI "Codebreaker" machine; Starplate Voyager: scripted ambient-tile `Event`) — incompatible with SUBW-01's v1 limitation to "use item to summon" types. User confirmed exclusion applies going forward, not just Phase 10 | Locked (Phase 10 planning, 2026-08-14) |
| Dungeon and Sulphurous Sea biome variants dropped from Phase 9's build scope mid-execution (D-07) | User decision after both had already been built once in isolated worktrees; discarded rather than merged, since no boss strictly required them yet (Polterghast/Old Duke) | Locked (Phase 9, 2026-08-14) — candidate for v1.1 |
| The Old Duke removed from v1 scope entirely despite its despawn bug being root-caused and fixed | Root cause: InfernumMode's per-world toggle resets to false inside the throwaway arena subworld, letting NoxusBoss's Old-Duke-hijack AI fire. Fix (`ForceInfernumModeActiveInArena()`) was implemented, build-verified, and kept (it also benefits Providence/Profaned Guardians/Astrum Deus/Astrum Aureus's gating). The Old Duke's own registration was still descoped by deliberate user choice rather than live re-verified and kept | Locked (quick task 260815-024, 2026-08-15) — candidate for v1.1 re-attempt |
| `ForceInfernumModeActiveInArena()`'s call site gated by an explicit `BossDefinition.RequiresInfernumToggle` flag, not a `bossKey` string-prefix/mod-name heuristic | First gated to "any boss when InfernumMode is installed" (too broad), then to `bossKey.StartsWith("calamity:")` (quick task 260815-to6) — a code review flagged this as a naming-convention proxy that silently missed `catalyst:astrageldon`. Replaced with a per-boss explicit flag (quick task 260815-u7g), decompile-verified via `ilspycmd` against `Libs/InfernumMode.dll` that Astrageldon has no actual InfernumMode dependency | Validated (quick tasks 260815-to6, 260815-u7g, 2026-08-15) |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd:transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd:complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-08-15 — v1.0 MVP milestone complete*
