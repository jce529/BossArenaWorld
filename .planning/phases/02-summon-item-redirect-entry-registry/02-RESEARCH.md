# Phase 2: Summon-Item Redirect & Entry Registry - Research

**Researched:** 2026-08-13
**Domain:** tModLoader modding — custom `ModTile` right-click interaction, item-use-effect replay, `SubworldLibrary` entry-timing hooks
**Confidence:** HIGH for the core mechanisms (all verified against tModLoader's own source/patch files and two independently-sourced decompiled Terraria snapshots that agree); MEDIUM for texture-authoring workflow and multitile registration specifics (not independently source-verified this session, but consistent with well-established tModLoader convention)

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**Entry mechanism — portal tile (supersedes original "item-only" design)**
- **D-01:** The original PROJECT.md decision ("existing summon items alone are the trigger, no separate portal item") is explicitly reversed for this phase, per user request during discussion. See PROJECT.md Key Decisions table for the superseded/superseding rows.
- **D-02:** A new placeable tile/furniture object, working name `Test1` (internal name only — NOT final, rename before ship), is the subworld's portal object.
- **D-03:** `Test1`'s appearance is a **brand-new custom `ModTile`** that visually benchmarks the Corruption Altar sprite (texture reused/referenced for visual similarity only). It must NOT reuse the actual vanilla Demon Altar/Crimson Altar tile type — no inherited vanilla altar behavior (hammer-smash hardmode trigger, "a horrible chill..." message, altar-crafting-recipe unlocks, etc.). This is a from-scratch tile with only our own interaction logic attached.
- **D-04:** Interaction trigger: player right-clicks the placed `Test1` tile while holding a registered boss-summon item in hand. This is the sole redirect trigger for this phase — direct use of the summon item elsewhere is untouched and keeps its normal vanilla/modded main-world behavior.
- **D-05:** Acquisition for this phase: `Test1` has no crafting recipe. It's obtained via the Creative menu / debug-only means (consistent with Phase 1's debug-tooling pattern) since the internal name and final itemization aren't decided yet.

**Summon-item registry scope**
- **D-06:** SUBW-01's central registry is data-driven/extensible in shape, but only needs one populated entry for this phase's proof.
- **D-07:** Registry scope for v1 of this phase is limited to **simple "use item to summon" style items** (e.g. Slime Crown, Suspicious Looking Eye) — NOT structurally different triggers like altar-thrown items (Guide Voodoo Doll) or bulb-break summons (Plantera). Those remain out of this phase's scope; revisit when/if a boss needing them is registered in a later phase.

**Proof boss**
- **D-08:** King Slime, via Slime Crown, is the boss/item used to prove this phase's mechanism — continuity with Phase 1's isolation-proof test, and Slime Crown is a non-consumable item so SUBW-04's "item not consumed" requirement is trivially satisfied for this proof.

**Boss auto-summon mechanism**
- **D-09:** Once the player arrives in the subworld, the boss is summoned by **replaying (re-triggering) the same held summon item's own use-effect** inside the subworld — not bespoke per-boss spawn code. This generalizes cleanly to any future item registered under D-07's scope, since "replay the item's use effect" works identically regardless of which specific boss the item summons.
- **D-10:** No specific spawn position/timing logic is needed beyond "immediately after arrival, in the subworld" — the existing 10,000-block-wide flat platform (Phase 1) is wide open, so there's no positioning concern to solve.

**Redirect feedback**
- **D-11:** A simple chat message is shown to the player at the moment of redirect (e.g. confirming they're being sent to the boss arena). No screen-transition effects or sound cues — those are explicitly out of this phase's scope (would be UX polish, not core mechanism).

### Claude's Discretion
- Exact chat message wording for D-11
- `Test1` tile's exact texture-reuse implementation approach (e.g. `ModContent.Request` against the vanilla altar's asset path vs. a copied sprite file) — technical detail, not a user-facing decision
- Exact mechanism for "replaying" a held item's use-effect in the subworld (e.g. calling the item's `UseItem`/`UseStyle` logic directly vs. another approach) — implementation detail for research/planning
- Tile placement rules (where in the main world the player may place `Test1`, light source, break/interaction sounds, etc.) beyond "visually similar to Corruption Altar"

### Deferred Ideas (OUT OF SCOPE)
- **Non-"simple-use" summon triggers** (altar-thrown items like Guide Voodoo Doll, bulb-break like Plantera) — explicitly out of this phase's registry scope (D-07). Revisit when a boss needing one of these trigger types is actually registered (likely Phase 4+ as Calamity/other mods' bosses come online).
- **Screen-transition effects / sound cues on redirect** — considered and explicitly deferred in favor of a simple chat message (D-11). Could be revisited as UX polish later, but isn't blocking any requirement.
- **`Test1`'s final name/itemization/crafting recipe** — explicitly deferred; this phase only needs a Creative-menu-obtainable placeholder to prove the mechanism (D-05).
- **Removing Phase 1's debug commands** — Phase 1's CONTEXT.md (D-02) says they're deleted once the real redirect lands; whether that removal happens inside this phase's plan or a small follow-up should be confirmed during planning, not assumed here.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| SUBW-01 | Central registry maps registered boss-summon items (vanilla or modded, simple "use-to-summon" only) to their target boss, keyed for redirect purposes | See "Architecture Patterns → Pattern 1: Item-Type-Keyed Summon Registry" and "Don't Hand-Roll" — registry design confirmed to generalize across the vanilla `ItemCheck_UseBossSpawners` family without per-item bespoke code |
| SUBW-02 | New placeable portal tile (`Test1`/`BossPortalTile`), custom `ModTile`, no inherited altar behavior; right-click while holding a registered item triggers redirect | See "Code Examples → ModTile.NewRightClick hook" and "Common Pitfalls → Pitfall 1 (deprecated RightClick), Pitfall 5 (texture authoring)" |
| SUBW-03 | Redirect sends player into the boss-arena subworld as the tile interaction's next step | See "Code Examples → Redirect trigger in NewRightClick" — confirmed `SubworldSystem.Enter<BossArenaSubworld>()` is the correct, already-proven call (Phase 1 debug command uses it identically) |
| SUBW-04 | Boss auto-summons on arrival by replaying the held item's use-effect; item not consumed | See "Architecture Patterns → Pattern 2: Generic Replay via NPC.SpawnOnPlayer" and "Common Pitfalls → Pitfall 2 (wrong entry hook), Pitfall 3 (item consumption risk)" — this is the highest-research-value finding of this document |
</phase_requirements>

