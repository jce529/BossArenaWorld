# Architecture Research

**Domain:** tModLoader mod — per-biome procedural arena world-gen (SubworldLibrary `Subworld`/`GenPass` pipeline)
**Researched:** 2026-08-15
**Confidence:** HIGH (current codebase read directly), MEDIUM (GenPass execution-order assumption, flagged below)

## Standard Architecture (current, as-built)

### System Overview

```
┌───────────────────────────────────────────────────────────────────────┐
│ Subworlds/BossArena{Plain,Corruption,Hallow,Underworld,Jungle,Space,   │
│           Desert,Astral,Briar}Subworld.cs   (9 ModType classes)       │
│  - autoloaded UNCONDITIONALLY at mod load (regardless of installed    │
│    content mods) -> JIT-prefiltered in full, every method             │
│  - Width/Height consts, ShouldSave=false, NoPlayerSaving=false        │
│  - Tasks => new List<GenPass> { new XPlatformPass(...) }  (1 entry)   │
│  - OnEnter()/OnExit(): ~35-field vanilla-downed-flag snapshot/restore │
│    guard, DUPLICATED VERBATIM in all 9 classes (SubworldLibrary       │
│    CopyDowned() workaround, applies per-subworld-type independently)  │
├───────────────────────────────────────────────────────────────────────┤
│ Subworlds/{FlatStone,Corruption,Hallow,Underworld,Jungle,Space,       │
│            Desert,Astral,Briar}PlatformPass.cs  (9 GenPass classes)   │
│  - plain C# class, NOT a ModType -> only JIT-prefiltered per-METHOD,  │
│    only constructed when its owning Subworld.Tasks getter runs        │
│  - single ApplyPass(): fills a flat rectangular strip (double for-    │
│    loop over x/thickness), sets Main.spawnTileX/Y                     │
│  - Astral/Briar ONLY: ApplyPass() tagged [JITWhenModsEnabled(modname)]│
│    because it references CalamityMod/SpiritMod tile types directly    │
├───────────────────────────────────────────────────────────────────────┤
│ Systems/BossArenaRoutingRegistry.cs                                   │
│  - static Dictionary<bossNpcType, Func<bool> Enter>, plus              │
│    HashSet<Type> _knownArenaTypes (grows via Register<T>())           │
│  - Systems/BossSummonPlayer.cs, Tiles/Test1Tile.cs consume this to    │
│    stay boss-agnostic / arena-agnostic                                │
└───────────────────────────────────────────────────────────────────────┘
```

**Correction to milestone-context framing:** the milestone brief describes "7 biome variants alongside the plain arena." The actual repo has **8** biome-specific `Subworld` classes (Corruption, Hallow, Underworld, Jungle, Space, Desert, Astral, Briar) plus the plain arena = **9 total `Subworld` classes and 9 paired `GenPass` classes**, not 8. All build-order and file-count guidance below uses the real count (9). Only 2 of the 9 GenPass classes (Astral, Briar) touch modded types and carry `[JITWhenModsEnabled]`; the other 7 (FlatStone, Corruption, Hallow, Underworld, Jungle, Space, Desert) are 100% vanilla (`Terraria.ID.TileID` only).

### Component Responsibilities

