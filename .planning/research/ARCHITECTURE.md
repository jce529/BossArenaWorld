# Architecture Research

**Domain:** tModLoader mod — dedicated boss-arena subworld with cross-mod boss-kill detection and carrier-item state replay
**Researched:** 2026-08-12
**Confidence:** MEDIUM (SubworldLibrary and core tModLoader interop patterns are HIGH confidence from official wiki/docs; per-target-mod integration specifics — Calamity/Spirit/Redemption/etc. — remain LOW/unresearched per PROJECT.md and are out of scope for this file)

## Standard Architecture

### System Overview

```
┌──────────────────────────────────────────────────────────────────────┐
│                         Main World (persistent)                      │
├──────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────┐   ┌───────────────────┐   ┌────────────────────┐│
│ │ Entry Item / NPC │   │ BossCoreItem       │   │ Per-mod Downed     ││
│ │ (trigger Enter)  │   │ (ModItem, carries  │──▶│ Flags / WorldGen   ││
│ │                  │   │  boss key as       │   │ (Calamity, Spirit, ││
│ │                  │   │  instance data)    │   │  Redemption, ...)  ││
│ └────────┬─────────┘   └───────────▲────────┘   └────────────────────┘│
│          │ SubworldSystem.Enter<>() │ use/right-click → Apply(key)    │
├──────────┼───────────────────────────┼─────────────────────────────────┤
│          ▼                           │            SubworldLibrary      │
│ ┌──────────────────────────────────────────────────────────────────┐ │
│ │              BossArenaSubworld (Subworld subclass)                │ │
│ │  - empty/minimal generation, no mod-placed content                │ │
│ │  - hosts the actual boss fight                                    │ │
│ └───────────────────────────────┬──────────────────────────────────┘ │
│                                  │ NPC.Kill()                          │
│                                  ▼                                    │
│ ┌──────────────────────────────────────────────────────────────────┐ │
│ │   BossKillGlobalNPC.OnKill()  →  BossRegistry.TryGetDefinition()  │ │
│ │        │ (only when subworld active)                              │ │
│ │        ▼                                                          │ │
│ │   spawn BossCoreItem tagged with boss key                         │ │
│ └──────────────────────────────────────────────────────────────────┘ │
├──────────────────────────────────────────────────────────────────────┤
│                     BossRegistry (ModSystem, static table)            │
│  key → { NPC type(s), Apply() delegate, side-effect delegate }        │
├──────────────────────────────────────────────────────────────────────┤
│   Per-mod Integration Classes (one per source mod, isolated files)    │
│   CalamityIntegration | SpiritIntegration | RedemptionIntegration |   │
│   CatalystIntegration | NoxusBossIntegration | DaybreakIntegration    │
│   — each self-registers into BossRegistry, each wrapped for safe      │
│     loading when its target mod isn't installed                       │
└──────────────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility | Typical Implementation |
|-----------|----------------|-------------------------|
| `BossArenaSubworld` | Defines the empty arena dimension — size, generation (near no-op), save behavior | `class BossArenaSubworld : Subworld` in SubworldLibrary, overrides `Size`, `Tasks` (GenPass list), `ShouldSave`, `NoPlayerSaving` |
| Subworld entry/exit trigger | Player-facing way to start/stop a boss run | `ModItem` or `NPC`/tile interaction calling `SubworldSystem.Enter<BossArenaSubworld>()`; return trip via `SubworldSystem.Exit()` |
| `BossKillGlobalNPC` | Detects a registered boss dying inside the arena subworld, converts the kill into a carrier item | `GlobalNPC.OnKill(NPC npc)` override, gated by `SubworldSystem.IsActive<BossArenaSubworld>()` |
| `BossRegistry` | Central lookup: NPC identity → boss key → apply/side-effect logic. The single seam every other component depends on | `ModSystem` (or static class populated in `PostSetupContent`) holding `Dictionary<string, BossDefinition>` |
| `BossCoreItem` | Carries the boss key as per-instance data from subworld kill back to main-world use | `ModItem` with `CloneNewInstances = true`, `SaveData`/`LoadData`/`Clone` persisting a string/int key |
| Per-mod integration classes | Translate one source mod's actual downed-flag API (property setter, raw static field, etc.) into a `BossRegistry` registration, including that mod's netcode/WorldGen side effects | One class per mod, isolated behind `[JITWhenModsEnabled("ModName")]` + `ModLoader.HasMod()` guard, or `Mod.Call` if the target mod exposes one |

## Recommended Project Structure

```
BossArenaSubWorld/
├── BossArenaSubWorld.cs           # Mod entry class
├── Subworlds/
│   └── BossArenaSubworld.cs       # Subworld definition (empty arena)
├── Systems/
│   ├── BossRegistry.cs            # ModSystem: key → BossDefinition table + Apply()
│   └── SubworldEntrySystem.cs     # optional: shared enter/exit helpers, state flags
├── GlobalNPCs/
│   └── BossKillGlobalNPC.cs       # OnKill hook, subworld-gated
├── Items/
│   └── BossCoreItem.cs            # carrier item, instance data, UseItem→Apply
├── Integrations/                  # one file per source mod, all isolated
│   ├── CalamityIntegration.cs
│   ├── SpiritIntegration.cs
│   ├── RedemptionIntegration.cs
│   ├── CatalystIntegration.cs
│   ├── NoxusBossIntegration.cs
│   └── DaybreakIntegration.cs
└── build.txt                      # weakReferences = CalamityMod, SpiritMod, ...
```

### Structure Rationale

- **`Subworlds/`:** SubworldLibrary auto-registers any `Subworld` subclass found in the mod — keeping it isolated makes it trivial to find and matches SubworldLibrary example-mod conventions.
- **`Systems/BossRegistry.cs`:** This is the seam the whole mod hinges on (per PROJECT.md Core Value). Isolating it means `GlobalNPC` and `ModItem` code never needs to know which source mod a boss came from — they only talk to the registry's key-based API.
- **`Integrations/`:** Each source mod's API shape is different (Calamity: wrapper properties with side-effecting setters; Spirit: raw static fields; others: unresearched). One file per mod keeps a broken/updated mod's integration from touching unrelated code, and lets each be built, tested, and shipped independently once the skeleton (Registry + GlobalNPC + Item) is proven with one low-risk boss.
- **`build.txt` weakReferences:** Every integration that compiles directly against another mod's types (rather than pure string-based reflection) must be declared as a `weakReferences` entry, or the mod fails to load when that target mod is absent.

## Architectural Patterns

### Pattern 1: Subworld as an isolated, no-save dimension

**What:** `Subworld` subclass overriding `Size`, `Tasks` (world-gen passes — can be a near-empty list for a flat/void arena), `ShouldSave` (whether tile edits persist between visits), and `NoPlayerSaving` (whether player stat/buff changes made inside revert on exit).
**When to use:** Any time you need a scratch dimension that must never accumulate mod-placed content (the entire premise of this project — avoiding the FPS collapse caused by dense modded world content).
**Trade-offs:** A fresh/void arena means bosses that expect specific terrain (e.g., some need liquid, platforms, or biome checks) may misbehave — this needs per-boss verification, not just per-mod. Also: `ShouldSave`/`NoPlayerSaving` semantics directly gate whether the `BossCoreItem` the player picks up actually survives the trip back to the main world — get this wrong and the whole pipeline silently fails at the exit boundary, not at the kill or apply step.

**Example:**
```csharp
public class BossArenaSubworld : Subworld
{
    public override int Width => 800;
    public override int Height => 600;
    public override List<GenPass> Tasks => new() { /* minimal/flat generation */ };
    public override bool ShouldSave => false;      // arena resets each visit
    public override bool NoPlayerSaving => false;   // player must KEEP inventory (the carrier item)
}
```

### Pattern 2: Central registry as the only cross-cutting seam

**What:** A single `BossRegistry` mapping a stable string/int key to a `BossDefinition` record `{ NpcType(s), Apply(), OnWorldGenSideEffect() }`. `GlobalNPC.OnKill` only ever asks "is this NPC type registered?"; `BossCoreItem` only ever calls "Apply(this.BossKey)". Neither touches any source mod's API directly.
**When to use:** Whenever multiple heterogeneous external systems (7 different content mods with 7 different downed-flag APIs) must be normalized behind one interface consumed by generic pipeline code.
**Trade-offs:** Adds one layer of indirection, but this is exactly what makes the pipeline "reliably reproduce a boss's full downed state for any registered boss" (per PROJECT.md Core Value) — without it, `GlobalNPC`/`ModItem` code would need a growing chain of per-mod `if` branches, which is fragile and hard to extend.

**Example:**
```csharp
public record BossDefinition(int[] NpcTypes, Action ApplyDowned);

