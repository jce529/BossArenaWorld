# Feature Research

**Domain:** Terraria (tModLoader) boss-arena subworld visual/layout design
**Researched:** 2026-08-15
**Confidence:** MEDIUM-HIGH (vanilla mechanics verified against official wiki.gg; project-specific constraints verified directly against this repo's existing `Subworlds/*PlatformPass.cs` decompile-sourced comments; a few wall/spawn-mechanic specifics are MEDIUM/LOW and flagged)

## Important Correction Surfaced by This Research

The milestone's own feature list pairs "torch lighting" with the goal of a safer/more pleasant arena, which invites the common assumption that light level suppresses monster spawns (true in some other survival games). **This is false in Terraria.** Per the official wiki: *"Light does not prevent Monsters from spawning, nor does it reduce their spawn rate."* Torches are a pure-visibility feature. The actual vanilla mechanism that blocks enemy spawning is a **safe background wall** (a wall of a type flagged safe, generally the crafted/"house-safe" variant of a wall, as opposed to the naturally-generated/biome-spread variant of the same-looking wall) placed directly behind the tile in question — see `Background walls` and `NPC spawning` sources below. This matters concretely for this project because the 7 biome-variant arenas are filled with real biome-qualifying tiles (Ebonstone, Pearlstone, JungleGrass, Sand/Sandstone, AstralStone, BriarGrass) purely to satisfy each boss's per-tick Zone/Biome flag — which means vanilla/biome-native enemies (corrupt slimes, hallow enemies, antlions, etc.) can currently spawn on these arena floors, since nothing in the existing `*PlatformPass.cs` code places any wall at all. Torches alone will not fix that; if unwanted-spawn suppression is ever wanted, it requires safe walls (or an explicit spawn-rate-reducing item/buff, out of this milestone's scope). This is noted below as a Differentiator, not folded silently into the Table Stakes torch feature, so the roadmap can decide deliberately whether to address it.

## Feature Landscape

### Table Stakes (Users/Players Expect These)

Baseline conventions any "properly built" Terraria boss arena has, per the official Arena guide and this project's own existing biome-flag engineering constraints.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Fall/void boundary (solid floor edges + bottom containment) | Vanilla arena guides consistently warn about building "too high" or near open drops; official guide flags falling out of a biome (which despawns Corruption/Crimson-gated bosses) and falling into open space as real failure modes, not just cosmetic concerns | LOW-MEDIUM | This project's world is a fixed 800-tile-tall void (`WorldHeight = 800` in every `Subworld` subclass) with only a thin 10-20-tile platform slab placed by each `*PlatformPass`. Everything above/below that slab is currently open void — a missed jump or knockback currently means an uninterrupted fall toward `y=0` or `y=800`. |
| Y-range boundary blocks tied to each biome's real Zone-flag window | Not just player comfort — for the 6 biome-gated arenas, going too far in Y can silently exit the exact Y-window the arena was engineered around (`ZoneSkyHeight`: `y <= 84`; `ZoneUnderworldHeight`: `y > 600`), which is the same failure class that caused the documented Hive Mind despawn bug (see `.planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md`) | MEDIUM | Space (`SpacePlatformPass`, surface at y=50) and Underworld (`UnderworldPlatformPass`, surface at y=650) are the two arenas where this is safety-critical, not cosmetic: a boss whose AI target-validity check reads the Zone flag (like Hive Mind's `ZoneCorrupt` check) would silently despawn again if the player/boss pair drifts outside the qualifying Y band. Boundary blocks must sit fully *inside* the qualifying window, not just "somewhere reasonable." |
| Multi-tier stacked platforms (2-4 layers) with vanilla jump-height spacing | Official Arena guide + Moon Lord strategy guide converge on this: rows of platforms spaced ~6-12 tiles apart vertically (6 tiles = base jump height), Moon Lord guide specifically recommends "at least 4 layers" for maximum mobility | MEDIUM | Every existing `*PlatformPass` places exactly **one** flat slab. All 8 arenas are currently single-tier. Multi-tier redesign must preserve each biome's weighted Zone-flag tile math (see Dependencies below) — this is the single biggest source of accidental regression risk in this milestone. |
| Basic torch/light placement for player visibility | Universal vanilla arena-guide convention ("As the fight will be at night, add Torches to improve visibility") — boss fights are frequently timed at night or in inherently dark biomes (Underworld, Space, Astral) where the player otherwise cannot see attack telegraphs | LOW | Pure visual/QoL feature, **does not** suppress spawns (see correction above) — no interaction with the biome-flag or spawn-mechanics constraints, safe to implement independently of everything else. |
| Biome-legible decorative theming without displacing Zone-qualifying tiles | This is the milestone's central ask ("바이옴별 테마에 맞는 시각 디자인") and matches how vanilla/mod-native biome arenas read at a glance (Corruption: chasms + purple grass; Hallow: crystal/rainbow tone; Underworld: ash/obsidian/lava glow; Jungle: mud+vines+hive; Desert: dunes+sandstone+cacti; Astral/Briar: mod-native palettes already defined by Calamity/Spirit's own tile art) | MEDIUM-HIGH | Constrained by real per-tick weighted-tile thresholds baked into each existing Pass (documented precisely in each file's header comment): Corruption 300, Hallow 125, Jungle 140, Desert 1500, Astral 950 (Calamity `BiomeTileCounterSystem`), Briar 80 (Spirit `BiomeTileCounts`). Decoration must be additive to, or drawn from, the qualifying tile ID sets — not a wholesale swap to pure-cosmetic non-weighted tiles, or the exact despawn-class bug already fixed once (Hive Mind/Corruption) can reappear on a different boss. |
| Preserve SubworldLibrary's existing return-to-main-world flow untouched | SubworldLibrary already ships the only entry/exit mechanism this project uses; per `PROJECT.md` current state, the only existing convenience is its built-in "Return" affordance | LOW | Every existing `Subworld` subclass's `OnEnter()`/`OnExit()` also carries the load-bearing vanilla-downed-flag snapshot/restore guard (see `BossArenaSubworld.cs` lines 15-33) that works around a confirmed SubworldLibrary sync bug. Any entry/exit UX work this milestone (portal placement, prep buffer) must not remove, reorder, or bypass those calls. |

### Differentiators (Beyond Bare Requirement)

Not strictly required by the milestone's literal feature list, but grounded in real Terraria mechanics and directly addressable at low incremental cost given work already being done for Table Stakes items above.

| Feature | Value Proposition | Complexity | Notes |
|---------|--------------------|------------|-------|
| Safe-wall-based unwanted-spawn suppression, layered onto the fall-prevention boundary walls | Turns a wall that has to be placed anyway (fall prevention) into a dual-purpose fix for the "biome-native enemies can currently spawn on these floors" gap surfaced above, without adding a separate feature/item | MEDIUM | Per the official Background Walls page: wall safety is generally described as "player-placed vs. naturally-generated," which is a simplification of an underlying per-wall-type safety flag; whether a `GenPass`-placed (i.e., programmatic, not literal player click) wall of a "safe" `WallID` is treated identically to a player-placed one is **not confirmed by this research** (LOW confidence on that specific mechanic) — flag for phase-level implementation research/live testing before relying on it. |
| "Duck under" solid corner blocks at the top of the highest platform tier, mirroring the vanilla Moon Lord-arena convention | Moon Lord's own strategy guide calls out "top left/right solid blocks... to duck under [the] beam attack, which is the most damaging attack it has" — the same shape (telegraphed horizontal beam/laser you dodge by getting under cover) recurs across many of this project's registered large-content-mod bosses (Calamity/Spirit bosses with laser or screen-wide projectile phases) | LOW-MEDIUM | Cheap to add once multi-tier platforms exist — it's one extra solid tile pair at each tier's outer edges, not a new system. |
| Space/Underworld arenas get decoration "for free" (no Zone-flag tile-weight constraint) | `SpacePlatformPass`/`UnderworldPlatformPass`'s own header comments confirm neither biome's Zone flag reads tile composition at all (`ZoneSkyHeight`/`ZoneUnderworldHeight` are pure-Y checks) — the `TileID.Stone`/`Ash`/`Hellstone` currently used there is placed "purely so the player has something solid to stand on," not for flag correctness | LOW | These two arenas can receive the most decorative freedom (any cosmetic tile/furniture, no weighted-count bookkeeping) — a good place to front-load visually strong theming early with the least regression risk to the existing flag-correctness work. |
| Short, deliberate "prep beat" between portal entry and boss auto-summon (a few seconds standing in the arena before the held summon item's use-effect replays) | Distinguishes a "you were just yanked into a fight" feeling from "you have a breath to get oriented" — common pattern in dedicated boss-rush tooling (e.g. the tModLoader Boss Rush API's teleport-then-fight sequencing) even though most of those tools default to zero prep time for true rush events | LOW | This project's summon flow already auto-replays the held item's use-effect on entry (per `PROJECT.md` Validated requirements) — inserting a short deliberate delay is a scope decision, not a technical blocker; keep it brief since the project's own convention (per `PROJECT.md`) is that entry is already a deliberate player action, not ambush-style. |

### Anti-Features (Explicitly Out of Scope for This Milestone)

Real, well-established Terraria arena conventions that would be reasonable to want, but are explicitly excluded from *this* milestone by `PROJECT.md`'s stated scope — included here so the roadmap doesn't accidentally reintroduce them.

| Feature | Why It's a Common Ask | Why Excluded Here | Alternative |
|---------|------------------------|--------------------|--------------|
| Buff stations (Campfire / Heart Lantern / Honey pool) | Standard vanilla arena convention — official guide recommends placing them "every 40-50 blocks" for passive regen during boss fights | `PROJECT.md` explicitly excludes "리소스/버프 지점" (resource/buff points) from this milestone's scope | Defer to a future milestone; note the vanilla placement convention (Campfire ~340-tile effective spacing before overlap waste, "every other platform tier") for when it is picked up |
| New biome-variant arenas (Dungeon, Sulphurous Sea) | Would round out full biome coverage (Polterghast/Old Duke candidates per `PROJECT.md` Out of Scope) | `PROJECT.md` explicitly excludes new biome variants from this milestone — scope is retrofitting the existing 8 | Already tracked as a v1.1+ candidate in `PROJECT.md`'s Out of Scope section |
| In-game UI notifications (e.g. "entering arena," "boss defeated" banners) | Common boss-rush/arena-mod UX polish | `PROJECT.md` explicitly excludes UI notification features from this milestone | Defer; SubworldLibrary's existing loading-UI hook (`loadingUIState`) is the extension point if picked up later |
| Dense decorative clutter (banners, critter spawners, statues, painting-heavy set dressing) | Tempting once biome theming work starts — "more decoration = more thematic" | Directly conflicts with the research question's own framing ("without being visually noisy") and with fight-readability during a boss encounter; some decorative "furniture" tiles are also non-solid and easy to confuse with real Zone-qualifying tiles during future maintenance | Keep decoration restricted to background-layer tiles/walls and a small number of readable foreground landmarks (e.g. one or two biome-signature structures), not dense clutter |
| A brand-new custom teleporter/portal network for exit, parallel to SubworldLibrary's built-in return mechanism | Seems like an obvious "polish" addition alongside entry-portal work already being done | Would duplicate an already-working, already-safeguarded exit path (the vanilla-downed-flag snapshot/restore guard in every `Subworld.OnExit()` is tightly coupled to SubworldLibrary's own exit call order — see `BossArenaSubworld.cs` lines 15-33) — building a second exit path risks bypassing that guard | Keep SubworldLibrary's built-in return as the only exit path; only add a clearly-signposted **return point/marker** near spawn (visual placement convenience), not a new mechanism |
| Genuine spawn-rate-reduction items (Peace Candle-equivalent) as the fix for "unwanted biome-native enemy spawns" | Sounds like the "correct" vanilla-idiomatic fix once the torches-don't-block-spawns correction (above) is understood | Out of this milestone's stated scope (no new items/resources); also weaker than the structural safe-wall fix since it requires the player to be carrying/using an item rather than the arena being safe by construction | Prefer safe background walls (structural, Table Stakes/Differentiator above) over an item-based fix if this gap is addressed at all this milestone |