## Project Constraints (from CLAUDE.md)

- **Tech stack lock:** tModLoader 1.4.4.9, .NET 8.0 SDK only (never 9/10), `dotnet msbuild`/`dotnet build` — already satisfied by the existing project (`global.json` pins `8.0.424`, confirmed present this session).
- **SubworldLibrary is a strong `modReferences` dependency** (already declared in `build.txt`), not weak — no change needed for this phase; `SubworldSystem.Enter<T>()`/`Exit()` are the only APIs this phase touches.
- **No hand-rolled `.csproj`** — project scaffolding already exists and is committed; this phase only adds new source files (`Tiles/`, `Items/`, `Systems/` or similar), no scaffold regeneration needed.
- **Never write directly to a boss's raw downed-flag backing field without going through its intended setter/side-effect path** — not directly relevant to Phase 2 (no flag-setting happens in this phase; that's Phase 3/4), but the same discipline applies by extension to NOT hand-rolling a bespoke "spawn King Slime" method when a generic, vanilla-idiomatic spawn call (`NPC.SpawnOnPlayer`) already exists and is what the source mod's/vanilla's own item-use code calls internally.
- **GSD workflow enforcement:** all file edits for this phase must happen through `/gsd:execute-phase`, not ad hoc.
- **Communication:** all chat/status output to the user must be in Korean; code, comments, and commit messages stay in English (existing codebase convention, unaffected by this research).

## Summary

This phase adds two new tModLoader content types the codebase has never used before (`ModTile`, and a placing `ModItem` for it) plus a small new registry (`ModSystem` or static class) and one new `ModPlayer` hook. All four are well-documented, stable tModLoader 1.4 APIs — no exotic or deprecated patterns are needed except one to actively avoid (the old, deprecated `ModTile.RightClick(int,int)` void-returning hook — use `NewRightClick(int,int)` instead, which is misleadingly *not* named with "New" in its role, just historically renamed and never fully cleaned up).

The single most important finding from this research session is **how "replay the item's use-effect" should actually be implemented**. Direct inspection of decompiled vanilla source (two independent snapshots, cross-verified) shows that vanilla boss-summon items like Slime Crown do **not** have an isolated, callable "use effect" method — their summon logic lives inside a large private `Player.ItemCheck_UseBossSpawners` method, keyed by hardcoded `item.type` magic numbers, that itself just calls `NPC.SpawnOnPlayer(playerIndex, bossNpcType)` after checking item-specific gates (night-time for Suspicious Looking Eye, biome zone for Worm Food, etc.). There is no clean, low-risk way to "re-invoke" that private method for a vanilla item. The correct generalization — and the one that actually satisfies D-09's *intent* ("no bespoke per-boss spawn code") — is to have the registry store **which boss NPC type** each item summons, and have the generic auto-summon logic call `NPC.SpawnOnPlayer(player.whoAmI, bossNpcType)` directly. This is not a workaround; it is *the exact same call* vanilla's own summon items make, and it is also the standard, documented pattern modded boss-summon items use in their own `ModItem.UseItem` overrides. One generic call handles every item in D-07's registry scope, present and future, without per-boss branching.

The second key finding concerns **timing**: the codebase's existing `Subworld.OnEnter()` override (used in Phase 1 for the downed-flag snapshot/restore fix) fires *before* the subworld has finished generating and *before* the player has been repositioned into it — it is the wrong hook for spawning an NPC near the player. Direct source-read of `SubworldSystem.cs`'s `ExitWorldCallBack`/`LoadWorld`/`SpawnPlayer` call chain shows the correct, safe hook is `ModPlayer.OnEnterWorld(Player player)` (the same vanilla event, `Player.Hooks.OnEnterWorld`, that `SubworldLibrary` itself hooks into), gated by `SubworldSystem.AnyActive<BossArenaSubworld>()`. At that point the subworld's terrain is fully generated and the player's position is valid, so `NPC.SpawnOnPlayer` can find a legal spawn point.

**Primary recommendation:** Build `Test1` as a from-scratch `ModTile` + placing `ModItem` pair; gate its `NewRightClick` hook on the held item being present in a new `Dictionary<int, int>` (itemType → bossNpcType) registry; on a match, snapshot the target boss NPC type into a static field, show the chat message, and call `SubworldSystem.Enter<BossArenaSubworld>()`; consume the snapshot and call `NPC.SpawnOnPlayer(player.whoAmI, snapshottedBossType)` from a `ModPlayer.OnEnterWorld` override gated on `SubworldSystem.AnyActive<BossArenaSubworld>()`.

## Standard Stack

### Core

