# Phase 1: Subworld Skeleton & Isolation Proof - Research

**Researched:** 2026-08-13
**Domain:** tModLoader 1.4.4.9 mod development — SubworldLibrary subworld creation, custom `GenPass` world generation, `ModCommand` chat commands, `ModPlayer` biome-zone overrides, vanilla boss-downed-flag isolation
**Confidence:** HIGH (all core API surfaces verified directly against SubworldLibrary v2.2.3.2 source and tModLoader 1.4.4 source/patch files, not just training-data recall or secondhand summaries)

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Test entry/exit mechanism**
- D-01: Use a debug-only chat command (e.g. `/bossarena-enter`, `/bossarena-exit`) to enter/exit the subworld for Phase 1 testing, since Phase 2's real summon-item redirect doesn't exist yet.
- D-02: This debug command is fully removed once Phase 2's real redirect lands — not kept behind a permanent debug flag. It's a Phase-1-only verification tool.

**Empty subworld terrain**
- D-03: The subworld has a minimal flat platform, not a bare void and not a fully decorated arena. Purpose is a walkable surface sufficient for Phase 1's isolation-proof test.
- D-04: Full arena decoration (multi-layer platforms, aesthetics) is explicitly deferred — Phase 1 builds only the minimal platform. This stays consistent with REQUIREMENTS.md's Out of Scope item "Full arena-building/decoration toolkit" (duplicates Luiafk).
- D-05: Platform material: stone blocks.
- D-06: Platform width: approximately 10,000 blocks wide, horizontal flat plane (user's explicit request — large enough to accommodate any boss's movement range in later phases).
- D-07: Platform thickness: simple/thin stone layer, no elaborate depth requirement (user: "just make it stone blocks"). Claude's discretion on exact value — a thin layer (roughly 10-20 blocks) is reasonable.
- D-08: No edge/boundary walls at the platform's ends — not needed given the platform's width.