public class BossRegistry : ModSystem
{
    private static readonly Dictionary<string, BossDefinition> _byKey = new();
    private static readonly Dictionary<int, string> _npcTypeToKey = new();

    public static void Register(string key, BossDefinition def)
    {
        _byKey[key] = def;
        foreach (int t in def.NpcTypes) _npcTypeToKey[t] = key;
    }

    public static bool TryGetKeyForNpc(int npcType, out string key) =>
        _npcTypeToKey.TryGetValue(npcType, out key);

    public static void Apply(string key) => _byKey[key].ApplyDowned();
}
```

### Pattern 3: Weak references + `[JITWhenModsEnabled]` for cross-mod static access

**What:** Compile directly against another mod's public types/members (obtained from that mod's built `.dll`, referenced via `<Reference>` in the `.csproj` and declared as `weakReferences = ModName` in `build.txt`), but wrap every call site in a method/property tagged `[JITWhenModsEnabled("ModName")]` so the JIT never resolves those types unless the mod is actually loaded at runtime. Always guard the call site itself with `ModLoader.TryGetMod("ModName", out _)` or `ModLoader.HasMod("ModName")`.
**When to use:** This is the tModLoader-recommended approach for "deep integration with type safety" when the referenced mod is optional but core to functionality — exactly this project's situation with Calamity (`DownedBossSystem` wrapper properties), Spirit (`MyWorld` static fields), and the other unresearched mods.
**Trade-offs:** Requires obtaining each target mod's compiled `.dll` before writing its integration file (a **build-order dependency**: you cannot write/compile `CalamityIntegration.cs` until Calamity's dll is available as a project reference). Raw `System.Reflection` (string-based `Assembly.GetType`/`GetField`) is the fallback only for private members or mods where no reference dll is available — the tModLoader wiki explicitly calls plain reflection for cross-mod content "fragile" and "a bad approach" compared to weak references. `Mod.Call` is a third option but only works if the *target* mod explicitly implements a `Call` handler; none of Calamity/Spirit's known APIs (direct property/field access) suggest they do.

**Example:**
```csharp
// CalamityIntegration.cs
public static class CalamityIntegration
{
    public static void Register()
    {
        if (!ModLoader.HasMod("CalamityMod")) return;
        BossRegistry.Register("calamity:desert_scourge", new BossDefinition(
            NpcTypes: GetDesertScourgeTypes(),
            ApplyDowned: ApplyDesertScourgeDowned));
    }