| Component | tModLoader API | Purpose | Why Standard |
|-----------|-----------------|---------|---------------|
| Portal tile | `ModTile` (`Terraria.ModLoader.ModTile`) | Defines `Test1`'s block behavior, texture, right-click hook | The only supported way to add a new tile type in tModLoader 1.4; codebase has no existing `ModTile`, this phase introduces the pattern |
| Portal placing item | `ModItem` with `Item.createTile` set to the new tile's registered type | Lets the tile actually be placed (all placeable tiles need a corresponding item) | Standard tModLoader furniture-item pattern; required even for a Creative-menu/debug-only item |
| Right-click hook | `ModTile.NewRightClick(int i, int j)` → `bool` | Detects player interaction with the placed tile; return `true` to claim the interaction | Confirmed via direct read of `patches/tModLoader/Terraria.ModLoader/ModTile.cs` (tModLoader `master` branch, current for 1.4.4-stable): the old `RightClick(int,int)` (`void`-returning) is marked `[Obsolete]` since v0.11.5 with an explicit migration note pointing at `NewRightClick` |
| Held-item check | `Main.LocalPlayer.HeldItem.type` | Reads which item the player is holding at the moment of the right-click | Standard singleplayer-safe read; this project is explicitly singleplayer-only (`PROJECT.md`/`REQUIREMENTS.md` "Out of Scope" table), so `Main.LocalPlayer` is unambiguous and correct — no need for multiplayer-safe `Player player` parameter resolution |
| Subworld entry | `SubworldLibrary.SubworldSystem.Enter<T>()` | Sends the player into `BossArenaSubworld` | Already proven working in this codebase (`Debug/SubworldDebugCommands.cs`'s `/bossarena-enter`); no new research needed, just reuse |
| Arrival hook | `ModPlayer.OnEnterWorld(Player player)` | One-shot detection of "player has just finished spawning into a (sub)world" | Source-confirmed (see Architecture Patterns → Pattern 2) to fire at the correct point in `SubworldSystem`'s internal sequence — after world generation, after the player's position has been set via `Player.Spawn(PlayerSpawnContext.SpawningIntoWorld)` |
| Generic boss spawn call | `NPC.SpawnOnPlayer(int plr, int npcType)` (`Terraria.NPC`, vanilla static method, fully public) | Spawns a boss NPC near a given player, with vanilla's own boss-appropriate positioning/safety logic and the "X has awoken!" banner message | Source-confirmed (see Standard Stack → "Don't Hand-Roll" and Architecture Patterns → Pattern 2) as the exact call vanilla's own `Player.ItemCheck_UseBossSpawners` makes for every "simple use-to-summon" boss item, and the documented pattern for modded boss-summon items too |
| Registry storage | Plain `Dictionary<int, int>` (or a small record type) inside a `ModSystem` or static class | itemType → bossNpcType lookup | Matches `.planning/research/ARCHITECTURE.md`'s already-established "central registry as the only cross-cutting seam" pattern (Pattern 2 in that doc), scaled down for Phase 2's item-keyed direction |

### Supporting

| Component | Purpose | When to Use |
|-----------|---------|-------------|
| `Terraria.ID.ItemID.SlimeCrown` (vanilla constant, `= 560`, source-confirmed) | The proof item for D-08 | Register `ItemID.SlimeCrown → NPCID.KingSlime` (`= 50`, source-confirmed) as the registry's single Phase 2 entry |
| `Terraria.ID.NPCID.KingSlime` (vanilla constant, `= 50`) | The proof boss's NPC type | Value the registry maps `SlimeCrown` to |
| `Terraria.ID.TileID.Altars` (vanilla constant; exact numeric value not independently re-derived this session, but the constant name is stable across tModLoader versions) | Reference point for visually benchmarking `Test1` against the Corruption/Crimson Altar sprite | Look up the constant directly in the installed tModLoader/Terraria assembly at implementation time (via IDE autocomplete against the referenced `TerrariaServer`/`Terraria` assembly) rather than trusting a hardcoded number from research — numeric tile IDs are easy to get stale |

### Alternatives Considered

| Instead of | Could use | Tradeoff |
|------------|-----------|----------|
| `NPC.SpawnOnPlayer(playerIndex, bossType)` for the "replay" mechanism | Literally re-invoking the held item's `UseItem`/`ItemCheck` logic (e.g. via reflection into `Player.ItemCheck_UseBossSpawners`, or simulating a real item-use input) | Rejected: that method is `private`, hardcoded per `item.type`, entangled with item-time/animation state, and re-invoking it risks re-triggering unrelated vanilla item-use side effects (sound, dust, `ApplyItemTime`) that don't matter here and could misfire during a world-transition frame. `NPC.SpawnOnPlayer` is the exact same underlying spawn call with none of that baggage, and works identically for both vanilla and (per the standard modded-item convention) most modded summon items |
| `ModPlayer.OnEnterWorld` for the auto-summon trigger | `Subworld.OnEnter()` (already used for the flag-snapshot fix) | Rejected for spawn timing: source-confirmed `OnEnter()` fires *before* world generation/`OnLoad()`/player repositioning — an NPC spawn attempt there would either fail (no valid tiles yet) or use the player's stale main-world position |
| `ModPlayer.OnEnterWorld` | `Subworld.OnLoad()` | Rejected: `OnLoad()` fires after world generation but *before* `Main.QueueMainThreadAction(SpawnPlayer)` has run — the player's position at that moment is still not guaranteed to be the subworld's, since `Player.Spawn()` (which sets it) happens later, on a deferred main-thread action |
| A single `Dictionary<int,int>` registry | Reusing/pre-building the full `BossRegistry` (NPC-type-keyed) described in `.planning/research/ARCHITECTURE.md` for Phase 3 | Rejected for this phase: Phase 3's registry is keyed the *opposite* direction (NPC type → boss key, for kill-detection) and doesn't need to exist yet. Building a separate, smaller, forward-compatible item-keyed registry now avoids scope creep into Phase 3 while still being easy to fold into a shared `BossDefinition` record later if useful |

**Installation:**

No new packages. All APIs used (`ModTile`, `ModItem`, `ModPlayer`, `NPC.SpawnOnPlayer`, `SubworldSystem.Enter<T>()`) are part of tModLoader's own core API surface or the already-referenced `SubworldLibrary`. No `build.txt` changes needed for this phase.

**Version verification:**

```bash
dotnet --version   # confirmed this session: 8.0.424, matches global.json pin
```

`ModTile.NewRightClick`/`ModPlayer.OnEnterWorld` signatures were verified against the tModLoader `master` branch's `patches/tModLoader/Terraria.ModLoader/ModTile.cs` (fetched live this session) — `master` currently tracks the 1.4.4-stable API surface this project targets (`tModLoader/tModLoader/1.4.4` as a git ref/tag returned 404; `master` is the correct current source of truth per the repo's branch structure at time of research).

## Architecture Patterns

### Recommended Project Structure (additions for this phase)