## Feature Dependencies

```
Multi-tier platforms
    └──requires──> Existing biome Zone-flag tile-weight budget preserved per tier
                       (Corruption >=300 / Hallow >=125 / Jungle >=140 / Desert >=1500 /
                        Astral >=950 / Briar >=80 weighted tiles within the ~200x140 scan window)

Biome-themed decoration
    └──requires──> Same Zone-flag tile-weight budget as above (decoration tiles must be
                       additive to, or drawn from, each biome's qualifying TileID/weight set)

Y-boundary containment blocks
    └──requires──> Correct placement strictly inside each biome's real Zone-flag Y-window
                       (Space: y <= 84 hard boundary; Underworld: y > 600 hard boundary;
                        all others: no Y constraint from the flag itself)

Fall/void boundary walls
    └──enhances──> Y-boundary containment (bottom/side containment is a special case of
                       the same "don't let the player leave the qualifying zone" problem)

Safe-wall spawn suppression (Differentiator)
    └──requires──> Fall/void boundary walls already being built (reuses the same wall
                       placement work, if the safe-wall mechanic is confirmed to apply to
                       GenPass-placed walls the same way as player-placed ones)

Torch/lighting placement
    ──independent of──> everything else (confirmed: no interaction with spawn mechanics
                       or Zone-flag tile math; safe to implement first/in isolation)

Entry/exit convenience (prep beat, return-point marker)
    └──must not disturb──> existing OnEnter()/OnExit() vanilla-downed-flag snapshot/restore
                       guard already present in every Subworld subclass
```