| Component | Responsibility | Current Implementation |
|-----------|----------------|-------------------------|
| `Subworlds/BossArena*Subworld.cs` (9x) | Declare world dimensions, `Tasks` list, `ShouldSave`/`NoPlayerSaving`, vanilla-downed-flag isolation guard | `Subworld` subclass, duplicated boilerplate per file, zero modded-type references even in Astral/Briar variants |
| `Subworlds/*PlatformPass.cs` (9x) | Fill the arena's floor/platform with biome-correct tiles at biome-correct Y-position; set spawn point | `GenPass` subclass, one `ApplyPass()` method; 2 of 9 need `[JITWhenModsEnabled]` |
| `Systems/BossArenaRoutingRegistry.cs` | Map a boss NPC type -> which arena `Subworld` to enter; track "is current subworld one of ours" | Static dictionary + `HashSet<Type>`, `Register<T>()`/`Enter()`/`IsAnyArenaActive()` |
| `Systems/BossSummonPlayer.cs` | On subworld arrival, auto-spawn the pending boss (+ Infernum-toggle force where flagged) | `ModPlayer.OnEnterWorld()`, static `PendingBossNpcType` consumed once |
| `Systems/BiomeOverridePlayer.cs` | Generic per-tick "force something while inside a boss arena" hook | `ModPlayer.PostUpdate()`, gated by `SubworldSystem.IsActive<BossArenaSubworld>()` (currently only checks the PLAIN arena — see Integration Points) |
| `Tiles/Test1Tile.cs` | Entry trigger: right-click while holding a registered summon item redirects into the routed arena | `ModTile.RightClick()`, this mod's own tile type (never conditional on another mod) |

## Recommended Project Structure (additions for v1.1)

```
Subworlds/
├── ArenaBuilder.cs                 # NEW — static helper, vanilla-only primitives
│                                    #   (FillRectangle, PlaceBoundaryWalls, PlaceAtInterval,
│                                    #    BuildTierPlatform, PlaceReturnPortal). Not a ModType,
│                                    #   not a GenPass — a plain static class both ArenaPolishPass
│                                    #   AND the 9 per-biome ApplyPass() methods can call.
├── ArenaPolishPass.cs              # NEW — single shared GenPass (boundary walls, Y-limit
│                                    #   containment tiles, interval torch lighting, multi-tier
│                                    #   deck placement, return-portal tile placement). 100%
│                                    #   vanilla types only — NO [JITWhenModsEnabled] needed.
├── BossArena{9 variants}Subworld.cs  # MODIFIED — Tasks list gets a 2nd entry:
│                                      #   new ArenaPolishPass(...) appended after the existing
│                                      #   biome fill pass, with per-arena surfaceY/thickness
│                                      #   passed as constructor args (each arena already knows
│                                      #   its own values today — Space=50, Underworld=650,
│                                      #   Desert thickness=20, everything else=Main.maxTilesY/2).
├── *PlatformPass.cs (9x)            # MODIFIED — each ApplyPass() gains biome-specific
│                                      #   DECORATION calls (the one part that cannot be shared).
│                                      #   For Astral/Briar, new decoration code referencing
│                                      #   Calamity/Spirit tile types stays INSIDE the already-
│                                      #   [JITWhenModsEnabled]-tagged ApplyPass() method — no new
│                                      #   tagging surface introduced.
Tiles/
└── ReturnPortalTile.cs             # NEW — ModTile placed by ArenaPolishPass near spawn;
                                       #   right-click calls SubworldSystem.Exit() (mirror of
                                       #   Test1Tile's entry pattern, this mod's own tile type,
                                       #   so it is unconditionally JIT-safe like Test1Tile).
Systems/
├── BossSummonPlayer.cs             # MODIFIED — add a short "preparation time" delay (tick
│                                       #   countdown) before NPC.SpawnOnPlayer() fires, instead
│                                       #   of spawning on the same tick as OnEnterWorld().
└── BiomeOverridePlayer.cs          # MODIFIED (or new sibling ModPlayer) — runtime Y-bound
                                       #   defense-in-depth: per-tick clamp/teleport-back if the
                                       #   player exceeds the arena's intended Y range, gated by
                                       #   BossArenaRoutingRegistry.IsAnyArenaActive() (already
                                       #   exists and already covers all 9 arena types — note
                                       #   BiomeOverridePlayer.PostUpdate() currently only checks
                                       #   IsActive<BossArenaSubworld>(), the PLAIN arena; this is
                                       #   a pre-existing narrowing bug worth fixing as part of
                                       #   this milestone regardless of the new Y-bound feature).
```

### Structure Rationale

