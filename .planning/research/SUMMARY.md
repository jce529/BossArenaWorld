# Project Research Summary

**Project:** BossArenaSubWorld — v1.1 아레나 서브월드 디자인 개선
**Domain:** tModLoader procedural world-generation retrofit (per-biome `Subworld`/`GenPass` visual & spatial polish)
**Researched:** 2026-08-15
**Confidence:** MEDIUM-HIGH

## Executive Summary

This milestone retrofits the 9 existing boss-arena subworlds (1 plain + 8 biome variants: Corruption, Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) with biome-themed decoration, multi-tier platforms, fall/Y-drift containment, torch lighting, and entry/exit convenience — without touching the core carrier-item pipeline, which is already shipped and out of scope here. No new technology is required: everything is achievable with tModLoader's existing `WorldGen`/`ModTile`/`TileID`/`TorchID` surface, the same API family already in use for the 8 existing single-tier `*PlatformPass` classes. The recommended approach is additive and shared-first: build a mod-agnostic `ArenaBuilder` static helper plus one shared `ArenaPolishPass : GenPass` (boundary walls, Y-limit tiles, torch spacing, multi-tier decking, return-portal placement) appended to every arena's `Tasks` list, while keeping biome-specific decoration inside each biome's own already-JIT-tagged `ApplyPass()` method. This composition-over-inheritance structure directly avoids the two architectural traps this codebase has already been burned by once: JIT-unsafe shared code touching modded types outside a guarded method, and per-biome duplication drifting out of sync with each arena's actual `surfaceY`.

The dominant risk in this milestone is not the visual work itself but its interaction with three already-hard-won, load-bearing invariants: (1) each biome's Zone-flag check reads a tile-weighted scan window centered on the *player*, not the boss, so multi-tier platforms that move the player vertically can silently flip a Zone flag false mid-fight — structurally the same class of bug as the already-fixed Hive Mind despawn, just triggered on the Y-axis instead of horizontal drift; (2) falling-tile types (Sand, Silt, Gravel, and likely Calamity's AstralSand) can reintroduce the Desert stack-overflow bug in any new decorative tier or dressing that isn't backed by solid, non-falling tiles; and (3) any new entry/exit convenience object must route through `SubworldSystem.Exit()` rather than hand-rolling a teleport, or it will silently bypass the vanilla-downed-flag snapshot/restore guard that is this project's single most load-bearing fix. A secondary but real risk is a folk-belief bug: torches do not suppress monster spawns in Terraria (safe background walls do) — this must be explicitly scoped as visual-only, not conflated with a currently-nonexistent spawn-suppression feature.

Recommended sequencing: build and validate the shared helper/pass against the plain arena first (zero biome-flag concerns, zero JIT concerns), then mechanically extend to the 6 vanilla-only biome variants, then extend to Astral/Briar last (re-running the "disable CalamityMod"/"disable SpiritMod" smoke test each time), with per-biome decoration and entry/exit systems work proceeding in parallel once the shared layer is proven.

## Key Findings

### Recommended Stack

No new core technologies or `build.txt` dependencies are needed. The milestone's five features map onto tModLoader's existing `Terraria.WorldGen` static class (`PlaceTile`/`PlaceObject`/`PlaceWall`), `TileID`/`TorchID`/`WallID` constants, and a custom `ModTile` for the invisible-boundary and return-portal tiles. The one meaningful shift from current convention: raw `Main.tile[x,y]` array writes (used today for bulk platform fills) are correct only for non-frame-important bulk fills — anything frame-important (torches, one-way platforms, the return-portal tile) must go through `WorldGen.PlaceTile`/`PlaceObject` so `FrameX`/`FrameY` render correctly, since raw writes leave framing at 0.

**Core technologies:**
- `Terraria.WorldGen.PlaceTile`/`PlaceObject`/`PlaceWall` — frame-aware placement for sparse, frame-important content (torches, platforms, portal tile), distinct from the existing raw-array bulk-fill convention
- `TorchID` constants + `TileID.Torches` — one vanilla tile ID with per-biome style selection, maps cleanly onto 6 of the 8 biome arenas with zero new dependency
- Custom `ModTile` with `PreDraw() => false` — standard idiom for an invisible solid boundary tile (vanilla has no built-in invisible-solid tile)
- StructureHelper (considered, not recommended) — would add a new external Workshop dependency for a task plain `for`-loops already solve simply; reserve only if platform geometry complexity grows significantly later