### Dependency Notes

- **Multi-tier platforms and biome decoration both require preserving each biome's tile-weight budget:** this is the single most important cross-cutting constraint for this milestone. Every existing `*PlatformPass.cs` file documents its exact threshold and margin in a header comment (e.g. Desert's 1500-weight threshold cleared with only ~2x margin at 20-tile thickness, hence the deliberate `thickness=20` choice and the documented gravity-tile stack-overflow bug already fixed once for Sand). Any redesign that thins, splits, or partially replaces these slabs with cosmetic-only tiles risks silently dropping back under threshold and reintroducing a Hive-Mind-class despawn bug, but for a different boss.
- **Y-boundary containment is safety-critical, not just convenience, for Space and Underworld:** unlike the other 5 biome-tile-weighted arenas (where drifting off the platform doesn't change the Zone flag, since the flag reads a tile-count scan window, not player Y), Space and Underworld's flags are pure-Y checks. A player/boss pair that drifts outside `y <= 84` or `y > 600` immediately loses the qualifying flag mid-fight — structurally identical to the already-documented Hive Mind bug, just triggered by vertical drift instead of missing tiles.
- **Fall/void boundary walls enhance Y-boundary containment:** they're the same underlying problem (don't let the player/knockback physics leave the zone that makes the arena valid) approached from "block the player" (walls) vs. "keep the flag-valid window generous enough that accidental drift doesn't matter" (Y-margin sizing) angles — likely worth solving together in the same pass rather than as two unrelated tickets.
- **Torch lighting is the one fully independent Table Stakes item:** confirmed no mechanical interaction with anything else in this list, so it carries zero sequencing risk and can be scheduled in any phase, including first.
- **Safe-wall spawn suppression should not block the Table Stakes fall-prevention wall work:** even if the "does a GenPass-placed wall behave as safe" question resolves negatively (LOW confidence, needs live verification), the fall-prevention walls still need to exist for the Table Stakes containment purpose — spawn suppression is a bonus outcome to verify opportunistically, not a blocking dependency.