- **`ArenaBuilder.cs` as a plain static class, not a `GenPass`:** it needs to be callable both from the new shared `ArenaPolishPass` AND from inside each existing per-biome `ApplyPass()` method (e.g., so `AstralPlatformPass.ApplyPass()` can call `ArenaBuilder.PlaceAtInterval(...)` for its own biome-flavored decoration while still resolving the Astral-specific tile type itself, inside its own already-tagged method). A plain static class carries no autoload/JIT-prefilter risk of its own as long as its method signatures never mention a modded type by name — exactly the discipline the codebase already documents for `AstralPlatformPass`/`BriarPlatformPass` ("Calamity type references live ONLY inside this class's using directives and ApplyPass() method body").
- **`ArenaPolishPass.cs` as ONE shared `GenPass`, not 9 duplicated ones:** boundary walls, Y-limit tiles, torch spacing, multi-tier decking, and return-portal placement are all biome-agnostic — none of them need to know whether the arena is Corruption or Astral, only where the platform surface is (a primitive `int`/`ushort`, passed in via constructor). This is the direct answer to "should there be a shared ArenaPolishPass" — yes, and it should be a genuinely single class appended to all 9 `Tasks` lists, not a base class each `*PlatformPass` inherits from (composition over inheritance keeps each biome pass's `ApplyPass()` — some of which are `[JITWhenModsEnabled]`-tagged — untouched in shape).
- **Per-biome decoration explicitly NOT centralized:** decoration requires biome-specific tile *types* (Corruption thorns vs. Hallow crystal shards vs. Astral decor items), which only each biome's own `ApplyPass()` can resolve safely (Astral/Briar's modded tile lookups must stay inside their tagged methods). `ArenaBuilder` supplies the shared *placement algorithm* (e.g., "place tile X every N columns along row Y"); each `*PlatformPass.ApplyPass()` supplies the *which tile* argument.
- **`ReturnPortalTile.cs` needs no JIT tag at all:** it is this mod's own `ModTile`, not a modded content-mod type, so — like `Test1Tile` — it is always present regardless of which content mods are installed/enabled. It is fully safe to place from the shared, untagged `ArenaPolishPass`.
- **Preparation-time and portal-based exit are NOT GenPass concerns:** they are runtime/tick logic, not world-gen tile placement, so they belong in `Systems/` (extending `BossSummonPlayer.cs`'s existing `OnEnterWorld()` boss-spawn trigger, and a new `ReturnPortalTile.RightClick()` mirroring `Test1Tile.RightClick()`'s existing exit-trigger shape), not in `Subworlds/`.

## Architectural Patterns

### Pattern 1: Shared vanilla-only `GenPass` appended to every arena's `Tasks` list

**What:** A single `ArenaPolishPass : GenPass` class, constructed once per arena with that arena's own `surfaceY`/`thickness` (and any other per-arena parameters, e.g. return-portal offset), appended as entry #2 in every `Subworld.Tasks` list after the existing biome fill pass.
**When to use:** Any new arena feature that does not need to know the biome (boundary walls, Y-limit containment, torch lighting, multi-tier decking, return-portal placement).
**Trade-offs:** Adds one extra `GenPass` execution per arena entry (cheap — same order of magnitude as the existing fill pass, single 10000-wide loop). Requires each `Subworld.Tasks` getter to be touched (9 one-line edits) but avoids 9x duplicated boundary/lighting/tier logic. Assumes `Tasks` list order = execution order (see confidence note below — verify with a quick in-game check before relying on "boundary walls after the biome floor exists" ordering).

**Example:**
```csharp
// Subworlds/BossArenaAstralSubworld.cs — Tasks getter, MODIFIED
public override List<GenPass> Tasks => new()
{
    new AstralPlatformPass("Astral Infection Boss Arena Platform", 1f),
    new ArenaPolishPass("Arena Polish", 1f, surfaceY: Main.maxTilesY / 2, thickness: 15)
};
```

