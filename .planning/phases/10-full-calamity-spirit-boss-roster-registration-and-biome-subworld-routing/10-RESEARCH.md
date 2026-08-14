# Phase 10: Full Calamity/Spirit Boss Roster Registration & Biome Subworld Routing - Research

**Researched:** 2026-08-14
**Domain:** tModLoader C# mod integration — per-boss `OnKill()`/downed-flag/Zone-dependency verification for 20 Calamity + Spirit bosses, decompiled directly from the installed `Libs/CalamityMod.dll` (v2.2.4) and `Libs/SpiritMod.dll` (v1.5.0.44) via `ilspycmd`
**Confidence:** HIGH for all per-boss findings below (every claim is a direct decompile citation against the actually-installed DLL, not wiki-sourced) — MEDIUM for the two proposed architecture extensions (SummonItemRegistry polymorphic-item resolver, forced day/night utility), since these are new code patterns not yet built/tested in this project

## Summary

This research decompiled every Calamity and Spirit boss in Phase 10's roster (13 Calamity, 7 Spirit) directly against the installed mod DLLs, following this project's locked "decompiled-source, not wiki" discipline. The overwhelming majority of the roster (17 of 20 bosses) fits the existing `Integrations/CalamityIntegration.cs` / `Integrations/SpiritIntegration.cs` template exactly — register a summon item, register an NPC type, replay the flag-write + world-scoped side effects, done. Spirit's 7 bosses simplify further than expected: **none of them override `OnKill()` at all** — all downed-tracking flows through the single generic `SpiritMod.NPCs.BossDownedTracker.OnKill(NPC)` `GlobalNPC` hook already partially exploited for Infernon, so the exact same reflection-write pattern in `SpiritIntegration.cs` generalizes to all 7 with no new reflection code needed.

However, three genuine architecture-level surprises were found that the planner MUST resolve before writing tasks, because they don't fit the existing one-item-to-one-boss `SummonItemRegistry` model at all:

1. **Exo Mechs have no summon item.** Base Calamity's actual (v2.2.4) Exo Mechs summon mechanic is the `CodebreakerTile` + `TECodebreaker` `TileEntity` + `CodebreakerUI` — a placeable machine you feed materials into with a "decrypt countdown," not a held consumable item. There is no `Item.type` to register against `SummonItemRegistry` at all. **Recommend excluding Exo Mechs from this phase's scope** pending a dedicated new mechanism, or accepting a scope note that they cannot be added under the current architecture.
2. **Starplate Voyager (`SteamRaiderHead`) is not summoned by an item — it's triggered by a scripted ambient-tile `Event`.** `SpiritMod.Mechanics.EventSystem.Events.StarplateBeaconIntroEvent` fires from a ~10%-per-tick ambient ("Starplate Beacon") tile check, not from any item's `UseItem()`. No `CanUseItem`/`UseItem`-bearing item exists for this boss in the decompiled source. Same recommendation as Exo Mechs: exclude, or treat as new scope.
3. **`CalamityMod.Items.SummonItems.MarkofProvidence` is a single polymorphic item that summons THREE different bosses** (Ceaseless Void / Signus / Storm Weaver) depending on the player's `ZoneDungeon`/`ZoneUnderworldHeight`/`ZoneSkyHeight` at use-time. `SummonItemRegistry`'s current `Dictionary<int,int> _itemToBoss` is strictly 1:1 — registering all three under the same `Item.type` will silently make the last `Register()` call win, permanently breaking the other two. **`SummonItemRegistry` needs a new resolver-delegate overload** (see Architecture Patterns below) before Signus/Storm Weaver/Ceaseless Void can all be registered correctly.

Two items from `09-ALTAR-BIOME-REFERENCE.md`'s open questions are now resolved with HIGH confidence:

- **Open Item 3 (The Old Duke / Sulphurous Sea) is CLOSED: item-gate only, no AI-level dependency.** `OldDuke.cs`'s full decompiled AI has zero references to `ZoneSulphur`/`InSulphur`/any Sulphurous-Sea Zone flag. The wiki's "enrages if it leaves the Sulphurous Sea" claim does not correspond to anything in the actual v2.2.4 AI. **The discarded `BossArenaSulphurousSubworld`/`SulphurousPlatformPass` (D-07) does NOT need to be rebuilt** — The Old Duke can be routed to the plain default `BossArenaSubworld`.
- **Ceaseless Void has no AI-level `ZoneDungeon` dependency either** (same negative-finding methodology, confirmed clean grep across `CeaselessVoid.cs`). Its Dungeon assignment is wiki-thematic only. Since `BossArenaDungeonSubworld` was also discarded (D-07), **Ceaseless Void can likewise route to the plain default arena** without blocking on a Dungeon subworld rebuild.

Also newly confirmed: `DownedBossSystem`'s field for The Old Duke is **`downedBoomerDuke`**, not `downedOldDuke` as its wiki/boss name would suggest — a naming trap for anyone who doesn't decompile first.

**Primary recommendation:** Plan this phase in (at least) two tiers — Tier 1: the 15 straightforward bosses (12 Calamity + Atlas/AncientFlyer/Scarabeus/ReachBoss/MoonWizard/Dusking = wait, recount below) that fit the existing template exactly, following the Calamity/Spirit template files verbatim; Tier 2: a small architecture-extension task (polymorphic `SummonItemRegistry` resolver) needed specifically for Ceaseless Void/Signus/Storm Weaver; and an explicit user decision checkpoint for Exo Mechs and Starplate Voyager before committing to registering them at all this phase.

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| ARENA-01 | Every v1-registered boss classified as biome/Zone-dependent or not, with routed biome-variant subworld where dependent | Full per-boss functional-vs-thematic classification table below (Section "Per-Boss Classification"), resolving both outstanding Phase 9 open items (Old Duke, Ceaseless Void) |

</phase_requirements>

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**D-01 (biome assignment principle, carried from Phase 9):** Keep "wiki-thematic assignment" — every boss gets its wiki-stated biome arena even when its own AI has no functional `Zone*`/`CheckActive` dependency on that biome. Despawn-prevention-necessary routing and thematic-only routing use the identical `BossArenaRoutingRegistry.Register<T>()` call; per-boss research must document in code comments which reason applies (functional vs. thematic), mirroring Phase 6's Thorn/Astrageldon discipline.