**Biome zone override infrastructure**
- D-09: Build a general-purpose hook/function now that can force-set `Player.Zone*` flags while inside the subworld, since some bosses (Wall of Flesh needs Underworld, Plantera needs Jungle, Duke Fishron needs Ocean, etc.) require specific biome conditions to spawn or behave correctly. This is infrastructure only — no boss-to-biome mapping exists yet (that's populated per-boss starting Phase 3+ once `BossRegistry` exists).

**Isolation-proof method**
- D-10: Use King Slime as the test boss for the empirical isolation-proof test (not Moon Lord — rejected because Moon Lord requires defeating 3 mechanical bosses + Golem first, making it impractical to test repeatedly).
- D-11: Actually summon and kill King Slime for real inside the subworld (not just toggling `NPC.downedSlimeKing` via debug command) — validates real gameplay behavior, not just the boolean.
- D-12: No `BossCoreItem`/`BossRegistry` carrier-item is used in this test — those don't exist until Phase 3. The test purely observes whether `NPC.downedSlimeKing` propagates from the subworld back to the main world on return, with no explicit sync action taken. Per `research/PITFALLS.md` Pitfall 1, the expected (correct) result is that it does NOT propagate — that's the premise being proven.

**World-backup & test-world strategy**
- D-13: Phase 1's own testing happens on a fresh, disposable test world with all other content mods (Calamity, Spirit, etc.) unloaded/disabled — not the player's real save (`HiPo's_Terrarium`). No backup is needed for this throwaway world.
- D-14: The VERIFY-02 world-backup guidance deliverable is still written in this phase, but as forward-looking documentation for later phases (4-8) when testing must happen against the real save with all content mods enabled. It is not exercised by Phase 1's own testing.

### Claude's Discretion
- Exact platform Y-level/vertical position within the subworld
- Precise platform thickness value (guideline: thin, ~10-20 blocks)
- `GenPass` implementation details for generating the flat stone platform
- Debug command naming/argument syntax specifics
- Format and location of the world-backup guidance document (e.g. a markdown doc vs. inline code comments)

### Deferred Ideas (OUT OF SCOPE)
- **Arena decoration (multi-layer platforms, aesthetics)** — resolved: Phase 1 builds only the minimal flat platform; richer arena construction deferred to whichever later phase actually needs a fight-ready arena, bounded by REQUIREMENTS.md's "Full arena-building/decoration toolkit" Out of Scope entry.
- **Per-boss biome-to-flag mapping** — only the generic override hook (D-09) is built now. Actual boss→biome wiring belongs with each boss's registration starting Phase 3+.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| SUBW-05 | The boss-arena subworld has zero placed mod content (custom `GenPass` list, not vanilla/modded worldgen) | Confirmed via direct read of SubworldLibrary v2.2.3.2 `SubworldSystem.LoadSubworld()`: only the `Subworld.Tasks` list you provide ever runs — `WorldGen.clearWorld()` is called first, then your `GenPass` list is the *entire* generation pipeline with no vanilla/modded passes silently injected. See "Architecture Patterns → Pattern 1" and "Code Examples". |
| SUBW-06 | Player can reliably exit/return from the subworld to the main world | Confirmed via `Subworld.NoPlayerSaving` semantics (source-verified: `false` = player file NOT reloaded from disk on exit, so inventory/carried items survive) and `SubworldSystem.Enter<T>()`/`Exit()` API. See "Architecture Patterns → Pattern 1" and "Common Pitfalls". |
| VERIFY-02 | World-backup guidance is documented and followed before any live testing against a real save | Addressed as a standalone guidance document (forward-looking per D-14) — see "Code Examples → World-backup guidance" and default tModLoader save-path research below. |
</phase_requirements>

## Summary

This phase builds three small, independently verifiable pieces on top of the already-confirmed-installed SubworldLibrary v2.2.3.2 (Steam Workshop, tagged compatible with tModLoader 1.4.3/1.4.4): (1) a `Subworld` subclass whose `Tasks` list contains exactly one custom `GenPass` that fills a ~10,000-block-wide, ~10-20-block-thick stone platform and nothing else; (2) a pair of debug-only `ModCommand`s that call `SubworldSystem.Enter<T>()`/`Exit()` directly, standing in for Phase 2's real summon-item redirect; and (3) a generic `ModPlayer.PostUpdate()` hook that force-sets `Player.Zone*` flags, verified via direct source inspection to run *after* vanilla's own per-tick zone recalculation. All three pieces were verified against the actual installed library/game source rather than assumed from training data — SubworldLibrary's `Subworld.cs`/`SubworldSystem.cs` and tModLoader's `Player.cs.patch` were fetched and read directly.

The empirical isolation-proof test (SUBW-05/06 adjacent, the actual point of the phase) is now mechanistically explained, not just documented as an observed bug: `SubworldSystem`'s exit/entry path reloads (or regenerates) each world's own `WorldFileData` independently, and vanilla's own world-file loader unconditionally overwrites process-wide static flags like `NPC.downedSlimeKing` from whatever is in that specific world's file. Since the arena subworld has `ShouldSave = false`, it is regenerated from scratch (never read from disk) on every entry, and the main world's file is never touched by the subworld visit — so `NPC.downedSlimeKing` reverts to the main world's own saved value the moment `SubworldSystem.Exit()` reloads it. This is the exact mechanism Phase 1's manual test must demonstrate.

**Primary recommendation:** Build the `Subworld` subclass with a single custom `GenPass`, confirmed-safe `ShouldSave = false` / `NoPlayerSaving = false`, wire in two throwaway `ModCommand`s for entry/exit, add a generic `Player.Zone*`-forcing `PostUpdate()` hook, and run the King Slime kill/return/verify test on a disposable, mod-free-except-SubworldLibrary test world — not the real save. Before starting implementation, pin the .NET SDK via `global.json` (see Environment Availability — the machine currently resolves `dotnet` to SDK 10.0.201 by default, which CLAUDE.md and the official wiki explicitly warn against for tModLoader builds, even though `TargetFramework` is hardcoded to `net8.0` in `tMLMod.targets`).

## Project Constraints (from CLAUDE.md)

These are locked, non-negotiable directives already in the project's `CLAUDE.md` and apply directly to this phase's plan:

- **.NET SDK**: Must use .NET 8.0 SDK for building. Explicitly avoid .NET 9.0/10.0 SDKs ("those will not work" per official tModLoader wiki). *(Research finding below: the dev machine's default `dotnet` resolves to 10.0.201 with no `global.json` present — this needs a Wave 0 fix.)*
- **Build command**: `dotnet msbuild` (or `dotnet build <ModName>.csproj` as fallback if references don't resolve — run `dotnet restore` first on `project.assets.json not found` errors).
- **Dependency**: SubworldLibrary referenced via `modReferences = SubworldLibrary` in `build.txt` — a **strong** reference (not weak), since the entire subworld mechanic depends on it. No `[JITWhenModsEnabled]` needed for SubworldLibrary calls.
- **Scaffolding**: Do not hand-write `.csproj` from a generic template — this project's `.csproj`/`build.txt` were already scaffolded by tModLoader's "New Mod" wizard; only edit them, don't regenerate from scratch.
- **Flag-setting discipline** (applies from Phase 3+, but the isolation-proof test in this phase must respect it too): never write directly to a raw backing field/bypass a mod's real setter — not directly relevant to Phase 1's read-only verification test (we only *read* `NPC.downedSlimeKing`, never set it), but the debug commands must not be tempted into "just set the flag to true" shortcuts (D-11 explicitly requires a real kill).
- **GSD workflow enforcement**: All file-changing work for this phase must go through `/gsd:execute-phase` — noted for the planner/executor, not actionable in research itself.

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|---------------|
| tModLoader | 1.4.4.9 | Mod runtime/loader | Already the project's target platform; confirmed via `.csproj`/`tModLoader.targets` import chain resolving to `net8.0` |
| .NET SDK | 8.0.424 (installed alongside 9.0.310 and 10.0.201 on this machine) | Compiler/toolchain | Verified installed via `dotnet --list-sdks`. **Risk:** `dotnet --version` in the project directory currently resolves to `10.0.201` (highest installed, no `global.json` pin) — see Environment Availability. |
| SubworldLibrary | 2.2.3.2 (confirmed via direct `build.txt` read in the installed Workshop copy, item 2785100219, tagged for tModLoader 1.4.3/1.4.4) | Subworld creation/entry/exit | Verified installed on this machine at `D:\SteamLibrary\steamapps\workshop\content\1281930\2785100219\2025.9\SubworldLibrary.tmod`. This is a strong `modReferences` dependency, not optional. |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `Terraria.WorldBuilding.GenPass` (built into tModLoader/vanilla, not a package) | N/A | Base class for the custom platform-generation pass | Subclass this for the one `GenPass` in `Tasks` |
| `Terraria.ModLoader.ModCommand` (built into tModLoader) | N/A | Debug-only `/bossarena-enter` / `/bossarena-exit` chat commands | Temporary, per D-01/D-02 — delete the whole file in Phase 2 |
| `Terraria.ModLoader.ModPlayer` (built into tModLoader) | N/A | `PostUpdate()` hook for forcing `Player.Zone*` flags | Confirmed (via source) to run after vanilla's own zone recalculation each tick — see Architecture Patterns |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `ModCommand` debug entry/exit | A temporary debug `ModItem` or NPC dialogue trigger | `ModCommand` is simpler for a throwaway Phase-1-only tool (no sprite/texture needed, trivially deletable per D-02); no reason to prefer an item here |
| `ModPlayer.PostUpdate()` for zone override | Hooking into a different, earlier point (e.g. `PreUpdate`) | `PreUpdate` runs *before* vanilla recalculates zones, so any override there gets immediately overwritten by vanilla the same tick — confirmed wrong by source inspection (see Architecture Patterns). `PostUpdate` is correct. |

**Installation (build.txt additions):**
```
modReferences = SubworldLibrary
```
No NuGet packages are involved — SubworldLibrary is resolved by tModLoader's own build pipeline via `modReferences`, using the already-installed Workshop copy. Run `dotnet restore` after editing `build.txt` if `project.assets.json not found` appears.

**Version verification:** SubworldLibrary version was verified directly (not via a registry, since this isn't an npm/NuGet package) by reading the installed copy's `build.txt`:
```
displayName=Subworld Library
author=John Snail
version=2.2.3.2
```
Location: `D:\SteamLibrary\steamapps\workshop\content\1281930\2785100219\2025.9\`. This directly contradicts nothing in prior research — the prior `ARCHITECTURE.md`/`PITFALLS.md` research already correctly identified this as the "Nov 24 2025" Workshop-updated build; `2.2.3.2` is the version string for that same build.

## Architecture Patterns

### Recommended Project Structure (Phase 1 slice)

```
BossArenaSubWorld/
├── BossArenaSubWorld.cs            # existing Mod entry class (untouched)
├── build.txt                       # add: modReferences = SubworldLibrary
├── Subworlds/
│   └── BossArenaSubworld.cs        # Subworld subclass + its GenPass(es)
├── Systems/
│   └── BiomeOverrideSystem.cs      # or a ModPlayer directly — generic Zone* force-set hook (D-09)
├── Debug/
│   └── SubworldDebugCommands.cs    # /bossarena-enter, /bossarena-exit — DELETE in Phase 2 (D-02)
└── docs/ or .planning/ (Claude's discretion per D-CONTEXT)
    └── WORLD_BACKUP_GUIDANCE.md    # VERIFY-02 deliverable
```

`Subworlds/` matches the existing `research/ARCHITECTURE.md` recommendation and SubworldLibrary's own auto-discovery: any `Subworld` subclass is auto-registered as a `ModType` (confirmed via source: `Register()` calls `SubworldSystem.subworlds.Add(this)`), no manual registration call needed anywhere.

### Pattern 1: `Subworld` subclass — exact confirmed API shape

**What:** `Subworld` is `public abstract class Subworld : ModType, ICopyWorldData, ILocalizedModType` in namespace `SubworldLibrary`. Verified directly from `Subworld.cs` (SubworldLibrary v2.2.3.2 source, fetched and read in full):

```csharp
public abstract class Subworld : ModType, ICopyWorldData, ILocalizedModType
{
    // Required overrides (abstract):
    public abstract int Width { get; }
    public abstract int Height { get; }
    public abstract List<GenPass> Tasks { get; }

    // Optional overrides (virtual, with defaults):
    public virtual WorldGenConfiguration Config => null;
    public virtual int ReturnDestination => -1;      // -1 = main world, int.MinValue = main menu
    public virtual bool ShouldSave => false;          // default already false
    public virtual bool NoPlayerSaving => false;      // default already false — DO NOT set true (see Common Pitfalls)
    public virtual bool NormalUpdates => false;       // vanilla world-tick loop off by default — fine for Phase 1
    public virtual bool ManualAudioUpdates => false;

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void Update() { }                  // between ModSystem.PreUpdateWorld and PostUpdateWorld
    public virtual void OnLoad() { }                  // after subworld generates OR loads from file
    public virtual void OnUnload() { }
    // + CopyMainWorldData/ReadCopiedMainWorldData/CopySubworldData/ReadCopiedSubworldData (ICopyWorldData) —
    //   NOT used in Phase 1 (D-12: no explicit sync of downed flags between worlds)
}
```

Minimal Phase 1 implementation:

```csharp
// Source: SubworldLibrary v2.2.3.2 Subworld.cs (direct source read, HIGH confidence)
using System.Collections.Generic;
using SubworldLibrary;
using Terraria.WorldBuilding;

namespace BossArenaSubWorld.Subworlds
{
    public class BossArenaSubworld : Subworld
    {
        public const int PlatformWidth = 10000;
        public const int WorldHeight = 800;     // total vertical bounds — separate from platform thickness (D-07)

        public override int Width => PlatformWidth;
        public override int Height => WorldHeight;

        public override List<GenPass> Tasks => new()
        {
            new FlatStonePlatformPass("Flat Stone Platform", 1f)
        };

        public override bool ShouldSave => false;      // never persisted — this IS the isolation guarantee
        public override bool NoPlayerSaving => false;  // MUST stay false or inventory/BossCoreItem is lost
    }
}
```

**When to use:** Exactly this project's scratch-arena use case.
**Trade-offs (source-confirmed):** Because `ShouldSave = false`, `SubworldSystem.LoadWorld()` skips `TryLoadWorldFile` entirely and always calls `LoadSubworld()` (regenerate-from-scratch) on every single entry — the arena is rebuilt from your `GenPass` list every time, which is desirable here (guarantees SUBW-05's "zero placed content" invariant can never accumulate across visits) but means any player-placed decoration would never persist either (consistent with D-04's decision to defer decoration).

### Pattern 2: Custom `GenPass` — confirmed exact shape, and why it satisfies SUBW-05

**What:** `GenPass` (namespace `Terraria.WorldBuilding`, vanilla/tModLoader built-in, not from SubworldLibrary) has constructor `GenPass(string name, float loadWeight)` and an abstract `protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)`. Verified against the real `ExampleOrePass` in tModLoader's own `ExampleMod/Content/Tiles/ExampleOre.cs` (1.4.4 branch):

```csharp
// Source: tModLoader ExampleMod, ExampleOre.cs (1.4.4 branch) — pattern confirmed, adapted for a flat platform
public class ExampleOrePass : GenPass
{
    public ExampleOrePass(string name, float loadWeight) : base(name, loadWeight) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "...";
        // ... tile edits via WorldGen.TileRunner / direct Main.tile[x,y] access
    }
}
```

Flat-platform adaptation, using the modern (1.4.4, "data-oriented tiles") `Tile` struct accessor API (`Main.tile[i,j].HasTile` / `.TileType`, not the old `.active()`/`.type` 1.3-era API) — this exact pattern is also independently confirmed in the SubworldLibrary wiki's own example `GenPass`:

```csharp
public class FlatStonePlatformPass : GenPass
{
    public FlatStonePlatformPass(string name, float loadWeight) : base(name, loadWeight) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating boss arena platform";

        int surfaceY = Main.maxTilesY / 2;     // Claude's discretion (D-07) — mid-height, tune as needed
        int thickness = 15;                     // within the 10-20 guideline

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = surfaceY; y < surfaceY + thickness; y++)
            {
                Tile tile = Main.tile[x, y];
                tile.HasTile = true;
                tile.TileType = TileID.GrayBrick == TileID.GrayBrick ? (ushort)TileID.Stone : (ushort)TileID.Stone; // TileID.Stone
            }
        }

        // Recommended: set spawn to just above the platform surface (see "Common Pitfalls" — spawn defaults to
        // exact world center, which will be mid-air/underground unless overridden)
        Main.spawnTileX = Main.maxTilesX / 2;
        Main.spawnTileY = surfaceY - 3;
    }
}
```

**Why this satisfies SUBW-05 (source-confirmed, not assumed):** Read directly from `SubworldSystem.cs` (`LoadSubworld()`, lines ~1516-1578 of the fetched source):

```csharp
// SubworldSystem.cs — LoadSubworld() — confirmed exact call sequence:
Main.maxTilesX = current.Width;
Main.maxTilesY = current.Height;
Main.spawnTileX = Main.maxTilesX / 2;
Main.spawnTileY = Main.maxTilesY / 2;
WorldGen.setWorldSize();
WorldGen.clearWorld();                 // <-- wipes ALL tiles to empty/air first
Main.worldSurface = Main.maxTilesY * 0.3;
Main.rockLayer = Main.maxTilesY * 0.5;
// ... ReadCopiedMainWorldData() (ICopyWorldData — not used here per D-12)