    [JITWhenModsEnabled("CalamityMod")]
    private static int[] GetDesertScourgeTypes() =>
        new[] { ModContent.NPCType<CalamityMod.NPCs.DesertScourge.DesertScourgeHead>() };

    [JITWhenModsEnabled("CalamityMod")]
    private static void ApplyDesertScourgeDowned()
    {
        CalamityMod.World.DownedBossSystem.downedDesertScourge = true; // wrapper property, has side-effecting setter
        CalamityMod.CalamityNetcode.SyncWorld();
        // + any CalamityGlobalNPC.SetNewBossJustDowned() equivalent, per PROJECT.md
    }
}
```

## Data Flow

### Boss-kill-to-apply flow

```
Player uses entry item/NPC in main world
    ↓
SubworldSystem.Enter<BossArenaSubworld>()
    ↓
[BossArenaSubworld active] player fights boss (mod content/AI runs as normal — only the *world* is empty)
    ↓
NPC dies → NPC.checkDead() → BossKillGlobalNPC.OnKill(npc)
    ↓ (gated: SubworldSystem.IsActive<BossArenaSubworld>())
BossRegistry.TryGetKeyForNpc(npc.type, out key)
    ↓ (match found)
spawn BossCoreItem instance, SetKey(key) → item.SaveData persists key in TagCompound
    ↓
