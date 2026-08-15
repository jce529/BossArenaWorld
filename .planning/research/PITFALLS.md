# Pitfalls Research

**Domain:** Arena subworld visual/layout design improvements (biome-themed decoration, multi-tier platforms, boundary/containment walls, automatic torch placement, entry/exit convenience) retrofitted onto BossArenaSubWorld's existing 9-arena GenPass pipeline (1 plain + Corruption/Hallow/Underworld/Jungle/Space/Desert/Astral/Briar biome variants)
**Researched:** 2026-08-15
**Confidence:** HIGH for codebase-specific findings (read directly from `Subworlds/*.cs`, `Systems/*.cs`, `Tiles/Test1Tile.cs`, `.planning/PROJECT.md`); MEDIUM for general vanilla-mechanic claims (Terraria Wiki, verified against two independent fetches); LOW-flagged explicitly where a claim could not be verified against source

## Critical Pitfalls

### Pitfall 1: Falling-tile stack overflow (the Desert bug) resurfaces in a NEW location the one-off fix didn't touch

**What goes wrong:**
A 10000-wide strip of any `TileID.Sets.Falling`-flagged tile (vanilla: Sand, Silt, Slush, Ash-family cousins, Gravel; modded: Calamity's `AstralSand` almost certainly registers as falling too, unverified) placed without full solid support underneath blows the native call stack via `WorldGen.SquareTileFrame → WorldGen.TileFrame → WorldGen.SpawnFallingBlockProjectile` mutual recursion. This already happened once (Phase 9, Desert arena) and was fixed *locally* — only `DesertPlatformPass.cs`'s base platform now keeps falling tiles to a 3-row cosmetic cap over solid `Sandstone`. That fix is not systemic: it lives in one file and covers one situation (the base platform fill). Nothing in the codebase prevents the same class of bug from being reintroduced by:
- Decorative "themed dressing" added on TOP of the Desert platform (e.g. loose sand dunes, gravel piles) that aren't capped the same way.
- Multi-tier platforms in the Astral arena if `AstralSand`/`HardenedAstralSand` (both counted in Calamity's `BiomeTileCounterSystem`, see `AstralPlatformPass.cs`) are used decoratively as a floating tier without solid backing.
- Any new floating torch/boundary "shelf" tile made of a falling-tile family for visual variety in ANY of the 9 arenas, not just Desert.

**Why it happens:**
The recursion trigger isn't necessarily immediate at placement time — it fires whenever `SquareTileFrame`/`TileFrame` next walks that tile (world-gen finalization, chunk load/sync, or a later pass). Because `BossArenaSubworld`-family `Tasks` lists contain only this mod's own single GenPass (no vanilla "Settle Liquids"/framing pass appended), the trigger point is easy to misjudge, and a developer adding a second decoration/torch/platform `GenPass` after the base platform pass can unknowingly place unsupported falling tiles that only crash later, making the bug hard to attribute to the new code during a quick manual test.

**How to avoid:**
- Before using ANY tile type in a new decoration/platform/torch/boundary pass, check `TileID.Sets.Falling[type]` (vanilla) or decompile-verify the modded tile's `TileObjectData`/`Main.tileFrameImportant`+gravity registration (Calamity/Spirit types) — do this per tile type, not per biome, since each biome variant can independently reintroduce the bug.
- Reuse the Desert fix's exact pattern for any falling-tile decoration: thin cosmetic top layer (1-3 rows) sitting directly on a solid, non-falling tile of the same or a zero/positive-weight biome tile — never a falling tile with open air or another falling tile beneath it, at ANY tier, including floating upper platforms.
- For multi-tier platforms specifically: every platform tier needs its own solid, non-falling tile column for its full width, not just the base strip — a "floating dune" tier is the exact shape of the original bug.
- Test each new pass by loading the arena and checking `Natives.log` / client log for "Stack overflow" near `SquareTileFrame`/`TileFrame`/`SpawnFallingBlockProjectile`, exactly how the original bug was diagnosed — don't just eyeball the tile placement.

**Warning signs:**
- Client crash/hang specifically on arena entry (not on mod load) after adding a new decoration/platform/torch pass to any arena's `Tasks` list.
- Natives.log or dotnet crash dump showing recursive frames through `WorldGen.SquareTileFrame`/`TileFrame`/`SpawnFallingBlockProjectile`.

**Phase to address:**
The phase that builds the shared multi-tier-platform / decoration-placement helper (early, shared-layer phase) — bake a "falling-tile safety" check or convention directly into the helper's API (e.g. the helper only accepts tile types from an explicit non-falling allowlist, or always places a solid backing row automatically) so every later per-biome retrofit phase inherits the protection instead of re-deriving it per biome.

---

### Pitfall 2: Torches are assumed to suppress monster spawns; they don't — the real mechanism is background walls, and this arena currently has zero spawn suppression at all

**What goes wrong:**
The milestone's stated goal ("횃불 배치로 광원 확보") risks an implicit assumption that adding light sources reduces or gates unwanted monster spawns inside the arena. Verified against the Terraria Wiki's NPC spawning mechanics: light level is **not** a documented factor in vanilla spawn-rate/spawn-eligibility calculation. The actual mechanism that suppresses spawns is a **safe background wall** at the player's center tile (`if the tile chosen ... has a safe wall that blocks enemies from spawning, the attempt is considered invalid`), completely independent of illumination. Torches placed for visual polish will do nothing to prevent ambient spawns.

This compounds an existing, currently-latent gap: grepping the codebase confirms there is **no spawn-suppression code anywhere** (`EditSpawnRate`, `EditSpawnRange`, `ModifySpawnPool`, safe-wall placement — none exist). Every biome-tagged arena (Corruption, Hallow, Jungle, Astral, Briar, etc.) places real biome tiles specifically so vanilla's `Player.UpdateBiomes()`/`SceneMetrics` Zone-flag checks pass for boss AI — but those same Zone flags simultaneously feed vanilla's biome-appropriate ambient spawn pool. Nothing has stopped Corruption/Jungle/Astral/Briar ambient enemies from spawning in these arenas since Phase 9; it likely hasn't been noticed because fights are short and the platforms are visually bare.

**Why it happens:** Confusing "torches = light = safety" (true in many games, and a common folk belief about Terraria specifically) with Terraria's actual, wall-based mechanic. Because no prior phase needed to think about ambient spawns (fights were short, platforms bare), this gap was never surfaced.

**How to avoid:**
- Do not treat torch placement as a spawn-prevention feature — scope it purely as illumination/visual polish, and say so explicitly in code comments to prevent a future contributor from assuming otherwise.
- If unwanted ambient spawns are actually a concern (they now become more visible once arenas look "real" rather than bare), the correct fix is either (a) a `ModPlayer.EditSpawnRate` override that sets spawn rate to effectively zero while `BossArenaRoutingRegistry.IsAnyArenaActive()` is true, mirroring the existing `ForcedTimeSystem.PreUpdateWorld` guard pattern already used in this codebase, or (b) placing safe background walls under/around the platform. Torches are unrelated to either.
- If torch placement uses `WorldGen.PlaceTile`/raw `Main.tile` assignment near a biome-gated arena, double check it doesn't also lay a wall type that happens to NOT be in the "safe wall" set, since some walls actively increase spawn chance rather than suppress it.

**Warning signs:**
- Playtesting an arena and seeing biome-appropriate ambient enemies (Corruption's Eater of Souls, Jungle enemies, Astral/Briar equivalents) spawn mid-fight and interfere with a boss encounter.

**Phase to address:**
The torch/lighting phase should explicitly NOT claim to solve spawn suppression. If spawn suppression is in scope at all for this milestone, it belongs in the same shared-layer phase as boundary/containment (both are "safety" features), implemented via `EditSpawnRate`, not via torch density.

---

### Pitfall 3: Boundary/containment walls placed at a single hardcoded Y-range will be wrong for most arenas, because each biome's platform sits at a different absolute Y

**What goes wrong:**
The 9 arenas do NOT share one platform Y-position. Confirmed by reading every `*PlatformPass.cs`:
- Space: `surfaceY = 50` (must stay `<= 84` for `ZoneSkyHeight`)
- Underworld: `surfaceY = 650` (must stay `> 600` for `ZoneUnderworldHeight`, near the bottom of the fixed `WorldHeight = 800`)
- Corruption/Hallow/Jungle/Flat/Desert: `surfaceY = Main.maxTilesY / 2` (400, "mid-height")
- Briar: `surfaceY = 150` (must stay `<= 240` for the Surface-variant's `ZoneOverworldHeight` requirement)
- Astral: `surfaceY = 400` (no height constraint from the biome check itself)

A generic "Y-boundary containment" helper written against one assumed platform position (e.g. "wall off everything more than 300 tiles above/below the mid-height platform") will place the ceiling/floor in the wrong place for Space (would sit deep underground relative to its real y=50 platform) and Underworld (would sit far above the sky relative to its real y=650 platform), either doing nothing useful or actively cutting off usable space near the real platform.

**Why it happens:** The per-biome Y placement is a deliberate, documented design choice (each pass's own Zone-flag math dictates its Y range) but is invisible unless every `*PlatformPass.cs` file is actually read — a shared containment feature built by only looking at one or two arenas (e.g. Flat/Corruption's mid-height convention) will silently miss Space/Underworld/Briar's very different Y placement.

**How to avoid:**
- The Y-boundary helper must take `surfaceY` (and platform thickness) as a parameter supplied by each `*PlatformPass`, not a hardcoded constant — compute wall offsets relative to each pass's own `surfaceY`, not relative to `Main.maxTilesY / 2`.
- When adding this to Space/Underworld/Briar specifically, re-derive the safe vertical margin against that biome's own Zone-flag boundary (e.g. Underworld's ceiling wall must not cross back above y=600 in a way that could put a flying boss above the `UnderworldLayer` threshold and flip `ZoneUnderworldHeight` false mid-fight).

**Warning signs:**
- A boundary wall visibly floats far from the platform, or is invisible/underground, in any arena other than the one it was tested in.
- A biome-gated boss despawns mid-fight after the boundary-wall phase ships, where it didn't before — likely means the wall pushed the boss (or the player, moving the Zone-flag scan window — see Pitfall 4) across the biome's own height/tile threshold.

**Phase to address:**
Shared-layer phase (the containment-wall helper itself) — parameterize on `surfaceY`/thickness from day one. Verification: manually enter all 9 arenas after the change and confirm the boundary sits at a sane distance from that specific arena's real platform, not just the plain/Corruption ones.

---

### Pitfall 4: Multi-tier platforms can move the PLAYER far enough from the base biome strip to flip a Zone flag false mid-fight, even though the boss never left

**What goes wrong:**
Every biome-detection Zone flag in this project (`ZoneCorrupt`, `ZoneHallow`, `ZoneJungle`, Calamity's Astral `IsBiomeActive`, Spirit's Briar `IsBiomeActive`) is recomputed **every tick** from a tile-count scan window **centered on the player**, not the boss (confirmed in `CorruptionPlatformPass.cs`'s decompiled-source comment: "buffScanAreaWidth x buffScanAreaHeight (~200 x ~140 tiles) centered on the player, every tick"). The existing platforms are deliberately filled full-width specifically so the player can drift anywhere along the platform without the scan window losing biome-tile density. A multi-tier platform adds a NEW axis of drift: if a player climbs to a raised platform tier built from a different (non-biome, e.g. plain Wood Platforms or Stone) material for traversal/visual variety, the scan window recentres around the player's new position and may no longer contain enough biome-weighted tiles from the base strip below, especially for biomes with high thresholds (Desert's 1500, this project's highest) or narrow tile-weight margins (Jungle: only `JungleGrass`/`JunglePlants`/`Hive`/`LihzahrdBrick` count, `Mud` counts zero).

If that flag flips false while the player is on an upper tier mid-fight, a biome-gated boss (Hive Mind is the documented precedent — see `.planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md`) can despawn instantly, even though nothing else about the encounter looks wrong to the player.

**Why it happens:** Multi-tier platforms are being requested specifically to give bosses "공격 패턴에 맞는" vertical room, which is exactly the kind of player movement (following the boss up/down) the original single-strip design was built to make irrelevant. Extending the arena vertically without also extending the biome-tile fill vertically reintroduces the exact despawn-class bug Phase 9 fixed for horizontal drift, but on the Y axis this time.

**How to avoid:**
- Any new platform tier added within roughly the scan window's height (~140 tiles) of the base strip should either (a) be built from the same biome tile as the base strip, or (b) be verified per-biome that the vertical scan-window overlap still clears the threshold from the base strip alone (do the math per biome — Desert's 1500 threshold has the least margin, per `DesertPlatformPass.cs`'s own comment about needing 2x margin vs. Corruption's ~10x).
- Prefer: if a tier is purely cosmetic/non-biome, keep it below ~1 scan-window-height of vertical distance from the base strip's Y so the window still substantially overlaps it, OR duplicate a thin biome-tile band on that tier too.

**Warning signs:**
- A boss (registered against a biome-gated arena) despawns or its AI behaves as if in the wrong biome specifically when the player is standing on an upper/lower platform tier, but not on the base strip.

**Phase to address:**
Multi-tier platform phase, but must be informed by the Zone-flag research already embedded in each `*PlatformPass.cs` file — do this per-biome, not as one generic pass, since thresholds and tile-weight margins differ per biome (Desert and Jungle are the tightest-margin cases and should be tested first/most carefully).

---

### Pitfall 5: A "shared arena polish" helper that references modded tile/wall types directly breaks the codebase's established JIT-safety discipline

**What goes wrong:**
Only 2 of the 9 arenas actually need `[JITWhenModsEnabled]` guards: `AstralPlatformPass.cs` (CalamityMod types: `AstralStone`, `AstralGrass`) and `BriarPlatformPass.cs` (SpiritMod types: `BriarGrass`) — confirmed by reading all 9 pass files; Flat/Corruption/Hallow/Underworld/Jungle/Space/Desert use only vanilla `TileID` and carry no guard. If a new shared "decoration/torch/boundary" helper class is written to be reusable across all 9 arenas and its **method signature** directly references a modded type (e.g. an overload typed `void PlaceDecor(ModTile astralOrBriarTile, ...)` that's called with `AstralGrass`/`BriarGrass` instances, or a shared class that itself imports `CalamityMod.Tiles.Astral` at the top for convenience), that shared class becomes JIT-unsafe for players without CalamityMod or SpiritMod installed — and a single `[JITWhenModsEnabled("X")]` tag cannot cover both mods at once, since the same helper is meant to be called from both `AstralPlatformPass` and `BriarPlatformPass`.

This project already learned (documented in `PROJECT.md`'s Key Decisions table, live-confirmed Phase 4, commit `0e19600`) that inline lambdas passed into JIT-guarded methods compile into a compiler-generated `<>c` class that does NOT inherit the enclosing method's `[JITWhenModsEnabled]` attribute and still throws a real `JITException` when the mod is disabled. The same class of trap applies to any new shared helper: passing a modded-type argument, or defining an inline delegate that closes over a modded type, from inside `AstralPlatformPass.ApplyPass` (correctly tagged) into an UNTAGGED shared helper is enough to reintroduce the bug, even though the call SITE is properly guarded.

**Why it happens:** "Share code across all 9 arenas" and "keep 2 of those 9 arenas' modded types isolated" are in tension, and it's easy to reach for a shared helper's convenience (generic tile-decoration function) without re-deriving the isolation discipline that was hard-won for the original passes.

**How to avoid:**
- The shared helper's public API must be entirely mod-agnostic: parameters should be `ushort`/`int` tile-type IDs (or plain coordinates/dimensions), never a modded `ModTile`/`ModWall` instance or type parameter constrained to a specific mod's namespace.
- Each biome's own `ApplyPass` (which is already correctly, individually JIT-tagged where needed) is responsible for resolving `ModContent.TileType<AstralGrass>()`/`ModContent.TileType<BriarGrass>()` into a plain `ushort` BEFORE calling into the shared helper — the resolution happens inside the guarded method; only the resulting primitive value crosses into the shared, untagged helper.
- Never pass an inline lambda/anonymous method that closes over a modded type from a guarded `ApplyPass` into a shared helper — use a named, separately `[JITWhenModsEnabled]`-tagged method if a callback is unavoidable, exactly per the existing codebase convention.
- Any brand-new per-biome pass class added for this milestone's decoration/torch/platform work (e.g. a dedicated `AstralDecorationPass`) must independently repeat BOTH established protections — non-`ModType` lazy construction AND an explicit `[JITWhenModsEnabled]` tag on every method that touches the modded types — since the codebase's own comment (`AstralPlatformPass.cs`) explicitly warns lazy construction alone was proven insufficient (live JITException, 2026-08-14).

**Warning signs:**
- Mod fails to load, or throws a `JITException` naming the shared helper class, specifically when CalamityMod or SpiritMod (but not both) is disabled — the smoke test this project already uses per mod.

**Phase to address:**
Shared-layer phase, at the point the "arena polish" helper's API is designed — get the primitive-only signature right before any per-biome phase starts calling it, since fixing this after 9 call sites exist is much more expensive.

---

### Pitfall 6: A custom entry/exit teleporter that doesn't call `SubworldSystem.Exit()` (or that sets `noReturn`) breaks the flag-restore guard and/or removes the only working return path

**What goes wrong:**
SubworldLibrary automatically adds a "Return" button to the pause menu for any `Subworld` that does not set `noReturn = true` (confirmed by this project's own `.planning/phases/02-summon-item-redirect-entry-registry/02-03-PLAN.md` verification step). That button's only job is to call `SubworldSystem.Exit()`. Every `BossArenaSubworld`-family class's `OnExit()` override does correctness-critical work: it restores the snapshotted vanilla downed/event flags captured in `OnEnter()`, defeating SubworldLibrary's own `CopyDowned()`/`ReadCopiedDowned()` behavior that would otherwise leak subworld-local flag changes back into the main world (see `.planning/debug/resolved/isolation-premise-flag-persistence.md` — this is the single most load-bearing fix in the whole project).

Two ways the new entry/exit convenience feature (return portal/teleporter object, "준비 시간 확보") can break this:
1. A custom in-arena teleporter tile/NPC that implements its own player-repositioning logic instead of calling `SubworldSystem.Exit()` — this would skip `OnExit()` entirely, silently reintroducing the exact flag-leak bug Phase 1 fixed.
2. Setting `Subworld.noReturn = true` on any arena (e.g. to force players to use only the new custom exit object, for "polish" reasons) removes the existing pause-menu Return button — if the custom object is ever unreachable (player falls off the platform into open space, gets stuck behind a new boundary wall, or the object itself has a placement bug), the player has no fallback exit at all in a still-singleplayer-only, still-in-development mod.

**Why it happens:** The convenience feature naturally invites a bespoke "walk into the portal" interaction, and it's easy to build that as a self-contained teleport (`player.Teleport(...)`) rather than routing it through the same `SubworldSystem.Exit()` primitive the existing Return button already uses correctly.

**How to avoid:**
- Any new exit-trigger object's interaction handler must call `SubworldSystem.Exit()` directly (mirroring `Test1Tile.RightClick`'s existing pattern of calling `BossArenaRoutingRegistry.Enter(...)` for entry) — never hand-roll a teleport/scene-change.
- Do not set `noReturn = true` on any arena subclass as part of this milestone unless the new exit object is proven to always be reachable (including after boundary walls and multi-tier platforms are added) — keep the pause-menu Return button as the guaranteed fallback.
- If "준비 시간 확보" means delaying the boss's auto-summon after entry (not delaying exit), see Pitfall 7 below — don't conflate the two features' failure modes.

**Warning signs:**
- Returning to the main world via the new custom object and finding a boss's downed-status changes (or fails to change) differently than returning via the pause-menu Return button — the two paths should be behaviorally identical since both should resolve to the same `SubworldSystem.Exit()` call.
- Player reports being unable to leave the arena at all in a build that also changed `noReturn`.

**Phase to address:**
Entry/exit convenience phase — implement the exit object as a thin wrapper around `SubworldSystem.Exit()` with no new state, and explicitly do NOT touch `noReturn` unless a later, dedicated verification step confirms the new object is unconditionally reachable in every arena.

---

### Pitfall 7: A "prep time before auto-summon" delay can double-summon the boss or break singleplayer's static-field consume-once guard

**What goes wrong:**
`BossSummonPlayer.OnEnterWorld()` currently spawns the boss immediately, exactly once, by consuming (nulling) the static `PendingBossNpcType` field the instant it's read — this is explicitly documented as "Pitfall 3 guard: prevents re-summon on a later, unrelated subworld entry." If "준비 시간 확보" is implemented as a delay before the boss spawns (e.g. a few seconds of grace period after entering, matching `ForcedTimeSystem`'s existing per-tick `PreUpdateWorld` pattern), the consume-once logic must move from a single `OnEnterWorld` read to a multi-tick countdown — and that countdown state is itself a new static field, subject to the exact same singleplayer-only, never-reset-across-sessions assumptions `PendingBossNpcType`/`ForcedTimeSystem.ActiveArenaBossNpcType` already rely on (explicitly justified in-code as safe only because "this project is singleplayer-only").

Failure modes if this is done carelessly: the boss spawns multiple times if the countdown isn't guarded the same way the original single-read was; the countdown state leaks into a later, unrelated arena visit if not scoped by `BossArenaRoutingRegistry.IsAnyArenaActive()` exactly like `ForcedTimeSystem.PreUpdateWorld` already guards itself; or the delay silently does nothing for `RequiresInfernumToggle`-gated bosses if the `InfernumMode` toggle-forcing call (currently also inside `OnEnterWorld`, gated on `PendingBossNpcType.Value`) isn't moved to fire before the delay elapses rather than after.

**Why it happens:** The existing spawn-trigger code was written and hardened (three separate documented "Pitfall" guards already exist in this ~60-line file) assuming a single, synchronous, one-shot spawn on world-enter. Turning that into a multi-tick delayed action is a structural change to a piece of code that already has non-obvious invariants, not just an additive one.

**How to avoid:**
- Reuse `ForcedTimeSystem`'s existing pattern as the template: a `PreUpdateWorld`-driven tick countdown, explicitly re-checking `BossArenaRoutingRegistry.IsAnyArenaActive()` every tick (not just once), with the boss-type field nulled only after the countdown completes and the boss actually spawns.
- Keep the `RequiresInfernumToggle` force-call and `NPC.SpawnOnPlayer` call adjacent in the same code path (whenever the countdown fires), not split across two different tick-driven systems that could race.
- Add an explicit test: enter the arena, wait through the full prep delay, confirm exactly one boss NPC exists; then separately confirm leaving and re-entering a DIFFERENT arena afterward doesn't spawn a stale/leftover boss from the countdown state.

**Warning signs:**
- Two copies of the boss NPC present after the prep delay elapses.
- A boss spawns unexpectedly on a later, unrelated arena entry that wasn't triggered by `Test1Tile.RightClick` at all.

**Phase to address:**
Entry/exit convenience phase — treat this as a refactor of `BossSummonPlayer`/`ForcedTimeSystem`'s existing invariants, not a bolt-on; review against all three of `BossSummonPlayer.cs`'s existing in-code "Pitfall" comments before merging.

---

### Pitfall 8: Boundary walls sized for the base platform can clip a flying/high-mobility boss's AI-expected vertical range, stalling its phase-transition logic

**What goes wrong:**
The arena world is a fixed `WorldHeight = 800` with the actual platform occupying only a thin 10-20 tile band (varies per biome per Pitfall 3) — the rest is currently wide-open vertical space. Several registered bosses in this project's 17-boss roster have AI that expects large uninterrupted vertical (or, less commonly, horizontal-charge) room: high-mobility/flying-phase bosses (Moon Lord's phases, Empress of Light's daytime dash patterns, Providence's fire-trail phases) and hard-charging ground bosses. If Y-boundary containment is added with a margin sized by eyeballing the base platform rather than by researching each boss's actual AI-driven movement envelope, a boss can end up trying to move to a position that's now inside solid boundary tile — vanilla/modded boss AI generally assumes open world movement and doesn't defensively check for solid tile collision at its intended destination the way player movement does. Depending on the specific AI, this can manifest as the boss visually clipping/getting stuck oscillating against the wall, a phase-transition check (frequently based on `NPC.Center` distance/position thresholds) never firing because the boss can't reach the expected trigger position, or — ironically, given this mod's entire purpose is solving FPS crashes — a tight retry loop in AI code that can't reach its target position every tick, adding CPU overhead.

**Why it happens:** A generic "keep the player from falling forever" containment wall is naturally sized by player-safety margins (how far can a player fall before it's obviously unfair/unsafe), not by researching each boss's own AI vertical-travel design — but this project's boundary walls affect the BOSS's movement space too, not just the player's, since both occupy the same subworld.

**How to avoid:**
- Do not size containment walls purely by "how far can the player safely fall" — cross-check against the vertical range used by each registered boss's actual AI where documented/known (Moon Lord and other high-mobility bosses need generous headroom well beyond what a player-safety-only margin would provide).
- Prefer a large, uniform vertical margin (the existing `WorldHeight = 800` budget is already generous — err toward using most of it rather than aggressively tightening it for cosmetic reasons) over a tight one tuned only against the plain/ground-boss arenas.
- Test boundary-wall changes against at least one confirmed high-mobility boss (not just a ground boss) per arena type before considering the containment feature complete for that arena.

**Warning signs:**
- A boss visibly "vibrates"/gets stuck against a boundary wall, stops attacking, or a fight that used to complete now stalls indefinitely after boundary walls are added to that arena.

**Phase to address:**
Boundary/containment phase (later, per-biome retrofit) — verify per arena against whichever bosses are actually routed there via `BossArenaRoutingRegistry`, not just the plain arena.

---

## Technical Debt Patterns

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|-----------------|------------------|
| Hardcoding one Y-range/margin for containment walls and copy-pasting it across all 9 `*PlatformPass.cs` files (mirroring how the vanilla-downed-flag snapshot fields are already duplicated verbatim per Subworld class) | Fast, matches an existing codebase convention (duplication over inheritance for the OnEnter/OnExit guard) | Wrong for Space/Underworld/Briar (different `surfaceY`) per Pitfall 3 — duplication here duplicates a bug, not just code | Never for the Y-value itself; the duplication-over-inheritance PATTERN is fine, but each copy must use that pass's own `surfaceY`, not a shared literal |
| Reusing `TileID.Torches` (plain vanilla torch) everywhere for the automatic lighting feature instead of per-biome torch variants | One tile type, simplest possible implementation | If the player has unlocked "Torch God's Favor," wrong-biome torches nearby can apply an "Unlucky" debuff (real vanilla mechanic, unverified whether it triggers inside a `ShouldSave = false` throwaway subworld — LOW confidence, worth a quick manual check) | Acceptable for the plain/Flat arena; verify per-biome arenas don't trigger the debuff before shipping, or deliberately pick biome-appropriate torch IDs where cheap to do (vanilla biomes only — Astral/Briar have no vanilla-equivalent torch anyway) |
| Building the multi-tier platform / decoration / torch placement as one big shared `GenPass` reused verbatim across all 9 arenas by parameterizing only tile type | Minimizes new files | Silently reintroduces Pitfall 5 (JIT-unsafe shared code) the moment any parameter is a modded type instead of a primitive, and Pitfall 4 (Zone-flag scan-window drift) if vertical tiers aren't biome-fill-aware | Only acceptable if the shared pass's public surface is strictly primitive-typed (ushort/int) per Pitfall 5's prevention |

## Integration Gotchas

| Integration | Common Mistake | Correct Approach |
|-------------|------------------|-------------------|
| SubworldLibrary's automatic pause-menu Return button (`noReturn` field) | Assuming a new custom exit object should replace/disable the built-in Return button for a "cleaner" UX | Leave `noReturn` false (default) unless the custom object's reachability is proven in every arena/layout state — see Pitfall 6 |
| SubworldLibrary's `CopyDowned()`/`ReadCopiedDowned()` whitelist sync, defeated per-arena by each `Subworld`'s own `OnEnter()`/`OnExit()` snapshot | Adding a NEW `Subworld` subclass (if this milestone adds one, e.g. for a distinct "prep room" before the fight) without duplicating the existing OnEnter/OnExit flag-snapshot guard verbatim | Copy the guard into any new `Subworld` subclass exactly as `BossArenaAstralSubworld.cs` duplicates it from `BossArenaSubworld.cs` — it is per-subclass, not inherited/shared automatically |
| Vanilla `WorldGen.PlaceTile` vs. raw `Main.tile[x,y]` field assignment (every existing pass uses raw assignment, not `PlaceTile`) | Switching a new decoration/torch pass to use `WorldGen.PlaceTile` for convenience (auto-framing, auto-style selection) without checking whether that changes WHEN/whether `SquareTileFrame`/falling-tile checks run relative to the existing raw-assignment convention | Stay consistent with the existing raw-assignment pattern for bulk fills; if `WorldGen.PlaceTile` is used for single frame-important objects (torches genuinely need correct `frameX`/`frameY` for their lit-state sprite, unlike the bulk rectangular fills), test that specific call in isolation for the Desert-bug recursion class before combining with bulk falling-tile fills in the same pass |

## Performance Traps

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|-----------------|
| Placing individual torches (or any frame-important decorative tile) one-by-one via per-tile loops across a 10000-wide platform, once per arena, times 9 arenas | Noticeably longer world-gen/loading time when entering an arena (GenerationProgress bar slower on this pass specifically) | Space torches at a fixed interval (e.g. every N tiles) rather than every tile; batch-place without invoking full tile-frame/lighting recalculation per placement where avoidable | Unlikely to be severe at this world's modest fixed size (`Width=10000, Height=800`), but worth profiling once torches + multi-tier + boundary walls are all combined in one `Tasks` list, since GenPass loadWeight/progress reporting assumes roughly proportional cost |

## UX Pitfalls

| Pitfall | User Impact | Better Approach |
|---------|-------------|-------------------|
| Torches marketed/coded as a "safety" feature (spawn prevention) when they aren't (Pitfall 2) | Player believes the arena is spawn-safe, gets ambushed by an ambient biome enemy mid-boss-fight, appears as a bug/unfairness rather than a known limitation | Scope torches as visual-only in both code comments and any player-facing text; implement real spawn suppression separately via `EditSpawnRate` if desired |
| A custom exit teleporter placed far from the boss's likely fight area (e.g. only at the original spawn point) after multi-tier platforms are added, forcing a long walk back after every fight | Tedious return trip, undermines the "편의성" goal the feature was meant to serve | Place the exit trigger, or make the pause-menu Return button clearly the primary path, reachable from anywhere on the platform's traversable tiers, not just ground level |

## "Looks Done But Isn't" Checklist

- [ ] **Boundary/containment walls:** Often verified only in the Flat/plain arena — verify against all 9 arenas individually, since `surfaceY` varies 50-650 (Pitfall 3) and only some arenas have modded-type JIT guards to also re-check (Pitfall 5).
- [ ] **Torch/lighting placement:** Often assumed to also suppress spawns — verify it does NOT claim to, and if spawn suppression is separately in scope, verify it's implemented via `EditSpawnRate`/safe walls, not light level (Pitfall 2).
- [ ] **Multi-tier platforms in biome-gated arenas (Corruption/Hallow/Jungle/Desert/Astral/Briar):** Often tested only by standing on the base strip — verify the boss doesn't despawn when the player is on an upper/lower tier, especially for Desert and Jungle (tightest tile-weight margins per Pitfall 4).
- [ ] **New entry/exit convenience object:** Often tested only via the happy path (walk up, interact, teleport) — verify it calls `SubworldSystem.Exit()` (not a hand-rolled teleport) and that the pause-menu Return button still works afterward in the same arena (Pitfall 6).
- [ ] **Shared "arena polish" helper:** Often smoke-tested only with all content mods enabled — verify the mod still loads with CalamityMod disabled AND separately with SpiritMod disabled after adding any new shared decoration/torch/boundary code (Pitfall 5).
- [ ] **Prep-time delay before boss auto-summon:** Often tested only for the delay itself firing once — verify no double-spawn, and verify entering a second, different arena afterward doesn't spawn a leftover/stale boss (Pitfall 7).

## Recovery Strategies

| Pitfall | Recovery Cost | Recovery Steps |
|---------|-----------------|------------------|
| Falling-tile stack overflow reintroduced (Pitfall 1) | LOW | Same fix pattern as the original Desert bug: identify the offending falling-tile fill, cap it to a thin cosmetic top layer over a solid, non-falling substrate; no data loss since `ShouldSave = false` means the arena regenerates clean next entry |
| Zone-flag flip from multi-tier drift (Pitfall 4) | LOW-MEDIUM | Revert or extend the biome-tile fill on the affected tier; no world-file corruption risk since this only affects the throwaway subworld's live tile state, not the main world |
| Custom exit teleporter skipping `OnExit()` flag restore (Pitfall 6) | MEDIUM | If discovered post-ship, the safest recovery is reverting the custom teleporter to call `SubworldSystem.Exit()` directly and re-testing the specific vanilla flags in the `.planning/debug/resolved/isolation-premise-flag-persistence.md` whitelist for leakage; if a real leak already occurred in a player's save, it's a manual main-world flag correction, same class of recovery as the original isolation bug |
| JIT-unsafe shared helper (Pitfall 5) | LOW | Refactor the helper's signature to primitive types per the prevention section; this is a compile-time-catchable class of bug once the "disable CalamityMod / disable SpiritMod" smoke test is run, so recovery is typically caught before ship |

## Pitfall-to-Phase Mapping

| Pitfall | Prevention Phase | Verification |
|---------|--------------------|----------------|
| 1. Falling-tile stack overflow reintroduced | Shared decoration/multi-tier helper phase (early) | Enter every arena that received new decoration/tiers; check client log for Natives.log "Stack overflow" near SquareTileFrame/TileFrame |
| 2. Torches mistaken for spawn suppression | Torch/lighting phase | Code review: confirm no claim that torches gate spawns; if spawn suppression is scoped, confirm `EditSpawnRate` (or safe walls) implementation, not torch density |
| 3. Hardcoded Y-boundary wrong per biome | Boundary/containment phase (shared helper) | Enter Space, Underworld, and Briar specifically (most divergent `surfaceY`) and confirm walls sit near that arena's real platform |
| 4. Multi-tier platform drift flips Zone flags | Multi-tier platform phase (per-biome) | Move player to every added tier while a biome-gated boss (e.g. Astral/Briar-routed boss) is alive; confirm no despawn |
| 5. JIT-unsafe shared "polish" helper | Shared-layer phase (helper API design) | Disable CalamityMod only, confirm mod loads; disable SpiritMod only, confirm mod loads; repeat after every helper API change |
| 6. Custom exit teleporter bypasses `SubworldSystem.Exit()` / `noReturn` misuse | Entry/exit convenience phase | Exit via the new object and via the pause-menu Return button separately; confirm identical downed-flag behavior; confirm Return button still present (`noReturn` untouched) |
| 7. Prep-time delay double-summons or leaks state | Entry/exit convenience phase | Full delay cycle in one arena, confirm exactly one boss NPC; enter a second, different arena afterward, confirm no stray spawn |
| 8. Boundary walls clip high-mobility boss AI | Boundary/containment phase (per-biome) | Fight at least one confirmed high-mobility/flying-phase boss per arena after walls are added; confirm no stalled phase-transition or visible wall-clipping |

## Sources

- Direct source read (HIGH confidence): `Subworlds/BossArenaSubworld.cs`, `Subworlds/BossArenaAstralSubworld.cs`, `Subworlds/FlatStonePlatformPass.cs`, `Subworlds/CorruptionPlatformPass.cs`, `Subworlds/DesertPlatformPass.cs`, `Subworlds/UnderworldPlatformPass.cs`, `Subworlds/SpacePlatformPass.cs`, `Subworlds/HallowPlatformPass.cs`, `Subworlds/JunglePlatformPass.cs`, `Subworlds/AstralPlatformPass.cs`, `Subworlds/BriarPlatformPass.cs`, `Systems/BossArenaRoutingRegistry.cs`, `Systems/BossSummonPlayer.cs`, `Systems/ForcedTimeSystem.cs`, `Tiles/Test1Tile.cs`, `.planning/PROJECT.md`
- `.planning/debug/resolved/isolation-premise-flag-persistence.md` (referenced from source comments, not independently re-read this session — treated as HIGH confidence per PROJECT.md's own repeated citation of it as a locked, load-bearing fix)
- `.planning/debug/resolved/hivemind-zonecorrupt-despawn-corruption-subworld.md` (referenced from source comments; same confidence basis)
- `.planning/phases/02-summon-item-redirect-entry-registry/02-03-PLAN.md` — source for the confirmed "SubworldLibrary auto-adds a Return button since BossArenaSubworld does not set noReturn" behavior
- https://terraria.wiki.gg/wiki/NPC_spawning — MEDIUM-HIGH confidence, official wiki: confirms spawn-rate factors (time, biome, ground tile, friendly-NPC proximity) and the safe-wall spawn-blocking mechanic; explicitly does NOT list light level as a spawn factor, directly contradicting the "light-level-gated" assumption in this milestone's research question
- https://github.com/tModLoader/tModLoader/wiki/World-Generation and https://docs.tmodloader.net/docs/stable/class_gen_pass.html — MEDIUM confidence, official docs: general `GenPass`/`ApplyPass` ordering semantics referenced for the "later pass triggers the recursion, not necessarily the placing pass itself" reasoning in Pitfall 1
- "Torch God's Favor" wrong-biome-torch debuff — flagged LOW confidence (training-data recall, not independently re-verified this session against current wiki text or tested inside this specific subworld context); included in Technical Debt table only, not promoted to a Critical Pitfall, specifically because it's unverified

---
*Pitfalls research for: BossArenaSubWorld v1.1 — arena subworld design improvements (biome decoration, multi-tier platforms, containment walls, torches, entry/exit convenience)*
*Researched: 2026-08-15*