double weight = 0;
for (int i = 0; i < current.Tasks.Count; i++) weight += current.Tasks[i].Weight;
// ... for each task in current.Tasks: task.Apply(progress, config)   <-- ONLY your Tasks list runs. Nothing else.
```

There is no fallback/default vanilla `GenPass` list silently merged in — `current.Tasks` (your `Subworld.Tasks` override) is the *entire* generation pipeline. Combined with `WorldGen.clearWorld()` running first, a single custom `GenPass` that writes only stone tiles guarantees zero vanilla ore/structure/biome placement and zero mod-placed content, satisfying SUBW-05 by construction rather than by convention.

### Pattern 3: `ModCommand` debug entry/exit (D-01/D-02)

**What:** Confirmed via tModLoader's official generated API docs (`ModCommand` class reference): abstract `string Command { get; }`, abstract `CommandType Type { get; }`, virtual `string Description { get; }`/`string Usage { get; }`, abstract `void Action(CommandCaller caller, string input, string[] args)`. Real-world usage pattern confirmed via tModLoader's own built-in `ModlistCommand` (`CommandType.Chat | CommandType.Server | CommandType.Console`).

```csharp
using Terraria.ModLoader;
using SubworldLibrary;
using BossArenaSubWorld.Subworlds;

namespace BossArenaSubWorld.Debug
{
    // DELETE THIS FILE in Phase 2 per D-02 — replaced by the real summon-item redirect
    public class BossArenaEnterCommand : ModCommand
    {
        public override string Command => "bossarena-enter";
        public override CommandType Type => CommandType.Chat;
        public override string Description => "[DEBUG] Enter the boss arena subworld.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            SubworldSystem.Enter<BossArenaSubworld>();
        }
    }

    public class BossArenaExitCommand : ModCommand
    {
        public override string Command => "bossarena-exit";
        public override CommandType Type => CommandType.Chat;
        public override string Description => "[DEBUG] Exit the boss arena subworld.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            SubworldSystem.Exit();
        }
    }
}
```

`CommandType.Chat` works in both singleplayer and multiplayer chat — sufficient for D-13's singleplayer disposable-world testing. `ModCommand` subclasses are auto-discovered by tModLoader the same way other `ModType` content is (no manual registration call needed), consistent with the rest of the mod's pattern.

**Note on `SubworldSystem.Enter<T>()`/`Exit()` semantics (source-confirmed):** Both are effectively no-ops if called while already mid-transition (`Enter` returns `false` if `current != cache`), and both funnel through the same internal `BeginEntering(index)` for singleplayer (`Main.netMode == 0`) — there is no separate "singleplayer path" you need to special-case for Phase 1. `Exit()` sends the player to `current.ReturnDestination`, which defaults to `-1` (main world) — no override needed.

### Pattern 4: Generic biome-zone override hook (D-09)

**What:** A `ModPlayer.PostUpdate()` override that force-sets `Player.Zone*` boolean fields every tick.

**Confirmed correct hook point (source-verified, not assumed):** Fetched and read tModLoader's `patches/tModLoader/Terraria/Player.cs.patch` (1.4.4 branch) directly. It shows:
- Vanilla's own per-tick zone-flag computation (the block that sets fields like `ZoneOverworldHeight` and calls `LoaderManager.Get<BiomeLoader>().UpdateBiomes(this)` for modded biomes) occurs at patch line ~3386, deep inside the main `Update(int i)` method.
- `PlayerLoader.PostUpdate(this)` — the call that invokes every `ModPlayer.PostUpdate()` — occurs at patch line ~4930, strictly later in the same method.
- Official docs independently corroborate this ordering: `PostUpdate()` is documented as "called at the very end of the Player.Update method."

Because the zone-flag computation happens **before** `PostUpdate()` fires in the same tick, overriding `Zone*` fields in `PostUpdate()` reliably wins against vanilla's own recalculation for that tick, and repeats correctly every subsequent tick (this is a per-tick override, not a one-shot patch — it must run every `PostUpdate()` call to keep sticking).

```csharp
using Terraria;
using Terraria.ModLoader;