### Expected Features

**Must have (table stakes) — matches PROJECT.md's stated v1.1 scope:**
- Fall/void + Y-range boundary blocks for all 9 arenas, sized per-arena (not one hardcoded margin — Space/Underworld/Briar all sit at very different absolute Y)
- Multi-tier platform layout (2-4 tiers, vanilla jump-height spacing) sized to accommodate each arena's registered bosses' AI movement envelopes
- Regularly-spaced torches for visibility — the one fully independent, lowest-risk item; safe to build first or in isolation
- Biome-legible decorative theming that stays additive to (or drawn from) each biome's existing Zone-flag tile-weight budget — the milestone's headline ask and its highest-regression-risk item
- Entry/exit convenience (return-point marker, brief prep beat before boss auto-summon) without disturbing SubworldLibrary's existing return flow or the vanilla-downed-flag snapshot/restore guard

**Should have (differentiators, low incremental cost once table-stakes work exists):**
- Safe-wall-based spawn suppression layered onto the fall-prevention walls (LOW confidence whether GenPass-placed walls inherit "safe" status the same way player-placed ones do — needs live verification before relying on it)
- "Duck under" solid corner blocks at the top platform tier, mirroring the vanilla Moon Lord-arena convention for dodging beam/laser attacks
- Space and Underworld arenas can receive the most decorative freedom "for free" since their Zone flags are pure-Y checks with no tile-weight constraint

**Defer (explicitly out of scope per PROJECT.md):**
- Buff stations (Campfire/Heart Lantern/Honey pool)
- New biome-variant arenas (Dungeon, Sulphurous Sea)
- In-game UI notifications
- A second, parallel exit mechanism competing with SubworldLibrary's built-in Return button

### Architecture Approach

The current architecture is 9 `Subworld` subclasses paired 1:1 with 9 single-entry `GenPass` classes; only 2 of the 9 (Astral, Briar) carry `[JITWhenModsEnabled]` guards for modded tile references. The recommended v1.1 structure adds a plain-static `ArenaBuilder` helper (mod-agnostic, primitive-typed API only) plus one shared `ArenaPolishPass : GenPass` appended as a second `Tasks` entry on every arena, handling everything biome-agnostic (boundary walls, Y-limit containment, torch spacing, multi-tier decking, return-portal placement). Per-biome decoration stays non-centralized, added directly inside each biome's own `ApplyPass()` method so modded-type resolution (Astral/Briar) never leaves its already-guarded call site.

**Major components:**
1. `ArenaBuilder.cs` (new, plain static class) — shared placement primitives (`FillRectangle`, `PlaceBoundaryWalls`, `PlaceAtInterval`, `BuildTierPlatform`, `PlaceReturnPortal`), callable from both JIT-tagged and untagged call sites because its signature never mentions a modded type
2. `ArenaPolishPass.cs` (new, shared `GenPass`) — one class appended to all 9 `Tasks` lists, constructed per-arena with that arena's own `surfaceY`/thickness; runs after each biome's fill pass
3. `*PlatformPass.cs` (9x, modified) — gains biome-specific decoration calls only; Astral/Briar's new decoration code stays inside their existing tagged `ApplyPass()`
4. `Tiles/ReturnPortalTile.cs` (new) — mirrors `Test1Tile`'s existing pattern; right-click calls `SubworldSystem.Exit()`, no JIT tag needed (this mod's own type)
5. `Systems/BossSummonPlayer.cs` / `BiomeOverridePlayer.cs` (modified) — prep-time countdown before auto-summon, and a runtime Y-clamp gated on `BossArenaRoutingRegistry.IsAnyArenaActive()` (also fixes a pre-existing gate bug where the runtime check currently only covers the plain arena)

### Critical Pitfalls