```
BossArenaSubWorld/
├── Tiles/
│   └── Test1Tile.cs             # ModTile: NewRightClick hook, registry lookup, redirect trigger
├── Items/
│   └── Test1Item.cs             # ModItem: places Test1Tile (createTile), no recipe (D-05)
├── Systems/
│   ├── BiomeOverridePlayer.cs   # existing (Phase 1)
│   ├── SummonItemRegistry.cs    # NEW: static/ModSystem Dictionary<int,int> itemType -> bossNpcType (SUBW-01)
│   └── BossSummonPlayer.cs      # NEW: ModPlayer, OnEnterWorld hook -> NPC.SpawnOnPlayer (SUBW-04)
├── Subworlds/
│   └── BossArenaSubworld.cs     # existing (Phase 1) — unchanged for this phase
└── Debug/
    └── SubworldDebugCommands.cs # existing (Phase 1) — candidate for removal once redirect verified (see Open Questions)
```

### Pattern 1: Item-Type-Keyed Summon Registry (SUBW-01, SUBW-02 gate)

**What:** A small `Dictionary<int, int>` (or `Dictionary<int, SummonDefinition>` if forward-compatibility with a future boss key is wanted) mapping a summon item's `Item.type` to the boss `NPC.type` it should spawn. Populated once, e.g. in a `ModSystem.PostSetupContent()` or `Mod.AddContent`-adjacent load step.

**When to use:** As the single gate `Test1Tile.NewRightClick` checks against — "is the currently held item a registered summon item?"

**Example:**
```csharp
// Systems/SummonItemRegistry.cs
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossArenaSubWorld.Systems
{
    public class SummonItemRegistry : ModSystem
    {
        // itemType -> bossNpcType. Data-driven per SUBW-01; extend with more
        // entries as later phases register additional "simple use-to-summon" items.
        private static readonly Dictionary<int, int> _itemToBoss = new();

        public override void PostSetupContent()
        {
            Register(ItemID.SlimeCrown, NPCID.KingSlime); // D-08 proof entry
        }

        public static void Register(int itemType, int bossNpcType) =>
            _itemToBoss[itemType] = bossNpcType;

        public static bool TryGetBoss(int itemType, out int bossNpcType) =>
            _itemToBoss.TryGetValue(itemType, out bossNpcType);
    }
}
```

### Pattern 2: Generic Replay via `NPC.SpawnOnPlayer` (SUBW-04 — the phase's highest-risk hook)

**What:** Instead of literally re-invoking a held item's private vanilla use-logic, store the *target boss NPC type* at redirect time, and spawn it generically once the player has arrived, via the same public API vanilla's own summon items call internally.

**Source evidence (HIGH confidence — direct source read, cross-verified across two independently-hosted decompiled Terraria snapshots that agree on the mechanism and call pattern):**

```csharp
// Decompiled Player.cs (private method Player.ItemCheck_UseBossSpawners),
// confirmed present (with matching structure) in two independent decompiled
// snapshots of modern (NPCID/ItemID-aligned) Terraria source:
private void ItemCheck_UseBossSpawners(int onWhichPlayer, Item sItem)
{
    // ... (guard: ItemTimeIsZero, itemAnimation, SummonItemCheck(), item.type whitelist)
    if (sItem.type == 560) // ItemID.SlimeCrown
    {
        this.ApplyItemTime(sItem);
        SoundEngine.PlaySound(15, ...);
        if (Main.netMode != 1)
            NPC.SpawnOnPlayer(onWhichPlayer, 50); // NPCID.KingSlime
        else
            NetMessage.SendData(61, ...); // multiplayer client -> server request
    }
    else if (sItem.type == 43) // Suspicious Looking Eye
    {
        if (Main.dayTime) return; // item-specific gate, NOT inside SpawnOnPlayer itself
        // ...
        NPC.SpawnOnPlayer(onWhichPlayer, 4); // NPCID.EyeofCthulhu
    }
    // ...further item.type branches, ALL calling NPC.SpawnOnPlayer with a hardcoded boss type
}
```

Every "simple use-to-summon" vanilla item (Slime Crown, Suspicious Looking Eye, Worm Food, Bloody Spine, Abyssal Diving Suit-crimson variant, Celestial Sigil-adjacent, etc.) funnels through this exact same `NPC.SpawnOnPlayer(playerIndex, hardcodedBossType)` call — the only per-item variance is the *gate* checked beforehand (night-time, biome zone, hardmode), not the spawn mechanism itself.

**Recommended implementation for this phase:**

```csharp
// Systems/BossSummonPlayer.cs
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;
using BossArenaSubWorld.Subworlds;

namespace BossArenaSubWorld.Systems
{
    public class BossSummonPlayer : ModPlayer
    {
        // Set by Test1Tile.NewRightClick right before SubworldSystem.Enter<>() is called.
        // Static + nulled-after-consume because this project is singleplayer-only
        // (Out of Scope: multiplayer) -- a per-ModPlayer instance field would also work
        // and generalizes better if multiplayer is ever revisited (see Open Questions).
        public static int? PendingBossNpcType;

        public override void OnEnterWorld()
        {
            if (!PendingBossNpcType.HasValue) return;
            if (!SubworldSystem.AnyActive<BossArenaSubworld>()) return;

            NPC.SpawnOnPlayer(Player.whoAmI, PendingBossNpcType.Value);
            PendingBossNpcType = null; // consume once -- prevents re-summon on later main-world re-entries
        }
    }
}
```

**Trade-offs:** This does not literally "replay UseItem/UseStyle" as D-09's wording suggests at a literal-code level, but it *does* satisfy D-09's stated intent — "generalizes cleanly to any future item registered under D-07's scope... without bespoke per-boss spawn code" — better than a literal replay attempt would, since a literal replay of vanilla's private method is neither clean nor safe to invoke externally. Document this substitution explicitly for the user/planner so the deviation from the literal decision wording is a visible, deliberate choice, not a silent drift.

**Residual risk for future (non-Phase-2) registrations:** Some modded summon items may not follow the `NPC.SpawnOnPlayer` convention (e.g., they might call `NPC.NewNPC` directly with custom positioning, or require a specific `Player.Zone*` flag to be set first — see `Systems/BiomeOverridePlayer.cs`, already built in Phase 1 for exactly this contingency). Each future item added to the registry should be spot-verified against its own mod's source/behavior before assuming `NPC.SpawnOnPlayer` alone is sufficient — flag this in the registry's own code comments as a per-entry verification checklist item, mirroring the discipline `.planning/research/PITFALLS.md` already establishes for Phase 4+ mod integrations.

