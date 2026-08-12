# Project Research Summary

**Project:** BossArenaSubWorld
**Domain:** Terraria tModLoader mod — dedicated boss-arena subworld with cross-mod, cross-world boss-progress carry-back
**Researched:** 2026-08-12
**Confidence:** MEDIUM-HIGH

## Executive Summary

BossArenaSubWorld is a tModLoader mod that solves a real, reproducible problem (FPS collapse from 40-50 to 1-2 during heavy boss fights with multiple large content mods installed) using an established pattern in the tModLoader ecosystem: a dedicated, content-free SubworldLibrary subworld for the fight, paired with a carrier-item mechanism to reproduce the boss's "downed" state back in the main world. Research confirms the core premise still holds even after recent SubworldLibrary updates — SubworldLibrary now natively syncs *vanilla* downed flags across the subworld boundary, but has no knowledge of modded content mods' private downed-state storage (Calamity's `DownedBossSystem`, Spirit's `MyWorld` statics, etc.), which is exactly the gap this project's `BossRegistry` + `BossCoreItem` pipeline fills. No existing mod covers this breadth (the one close analogue, "Calamity Boss Resyncer," is single-mod, automatic-resync, and has unresolved bugs for paired/Infernum-mode bosses) — the multi-mod registry architecture and explicit, player-triggered, idempotent carrier-item design are genuine differentiators, not novelty for its own sake.

The recommended approach is: tModLoader 1.4.4.9 targeting .NET 8 SDK (hard constraint — 9/10 will not work), SubworldLibrary as a strong `modReferences` dependency, and every content mod (Calamity, Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak) as `weakReferences` so the mod still loads when a player has only some of them installed. Architecturally, a single `BossRegistry` (key to `BossDefinition{NpcTypes, Apply()}`) is the seam every other component depends on — `GlobalNPC.OnKill` and `BossCoreItem`'s use-hook never branch on source mod directly; all mod-specific knowledge is isolated into one `Integrations/` file per mod. This keeps the marginal cost of registering an additional mod's bosses bounded and uniform, which validates the project's own "no priority ordering" decision.

The dominant risk category is **silent, partial correctness failures**, not crashes: setting a raw downed boolean without replaying the source mod's full side-effect chain (netcode sync, achievement/bestiary hooks, WorldGen ore/dungeon triggers) produces a boss that looks "downed" in a checklist mod while dependent systems (Lantern Night, hardmode ore generation) remain broken — and this failure mode is easy to miss because it looks done. A second, JIT-specific risk (unresolvable type in a method body crashing the whole mod load even behind a correct null-check) is well-documented by tModLoader's own wiki and must be designed around from the very first cross-mod integration, not retrofitted. One notable point of disagreement between research files: ARCHITECTURE.md and STACK.md recommend compile-time `weakReferences` + `[JITWhenModsEnabled]` as the primary cross-mod access pattern, while PITFALLS.md recommends defaulting to pure runtime reflection (no compiled type reference at all) specifically for the boss-flag interop, to sidestep the JIT hazard by construction — this should be resolved explicitly during roadmap/phase planning (see Gaps below), since it affects both `build.txt` structure and the Integrations/ file design.

## Key Findings

### Recommended Stack

Core toolchain is fixed by the platform: tModLoader 1.4.4.9 requires .NET 8.0 SDK exactly (9/10 explicitly unsupported), C# via `dotnet msbuild`/`dotnet build`, developed in VS Code + C# Dev Kit — all matching `PROJECT.md`'s existing constraints. SubworldLibrary (jjohnsnaill fork, the actively-maintained Steam Workshop distribution, not the older Mirsario original) is the required subworld dependency, referenced as `modReferences`. No third-party NuGet packages are needed; cross-mod interop in this ecosystem is conventionally done via weak references/reflection against other mods' compiled types, not shared libraries. Built-in `TagCompound` (`SaveWorldData`/`LoadWorldData` for `ModSystem`, `SaveData`/`LoadData` for `ModItem`) is the serialization mechanism for `BossRegistry` and `BossCoreItem` state.

**Core technologies:**
- tModLoader 1.4.4.9 + .NET 8.0 SDK — hard platform requirement, no substitute
- SubworldLibrary (jjohnsnaill fork) — required for subworld creation/entry/exit; use `modReferences`, not weak
- `weakReferences` + `[JITWhenModsEnabled]` (built-in tModLoader features) — the documented, officially-recommended pattern for optional content-mod interop, though PITFALLS.md flags pure reflection as a safer default for this project's specific use case (see Gaps)

### Expected Features