1. **Falling-tile stack overflow (Desert bug) resurfaces in a new location** — any new decorative tier/dressing built from a falling-tile family (Sand, Silt, Gravel, likely AstralSand) without full solid backing can retrigger the `SquareTileFrame`/`TileFrame`/`SpawnFallingBlockProjectile` recursion crash. Bake a falling-tile safety convention directly into the shared helper (allowlist or automatic solid backing row) so every per-biome retrofit inherits the protection.
2. **Torches ≠ spawn suppression** — light level is not a spawn-rate factor in Terraria; the actual mechanism is safe background walls. Scope torches as visual-only in code and comments; if spawn suppression is wanted, implement via `EditSpawnRate` or safe walls, not torch density.
3. **Hardcoded Y-boundary is wrong for most arenas** — `surfaceY` ranges from 50 (Space) to 650 (Underworld); a containment helper must take `surfaceY`/thickness as a parameter, never a shared literal, or it will place walls uselessly far from (or actively inside) the real platform for Space/Underworld/Briar.
4. **Multi-tier platforms can flip a Zone flag false mid-fight** — Zone-flag checks scan a window centered on the *player*; a player climbing to a non-biome upper tier can drift the scan window off the base strip's biome tiles, reproducing the Hive Mind despawn bug class on the Y-axis. Desert (1500 threshold) and Jungle (narrow qualifying tile set) are the tightest-margin cases.
5. **Shared "arena polish" helper leaking modded types breaks JIT-safety discipline** — the shared helper's public API must be strictly `ushort`/`int`-typed; modded-type resolution (Astral/Briar) must happen only inside the already-guarded `ApplyPass()` methods, never inside the shared, untagged helper or pass.
6. **Custom exit object bypassing `SubworldSystem.Exit()`** — any new return-portal/teleporter must call `SubworldSystem.Exit()` directly (never a hand-rolled `Player.Teleport`), and `noReturn` must not be set on any arena unless the new object's reachability is proven in every layout state — this protects the load-bearing vanilla-downed-flag snapshot/restore guard.

## Implications for Roadmap

Based on combined research, suggested phase structure:

### Phase 1: Shared Arena-Polish Foundation (plain arena only)
**Rationale:** Zero biome-flag concerns and zero JIT concerns on the plain arena — the safest possible integration target to prove the shared-layer design before touching any biome-specific code.
**Delivers:** `ArenaBuilder.cs` (mod-agnostic static helper) + `ArenaPolishPass.cs` (shared `GenPass`: boundary walls, Y-limit containment, interval torches, multi-tier decking, return-portal placement), wired into the plain arena's `Tasks` list only.
**Addresses:** Fall/void boundary, torch lighting (table stakes), multi-tier platform structure (table stakes, generic version)
**Avoids:** Pitfall 1 (bake falling-tile safety into the helper from day one), Pitfall 5 (get the primitive-only API right before any call site exists), Pitfall 3 (parameterize on `surfaceY`/thickness from the start, even though the plain arena only exercises one value)

### Phase 2: Extend Shared Layer to Vanilla Biome Variants (Corruption, Hallow, Underworld, Jungle, Space, Desert)
**Rationale:** These 6 carry the same JIT profile as the plain arena (zero modded-type references), so extending the proven Phase 1 pass here is low-risk mechanical repetition — but each has a genuinely different `surfaceY` and, for Space/Underworld, a hard Y-window from its Zone-flag check, and Desert/Jungle have the tightest biome-tile-weight margins.
**Delivers:** One-line `Tasks`-list append + correct `surfaceY`/thickness constructor args per arena; live in-game verification that boundary walls sit sanely relative to each arena's real platform.
**Addresses:** Fall/void + Y-range boundary blocks (table stakes) for the remaining vanilla arenas
**Avoids:** Pitfall 3 (per-arena `surfaceY`, verified live in Space/Underworld specifically, the most divergent cases), Pitfall 4 (test Desert and Jungle multi-tier drift most carefully — tightest margins)

### Phase 3: Extend Shared Layer to Modded Biome Variants (Astral, Briar)
**Rationale:** The only 2 of 9 arenas with an existing JIT-tagged surface — deliberately sequenced last so the shared pass's mod-agnostic API is already proven stable before touching the highest-risk integration points.
**Delivers:** `ArenaPolishPass` appended to Astral/Briar `Tasks` lists; live-verified with CalamityMod disabled (Astral) and SpiritMod disabled (Briar) separately, mirroring the project's established per-mod-disable smoke test.
**Addresses:** Boundary/lighting/tier parity for the last 2 arenas
**Avoids:** Pitfall 5 (this is exactly the scenario the codebase was burned by once already — repeat the disable-and-reload smoke test as the acceptance criterion, not just a code review)