Player picks up item, exits subworld → SubworldSystem.Exit()
    ↓ (NoPlayerSaving/inventory must survive this transition — verify in Phase: subworld skeleton)
Player back in main world, inventory contains BossCoreItem
    ↓
Player uses/right-clicks BossCoreItem → ModItem hook reads instance key
    ↓
BossRegistry.Apply(key) → per-mod integration's ApplyDowned()
    ↓
source mod's downed flag set + its netcode sync call + any WorldGen side effect (ore gen, dungeon activation, etc.)
    ↓
item consumed
```

### Registration flow (mod load time, one-directional, no runtime cost per kill)

```
Mod.Load() / PostSetupContent()
    ↓
CalamityIntegration.Register() ─┐
SpiritIntegration.Register()    ─┤
RedemptionIntegration.Register()─┼──▶ BossRegistry._byKey / _npcTypeToKey populated once
CatalystIntegration.Register()  ─┤
NoxusBossIntegration.Register() ─┤
DaybreakIntegration.Register()  ─┘
```

### Key Data Flows

1. **Kill → carrier item:** One-way, subworld-scoped. `BossKillGlobalNPC` never talks to a source mod directly — it only queries `BossRegistry` by NPC type. This keeps the kill-detection hook generic across all 7+ target mods.
2. **Carrier item → main-world apply:** One-way, main-world-scoped, triggered by explicit player action (matches PROJECT.md's "manual" design philosophy — no automatic subworld-to-main sync, since that's the very SubworldLibrary limitation this mod works around).
3. **Registration → lookup:** Static, populated once at load, read many times (once per boss kill and once per item use) — no need for thread safety or lazy invalidation.

## Scaling Considerations

Not a traditional user-scaling problem — the relevant "scale" axis here is **number of registered bosses / source mods** (2 researched so far, growing to 7+ per PROJECT.md).

| Scale | Architecture Adjustments |
|-------|---------------------------|
| 1-2 source mods (Calamity, Spirit) | Flat `Dictionary<string, BossDefinition>` in `BossRegistry` is sufficient; per-mod integration files can be written and tested serially |
| 3-5 source mods (+ Redemption, CatalystMod, NoxusBoss) | Still flat dictionary; the only added cost is per-mod research time (API shape unknown for each), not architecture — this is why PROJECT.md's "no priority ordering" decision holds: registration mechanics don't get more complex, they just repeat |
| 6-7+ source mods (+ ContinentOfJourney/Daybreak) | Watch for **NPC type collisions** if two mods reuse a type id space unexpectedly (unlikely in tModLoader since types are assigned per-mod at runtime, but worth a defensive check/log in `BossRegistry.Register` if a key or npcType is registered twice) |

### Scaling Priorities

1. **First bottleneck:** Build-order fragility — each weakly-referenced integration needs that mod's dll available at compile time. If a target mod updates and changes its API (e.g., Calamity renames `DownedBossSystem`), only that one integration file breaks — isolate for exactly this reason.
2. **Second bottleneck:** Side-effect completeness, not raw scale — PROJECT.md flags that under-reproducing a mod's `OnKill` side effects (netcode sync, WorldGen triggers) breaks other systems that key off those flags (e.g. vanilla Lantern Night). This is a per-boss correctness risk, not an architectural one — mitigate by keeping each integration's `ApplyDowned()` as a faithful line-by-line replay of the source mod's actual `OnKill`, not a shortcut.

## Anti-Patterns

### Anti-Pattern 1: Branching on source mod inside `GlobalNPC`/`ModItem`

**What people do:** Put `if (npc.ModNPC?.Mod.Name == "CalamityMod") { ... } else if (... == "SpiritMod") { ... }` directly inside `OnKill` or the item's use hook.
**Why it's wrong:** Couples the generic pipeline to every source mod's internals, makes `OnKill`/`UseItem` grow linearly with mod count, and violates the "generic mechanism must work for any registered boss" Core Value in PROJECT.md.
**Do this instead:** `BossRegistry` lookup only. All mod-specific knowledge lives in `Integrations/`.

### Anti-Pattern 2: Reflection everywhere instead of weak references where a dll is obtainable

**What people do:** Use `Assembly.GetType("CalamityMod.World.DownedBossSystem")` + `FieldInfo`/`PropertyInfo` string lookups for every cross-mod call, to "avoid compile-time dependency."
**Why it's wrong:** tModLoader's own wiki calls this fragile — it breaks silently (no compile error) when the target mod renames/moves a member, and it's slower and harder to debug than compiled access.
**Do this instead:** Add the target mod as a `weakReferences` entry with a project reference to its dll, wrap access in `[JITWhenModsEnabled]`, and let the compiler catch breakage when rebuilding against an updated dll. Reserve raw reflection for members that are genuinely private/internal or for mods where no reference dll can be sourced.

### Anti-Pattern 3: Setting downed flags as booleans without replaying side effects

**What people do:** Treat "apply boss downed" as `SomeMod.downedX = true;` and stop there.
**Why it's wrong:** PROJECT.md explicitly documents that Calamity's flags are wrapper properties whose setters trigger `CalamityNetcode.SyncWorld()`/`SetNewBossJustDowned()`, and that world-altering bosses need WorldGen side effects (ore gen, dungeon activation) replayed too. Skipping these leaves the flag set but dependent systems (vanilla events, mod-internal caches) unaware.
**Do this instead:** Each integration's `ApplyDowned()` should call the *same* code path the source mod's own `OnKill` calls, not just assign the flag.

### Anti-Pattern 4: Relying on SubworldLibrary's automatic save/sync for the downed flag

**What people do:** Assume killing a boss in a subworld will naturally propagate to the main world's save file on exit.
**Why it's wrong:** PROJECT.md documents this as a known SubworldLibrary-ecosystem bug — downed flags are serialized per-world-file and unconditionally overwritten on world load, so subworld kills do not propagate automatically. This is precisely why the carrier-item pattern exists.
**Do this instead:** Treat the subworld and main world as fully separate save states connected only by the `BossCoreItem` the player physically carries in inventory across the transition.

## Integration Points

### External Mods (weak dependencies)

| Mod | Integration Pattern | Notes |
|-----|----------------------|-------|
| SubworldLibrary | Strong/mod reference (required, not weak) — `Subworld` base class, `SubworldSystem.Enter<T>()`/`Exit()` | Foundation dependency; must be a real `modReferences` entry, not weak, since the whole subworld mechanic depends on it |
| CalamityMod | Weak reference + `[JITWhenModsEnabled]`; `DownedBossSystem` wrapper properties, `CalamityNetcode.SyncWorld()`, `CalamityGlobalNPC.SetNewBossJustDowned()` | API shape confirmed per PROJECT.md research |
| SpiritMod | Weak reference + `[JITWhenModsEnabled]`; `MyWorld` plain public static bool fields | PROJECT.md notes version may have moved from `ModWorld` to `ModSystem` — recheck against installed copy before writing integration |
| Redemption, CatalystMod, NoxusBoss, ContinentOfJourney, Daybreak | Unresearched — API shape unknown | Each needs its own research pass (per-mod decompile/source read) before an integration file can be written; treat as a per-mod research spike, same shape as Calamity/Spirit research already done |
| Infernum / Wrath of the Gods | No separate integration needed | These rework existing boss AI on top of Calamity/vanilla bosses rather than adding new downed flags — covered automatically once the underlying boss is registered, per PROJECT.md |

### Internal Boundaries

| Boundary | Communication | Notes |
|----------|----------------|-------|
| Subworld entry trigger ↔ `BossArenaSubworld` | `SubworldSystem.Enter<T>()` / `Exit()` (SubworldLibrary API) | No custom data channel needed here — this boundary is pure navigation |
| `BossKillGlobalNPC` ↔ `BossRegistry` | Direct static method call, read-only lookup | Must be subworld-gated (`SubworldSystem.IsActive<BossArenaSubworld>()`) so ordinary main-world kills of the same NPC types don't also spawn carrier items |
| `BossCoreItem` ↔ `BossRegistry` | Direct static method call (`Apply(key)`), triggered from a use/right-click hook | Item must carry the key through `SaveData`/`LoadData` (TagCompound) and `Clone` (with `CloneNewInstances = true`) so the key survives serialization and item-instance duplication |
| `BossRegistry` ↔ per-mod `Integrations/*` | Registration calls at load time (`Register()` static methods invoked from mod's `Load`/`PostSetupContent`) | One-directional: integrations push definitions in; registry never calls back into integration code except via the stored delegate |
| Integration classes ↔ target mod dlls | Compiled access via weak reference + `[JITWhenModsEnabled]` (primary) or raw reflection (fallback) | Build-order dependency: target mod's dll must be available as a project reference before the integration file compiles |

## Sources

- [SubworldLibrary Wiki (jjohnsnaill/SubworldLibrary)](https://github.com/jjohnsnaill/SubworldLibrary/wiki) — Subworld class structure, `OnEnter`/`OnLoad`/`OnExit`/`OnUnload`, `ShouldSave`/`NoPlayerSaving`, `SubworldSystem.Enter`/`Exit`/`IsActive`/`AnyActive` — MEDIUM confidence (fetched via summarized WebFetch, not directly verified against source code)
- [tModLoader Wiki: Expert Cross-Mod Content](https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content) — weak references, `build.txt` `weakReferences`, `[JITWhenModsEnabled]`, `ModLoader.HasMod`/`TryGetMod`, comparison of weak references vs `Mod.Call` vs reflection — HIGH confidence (official tModLoader wiki)
- [tModLoader: GlobalNPC Class Reference](https://docs.tmodloader.net/docs/stable/class_global_n_p_c.html) — `OnKill` hook — HIGH confidence (official API docs)
- [BossChecklist GitHub — BossChecklistIntegrationExample.cs](https://github.com/JavidPack/BossChecklist/blob/1.4/BossChecklistIntegrationExample.cs) — real-world example of `ModLoader.TryGetMod` + `Mod.Call` weak-reference cross-mod pattern — HIGH confidence (widely-used community mod, official example integration file)
- [tModLoader Wiki: Saving and loading using TagCompound](https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound) — `SaveData`/`LoadData` pattern for `ModItem` instance data — HIGH confidence (official wiki)
- ExampleMod `ExampleInstancedItem`/`CloneNewInstances` pattern (referenced via search, not directly fetched) — MEDIUM confidence; recommend verifying directly against `tModLoader/tModLoader` GitHub `ExampleMod/Items/` during implementation
- `.planning/PROJECT.md` — project-specific facts about Calamity `DownedBossSystem`, Spirit `MyWorld`, and the SubworldLibrary downed-flag propagation bug — project source of truth

---
*Architecture research for: tModLoader boss-arena subworld mod*
*Researched: 2026-08-12*