Table-stakes UX (manual portal-item entry, obvious return path, inventory/loot carrying over automatically via SubworldLibrary's live player object) is already provided by SubworldLibrary itself or trivially achieved — the mod's job is not to build these, only to not break them. Vanilla boss downed-flag sync between subworld and main world is now handled natively by SubworldLibrary as of a Jan 2025 fix; this project's entire value is in the gap that remains for **modded** bosses. The core differentiator is a generic, multi-mod `BossRegistry` covering 6+ content mods where the only comparable existing mod (Calamity Boss Resyncer) covers exactly one and has known unresolved bugs.

**Must have (table stakes):**
- Manual subworld entry/exit via item — SubworldLibrary provides the mechanism, mod just wires it up
- Arena isolation guarantee (no mod content ever placed in the subworld) — the entire premise of the FPS fix
- Faithful side-effect reproduction on boss-kill apply, not just a boolean flag — precedent (Calamity Boss Resyncer) shows flag-only sync produces permanently-broken tracker state

**Should have (competitive/differentiating):**
- Generic multi-mod `BossRegistry` (key to apply-function) architecture covering Calamity, Spirit, Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak — no existing mod has this breadth
- Explicit, player-controlled, idempotent carrier-item application (vs. silent automatic resync) — avoids re-firing WorldGen/netcode side effects on every subworld transition, a real bug category in the automatic-resync competitor

**Defer (v2+):**
- Multiplayer/dedicated-server support — subserver sync adds real risk (known SubworldLibrary GitHub issues) on top of an already-workaround-dependent pipeline; ship singleplayer first
- Automatic subworld entry detection — reliable imminent-boss-fight detection across many mods' summon patterns is itself a hard, per-mod problem
- Full arena decoration/building toolkit — out of scope, duplicates existing mods (Luiafk)

### Architecture Approach

A layered pipeline with one non-negotiable seam: `BossRegistry` is the only place that knows how to map an NPC kill to a boss key and how to apply that key's downed state. `BossKillGlobalNPC.OnKill` (subworld-gated) and `BossCoreItem`'s use-hook both talk only to the registry, never to a source mod directly — all mod-specific knowledge (Calamity's wrapper properties + netcode calls, Spirit's raw static fields, etc.) lives in isolated `Integrations/*.cs` files, one per source mod, each self-registering at load time and each safely no-op-ing when its target mod isn't installed.

**Major components:**
1. `BossArenaSubworld` (Subworld subclass) — empty/minimal-generation dimension; `ShouldSave = false`, `NoPlayerSaving = false` (critical: must stay false or the carrier item itself gets wiped on exit)
2. `BossRegistry` (ModSystem, static table) — key to `{NpcTypes, Apply(), side-effect delegate}`; the sole cross-cutting seam
3. `BossKillGlobalNPC` — `OnKill` hook, subworld-gated, converts a registered kill into a `BossCoreItem` drop
4. `BossCoreItem` (ModItem, `CloneNewInstances = true`) — carries boss key via `SaveData`/`LoadData`; use-hook calls `BossRegistry.Apply(key)`
5. `Integrations/*.cs` (one file per source mod) — translates each mod's actual downed-flag API + side effects into a registry entry, isolated behind mod-presence guards

### Critical Pitfalls

1. **World-flag isolation between subworld and main world (the project's founding premise)** — a boss killed in the subworld does not propagate to the main world automatically, on any SubworldLibrary version; treat the carrier-item pattern as the only supported path and verify this empirically (kill without using the item, confirm main-world flag stays false) before building anything on top.
2. **JIT crashes from weak-reference code, even behind a correct null-check** — an unresolvable type anywhere in a method body can crash the whole mod load when a soft-dependency mod is absent, because the JIT resolves the full method, not just the reachable branch. Every cross-mod-type-touching method must be isolated and either marked `[JITWhenModsEnabled]` or replaced with pure runtime reflection; smoke-test with each soft dependency disabled, per mod, from the first one added onward.
3. **Setting the raw boolean flag instead of replaying the full side-effect chain** — misses achievement/bestiary hooks, netcode sync calls, and WorldGen effects (ore gen, dungeon activation) that dependent systems (Lantern Night, hardmode progression) key off of; treat "flag set" and "side effects replayed" as two separately-verified checklist items per boss.
4. **Reflection into another mod's internals breaks silently after that mod updates** — cache all reflective lookups once at `PostSetupContent`, wrap every reflective access in try/catch with explicit warning-level logging, and disable only the affected boss's registration rather than crashing the whole mod.
5. **Player-scoped vs. world-scoped double-grants** — `Subworld.NoPlayerSaving` stays `false` by design (required for the carrier item to survive), which means player-scoped rewards (recipes, journal entries) already survive the subworld trip automatically; replaying them again in `Apply()` double-grants. Classify every side effect as world-scoped (needs replay) vs. player-scoped (must NOT be replayed) per mod during research.

## Implications for Roadmap

Based on research, suggested phase structure:

### Phase 1: Subworld Skeleton & Isolation Proof
**Rationale:** Everything downstream depends on the subworld existing, being genuinely empty of mod content, and correctly preserving inventory across the boundary. This phase also proves the founding pitfall (world-flag isolation) is real and reproducible before any registry abstraction is built on top of an unverified assumption.
**Delivers:** `BossArenaSubworld` (Subworld subclass), manual entry/exit item, empirical confirmation that (a) inventory/carrier items survive the round trip (`NoPlayerSaving = false`) and (b) a vanilla boss's downed flag does NOT propagate back without explicit action.
**Addresses:** Table-stakes features — manual entry/exit, arena isolation guarantee, safe death handling.
**Avoids:** Pitfall 1 (world-flag isolation) — proves it rather than assumes it; Pitfall 5 groundwork — confirms `NoPlayerSaving` default is correct.

### Phase 2: BossRegistry + BossCoreItem + GlobalNPC Skeleton (Proof of Concept with One Boss)
**Rationale:** The registry/item/GlobalNPC seam is the architectural backbone every per-mod integration depends on; proving it end-to-end with one low-risk boss (start with a vanilla boss, per PITFALLS.md's Pitfall 1 verification recommendation) before adding content-mod complexity isolates architecture bugs from per-mod API bugs.
**Delivers:** Working `BossRegistry` (key to `BossDefinition`), `BossKillGlobalNPC.OnKill` (subworld-gated), `BossCoreItem` with `SaveData`/`LoadData`/`Clone`, full kill-to-item-to-apply pipeline verified end-to-end in singleplayer with a world backup.
**Uses:** `TagCompound` save/load pattern from STACK.md; `ModSystem`/`ModItem`/`GlobalNPC` base classes.
**Implements:** Architecture Pattern 2 (central registry as the only cross-cutting seam) and Pattern 1 (isolated no-save subworld).

### Phase 3: Reflection/Weak-Reference Helper Layer (Shared Infrastructure)
**Rationale:** Pitfalls 2, 3, 6, 7 all point to the same root cause class (unsafe cross-mod type/member access) and the same fix shape (a shared, cached, exception-safe reflection helper). Building this once, before the first real content-mod integration, turns "safe cross-mod access" into a pattern every subsequent integration inherits rather than a retrofit applied piecemeal per mod.
**Delivers:** A shared helper for resolving target-mod types/members safely (via `ModLoader.TryGetMod` then `targetMod.Code.GetType(fullName)`, never bare `Type.GetType` or `.GetTypes()`), with cached lookups, try/catch-and-log-per-boss failure isolation, and a documented decision on weak-reference+`[JITWhenModsEnabled]` vs. pure reflection (see Research Flags below — this decision should be made explicitly here, not mid-implementation).
**Addresses:** Pitfalls 2, 3, 6, 7 directly — this phase exists specifically because of this pitfall cluster.
**Avoids:** JIT crash on mod-disabled load; silent reflection breakage; `Assembly.GetTypes()` failures; `Type.GetType` false negatives.

### Phase 4: Calamity Integration (First Real Content Mod)
**Rationale:** Calamity is the mod with the most-researched API shape already (`DownedBossSystem` wrapper properties + `CalamityNetcode.SyncWorld()` + `SetNewBossJustDowned()`, per `PROJECT.md`), making it the lowest-risk first real integration and the one most likely to surface any remaining architecture gaps before scaling to less-understood mods.
**Delivers:** `CalamityIntegration.cs` registering at least one Calamity boss with full side-effect replay (flag + netcode sync + "just downed" call), smoke-tested with CalamityMod both enabled and disabled.
**Addresses:** Active requirement "Calamity bosses registered via `DownedBossSystem` pattern."
**Avoids:** Pitfall 4 (under-reproduced side effects) — this is the first phase where the full side-effect-chain discipline must be demonstrated correctly, not just the flag.

### Phase 5: Spirit Integration (Second Content Mod, Structurally Different API)
**Rationale:** Spirit's raw-static-field API (vs. Calamity's wrapper properties) is structurally different enough to prove the `Integrations/` pattern generalizes across API shapes, not just Calamity's specific one — this is the validation point for the "generic registry" differentiator claim.
**Delivers:** `SpiritIntegration.cs`, with the `ModWorld`-vs-`ModSystem` location re-verified against the installed copy (flagged as uncertain in `PROJECT.md`).
**Addresses:** Active requirement "Spirit bosses registered via `MyWorld` static-field pattern."
**Avoids:** Pitfall 5 (player-scoped vs. world-scoped double-grant) — Spirit's simpler flag model is a good place to explicitly practice this classification before the more complex remaining mods.

### Phase 6+: Remaining Mod Integrations (Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak)
**Rationale:** Per Feature/Architecture research, marginal cost per mod is uniform once the skeleton (Phases 2-3) and pattern-proof (Phases 4-5) exist — these are bounded, parallelizable research-then-integrate units, not architecturally novel work. No priority ordering needed among them (confirms `PROJECT.md`'s own decision).
**Delivers:** One integration file + registered bosses per remaining mod, each independently research-spiked, built, and smoke-tested (enabled/disabled) before merging.
**Addresses:** Remaining Active requirements for Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak.

### Phase Ordering Rationale

- Subworld isolation (Phase 1) must be proven before any registry work, because it's the entire reason the carrier-item architecture exists — building on an unverified assumption here would invalidate everything downstream.
- The registry/item/GlobalNPC skeleton (Phase 2) is proven with a vanilla boss (zero cross-mod risk) before any content-mod integration, isolating "does the pipeline mechanism work" from "does this specific mod's API integration work."
- The shared reflection/weak-reference helper (Phase 3) is built as its own phase, before the first real integration, specifically because 4 of 7 identified pitfalls (2, 3, 6, 7) share a root cause and a fix shape — building it once prevents four separate retrofits.
- Calamity before Spirit (Phases 4-5) because Calamity's API is already researched in more depth (`PROJECT.md`), reducing risk in the phase that also validates the full side-effect-replication discipline for the first time.
- Remaining mods (Phase 6+) are explicitly unordered/parallelizable per research — no "worst offender first" logic, confirmed sound by Feature research's competitor analysis (marginal cost per boss is uniform once the skeleton exists).

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 3 (reflection/weak-reference helper layer):** STACK.md/ARCHITECTURE.md recommend weak-references + `[JITWhenModsEnabled]` as primary; PITFALLS.md recommends defaulting to pure runtime reflection to sidestep the JIT hazard by construction. This is an unresolved tension between research files (see Gaps) and should get a `/gsd:research-phase` pass to settle the pattern before it's baked into every subsequent integration.
- **Phase 6+ (Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak):** API shape is explicitly unresearched per `PROJECT.md` — each needs its own per-mod research spike (decompile/source read) before an integration file can be written, same as the Calamity/Spirit research already completed.

Phases with standard patterns (skip research-phase):
- **Phase 1 (subworld skeleton):** SubworldLibrary's `Subworld`/`SubworldSystem` API is well-documented (official wiki + source-verified) and has direct precedent in reviewed ecosystem mods (Abyssal Subworld, Arena Dimensions).
- **Phase 2 (registry/item/GlobalNPC skeleton):** Directly mirrors tModLoader's own `ExampleMod` patterns (`DownedBossSystem`/`ExampleGlobalNPC`) and official `TagCompound` save/load docs — HIGH confidence, no further research needed.
- **Phases 4-5 (Calamity, Spirit):** API shapes already researched in `PROJECT.md`/`DESIGN_1.md` prior to this research pass; implementation-level verification against installed DLLs is still needed but is not open-ended research.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Core toolchain (tModLoader/.NET 8/build flow) verified against official wiki; SubworldLibrary specifics MEDIUM (GitHub + Steam Workshop, not Context7-indexed, one stale third-party mirror explicitly flagged and discounted) |
| Features | MEDIUM-HIGH | Core mechanics (SubworldLibrary behavior) verified directly against source; ecosystem examples (Abyssal Subworld, Arena Dimensions, Multiverse Reloaded) verified via multiple independent community sources at MEDIUM confidence; exact per-content-mod OnKill side effects deferred to per-mod research (out of scope for this research pass by design) |
| Architecture | MEDIUM | SubworldLibrary and core tModLoader interop patterns are HIGH confidence (official wiki/docs); per-target-mod integration specifics (Calamity/Spirit/Redemption/etc.) remain LOW/unresearched by design, deferred to per-mod research spikes |
| Pitfalls | MEDIUM-HIGH | SubworldLibrary source/wiki and tModLoader's official "Expert Cross Mod Content" wiki are authoritative and current; per-mod internal field names carry lower confidence and must be re-verified against installed DLLs at implementation time |

**Overall confidence:** MEDIUM-HIGH

### Gaps to Address

- **Weak-references vs. pure reflection for cross-mod access — unresolved disagreement between research files:** ARCHITECTURE.md and STACK.md recommend `weakReferences` + `[JITWhenModsEnabled]` (the officially-documented tModLoader pattern) as primary, with raw reflection as fallback only for members with no reference DLL available. PITFALLS.md, focused specifically on the JIT-crash failure mode, recommends defaulting to pure runtime reflection for the boss-flag interop specifically, to eliminate the JIT hazard by construction rather than mitigate it via careful isolation. This should be resolved as an explicit architectural decision during Phase 3 planning (or earlier, via `/gsd:research-phase`) — it affects `build.txt` structure, project references, and the shape of every `Integrations/*.cs` file going forward, so deciding late is costly.
- **Redemption, CatalystMod, NoxusBoss, ContinentOfJourney/Daybreak API shapes are entirely unresearched:** Confirmed as out of scope for this research pass (per `PROJECT.md`); each needs a dedicated per-mod research spike (decompile/source read against the installed DLL) before its integration phase can be planned in detail — flag each of these phases for `/gsd:research-phase` rather than assuming a shape based on Calamity/Spirit precedent.
- **Multi-boss/combo-encounter handling (paired/simultaneous fights) is a known real-world bug source (Infernum-style Bereft Vassal + Great Sand Shark) but unscoped:** Feature research flags this as likely to surface during Redemption/CatalystMod/NoxusBoss research; no architecture decision has been made yet for how `BossRegistry` should represent a boss that is actually two NPCs with a shared downed condition — worth a design note before Phase 6+ hits the first such case.
- **`HomewardSubworld` / Abyssal Subworld's actual cross-world data-carry implementation was not directly confirmed:** STACK.md flags this as worth a manual source read (`AbyssalSubworld.cs`/`HomewardSubworld.cs`) during implementation, since WebFetch could not retrieve enough detail to confirm whether it uses a pattern comparable to (or better than) the carrier-item design already chosen.

## Sources

### Primary (HIGH confidence)
- https://github.com/tModLoader/tModLoader/wiki/Developing-with-Visual-Studio-Code — .NET 8 SDK requirement, build flow, C# Dev Kit setup
- https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content — weak references, `[JITWhenModsEnabled]`, `Mod.Call`, reflection guidance
- https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound — `SaveData`/`LoadData` patterns
- https://github.com/tModLoader/tModLoader/blob/1.4/ExampleMod/Common/Systems/DownedBossSystem.cs — canonical downed-flag pattern
- https://docs.tmodloader.net/docs/stable/class_global_n_p_c.html — `GlobalNPC.OnKill` API reference
- https://docs.tmodloader.net/docs/1.4-stable/class_terraria_1_1_mod_loader_1_1_mod.html — `Mod.Code`, `AssemblyManager.GetLoadableTypes` warning
- https://github.com/jjohnsnaill/SubworldLibrary (source, master branch) — `Subworld.cs`, `SubworldSystem.cs` read directly
- https://github.com/JavidPack/BossChecklist/blob/1.4/BossChecklistIntegrationExample.cs — real-world weak-reference cross-mod example
- https://github.com/CalamityTeam/CalamityModPublic/blob/master/CalamityNetcode.cs — confirms `SyncWorld()` is a safe no-op in singleplayer

### Secondary (MEDIUM confidence)
- https://steamcommunity.com/workshop/filedetails/?id=2785100219 (SubworldLibrary Steam Workshop page + changelog) — current version/compatibility, world-data sync fixes
- https://steamcommunity.com/sharedfiles/filedetails/?id=3417899539 ("Calamity Boss Resyncer") — evidence the Calamity flag-loss bug persisted into 2025, unresolved paired-boss bugs
- https://github.com/jjohnsnaill/SubworldLibrary/wiki and /issues — API shape confirmation, known multiplayer/load-timing bugs (#12, #49)
- Abyssal Subworld, Arena Dimensions, Multiverse Reloaded (Steam Workshop listings) — ecosystem UX precedent for portal-item entry/exit patterns

### Tertiary (LOW confidence)
- https://mirror.sgkoi.dev/Mods/Details/SubworldLibrary — explicitly flagged as stale/unreliable, do not use for version decisions
- https://github.com/GabeHasWon/HomewardSubworld — repo exists but source detail not confirmed via WebFetch; needs manual read
- AnswerOverflow thread title re: Infernum + subworld progress reset — title only, corroborates known blocker but not independently verified in depth

---
*Research completed: 2026-08-12*
*Ready for roadmap: yes*
