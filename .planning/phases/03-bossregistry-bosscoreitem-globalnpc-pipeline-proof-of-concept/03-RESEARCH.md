# Phase 3: BossRegistry + BossCoreItem + GlobalNPC Pipeline (Proof of Concept) - Research

**Researched:** 2026-08-13
**Domain:** tModLoader 1.4.4.9 C# modding API — `GlobalNPC.ModifyNPCLoot`, custom `IItemDropRule`, `ModItem` instance data, vanilla `NPC.SetEventFlagCleared`
**Confidence:** HIGH (every API signature below was cross-checked directly against the installed `tModLoader.dll` v1.4.4.9 via `System.Reflection.MetadataLoadContext`, the same technique this project's own Phase 1/2 execution already established — not just official docs/wiki paraphrase)

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**D-01 (Idempotency / APPLY-04):** `BossRegistry.Apply(key)` checks the boss's current downed state via a per-boss "already downed" getter (part of each `BossDefinition`) *before* calling the apply/side-effect logic. If already downed, `Apply()` is a no-op — no error, no re-application. No separate applied-tracking set is stored in `BossRegistry`'s own world data; checking the live flag is the single source of truth.

**D-02 (BossCoreItem consumption policy):** `BossCoreItem` is consumed only when `Apply()` succeeds. On failure (e.g. registry lookup miss for the item's stored key, mod-specific data unavailable), the item is retained in the player's inventory and a chat message explains what happened, so the player can retry or report the issue.

**D-03 (BossRegistry key design):** Registry keys are namespaced strings, e.g. `"vanilla:king_slime"`, with future entries following the same `modprefix:boss_name` convention. Keys are decoupled from raw `NPC.type` — a boss key maps to one or more NPC types, not the reverse.

**D-04 (King Slime downed-flag fidelity):** Applying King Slime's downed state replays the same helper vanilla itself uses on a real kill — `NPC.SetEventFlagCleared(ref NPC.downedSlimeKing, ...)` — not a raw boolean assignment. This reproduces vanilla's achievement-progression notification and its (singleplayer-no-op) multiplayer sync call.

### Claude's Discretion

- `BossCoreItem` itemization (sprite/display name/rarity) for this POC — follow the `Test1Item` precedent (minimal/functional placeholder, no polish). Obtained only via the kill drop (`ModifyNPCLoot` + `ItemDropRule`) — no debug give-command needed.
- Exact shape of the per-boss "already downed" getter on `BossDefinition` (e.g. `Func<bool> IsDowned` field vs. a method) — implementation detail for planning.
- Exact chat message wording for success/failure feedback (D-02).
- File/class naming within the `Systems/`, `GlobalNPCs/`, `Items/` structure already sketched in `research/ARCHITECTURE.md`.

### Deferred Ideas (OUT OF SCOPE)

None — discussion stayed within phase scope. APPLY-02/APPLY-03 mod-specific side-effect reproduction was raised only as a boundary clarification for D-04, not pulled into this phase's scope; it remains Phase 4's responsibility.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| DROP-01 | A central NPC.type → bossKey mapping registers which NPCs count as trackable bosses | `BossRegistry` extends the existing `SummonItemRegistry.cs` `ModSystem`/`PostSetupContent` convention — see Architecture Patterns, Pattern 1 |
| DROP-02 | Registered bosses drop a `BossCoreItem` via a conditional `ItemDropRule` in `GlobalNPC.ModifyNPCLoot`, gated to the subworld | Confirmed via reflection: `ModifyNPCLoot` runs **once per NPC type at mod load**, not per kill — the subworld gate MUST live inside a custom `IItemDropRule.CanDrop(DropAttemptInfo)`, not as an `if` in `ModifyNPCLoot` itself. See Common Pitfalls #1 and Code Examples |
| DROP-03 | `BossCoreItem` stores `BossKey` as instance data (`Clone`/`SaveData`/`LoadData`), set at spawn time inside the custom drop rule | Confirmed exact base-class location of `Clone`/`CloneNewInstances` (declared on `ModType<Item, ModItem>`, inherited by `ModItem`) and the `Item.NewItem(...)` overload + `Item.ModItem` property needed to set the key immediately after spawning inside the rule. See Code Examples |
| APPLY-01 | Using `BossCoreItem` calls `BossRegistry.Apply(key)`, sets the boss's downed flag | `ModItem.UseItem(Player)` confirmed signature (`bool?`, client-only) driving `BossRegistry.Apply(key)` → `NPC.SetEventFlagCleared(ref NPC.downedSlimeKing, gameEventId)`. See Code Examples |
| APPLY-04 | Idempotent re-use; world-scoped vs. player-scoped classification | `BossRegistry.Apply` returns a 3-state result (`Applied` / `AlreadyDowned` / `UnknownKey`) mapping directly to D-01 + D-02's consume-vs-retain policy. King Slime has no player-scoped reward, so no double-grant risk this phase (Pitfall 5 from prior research doesn't apply to this specific boss, but the `ApplyResult` shape generalizes for Phase 4+ bosses that do). See Architecture Patterns, Pattern 3 |
</phase_requirements>

## Summary

This phase's API surface is narrow and now fully verified against the actual installed binary (`D:\SteamLibrary\steamapps\common\tModLoader\tModLoader.dll`, tModLoader 1.4.4.9), not just paraphrased wiki text. The single most important, non-obvious finding is that **`GlobalNPC.ModifyNPCLoot` only runs once per NPC type at mod-load time** — the tModLoader docs explicitly warn "any dynamic behavior must be contained in the rules themselves." This means the "gated to only trigger when the kill happens inside the boss-arena subworld" requirement (DROP-02) cannot be an `if (SubworldSystem.IsActive<...>())` check written inline in `ModifyNPCLoot` — it must live inside a custom `IItemDropRule.CanDrop(DropAttemptInfo info)` implementation, which tModLoader calls dynamically on every actual kill. The same custom rule class is also the natural place to satisfy DROP-03's "set at spawn time inside the custom drop rule": `Item.NewItem(...)` returns the spawned item's `Main.item[]` index, and `Main.item[index].ModItem` gives direct access to set `BossCoreItem.BossKey` before the rule returns.

For D-04, `NPC.SetEventFlagCleared(ref bool eventFlag, int gameEventId)` is confirmed to exist exactly as documented (verified by reflection), and the official tModLoader Migration Guide gives a concrete, sourced example (`NPC.SetEventFlagCleared(ref myDownedBool, -1);`) explicitly stating this call handles the flag, the `MessageID.WorldData` netcode sync, *and* triggers Lantern Night eligibility checks — directly validating the exact fidelity bar `PITFALLS.md` Pitfall 4 and this phase's D-04 are both built around. The only unresolved detail is the precise `gameEventId` integer vanilla's own internal King Slime kill code passes (relevant only for achievement-notification parity, not for flag/sync/event-trigger correctness) — flagged as an Open Question with a safe fallback.

**Primary recommendation:** Implement `BossCoreDropRule : IItemDropRule` (not `LeadingConditionRule` + `ItemDropRule.Common`) as the DROP-02/DROP-03 mechanism — it is the only construct that lets the subworld gate be dynamic *and* lets the spawned item's instance key be set inline, in one class, matching both locked requirements exactly.

## Standard Stack

No new libraries. This phase is pure tModLoader/vanilla-Terraria API usage on top of the stack already established in `research/STACK.md` (tModLoader 1.4.4.9, .NET 8 SDK, SubworldLibrary). All types used below (`Terraria.NPC`, `Terraria.Item`, `Terraria.ModLoader.GlobalNPC`, `Terraria.ModLoader.ModItem`, `Terraria.GameContent.ItemDropRules.*`) ship inside `tModLoader.dll` itself — no `weakReferences`/`[JITWhenModsEnabled]` needed for this phase (that pattern is reserved for Phase 4+'s content-mod integrations).

**Version verification:** Confirmed installed version is exactly **tModLoader 1.4.4.9** (assembly identity read directly off `Terraria.ModLoader.ModType\`2` base type during reflection: `Version=1.4.4.9`). This matches `research/STACK.md`'s targeted version — no drift.

## Architecture Patterns

### Recommended Project Structure (extends `research/ARCHITECTURE.md`)

```
BossArenaSubWorld/
├── Systems/
│   ├── SummonItemRegistry.cs      # existing (Phase 2)
│   ├── BossSummonPlayer.cs        # existing (Phase 2)
│   └── BossRegistry.cs            # NEW: key -> BossDefinition, ApplyResult enum, Apply(key)
├── GlobalNPCs/
│   └── BossKillGlobalNPC.cs       # NEW: ModifyNPCLoot override, npc.type -> key lookup, adds BossCoreDropRule
├── ItemDropRules/
│   └── BossCoreDropRule.cs        # NEW: custom IItemDropRule -- subworld gate (CanDrop) + spawn+tag (TryDroppingItem)
└── Items/
    ├── Test1Item.cs               # existing (Phase 2)
    └── BossCoreItem.cs            # NEW: instance-data carrier, UseItem -> BossRegistry.Apply
```

**Rationale for the new `ItemDropRules/` folder:** Mirrors vanilla's own `Terraria.GameContent.ItemDropRules` namespace convention and keeps the one genuinely tricky class (a hand-rolled `IItemDropRule`) isolated and easy to find, rather than burying it inside `BossKillGlobalNPC.cs`.

### Pattern 1: `BossRegistry` extends the existing `SummonItemRegistry` convention

**What:** Same `ModSystem` + `PostSetupContent()` + static `Dictionary` shape as `Systems/SummonItemRegistry.cs` (already in this codebase), but keyed by string (D-03) and valued by a richer `BossDefinition` record carrying an apply delegate *and* an idempotency getter (D-01).
**When to use:** This phase's sole registration entry (`"vanilla:king_slime"`); the same shape Phase 4+ will reuse per-mod.

```csharp
namespace BossArenaSubWorld.Systems
{
    public record BossDefinition(int[] NpcTypes, Action ApplyDowned, Func<bool> IsDowned);

    public enum ApplyResult { Applied, AlreadyDowned, UnknownKey }

    public class BossRegistry : ModSystem
    {
        private static readonly Dictionary<string, BossDefinition> _byKey = new();
        private static readonly Dictionary<int, string> _npcTypeToKey = new();

        public override void PostSetupContent()
        {
            Register("vanilla:king_slime", new BossDefinition(
                NpcTypes: new[] { NPCID.KingSlime },
                ApplyDowned: () => NPC.SetEventFlagCleared(ref NPC.downedSlimeKing, -1),
                IsDowned: () => NPC.downedSlimeKing));
        }

        public static void Register(string key, BossDefinition def)
        {
            _byKey[key] = def;
            foreach (int t in def.NpcTypes) _npcTypeToKey[t] = key;
        }

        public static bool TryGetKeyForNpc(int npcType, out string key) =>
            _npcTypeToKey.TryGetValue(npcType, out key);

        public static ApplyResult Apply(string key)
        {
            if (!_byKey.TryGetValue(key, out var def))
                return ApplyResult.UnknownKey;

            if (def.IsDowned())
                return ApplyResult.AlreadyDowned;

            def.ApplyDowned();
            return ApplyResult.Applied;
        }
    }
}
```

*Source for `NPC.SetEventFlagCleared(ref bool, int)` signature: confirmed by reflection against the installed `tModLoader.dll` (HIGH confidence — exact match, no ambiguity). Source for the `-1` argument convention: [tModLoader Wiki, Update Migration Guide](https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide-Previous-Versions) — verbatim: "Flagging a boss as defeated will not have to be manually synced anymore (`MessageID.WorldData` will be sent after the `OnKill` hook), and it will also trigger a lantern night if you use this method: `NPC.SetEventFlagCleared(ref myDownedBool, -1);`" (HIGH confidence, official wiki, directly validates D-04's fidelity bar).*

### Pattern 2: Custom `IItemDropRule` — the only mechanism that satisfies both DROP-02 and DROP-03

**What:** `GlobalNPC.ModifyNPCLoot(NPC npc, NPCLoot npcLoot)` is confirmed (reflection: exact signature `Void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)`) to run **once per NPC type at mod-load time** — official docs state "this hook only runs once per npc type during mod loading, any dynamic behavior must be contained in the rules themselves." Built-in rules like `ItemDropRule.Common`/`LeadingConditionRule` (verified via the actual decompiled-source patch, `CommonDrop.cs.patch`) call `CommonCode.DropItem(info, itemId, stack)` internally and return only a `State` in their `ItemDropAttemptResult` — **no item index is exposed back to the caller**, so there is no way to reach in afterward and set `BossCoreItem.BossKey` on the spawned instance using the built-in rules. A custom rule implementing `IItemDropRule` directly is required to (a) evaluate the subworld gate dynamically per kill via `CanDrop`, and (b) capture the spawned item's array index via `Item.NewItem(...)`'s `int` return value so `BossKey` can be set immediately, inside the rule, before it returns.

**`IItemDropRule` interface (confirmed via reflection, exact members):**
```csharp
public interface IItemDropRule
{
    List<IItemDropRuleChainAttempt> ChainedRules { get; }
    bool CanDrop(DropAttemptInfo info);
    ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info);
    void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo);
}
```

**`DropAttemptInfo` fields (confirmed via reflection):** `NPC npc`, `int item`, `Player player`, `UnifiedRandom rng`, `bool IsInSimulation`, `bool IsExpertMode`, `bool IsMasterMode`.

**`ItemDropAttemptResultState` enum (confirmed via reflection, exact 4 values):** `DoesntFillConditions`, `FailedRandomRoll`, `Success`, `DidNotRunCode`.

```csharp
namespace BossArenaSubWorld.ItemDropRules
{
    public class BossCoreDropRule : IItemDropRule
    {
        private readonly string _bossKey;

        public BossCoreDropRule(string bossKey) => _bossKey = bossKey;

        public List<IItemDropRuleChainAttempt> ChainedRules { get; } = new();

        // Dynamic, per-kill gate -- this is where the subworld check MUST live
        // (ModifyNPCLoot itself only runs once at load, see Pitfall #1 below).
        public bool CanDrop(DropAttemptInfo info) =>
            SubworldSystem.IsActive<BossArenaSubworld>();

        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            int index = Item.NewItem(
                info.npc.GetSource_Loot("BossArenaSubWorld:BossCoreDrop"),
                info.npc.getRect(),
                ModContent.ItemType<BossCoreItem>(),
                1);

            if (Main.item[index].ModItem is BossCoreItem coreItem)
                coreItem.BossKey = _bossKey;

            return new ItemDropAttemptResult { State = ItemDropAttemptResultState.Success };
        }

        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) { }
    }
}
```

```csharp
namespace BossArenaSubWorld.GlobalNPCs
{
    public class BossKillGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (BossRegistry.TryGetKeyForNpc(npc.type, out string key))
                npcLoot.Add(new BossCoreDropRule(key));
        }
    }
}
```

**Note on `NPC.GetSource_Loot(string context)`:** Confirmed via reflection that this method (inherited from `Terraria.Entity`, not declared directly on `NPC`) takes a **required `string context` parameter** — the tModLoader wiki's prose summary ("NPC spawning item drops should use `NPC.GetSource_Loot()`") omits this parameter; the actual signature is `IEntitySource GetSource_Loot(string context)`. Pass a short descriptive string (used for debugging/logging inside tModLoader, not gameplay-visible).

**Trade-offs:** A hand-rolled `IItemDropRule` is slightly more code than `LeadingConditionRule` + `ItemDropRule.Common`, but it is the only option that satisfies DROP-03's literal requirement ("set at spawn time inside the custom drop rule") without a second mechanism (e.g. `ModItem.OnSpawn(IEntitySource)` reading `EntitySource_Loot`, which is a viable *alternative* the docs also mention, but D-03's wording specifically points at the drop rule itself).

### Pattern 3: `BossCoreItem` — instance data + conditional consumption

**Confirmed via reflection:** `Clone(Item)` and `CloneNewInstances` are **not** declared directly on `ModItem` — they live on the generic base `ModType<Item, ModItem>` (one level up), inherited transparently. `CloneNewInstances` defaults to `false` (uses the default constructor on clone) and must be overridden `true` for any item carrying per-instance reference/value data beyond what `SaveData`/`LoadData` alone would restore on a fresh instance — needed here because `Item.Clone()` (e.g. when the item is split across inventory slots, or picked up) must propagate `BossKey` without a save/load round-trip. `Item.ModItem` is a `{ get; set; }` property (confirmed) holding the attached `ModItem` instance — used above to reach the freshly spawned `BossCoreItem`.

`ModItem.UseItem(Player player)` is confirmed to return `Nullable<bool>` (`bool?`) and, per official docs, is **called on the local client only**. In singleplayer (this phase's only in-scope mode per `REQUIREMENTS.md` Out of Scope) client and server are the same process, so this is not a blocker — but it is worth noting explicitly since `BossRegistry.Apply` mutates world-level flags, which is normally server-authoritative logic. Flag for Phase 4+ / MP-scope (v2, deferred) planning, not an issue for this phase.

```csharp
namespace BossArenaSubWorld.Items
{
    public class BossCoreItem : ModItem
    {
        public string BossKey = string.Empty;

        public override bool CloneNewInstances => true;

        public override ModItem Clone(Item newEntity)
        {
            BossCoreItem clone = (BossCoreItem)base.Clone(newEntity);
            clone.BossKey = BossKey;
            return clone;
        }

        public override void SaveData(TagCompound tag) => tag["BossKey"] = BossKey;

        public override void LoadData(TagCompound tag) => BossKey = tag.GetString("BossKey");

        public override void SetDefaults()
        {
            Item.maxStack = 1;
            Item.consumable = true;
            Item.value = 0;
            Item.width = 20;
            Item.height = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = Item.useAnimation = 20;
        }

        public override bool? UseItem(Player player)
        {
            switch (BossRegistry.Apply(BossKey))
            {
                case ApplyResult.Applied:
                    Main.NewText($"Boss credential applied: {BossKey}", Color.LimeGreen);
                    return true; // consumable=true + return true -> item is consumed
                case ApplyResult.AlreadyDowned:
                    Main.NewText($"This boss was already marked defeated ({BossKey}).", Color.Yellow);
                    return true; // not a failure -- still consume (D-01: no-op, no error)
                case ApplyResult.UnknownKey:
                default:
                    Main.NewText($"Could not apply boss credential '{BossKey}' -- registry lookup failed. Item was not consumed; please report this.", Color.Red);
                    return false; // D-02: retain item on failure
            }
        }
    }
}
```

*Source: `Item.consumable`/`UseItem` return-value consumption relationship confirmed via [GitHub Issue #2580](https://github.com/tModLoader/tModLoader/issues/2580) discussion and cross-checked against `ModItem.UseItem` doc summary ("if item is consumable and UseItem returns true, item will be consumed... if false is returned, OnConsumeItem is never called") — MEDIUM-HIGH confidence (community-verified GitHub issue thread + docs, not directly reflectable since consumption logic lives in vanilla `Player`/`ItemLoader` IL bodies, not just signatures).*

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|--------------|-----|
| Subworld-gated conditional NPC loot | A manual `OnKill` check that calls `Item.NewItem` directly, bypassing the loot-rule system entirely | The custom `IItemDropRule` registered via `ModifyNPCLoot` (Pattern 2) | DROP-02 explicitly locks "via a conditional `ItemDropRule` added in `GlobalNPC.ModifyNPCLoot`" per `PROJECT.md` Key Decisions — already decided, not a gray area. Bypassing it via `OnKill` would also lose Bestiary/loot-table integration for free (rules automatically feed `NPCLoot`'s reporting/UI systems even for custom rules, since `ReportDroprates` is part of the same interface). |
| Vanilla boss-downed flag replication | `NPC.downedSlimeKing = true;` (raw assignment) | `NPC.SetEventFlagCleared(ref NPC.downedSlimeKing, gameEventId)` | Confirmed by the official Migration Guide: raw assignment skips the `MessageID.WorldData` netcode sync AND the Lantern Night event-trigger check that `SetEventFlagCleared` performs internally — exactly the failure mode `PITFALLS.md` Pitfall 4 and this phase's D-04 exist to prevent. |
| Item per-instance data propagation across inventory operations (split stacks, etc.) | Manually intercepting every inventory-mutation code path to copy a custom field | `CloneNewInstances = true` + `Clone(Item newEntity)` override | This is the documented, tModLoader-native mechanism for exactly this problem (`ExampleInstancedItem` in `ExampleMod` is the canonical reference, per prior `ARCHITECTURE.md` research) — reinventing it risks missing an inventory code path tModLoader's own `Clone` machinery already covers. |

**Key insight:** Every "don't hand-roll" item above has a name-brand tModLoader mechanism that already exists specifically because the underlying problem (dynamic per-kill loot conditions, boss-flag side-effect fidelity, per-instance item data) is common enough across all tModLoader mods that the framework solved it once, generically. The only genuinely custom code this phase needs is the ~20-line `BossCoreDropRule` class gluing two already-solved primitives (`IItemDropRule` + `Item.NewItem`) together for this project's specific subworld-gating requirement.

## Common Pitfalls

### Pitfall 1: Writing the subworld gate as an `if` inside `ModifyNPCLoot` (does nothing)

**What goes wrong:** `ModifyNPCLoot(NPC npc, NPCLoot npcLoot) { if (SubworldSystem.IsActive<BossArenaSubworld>() && npc.type == NPCID.KingSlime) npcLoot.Add(...); }` compiles and looks correct, but the drop rule gets added (or not added) **once, permanently, at mod load** — whichever subworld-active state happened to be true at startup (almost always `false`, since the mod hasn't loaded a world yet). The drop rule is then either always present or always absent for the rest of the game session, regardless of whether the player is actually in the subworld when a King Slime is later killed.
**Why it happens:** `ModifyNPCLoot` is confirmed (docs + reflection) to run once per NPC type during mod loading, not per kill. Any state that can change at runtime (subworld active/inactive) must be re-evaluated inside the drop rule's own `CanDrop`/`TryDroppingItem`, which tModLoader calls fresh on every actual kill.
**How to avoid:** Always add the drop rule unconditionally for registered boss NPC types in `ModifyNPCLoot`; put the dynamic subworld check inside the rule's `CanDrop(DropAttemptInfo info)` (see Pattern 2 above).
**Warning signs:** Killing King Slime in the main world (outside the subworld) also drops a `BossCoreItem`, or killing it inside the subworld never drops one — both indicate the gate got evaluated once at load instead of per-kill.

### Pitfall 2: Using a built-in rule (`ItemDropRule.Common` / `LeadingConditionRule`) and trying to set instance data afterward

**What goes wrong:** `ItemDropRule.Common(...)` and `LeadingConditionRule` are convenient for simple conditional drops, but their `TryDroppingItem` (confirmed via the decompiled-source patch for `CommonDrop.cs`) only returns an `ItemDropAttemptResult { State = ... }` — no reference to the spawned item, no index. There is no supported way to reach back into a built-in rule's internals to tag the item it just spawned with `BossKey`.
**Why it happens:** These rules are designed for stateless, un-tagged drops (the vast majority of vanilla/modded loot). Per-instance tagging at spawn time is an edge case the built-in rules don't need to support.
**How to avoid:** Implement a custom `IItemDropRule` (Pattern 2) that calls `Item.NewItem(...)` directly and captures its `int` return value, or fall back to `ModItem.OnSpawn(IEntitySource source)` reading an `EntitySource_Loot`-typed source to self-assign the key (a documented, viable alternative mechanism, though D-03's exact wording points at "inside the custom drop rule").
**Warning signs:** `BossCoreItem.BossKey` is always empty string / null after a kill, even though the item itself drops correctly.

### Pitfall 3: Calling `NPC.GetSource_Loot()` with no arguments

**What goes wrong:** Community wiki prose describes this as a parameterless call; the actual method (confirmed via reflection, inherited from `Terraria.Entity`) is `IEntitySource GetSource_Loot(string context)` — a required string parameter. Code copy-pasted from paraphrased wiki examples will fail to compile.
**How to avoid:** Always pass a short descriptive context string, e.g. `info.npc.GetSource_Loot("BossArenaSubWorld:BossCoreDrop")`.

### Pitfall 4: Assuming `Clone`/`CloneNewInstances` are declared on `ModItem` itself

**What goes wrong:** Searching `ModItem`'s own declared members (e.g. via IntelliSense scoped too narrowly, or a hasty grep of `ModItem.cs`) will not find `Clone`/`CloneNewInstances` — they're one level up, on the shared generic base `ModType<TEntity, TModType>` (confirmed via reflection: `ModItem : ModType<Item, ModItem> : ModType<Item> : ModType`). This is a non-issue for `override` (C# resolves inherited virtuals fine), but can cause confusion when searching decompiled/doc sources scoped only to `ModItem.cs`.
**How to avoid:** `override bool CloneNewInstances => true;` and `override ModItem Clone(Item newEntity) { ... }` both compile and work correctly as overrides despite being declared on the base class — just be aware when cross-referencing documentation/decompiled source that these members live on `ModType<Item, ModItem>`, not `ModItem.cs` itself.

### Pitfall 5: Forgetting `UseItem` is client-only when reasoning about `BossRegistry.Apply`'s server-authority

**What goes wrong:** Not a bug for this phase (singleplayer, client==server in-process), but a design trap if this pattern is naively copied into a future multiplayer-scoped phase: `ModItem.UseItem(Player player)` runs on the local client only, yet `BossRegistry.Apply` mutates a world-level flag that should be server-authoritative in MP.
**How to avoid this phase:** No action needed — MP is explicitly out of scope for v1 (`REQUIREMENTS.md` Out of Scope). Just don't treat this phase's `UseItem`-triggers-`Apply()` wiring as a pattern to copy verbatim into the deferred MP work without adding server-authority handling then.

## Code Examples

All code above (Patterns 1-3) is drawn from and verified against the actual installed API surface. One additional cross-cutting example — the `GlobalNPC` registration itself requires no `[Autoload]`/manual registration; tModLoader auto-discovers any `GlobalNPC`/`ModItem`/`ModSystem` subclass in the mod's assembly at load time (same auto-discovery already relied upon by every existing class in this codebase, e.g. `SummonItemRegistry`, `BossSummonPlayer`, `Test1Item`).

## Open Questions

1. **Exact `gameEventId` integer vanilla passes for King Slime's own internal `SetEventFlagCleared` call**
   - What we know: The method signature is 100% confirmed (`static void SetEventFlagCleared(ref bool eventFlag, int gameEventId)`), and the official tModLoader Migration Guide confirms `-1` is a safe, documented value that still produces the full flag+netcode-sync+Lantern-Night-trigger side-effect chain (`NPC.SetEventFlagCleared(ref myDownedBool, -1);` — used for *custom* mod boss flags in that example).
   - What's unclear: Whether vanilla's own internal King Slime kill-handling code passes a *different*, achievement-specific integer (Terraria does have a "King Slayer" achievement tied to defeating King Slime) that `-1` would skip notifying.
   - Recommendation: Use `-1` for this phase (matches this phase's stated success criteria: flag + pipeline correctness, not achievement-unlock parity — achievements are not named in DROP-01..APPLY-04 or the phase's Success Criteria). If full parity is desired later, the exact value requires IL-level decompilation of `Terraria.NPC`'s kill-handling method body (not reachable via `MetadataLoadContext`, which only exposes signatures/metadata, not method bodies) — e.g. via ILSpy/dnSpy against `D:\SteamLibrary\steamapps\common\tModLoader\tModLoader.dll`, following the same "verify against the real local binary" discipline this project already established in Phase 1/2 (see `01-02-SUMMARY.md`, `02-VERIFICATION.md`).

2. **Should `BossKillGlobalNPC` unconditionally add `BossCoreDropRule` for every registered boss, or only when `BossRegistry` has at least one entry?**
   - What we know: `BossRegistry.PostSetupContent()` (Pattern 1) registers `"vanilla:king_slime"` unconditionally, so `BossRegistry.TryGetKeyForNpc` will always have at least this one entry for this phase.
   - What's unclear: Not actually ambiguous for this phase — flagged only because Phase 4+ will need to confirm the same `ModifyNPCLoot` loop still works correctly once `BossRegistry` holds many more entries across several mods' NPC types (it will, since the lookup is by `npc.type` per call, independent of how many total entries exist) — no design change needed, just noting for planner awareness that this pattern scales without modification.
   - Recommendation: No action needed this phase; confirms Pattern 1/2 as written already generalize correctly.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| tModLoader.dll (compile-time reference source + reflection-verification target) | All API signatures in this phase; `Libs/SubworldLibrary.dll` extraction convention already established | ✓ | 1.4.4.9, confirmed at `D:\SteamLibrary\steamapps\common\tModLoader\tModLoader.dll` | — |
| .NET 8 SDK | `dotnet build`/`dotnet msbuild` | ✓ | 8.0.424 | — |

**Correction to a stale assumption:** `C:\Program Files (x86)\Steam\steamapps\common\tModLoader` (a *second*, separate Steam library on the C: drive) contains only a bundled `dotnet` runtime and log files — **not** the actual game/mod-loader binaries. The real, active tModLoader installation this project's `tModLoader.targets` import (`ModSources\tModLoader.targets` → `D:\SteamLibrary\steamapps\common\tMLMod.targets`) and `build.txt` resolution both depend on lives at **`D:\SteamLibrary\steamapps\common\tModLoader`**. This is not a blocker (the correct path is already hardcoded in the existing, working `tModLoader.targets` file, and `dotnet build BossArenaSubWorld.csproj` already succeeds per Phase 1/2 verification), but worth flagging so a future environment audit or fresh-machine setup doesn't waste time investigating the wrong Steam library path.

**Missing dependencies with no fallback:** None.
**Missing dependencies with fallback:** None — both dependencies are present and correctly resolved via the existing `Libs/SubworldLibrary.dll` extraction + `tModLoader.targets` import mechanism already established in this project.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None — tModLoader mod, no automated unit-test harness (confirmed: no test project/config exists anywhere in this repo; matches Phase 1/2's established `01-VALIDATION.md`/`02-VALIDATION.md` precedent) |
| Config file | none |
| Quick run command | `dotnet build BossArenaSubWorld.csproj` |
| Full suite command | N/A — no automated suite. "Full verification" = one live in-game playthrough of the phase's 5 Success Criteria in sequence, with a world backup taken first (per phase description and `PITFALLS.md` UX Pitfalls guidance) |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| DROP-01 | `BossRegistry` registers `"vanilla:king_slime"` -> `NPCID.KingSlime`, queryable by `npc.type` | build | `dotnet build BossArenaSubWorld.csproj` | ❌ Wave 0 (new file) |
| DROP-02 | Killing King Slime inside the subworld drops `BossCoreItem`; killing it outside does not | manual-only | live in-game kill test (in-subworld and, ideally, a negative check in the main world) | ❌ Wave 0 (new file), no harness for live NPC kill/loot simulation |
| DROP-03 | Dropped `BossCoreItem` carries `BossKey = "vanilla:king_slime"` across the subworld exit (inventory pickup -> return trip) | manual-only | live in-game: pick up item, exit subworld, inspect via a temporary debug print or tooltip in `BossCoreItem` (discretion item) | ❌ Wave 0 (new file) |
| APPLY-01 | Using `BossCoreItem` in the main world sets `NPC.downedSlimeKing = true` via `SetEventFlagCleared` | manual-only | live in-game: use item, check flag (e.g. via a Boss Checklist-style mod, or a temporary debug check) | ❌ Wave 0 (new file) |
| APPLY-04 | Re-using a `BossCoreItem` (or using a second one) after the flag is already set is a no-op (item still consumed, chat message differs, no error) | manual-only | live in-game: use a second `BossCoreItem` (obtain by killing King Slime again) after the first successful apply | ❌ Wave 0 (new file) |

### Sampling Rate
- **Per task commit:** `dotnet build BossArenaSubWorld.csproj` (0 warnings, 0 errors expected — matches Phase 1/2 convention)
- **Per wave merge:** Same build command; no separate full suite exists
- **Phase gate:** One live in-game checkpoint (world backup first) covering all 5 phase Success Criteria in sequence, mirroring `02-VALIDATION.md`'s "Manual-Only Verifications" checkpoint structure

### Wave 0 Gaps
- No automated test framework exists or is feasible for in-game tModLoader NPC/item/world-state behavior (same conclusion as Phase 1/2's `01-VALIDATION.md`/`02-VALIDATION.md`) — this is expected, not a gap to close.
- All new files (`Systems/BossRegistry.cs`, `GlobalNPCs/BossKillGlobalNPC.cs`, `ItemDropRules/BossCoreDropRule.cs`, `Items/BossCoreItem.cs`) do not yet exist — each task's automated verification is the `dotnet build` compile gate; live/manual verification is the phase-gate checkpoint described above.

*(No automated Wave 0 test-infrastructure gap beyond the build gate — matches this project's established, previously-approved manual-verification model for tModLoader mods.)*

## Sources

### Primary (HIGH confidence — verified directly against installed `tModLoader.dll` v1.4.4.9 via `System.Reflection.MetadataLoadContext`)
- `Terraria.NPC.SetEventFlagCleared(ref bool, int)` — exact signature confirmed
- `Terraria.Entity.GetSource_Loot(string context)` (inherited by `NPC`) — exact signature confirmed, corrects the wiki's parameterless paraphrase
- `Terraria.NPC.getRect()` — confirmed
- `Terraria.ModLoader.GlobalNPC.ModifyNPCLoot(NPC, NPCLoot)` and `.OnKill(NPC)` — confirmed
- `Terraria.GameContent.ItemDropRules.IItemDropRule` (`ChainedRules`, `CanDrop`, `TryDroppingItem`, `ReportDroprates`) — confirmed, full member list
- `Terraria.GameContent.ItemDropRules.LeadingConditionRule` — confirmed same interface members
- `Terraria.GameContent.ItemDropRules.DropAttemptInfo` fields (`npc`, `item`, `player`, `rng`, `IsInSimulation`, `IsExpertMode`, `IsMasterMode`) — confirmed
- `Terraria.GameContent.ItemDropRules.ItemDropAttemptResultState` enum (`DoesntFillConditions`, `FailedRandomRoll`, `Success`, `DidNotRunCode`) — confirmed, exact 4 values
- `Terraria.Item.NewItem(...)` — 11 overloads confirmed, including `NewItem(IEntitySource, Rectangle, int, int, ...)` used above
- `Terraria.Item.ModItem` property — confirmed `{ get; set; }`
- `Terraria.ModLoader.ModItem` full declared method/property list — confirmed (`UseItem`, `SaveData`, `LoadData`, `OnSpawn`, etc.)
- `Terraria.ModLoader.ModType<Item, ModItem>` — confirmed as the actual declaring type for `Clone(Item)` and `CloneNewInstances` (not `ModItem` itself)
- `Terraria.Item` fields (`consumable`, `maxStack`, `value`, `rare`, `useStyle`, `useTime`, `useAnimation`, `UseSound`, `width`, `height`) — confirmed
- Assembly version identity confirmed as tModLoader `1.4.4.9`, matching `research/STACK.md`

### Secondary (HIGH-MEDIUM confidence — official tModLoader wiki/GitHub, cross-checked against reflection where possible)
- [tModLoader Wiki: Update Migration Guide (Previous Versions)](https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide-Previous-Versions) — verbatim quote confirming `SetEventFlagCleared`'s flag+netcode-sync+Lantern-Night-trigger side effects and the `-1` argument convention
- [tModLoader Wiki: Basic NPC Drops and Loot 1.4](https://github.com/tModLoader/tModLoader/wiki/Basic-NPC-Drops-and-Loot-1.4) — `ModifyNPCLoot` "once per npc type at load" behavior, `LeadingConditionRule`/`IItemDropRuleCondition` usage patterns
- [tModLoader/tModLoader ExampleMod, `ExampleNPCLoot.cs` (1.4 branch)](https://github.com/tModLoader/tModLoader/blob/1.4/ExampleMod/Common/GlobalNPCs/ExampleNPCLoot.cs) — `LeadingConditionRule` usage example
- [tModLoader/tModLoader, `AlwaysAtleastOneSuccessDropRule.cs` (1.4.4 branch, full non-patch source)](https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/GameContent/ItemDropRules/AlwaysAtleastOneSuccessDropRule.cs) — real, complete `IItemDropRule` implementation confirming interface member shape (`CanDrop`, `TryDroppingItem`, `ReportDroprates`, `ChainedRules`) in situ
- [tModLoader/tModLoader, `CommonDrop.cs.patch` and `CommonCode.cs.patch` (1.4.4 branch)](https://github.com/tModLoader/tModLoader/tree/1.4.4/patches/tModLoader/Terraria/GameContent/ItemDropRules) — decompiled-source patch diffs confirming built-in rules do not expose the spawned item's index/instance back to the caller
- [tModLoader Wiki: IEntitySource](https://github.com/tModLoader/tModLoader/wiki/IEntitySource) — `NPC.GetSource_Loot()` vs `GetSource_DropAsItem()` usage guidance, `ModItem.OnSpawn(IEntitySource)` as an alternative spawn-time-tagging mechanism
- [GitHub Issue tModLoader/tModLoader #2580](https://github.com/tModLoader/tModLoader/issues/2580) — `Item.consumable` + `UseItem` return-value consumption relationship
- `.planning/research/ARCHITECTURE.md`, `.planning/research/PITFALLS.md`, `.planning/research/FEATURES.md`, `.planning/research/STACK.md` — prior project-wide research, source of the `BossRegistry`/`BossDefinition` sketch this phase implements and the D-04/Pitfall-4/Pitfall-5 fidelity requirements

### Tertiary (LOW confidence — flagged, not relied upon for any code example above)
- General WebSearch AI summaries of `docs.tmodloader.net` pages (used only to identify *which* primary sources to fetch/reflect against; every claim actually used in this document was cross-verified against either the reflected binary or a directly-quoted wiki/source excerpt, not left as an unverified summary)

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — no new dependencies, version confirmed via reflected assembly identity
- Architecture (drop rule mechanism): HIGH — every signature reflection-verified against the exact installed binary; the "ModifyNPCLoot runs once at load" finding is the phase's key architectural correction and is corroborated by both official docs prose and the reflected method signature
- Pitfalls: HIGH — Pitfalls 1-4 are all reflection/decompiled-source-confirmed; Pitfall 5 is a forward-looking design note, not a bug in this phase's scope
- King Slime `gameEventId` exact value: LOW — flagged explicitly as an Open Question with a sourced, safe fallback (`-1`)

**Research date:** 2026-08-13
**Valid until:** Stable for the life of tModLoader 1.4.4.9 (no fast-moving dependency in this phase) — re-verify only if the installed tModLoader version changes.
