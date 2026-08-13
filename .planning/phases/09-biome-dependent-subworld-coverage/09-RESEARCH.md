# Phase 9: Biome-Dependent Subworld Coverage - Research

**Researched:** 2026-08-14
**Domain:** Vanilla tModLoader biome/Zone-flag detection (`Player.UpdateBiomes()`, `SceneMetrics`, `TileID.Sets`) and Calamity/Spirit's own modded-biome equivalents (`ModBiome.IsBiomeActive`, `TileCountsAvailable` hook), applied to building 9 new `Subworld`+`GenPass` pairs that reproduce each biome's real Zone-flag detection inside a content-free arena — extending the `BossArenaCorruptionSubworld`/`CorruptionPlatformPass` precedent (Phase 4) to Hallow, Underworld, Astral Infection, Jungle, Space, Dungeon, Desert, Briar, Sulphurous Sea.
**Confidence:** HIGH — every threshold, tile-weight table, and Zone-flag formula below was read directly out of decompiled `tModLoader.dll` (`D:\SteamLibrary\steamapps\common\tModLoader\tModLoader.dll`), `Libs/CalamityMod.dll`, and the already-decompiled local source tree `ModReader/SpiritMod/`, following this project's own established `hivemind-zonecorrupt-despawn-corruption-subworld.md` methodology (`Player.UpdateBiomes()` → `SceneMetrics` → `TileLoader.RecountTiles()`/`TileCountsAvailable` → `TileID.Sets`/mod-specific weight tables). No claim below is inherited from training-data assumptions about vanilla biome mechanics.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**D-01 — Phase sequencing:** Phase 9 proceeds now, ahead of Phase 6/7/8, building the 9 biome `Subworld`/`GenPass` pairs preemptively so Phase 6/7 can wire `BossArenaRoutingRegistry.Register<T>()` calls in later when those bosses are actually registered.

**D-02 — Portal/entry architecture NOT changing:** No new player-facing altar/portal items. The existing single `Test1Tile` remains the ONLY entry point, with fully automatic routing via `BossArenaRoutingRegistry`. The "recolored Demon Altar item per biome" concept is explicitly REJECTED — altar names survive only as internal `Subworld`/`GenPass` class-name reference. Do not build a multi-altar item system.

**D-03 — Mod/boss scope:** Calamity + Spirit + CatalystMod only. Redemption and NoxusBoss/Wrath of the Gods are entirely excluded from this phase's biome-coverage goal (not researched here).

**D-04 — Infernum handling deferred to Phase 6:** Phase 9 does NOT implement `ModLoader.HasMod("InfernumMode")`-conditional registration logic. This phase only builds the biome `Subworld`/`GenPass` infrastructure. The actual conditional `Register<T>()` calls (and Infernum presence checks) belong to Phase 6.

**D-05 — Day/night forcing out of scope:** A forced day/night mechanism (needed for Astrum Deus/Aureus under Infernum) is explicitly OUT of Phase 9's scope. ARENA-01 covers biome/Zone-flag dependence only, not time-of-day.

**D-06 — Build scope: all 9 biome variants, not a subset:** Build Hallow, Underworld, Astral Infection, Jungle, Space, Dungeon, Desert, Briar, Sulphurous Sea in this single phase — including Sulphurous Sea, which currently has zero assignable boss without Infernum, built now for uniform-marginal-cost reasoning (matches `PROJECT.md`'s "no boss priority ordering" principle).

### Claude's Discretion

- Exact vanilla/modded tile IDs, weights, and fill thickness needed to reliably satisfy each biome's Zone-flag detection — **resolved by this research below, per-biome, via direct decompilation** (not assumed from memory).
- Exact class/file naming for the 9 new `Subworld`/`GenPass` pairs — this research follows the existing `BossArenaCorruptionSubworld`/`CorruptionPlatformPass` naming convention exactly (e.g. `BossArenaHallowSubworld`/`HallowPlatformPass`).
- Whether each new Subworld class duplicates `BossArenaCorruptionSubworld`'s vanilla-downed-flag `OnEnter`/`OnExit` snapshot/restore guard verbatim — **confirmed YES below**, this guard is required independently per subclass (SubworldLibrary's `CopyDowned()`/`ReadCopiedDowned()` applies per-subworld, not project-wide — see `.planning/debug/resolved/isolation-premise-flag-persistence.md`).
- Astral Infection biome specifically needs a Calamity-specific zone-detection equivalent — **resolved by this research**: it is NOT a vanilla weighted-tile-count system at all, but a modern `ModBiome.IsBiomeActive()` + `TileCountsAvailable(ReadOnlySpan<int>)` hook (`CalamityMod.BiomeManagers.AstralInfectionBiome` + `CalamityMod.Systems.BiomeTileCounterSystem`), a structurally different mechanism than vanilla's `TileID.Sets`/`SceneMetrics` system. Full detail below.

### Deferred Ideas (OUT OF SCOPE)

- Mod of Redemption bosses, NoxusBoss/Wrath of the Gods bosses — permanently excluded, not researched.
- Infernum-conditional registration logic — deferred to Phase 6.
- Forced day/night utility mechanism — deferred, not a tracked requirement.
- Actual `BossArenaRoutingRegistry.Register<T>()` calls connecting any boss to these new subworlds — that is Phase 6/7's job. This phase builds the 9 `Subworld`/`GenPass` pairs only, structurally ready for a future `Register<T>()` call.
- CatalystMod's Astrageldon (Astral Infection) — its own registration/summon-item-lockout caveat is Phase 6/7's concern; this phase's Astral Infection subworld only needs to satisfy CalamityMod's own `ZoneAstral`, which is independent of CatalystMod.