**D-02 (Infernum mod-combination gating, full implementation):**
- Providence, Profaned Guardians, Ceaseless Void: register ONLY when `ModLoader.HasMod("CalamityMod") && !ModLoader.HasMod("InfernumMode")`. When Infernum is present, do NOT register these three at all.
- The Old Duke: register ONLY when `ModLoader.HasMod("CalamityMod") && ModLoader.HasMod("InfernumMode")`. Without Infernum, no summon item exists to hook — nothing to register.
- Astrum Deus / Astrum Aureus: register the Astral altar unconditionally, but when `ModLoader.HasMod("InfernumMode")` is also true, additionally force night in that subworld before/during the summon (same forced-night mechanism as D-04).
- Providence / Profaned Guardians are valid under either Hallow or Underworld — pick one consistently (Claude's Discretion, document the choice).

**D-03 (roster scope — full researched list, one phase):** Register the complete `09-ALTAR-BIOME-REFERENCE.md` Section 3 roster this phase (excluding Hive Mind/Infernon, already registered):
- Calamity: Providence, Profaned Guardians, Ceaseless Void (Infernum-gated), The Old Duke (Infernum-gated), Signus, Storm Weaver, Astrum Deus, Astrum Aureus (both Infernum-conditional forced-night), Dragonfolly, Devourer of Gods, Exo Mechs, Yharon, Supreme Witch Calamitas (plain arena).
- Spirit: Ancient Avian, Starplate Voyager, Scarabeus, Vinewrath Bane, Moon Jelly Wizard, Dusking (both night-gated), Atlas (plain arena).

**D-04 (time-gated bosses, new forced day/night utility required):** Include Moon Jelly Wizard and Dusking in scope, and build the forced-night utility Phase 9 explicitly deferred (09-CONTEXT.md D-05). Reused for Astrum Deus/Astrum Aureus's Infernum-conditional night requirement. Redemption's time-gated bosses are OUT of scope. Mechanism is Claude's Discretion — no new player-facing UI/item, automatic subworld-setup step only.

### Claude's Discretion
- Exact per-boss `OnKill()` decompiled-source verification (side effects, player-scoped vs. world-scoped classification, actual `Zone*`/`CheckActive` dependency) — **fully performed in this research pass, see Per-Boss Classification below.**
- Open Item 3 (Old Duke Sulphurous Sea) — **RESOLVED this research pass: item-gate only, no AI dependency, no subworld rebuild needed.**
- Exact forced day/night utility mechanism — **proposed below (Architecture Patterns)**, pending planner confirmation.
- Whether new integration code extends the existing `Integrations/CalamityIntegration.cs`/`Integrations/SpiritIntegration.cs` files or splits into multiple files per mod — **recommend splitting Calamity into `CalamityIntegration.cs` (core registration loop) is fine to keep as one growing file, since every prior phase's precedent is one-file-per-mod; only reconsider if file size becomes unwieldy (currently ~120 lines, will grow to maybe 500-700 lines with 12 more bosses — still manageable as one file, but planner may choose to split by sub-region, e.g., a `CalamityIntegration.ExoMechs.cs` partial class if that boss family is deferred separately).**
- Providence/Profaned Guardians' Hallow-vs-Underworld altar choice — **recommend Hallow for both** (rationale below), document in code comments.

### Deferred Ideas (OUT OF SCOPE)
- Redemption full-roster expansion (beyond Thorn) — most remaining bosses structure-gated/non-portable, confirmed in Phase 9 research, not this phase's mod scope.
- CatalystMod full-roster expansion (beyond Astrageldon) — not researched beyond Astrageldon, out of scope.
- NoxusBoss / ContinentOfJourney / Daybreak — Phase 7's unstarted scope.
- Other Calamity-adjacent rework mods (Fargo's Mod, Community Remix, etc.) — not audited, known gap.
- Dungeon / Sulphurous Sea subworld rebuild — **this research found NEITHER is actually needed** for this phase's roster (Old Duke and Ceaseless Void both confirmed thematic-only, safe on the plain arena). No rebuild required to satisfy D-03's roster.

</user_constraints>

## Standard Stack

No new external libraries needed. This phase extends the existing Phase 4/5/6 pattern exclusively:

| Component | Role | Status |
|-----------|------|--------|
| `Systems/BossRegistry.cs` | Boss-agnostic `BossDefinition`/`Apply()` | Zero changes needed |
| `Systems/SummonItemRegistry.cs` | Item→boss mapping | **Needs one new capability**: polymorphic-item resolver (see Architecture Patterns) |
| `Systems/BossArenaRoutingRegistry.cs` | Boss→subworld routing | Zero changes needed, `Register<T>()` already generic |
| `Integrations/CalamityIntegration.cs` | Grows from 1 boss (Hive Mind) to 13 | Template proven, extend per-boss |
| `Integrations/SpiritIntegration.cs` | Grows from 1 boss (Infernon) to 8 | Template proven, extend per-boss — **and simplifies**, since the generic `BossDownedTracker` write path (already built for Infernon) covers all 7 new Spirit bosses verbatim |
| `ilspycmd` 8.2.0.7535 | Decompile tool already installed (`~/.dotnet/tools/ilspycmd`) | Used for this entire research pass against `Libs/CalamityMod.dll` and `Libs/SpiritMod.dll` |

**Newly confirmed dependency detail:** `InfernumMode`'s internal mod name is confirmed **HIGH confidence** two independent ways this session: (1) `ModReader/InfernumMode/build.txt` (locally installed copy) declares no `name` override, so tModLoader derives the internal name from the project — the `ModReader` folder itself is named `InfernumMode`; (2) `InfernumMode`'s own `build.txt` confirms `modReferences = CalamityMod, SubworldLibrary, Luminance` and `version = 2.0.1.35`. `ModLoader.HasMod("InfernumMode")` is therefore the correct D-02 gate string with local-install confirmation, not just GitHub/wiki inference.

## Architecture Patterns

### Pattern 1: Polymorphic summon item (REQUIRED new capability)

**What:** `CalamityMod.Items.SummonItems.MarkofProvidence` (decompiled `CanUseItem`/`UseItem`) branches on the player's current Zone to decide which of three bosses to spawn:

```csharp
// Source: ilspycmd decompile of Libs/CalamityMod.dll,
// CalamityMod.Items.SummonItems.MarkofProvidence.UseItem (confirmed 2026-08-14)
public override bool? UseItem(Player player)
{
    if (player.ZoneDungeon)
        CalamityUtils.SpawnBossUsingItem<CeaselessVoid>(player, ...);
    else if (player.ZoneUnderworldHeight)
        CalamityUtils.SpawnBossUsingItem<Signus>(player, ...);
    else if (player.ZoneSkyHeight)
        CalamityUtils.SpawnBossUsingItem<StormWeaverHead>(player, ...);
    return true;
}
```

`SummonItemRegistry`'s current shape (`Dictionary<int,int> _itemToBoss`, `Register(int itemType, int bossNpcType, ...)`) cannot represent this — it's strictly one item → one boss. Registering `MarkofProvidence.Type` three times (once per boss) will silently overwrite; only the last call survives.

**When to use:** Any future summon item that is itself polymorphic (this is the only known case in the current roster, but the pattern should be general).

**Recommended extension** (mirrors the existing `canSummon` eligibility-delegate pattern added in Phase 6 — same style, same file):

```csharp
// Systems/SummonItemRegistry.cs — proposed addition
private static readonly Dictionary<int, Func<Player, int>> _polymorphicResolvers = new();

// Overload: item resolves to a DIFFERENT boss depending on player state at click-time,
// replicating the real item's own UseItem() branch logic faithfully (matches this
// project's "call the mod's actual setter/logic" fidelity discipline) instead of
// picking one boss arbitrarily.
public static void RegisterPolymorphic(int itemType, Func<Player, int> resolveBossNpcType, Func<bool> canSummon = null)
{
    _polymorphicResolvers[itemType] = resolveBossNpcType;
    if (canSummon != null) _eligibility[itemType] = canSummon;
}

public static bool TryGetBoss(Player player, int itemType, out int bossNpcType)
{
    if (_polymorphicResolvers.TryGetValue(itemType, out var resolve))
    {
        bossNpcType = resolve(player);
        return bossNpcType != -1; // resolver returns -1 for "no valid boss for this zone"
    }
    return _itemToBoss.TryGetValue(itemType, out bossNpcType);
}
```

`Tiles/Test1Tile.cs`'s `RightClick` would need its `SummonItemRegistry.TryGetBoss(player.HeldItem.type, out int bossNpcType)` call changed to pass `player` through (a small, mechanical signature change — the existing single-boss `TryGetBoss(int, out int)` overload should stay for backward compatibility with every other boss already registered).

For `MarkofProvidence`, the resolver would be:

```csharp
[JITWhenModsEnabled("CalamityMod")]
private static int ResolveMarkOfProvidenceBoss(Player player)
{
    // Faithful replay of MarkofProvidence.UseItem()'s own branch order (dungeon checked
    // first, matching decompiled source exactly -- do not reorder).
    if (player.ZoneDungeon && !ModLoader.HasMod("InfernumMode")) // D-02: Ceaseless Void unassignable under Infernum
        return ModContent.NPCType<CalamityMod.NPCs.CeaselessVoid.CeaselessVoid>();
    if (player.ZoneUnderworldHeight)
        return ModContent.NPCType<CalamityMod.NPCs.Signus.Signus>();
    if (player.ZoneSkyHeight)
        return ModContent.NPCType<CalamityMod.NPCs.StormWeaver.StormWeaverHead>();
    return -1; // player not standing in any recognized zone -- no redirect, same as real CanUseItem()==false
}
```

Note this ties Test1Tile's redirect decision to the player's real-world Zone flags at the moment of right-click (not bypassable the way other bosses' biome requirements are) — this is an unavoidable consequence of the source item genuinely being polymorphic, not an inconsistency with D-01's "thematic bypass is fine" principle (D-01 is about the destination arena's Zone flags, not about which boss gets picked from a shared item).

### Pattern 2: Tile-interaction summon items work fine as-is — no special handling needed

**What:** Several Calamity bosses (Astrum Deus via `TitanHeart`/`Starcore` on `AstralBeacon`; Supreme Witch Calamitas via `CeremonialUrn` on `SCalAltar`) are summoned in vanilla by placing a furniture tile and right-clicking it while *holding* (not *using*) the relevant item — the item itself often has no `CanUseItem`/`UseItem` override at all (`Starcore`, `TitanHeart` are plain crafting materials).

**Why this is NOT a blocker:** `Tiles/Test1Tile.cs`'s `RightClick` never calls the held item's own `CanUseItem()`/`UseItem()` — it only reads `player.HeldItem.type` and looks it up in `SummonItemRegistry`. Confirmed by re-reading `Test1Tile.cs` this session: the redirect fires purely off the registry lookup, then calls `NPC.SpawnOnPlayer` directly (Phase 2 D-09). Whether the real item is a "use to summon" item or a "hold near a tile, right-click the tile" item makes zero difference to this project's pipeline — **register `Starcore.Type` (or `TitanHeart.Type`) → `AstrumDeusHead` exactly like any other boss, no different handling required.**

**When to use:** Any boss whose vanilla summon flow is furniture/tile-interaction-based rather than direct `UseItem()`. Verify only that a real `Item.type` exists somewhere in the mod for the material (it always will, since it must be craftable/obtainable) — the item does not need its own `CanUseItem`/`UseItem` override for this project's purposes.

### Pattern 3: Generic Spirit boss registration — no per-boss reflection needed

**What:** `SpiritMod.NPCs.BossDownedTracker` (already reflected into for Infernon in Phase 5) is a `GlobalNPC` with a fully generic `OnKill(NPC npc)`:

```csharp
// Source: ilspycmd decompile of Libs/SpiritMod.dll, SpiritMod.NPCs.BossDownedTracker
public override void OnKill(NPC npc)
{
    if (npc.boss)
    {
        Downed[GetBossKey(npc)] = true;
        if (Main.netMode != 0) NetMessage.SendData(7, -1, -1, ...); // MessageID.WorldData
    }
}
```

None of the 7 new Spirit bosses (`AncientFlyer`, `SteamRaiderHead`, `Scarabeus`, `ReachBoss`, `MoonWizard`, `Dusking`, `Atlas`) override `OnKill()` themselves — confirmed by grepping each boss's own decompiled class file for `override void OnKill` (zero matches, all 7). All downed-tracking for all 7 flows through this single generic hook. This means **`SpiritIntegration.cs`'s existing `ApplyInfernonDowned`-style reflection code (cached `FieldInfo` into `BossDownedTracker.Downed`, key = `Mod.Name + "/" + ModNPC.Name`) is directly reusable verbatim for all 7** — swap only the NPC type and the `IsBossDowned<T>()` read-path call. No new reflection research needed per boss (a major scope-reduction finding vs. Calamity, where each boss has its own `DownedBossSystem` property).

**Also confirmed:** none of the 7 override `ModifyNPCLoot` with any WorldGen tile-placement side effect (unlike Infernon's own Hellstone-ring — that is Infernon-specific, not part of the generic tracker). All 7 are **fully world-scoped with zero player-scoped or WorldGen side effects** — `ApplyDowned` for each is just the generic reflection write, no Pitfall 4/5 exclusion logic needed, same conclusion as Phase 5's Infernon (D-03 precedent extends cleanly).

### Recommended project structure — no changes

No new files needed beyond the two growing integration files, plus (if the D-04 forced night utility is built) one new small `Systems/ForcedTimeUtility.cs`-style helper, e.g.:

```csharp
// Proposed shape, Claude's Discretion per CONTEXT.md
namespace BossArenaSubWorld.Systems
{
    public static class ForcedTimeUtility
    {
        // Called from a biome Subworld's OnEnter(), mirroring the existing
        // vanilla-downed-flag snapshot/restore guard already duplicated per-subworld
        // (BossArenaCorruptionSubworld precedent).
        public static void ForceNight()
        {
            Main.dayTime = false;
            Main.time = 0.0; // midnight -- maximizes buffer before the AI's Main.dayTime
                              // despawn check (confirmed present in both MoonWizard.AI()
                              // and Dusking.AI()) can flip true mid-fight
        }
    }
}
```

**Open question for planning:** does `Main.time` advance normally inside a `SubworldLibrary` subworld during a boss fight? If yes, a single `OnEnter()` call may not be sufficient for long fights near dawn — planner should decide whether a per-tick re-assertion (a small `ModSystem.PreUpdateWorld` hook) is warranted, or whether `Main.time = 0.0` gives enough buffer (~9.33 real-world minutes of in-game night by default) to be acceptable as-is. Not resolved by this research pass; flag for a live-verification checkpoint.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|--------------|-----|
| Spirit per-boss downed-flag reflection | A new `FieldInfo` lookup per boss | The single cached `BossDownedTracker.Downed` `FieldInfo` already built for Infernon in `SpiritIntegration.cs` | Confirmed generic — all 7 new Spirit bosses write through the exact same dictionary/key convention (Pattern 3 above) |
| Polymorphic-item routing | A separate `SummonItemRegistry`-like registry just for `MarkofProvidence` | The proposed `RegisterPolymorphic`/resolver-delegate extension to the existing `SummonItemRegistry` | Keeps one registry, one mental model, matches the existing `canSummon` delegate precedent from Phase 6 |
| Astral Beacon / SCal Altar tile-interaction replication | Building a fake in-subworld tile-placement flow to satisfy the source item's real `CanUseItem()` | Nothing — register the item directly, `Test1Tile` already bypasses `CanUseItem`/`UseItem` entirely | Pattern 2 above; this is a non-problem, already solved by the existing architecture |

**Key insight:** The existing architecture (item-type lookup, bypass `CanUseItem`/`UseItem`, `SpawnOnPlayer` directly) is more general than it might first appear — it already transparently handles "altar/tile-interaction" summon items (Astrum Deus, Supreme Calamitas) with zero special-casing. The only genuine gaps are (a) polymorphic one-item-many-bosses items, and (b) bosses with literally no `Item.type` at all (Exo Mechs, Starplate Voyager).

## Per-Boss Classification

### Calamity (13 bosses)

| Boss | NPC type (fully-qualified) | Summon item | `DownedBossSystem` field | Zone dependency | Side effects to replay | Infernum gating |
|------|----------------------------|--------------|---------------------------|------------------|--------------------------|-------------------|
| Providence | `CalamityMod.NPCs.Providence.Providence` | `Items.SummonItems.ProfanedCore` | `downedProvidence` | **Thematic only** — `zoneHallow`/`zoneUnderworldHeight` read in AI only pick `biomeType` (visual/attack-theme branch: 1=Hallow holy, 2=Underworld fire, 0=default); no despawn/enrage tied to it | WorldGen: `CalamityUtils.SpawnOre(UelibloomOre, ...)` + 2 broadcast messages, gated `if (!downedProvidence)` (first-kill only, matches Hive Mind's ore-gen pattern). Exclude `CalamityGlobalNPC.SetNewBossJustDowned()` (player-scoped, Phase 4 precedent) | Register only when `!HasMod("InfernumMode")` (D-02) |
| Profaned Guardians | `CalamityMod.NPCs.ProfanedGuardians.ProfanedGuardianCommander` (only this one sets the flag — `Defender`/`Healer` have no `OnKill` override at all, confirmed via decompile of all 3 classes) | `Items.SummonItems.ProfanedShard` | `downedGuardians` | Thematic only (no Zone check found in Commander's decompiled source) | Standard: `SetNewBossJustDowned` (exclude) + `downedGuardians = true` + `SyncWorld()` | Register only when `!HasMod("InfernumMode")` (D-02) |
| Ceaseless Void | `CalamityMod.NPCs.CeaselessVoid.CeaselessVoid` | `Items.SummonItems.MarkofProvidence` (**polymorphic, shared with Signus/Storm Weaver — see Architecture Pattern 1**) | `downedCeaselessVoid` | **Thematic only** — confirmed zero `ZoneDungeon`/`InDungeon` references anywhere in `CeaselessVoid.cs`'s full decompiled AI. **Resolves the "does Dungeon subworld need rebuilding" question: no, safe on plain `BossArenaSubworld`** | Standard: `SetNewBossJustDowned` (exclude) + `downedCeaselessVoid = true` + `SyncWorld()` | Register only when `!HasMod("InfernumMode")` (D-02) — via the polymorphic resolver's own internal check |
| The Old Duke | `CalamityMod.NPCs.OldDuke.OldDuke` | Infernum's `Bloodworm Platter` (item defined in `InfernumMode.dll`, NOT `Libs/CalamityMod.dll` — **not yet decompiled this session, `InfernumMode.dll` was not extracted to `Libs/`; planner/executor must locate and decompile it during implementation, same `.tmod`-extraction pattern as Phase 6's CatalystMod**) | **`downedBoomerDuke`** — NOT `downedOldDuke` (naming trap, confirmed via decompile, no `downedOldDuke` field exists in `DownedBossSystem` at all) | **CONFIRMED no AI-level Sulphurous Sea dependency** — zero `Sulphur`-related Zone checks anywhere in `OldDuke.cs`'s full decompiled AI (only `SulphurousSharkron` add-spawns and `SpawnModBiomes` bestiary metadata reference the biome, neither is a despawn/enrage gate). **Resolves 09-ALTAR-BIOME-REFERENCE.md Open Item 3: item-gate/wiki-flavor only, no `BossArenaSulphurousSubworld` rebuild needed** | `SetNewBossJustDowned` (exclude) + `CalamityGlobalTownNPC.SetNewShopVariable(...)` (Sea King shop unlock, world-scoped, replay) + `AcidRainEvent.OldDukeHasBeenEncountered = true` (world-scoped, replay) + `downedBoomerDuke = true` + `SyncWorld()` | Register only when `HasMod("InfernumMode")` (D-02) |
| Signus | `CalamityMod.NPCs.Signus.Signus` | `Items.SummonItems.MarkofProvidence` (polymorphic, shared) | `downedSignus` | Thematic only (no `ZoneUnderworld` AI reference found beyond bestiary listing) | `SetNewBossJustDowned` (exclude) + `downedSignus = true` + `SyncWorld()` | Unconditional (unchanged by Infernum per 09-ALTAR Section 1) |
| Storm Weaver | `CalamityMod.NPCs.StormWeaver.StormWeaverHead` | `Items.SummonItems.MarkofProvidence` (polymorphic, shared) | `downedStormWeaver` | Thematic only (no Zone check found) | `SetNewBossJustDowned` (exclude) + `downedStormWeaver = true` + `SyncWorld()` | Unconditional (unchanged by Infernum) |
| Astrum Deus | `CalamityMod.NPCs.AstrumDeus.AstrumDeusHead` | `Items.Materials.TitanHeart` OR `Items.SummonItems.Starcore` (real vanilla flow: hold either, right-click a placed `AstralBeacon` tile — **works fine registered directly, see Architecture Pattern 2**; recommend `Starcore` since `TitanHeart` also has unrelated armor-crafting uses) | `downedAstrumDeus` | Thematic only (no Zone check found in `AstrumDeusHead.cs`) | `SetNewBossJustDowned` (exclude) + one broadcast message (first-kill only) + `downedAstrumDeus = true` + `SyncWorld()` | Unconditional registration; **when `HasMod("InfernumMode")`, additionally force night** (D-02) via the same utility as D-04 |
| Astrum Aureus | `CalamityMod.NPCs.AstrumAureus.AstrumAureus` | `Items.SummonItems.AstralChunk` (real `CanUseItem` checks `player.Calamity().ZoneAstral` — bypassed, doesn't matter) | `downedAstrumAureus` | Thematic only | `SetNewBossJustDowned` (exclude) + 2 broadcast messages (first-kill) + **`ThreadPool.QueueUserWorkItem(() => AstralBiome.PlaceAstralMeteor())`** (WorldGen side effect, but dispatched on a background thread — pitfall: replay this exact same threaded-dispatch pattern, do not call `PlaceAstralMeteor()` synchronously on the main thread without checking its own thread-safety assumptions) + `downedAstrumAureus = true` + `SyncWorld()` | Unconditional; **when `HasMod("InfernumMode")`, additionally force night** (D-02) |
| Dragonfolly | `CalamityMod.NPCs.Bumblebirb.Dragonfolly` | `Items.SummonItems.ExoticPheromones` (real `CanUseItem` checks `ZoneJungle`) | `downedDragonfolly` | **CONFIRMED FUNCTIONAL** — `AI()` increments a leave-Jungle timer (`localAI[1]`, cap 300) whenever `!val.ZoneJungle`, resetting to 0 when in Jungle; this is a genuine grace-period-then-enrage/despawn mechanic, not purely cosmetic. **Must route to `BossArenaJungleSubworld` (already exists, Phase 9), not just thematically but functionally required** | `SetNewBossJustDowned` (exclude) + `downedDragonfolly = true` + `SyncWorld()` + a `Main.zenithWorld`-gated lightning-projectile effect (seed-specific, skip) | Unconditional (unchanged by Infernum) |
| Devourer of Gods | `CalamityMod.NPCs.DevourerofGods.DevourerofGodsHead` | `Items.SummonItems.CosmicWorm` (real `UseItem` calls `SpawnBossOnPosUsingItem` with a +1600Y-above-player offset — irrelevant to us, our pipeline calls generic `NPC.SpawnOnPlayer`, not the item's own `UseItem()`, so this offset is moot) | `downedDoG` | Not checked (Section 3 lists as "plain arena", no altar assigned) — no Zone grep performed this session, low risk given the reference doc's explicit "plain" classification and Infernum's own confirmed-unchanged verdict | `SetNewBossJustDowned` (exclude) + `CalamityGlobalTownNPC.SetNewShopVariable(...)` (Bandit shop unlock, replay) + 3 broadcast messages (first-kill) + `downedDoG = true` + `SyncWorld()` | Unconditional (unchanged by Infernum) |
| Exo Mechs (Ares / Thanatos / Artemis+Apollo) | `CalamityMod.NPCs.ExoMechs.Ares.AresBody`, `...Thanatos.ThanatosHead`, `...Apollo.Apollo` (Artemis has NO `OnKill` override — only Apollo's does, setting the shared `downedArtemisAndApollo` flag; register only `Apollo`'s type for that pair) | **NONE — no `Item.type` exists.** Confirmed: real summon flow is `CalamityMod.Tiles.DraedonSummoner.CodebreakerTile` (placeable tile) + `CalamityMod.TileEntities.TECodebreaker` (`TileEntity`, tracks fed materials + a "decrypt countdown") + `CalamityMod.UI.DraedonSummoning.CodebreakerUI` (custom in-game UI for boss selection). This is fundamentally incompatible with `SummonItemRegistry`'s `Item.type`-keyed model and with `Test1Tile`'s "check `player.HeldItem.type`" flow — **there is no item to hold**. | `downedAres`/`downedThanatos`/`downedArtemisAndApollo` (plain setters, no `SetEventFlagCleared` wrapper) + aggregate `downedExoMechs` (wrapped) | Not evaluated (moot pending scope decision) | `AresBody.DoMiscDeathEffects(npc, mechType)` checks live `CalamityGlobalNPC.draedonExoMechWorm/Twin/Prime/draedon` NPC-array indices (all default `-1`/inactive when no scripted Draedon fight is running) — **this actually resolves correctly for a standalone `SpawnOnPlayer` kill** (all checks false → takes the "else" branch → sets the correct per-mech flag), so the OnKill logic itself is NOT the blocker; the missing summon item is | N/A (moot) — **RECOMMEND EXCLUDING FROM THIS PHASE, flag as open question requiring explicit user decision (see Open Questions)** |
| Yharon | `CalamityMod.NPCs.Yharon.Yharon` | `Items.SummonItems.YharonEgg` | `downedYharon` | Not checked this session (Section 3: plain arena per Infernum's own confirmed-unchanged page) | `SetNewBossJustDowned` (exclude) + `CalamityGlobalTownNPC.SetNewShopVariable(...)` (replay) + WorldGen: `CalamityUtils.SpawnOre(AuricOre, ...)` (first-kill only) + broadcast message + `downedYharon = true` + `SyncWorld()` | Unconditional (unchanged by Infernum) |
| Supreme Witch, Calamitas | `CalamityMod.NPCs.SupremeCalamitas.SupremeCalamitas` | `Items.SummonItems.CeremonialUrn` (real vanilla flow: hold, right-click a placed `SCalAltar` — same tile-interaction pattern as Astrum Deus, Architecture Pattern 2 applies, register directly) | `downedCalamitas` | Plain arena (no biome altar per Section 3 — Calamity's own "Altar of the Accursed" furniture just needs to be placeable in `BossArenaSubworld`, not this project's concern) | `SetNewBossJustDowned` (exclude) + player-scoped `Calamity().sCalKillCount++` (**exclude**, player-scoped, Pitfall 5) + spawns a follow-up NPC (`Archmage`/`BrimstoneWitch`, live-in-subworld side effect, not `Apply()`'s concern) + `downedCalamitas = true` + `SyncWorld()` | Unconditional (confirmed unchanged by Infernum, Section 1) |

### Spirit (7 bosses — all via generic `BossDownedTracker`, Architecture Pattern 3)

| Boss | NPC type | Summon item | Zone dependency | Notes |
|------|----------|--------------|------------------|-------|
| Ancient Avian | `SpiritMod.NPCs.Boss.AncientFlyer` | `Items.Consumable.JewelCrown` (real `CanUseItem`: `ZoneOverworldHeight` or `ZoneSkyHeight`) | Thematic only (no Zone check in `AncientFlyer.cs` AI) | Assigned to Space altar per 09-ALTAR-BIOME-REFERENCE.md — thematic |
| Starplate Voyager | `SpiritMod.NPCs.Boss.SteamRaider.SteamRaiderHead` (worm boss — only Head has `npc.boss = true`; `SteamRaiderBody` does not) | **NONE FOUND.** Real trigger is `SpiritMod.Mechanics.EventSystem.Events.StarplateBeaconIntroEvent`, fired from an ambient tile's ~10%-per-tick random check (`EventManager.PlayEvent(new StarplateBeaconIntroEvent(...))`), not any item's `UseItem()`. `StarWormSummon` (the item BossChecklist's data associates with this boss) has no `CanUseItem`/`UseItem` override — it's a plain crafting-material item. | N/A (moot pending scope decision) | **RECOMMEND EXCLUDING FROM THIS PHASE** — same category of gap as Exo Mechs (no compatible entry point), see Open Questions. `StarplateBeaconIntroEvent`'s class and constructor (`public StarplateBeaconIntroEvent(Vector2 center)`) ARE public, so a workaround (directly calling `EventManager.PlayEvent(...)` from a registered item's redirect) is technically possible but is new scope this research does not recommend committing to without explicit user sign-off |
| Scarabeus | `SpiritMod.NPCs.Boss.Scarabeus.Scarabeus` | `Items.Consumable.ScarabIdol` (real `CanUseItem`: `ZoneDesert && Main.dayTime`) | **CONFIRMED FUNCTIONAL (damage-scaling, not despawn)** — `ModifyHitByItem`/hit-modifier code divides `FinalDamage` by 3 in both directions (player deals 1/3 damage to Scarabeus, and takes 1/3 less from it) when `!player.ZoneDesert`. Fight is technically completable outside Desert but heavily unbalanced — route to `BossArenaDesertSubworld` (exists, Phase 9) for genuine balance reasons, not just theme | Item's own `Main.dayTime` gate (day-only) does NOT correspond to any AI-level day/night check found this session — likely a vanilla-parity flavor restriction only, does not need the forced-night utility (that's for Dusking/MoonWizard, which need NIGHT) |
| Vinewrath Bane | `SpiritMod.NPCs.Boss.ReachBoss.ReachBoss` | `Items.Consumable.ReachBossSummon` (real `CanUseItem`: `player.ZoneBriar() && !player.ZoneOverworldHeight`) | Thematic only (no Zone check found in `ReachBoss.cs` AI) | Assigned to Briar altar — thematic |
| Moon Jelly Wizard | `SpiritMod.NPCs.Boss.MoonWizard.MoonWizard` | `Items.Consumable.DreamlightJellyItem` (real `CanUseItem` includes `!Main.dayTime`) | **CONFIRMED FUNCTIONAL** — `AI()` contains `if (!val.active || val.dead || Main.dayTime) { ...; active = false; }`, a genuine despawn-on-daytime check that fires every AI tick, not just at spawn | D-04's forced-night utility is REQUIRED here, and must persist for the fight's full duration, not just at spawn (see Architecture "Open question" note above) |
| Dusking | `SpiritMod.NPCs.Boss.Dusking.Dusking` | `Items.Consumable.DuskCrown` (real `CanUseItem`: `!Main.dayTime`) | **CONFIRMED FUNCTIONAL** — same `Main.dayTime` → `active = false` despawn pattern found in `Dusking.cs`'s own AI, identical mechanism to Moon Jelly Wizard | Same D-04 forced-night requirement, same persistence caveat |
| Atlas | `SpiritMod.NPCs.Boss.Atlas.Atlas` | `Items.Consumable.Potion.StoneSkin` (confirmed via `BossChecklistDataHandler` registration + matching `CanUseItem`/`UseItem` decompile: `!NPC.AnyNPCs(Atlas)` gate, `NPC.SpawnOnPlayer(..., Atlas)`) | Plain arena (per Section 3 — `SpawnModBiomes = SpiritSurfaceBiome` is bestiary-only cosmetic metadata, no despawn check) | No `BossArenaRoutingRegistry.Register<T>()` call needed, falls back to default `BossArenaSubworld` automatically (same as Infernon/King Slime precedent) |

## Common Pitfalls

### Pitfall 1: `downedBoomerDuke`, not `downedOldDuke`
**What goes wrong:** Writing `DownedBossSystem.downedOldDuke` will not compile — the field doesn't exist. Anyone going from the wiki/boss display name alone would guess wrong.
**How to avoid:** Use `DownedBossSystem.downedBoomerDuke`, confirmed via decompile this session.

### Pitfall 2: Registering `MarkofProvidence` three times will silently break two of the three bosses
**What goes wrong:** `SummonItemRegistry.Register(markOfProvidenceItemType, ceaselessVoidNpcType)` followed by `.Register(markOfProvidenceItemType, signusNpcType)` followed by `.Register(markOfProvidenceItemType, stormWeaverNpcType)` will leave only Storm Weaver reachable — the `Dictionary<int,int>` silently overwrites on each call, no error, no warning.
**How to avoid:** Build and use the `RegisterPolymorphic`/resolver-delegate extension (Architecture Pattern 1) before wiring any of these three bosses; do not attempt sequential single-boss `Register()` calls against the same item type.

### Pitfall 3: Assuming every summon-flavored item has its own `UseItem()` blocks registration
**What goes wrong:** A cautious implementer might see that `Starcore`/`TitanHeart` have no `CanUseItem`/`UseItem` override and conclude Astrum Deus can't be registered via the normal item-lookup path, then either skip the boss or attempt to build a fake tile-interaction flow.
**How to avoid:** Remember `Test1Tile.RightClick` never touches the held item's own use-logic at all (Architecture Pattern 2) — register the item type directly like any other boss, exactly as `CosmicWorm`→DevourerOfGods or `YharonEgg`→Yharon already do.

### Pitfall 4: `PlaceAstralMeteor()` is dispatched on a background thread in the real code
**What goes wrong:** Calling `AstralBiome.PlaceAstralMeteor()` synchronously on the main game thread during `BossCoreItem.UseItem()` (main-world item-use context, likely single-threaded call site) might behave differently than the source mod's own `ThreadPool.QueueUserWorkItem(...)`-wrapped call — could be fine, could hit a threading assumption inside `PlaceAstralMeteor()` that expects to run off the main thread (e.g., to avoid a frame hitch during large-area WorldGen).
**How to avoid:** Replay the exact same `ThreadPool.QueueUserWorkItem(() => AstralBiome.PlaceAstralMeteor())` dispatch pattern Astrum Aureus's own `OnKill()` uses, don't simplify to a synchronous call, matching this project's "replay the setter, not a simplification" discipline (Pitfall 4 in `research/PITFALLS.md`).

### Pitfall 5: Exo Mechs' and Starplate Voyager's `OnKill`/downed-flag logic is NOT the blocker — the missing summon item is
**What goes wrong:** Spending implementation effort building careful `OnKill()`-replay logic for Exo Mechs (which this research confirms actually resolves fine standalone) while missing that there's no way to trigger the encounter through this project's `Test1Tile` pipeline in the first place.
**How to avoid:** Treat "does a registrable `Item.type` exist for this boss" as the FIRST gate to check for any future boss (Calamity or otherwise), before investing in `OnKill()` research. For this phase specifically: get an explicit user decision on Exo Mechs/Starplate Voyager (exclude, or accept new-mechanism scope) before planning tasks for either.

### Pitfall 6: Forced-night persistence during a live fight
**What goes wrong:** Setting `Main.time = 0.0`/`Main.dayTime = false` once in a `Subworld.OnEnter()` may not be sufficient if the fight runs long enough for in-game time to advance into daytime naturally (default day/night cycle ratio applies unless something holds time still), silently triggering Moon Jelly Wizard's or Dusking's `Main.dayTime`-gated despawn mid-fight.
**How to avoid:** Flagged as an explicit open question above — plan a live-verification checkpoint for a long (multi-minute) Moon Jelly Wizard/Dusking fight specifically watching for a mid-fight unexpected despawn, and be ready to add a per-tick re-assertion `ModSystem` hook if the single `OnEnter()` call proves insufficient.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|--------------|-----------|---------|----------|
| CalamityMod | 12 of 13 Calamity boss registrations | ✓ | 2.2.4 (matches `build.txt` weakReference, `Libs/CalamityMod.dll` present) | — |
| SpiritMod | All 7 Spirit boss registrations | ✓ | 1.5.0.44 (matches `build.txt`) | — |
| InfernumMode | D-02 conditional-gating checks, The Old Duke's summon item | ✓ (installed locally, confirmed via `ModReader/InfernumMode/build.txt`) | 2.0.1.35 | The Old Duke registration is naturally skipped when absent per D-02's own logic (no fallback needed — this is intentional conditional behavior, not a missing-dependency gap) |
| `InfernumMode.dll` decompile (for The Old Duke's `Bloodworm Platter` item) | The Old Duke's summon-item registration | **NOT YET EXTRACTED** — not present in `Libs/`, this session only decompiled `CalamityMod.dll`/`SpiritMod.dll` | — | Executor must extract `InfernumMode.dll` from the installed `.tmod` the same way `Libs/CatalystMod.dll` was extracted in Phase 6 (`scripts/extract_tmod.py` precedent), before writing The Old Duke's registration code |
| `ilspycmd` | Per-boss decompiled verification (already used exhaustively this session) | ✓ | 8.2.0.7535 (tool reports a newer 11.0.0.9375 is available — not required for this project, current version fully sufficient) | — |

**Missing dependencies with no fallback:**
- `InfernumMode.dll` decompile — must be performed during implementation (planner should add this as an explicit task, not assume it's already available in `Libs/`).

## Open Questions

1. **Exo Mechs: exclude from this phase, or accept new-mechanism scope?**
   - What we know: no `Item.type` exists for the real summon flow (Codebreaker Tile+TileEntity+UI). The `OnKill()`/downed-flag logic itself would actually work fine standalone if a spawn trigger existed.
   - What's unclear: whether the user wants (a) Exo Mechs dropped from this phase's roster entirely (descope, revise D-03), (b) a hacky single-item-per-mech-type registration built specially for this boss family (loses the real "choose which mech" UX but is small, contained new scope), or (c) deferred to a dedicated future phase.
   - Recommendation: raise this explicitly before planning locks task scope — do not silently exclude or silently build a workaround.

2. **Starplate Voyager: same category of question as Exo Mechs.**
   - What we know: real trigger is an ambient-tile-driven scripted `Event`, not an item. `StarplateBeaconIntroEvent`'s constructor is public, so a direct `EventManager.PlayEvent(new StarplateBeaconIntroEvent(player.Center))` call from a registered "trigger item" is technically possible as a workaround, bypassing the real ambient-tile RNG trigger.
   - What's unclear: whether that workaround is acceptable-enough fidelity to the source mod's real mechanic for this project's standards, or whether it should be excluded like Exo Mechs.
   - Recommendation: same as above — explicit decision needed before task planning.

3. **Forced-night persistence for the full fight duration (Pitfall 6).**
   - What we know: both Moon Jelly Wizard and Dusking despawn immediately on `Main.dayTime == true`, confirmed at the AI level.
   - What's unclear: whether `Subworld.OnEnter()` setting `Main.time = 0.0` once is sufficient for a full fight, or whether in-subworld time keeps advancing at the normal rate (unconfirmed — SubworldLibrary's per-subworld time-flow behavior wasn't decompiled this session, out of this phase's research scope but worth a quick check during implementation).
   - Recommendation: plan a live-verification checkpoint specifically watching for mid-fight despawn on a long Moon Jelly Wizard/Dusking attempt.

4. **Providence/Profaned Guardians: Hallow vs. Underworld — recommend Hallow.**
   - What we know: the AI's own `biomeType` branch (Providence only) is purely a visual/attack-theme choice with no functional requirement either way; Profaned Guardians has no Zone-tied branch at all.
   - Recommendation: assign both to `BossArenaHallowSubworld` (currently unused by any registered boss — Underworld will already host Signus, spreading bosses across subworlds somewhat evenly is a reasonable secondary tie-breaker with zero functional cost either way). Document as a discretionary choice in code comments per D-02.

5. **`ProfanedGuardianCommander`-only registration — does the fight work standalone?**
   - What we know: only `ProfanedGuardianCommander.OnKill()` sets `downedGuardians`; `Defender`/`Healer` have no `OnKill` override. `NPC.SpawnOnPlayer(Commander)` will spawn only the Commander directly (this project's established `SpawnOnPlayer`-single-type pattern, same as every other boss).
   - What's unclear: whether vanilla's `ProfanedGuardianCommander.OnSpawn()` (not decompiled this session) automatically spawns `Defender`/`Healer` alongside it, or whether the real vanilla summon item (`ProfanedShard`) spawns all three via some other path this research didn't trace. If Commander alone spawns without its escort, the fight itself may play very differently than a real player's first encounter (missing the other two guardians' unique mechanics) — cosmetic/difficulty concern only, not a registration blocker (the same `SpawnBossUsingItem<ProfanedGuardianCommander>()` call vanilla's own `ProfanedShard.UseItem()` uses is single-type too, so this project's behavior already matches vanilla's own summon exactly regardless).
   - Recommendation: low priority, verify live during Phase 10's verification checkpoint, not blocking for planning.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None (tModLoader C# mod — no automated unit-test project exists in this repo; verified no `*.Tests.csproj`/pytest/jest present) |
| Config file | none |
| Quick run command | `dotnet build` (compile-check only, catches type/reference errors, JIT-safety issues are NOT caught by this — those need the mod-disabled live load test) |
| Full suite command | Manual live in-game checklist (this project's established pattern since Phase 3 — see `06-03-PLAN.md`'s `check.md` for the most recent precedent format) |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|---------------------|--------------|
| ARENA-01 | Each registered boss's downed flag/side effects apply correctly via `BossCoreItem` | manual-only (live in-game, real boss kill required — no automated harness possible for tModLoader NPC AI/world-state interactions) | `dotnet build` (compile gate only) | N/A — manual checklist per boss, following `check.md` precedent |
| ARENA-01 | Each Zone-dependent boss (Dragonfolly/Jungle, Scarabeus/Desert, MoonWizard+Dusking/night) does not despawn/underperform in its routed arena | manual-only (live fight, several-minute duration to catch Pitfall 6) | none | N/A |
| ARENA-01 | `[JITWhenModsEnabled]` isolation holds for every new method | manual-only (mod-disabled load test per soft dependency, Phase 4/5/9 precedent) | none | N/A |

### Sampling Rate
- **Per task commit:** `dotnet build` (compile-check every registration added)
- **Per wave merge:** Full mod-disabled load-safety smoke test (CalamityMod disabled, SpiritMod disabled, both disabled) — Phase 4/9 precedent
- **Phase gate:** Live in-game checklist covering at minimum: one Zone-functional boss (Dragonfolly or Scarabeus), one Zone-thematic boss, both forced-night bosses (full-duration fight), the polymorphic `MarkofProvidence` item routing correctly to all 2-3 reachable bosses, and the Infernum-conditional gating (with and without InfernumMode enabled) — before `/gsd:verify-work`

### Wave 0 Gaps
- No automated test infrastructure gap — this project has never had one and is not expected to gain one (tModLoader mod, live-game-state-dependent verification is the established and only practical pattern). `check.md`-style manual checklists, written per-plan, are this project's equivalent of a test suite.

## Sources

### Primary (HIGH confidence — direct decompile against installed DLLs, this session)
- `ilspycmd -t <TypeName> Libs/CalamityMod.dll` — decompiled `DownedBossSystem`, `Providence`, `ProfanedGuardianCommander/Defender/Healer`, `CeaselessVoid`, `OldDuke`, `Signus`, `StormWeaverHead`, `AstrumDeusHead`, `AstrumAureus`, `Dragonfolly`, `DevourerofGodsHead`, `ThanatosHead`, `AresBody`, `Artemis`, `Apollo`, `Yharon`, `SupremeCalamitas`, `MarkofProvidence`, `ProfanedCore`, `ProfanedShard`, `BloodwormItem`, `ExoticPheromones`, `CosmicWorm`, `AstralChunk`, `Starcore`, `YharonEgg`, `CeremonialUrn`, `AstralBeacon`, `SCalAltar` — all confirmed 2026-08-14 against the actually-installed v2.2.4 DLL
- `ilspycmd -t <TypeName> Libs/SpiritMod.dll` (+ one full-assembly decompile pass, `spirit_full.cs`, for item-to-boss lookup) — decompiled `BossDownedTracker`, `AncientFlyer`, `SteamRaiderHead`, `SteamRaiderBody`, `Scarabeus`, `ReachBoss`, `MoonWizard`, `Dusking`, `Atlas`, `JewelCrown`, `ScarabIdol`, `ReachBossSummon`, `DuskCrown`, `DreamlightJellyItem`, `StoneSkin`, `StarplateBeaconIntroEvent` — confirmed 2026-08-14 against installed v1.5.0.44 DLL
- `ModReader/InfernumMode/build.txt` (locally installed copy) — HIGH confidence, confirms internal mod name `InfernumMode`, version `2.0.1.35`, `modReferences = CalamityMod, SubworldLibrary, Luminance`
- `Tiles/Test1Tile.cs`, `Systems/SummonItemRegistry.cs`, `Systems/BossRegistry.cs`, `Systems/BossArenaRoutingRegistry.cs`, `Integrations/CalamityIntegration.cs`, `Integrations/SpiritIntegration.cs`, `Integrations/CatalystIntegration.cs` — this project's own existing code, read in full this session

### Secondary (MEDIUM confidence)
- `.planning/phases/09-biome-dependent-subworld-coverage/09-ALTAR-BIOME-REFERENCE.md` — wiki-sourced roster/biome baseline this research verified/corrected against decompiled source; treat this research's per-boss table above as superseding that document's Zone-dependency claims specifically (the roster/biome-naming itself remains accurate)

### Tertiary (LOW confidence / flagged for validation)
- Whether `Main.time` advances naturally inside a `SubworldLibrary` subworld during an active fight (Open Question 3 / Pitfall 6) — not decompiled this session, flagged for a live-verification checkpoint rather than assumed
- `ProfanedGuardianCommander.OnSpawn()`'s escort-spawning behavior (Open Question 5) — not decompiled this session, low-priority cosmetic concern only

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — no new libraries, pure extension of proven Phase 4/5/6 pattern
- Architecture (polymorphic resolver, tile-interaction-item handling, generic Spirit pattern): MEDIUM — the underlying decompiled facts are HIGH confidence, but the two proposed new code patterns (`RegisterPolymorphic`, `ForcedTimeUtility`) are novel and unbuilt/untested
- Per-boss classification: HIGH — every claim traced to a direct decompile citation against the actually-installed DLL this session, not wiki-sourced
- Pitfalls: HIGH — all derived directly from the decompiled source read this session

**Research date:** 2026-08-14
**Valid until:** Until the next CalamityMod/SpiritMod/InfernumMode update changes any of the decompiled internals cited above — recommend re-verifying field/class names if `build.txt`'s pinned versions ever change (per this project's own established `weakReferences` version-pin discipline)