namespace BossArenaSubWorld.Systems
{
    public class BiomeOverridePlayer : ModPlayer
    {
        // Generic hook (D-09) — no boss-to-biome mapping wired up yet; populated per-boss starting Phase 3+.
        // Example of the shape a future caller would use:
        public static void ForceZone(Player player, System.Action<Player> apply) => apply(player);

        public override void PostUpdate()
        {
            if (!SubworldLibrary.SubworldSystem.IsActive<Subworlds.BossArenaSubworld>())
                return;

            // Phase 1: infrastructure only, no active override applied.
            // Future example (Phase 3+, per-boss): Player.ZoneJungle = true;
        }
    }
}
```

**Trade-off:** Confirmed this must be re-applied every tick, not set once — vanilla recomputes zones every tick regardless of what you set the tick before.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|--------------|-----|
| Subworld creation/entry/exit/save-isolation | A custom "second world" system via manual `WorldFile` manipulation | SubworldLibrary's `Subworld`/`SubworldSystem` | Already handles world-file separation, player-state carryover, spawn setup, and the load/regenerate distinction correctly (all source-verified above); reimplementing this is exactly the kind of "ugly reflection-into-vanilla" work SubworldLibrary's own source comments allude to |
| Detecting "am I in the boss arena" | A custom static bool flag toggled manually in entry/exit code | `SubworldSystem.IsActive<BossArenaSubworld>()` | Already tracks this correctly and is race-safe against SubworldLibrary's own transition state machine |
| World generation pipeline plumbing (progress bar, weighting, RNG seeding per pass) | Custom progress/weight tracking for the platform GenPass | `GenPass` base class + `Subworld.Tasks` | `SubworldSystem.LoadSubworld()` already computes total weight and seeds `WorldGen._genRand`/`Main.rand` per pass; a custom `GenPass` only needs to implement `ApplyPass` |

**Key insight:** Every piece of Phase 1's plumbing (world creation, save isolation, player carryover, generation pipeline) is already provided by SubworldLibrary/vanilla `GenPass`. The only genuinely new code this phase writes is: the platform tile-writing loop, two throwaway chat commands, and a currently-empty per-tick zone-override hook.

## Common Pitfalls

### Pitfall 1: `NoPlayerSaving = true` silently deletes everything gained in the subworld

**What goes wrong:** Setting `Subworld.NoPlayerSaving => true` reloads the player's file data fresh from disk on exit, discarding all inventory/buff changes made during the visit — including anything picked up in the arena.
**Why it happens:** Source-confirmed in `SubworldSystem.cs` (`BeginEntering`/exit path): `if (cache != null && cache.NoPlayerSaving) { PlayerFileData playerData = Player.GetFileData(...); playerData.SetAsActive(); }` — this explicitly re-reads the player file from disk, discarding the live in-memory player object's changes.
**How to avoid:** Leave `NoPlayerSaving` at its default `false` (do not override it, or override it explicitly to `false` for clarity). This is required for SUBW-06 (reliable return with inventory intact) and, from Phase 3 onward, for the `BossCoreItem` carrier pattern to work at all.
**Warning signs:** Items/buffs gained in the subworld vanish immediately upon exit.

### Pitfall 2: Default spawn point lands the player mid-air or underground

**What goes wrong:** `SubworldSystem.LoadSubworld()` sets `Main.spawnTileX = Width/2` and `Main.spawnTileY = Height/2` *before* your `GenPass` runs, as a generic default. If your platform isn't centered at exactly `Height/2`, the player spawns off the platform.
**Why it happens:** This default is a blind midpoint calculation with no awareness of where your custom `GenPass` actually places terrain.
**How to avoid:** Have your `GenPass`'s `ApplyPass` explicitly set `Main.spawnTileX`/`Main.spawnTileY` to a point just above the platform surface, after placing the platform tiles (shown in the Code Examples above).
**Warning signs:** Player falls indefinitely or spawns entombed in stone on first entry.

### Pitfall 3: Assuming the world-flag isolation is a "bug that might get fixed" rather than structural

**What goes wrong:** Treating SubworldLibrary's changelog entries about "world-data sync improvements" (documented in prior `PITFALLS.md` research) as meaning boss flags might now propagate, and skipping the empirical test.
**Why it happens:** SubworldLibrary does have an opt-in generic `ICopyWorldData`/`WorldData` store — but nothing in this phase implements that interface for `NPC.downedSlimeKing`, and even if it did, that store only copies what you explicitly write via `SubworldSystem.CopyWorldData(...)`.
**How to avoid:** This phase intentionally does NOT implement `ICopyWorldData` (D-12) so the test is a true negative-control: confirm `NPC.downedSlimeKing` does NOT survive the round trip with zero explicit sync code written. Mechanistically this is guaranteed by `Subworld.ShouldSave = false` (arena never persists, always regenerates fresh) plus the main world's own file reload on `Exit()` (which restores whatever `NPC.downedSlimeKing` value was already saved in the main world's `.wld`, discarding the in-memory `true` picked up during the subworld visit).
**Warning signs:** N/A for Phase 1 — if the flag *does* propagate unexpectedly, that itself is the actionable finding (re-open the isolation-proof premise before proceeding to Phase 2+).

### Pitfall 4: Building/testing on the real save

**What goes wrong:** Running the King Slime kill test (or any Phase 1 debug-command experimentation) against the player's actual world (`HiPo's_Terrarium`) risks corrupting real progress if something in the still-unproven subworld pipeline misbehaves.
**Why it happens:** Convenience — the real save has all the installed content mods, so it's tempting to "just test there."
**How to avoid:** Per D-13, use a fresh, disposable test world with only SubworldLibrary (and this mod) enabled — Calamity/Spirit/etc. explicitly disabled. Per D-14, still write the VERIFY-02 world-backup guidance doc now, for later phases (4-8) that must test against the real save.
**Warning signs:** N/A — this is a process discipline, not a code symptom.