</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| ARENA-01 | Every v1-registered boss whose AI depends on a biome/Zone flag has a matching routed biome-variant subworld, audited systematically | This research supplies the decompiled, per-biome Zone-flag detection formula (threshold, tile-weight table, and any extra positional/wall constraint) for all 9 biomes named in `09-CONTEXT.md` D-06, following the exact `Player.UpdateBiomes()`→`SceneMetrics`→`TileID.Sets` tracing methodology already used for Corruption in Phase 4, extended to Calamity's own `ModBiome`/`BiomeTileCounterSystem` mechanism for Astral Infection/Sulphurous Sea, and Spirit's own `ModBiome`/`BiomeTileCounts` mechanism for Briar. Also identifies a previously-untested JIT-safety risk specific to this phase (Calamity/Spirit type references living inside a `Subworld`'s `Tasks` getter rather than an `[JITWhenModsEnabled]`-tagged `Integrations/*.cs` method) and confirms it is safe by construction as long as the existing lazy-property-getter pattern is followed. |

</phase_requirements>

## Summary

This phase's entire technical risk is "does the fake arena actually make vanilla/Calamity/Spirit's *real*, per-tick-recomputed biome detection return true," not summon-item mechanics (already solved, Phase 2) or downed-flag persistence (already solved, Phase 3/4/5). Decompiling `Terraria.Player.UpdateBiomes()`, `Terraria.SceneMetrics`, and `Terraria.ModLoader.TileLoader.RecountTiles()` in full (not just the `ZoneCorrupt` slice the Phase 4 debug session captured) reveals that **vanilla's 9-ish Zone flags fall into two structurally different families**, and treating them uniformly (as one might assume from Corruption's precedent) would silently under- or over-build several of the 9 target biomes:

1. **Tile-weighted biomes** (Hallow, Jungle, Desert, Dungeon) — same family as Corruption: a per-tick weighted sum of nearby tile types (`SceneMetrics.HolyTileCount`/`JungleTileCount`/`SandTileCount`/`DungeonTileCount`) compared against a threshold. Each has its **own threshold and tile-weight table** (verified below) — Hallow's threshold (125) and Desert's (1500) differ by **12x**, so reusing Corruption's exact fill dimensions without checking the target threshold would under-build Hallow's margin or (more importantly) risk under-building Desert's.
2. **Purely height-based biomes** (Underworld, Space/"Sky") — **no tile composition matters at all**. `ZoneUnderworldHeight = player.Y > Main.UnderworldLayer` (`maxTilesY - 200`) and `ZoneSkyHeight = player.Y <= Main.worldSurface * 0.35`. Building a themed Underworld/Space arena with Hellstone/asteroid tiles is cosmetically nice but **functionally irrelevant to the Zone flag** — the only thing that matters is *where in the subworld's vertical span* the platform sits. This is the single most important, non-obvious finding of this research: two of the nine "biomes" require a **completely different platform Y-position**, not a different tile palette.

A third, orthogonal family exists for the two Calamity biomes:

3. **Modded `ModBiome` biomes** (Astral Infection, Sulphurous Sea) — Calamity does NOT extend vanilla's `TileID.Sets`/`SceneMetrics` weighted-count system for its own biomes. It uses the **modern tModLoader `ModBiome.IsBiomeActive(Player)` + `ModSystem.TileCountsAvailable(ReadOnlySpan<int>)` hook** (`CalamityMod.BiomeManagers.AstralInfectionBiome`/`SulphurousSeaBiome` + `CalamityMod.Systems.BiomeTileCounterSystem`), reading the SAME underlying per-tick tile-count scan vanilla uses, but through a different, mod-specific API surface with its OWN thresholds (Astral: 950 tiles + `!ZoneDungeon`; Sulphur: 300 tiles, unconditional). Spirit's Briar biome (`SpiritMod.Biomes.BriarSurfaceBiome`/`BriarUndergroundBiome` + `BiomeTileCounts`) uses the identical modern pattern, one layer of abstraction below Calamity's.

A fourth finding, not anticipated by `09-CONTEXT.md`, is a **new class of JIT-safety risk specific to this phase**: `Subworld` (SubworldLibrary) is itself an auto-loaded tModLoader `ModType` (confirmed via decompiling `SubworldLibrary.Subworld : ModType, ILoadable`) — every `Subworld` subclass in this assembly gets instantiated and has `Register()`/`SetupContent()`/`SetStaticDefaults()` invoked **unconditionally at mod load, regardless of whether CalamityMod/SpiritMod is installed**. Unlike `Integrations/CalamityIntegration.cs` (where the JIT boundary is an explicit `[JITWhenModsEnabled]`-tagged method), the JIT boundary for `BossArenaAstralSubworld`/`AstralPlatformPass` must instead be the **lazy property getter** (`Tasks => new() { new AstralPlatformPass(...) }`) — confirmed NOT invoked during `Register()`/`SetupContent()` (only inside `SubworldSystem`'s `LoadSubworld()`, itself only reachable via `Enter<T>()`, itself only ever called from an `[JITWhenModsEnabled]`-guarded `Integrations/*.cs` registration). This is safe by construction, but ONLY if Calamity/Spirit type references are kept exclusively inside the `Tasks`/`ApplyPass()` method bodies and never leak into a constructor, field initializer, or `SetStaticDefaults()` override.

**Primary recommendation:** Build all 9 `Subworld`/`GenPass` pairs by copying `BossArenaCorruptionSubworld.cs`/`CorruptionPlatformPass.cs` verbatim (including the duplicated vanilla-downed-flag guard) and substituting, per biome, the exact fill spec in the table below — paying special attention to Underworld/Space's height-only placement (no tile fill needed for the Zone flag itself), Dungeon's wall requirement (not just tiles), Jungle's zero-weight Mud pitfall, and Astral Infection/Sulphurous Sea/Briar's `ModContent.TileType<...>()` calls being confined to `ApplyPass()` bodies only.

## Standard Stack

### Core (unchanged from Phases 1-5, no new runtime dependency)

| Component | Version | Purpose | Why Standard |
|-----------|---------|---------|---------------|
| tModLoader | 1.4.4.9 (locally installed at `D:\SteamLibrary\steamapps\common\tModLoader\tModLoader.dll`) | Source of vanilla `Player.UpdateBiomes()`/`SceneMetrics`/`TileLoader`/`TileID.Sets` | Already the project's compile target; decompiled directly this pass |
| CalamityMod | 2.2.4 (`Libs/CalamityMod.dll`, already referenced per `build.txt`) | Source of `CalamityPlayer.ZoneAstral`/`ZoneSulphur`, `AstralInfectionBiome`/`SulphurousSeaBiome`, `BiomeTileCounterSystem` | Already a weak reference; no new build.txt/csproj change needed for this phase |
| SpiritMod | 1.5.0.44 (`Libs/SpiritMod.dll`, already referenced; source also at `ModReader/SpiritMod/`) | Source of `BriarSurfaceBiome`/`BriarUndergroundBiome`, `BiomeTileCounts` | Already a weak reference; no new build.txt/csproj change needed for this phase |
| SubworldLibrary | Already `modReferences` | `Subworld`/`GenPass` base classes; confirmed `Subworld : ModType, ILoadable` (autoloaded) this pass | Existing dependency |

**No `build.txt` or `.csproj` changes required this phase** — CalamityMod and SpiritMod are already declared as `weakReferences` (`weakReferences = CalamityMod@2.2.4, SpiritMod@1.5.0.44`), and this phase adds no new mod dependency (CatalystMod is NOT touched — Astral Infection's Zone flag is entirely CalamityMod's own, confirmed below; CatalystMod's Astrageldon registration is deferred to Phase 6/7 and is out of this phase's scope per D-03/deferred ideas).

### CatalystMod availability note (does not block this phase)

`CatalystMod.dll` is **not** currently extractable locally: it is not present in `Mods/` (not currently subscribed — `Mods/enabled.json` lists only `CalamityModMusic, SubworldLibrary, CheatSheet, BossArenaSubWorld, CalamityMod, SpiritMod, BossChecklist`), and the global `ModReader/CatalystMod/extract.log` shows every file marked `[hidden]` (the mod author enabled tModReader's "hide code" flag). `LastLaunchedMods.txt` confirms the last-played version was `CatalystMod 1.1.8`. This is a real gap for whichever future phase registers Astrageldon, but **does not block Phase 9**: the Astral Infection subworld this phase builds only needs to satisfy `CalamityMod.CalPlayer.CalamityPlayer.ZoneAstral`, which this research fully resolves from `CalamityMod.dll` alone (no CatalystMod involvement in the Zone-flag mechanism itself).

## Architecture Patterns

### Recommended Project Structure (extends existing `Subworlds/`)

```
Subworlds/
├── BossArenaSubworld.cs              # existing, unchanged (default/plain arena)
├── FlatStonePlatformPass.cs          # existing, unchanged
├── BossArenaCorruptionSubworld.cs    # existing, unchanged (Phase 4)
├── CorruptionPlatformPass.cs         # existing, unchanged (Phase 4)
├── BossArenaHallowSubworld.cs        # NEW
├── HallowPlatformPass.cs             # NEW
├── BossArenaUnderworldSubworld.cs    # NEW
├── UnderworldPlatformPass.cs         # NEW
├── BossArenaAstralSubworld.cs        # NEW (Calamity types confined to ApplyPass())
├── AstralPlatformPass.cs             # NEW
├── BossArenaJungleSubworld.cs        # NEW
├── JunglePlatformPass.cs             # NEW
├── BossArenaSpaceSubworld.cs         # NEW (height-only, no special tiles)
├── SpacePlatformPass.cs              # NEW
├── BossArenaDungeonSubworld.cs       # NEW
├── DungeonPlatformPass.cs            # NEW (tiles + wall)
├── BossArenaDesertSubworld.cs        # NEW
├── DesertPlatformPass.cs             # NEW
├── BossArenaBriarSubworld.cs         # NEW (Spirit types confined to ApplyPass())
├── BriarPlatformPass.cs              # NEW
├── BossArenaSulphurousSubworld.cs    # NEW (Calamity types confined to ApplyPass())
└── SulphurousPlatformPass.cs         # NEW
```

Each `BossArenaXSubworld.cs` is a **verbatim structural copy** of `BossArenaCorruptionSubworld.cs`: same `PlatformWidth`/`WorldHeight` constants (unless the biome needs a different `WorldHeight` — none do, see below), same `ShouldSave = false`/`NoPlayerSaving = false`, same duplicated vanilla-downed-flag `OnEnter`/`OnExit` guard (33 fields, copy-pasted, per `isolation-premise-flag-persistence.md`), differing only in `Tasks` returning the biome-specific `GenPass`.

### Foundational finding: `Main.worldSurface`/`Main.rockLayer`/`Main.UnderworldLayer` are fixed functions of `WorldHeight`, set BEFORE any custom `GenPass` runs

Decompiling `SubworldLibrary.SubworldSystem.LoadSubworld(string, bool)` (`Libs/SubworldLibrary.dll`) shows the exact sequence:

```csharp
// Source: SubworldLibrary.SubworldSystem.LoadSubworld (decompiled)
Main.maxTilesX = current.Width;
Main.maxTilesY = current.Height;
Main.spawnTileX = Main.maxTilesX / 2;
Main.spawnTileY = Main.maxTilesY / 2;
WorldGen.setWorldSize();
WorldGen.clearWorld();
Main.worldSurface = (double)Main.maxTilesY * 0.3;   // <-- fixed BEFORE any GenPass runs
Main.rockLayer = (double)Main.maxTilesY * 0.5;      // <-- fixed BEFORE any GenPass runs
GenVars.waterLine = Main.maxTilesY;
// ... then current.Tasks (our GenPass list) runs
```

And `Terraria.Main.UnderworldLayer => maxTilesY - 200` (a computed property, always live).

With the existing project's `WorldHeight = 800` (used by both `BossArenaSubworld` and `BossArenaCorruptionSubworld`), this yields **fixed, deterministic thresholds usable by every new subworld that keeps the same `WorldHeight = 800`**:

| Constant | Formula | Value @ WorldHeight=800 |
|---|---|---|
| `worldSurface` | `maxTilesY * 0.3` | 240 |
| `rockLayer` | `maxTilesY * 0.5` | 400 |
| `UnderworldLayer` | `maxTilesY - 200` | 600 |
| `ZoneSkyHeight` boundary | `worldSurface * 0.35` | 84 |

**Recommendation: keep `WorldHeight = 800` for all 9 new subworlds** (do not introduce a per-biome world height) — every biome's placement need is satisfiable within this fixed 800-row span by choosing the right `surfaceY`, as shown in the per-biome table below. This avoids introducing a second free variable (world height) on top of platform Y, keeping every new subworld a drop-in structural copy of the existing template.

### Full per-biome Zone-flag detection table (all entries decompiled this pass)

| Biome | Flag read by boss AI | Mechanism | Threshold | Extra constraint | Recommended `surfaceY` (WorldHeight=800) | Weighted tiles (all weight 1 unless noted) |
|---|---|---|---|---|---|---|
| Hallow | `player.ZoneHallow` | vanilla `SceneMetrics.HolyTileCount` (`TileID.Sets.HallowBiome`) | ≥125 | none | 400 (reuse Corruption's mid-height convention) | `Pearlstone`(117) body, `HallowedGrass`(109) surface — also in table: `HallowedPlants`(110), `HallowedPlants2`(113), `Pearlsand`(116), `HallowedIce`(164), `HallowHardenedSand`(402), `HallowSandstone`(403) |
| Underworld | `player.ZoneUnderworldHeight` | **height only**, no tile composition | N/A | player tile-Y **> `UnderworldLayer` (600)** | **650** (near bottom, NOT mid-height) | None required for the flag itself; `Ash`/`Hellstone` recommended purely for cosmetic fidelity |
| Astral Infection | `player.Calamity().ZoneAstral` | Calamity `ModBiome.IsBiomeActive` + `BiomeTileCounterSystem.AstralTiles` | >950 | **AND `!player.ZoneDungeon`** | 400 | `CalamityMod.Tiles.Astral.AstralStone`, `AstralGrass` (also counted: `AstralDirt`, `AstralSand`, `AstralSandstone`, `HardenedAstralSand`, `AstralIce`, `AstralSnow`, `AstralOre`, `NovaeSlag`, `AstralClay`, `CelestialRemains`) |
| Jungle | `player.ZoneJungle` | vanilla `SceneMetrics.JungleTileCount` (`TileID.Sets.JungleBiome`) | ≥140 | AND player tile-Y `< UnderworldLayer` (600) — trivially true at mid-height | 400 | **`JungleGrass`(60) for the FULL fill thickness** — see Pitfall below, `Mud` (the usual jungle "body" tile) carries **zero** weight |
| Space | `player.ZoneSkyHeight` | **height only**, no tile composition | N/A | player tile-Y **≤ `worldSurface * 0.35` (84)** | **50** (near top, NOT mid-height) | None required for the flag itself; any solid tile works (`Stone` is fine) |
| Dungeon | `player.ZoneDungeon` | vanilla `SceneMetrics.DungeonTileCount` (`TileID.Sets.DungeonBiome`) **+ exact-tile wall check** | ≥250 | AND `Main.wallDungeon[wall at player's exact tile] == true` AND player tile-Y `> worldSurface` (240) | 400 | `BlueDungeonBrick`(41)/`GreenDungeonBrick`(43)/`PinkDungeonBrick`(44) tiles **+ `WallID.BlueDungeonUnsafe`(7)** (or 8/9/94-99) wall across full width — see Pitfall: the player-*placeable* "safe" dungeon wall variant does NOT count |
| Desert | `player.ZoneDesert` | vanilla `SceneMetrics.SandTileCount` (`TileID.Sets.SandBiome`) | ≥1500 (high) | none | 400 | `Sand`(53) for full fill thickness (also counted: `Ebonsand`(112), `Pearlsand`(116), `Crimsand`(234), `HardenedSand` family, `Sandstone` family) |
| Briar | Spirit `BiomeTileCounts.InBriar` via `BriarSurfaceBiome`/`BriarUndergroundBiome` | Spirit `ModBiome.IsBiomeActive` + `BiomeTileCounts.briarCount` | >80 | Surface variant: AND `(ZoneSkyHeight \|\| ZoneOverworldHeight)` i.e. Y ≤ 240. Underground variant: AND `(ZoneRockLayerHeight \|\| ZoneDirtLayerHeight)` i.e. 240 < Y < 600 | **150** if targeting Surface variant (matches Vinewrath Bane's `SpawnModBiomes`); **400** if targeting Underground variant (simpler, reuses default mid-height) — see Discretion note below | `SpiritMod.Tiles.Block.BriarGrass` only |
| Sulphurous Sea | Calamity `ZoneSulphur` via `SulphurousSeaBiome` | Calamity `ModBiome.IsBiomeActive` + `BiomeTileCounterSystem.SulphurTiles` | ≥300 | none (tile-count path returns `true` unconditionally once ≥300; no position/height gate) | 400 | `CalamityMod.Tiles.Abyss.SulphurousSand`, `SulphurousSandstone`, `HardenedSulphurousSandstone` |

**No AI-level dependency confirmed (build for wiki-fiction/theming only, per `09-CONTEXT.md`'s wiki-first directive), decompiled this pass:**
- **Ancient Avian** (`SpiritMod.NPCs.Boss.AncientFlyer`, → Space): full source read, no `Zone*`/biome check in `AI`/`PreAI` — only a cosmetic Bestiary "Sky" tag. Same category as Infernon (Phase 5).
- **Starplate Voyager** (`SpiritMod.NPCs.Boss.SteamRaider.SteamRaiderHead`, → Space/Asteroid): full source read, no `Zone*`/biome despawn check in `AI`. `SpawnModBiomes = AsteroidBiome` only affects natural-spawn eligibility (irrelevant to this project's `NPC.SpawnOnPlayer` mechanism, same category as `CursedCloth`'s depth gate in Phase 5).
- **Vinewrath Bane** (`SpiritMod.NPCs.Boss.ReachBoss.ReachBoss1`, → Briar): full source read, no `Zone*`/biome check in `AI`. `SpawnModBiomes = BriarSurfaceBiome` only affects natural-spawn eligibility, same category.

**Confirmed genuine AI-level dependency (decompiled this pass, not just thematic):**
- **Scarabeus** (`SpiritMod.NPCs.Boss.Scarabeus.Scarabeus`, → Desert): `ModifyHitByProjectile`/`ModifyHitByItem` both divide incoming damage by 3 when `!player.ZoneDesert`. This is NOT a despawn bug (Hive Mind's category) but a genuine **3x-longer-fight** balance dependency — confirms the Desert subworld is functionally, not just thematically, needed for this boss.

## Common Pitfalls

### Pitfall 1: Assuming every biome needs a themed tile fill (Underworld/Space are height-only)
**What goes wrong:** Building `BossArenaUnderworldSubworld`/`BossArenaSpaceSubworld` with the same mid-height (`surfaceY = 400`) placement used by every other biome, because that's what the Corruption/Hallow/Jungle/Desert/Dungeon precedent looks like.
**Why it happens:** `Player.UpdateBiomes()`'s tile-weighted family (Corrupt/Crimson/Hallow/Jungle/Snow/Desert/Glowshroom/Meteor) is a much larger, more visible code block than the two one-line height checks (`ZoneUnderworldHeight`, `ZoneSkyHeight`), making it easy to assume all Zone flags work the same way.
**How to avoid:** Underworld needs `surfaceY ≥ 601` (tile row, i.e. `> UnderworldLayer = maxTilesY - 200`); Space needs `surfaceY ≤ 84` (`≤ worldSurface * 0.35`). At `WorldHeight = 800`, recommended concrete values are `650` (Underworld) and `50` (Space) — see the per-biome table.
**Warning signs:** If a future live checkpoint shows an Underworld- or Space-gated boss still despawning/behaving as if the biome weren't detected, check platform Y first, not tile composition — the tile-fill code could be flawless and still fail purely because it's in the wrong vertical third of the subworld.

### Pitfall 2: Jungle's "body" tile (Mud) carries zero `JungleBiome` weight
**What goes wrong:** Copying Corruption's two-tile pattern (a bulk "stone" body + a thin "grass" surface row) naively for Jungle — filling most of the platform with `Mud` (the vanilla Jungle "dirt" tile) and only the top row with `JungleGrass`.
**Why it happens:** `TileID.Sets.JungleBiome` (decompiled) is `CreateIntSet(0, 60,1, 61,1, 62,1, 74,1, 226,1, 225,1)` — `JungleGrass`(60), `JunglePlants`(61), `JungleVines`(62), `JunglePlants2`(74), `LihzahrdBrick`(226), `Hive`(225). **`Mud` (the actual body tile, ID 59) is not in this table at all** — unlike Corruption where both `Ebonstone` (body) and `CorruptGrass` (surface) carry weight 1.
**How to avoid:** Fill the FULL platform thickness with `JungleGrass`(60), not a thin surface veneer over `Mud`. `JungleGrass` is itself a solid, mergeable tile (visually a green-topped block) — there is no need for a separate "body" tile at all.
**Warning signs:** `JungleTileCount` staying near zero despite an apparently large fill, if a Mud-body design were used by mistake.

### Pitfall 3: Dungeon needs a WALL, not just tiles — and only the "unsafe" wall variant counts
**What goes wrong:** Filling the platform with Dungeon Brick tiles alone (mirroring every other tile-weighted biome), assuming `DungeonTileCount ≥ 250` alone flips `ZoneDungeon` true.
**Why it happens:** `Player.UpdateBiomes()` (decompiled, full method): `ZoneDungeon` additionally requires, at the player's EXACT current tile (`Center.X/16, Center.Y/16`), `Main.wallDungeon[Main.tile[x,y].wall] == true` AND `Center.Y > worldSurface*16`. This is a per-position check layered on top of the weighted count, unlike every other tile-weighted biome in this table.
**How to avoid:** Place a Dungeon wall (`tile.WallType = WallID.BlueDungeonUnsafe` /* 7 */, or 8/9/94-99) across the FULL platform width — matching the full-width-tile-fill philosophy already established, since the player can stand anywhere across the platform. **Critically, decompiling `Main.wallDungeon`'s initializer shows only the "Unsafe" wall constants are flagged true** (`BlueDungeonUnsafe`=7, `GreenDungeonUnsafe`=8, `PinkDungeonUnsafe`=9, `BlueDungeonSlabUnsafe`=94, `BlueDungeonTileUnsafe`=95, `PinkDungeonSlabUnsafe`=96, `PinkDungeonTileUnsafe`=97, `GreenDungeonSlabUnsafe`=98, `GreenDungeonTileUnsafe`=99) — the player-*placeable* "safe" Dungeon Brick Wall item (a different WallID) is NOT in this set and will NOT satisfy the check, even though it looks visually identical in-game.
**Warning signs:** `DungeonTileCount` reads correctly (≥250) but `ZoneDungeon` still false — check the wall type at the player's exact tile next, not the tile count.

### Pitfall 4: Leaking a Calamity/Spirit type reference outside the `Tasks`/`ApplyPass()` lazy boundary
**What goes wrong:** Writing `BossArenaAstralSubworld`'s constructor, a field initializer, or a `SetStaticDefaults()` override to reference a Calamity type directly (e.g. `ModContent.TileType<AstralStone>()` as a field initializer instead of inside `ApplyPass()`), on the assumption that `[JITWhenModsEnabled]`-style isolation only matters for `Integrations/*.cs`.
**Why it happens:** `SubworldLibrary.Subworld` (decompiled) is declared `public abstract class Subworld : ModType, ICopyWorldData, ILoadable, ...` — tModLoader's standard `ModType` autoload pipeline instantiates every `Subworld` subclass and calls `Register()` (`ModTypeLookup<Subworld>.Register(this); SubworldSystem.subworlds.Add(this);`) and `SetupContent()` (`SetStaticDefaults()`) **unconditionally at mod load**, regardless of whether CalamityMod/SpiritMod is installed. This is different from `Integrations/*.cs`'s `PostSetupContent()`-gated pattern.
**How to avoid:** Confirmed safe by construction (decompiled `SubworldSystem.LoadSubworld()`): `current.Width`/`Height`/`Tasks` are only READ inside `LoadSubworld()`, itself only reachable via `SubworldSystem.Enter<T>()`, itself only ever called from inside an `[JITWhenModsEnabled("CalamityMod")]`-tagged registration method (which only runs if `ModLoader.HasMod("CalamityMod")` was already true). Since C#/CLR JIT-compiles method bodies lazily, the `Tasks =>` property getter (and `AstralPlatformPass.ApplyPass()`) are never touched if the biome subworld is never entered — mirroring exactly how `CalamityIntegration.RegisterHiveMind()`'s own JIT boundary works, just at a different call site. **The discipline required: keep every Calamity/Spirit type reference strictly inside the `Tasks` getter body and the `GenPass.ApplyPass()` method body — never in a constructor, field initializer, or `SetStaticDefaults()` override.**
**Warning signs:** A JITException naming `BossArenaAstralSubworld` (not a method inside `Integrations/CalamityIntegration.cs`) during the CalamityMod-disabled live checkpoint would indicate this discipline was violated somewhere in the new Subworld/GenPass classes.

### Pitfall 5: Desert's threshold (1500) is 5-12x higher than the other tile-weighted biomes
**What goes wrong:** Reusing Corruption's exact 15-tile-thick fill without checking whether it clears Desert's threshold with adequate margin.
**Why it happens:** `SceneMetrics.DesertTileThreshold = 1500` vs. `CorruptionTileThreshold = 300`, `HallowTileThreshold = 125`, `JungleTileThreshold = 140`, `DungeonTileCount` threshold `250`. A 15-thick × ~200-wide-scan-window slice of weight-1 tiles gives 3000 — still 2x over threshold, so the existing 15-tile convention is NOT broken, but the margin is much thinner than Corruption's ~10x. Not a functional bug, but worth flagging so a future biome-tile-weight change (e.g. a tModLoader update altering `DesertTileThreshold`) is caught by re-verification, not assumed stable forever.
**How to avoid:** Keep 15-tile thickness (verified sufficient, ~2x margin) or increase to 20 for extra safety margin if the planner wants parity with Corruption's comfort level. Either is acceptable; document the threshold explicitly in the GenPass's own comment (matching this project's established documentation convention) so a future re-verification pass knows exactly what margin was assumed.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|--------------|-----|
| Detecting whether a biome's Zone flag will be true in a given arena | Guessing tile IDs from memory/prior Terraria knowledge, or copying Corruption's exact fill dimensions for every biome uncritically | Decompile the specific `SceneMetrics.EnoughTilesFor<X>` / `ModBiome.IsBiomeActive` for THAT biome, per this document's table | Already caught 3 non-obvious divergences this pass (Underworld/Space are height-only; Jungle's Mud carries zero weight; Dungeon needs a wall, not just tiles) that a "just copy Corruption" approach would silently miss |
| Forcing `player.ZoneX = true` every tick via a `ModPlayer` override | A per-boss `BiomeOverridePlayer` Zone-flag poke, growing indefinitely as more boss biomes are added | A real tile/wall/height-composed arena (this document's approach) | Already explicitly rejected by the user twice (Phase 4); `Player.UpdateBiomes()` recomputes every tick from `Main.SceneMetrics`, so a one-shot flag override doesn't survive the next tick anyway — a real biome is the only approach that's both faithful and doesn't need an every-tick hook |
| Reproducing Calamity's Astral Infection/Sulphurous Sea zone detection | A custom weighted-tile-count reimplementation, assuming Calamity extends vanilla's `TileID.Sets` system the same way vanilla biomes do | `CalamityMod.BiomeManagers.AstralInfectionBiome.IsBiomeActive`/`SulphurousSeaBiome.IsBiomeActive` (already-implemented `ModBiome` classes reading `BiomeTileCounterSystem`'s own count) — this project doesn't need to call these directly, just place enough of the correct tile types for Calamity's own hook to compute the count correctly | Calamity's mechanism is structurally different (modern `ModBiome`+`TileCountsAvailable` hook, not the legacy `TileID.Sets`/`SceneMetrics` weighted-array system) — assuming otherwise would produce code that compiles but never actually satisfies `ZoneAstral`/`ZoneSulphur` |

**Key insight:** This phase's entire job is arena *construction* (place the right tiles/walls at the right Y), never arena *detection-logic* (the game itself computes `ZoneX` every tick from whatever's placed — this project's `GenPass`es never touch `ZoneX` fields directly, following the same non-interventionist discipline already established for `ZoneCorrupt` in Phase 4).

## Architecture Pattern: Representative Code Examples

### Pattern A — Height-only biome (simplest family: Underworld, Space)

No special tile IDs are required for the Zone flag itself; only the platform's Y position matters. Example for Space (`ZoneSkyHeight`):

```csharp
// Source: derived from decompiled Terraria.Player.UpdateBiomes()
// ZoneSkyHeight = (double)val.Y <= Main.worldSurface * 0.3499999940395355;
// At WorldHeight=800, worldSurface=240, so the boundary is tile row 84.
public class SpacePlatformPass : GenPass
{
    public SpacePlatformPass(string name, float loadWeight) : base(name, loadWeight) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating space boss arena platform";

        int surfaceY = 50; // well under the ZoneSkyHeight boundary (84) with margin
        int thickness = 10;

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = surfaceY; y < surfaceY + thickness; y++)
            {
                Tile tile = Main.tile[x, y];
                tile.HasTile = true;
                tile.TileType = TileID.Stone; // cosmetic only -- ZoneSkyHeight has no tile requirement
            }
        }

        Main.spawnTileX = Main.maxTilesX / 2;
        Main.spawnTileY = surfaceY - 3;
    }
}
```

`BossArenaUnderworldSubworld`/`UnderworldPlatformPass` follows the identical shape with `surfaceY = 650` (satisfies `Y > UnderworldLayer = 600`) — optionally `TileID.Ash`/`TileID.Hellstone` for cosmetic fidelity, since neither affects `ZoneUnderworldHeight`.

### Pattern B — Tile-weighted vanilla biome with a positional/wall extra constraint (Dungeon)

```csharp
// Source: derived from decompiled Terraria.Player.UpdateBiomes() + Terraria.Main.wallDungeon
// ZoneDungeon requires: SceneMetrics.DungeonTileCount >= 250 AND
// Main.wallDungeon[wall at player's exact tile] AND player.Center.Y > worldSurface*16
public class DungeonPlatformPass : GenPass
{
    public DungeonPlatformPass(string name, float loadWeight) : base(name, loadWeight) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating dungeon boss arena platform";

        int surfaceY = 400; // > worldSurface(240), satisfies the height half of the check
        int thickness = 15;

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = surfaceY; y < surfaceY + thickness; y++)
            {
                Tile tile = Main.tile[x, y];
                tile.HasTile = true;
                tile.TileType = TileID.BlueDungeonBrick; // weight 1 in TileID.Sets.DungeonBiome
                // Wall MUST be an "Unsafe" dungeon wall variant -- the safe/placeable
                // Dungeon Brick Wall item is NOT in Main.wallDungeon's true set.
                tile.WallType = WallID.BlueDungeonUnsafe; // = 7
            }
        }

        Main.spawnTileX = Main.maxTilesX / 2;
        Main.spawnTileY = surfaceY - 3;
    }
}
```

### Pattern C — Calamity `ModBiome` (Astral Infection), with the JIT-safety discipline from Pitfall 4

```csharp
// Subworlds/BossArenaAstralSubworld.cs
// Calamity types appear ONLY inside the Tasks getter body -- never in a constructor,
// field initializer, or SetStaticDefaults() override. Subworld is an autoloaded ModType
// (SubworldLibrary.Subworld : ModType, ILoadable) whose Register()/SetupContent() run
// unconditionally at mod load regardless of CalamityMod's presence -- see Pitfall 4.
public class BossArenaAstralSubworld : Subworld
{
    public const int PlatformWidth = 10000;
    public const int WorldHeight = 800;

    public override int Width => PlatformWidth;
    public override int Height => WorldHeight;

    // Lazily evaluated -- only invoked by SubworldSystem.LoadSubworld(), itself only
    // reachable via Enter<BossArenaAstralSubworld>(), itself only ever called from an
    // [JITWhenModsEnabled("CalamityMod")]-tagged method in CalamityIntegration.cs.
    public override List<GenPass> Tasks => new()
    {
        new AstralPlatformPass("Astral Infection Boss Arena Platform", 1f)
    };

    public override bool ShouldSave => false;
    public override bool NoPlayerSaving => false;

    // ... duplicated vanilla-downed-flag OnEnter/OnExit guard, verbatim, same 33 fields
    // as BossArenaCorruptionSubworld.cs (omitted here for brevity -- copy exactly).
}
```

```csharp
// Subworlds/AstralPlatformPass.cs
// Source: CalamityMod.BiomeManagers.AstralInfectionBiome.IsBiomeActive (decompiled):
//   return !player.ZoneDungeon && BiomeTileCounterSystem.AstralTiles > 950;
// BiomeTileCounterSystem.TileCountsAvailable (decompiled) sums, with weight 1 each:
// AstralSand/AstralSandstone/HardenedAstralSand/CelestialRemains/AstralIce/AstralSnow/
// AstralDirt/AstralStone/AstralGrass/AstralOre/NovaeSlag/AstralClay.
using CalamityMod.Tiles.Astral;

public class AstralPlatformPass : GenPass
{
    public AstralPlatformPass(string name, float loadWeight) : base(name, loadWeight) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating astral infection boss arena platform";

        int surfaceY = 400; // no height constraint for ZoneAstral itself
        int thickness = 15; // 200(scan window)*15 = 3000 >> 950 threshold

        ushort astralStone = (ushort)ModContent.TileType<AstralStone>();
        ushort astralGrass = (ushort)ModContent.TileType<AstralGrass>();

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = surfaceY; y < surfaceY + thickness; y++)
            {
                Tile tile = Main.tile[x, y];
                tile.HasTile = true;
                tile.TileType = (y == surfaceY) ? astralGrass : astralStone;
                // No dungeon wall/tiles anywhere in this arena -- required so
                // !player.ZoneDungeon holds (IsBiomeActive's AND-condition).
            }
        }

        Main.spawnTileX = Main.maxTilesX / 2;
        Main.spawnTileY = surfaceY - 3;
    }
}
```

`SulphurousPlatformPass` follows the identical Pattern C shape, substituting `CalamityMod.Tiles.Abyss.SulphurousSand`/`SulphurousSandstone` (threshold ≥300, no extra positional constraint), and `BriarPlatformPass` follows it substituting `SpiritMod.Tiles.Block.BriarGrass` (threshold >80) at `surfaceY = 150` if targeting the Surface `ModBiome` variant (see Discretion note below).

### Open discretion: Briar Surface vs. Underground `ModBiome` variant

Both `BriarSurfaceBiome.IsBiomeActive` (`BiomeTileCounts.InBriar && (ZoneSkyHeight || ZoneOverworldHeight)`, i.e. Y ≤ 240) and `BriarUndergroundBiome.IsBiomeActive` (`BiomeTileCounts.InBriar && (ZoneRockLayerHeight || ZoneDirtLayerHeight)`, i.e. 240 < Y < 600) were decompiled and confirmed to exist. Since no AI-level dependency was found for Vinewrath Bane on either variant (biome assignment here is purely for wiki-fiction theming per `09-CONTEXT.md`'s directive), either satisfies ARENA-01's literal "has a matching routed biome-variant subworld" requirement. This research recommends the **Surface** variant (`surfaceY = 150`) for closer fidelity to `ReachBoss1`'s own `SpawnModBiomes = BriarSurfaceBiome` declaration, but flags this as a low-stakes implementation detail the planner may resolve either way without additional research.

## Anti-Patterns to Avoid

- **Building a single shared "biome tile fill" helper parameterized by tile IDs, used identically for all 9 biomes.** Resist this DRY temptation — Underworld/Space need a completely different Y-placement algorithm (no tile fill relevance at all), and Dungeon needs an extra wall-placement step no other biome needs. A shared low-level `FillRect(tileType, y, thickness)` helper is fine; a single "GenerateBiomeArena(BiomeSpec)" abstraction that tries to unify wall-placement, height-only, and tile-weighted logic into one code path is very likely worse than 9 explicit, individually-verifiable `GenPass` classes (each already following an established, working template).
- **Assuming `ZoneDesert`/`ZoneUndergroundDesert` are the same flag.** Scarabeus's confirmed AI dependency reads `player.ZoneDesert` (the simple tile-weighted flag), NOT `ZoneUndergroundDesert` (a different flag requiring `behindBackWall` + `WallID.Sets.Conversion.Sandstone`/`HardenedSand` specifically, decompiled at `Player.UpdateBiomes()` line ~15331). Building the wrong one would compile fine and still not satisfy Scarabeus's actual damage-reduction check.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None — tModLoader mod, no automated in-game test harness (matches Phases 1-5's established precedent) |
| Config file | none |
| Quick run command | `dotnet build BossArenaSubWorld.csproj` |
| Full suite command | N/A — full verification is live in-game per-biome checkpoints (see below) |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| ARENA-01 | All 9 new `Subworld`/`GenPass` pairs compile against `Libs/CalamityMod.dll`/`Libs/SpiritMod.dll` | build (compile-time type check) | `dotnet build BossArenaSubWorld.csproj` | ❌ Wave 0 (18 new files across `Subworlds/`) |
| ARENA-01 | Each of the 9 new subworlds is independently enterable via a temporary debug hook (per `09-CONTEXT.md`'s framing: "9 new empty biome arenas exist and are individually enterable/testable") and does not crash on entry | manual-only | live in-game: temporarily route a known-safe existing summon item (or a debug chat command mirroring Phase 1's now-removed `Debug/SubworldDebugCommands.cs` pattern) to each new subworld type in turn, confirm generation completes and player spawns correctly | ❌ Wave 0 |
| ARENA-01 | The correct Zone/Biome flag actually reads `true` while standing on each new platform (not just "looks right") | manual-only | live in-game: for vanilla-flag biomes, a temporary debug print of `player.ZoneHallow`/`ZoneJungle`/`ZoneDesert`/`ZoneDungeon`/`ZoneUnderworldHeight`/`ZoneSkyHeight`; for Calamity biomes, `player.Calamity().ZoneAstral`/`ZoneSulphur`; for Spirit, `SpiritMod.Biomes.BiomeTileCounts.InBriar` combined with the relevant vanilla height flag | ❌ Wave 0 |
| ARENA-01 | Mod continues to load safely with CalamityMod/SpiritMod disabled (validates Pitfall 4's JIT-safety discipline for the 3 modded-type-referencing subworlds: Astral, Sulphurous, Briar) | manual-only, real checkpoint (mirrors Phase 4/5's D-05) | disable CalamityMod (and separately SpiritMod) in Mod Configuration, launch, confirm no JITException naming `BossArenaAstralSubworld`/`AstralPlatformPass`/`BossArenaSulphurousSubworld`/`SulphurousPlatformPass`/`BossArenaBriarSubworld`/`BriarPlatformPass` | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet build BossArenaSubWorld.csproj` (0 warnings/errors expected)
- **Per wave merge:** same build command
- **Phase gate:** all 9 subworlds individually entered and Zone-flag-confirmed live, plus the CalamityMod/SpiritMod-disabled safety checkpoint, before `/gsd:verify-work` — structured as per-biome or grouped `checkpoint:human-verify` tasks, mirroring Phase 4/5's shape. Given the count (9 biomes), the planner may reasonably group these into 2-3 checkpoint tasks (e.g. "vanilla tile-weighted biomes," "height-only biomes," "modded ModBiome biomes") rather than 9 separate ones, at planning discretion.

### Wave 0 Gaps
- [ ] 9× `Subworlds/BossArenaXSubworld.cs` + 9× `Subworlds/XPlatformPass.cs` — all new files, no automated test beyond the build gate
- [ ] A temporary debug entry mechanism to reach each new subworld individually for live verification, since D-02 forbids any new permanent player-facing entry point — recommend a short-lived debug chat command (removed after this phase's checkpoints pass, mirroring Phase 1/2's now-deleted `Debug/SubworldDebugCommands.cs` precedent) rather than any permanent UI

*No automated test-framework gap beyond the build gate and the live checkpoints — matches this project's established, previously-approved manual-verification model.*

## Open Questions

1. **Exact numeric value of `SceneMetrics`'s per-tick scan window at the resolution live testing will actually use.**
   - What we know: `Main.buffScanAreaWidth`/`buffScanAreaHeight` are sized off `maxScreenWidth`/`maxScreenHeight` (max *supported* resolution, not the tester's current window), confirmed in the Phase 4 debug session as "~200x140 tiles." This research reuses that figure for margin calculations (e.g. Desert's 200×15=3000 vs. threshold 1500).
   - What's unclear: The exact figure was not re-derived from scratch this pass (reused from the prior debug session's finding); if `maxScreenWidth`/`maxScreenHeight` differ from what that session assumed, the safety margins in this table would shift proportionally.
   - Recommendation: Not blocking — every margin calculated in this document has at least ~2x headroom (Desert, the tightest case) even under the existing 15-tile-thickness convention; not worth re-deriving unless a live checkpoint actually shows a threshold-not-met symptom.

2. **Whether `WorldGen.SquareTileFrame`/`NetMessage.SendTileSquare` calls are needed after wall placement in `DungeonPlatformPass`, the way `SpiritIntegration.ReplayInfernonTileRing` calls them for its tile ring.**
   - What we know: `CorruptionPlatformPass` (the direct precedent) sets `tile.HasTile`/`TileType` with no framing/sync calls at all, since this runs during world GENERATION (before any player is present to observe framing artifacts), not during live gameplay.
   - What's unclear: Whether wall tiles specifically need any framing call during GenPass execution that plain tile placement doesn't (e.g. for the wall's visual frame to render correctly on first entry).
   - Recommendation: Follow `CorruptionPlatformPass`'s precedent (no framing calls) as the default; if live testing shows visual wall-frame glitches (not a `ZoneDungeon`-correctness issue, since the Zone check only reads `tile.wall`'s type ID, not its frame), address as a purely cosmetic follow-up, not a phase blocker.

3. **CatalystMod's own biome requirements for Astrageldon**, if any beyond CalamityMod's `ZoneAstral` (e.g. a CatalystMod-specific structure/surface-level requirement mentioned in `09-ALTAR-BIOME-REFERENCE.md`'s "also needs surface-level placement" caveat).
   - What we know: `09-ALTAR-BIOME-REFERENCE.md` Section 3 flags "Astrageldon (CatalystMod — also needs surface-level placement)" as a caveat, and CatalystMod's own DLL is not locally extractable this pass (see Environment Availability).
   - What's unclear: Whether Astrageldon's own AI has a CatalystMod-specific Zone/position check beyond CalamityMod's `ZoneAstral`, and whether this phase's `surfaceY = 400` (mid-height, not surface-level) placement would need to change to accommodate it.
   - Recommendation: Not blocking THIS phase (Astrageldon registration is Phase 6/7's job, per D-03/deferred ideas) — but flag for whichever phase researches CatalystMod that the Astral Infection subworld built here may need a follow-up "surface-level" placement adjustment (or a second Astral variant) once CatalystMod's DLL becomes locally extractable (requires the user to re-subscribe via Steam Workshop).

## Sources

### Primary (HIGH confidence — read directly from locally-installed/decompiled binaries)

- `D:\SteamLibrary\steamapps\common\tModLoader\tModLoader.dll` (decompiled with `ilspycmd 8.2.0.7535`) — `Terraria.Player.UpdateBiomes()` (full method body, all Zone-flag assignments), `Terraria.SceneMetrics` (all thresholds: `HallowTileThreshold=125`, `JungleTileThreshold=140`, `DesertTileThreshold=1500`, `CorruptionTileThreshold=300`, plus `DungeonTileCount`), `Terraria.ModLoader.TileLoader.RecountTiles()`, `Terraria.ID.TileID+Sets` (`HallowBiome`, `JungleBiome`, `SandBiome`, `DungeonBiome` weight arrays), `Terraria.ID.TileID` (tile ID → name resolution), `Terraria.ID.WallID` (`BlueDungeonUnsafe`=7 and related), `Terraria.Main` (`UnderworldLayer`, `wallDungeon` initializer, `worldSurface`/`rockLayer` field declarations), `Terraria.WorldGen` (confirms `GenVars.worldSurface = 0.0` is the only Main-adjacent default before a Reset pass runs), `Terraria.Tile` (`WallType` property confirmed)
- `Libs/CalamityMod.dll` (decompiled with `ilspycmd`) — `CalamityMod.CalPlayer.CalamityPlayer` (`ZoneAstral`, `ZoneSulphur`, `ZoneAbyss` property bodies), `CalamityMod.BiomeManagers.AstralInfectionBiome`/`SulphurousSeaBiome` (`IsBiomeActive` full bodies), `CalamityMod.Systems.BiomeTileCounterSystem` (`TileCountsAvailable` full body, `AstralTiles`/`SulphurTiles` computation), `CalamityMod.World.AstralBiome` (world-gen spawn-eligibility, read but not load-bearing for this phase's Zone-flag question)
- `Libs/SubworldLibrary.dll` (decompiled with `ilspycmd`) — `SubworldLibrary.SubworldSystem.LoadSubworld()` (full body: `Main.worldSurface`/`rockLayer` assignment sequence, confirms these run BEFORE the subworld's own `Tasks`), `SubworldLibrary.Subworld` base class (confirms `: ModType, ICopyWorldData, ILoadable, ...` and the `Register()`/`SetupContent()` lifecycle — the source of Pitfall 4's finding)
- `ModReader/SpiritMod/Biomes/BriarSurfaceBiome.cs`, `BriarUndergroundBiome.cs`, `BiomeTileCounts.cs` (already-decompiled local source, read directly) — `IsBiomeActive` bodies, `briarCount`/`InBriar` threshold (>80)
- `ModReader/SpiritMod/NPCs/Boss/ReachBoss/ReachBoss1.cs`, `NPCs/Boss/AncientFlyer.cs`, `NPCs/Boss/SteamRaider/SteamRaiderHead.cs`, `NPCs/Boss/Scarabeus/Scarabeus.cs` (already-decompiled local source, read directly) — confirmed no AI-level `Zone*` dependency for Vinewrath Bane/Ancient Avian/Starplate Voyager (wiki-thematic assignment only), confirmed genuine `ZoneDesert` damage-scaling dependency for Scarabeus
- This project's own `Subworlds/BossArenaCorruptionSubworld.cs`, `CorruptionPlatformPass.cs`, `Systems/BossArenaRoutingRegistry.cs`, `Tiles/Test1Tile.cs`, `Systems/BossSummonPlayer.cs`, `Integrations/CalamityIntegration.cs`, `Integrations/SpiritIntegration.cs`, `.planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md` — confirmed existing template shape, routing mechanism, and the prior debug session's `ZoneCorrupt`/scan-window findings this research extends

### Secondary (MEDIUM confidence)
- `LastLaunchedMods.txt`, `Mods/enabled.json`, global `ModReader/CatalystMod/extract.log` (project-local/tModLoader-authored files) — confirms CatalystMod 1.1.8 was last-played but its DLL is not locally extractable this pass (hidden-code flag + not currently subscribed)

### Tertiary (LOW confidence)
- None — every claim in this document traces to directly-read decompiled source or already-decompiled local source, per this project's established "trust the installed binary, not training data" discipline.

## Metadata

**Confidence breakdown:**
- Standard stack (no new build.txt/csproj changes needed): HIGH — verified current `build.txt`/`.csproj` state directly, both mods already weak-referenced
- Architecture (per-biome Zone-flag formulas, all 9): HIGH — every threshold and tile-weight table decompiled directly this pass, none inherited from training-data assumptions about vanilla Terraria mechanics
- Pitfall 4 (Subworld autoload/JIT-safety): HIGH — `SubworldLibrary.Subworld`'s `ModType`/`ILoadable` inheritance and `Register()`/`SetupContent()` bodies read directly; the safety conclusion (lazy `Tasks` getter is the correct JIT boundary) follows directly from `LoadSubworld()`'s own decompiled call sequence, not inference
- Scarabeus/Ancient Avian/Starplate Voyager/Vinewrath Bane AI-dependency classification: HIGH — full relevant source files read directly, not just grepped for keywords
- CatalystMod/Astrageldon open question: LOW/unresolved — explicitly flagged as non-blocking for this phase, DLL genuinely unavailable locally

**Research date:** 2026-08-14
**Valid until:** Re-verify if CalamityMod updates past 2.2.4 or SpiritMod past 1.5.0.44 (both large, actively-developed mods; `BiomeTileCounterSystem`/`AstralInfectionBiome`/`SulphurousSeaBiome` are exactly the kind of internal system that could be refactored between versions, same caution already established in `04-RESEARCH.md`/`05-RESEARCH.md`). Vanilla tModLoader's `Player.UpdateBiomes()`/`SceneMetrics`/`TileID.Sets` portions are stable (30+ days, unlikely to change within a 1.4.4.x patch branch).

---
*Phase: 09-biome-dependent-subworld-coverage*
*Researched: 2026-08-14*