### Pattern 3: Redirect Trigger Inside `ModTile.NewRightClick`

**What:** The tile's right-click hook is the single point where the registry is checked, the pending-boss field is set, the chat message fires, and subworld entry is triggered.

**Example:**
```csharp
// Tiles/Test1Tile.cs
using Terraria;
using Terraria.ModLoader;
using SubworldLibrary;
using BossArenaSubWorld.Subworlds;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Tiles
{
    public class Test1Tile : ModTile
    {
        public override void SetStaticDefaults()
        {
            // Multitile/frame/interactive-tile registration (TileObjectData, Main.tileFrameImportant,
            // Main.tileSolid = false, etc.) -- benchmark against vanilla TileID.Altars registration
            // at implementation time; not reproduced in full here since D-03 forbids inheriting
            // the vanilla altar's actual TileID/behavior, only its visual footprint.
        }

        public override bool NewRightClick(int i, int j)
        {
            Player player = Main.LocalPlayer; // singleplayer-only project (Out of Scope: multiplayer)
            if (!SummonItemRegistry.TryGetBoss(player.HeldItem.type, out int bossNpcType))
                return false; // not a registered item -- no interaction, no tile-hover text lock-in

            Main.NewText("...redirect confirmation message (D-11, wording at Claude's discretion)...");
            BossSummonPlayer.PendingBossNpcType = bossNpcType;
            SubworldSystem.Enter<BossArenaSubworld>();
            return true;
        }
    }
}
```