### Pitfall 5: SDK version mismatch breaking the build silently or with confusing errors

**What goes wrong:** `dotnet build`/`dotnet msbuild` picks the highest installed SDK by default absent a `global.json` pin. This machine has 8.0.424, 9.0.310, and 10.0.201 installed, and `dotnet --version` in the project root currently resolves to **10.0.201**.
**Why it happens:** No `global.json` exists anywhere under `ModSources/` or `BossArenaSubWorld/` to pin the SDK. `tMLMod.targets` hardcodes `<TargetFramework>net8.0</TargetFramework>`, which constrains what framework the *output* targets, but does not control which SDK/toolchain version `dotnet` itself invokes to do the compiling.
**How to avoid:** Add a `global.json` (in `ModSources/BossArenaSubWorld/` or higher) pinning `"sdk": { "version": "8.0.424", "rollForward": "latestFeature" }` (or similar) before the first build in this phase, per CLAUDE.md's explicit "avoid .NET 9.0 and .NET 10.0" directive.
**Warning signs:** Build succeeds when run from Visual Studio (which may use its own bundled toolchain) but fails or behaves inconsistently from `dotnet msbuild` on the command line; or subtle compiler-behavior differences vs. what tModLoader's own CI/other modders experience.

## Code Examples

### World-backup guidance (VERIFY-02 deliverable)