```csharp
// Subworlds/ArenaPolishPass.cs — NEW, no [JITWhenModsEnabled] anywhere in this file
public class ArenaPolishPass : GenPass
{
    private readonly int _surfaceY;
    private readonly int _thickness;

    public ArenaPolishPass(string name, float loadWeight, int surfaceY, int thickness) : base(name, loadWeight)
    {
        _surfaceY = surfaceY;
        _thickness = thickness;
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Polishing boss arena";
        ArenaBuilder.PlaceBoundaryWalls(minY: _surfaceY - 40, maxY: _surfaceY + _thickness + 40, worldWidth: Main.maxTilesX);
        ArenaBuilder.PlaceAtInterval(y: _surfaceY - 1, worldWidth: Main.maxTilesX, interval: 40, tileType: TileID.Torches);
        ArenaBuilder.BuildTierPlatform(y: _surfaceY - 15, x0: Main.maxTilesX / 2 - 500, x1: Main.maxTilesX / 2 + 500, tileType: TileID.Platforms);
        ArenaBuilder.PlaceReturnPortal(x: Main.maxTilesX / 2 + 5, y: _surfaceY - 4);
    }
}
```

### Pattern 2: Per-method JIT-tagging boundary preserved for the shared helper

**What:** `ArenaBuilder`'s own method signatures accept only vanilla primitives (`int`, `ushort` tile-type IDs, `bool`) — never a `CalamityMod.*`/`SpiritMod.*` type by name. Biome-specific decoration keeps resolving `ModContent.TileType<AstralStone>()` etc. *inside* the already-`[JITWhenModsEnabled]`-tagged `ApplyPass()` methods, then passes the resulting `ushort` down into `ArenaBuilder`.
**When to use:** Any time a shared helper needs to be callable from both JIT-tagged (Astral/Briar) and untagged (the other 7) call sites. This is the load-bearing rule that lets one `ArenaBuilder`/`ArenaPolishPass` exist without needing 9 separate copies or a second tag matrix.
**Trade-offs:** Requires discipline — a future contributor adding, say, a Calamity-specific decorative tile check *inside* `ArenaBuilder` itself would silently reintroduce the exact `JITException` the codebase already hit once (Phase 9, D-01/Pitfall 4: "lazy class construction alone is NOT sufficient JIT protection" — tModLoader JIT-prefilters every method in the assembly regardless of reachability). Worth a one-line comment atop `ArenaBuilder.cs` codifying this constraint, matching the existing comment style in `AstralPlatformPass.cs`/`BriarPlatformPass.cs`.

### Pattern 3: Constructor-parameterized per-arena config instead of hardcoded per-pass constants

**What:** `ArenaPolishPass` (and `ReturnPortalTile` placement) take `surfaceY`/`thickness`/offsets as constructor arguments supplied by each `Subworld.Tasks` getter, rather than re-deriving or hardcoding them a second time.
**When to use:** Whenever a new shared component needs to know where the existing biome-specific fill pass already put the floor. Each arena already has these values today as inline literals inside its own `*PlatformPass.ApplyPass()` (Space=50/10, Underworld=650/10, Desert=Main.maxTilesY/2/20, everything else=Main.maxTilesY/2/15) — this milestone is a natural point to hoist them into named `public const` fields on each `*PlatformPass` class (or each `Subworld` class) so both the fill pass and `ArenaPolishPass` reference the same source of truth instead of two independently-hardcoded numbers drifting apart.
**Trade-offs:** Small refactor (9 files gain 1-2 named constants each) but removes an easy-to-miss duplication bug (e.g., changing Desert's `thickness` from 20 to 25 later would silently desync `ArenaPolishPass`'s boundary-wall Y math unless it reads the same constant).

## Data Flow

### World-gen (arena creation) flow