**Trade-offs:** Because right-clicking an interactable tile takes input priority over an item's own `AltFunctionUse` (right-click item behavior) per tModLoader's own doc comment on `NewRightClick`'s return value ("preventing other right click actions... from happening"), there is no need for a separate "cancel the item's use" step — the item's `UseItem`/`AltFunctionUse` path is simply never reached when the click lands on an interactable tile. This directly answers research question 2: **no explicit cancellation code is needed**, because the trigger design (tile right-click, not item use) never enters the item's use pipeline in the first place.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|--------------|-----|
| Spawning a boss NPC near the player with safe positioning | A custom "find valid ground near player" search loop | `NPC.SpawnOnPlayer(playerIndex, npcType)` | Source-confirmed to already implement exactly this (tile-solidity search, `safeRange`/`spawnRange` avoidance, and even boss-specific positioning logic for King Slime specifically — `Type == 50` gets dedicated line-of-sight checks in vanilla's own implementation) |
| Detecting "player just arrived in this subworld" | A tick-based `SubworldSystem.AnyActive<T>()` poll in `ModSystem.PostUpdateWorld` with manual one-shot-flag bookkeeping | `ModPlayer.OnEnterWorld()` gated by `SubworldSystem.AnyActive<T>()`, consuming a pending-flag once | `OnEnterWorld` is already an event-driven, exactly-once-per-world-entry hook (backed by vanilla's `Player.Hooks.OnEnterWorld`) — a tick poll would need its own one-shot bookkeeping to avoid re-firing every frame, reinventing what the hook already provides |
| Subworld entry/exit | Custom world-transition/save-state logic | `SubworldSystem.Enter<T>()` / `Exit()` | Already proven in this exact codebase (Phase 1's debug commands); no reason to reimplement |
| Right-click detection on a custom tile | Reading raw mouse/tile-hover state in a `ModPlayer`/`ModSystem` update loop | `ModTile.NewRightClick(int i, int j)` | Purpose-built hook for exactly this; also self-documents intent (`return true` = "click Consumed here") |

**Key insight:** Every mechanism this phase needs already exists as a first-class tModLoader or SubworldLibrary hook or a public vanilla static method. The research risk in this phase was never "does an API exist" — it was "which of several similarly-named/similarly-timed hooks is the *correct* one" (`RightClick` vs `NewRightClick`; `Subworld.OnEnter` vs `OnLoad` vs `ModPlayer.OnEnterWorld`; literally replaying item-use vs. calling the shared spawn primitive). Get the timing/hook choice right and the implementation is short.

## Common Pitfalls

### Pitfall 1: Using the deprecated `ModTile.RightClick(int,int)` instead of `NewRightClick`

**What goes wrong:** `RightClick` still compiles (marked `[Obsolete]`, not removed) and returns `void`, so there's no way to signal "interaction handled," and worse, tModLoader's own doc string says nothing stops the click from also propagating into other right-click behavior.
**Why it happens:** IDE autocomplete may surface the old, shorter-named `RightClick` first since it appears earlier in the base class and the name doesn't obviously read as "old."
**How to avoid:** Always override `NewRightClick(int i, int j)` returning `bool`, and return `true` on a successful redirect trigger.
**Warning signs:** A compiler warning (`CS0672`/obsolete-member warning) on build if `RightClick` is overridden instead.

### Pitfall 2: Putting the auto-summon call in the wrong subworld-entry hook

**What goes wrong:** Calling `NPC.SpawnOnPlayer` from `Subworld.OnEnter()` (or even `OnLoad()`) either silently fails to find a valid position (world not generated yet at `OnEnter()` time) or spawns the boss at the player's *stale main-world coordinates* instead of their actual arena position, because the player hasn't been repositioned yet.
**Why it happens:** `OnEnter()` is the hook already used in this exact file (`Subworlds/BossArenaSubworld.cs`) for the Phase 1 flag-snapshot fix, making it an easy (but wrong-for-this-purpose) hook to reach for again.
**How to avoid:** Source-confirmed correct order (from direct read of `SubworldSystem.cs`'s `ExitWorldCallBack`/`LoadWorld`): `current.OnEnter()` → `LoadWorld()` → world generation → `current.OnLoad()` → (next frame, via `Main.QueueMainThreadAction`) `Main.LocalPlayer.Spawn(PlayerSpawnContext.SpawningIntoWorld)`, which is what fires `Player.Hooks.OnEnterWorld` / tModLoader's `ModPlayer.OnEnterWorld`. Use `ModPlayer.OnEnterWorld`, gated by `SubworldSystem.AnyActive<BossArenaSubworld>()`.
**Warning signs:** Boss NPC never appears, or appears at what looks like the main world's old player position/underground instead of on the flat arena platform.

### Pitfall 3: Forgetting to gate/consume the pending-boss flag, causing re-summon on unrelated world entries

**What goes wrong:** If the "pending boss to summon" flag isn't cleared after one use, exiting and re-entering the arena subworld later (e.g. via the Phase 1 debug `/bossarena-enter` command, or a later real re-entry) could unexpectedly summon King Slime again, or re-summoning on a completely unrelated main-world load if the gate check is missing entirely.
**Why it happens:** `ModPlayer.OnEnterWorld` fires on *every* world entry (main world included) — the static/instance pending field persists across calls unless explicitly cleared, and the `SubworldSystem.AnyActive<T>()` gate alone isn't sufficient if the player re-enters the same subworld a second time without a fresh tile interaction.
**How to avoid:** Consume (null out / reset) the pending field immediately after `NPC.SpawnOnPlayer` is called, exactly once, inside the same `OnEnterWorld` call that used it.
**Warning signs:** King Slime spawns again on a second, unrelated subworld visit (e.g. testing the Phase 1 debug commands after Phase 2 lands).

### Pitfall 4: Item consumption via the standard `ModItem.UseItem` path (not applicable here, but a related trap for future item registrations)

**What goes wrong:** If a future implementation detour routes the redirect through the item's own `UseItem`/`ItemLoader.UseItem` pipeline instead of a pure tile-interaction check, vanilla's default item-consumption logic (`Item.consumable`) could fire and consume the summon item, violating SUBW-04's "item not consumed" requirement even for items that are normally consumable when used the *normal* way.
**Why it happens:** It would be tempting, for a "more literal" reading of D-09, to actually call into the item's use pipeline to "replay" it — but doing so re-enters the same code path that also handles consumption, cooldowns, and mana costs.
**How to avoid:** This phase's design (tile-`RightClick`-triggered, registry-gated, `NPC.SpawnOnPlayer`-based) never touches `Item.UseItem`/`ItemLoader.UseItem` at all, so consumption logic is never invoked in the first place — SUBW-04's "not consumed" requirement is satisfied by construction, not by an extra guard. Slime Crown is non-consumable to begin with (D-08), so this specific proof wouldn't even surface the bug if it existed — worth calling out explicitly so a future genuinely-consumable item registered under D-07 doesn't silently regress this.
**Warning signs:** A future registered item (not Slime Crown) disappears from inventory after a redirect.

### Pitfall 5: `Test1`'s texture cannot simply "point at" the vanilla altar's asset path the same way `ModTile.Texture` normally resolves a mod's own asset

**What goes wrong:** tModLoader's standard `ModTile` autoload convention resolves `Texture` (a string property, defaulting to the class's mod-relative namespace/folder path) to a `.png`/`.rawimg` file that must physically exist as an asset *inside this mod's own Content pipeline* — it is not a generic "any game asset by path" lookup for the tile's main auto-draw pass. Vanilla assets are reachable via `ModContent.Request<Texture2D>("Terraria/Images/Tiles_<id>")` for ad hoc draw calls (e.g. inside a custom `PreDraw` override), but that is a different, lower-level code path than the `Texture` property tModLoader's default tile-rendering pipeline consumes.
**Why it happens:** "Reuse the vanilla altar's texture" sounds like it should be a one-line asset-path reference; the actual tModLoader asset pipeline distinguishes "this mod's own shipped texture" (`Texture` property, auto-drawn) from "any texture loadable at runtime via `ModContent.Request`" (usable only from custom draw code you write yourself, e.g. `PreDraw`).
**How to avoid:** The practical, low-risk path (recommended, Claude's discretion per CONTEXT.md) is to extract/copy the vanilla Corruption/Crimson Altar sprite into this mod's own `Tiles/` asset folder as an original file the `Test1Tile.Texture` property can point to normally — this uses the standard, well-tested auto-draw pipeline (multitile framing, lighting, animation all "just work") instead of hand-rolling a custom `PreDraw` that manually draws a `ModContent.Request`-loaded vanilla texture (workable, but strictly more code and more edge cases — e.g. multitile frame offsets — to get right for no added benefit here).
**Warning signs:** Tile renders as tModLoader's "missing texture" placeholder (a bright magenta/checker texture) if `Texture` points at a path with no matching file in the mod's own compiled content.
**Confidence:** MEDIUM — this reflects well-established, stable tModLoader convention (unchanged across 1.3→1.4), but was not independently re-verified via a fresh official-doc fetch this session (the DeepWiki summary consulted didn't cover this specific mechanism explicitly); recommend the implementer do a quick confirmation pass against `Basic Tile Entity`/an `ExampleMod` tile file before writing `Test1Tile.SetStaticDefaults()`.

## Code Examples

### `ModTile.NewRightClick` — current, non-deprecated signature (source-verified)

```csharp
// Source: tModLoader GitHub, patches/tModLoader/Terraria.ModLoader/ModTile.cs (master branch,
// fetched live this session; matches 1.4.4-stable API surface)
/// <summary>
/// Allows you to make something happen when this tile is right-clicked by the player.
/// Return true to indicate that a tile interaction has occurred, preventing other right
/// click actions like minion targetting from happening. Returns false by default.
/// </summary>
public virtual bool NewRightClick(int i, int j) {
    return false;
}
```

### `SubworldLibrary.Subworld` lifecycle hooks and their confirmed firing order (source-verified)

```csharp
// Source: SubworldLibrary GitHub, Subworld.cs + SubworldSystem.cs (master branch,
// fetched live this session, matches installed v2.2.3.2 per this project's own
// prior Phase 1 research)
//
// Confirmed order for SubworldSystem.Enter<T>() -> BeginEntering() -> ExitWorldCallBack():
//   1. current.OnEnter()               <-- world context switches, generation NOT yet run
//   2. LoadWorld() -> LoadSubworld()   <-- world generation (GenPass Tasks) runs
//   3. current.OnLoad()                <-- subworld content now exists
//   4. Main.QueueMainThreadAction(SpawnPlayer)   <-- deferred to next frame
//   5. SpawnPlayer(): Main.LocalPlayer.Spawn(PlayerSpawnContext.SpawningIntoWorld)
//      -> fires Player.Hooks.OnEnterWorld -> tModLoader ModPlayer.OnEnterWorld
//
// SubworldLibrary itself subscribes to the same event:
//   Player.Hooks.OnEnterWorld += OnEnterWorld;  // SubworldSystem.OnModLoad()
```

### `NPC.SpawnOnPlayer` — the generic boss-spawn primitive (source-verified, decompiled vanilla)

```csharp
// Source: decompiled Terraria NPC.cs (two independently-hosted snapshots agree on
// signature and call convention; exact line numbers vary by snapshot/version)
public static void SpawnOnPlayer(int plr, int Type)
{
    if (Main.netMode == 1) return; // client can't self-authorize a boss spawn
    // ... special-cased positioning logic per Type (e.g. Type == 50 for King Slime
    // gets extra line-of-sight/collision checks), then NPC.NewNPC(...) at a found position,
    // then a "TypeName has awoken!" chat/network broadcast.
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|-------------------|---------------|--------|
| `ModTile.RightClick(int,int)` (void) | `ModTile.NewRightClick(int,int)` (bool) | tModLoader v0.11.5 (pre-1.4, still marked obsolete rather than removed in current 1.4.4-stable) | Must use the bool-returning version; the void one still compiles but cannot claim/consume the interaction |

**Deprecated/outdated:** None of the APIs this phase needs are deprecated in current tModLoader beyond the `RightClick`/`NewRightClick` split noted above.

## Open Questions

1. **Should Phase 1's debug commands (`/bossarena-enter`, `/bossarena-exit`, `/bossarena-checkflag`) be deleted inside this phase's plan, or in a small follow-up?**
   - What we know: Phase 1's CONTEXT.md (D-02) says they're deleted once "the real redirect fully lands and is verified." `Debug/SubworldDebugCommands.cs`'s own header comment says the same.
   - What's unclear: Whether "verified" means immediately upon this phase's plan completing, or only after a live in-game smoke test confirms the new tile-based redirect actually works end-to-end (mirroring how Phase 1's own isolation-proof checkpoint required a live test before being marked complete).
   - Recommendation: Keep the debug commands through this phase's implementation and initial live verification (they remain useful for isolating "is the arena/registry broken" vs. "is the tile interaction broken" during debugging), and delete them as the final task of this phase's last plan, only after the tile-based redirect is confirmed working live — matching the discipline Phase 1 already established (code complete ≠ verified; a build passing doesn't confirm in-game behavior for a tModLoader mod).

2. **Exact numeric value of `TileID.Altars` and the Corruption/Crimson Altar's multitile frame dimensions (for benchmarking `Test1`'s visual size/footprint).**
   - What we know: The constant name `TileID.Altars` is stable; the altar is a known 3-wide multitile furniture piece in live gameplay.
   - What's unclear: The exact `TileObjectData` registration parameters (frame width/height in the sprite sheet, `newTile.CoordinateHeights`, etc.) were not independently re-derived from source this session due to time budget — general `ModTile`/`TileObjectData` research depth was prioritized over this one numeric detail.
   - Recommendation: Look this up directly via IDE (autocomplete against the referenced `TerrariaServer.dll`/`Terraria.dll`) at implementation time rather than trusting a stale hardcoded number — this is exactly the kind of "verify against installed DLL, not memory" discipline `.planning/research/PITFALLS.md` already establishes project-wide (Pitfall 3).

3. **Does `NPC.SpawnOnPlayer`'s vanilla positioning logic behave sensibly on the Phase 1 arena's specific terrain (10,000-wide flat stone platform, `Height=800`)?**
   - What we know: The method's generic (non-King-Slime-specific) path searches for solid ground near the player within `spawnRangeX/Y`/`safeRangeX/Y`; the King-Slime-specific branch (`Type == 50`) additionally requires line-of-sight collision checks between two points above the found ground.
   - What's unclear: Whether the flat platform's simple, uniform terrain could cause the search loop to behave unexpectedly (e.g., always picking the same offset, or being slow/failing in an unusual edge case) — this was not live-tested this session.
   - Recommendation: This is exactly the kind of thing D-10 already anticipates ("no positioning concern to solve" because the platform is wide open) — treat as a live-verification checkpoint in this phase's plan (per this project's established "code complete ≠ verified in-game" discipline), not a blocking research gap.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|--------------|-----------|---------|----------|
| .NET 8.0 SDK | Build (`dotnet build`/`dotnet msbuild`) | ✓ | 8.0.424 (confirmed this session, matches `global.json` pin) | — |
| tModLoader / SubworldLibrary reference | `SubworldSystem.Enter<T>()`, `ModTile`, `ModItem`, `ModPlayer` APIs | ✓ | Already referenced (`build.txt`: `modReferences = SubworldLibrary`; `Libs/SubworldLibrary.dll` present) — unchanged from Phase 1, no new dependency needed for Phase 2 | — |

No new external dependencies are introduced by this phase — everything needed (`ModTile`, `ModItem`, `ModPlayer`, vanilla `NPC.SpawnOnPlayer`) ships as part of tModLoader's own core API and the already-integrated `SubworldLibrary`.

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | None — this is a tModLoader mod with no automated unit-test harness; verification is manual, live, in-game, matching the pattern already established in Phase 1 (see `.planning/phases/01-subworld-skeleton-isolation-proof/01-VERIFICATION.md` and the resolved `isolation-premise-flag-persistence.md` debug session, both of which relied on live in-game checks, not automated tests) |
| Config file | none |
| Quick run command | `dotnet build BossArenaSubWorld.csproj` (compile-time smoke check only — confirms the code builds, not that it behaves correctly in-game) |
| Full suite command | N/A — no automated suite exists for this project; "full verification" = a live in-game playthrough of the phase's success criteria |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|---------------------|--------------|
| SUBW-01 | Registered item (Slime Crown) recognized by the registry | manual-only | N/A — `dotnet build` only confirms compile correctness; recognizing the held item requires a live right-click test | ❌ no automated harness in this project |
| SUBW-02 | `Test1` tile places, renders, and its `NewRightClick` fires only for registered items | manual-only | N/A | ❌ |
| SUBW-03 | Right-click with Slime Crown held sends the player into `BossArenaSubworld` | manual-only | N/A | ❌ |
| SUBW-04 | King Slime auto-spawns on arrival; Slime Crown remains in inventory afterward | manual-only | N/A | ❌ |

**Justification for manual-only across all four requirements:** tModLoader mods have no in-process unit-test framework capable of simulating a live `Main`/`Player`/`WorldGen` game loop, world transitions, or NPC spawning — this is a structural constraint of the platform, not a gap in this project's tooling. Phase 1 already established and followed this same manual-verification discipline (`01-VERIFICATION.md`); Phase 2 should follow the identical pattern: build-passes as a fast per-task gate, then one live in-game checkpoint (place `Test1`, hold Slime Crown, right-click, confirm subworld entry + King Slime spawn + Slime Crown still in inventory) as the phase-gate verification, mirroring Phase 1's King Slime isolation-proof checkpoint.

### Sampling Rate

- **Per task commit:** `dotnet build BossArenaSubWorld.csproj` (0 warnings, 0 errors expected, matching this project's established bar)
- **Per wave merge:** Same build command; no separate "full suite" exists
- **Phase gate:** One live in-game checkpoint covering all four success criteria in sequence (place tile → hold Slime Crown → right-click → confirm subworld entry → confirm King Slime auto-spawn → confirm item not consumed → exit → confirm no unexpected leftover state), before `/gsd:verify-work`

### Wave 0 Gaps

None — no test framework exists to gap-fill; this project's validation model is build-gate + live-verification-gate, not automated-test-gate, and that model is already fully established from Phase 1. Recommend the plan explicitly schedule a live-verification checkpoint as its final task, matching Phase 1's `01-04-PLAN.md` structure (a dedicated "checkpoint" plan/task, not an implicit assumption).

## Sources

### Primary (HIGH confidence)

- tModLoader GitHub, `patches/tModLoader/Terraria.ModLoader/ModTile.cs` (master branch, fetched live this session) — `NewRightClick`/`RightClick` signatures and obsolete-migration doc comment
- SubworldLibrary GitHub (jjohnsnaill/SubworldLibrary), `Subworld.cs` and `SubworldSystem.cs` (master branch, fetched live this session; matches installed v2.2.3.2 per this project's own Phase 1 research and this session's own `dotnet --version`/`build.txt` check) — `OnEnter`/`OnLoad`/`OnExit` doc comments, `ExitWorldCallBack`/`LoadWorld`/`SpawnPlayer` call-order source read, `Player.Hooks.OnEnterWorld` subscription confirming `ModPlayer.OnEnterWorld` alignment
- Decompiled Terraria `Player.cs`/`NPC.cs`, two independently-hosted snapshots (`AliceSavard/Terarria1405` and an older `TheVamp/Terraria-Source-Code` mirror), both fetched live this session and cross-checked — confirms `Player.ItemCheck_UseBossSpawners`'s hardcoded `item.type` → `NPC.SpawnOnPlayer(playerIndex, npcType)` dispatch pattern for Slime Crown (`ItemID 560` → `NPCID 50`) and sibling vanilla boss-summon items
- This project's own `Subworlds/BossArenaSubworld.cs`, `Debug/SubworldDebugCommands.cs`, `.planning/debug/isolation-premise-flag-persistence.md`, `.planning/research/ARCHITECTURE.md`, `.planning/research/PITFALLS.md`, `.planning/REQUIREMENTS.md`, `.planning/ROADMAP.md`, `.planning/STATE.md`, `.planning/phases/02-.../02-CONTEXT.md` — all read directly this session

### Secondary (MEDIUM confidence)

- WebSearch results confirming `NPC.SpawnOnPlayer(player.whoAmI, npcType)` as the standard, documented pattern for **modded** boss-summon items' `ModItem.UseItem` overrides (not independently source-verified against a specific modded item's actual code this session — inferred from community tutorial/forum consensus, consistent with the vanilla mechanism confirmed via primary sources above)
- tModLoader `ModTile`/asset-pipeline texture-authoring convention (Pitfall 5) — well-established, stable tModLoader knowledge, but not re-confirmed via a fresh official-doc fetch this session (DeepWiki summary consulted didn't cover the specific mechanism)
- Tile right-click taking input priority over item `AltFunctionUse` — confirmed via tModLoader's own `NewRightClick` doc-comment wording ("preventing other right click actions... from happening") plus well-established general Terraria player-level knowledge (right-clicking an interactable tile like a chest/door never simultaneously triggers a held item's alt-use)

### Tertiary (LOW confidence)

- None flagged this session — all findings that couldn't be source-verified were either explicitly marked MEDIUM above with a stated reason, or left as an Open Question rather than asserted.

## Metadata

**Confidence breakdown:**
- Standard stack (hooks, API signatures): HIGH — every core API call in this document was verified against live-fetched tModLoader/SubworldLibrary source this session, not asserted from training-data memory alone
- Architecture (registry design, hook timing, spawn-replay mechanism): HIGH — the two riskiest design questions (how to "replay" a summon item, and which hook fires at the right time) were both resolved via direct source reads, not inference
- Pitfalls: HIGH for hook-timing/deprecation pitfalls (source-confirmed); MEDIUM for the texture-authoring pitfall (well-established convention, not freshly re-verified)

**Research date:** 2026-08-13
**Valid until:** ~30 days (tModLoader 1.4.4-stable and SubworldLibrary's public API surface are stable/slow-moving; re-verify if either updates before this phase is implemented, per this project's own established "recheck against installed DLL" discipline)

---
*Phase: 02-summon-item-redirect-entry-registry*
*Research completed: 2026-08-13*