### Phase 4: Per-Biome Decorative Theming
**Rationale:** The one piece of this milestone that is genuinely non-shareable — requires biome-specific tile types resolved inside each biome's own `ApplyPass()`. Sequenced after the shared containment/lighting layer is stable so decoration can be layered onto a known-safe base.
**Delivers:** Biome-legible decoration (Corruption chasms/purple grass, Hallow crystal tones, Underworld ash/obsidian/lava glow, Jungle mud/vines, Desert dunes/sandstone, Astral/Briar mod-native palettes) added inside each `*PlatformPass.ApplyPass()`, additive to each biome's existing Zone-flag tile-weight set.
**Uses:** `WorldGen.PlaceTile`/`PlaceWall` for frame-important decoration; existing raw-array convention retained for any further bulk fills
**Implements:** Per-biome decoration component (Architecture: "Per-biome decoration explicitly NOT centralized")

### Phase 5: Entry/Exit Convenience
**Rationale:** Touches `Systems/` (runtime tick logic), not `Subworlds/` (world-gen), so it has no dependency on which biome-polish phase is complete — can run in parallel with Phases 2-4, but grouped last here since it's best sanity-checked on the simplest arena first, same as Phase 1's rationale.
**Delivers:** `Tiles/ReturnPortalTile.cs` (mirrors `Test1Tile`, calls `SubworldSystem.Exit()`), a prep-time countdown before `BossSummonPlayer`'s auto-summon (refactored from `ForcedTimeSystem`'s existing tick-driven pattern), and the `BiomeOverridePlayer` gate fix (switch from `IsActive<BossArenaSubworld>()` to `IsAnyArenaActive()`).
**Delivers:** Return-point convenience + prep beat (table stakes) without disturbing the existing OnEnter/OnExit flag-restore guard
**Avoids:** Pitfall 6 (never hand-roll a teleport, never touch `noReturn`), Pitfall 7 (treat the prep delay as a refactor of `BossSummonPlayer`'s existing consume-once invariant, not a bolt-on)

### Phase Ordering Rationale

- Shared-layer-first, plain-arena-first ordering directly follows the Architecture research's own recommended Build Order and lets the highest-risk JIT-safety and Zone-flag-parameterization decisions get proven cheaply before they're replicated across 8 more files.
- Vanilla-biome-variants before modded-biome-variants (Astral/Briar) isolates the JIT-discipline risk to a single, late, clearly-scoped phase with its own acceptance test (disable-mod-and-reload), rather than interleaving it with every other phase's verification.
- Per-biome decoration is deliberately its own phase, after (not interleaved with) the shared containment/lighting work, because it's the one part of the milestone that cannot be shared and carries the Zone-flag-tile-weight regression risk (Pitfall 4) — isolating it makes that risk easier to test per-biome rather than compounding it with new containment-wall changes in the same commit.
- Entry/exit convenience is systems-layer, not world-gen, so it's architecturally independent of the biome-polish phases — sequenced last mainly for testing discipline (validate on the simplest arena) rather than a hard dependency.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 3 (Astral/Briar polish):** Needs a targeted look at whether `AstralSand`/`HardenedAstralSand` register as falling tiles (unverified this session) before any Astral-specific falling-tile decoration is added — feeds directly into Pitfall 1.
- **Phase 4 (per-biome decoration):** Each biome's exact qualifying tile/weight set (documented in each `*PlatformPass.cs` header comment) must be re-read per-biome before adding any new tile, not assumed from one biome's convention — Desert (1500 threshold, least margin) and Jungle (narrow qualifying set) warrant the most care.
- **Phase 5 (entry/exit convenience):** The prep-time countdown's interaction with `RequiresInfernumToggle`-gated bosses (the toggle-force call currently lives inside the same `OnEnterWorld` read as the boss-type consume) needs explicit design attention before implementation, not just testing after the fact.

Phases with standard patterns (skip research-phase):
- **Phase 1 (shared foundation):** Vanilla `WorldGen`/`TileID`/`TorchID`/`ModTile` APIs are well-documented (HIGH confidence, official docs) and this project's own `Test1Tile.cs`/`ArenaBuilder`-equivalent patterns are already established conventions.
- **Phase 2 (vanilla biome variants):** Mechanical repetition of a proven Phase 1 pattern with per-arena constants already known and documented in existing code comments.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | MEDIUM-HIGH | Core `WorldGen`/`ModTile`/`TileID`/`TorchID` APIs verified against official tModLoader docs; StructureHelper alternative verified against its own wiki but not the actually-installed local copy (moot since not recommended) |
| Features | MEDIUM-HIGH | Vanilla arena/spawn mechanics verified against official wiki.gg; project-specific constraints (Zone-flag thresholds, Y-windows) verified directly against this repo's own `*PlatformPass.cs` header comments; safe-wall-behind-GenPass-placement mechanic is explicitly LOW confidence and flagged for live verification |
| Architecture | HIGH (current-state), MEDIUM (one assumption) | Current repository structure read directly; the one flagged assumption is that `Subworld.Tasks` list order is guaranteed sequential execution — inferred from standard `GenPass` semantics but never actually exercised in this codebase (every existing `Tasks` list has exactly one entry today) — recommend a quick in-game check as the literal first implementation step |
| Pitfalls | HIGH (codebase-specific), MEDIUM (general vanilla mechanics) | Codebase-specific findings read directly from source and existing resolved-debug docs; general Terraria mechanic claims verified against two independent wiki fetches; the "Torch God's Favor wrong-biome debuff inside a throwaway subworld" claim is explicitly LOW confidence (unverified recall) and kept out of the Critical Pitfalls list accordingly |

**Overall confidence:** MEDIUM-HIGH

### Gaps to Address

- **`Tasks` list execution order (Architecture, MEDIUM):** Verify with a simple in-game/log check (e.g. confirm `Main.tile[x, surfaceY].HasTile == true` inside `ArenaPolishPass.ApplyPass()`) before relying on "polish pass sees the already-filled floor" for anything load-bearing. Do this as the literal first step of Phase 1.
- **GenPass-placed wall "safe" status (Features, LOW):** Whether a programmatically-placed (not player-clicked) safe-flagged `WallID` actually suppresses spawns the same way a player-placed one does is unconfirmed. Treat safe-wall spawn suppression as an opportunistic bonus to verify live, not a blocking dependency for the fall-prevention wall work itself (which is needed regardless).
- **AstralSand/HardenedAstralSand falling-tile registration (Pitfalls, unverified):** Confirm via decompile or live test before using either tile decoratively in a way that could reintroduce the Desert-class stack-overflow bug in the Astral arena.
- **Torch God's Favor debuff inside a `ShouldSave = false` subworld (Pitfalls, LOW/unverified):** Worth a quick manual check once torches are placed in biome-mismatched arenas, but not blocking — low player impact even if true.

## Sources

### Primary (HIGH confidence)
- Direct read of current repository source: `Subworlds/*.cs` (all 9 `Subworld`/`GenPass` pairs), `Systems/BossArenaRoutingRegistry.cs`, `Systems/BossSummonPlayer.cs`, `Systems/ForcedTimeSystem.cs`, `Tiles/Test1Tile.cs`
- `.planning/PROJECT.md` — project scope, Key Decisions, Phase 9 D-01/Pitfall 4 JIT discipline precedent
- `.planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md`, `.planning/debug/resolved/isolation-premise-flag-persistence.md` — precedent bugs this research repeatedly cross-references
- https://docs.tmodloader.net/docs/preview/class_world_gen.html — `PlaceTile`/`PlaceObject`/`PlaceWall` signatures
- https://docs.tmodloader.net/docs/1.4-stable/class_terraria_1_1_i_d_1_1_torch_i_d-members.html — `TorchID` constants
- https://docs.tmodloader.net/docs/stable/class_mod_player.html, class_player.html, class_mod_block_type.html — hook ordering, `Teleport` scope, `PreDraw` semantics
- https://terraria.wiki.gg/wiki/Guide:Arena, https://terraria.wiki.gg/wiki/NPC_spawning, https://terraria.wiki.gg/wiki/Background_walls — vanilla arena/spawn-mechanic conventions, torches-don't-suppress-spawns correction

### Secondary (MEDIUM confidence)
- https://github.com/ScalarVector1/StructureHelper/wiki/Generator-%283.0%29 — considered alternative, not recommended this milestone
- https://terraria.fandom.com/wiki/Guide:Moon_Lord_strategies — 4+ layer arena convention, duck-under-beam corner-block convention
- https://github.com/tieeeeen1994/tModLoader-BossRush — boss-rush prep/teleport sequencing reference

### Tertiary (LOW confidence)
- "Torch God's Favor" wrong-biome-torch debuff inside a throwaway subworld — unverified recall, not promoted to a Critical Pitfall
- Whether GenPass-placed walls inherit "safe" spawn-suppression status identically to player-placed walls — explicitly flagged as needing live verification

---
*Research completed: 2026-08-15*
*Ready for roadmap: yes*
