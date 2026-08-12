# BossArenaSubWorld

## What This Is

A Terraria tModLoader mod that lets a player fight lag-heavy bosses (Moon Lord, Infernum/Wrath-reworked bosses, and other bosses from large content mods) inside a dedicated subworld that has never had any mod content placed in it, then carries the boss-kill progress back to the main world. Solves severe FPS crashes (40-50 → 1-2) caused by running multiple large content mods (Calamity, Spirit, Redemption, etc.) simultaneously during heavy boss fights.

## Core Value

The generic boss-kill → carrier-item → main-world-apply mechanism (BossRegistry + BossCoreItem + GlobalNPC) must reliably reproduce a boss's full "downed" state in the main world — flags, netcode sync, and any WorldGen side effects — for any registered boss. If this pipeline doesn't work end-to-end, nothing else matters.

## Requirements

### Validated

(None yet — ship to validate)

### Active

- [ ] Using an existing, registered boss-summon item (vanilla or modded) cancels its main-world summon effect and redirects the player into a dedicated boss-arena subworld (no mod content ever placed) instead — no separate portal item needed, and the boss auto-summons once inside
- [ ] Killing a registered boss in the subworld drops a BossCoreItem carrying that boss's key (GlobalNPC.OnKill detection)
- [ ] Using BossCoreItem in the main world applies the boss's downed flag via BossRegistry.Apply(key)
- [ ] Boss-specific side effects (netcode sync calls, "boss just downed" messages) are reproduced when the item is used, matching each source mod's original OnKill behavior
- [ ] World-altering bosses (mechanical bosses, Plantera, etc.) also trigger their WorldGen side effects (ore generation, dungeon activation, etc.) when the item is used in the main world
- [ ] Calamity bosses registered via `DownedBossSystem` pattern
- [ ] Spirit bosses registered via `MyWorld` static-field pattern
- [ ] Redemption bosses researched and registered
- [ ] CatalystMod bosses researched and registered
- [ ] NoxusBoss (Devourer of Universes) researched and registered
- [ ] ContinentOfJourney / Daybreak (Homeward) bosses researched and registered
- [ ] Full pipeline verified end-to-end in singleplayer (subworld kill → item → main world apply), with world backup before testing

### Out of Scope

- Multiplayer / dedicated server support — netcode complexity deferred; v1 targets singleplayer only
- Automatic subworld entry based on game-state heuristics (e.g. auto-detecting an imminent boss fight) — v1 redirects only on explicit use of an existing boss-summon item, still a deliberate player action
- Boss priority ordering / phased rollout by "worst offender" — registration cost is uniform per boss once the BossRegistry/BossCoreItem/GlobalNPC skeleton exists, so there's no value in special-casing specific bosses first

## Context

Player runs several large Terraria content mods together (Calamity, Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak, Infernum, Wrath of the Gods) plus QoL/library mods (SubworldLibrary, StructureHelper, Luminance, etc.). Heavy boss fights (Moon Lord, Infernum/Wrath-reworked bosses) combine projectile-spam with the elevated background load these content mods introduce even when idle, causing GC stalls and collision-detection bottlenecks that crash framerate to 1-2 FPS.

Disabling content mods outright risks crashes because their content is already placed in the world. The adopted fix: keep all mods enabled, but run the heaviest boss fights in a subworld (via SubworldLibrary) that has never had any content placed in it.

Known blocker: boss "downed" flags are serialized per-world-file and unconditionally overwritten on world load, so a subworld boss kill does not propagate to the main world automatically — this is a reported SubworldLibrary-ecosystem bug (workaround mods like "Calamity Boss Resyncer" exist for it). This mod works around it with a carrier item: kill drops a `BossCoreItem` tagged with a boss key; using that item in the main world looks up and replays the registered downed-flag + side-effect logic.

Mod-specific research completed so far (see `DESIGN_1.md` for full detail, originally at `C:\Users\chang\Downloads\DESIGN_1.md`):
- **Calamity**: `CalamityMod.DownedBossSystem`, wrapper properties whose setters call `NPC.SetEventFlagCleared`; requires `CalamityNetcode.SyncWorld()` and `CalamityGlobalNPC.SetNewBossJustDowned()` side effects.
- **Spirit**: `SpiritMod.MyWorld`, plain public static bool fields, no wrapper needed (version may have moved from `ModWorld` to `ModSystem` — recheck against installed copy).
- **Infernum / Wrath of the Gods**: rework existing boss AI only, no separate flags — covered automatically once the underlying Calamity/vanilla boss is registered (Wrath's own boss status needs a recheck — enabled.json only shows the KR localization file, base mod presence unconfirmed).
- **Not yet researched**: Redemption (`Hallam9K/RedemptionAlpha` on GitHub), CatalystMod, NoxusBoss, ContinentOfJourney, Daybreak, and the HomewardSubworld bridge module (useful as reference for subworld data-sync patterns).

## Constraints

- **Tech stack**: tModLoader mod in C#, .NET 8.0 SDK, developed in VS Code with C# Dev Kit, built via `dotnet msbuild`
- **Dependency**: SubworldLibrary for subworld creation/management
- **Compatibility**: must reproduce each source mod's actual `OnKill` side effects (flag + netcode sync + any WorldGen calls) rather than just setting a boolean — under-reproducing breaks vanilla systems that key off those flags (e.g. Lantern Night event triggers)
- **API variance**: each content mod exposes downed-progress differently (Calamity: wrapper properties with hooks; Spirit: raw static fields) — registration code must be written per-mod after per-mod research, no generic shortcut

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Carrier-item pattern (BossCoreItem) instead of trying to sync subworld state directly | SubworldLibrary doesn't reliably propagate downed flags across worlds; item-use on return is a known, controllable workaround | — Pending |
| Research all target mods (Calamity, Spirit, Redemption, CatalystMod, NoxusBoss, Homeward) before writing implementation code | User explicitly chose full-research-first over incremental research+build | — Pending |
| No boss priority ordering in v1 | Once the BossRegistry/BossCoreItem/GlobalNPC skeleton exists, registering any individual boss costs the same — no benefit to special-casing "worst offenders" like Moon Lord first | — Pending |
| Singleplayer-only for v1 | Netcode/dedicated-server sync adds significant complexity; explicitly deferred | — Pending |
| Existing boss-summon items are the subworld entry trigger, not a new dedicated portal item | Simpler for the player (reuse the item they already have), less new content to build/maintain than a custom portal item/NPC; still an explicit, deliberate player action | — Pending |
| BossCoreItem drop via `GlobalNPC.ModifyNPCLoot` + conditional `ItemDropRule` (gated to boss-arena subworld) instead of imperative `OnKill()` spawn | More idiomatic tModLoader loot pipeline (bestiary/expert-mode integration); custom drop rule can set the item's BossKey instance data at spawn time in one step | — Pending |

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
*Last updated: 2026-08-12 after initialization*