## MVP Definition (This Milestone, v1.1)

### Launch With (v1.1 — matches `PROJECT.md`'s stated target features)

- [ ] Fall/void + Y-range boundary blocks for all 8 arenas — prevents both a comfort problem (falling forever) and, for Space/Underworld specifically, a correctness problem (Zone-flag drift)
- [ ] Multi-tier platform layout per arena, sized to accommodate ground/flying/area-denial boss patterns already registered against each biome — table stakes per vanilla Arena/Moon Lord guide convention
- [ ] Regularly-spaced torches for visibility — cheapest, lowest-risk item, no dependency chain
- [ ] Biome-legible decorative theming that respects each arena's existing Zone-flag tile-weight budget — the milestone's headline ask
- [ ] Entry/exit convenience (return-point marker near arena spawn, brief prep beat) — without disturbing the existing SubworldLibrary return flow or the vanilla-downed-flag snapshot/restore guard

### Add After Validation (v1.2+)

- [ ] Safe-wall-based spawn suppression, once live-verified that GenPass-placed walls inherit "safe" status the same way player-placed ones do
- [ ] "Duck under" corner-block convention at each arena's top tier, once multi-tier layout is in place

### Future Consideration (Explicitly Deferred — see Anti-Features)

- [ ] Buff stations (Campfire/Heart Lantern/Honey) — `PROJECT.md` Out of Scope
- [ ] New biome variants (Dungeon, Sulphurous Sea) — `PROJECT.md` Out of Scope
- [ ] UI notifications — `PROJECT.md` Out of Scope

## Feature Prioritization Matrix

| Feature | Player Value | Implementation Cost | Priority |
|---------|---------------|----------------------|----------|
| Fall/void + Y-range boundary blocks | HIGH | MEDIUM | P1 |
| Multi-tier platforms | HIGH | MEDIUM | P1 |
| Torch lighting | MEDIUM | LOW | P1 |
| Biome-legible decoration (within tile-weight budget) | HIGH | HIGH | P1 |
| Entry/exit convenience (return marker + prep beat) | MEDIUM | LOW | P1 |
| Safe-wall spawn suppression | MEDIUM | MEDIUM (LOW-confidence mechanic, needs live verification) | P2 |
| Duck-under corner blocks | LOW-MEDIUM | LOW | P2 |
| Buff stations / new biomes / UI notifications | — | — | Out of scope this milestone (P3/deferred) |