tModLoader's default save locations on Windows (standard, well-established paths — not project-specific):
```
Worlds:  %UserProfile%\Documents\My Games\Terraria\tModLoader\Worlds\
Players: %UserProfile%\Documents\My Games\Terraria\tModLoader\Players\
```
On this machine: `C:\Users\chang\Documents\My Games\Terraria\tModLoader\Worlds\` and `...\Players\`.

Recommended guidance doc content (Claude's discretion on exact location, e.g. `docs/WORLD_BACKUP_GUIDANCE.md`):
1. Before testing any phase against the real save (`HiPo's_Terrarium`), copy both the `.wld` (and its `.bak` if present) from `Worlds\` and the corresponding `.plr` from `Players\` to a separate backup folder, timestamped.
2. Subworld files themselves are stored under `Worlds\<main-world-UniqueId-GUID>\<ModName>_<SubworldClassName>.wld` (confirmed via `SubworldSystem.CurrentPath` source) — but since this phase's `ShouldSave = false`, no subworld file is ever written, so there is nothing to back up for the arena itself.
3. Phase 1 itself does not require this step (D-14) — test only on a disposable world per D-13.

### Empirical isolation-proof test procedure (for the planner to turn into verification steps)

1. On the disposable test world (SubworldLibrary + this mod only), confirm `NPC.downedSlimeKing == false` in the main world (fresh world default).
2. `/bossarena-enter` → confirm player is on the stone platform, not falling/entombed.
3. Summon King Slime for real (vanilla Slime Crown item, or natural Slime Rain if triggered — either is "a real kill" per D-11) and defeat it.
4. Do NOT use any carrier item (none exists yet) — just confirm in-subworld that the kill registered normally (despawn behavior, any in-subworld UI).
5. `/bossarena-exit` → back in the main world.
6. Confirm `NPC.downedSlimeKing == false` in the main world — this is the expected/correct result proving isolation.
7. Confirm inventory is intact (nothing lost from the trip) — proves SUBW-06/`NoPlayerSaving=false` behaves as expected.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|-------------------|---------------|--------|
| 1.3-era `Tile` field-based access (`tile.active(true)`, `tile.type = ...`) | 1.4.4 "data-oriented tiles" struct accessor (`Main.tile[x,y].HasTile = true`, `.TileType = ...`) | tModLoader 1.4.x rework | The `GenPass` code example must use the new accessor pattern — confirmed via both the SubworldLibrary wiki example and tModLoader's own `ExampleOre.cs`, both already using the new API |