```
Test1Tile.RightClick()
    ↓ (SUBW-01 entry trigger)
BossArenaRoutingRegistry.Enter(bossNpcType)
    ↓
SubworldSystem.Enter<T>()  →  T.Tasks getter invoked
    ↓
[1] new XPlatformPass(...).ApplyPass()      — biome floor fill + spawn point (existing, per-biome)
[2] new ArenaPolishPass(...).ApplyPass()    — boundary walls, Y-limit, torches, tiers, return
                                                portal (NEW, shared, runs after [1] so it can
                                                build relative to the already-placed floor —
                                                MEDIUM confidence this is guaranteed by Tasks
                                                list order; verify once in-game before depending
                                                on it for anything load-bearing like "torches sit
                                                exactly N tiles above the real floor")
    ↓
BossSummonPlayer.OnEnterWorld()
    ↓ [MODIFIED] short prep-time countdown (new) → then:
NPC.SpawnOnPlayer(PendingBossNpcType)
```

### Runtime containment flow (new)

```
Every tick, while BossArenaRoutingRegistry.IsAnyArenaActive():
    BiomeOverridePlayer.PostUpdate() (or new sibling ModPlayer)
        → if player.position.Y < arena's minY OR > arena's maxY:
              clamp/teleport player back inside bounds
```
This mirrors the existing `BiomeOverridePlayer` pattern (per-tick force-check gated by "are we in one of our arenas," already established for biome-flag forcing) — reuse the shape, don't invent a new one. Note the existing gate (`SubworldSystem.IsActive<BossArenaSubworld>()`) only checks the **plain** arena today; extending it to all 9 requires switching to `BossArenaRoutingRegistry.IsAnyArenaActive()`, which already exists and already tracks every registered arena type via `_knownArenaTypes`.

### Key Data Flows

1. **Arena assembly:** `Subworld.Tasks` list order → biome fill pass, then shared polish pass. Both write directly to `Main.tile[x,y]`/`Main.spawnTileX/Y` (same primitive as today, no new abstraction needed for tile writes themselves).
2. **JIT-safety boundary:** modded-type resolution happens exactly once per biome, inside that biome's own tagged `ApplyPass()`; everything downstream of that point (into `ArenaBuilder`, into `ArenaPolishPass`) only ever sees `ushort`/`int` — never a modded `Type` reference.
3. **Containment defense-in-depth:** GenPass-time boundary tiles (physical, one-time, cheap) + runtime per-tick Y-clamp (`BiomeOverridePlayer`-style, catches drills/teleport items/anything that bypasses the physical wall) — two layers, not one, because Terraria tiles are minable by design and a purely physical wall is not a hard guarantee.

## Build Order (Rollout Sequencing)

Given the existing JIT-safety and per-biome isolation constraints, build in this order:

1. **`ArenaBuilder.cs`** — pure static helper, zero dependencies on any `Subworld`/`GenPass`, easiest to write/reason about in isolation. No JIT tag anywhere in this file (enforce via the Pattern 2 discipline above).
2. **`ArenaPolishPass.cs`** — shared `GenPass` built against `ArenaBuilder`. Wire it into **`BossArenaSubworld.cs` (the plain arena) FIRST.** This is the safest possible integration target: zero biome-flag concerns (Pattern in `FlatStonePlatformPass`'s own header comment — "absence-by-construction"), zero JIT concerns (no modded types anywhere on this arena's call path today). Boundary walls, torch spacing, multi-tier decking, return portal, and prep-time delay should all be live-verified in-game here before touching any other arena.
3. **Extend to the 6 remaining non-modded biome variants** (Corruption, Hallow, Underworld, Jungle, Space, Desert) — one-line `Tasks`-list append + correct `surfaceY`/`thickness` constructor args per arena, following each arena's existing per-biome Y-placement comment (Space=50, Underworld=650, Desert thickness=20, rest=`Main.maxTilesY/2`/15). These carry the same JIT profile as the plain arena today (zero modded-type references anywhere in their `Tasks` chain), so this step is still low-risk mechanical repetition, not new research.
4. **Extend to Astral and Briar LAST**, specifically because they are the only 2 of the 9 with an existing JIT-tagged surface. Appending an *untagged* `ArenaPolishPass` instance to their `Tasks` list does not by itself introduce new JIT risk (the new pass's own methods reference no modded types), but this is exactly the class of change the project has been burned by once already (Phase 9, D-01) — so treat this step as requiring the same live-verification discipline already established: **build with `CalamityMod` disabled and re-confirm `BossArenaAstralSubworld` still loads without a `JITException`; separately, with `SpiritMod` disabled, re-confirm `BossArenaBriarSubworld` still loads.** This is a cheap sanity check that directly matches the project's own documented pitfall.
5. **Per-biome decoration LAST, inside each `*PlatformPass.ApplyPass()` individually** — the one piece of this milestone that is genuinely non-shareable. For the 7 vanilla-only passes, add decoration calls freely. For Astral/Briar, add decoration calls *inside the already-tagged `ApplyPass()` method* only — do not extract new decoration logic into a separate untagged helper method on those two classes, or the JIT-prefilter issue resurfaces for that specific new method.
6. **Systems-layer changes (prep-time delay, return-portal exit trigger, Y-bound runtime clamp) can proceed in parallel with steps 3-5** — they touch `Systems/BossSummonPlayer.cs`, `Systems/BiomeOverridePlayer.cs` (or a new sibling `ModPlayer`), and the new `Tiles/ReturnPortalTile.cs`, none of which depend on which arena variant is currently being polished. Recommend doing this alongside step 2 (plain-arena validation), since prep-time and return-portal behavior are best sanity-checked on the simplest arena first, same rationale as step 2.

## Anti-Patterns

### Anti-Pattern 1: A base-class `AbstractArenaGenPass` that all 9 `*PlatformPass` classes inherit from

**What people do:** Reach for inheritance to "share" boundary/lighting/tier logic by putting it in a shared base `GenPass` class and having `AstralPlatformPass : AbstractArenaGenPass` etc.
**Why it's wrong:** Two of the nine subclasses (Astral, Briar) need `[JITWhenModsEnabled]` on their `ApplyPass()` override; the other seven don't. Baking shared logic into a base class's own `ApplyPass()` (or a base method the subclasses call via `base.ApplyPass()`) blurs exactly the per-method JIT-tagging boundary this codebase has already had to fix once (D-01/Pitfall 4). Composition (a separate `ArenaPolishPass` instance appended to the `Tasks` list, plus a static `ArenaBuilder` helper) keeps every existing `*PlatformPass` class's shape — and its existing JIT tags — completely untouched.
**Do this instead:** Composition via an extra `Tasks` list entry (Pattern 1) and a static helper (Pattern 2).

### Anti-Pattern 2: Hardcoding return-portal/torch/wall tile placement using each biome's OWN decorative tile type

**What people do:** Since `ArenaPolishPass` is per-arena-constructed anyway, it's tempting to also pass in a biome-flavored torch/wall tile (e.g., Astral-themed torches) to make the "shared" pass feel more thematic.
**Why it's wrong:** The moment `ArenaPolishPass` (or `ArenaBuilder`) accepts a parameter whose *resolution* requires a modded type (even if the parameter itself is typed `ushort`), the caller resolving that modded tile type must do so somewhere — and if that "somewhere" is inside `ArenaPolishPass`'s own constructor call site in an UNTAGGED `Subworld.Tasks` getter (recall: `Subworld` is an autoloaded `ModType`, always JIT-prefiltered, and per the codebase's own `BossArenaAstralSubworld.cs` header comment must contain **zero** direct Calamity/Spirit type references), that reintroduces the exact bug class Phase 9 already fixed once.
**Do this instead:** Keep `ArenaPolishPass`'s vanilla-flavored torches/walls/tiers (plain `TileID.Torches`, `TileID.Platforms`, etc. — these work identically in every biome and don't affect any biome's tile-weighted Zone-flag count, since none of them appear in any biome's weighted tile set per the existing `*PlatformPass` header comments). Reserve biome-flavored decoration for the per-biome `ApplyPass()` methods, where modded-type resolution is already known-safe.

## Integration Points

### External Dependencies (unchanged by this milestone)

| Dependency | Integration Pattern | Notes |
|---------|---------------------|-------|
| SubworldLibrary (`Subworld`, `SubworldSystem`, `GenPass`) | `Subworld.Tasks` list execution, `SubworldSystem.Enter<T>()`/`Exit()` | No new SubworldLibrary API surface needed for any of the 6 requested features — everything is expressible as additional `GenPass` steps + existing `ModPlayer`/`ModTile` hooks |
| CalamityMod / SpiritMod (Astral/Briar tile types only) | `[JITWhenModsEnabled]`-tagged method bodies | Unchanged pattern; new decoration code for these two arenas must stay inside the existing tagged methods |

### Internal Boundaries (new/modified)

| Boundary | Communication | Notes |
|----------|---------------|-------|
| `Subworld.Tasks` getter ↔ `ArenaPolishPass` | Direct constructor call, `int`/`ushort` params only | 9 files touched (1-2 line addition each), zero files touched for `ArenaPolishPass.cs` itself since it's brand new |
| `*PlatformPass.ApplyPass()` ↔ `ArenaBuilder` | Static method calls, `ushort` tile-type params resolved by the caller | Applies to all 9 `ApplyPass()` methods if the fill-loop duplication is also refactored (optional but recommended given "리토픽 전체" scope already touches every file) |
| `ArenaPolishPass` ↔ `ReturnPortalTile` | `ArenaPolishPass.ApplyPass()` places the tile directly (`Tile.TileType = (ushort)ModContent.TileType<ReturnPortalTile>()`) — this mod's own type, always safe | No JIT concern; same category as `Test1Tile` |
| `BossSummonPlayer.OnEnterWorld()` ↔ prep-time delay | New tick-countdown field + `PostUpdate()` (or reuse `OnEnterWorld` with a deferred flag checked next frame) | Needs a design decision: delay via `ModPlayer.PostUpdate()` countdown (consistent with `BiomeOverridePlayer`'s existing per-tick style) vs. a one-shot timer — recommend the countdown-field approach for consistency with the codebase's established per-tick pattern |
| `BiomeOverridePlayer` (or new ModPlayer) ↔ `BossArenaRoutingRegistry.IsAnyArenaActive()` | Already-existing static method call | **Pre-existing gap worth fixing regardless of this milestone:** `BiomeOverridePlayer.PostUpdate()` currently gates on `SubworldSystem.IsActive<BossArenaSubworld>()` (plain arena ONLY), not `BossArenaRoutingRegistry.IsAnyArenaActive()` (all 9). Any new Y-bound containment logic added here should use the registry check, and it would be reasonable to fix the existing gate at the same time since it's a one-line change in a file already being modified for this milestone. |

## Sources

- Direct read of current repository source (`Subworlds/*.cs`, `Systems/*.cs`, `Tiles/Test1Tile.cs`) — HIGH confidence, ground truth for "what exists today"
- `.planning/PROJECT.md` — HIGH confidence, project-authored history/decisions log (Phase 9 D-01/Pitfall 4 JIT discipline, Phase 4 biome-routing rationale)
- Assumption flagged MEDIUM confidence: `Subworld.Tasks` list execution order is sequential top-to-bottom (`ArenaPolishPass` after the biome fill pass sees an already-filled floor). Inferred from every existing `Subworld.Tasks` list currently containing exactly one entry (so order has never actually been exercised in this codebase yet) plus standard `GenPass`/`WorldGenerator` semantics in tModLoader/vanilla world-gen (passes execute in list order, each seeing the previous pass's tile writes). Recommend a quick in-game check (e.g., a debug `Main.NewText` in `ArenaPolishPass.ApplyPass()` confirming `Main.tile[x, surfaceY].HasTile == true` before building on top of it) as the very first step of implementation, before relying on this ordering for anything load-bearing.

---
*Architecture research for: tModLoader boss-arena subworld design/visual-polish milestone (v1.1)*
*Researched: 2026-08-15*