## Competitor / Reference-Convention Analysis

"Competitors" here are (a) vanilla Terraria's own experienced-player arena-building convention, and (b) the actual native arenas the source content mods (Calamity, Spirit) designed their bosses around — since this project's whole premise is reproducing those bosses faithfully outside their home world.

| Concern | Vanilla convention | Calamity/Spirit's own native biome arenas | This project's approach |
|---------|---------------------|---------------------------------------------|---------------------------|
| Biome legibility | Real biome tiles (Corruption/Hallow/etc.), not paint/color tricks | Same — Calamity's Astral biome and Spirit's Briar biome are real tile-weighted `ModBiome`s with their own signature tile art | Already matches structurally (real tile-weighted biomes, not faked); this milestone adds the decorative layer on top without disturbing the weight math |
| Platform tiers | 2-4 rows, jump-height spacing (~6-12 tiles), extra tiers for flying/laser bosses (Moon Lord: "at least 4 layers") | Individual bosses' AI (laser sweeps, area-denial explosions) implicitly assume the player has vertical room to reposition — most large-mod boss arenas players build by hand already default to multi-tier for exactly this reason | Currently single-tier for all 8 arenas (biggest gap vs. both references) — this milestone's core structural fix |
| Spawn control | Safe walls (not light) block unwanted spawns; Peace Candle/Sunflower items reduce spawn *rate* | N/A — most mod bosses fight in the player's already-built arena, so this is on the player, not the mod | Currently absent (walls); explicit correction folded in as a Differentiator, not silently assumed via torches |
| Boundary/fall safety | Player-built walls/floors sized to the specific boss fight | N/A (player's own arena) | Currently absent (open void above/below every platform slab) — this milestone's other core structural fix |

## Sources

- https://terraria.wiki.gg/wiki/Guide:Arena — HIGH confidence, official wiki: platform spacing, arena width conventions, biome-boundary despawn warnings
- https://terraria.wiki.gg/wiki/NPC_spawning — HIGH confidence, official wiki: solid-tile/safe-wall spawn validity checks, 2x3 spawn-space requirement, actuated-block exception
- https://terraria.wiki.gg/wiki/Background_walls — HIGH confidence for the safe/unsafe distinction existing; MEDIUM confidence on the precise "player-placed vs. natural" framing as it applies to programmatic (GenPass) wall placement — flagged as needing live verification
- https://terraria.fandom.com/wiki/Guide:Moon_Lord_strategies (via WebSearch synthesis) — MEDIUM confidence: 4+ layer arena convention, top-corner duck-under-beam convention, "avoid building near Space" Y-drift warning
- https://terraria.fandom.com/wiki/Torches / general WebSearch on light-level spawn mechanics — HIGH confidence (converges with NPC_spawning page): light level does not affect spawn rate; Peace Candle is the actual spawn-rate-reducing item
- https://terraria.fandom.com/wiki/Heart_Lantern / https://terraria.wiki.gg/wiki/Campfires — MEDIUM confidence: buff-station spacing convention (documented for completeness/future milestone, explicitly out of scope now)
- https://github.com/tieeeeen1994/tModLoader-BossRush — MEDIUM confidence: reference for boss-rush-style teleport/prep sequencing conventions in the tModLoader ecosystem
- This repo's own `Subworlds/*PlatformPass.cs` files (`FlatStonePlatformPass.cs`, `CorruptionPlatformPass.cs`, `SpacePlatformPass.cs`, `UnderworldPlatformPass.cs`, `JunglePlatformPass.cs`, `DesertPlatformPass.cs`, `HallowPlatformPass.cs`, `AstralPlatformPass.cs`, `BriarPlatformPass.cs`) and `Subworlds/BossArenaSubworld.cs`/`BossArenaCorruptionSubworld.cs` — HIGH confidence, primary source: exact Zone-flag thresholds, Y-windows, and existing OnEnter/OnExit safeguards documented directly in decompile-sourced code comments already in this codebase
- `.planning/PROJECT.md` and `.planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md` — HIGH confidence, primary source: milestone scope boundaries and the precedent despawn bug this research repeatedly cross-references

---
*Feature research for: Terraria tModLoader boss-arena subworld visual/layout design*
*Researched: 2026-08-15*