No other deprecated/outdated approaches identified as directly relevant to this phase's scope.

## Open Questions

1. **Exact platform Y-level and total subworld `Height`**
   - What we know: `Height` (world vertical bound) is independent from platform thickness (D-07, ~10-20 blocks); `Main.spawnTileY` must be set relative to wherever the platform actually is.
   - What's unclear: No locked value for `Height` or the platform's vertical position — left to Claude's discretion per CONTEXT.md.
   - Recommendation: A `Height` around 600-1000 with the platform placed near the vertical middle gives headroom for boss movement/projectiles above and below without being excessive; not a hard requirement, low risk either way since this is a scratch dimension with `NormalUpdates = false`.

2. **Whether `WorldGen.setWorldSize()` has any hidden assumptions at width ≈ 10,000 (larger than vanilla's largest preset, 8400)**
   - What we know: `LoadSubworld()` calls `WorldGen.setWorldSize()` unconditionally regardless of `Width`/`Height` values, with no min/max validation observed in the traced call path.
   - What's unclear: Whether any derived constant `WorldGen.setWorldSize()` computes (tree density thresholds, etc.) misbehaves at unusual sizes — none of these are exercised by a custom `GenPass` that ignores vanilla tree/ore generation entirely, so this is very low practical risk for Phase 1, but worth a quick sanity check (does the world load without exceptions) during implementation.
   - Recommendation: Treat as low-risk; verify empirically on first successful generation rather than researching further — no evidence of a hard limit found.

3. **Whether King Slime can be summoned without prerequisite world state (e.g. does Slime Crown work with zero NPCs ever spawned in a brand-new subworld)**
   - What we know: Slime Crown is a vanilla item usable to summon King Slime directly, with no biome/prerequisite requirement in vanilla.
   - What's unclear: Not verified against a live subworld instance (would require actual in-game testing, out of scope for research).
   - Recommendation: Plan the verification step assuming Slime Crown works as normal; if it doesn't, natural Slime Rain (75-150 slime kills) is the documented fallback trigger.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|--------------|-----------|---------|----------|
| .NET SDK 8.0.x | Build (`dotnet msbuild`) | ✓ (installed) | 8.0.424 | — |
| .NET SDK (default `dotnet` resolution) | Build invocation | ⚠ resolves to wrong version | 10.0.201 (no `global.json` pin) | Add `global.json` pinning to `8.0.424` before first build (see Common Pitfalls #5) |
| SubworldLibrary (Workshop) | Core dependency (`modReferences`) | ✓ installed | 2.2.3.2, tagged tModLoader 1.4.3/1.4.4 compatible | — |
| tModLoader | Runtime | ✓ (installed at `D:\SteamLibrary\steamapps\common\tModLoader\`) | Confirms `tMLMod.targets` present and `net8.0` TargetFramework hardcoded | — |
| VS Code + C# Dev Kit | Dev environment (per CLAUDE.md) | ✓ VS Code 1.132.1 installed | — | — |

**Missing dependencies with no fallback:**
- None.

**Missing dependencies with fallback:**
- Correct SDK resolution — not literally "missing" (8.0.424 IS installed) but not the *default*-resolved SDK; fallback is a `global.json` pin, a trivial Wave 0 task.

## Validation Architecture

> Included per `.planning/config.json` (`workflow.nyquist_validation: true`, no override for this project).

### Test Framework

This is a tModLoader in-game mod; there is no automated unit-test framework in this codebase or convention in the tModLoader ecosystem for this kind of world-generation/subworld-transition behavior. Verification in this domain is manual, in-game, and observational (chat commands, visual confirmation, checking static field values via a debug print).

| Property | Value |
|----------|-------|
| Framework | None (manual in-game verification — standard for tModLoader mods) |
| Config file | none |
| Quick run command | `dotnet build BossArenaSubWorld.csproj` (compile-check only; catches syntax/type errors, not gameplay correctness) |
| Full suite command | Manual test procedure below, run in-game via tModLoader with the mod loaded |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Verification | File Exists? |
|--------|----------|-----------|---------------|--------------|
| SUBW-05 | Subworld generates with zero placed mod/vanilla content | manual, visual + code-review | Load the arena, confirm only stone-platform tiles exist (fly around, check map); code-review confirms `Tasks` contains exactly one `GenPass` and no vanilla `GenPass` list is referenced | ❌ Wave 0 (no test harness exists — this is inherent to the domain, not a gap to close) |
| SUBW-06 | Player can reliably enter/exit without losing inventory | manual, in-game | `/bossarena-enter`, pick up an item or note current inventory, `/bossarena-exit`, confirm inventory unchanged | ❌ Wave 0 (same as above) |
| VERIFY-02 | World-backup guidance documented and (for later phases) followed | manual, doc review | Guidance doc exists at the chosen location and covers world/player save paths + subworld file location | N/A — documentation deliverable, not a runtime test |

### Sampling Rate
- **Per task commit:** `dotnet build BossArenaSubWorld.csproj` (compile check only)
- **Per wave merge:** Full manual in-game test procedure (isolation-proof test above)
- **Phase gate:** King Slime isolation test must show `NPC.downedSlimeKing == false` in the main world after the round trip, with inventory intact, before this phase is considered verified

### Wave 0 Gaps
- No automated test framework applies to this domain (world generation, subworld transitions, and biome-flag timing cannot be meaningfully unit-tested outside a running tModLoader instance). This is a structural characteristic of tModLoader modding, not a gap to close with tooling in this phase.
- Recommend the executor add a `global.json` SDK pin (see Pitfall 5 / Environment Availability) as an actual Wave 0 setup task, since it is a genuine, closeable gap, unlike test-framework absence.

## Sources

### Primary (HIGH confidence — verified by direct source read, not summarized/secondhand)

- SubworldLibrary v2.2.3.2 `Subworld.cs` — fetched full source via `raw.githubusercontent.com/jjohnsnaill/SubworldLibrary/master/Subworld.cs`, read in full (196 lines). Confirms exact class shape, all virtual/abstract members, XML-doc comments.
- SubworldLibrary v2.2.3.2 `SubworldSystem.cs` — fetched full source (1730 lines), read `Enter`/`Exit`/`IsActive`/`AnyActive`/`MovePlayerToSubworld`/`BeginEntering`/`ExitWorldCallBack`/`LoadWorld`/`LoadSubworld`/`SpawnPlayer` sections directly. Confirms world-regeneration-on-entry behavior when `ShouldSave=false`, spawn-point default calculation, `NoPlayerSaving` player-file-reload behavior, and the exact generation-pass execution loop.
- SubworldLibrary installed copy's `build.txt` (read directly from `D:\SteamLibrary\steamapps\workshop\content\1281930\2785100219\2025.9\`) — confirms version `2.2.3.2` and Workshop tags `1.4.3`/`1.4.4`.
- tModLoader `ExampleMod/Content/Tiles/ExampleOre.cs` (1.4.4 branch) — confirms real, in-repo `GenPass` subclass syntax and constructor signature.
- tModLoader `patches/tModLoader/Terraria/Player.cs.patch` (1.4.4 branch) — confirms relative call order of vanilla zone-flag computation (`LoaderManager.Get<BiomeLoader>().UpdateBiomes(this)`, ~line 3386) vs. `PlayerLoader.PostUpdate(this)` (~line 4930), directly supporting the `ModPlayer.PostUpdate()` recommendation for Pattern 4.
- Local environment probes (`dotnet --list-sdks`, `dotnet --version` in-project, filesystem checks of `ModAssemblies/`, Steam Workshop content folder, `tMLMod.targets`) — all HIGH confidence, directly observed on this machine.
- tModLoader docs.tmodloader.net official generated API references for `GenPass`, `ModCommand`, `ModPlayer` — official generated documentation, cross-checked against source where possible.

### Secondary (MEDIUM confidence)
- `NPC.downedSlimeKing` field existence/name — confirmed via `docs.tmodloader.net/docs/1.4-stable/functions_vars_d.html` (official generated docs, class `Terraria.NPC`) and cross-checked against a vanilla source mirror's field declarations (which, notably, did NOT contain this field, indicating that mirror is from an older pre-1.4 Terraria version — the official 1.4-stable docs entry is the authoritative, current source here).
- SubworldLibrary wiki example `GenPass` (fetched via WebFetch summarization, not raw diff) — consistent with the directly-verified `ExampleOre.cs` pattern, corroborating rather than sole source.

### Tertiary (LOW confidence, flagged)
- Vanilla Slime Crown/King Slime summon mechanics (no-prerequisite summoning) — standard, long-established game knowledge, not independently re-verified against current source in this research pass; low risk since it's easily observable in-game during Phase 1 execution itself.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — SubworldLibrary version and installation directly confirmed on-disk; SDK versions directly confirmed via `dotnet --list-sdks`.
- Architecture (Subworld/GenPass/ModCommand/PostUpdate patterns): HIGH — all four patterns verified against actual fetched source (SubworldLibrary source, tModLoader ExampleMod source, tModLoader Player.cs.patch), not training-data recall alone.
- Pitfalls: HIGH for the SDK-version and `NoPlayerSaving`/spawn-point pitfalls (source-confirmed); MEDIUM for the King-Slime-summon-mechanics assumption (standard game knowledge, not re-verified in this pass).

**Research date:** 2026-08-13
**Valid until:** ~30 days (stable APIs — SubworldLibrary and tModLoader 1.4.4 are both in maintenance-level release cadence, not actively churning); re-verify if SubworldLibrary or tModLoader receives an update before Phase 1 implementation begins.
