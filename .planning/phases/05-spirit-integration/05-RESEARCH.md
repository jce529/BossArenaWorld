# Phase 5: Spirit Integration - Research

**Researched:** 2026-08-13
**Domain:** tModLoader cross-mod interop — reading/writing SpiritMod's boss-downed tracking state from BossArenaSubWorld, including a case where the target mod's internal storage class is not compile-time accessible even via `weakReferences`
**Confidence:** HIGH (all API-shape claims verified by directly reading the actually-installed SpiritMod's decompiled source at `ModReader/SpiritMod/`, and by applying standard C# accessibility rules, which are unambiguous language spec, not mod-specific trivia)

## Summary

CONTEXT.md's D-01 already corrected `PROJECT.md`'s stale "plain `MyWorld` static bools" assumption: Infernon's real downed state lives in `SpiritMod.NPCs.BossDownedTracker`, an internal `GlobalNPC` with a static `Dictionary<string, bool> Downed`, keyed `"{npc.type}"` for vanilla NPCs or `"{Mod.Name}/{ModNPC.Name}"` for modded ones. This research goes one level deeper and finds the fact that changes the implementation approach: **`BossDownedTracker` is declared `internal`**. Even though its members (`Downed`, `IsBossDowned<T>()`, `GetBossKey<T>()`) are individually marked `public`, C#'s accessibility rules cap a member's *effective* visibility at its containing type's visibility — so none of it is reachable from BossArenaSubWorld's assembly at compile time, `weakReferences` or not. This is a structurally different obstacle than Calamity's Phase 4 case (where `CalamityMod.DownedBossSystem.downedHiveMind` was a plain public field on a public class), and it means Phase 5 is the **first phase in this project that actually requires runtime reflection**, not just weak references + `[JITWhenModsEnabled]`.

The good news: reading the flag does NOT need reflection. `SpiritMod.MyWorld.DownedInfernon` is a public static get-only property (`=> BossDownedTracker.IsBossDowned<InfernoSkull>()`), and SpiritMod additionally exposes a documented `Mod.Call("downed", "Infernon")` API that returns the identical value — both are safe, compile-time-or-fully-string-based, public read paths. Writing the flag has no public path at all (no setter, no `Mod.Call` "set downed" context exists) — the only way to reproduce what `BossDownedTracker.OnKill()` itself does (`Downed[key] = true`) is targeted reflection into that internal static field. This is not a shortcut around CLAUDE.md's "use the mod's actual setter" rule — it's the closest possible fidelity to that rule when no public setter exists.

A second finding worth flagging to the planner: `BossDownedTrackingIO.HandleBossSyncing(BitsByte)` — the only other public helper touching `Downed` — is **not** a settable "apply progress" helper. It's the client-side network-receive handler for `MyWorld.NetReceive`, and it writes to `GetBossKey<Infernon>()` (Infernon's own key), not `GetBossKey<InfernoSkull>()` (the key `MyWorld.DownedInfernon`/BossChecklist actually read). This is an internal inconsistency in Spirit's own code (the comment calls it "ported from old impl.") — calling it would silently set the *wrong* dictionary entry. Do not use it.

Infernon (D-02's chosen worked-example boss) needs no special arena routing: unlike Hive Mind, its AI has no biome/`Zone*` despawn dependency (full `PreAI`/`AI` read for both `Infernon` and `InfernoSkull`, no such check exists). Its actual summon item, `CursedCloth`, does have a `CanUseItem` position gate (`player.position.Y/16f > Main.maxTilesY - 200`, i.e. near the world's bottom) — but this project's established SUBW-04 mechanism (`BossSummonPlayer.OnEnterWorld` → `NPC.SpawnOnPlayer`) never calls the summon item's own `CanUseItem`/`UseItem`, so this gate is structurally inert for this pipeline (same category as Phase 4's Teratoma `ZoneCorrupt` gate finding in `04-RESEARCH.md`).

**Primary recommendation:** Register both `Infernon` and `InfernoSkull` NPC types under one `BossDefinition("spirit:infernon", ...)` (the record already supports multi-NPC-type bosses); read via `MyWorld.DownedInfernon` (or `Mod.Call("downed","Infernon")`); write via cached reflection into `BossDownedTracker.Downed[ComputedKey] = true` wrapped in try/catch with `Mod.Logger` warnings, matching PITFALLS.md's Pitfall 3/7 guidance that this phase is the first to actually need.

## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01 (API correction):** `PROJECT.md`'s "Spirit: `MyWorld` plain static bool fields" note is outdated for the actually-installed copy. Real bosses (Scarabeus, AncientFlyer, SteamRaiderHead, Atlas, Infernon/InfernoSkull, MoonWizard, ReachBoss1, Dusking) are tracked via `SpiritMod.NPCs.BossDownedTracker`, a `GlobalNPC` with a static `Dictionary<string, bool> Downed` keyed `"{npc.type}"` (vanilla) or `"{ModName}/{ModNPCName}"` (modded), populated in `BossDownedTracker.OnKill(NPC npc)`. `MyWorld`'s plain static bools remain accurate ONLY for non-boss events/minibosses. Persistence/netcode: `BossDownedTrackingIO : ModSystem` handles `SaveWorldData`/`LoadWorldData` (world-scoped) and `OnWorldUnload()` clears the dictionary; `BossDownedTracker.OnKill()` calls `NetMessage.SendData(MessageID.WorldData)` only when `Main.netMode != NetmodeID.SinglePlayer`.
- **D-02 (worked-example boss):** Infernon (tracked via `BossDownedTracker.IsBossDowned<InfernoSkull>()`). Selection rationale: of 8 Spirit-tracked bosses, Infernon is the only one whose own `OnKill()` has a side effect beyond the generic flag write (places a ring of `TileID.HellstoneBrick` around its death position). Known nuance: this tile-ring is anchored to `NPC.position`, a live NPC that won't exist at `BossCoreItem`-use time — user confirmed anchoring the replay on the player's current position instead is fine, cosmetic-only, no special design effort needed.
- **D-03 (player-scoped vs world-scoped):** No player-scoped double-grant risk found in Spirit's tracked-boss `OnKill` paths — `BossDownedTracker.OnKill()` is a pure world-scoped dictionary write + singleplayer-no-op netcode; `Infernon.OnKill()` is a world-scoped tile mutation via `Main.tile`, no player-object writes. Same category as Phase 3's King Slime (no player-scoped reward to worry about). Satisfied by documenting this empirical finding, not building exclusion logic.
- **D-04 (scope):** Register exactly one Spirit boss this phase (Infernon). Registering the remaining 7 is explicitly deferred.
- **D-05 (live verification):** Because Infernon has a real WorldGen tile-mutation side effect, live verification uses a freshly created, dedicated test world (SpiritMod + SubworldLibrary + BossArenaSubWorld enabled), not the backed-up main save. A second checkpoint verifies the mod loads/runs safely with SpiritMod disabled.

### Claude's Discretion
- Exact `weakReferences` version pin syntax in `build.txt` for SpiritMod — **resolved by this research: `SpiritMod@1.5.0.44`**, see Standard Stack.
- Exact shape/naming of `Integrations/SpiritIntegration.cs`, following `Integrations/CalamityIntegration.cs`'s convention.
- How exactly to anchor Infernon's tile-ring replay position in the main world — player position at `BossCoreItem` use time (confirmed low-stakes, exact implementation deferred to planning; this research provides a ready-to-use code example below).
- Whether to replay `NetMessage.SendData(MessageID.WorldData)` in the main-world apply path — default to yes (matches Phase 4's `SyncWorld()` replay choice), singleplayer no-op.
- **CRITICAL — carried forward from Phase 4:** any delegate passed into a `[JITWhenModsEnabled("SpiritMod")]`-guarded registration call must be a named, separately-tagged method, never an inline lambda (Phase 4's JITException lesson, commit `0e19600`). Applies identically here.
- **New discretion surfaced by this research (not anticipated in CONTEXT.md):** whether/how to signal reflection failure out of `ApplyDowned`/`IsDowned` — see Open Questions below. `BossDefinition`'s current `Action`/`Func<bool>` shape has no built-in failure channel, and Phase 5 is the first registration where the underlying call can genuinely throw (missing field, mod update) rather than just "type not present."

### Deferred Ideas (OUT OF SCOPE)
- Registering the remaining 7 Spirit bosses (Scarabeus, AncientFlyer, SteamRaiderHead/Starplate, Atlas, MoonWizard, ReachBoss1/Vinewrath, Dusking) — same "marginal registration cost is uniform once the pattern is proven" reasoning as Phase 4's deferred Calamity bosses.
- Wrath of the Gods base mod — only the Korean localization patch is currently subscribed; not relevant to Phase 5-8 registration work.

</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MOD-02 | Spirit bosses registered via Spirit's actual downed-progress API | This research identifies the exact access pattern: read via public `MyWorld.DownedInfernon` (or `Mod.Call("downed","Infernon")`), write via reflection into the internal `BossDownedTracker.Downed` dictionary (no public setter exists — verified, not assumed). Provides ready-to-adapt `RegisterInfernon()`/`ApplyInfernonDowned()`/`IsInfernonDowned()` code matching the project's `Integrations/CalamityIntegration.cs` template exactly. Confirms no special arena routing is needed (unlike Hive Mind) and that `CursedCloth`'s position gate is structurally inert for this project's summon mechanism. |

</phase_requirements>

## Standard Stack

### Core
| Component | Version | Purpose | Why Standard |
|-----------|---------|---------|---------------|
| SpiritMod | **1.5.0.44** (per `LastLaunchedMods.txt`, tModLoader's own record of the last version actually launched/played on this machine — MEDIUM-HIGH confidence; the raw `.tmod` is not currently present in `Mods/` to read its header directly the way Phase 4 did for CalamityMod, see Environment Availability) | The content mod whose Infernon boss this phase registers | Project's own stated dependency list; already decompiled locally at `ModReader/SpiritMod/` |
| `System.Reflection` (BCL, no package) | Bundled with .NET 8 SDK | Reach `BossDownedTracker`'s internal static `Downed` field | Required — no public setter/API exists for writing Spirit's boss-downed state (verified below); this is the first phase in the project that genuinely needs it, matching PITFALLS.md's Pitfall 2/3/6/7 guidance that was previously theoretical |
| `weakReferences` + `[JITWhenModsEnabled]` (built-in tModLoader) | N/A | Compile-time access to SpiritMod's *public* types (`Infernon`, `InfernoSkull`, `CursedCloth`, `MyWorld`) | Same locked pattern as Phase 4 — required for any direct type reference, not needed for the reflection-only piece itself |

**`build.txt` addition:**
```
weakReferences = CalamityMod@2.2.4 SpiritMod@1.5.0.44
```
(tModLoader's `build.txt` allows space-separated multiple entries on one `weakReferences` line, matching existing wiki syntax; alternatively keep as two separate lines if the existing file's formatting convention is preferred — verify against the current `build.txt`'s exact style before writing.)

**`.csproj` addition** (mirrors the existing `CalamityMod` `<Reference>` block exactly):
```xml
<Reference Include="SpiritMod" Condition="Exists('Libs\SpiritMod.dll')">
    <HintPath>Libs\SpiritMod.dll</HintPath>
    <Private>false</Private>
</Reference>
```

**Version verification / DLL sourcing — important shortcut found:**
Unlike Calamity (extracted fresh from `Mods/2026.6CalamityMod.tmod` via `scripts/extract_tmod.py`), SpiritMod's `.tmod` is **not currently present** in the local `Mods/` folder at all (only `CalamityModMusic`, `CalamityMod`, and `BossArenaSubWorld` `.tmod` files exist there today). However, `ModReader/SpiritMod/SpiritMod.dll` (and `.pdb`) already exist locally from a prior decompilation pass — copy that file directly to `Libs/SpiritMod.dll` instead of re-running `extract_tmod.py` against a `.tmod` that doesn't currently exist. If a byte-for-byte-current DLL is wanted instead, the user must first re-subscribe to SpiritMod via Steam Workshop/Mod Browser (see Environment Availability).

### Alternatives Considered
| Instead of | Could use | Tradeoff |
|------------|-----------|----------|
| Direct reflection into `BossDownedTracker.Downed` | Reflectively invoking the internal `GetBossKey<T>()`/`IsBossDowned<T>()` generic methods via `MakeGenericMethod` | More "faithful" to Spirit's exact algorithm if it ever changes, but more reflection surface area (extra `MethodInfo`/generic binding) for no real gain — the key format (`Mod.Name + "/" + ModNPC.Name`) is a stable tModLoader-wide convention, not Spirit-specific logic likely to change. Recommend computing the key via public APIs (`ModContent.GetInstance<InfernoSkull>()`) instead, and reserve full reflection for the one thing that truly requires it (the `Downed` field itself). |
| `MyWorld.DownedInfernon` (public property read) | `Mod.Call("downed", "Infernon")` | Both return the identical value (`Call`'s `BossDowned()` literally forwards to `MyWorld.DownedInfernon`). `Mod.Call` is SpiritMod's documented external-mod contract (see `SpiritMod.Call.cs`) and needs no compile-time type reference at all, making it marginally more resilient to a future internal refactor of `MyWorld`/`BossDownedTracker`. `MyWorld.DownedInfernon` is simpler code and already needs a compile-time SpiritMod type reference anyway (for `Infernon`/`InfernoSkull` NPC-type registration in the same method), so the resilience benefit is smaller here than it would be in isolation. Either is acceptable; this research recommends `Mod.Call` for the marginal robustness. |

## Architecture Patterns

### Recommended file
```
Integrations/
└── SpiritIntegration.cs   # mirrors Integrations/CalamityIntegration.cs exactly
```

### Pattern 1: Public-property/Mod.Call read, reflection-only write
**What:** Split the read path (public, safe, no reflection) from the write path (reflection, the only option) rather than reflecting for both.
**When to use:** Any future soft-dependency mod (Redemption, CatalystMod, NoxusBoss, Homeward — Phase 6/7, "entirely unresearched APIs" per STATE.md) where the internal storage class turns out to be non-public. Check accessibility explicitly before assuming reflection is needed for reads too — it often isn't.
**Example (adapt boss/type names per CONTEXT.md D-02, verified against the actual installed source above):**
```csharp
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Integrations
{
    public class SpiritIntegration : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!ModLoader.HasMod("SpiritMod")) return;
            RegisterInfernon();
        }

        [JITWhenModsEnabled("SpiritMod")]
        private void RegisterInfernon()
        {
            int infernonType = ModContent.NPCType<SpiritMod.NPCs.Boss.Infernon.Infernon>();
            int infernoSkullType = ModContent.NPCType<SpiritMod.NPCs.Boss.Infernon.InfernoSkull>();

            // Whichever entity actually lands the "fight over" kill (Infernon itself in
            // Normal Mode's real PreKill()==true death path, or InfernoSkull in Expert Mode
            // where Infernon fades out via alpha/active=false WITHOUT calling its own OnKill --
            // see Common Pitfalls below) still triggers a carrier-item drop.
            SummonItemRegistry.Register(
                ModContent.ItemType<SpiritMod.Items.Consumable.CursedCloth>(), infernonType);

            // No BossArenaRoutingRegistry.Register<T>() call needed -- unlike Hive Mind,
            // Infernon's AI has no Zone*/biome despawn dependency (verified: full PreAI/AI
            // read for both Infernon and InfernoSkull). Falls back to the default
            // BossArenaSubworld automatically.

            BossRegistry.Register("spirit:infernon", new BossDefinition(
                NpcTypes: new[] { infernonType, infernoSkullType },
                ApplyDowned: ApplyInfernonDowned,
                IsDowned: IsInfernonDowned));
        }

        // Read path: no reflection needed. MyWorld is a public class, DownedInfernon is a
        // public static get-only property. (Mod.Call("downed","Infernon") is an equally valid
        // alternative -- see Alternatives Considered.)
        [JITWhenModsEnabled("SpiritMod")]
        private static bool IsInfernonDowned() => SpiritMod.MyWorld.DownedInfernon;

        // Write path: BossDownedTracker (SpiritMod.NPCs.BossDownedTracker) is declared
        // `internal`. Even though Downed/IsBossDowned<T>()/GetBossKey<T>() are individually
        // `public static`, C# caps effective member visibility at the containing type's
        // visibility -- direct `SpiritMod.NPCs.BossDownedTracker.Downed[...] = true;` is a
        // CS0122 compile error, weakReferences or not. No public setter and no Mod.Call
        // "set downed" context exist either (confirmed: SpiritMod.Call.cs's Downed context is
        // read-only). Reflection into the internal Dictionary<string,bool> field is the ONLY
        // path -- and it replicates EXACTLY what BossDownedTracker.OnKill(npc) itself does
        // (`Downed[GetBossKey(npc)] = true`), satisfying CLAUDE.md's "call the mod's actual
        // setter, don't bypass it" intent since no other path is exposed.
        [JITWhenModsEnabled("SpiritMod")]
        private static void ApplyInfernonDowned()
        {
            try
            {
                var spiritMod = ModLoader.GetMod("SpiritMod");
                var trackerType = spiritMod.Code.GetType("SpiritMod.NPCs.BossDownedTracker");
                var downedField = trackerType.GetField("Downed", BindingFlags.NonPublic | BindingFlags.Static);
                var downed = (Dictionary<string, bool>)downedField.GetValue(null);

                // Key format replicated from BossDownedTracker.GetBossKey(NPC) via PUBLIC APIs
                // only (ModContent.GetInstance<T>()) -- avoids reflecting into GetBossKey itself.
                var infernoSkull = ModContent.GetInstance<SpiritMod.NPCs.Boss.Infernon.InfernoSkull>();
                string key = infernoSkull.Mod.Name + "/" + infernoSkull.Name; // "SpiritMod/InfernoSkull"

                downed[key] = true;

                // Matches BossDownedTracker.OnKill()'s own netcode replay exactly; singleplayer no-op.
                if (Main.netMode != NetmodeID.SinglePlayer)
                    NetMessage.SendData(MessageID.WorldData);
            }
            catch (System.Exception e)
            {
                // Pitfall 3/7 discipline: fail loud in the log, fail soft to the player --
                // do NOT let a broken reflection lookup crash the whole item-use.
                ModContent.GetInstance<BossArenaSubWorld.BossArenaSubWorld>().Logger.Warn(
                    "SpiritIntegration: failed to set Infernon downed flag via reflection: " + e);
            }

            // WorldGen side effect replay (D-02): Infernon.OnKill()/InfernoSkull.OnKill() both
            // place a ring of TileID.HellstoneBrick + drain liquid to Lava around the boss's
            // death position. No live NPC exists at BossCoreItem-use time -- anchor on the
            // player's current position instead (confirmed cosmetic/non-blocking by user).
            ReplayInfernonTileRing(Main.LocalPlayer.Center);
        }

        [JITWhenModsEnabled("SpiritMod")]
        private static void ReplayInfernonTileRing(Vector2 center)
        {
            int centerX = (int)center.X / 16;
            int centerY = (int)center.Y / 16;
            int halfLength = 160 / 2 / 16 + 1; // Infernon.NPC.width == 160 (source-confirmed)
            for (int x = centerX - halfLength; x <= centerX + halfLength; x++)
            {
                for (int y = centerY - halfLength; y <= centerY + halfLength; y++)
                {
                    Tile tile = Main.tile[x, y];
                    bool onRingEdge = x == centerX - halfLength || x == centerX + halfLength
                                    || y == centerY - halfLength || y == centerY + halfLength;
                    if (onRingEdge && !tile.HasTile)
                    {
                        tile.TileType = TileID.HellstoneBrick;
                        tile.HasTile = true;
                    }
                    tile.LiquidType = LiquidID.Lava;
                    tile.LiquidAmount = 0;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendTileSquare(-1, x, y, 1);
                    else
                        WorldGen.SquareTileFrame(x, y, true);
                }
            }
        }
    }
}
```
*(`Main.LocalPlayer.Center`, `[JITWhenModsEnabled]` gating, and the reflection block above are all illustrative and should be re-verified against the actual installed `SpiritMod.dll`/`tModLoader.dll` at implementation time, matching this project's own established "trust the installed binary over research notes" discipline — see `03-RESEARCH.md`'s precedent of catching two illustrative-code bugs during Phase 3 implementation.)*

### Anti-Patterns to Avoid
- **Calling `BossDownedTrackingIO.HandleBossSyncing(BitsByte)` as a "set downed" helper:** it is the network-*receive* deserializer for `MyWorld.NetReceive`, and it writes `Downed[GetBossKey<Infernon>()]` (Infernon's own key) — a *different* dictionary entry than `MyWorld.DownedInfernon` reads (`GetBossKey<InfernoSkull>()`). Calling it would silently set the wrong flag while `DownedInfernon`/BossChecklist continue reading false.
- **Hardcoding the computed key string (`"SpiritMod/InfernoSkull"`) instead of computing it from `ModNPC.Mod.Name`/`.Name`:** works today but is exactly the kind of brittle string literal PITFALLS.md's Pitfall 3 warns about; compute it from the public `ModContent.GetInstance<InfernoSkull>()` instance instead.
- **Un-cached reflection per `Apply()` call:** low-cost here since carrier-item use is a rare event (not per-kill), but caching `FieldInfo` once at `PostSetupContent` is nearly free and improves failure diagnostics per Pitfall 3 — worth doing even though this phase's scale doesn't strictly require it.

## Don't Hand-Roll

| Problem | Don't build | Use instead | Why |
|---------|-------------|--------------|-----|
| Reading whether Infernon is downed | A custom reflection lookup into `BossDownedTracker` | `SpiritMod.MyWorld.DownedInfernon` (public property) or `Mod.Call("downed","Infernon")` | Both are already public, safe, zero-reflection paths that return the exact same value the mod's own checklist integration (`Infernon.RegisterToChecklist`) uses |
| Computing the Spirit boss dictionary key for a modded NPC | Reflecting into `BossDownedTracker.GetBossKey<T>()` | `ModContent.GetInstance<T>().Mod.Name + "/" + ModContent.GetInstance<T>().Name` | Standard tModLoader-wide naming convention (not Spirit-specific logic), fully public API, avoids one extra reflection call for no real robustness gain |
| Netcode sync after applying the flag | A custom broadcast/packet | `NetMessage.SendData(MessageID.WorldData)` | Exactly what `BossDownedTracker.OnKill()` itself calls; safe no-op in singleplayer (same category as Phase 4's `CalamityNetcode.SyncWorld()` finding) |

**Key insight:** This mod's own carrier-item architecture already solves "state doesn't survive the subworld round-trip" (Pitfall 1) — Phase 5's only real new problem is *reaching* an internal type, which is a one-time reflection helper, not a parallel tracking system. Resist the temptation to build a whole custom "shadow" downed-tracking layer just because the real one is `internal`.

## Common Pitfalls

### Pitfall A: `BossDownedTracker` being `internal` blocks direct compile-time access despite `weakReferences`
**What goes wrong:** Code like `SpiritMod.NPCs.BossDownedTracker.Downed[key] = true;` fails to compile (`CS0122: 'BossDownedTracker' is inaccessible due to its protection level`), even with `weakReferences = SpiritMod@...` correctly declared and `[JITWhenModsEnabled("SpiritMod")]` correctly applied.
**Why it happens:** The class itself is `internal class BossDownedTracker : GlobalNPC` (namespace `SpiritMod.NPCs`). C#'s accessibility rules cap every member's effective visibility at its containing type's declared visibility — `public static` members of an `internal` class are still only assembly-internal. This is unrelated to and unaffected by `weakReferences`/`InternalsVisibleTo` (SpiritMod does not declare `InternalsVisibleTo` for this project's assembly).
**How to avoid:** Confirmed via reflection instead (see Architecture Pattern 1 above): `Assembly.GetType("SpiritMod.NPCs.BossDownedTracker")` + `BindingFlags.NonPublic | BindingFlags.Static` on the `Downed` field. Reflection bypasses C#'s compile-time accessibility checks entirely (it operates at the CLR/metadata level), so this works even though direct code would not compile.
**Warning signs:** A `CS0122` build error naming `BossDownedTracker` (or any other internal type) the moment a per-mod integration file is written without first checking the target class's declared accessibility against the actual decompiled source.

### Pitfall B: `MyWorld.DownedInfernon`'s tracked key (`InfernoSkull`) can diverge from which NPC the player actually saw die
**What goes wrong:** In Normal Mode, `Infernon.PreKill()` returns `true`, so Infernon can die "for real" via the standard vanilla kill flow, firing `Infernon`'s own `OnKill()` — which tags `BossDownedTracker.Downed` with **Infernon's own key**, not `InfernoSkull`'s. But `MyWorld.DownedInfernon`/BossChecklist only read the `InfernoSkull`-keyed entry. In Expert Mode, `Infernon.HitEffect()`'s `if (Main.expertMode)` branch spawns another `InfernoSkull` and `PreKill()` returns `false` (undying) — Infernon instead fades out via `NPC.alpha`/`active = false` in a later `PreAI` branch, **never calling its own `OnKill()` at all**; the actual "downed" trigger becomes whichever `InfernoSkull` add is later killed for real.
**Why it happens:** This looks like an unresolved internal inconsistency in Spirit's own source between the two death paths, not a project-side bug to fix. `PreAI` also spawns an `InfernoSkull` add automatically once Infernon's life ≤ 7000 in *both* modes, so by the time Infernon's own HP reaches 0, a live `InfernoSkull` is very likely (but not guaranteed) present nearby.
**How to avoid:** Register **both** `Infernon` and `InfernoSkull` NPC types under the same `BossDefinition` (the record already supports multi-type bosses per its own code comment). This guarantees a `BossCoreItem` drop regardless of which entity lands the fight-ending kill. `ApplyDowned()`/`IsDowned()` should still target the `InfernoSkull`-keyed flag specifically (matching what the game itself reads), independent of which NPC type triggered the drop.
**Warning signs:** Player defeats Infernon, item drops, but using it produces "Applied" while `MyWorld.DownedInfernon` was already coincidentally true from a mid-fight `InfernoSkull` kill (harmless — idempotent), or the reverse: item never drops because only `InfernoSkull` was registered and the player's specific playthrough ended via Infernon's own death without an `InfernoSkull` present.

### Pitfall C: `HandleBossSyncing` looks like a setter but is a network-receive deserializer, and writes the wrong key for Infernon
**What goes wrong:** Calling `BossDownedTrackingIO.HandleBossSyncing(someBitsByte)` to "apply" Infernon's downed state would set `Downed[GetBossKey<Infernon>()]`, not `Downed[GetBossKey<InfernoSkull>()]` — the wrong entry relative to what `MyWorld.DownedInfernon` reads.
**Why it happens:** The method's own comment says it was "ported from old impl. in `MyWorld.NetRecieve`" — a leftover from before the tracked type was changed to `InfernoSkull`, seemingly never updated to match.
**How to avoid:** Never call `HandleBossSyncing`. Write directly to the `Downed` dictionary via the reflection path in Architecture Pattern 1, using the `InfernoSkull`-computed key.
**Warning signs:** `MyWorld.DownedInfernon` still false / BossChecklist still shows undefeated after using the carrier item, even though no exception was thrown.

### Pitfall D: Assuming `CursedCloth`'s depth gate matters for this project's summon mechanism
**What goes wrong:** Spending planning effort designing a special deep/Underworld-positioned arena subworld for Infernon (mirroring Phase 4's `BossArenaCorruptionSubworld` pattern for Hive Mind) because `CursedCloth.CanUseItem` requires `player.position.Y/16f > Main.maxTilesY - 200`.
**Why it happens:** Superficially resembles Hive Mind's `ZoneCorrupt` biome-gate problem from Phase 4.
**How to avoid:** This project's `BossSummonPlayer.OnEnterWorld` (SUBW-04) calls `NPC.SpawnOnPlayer(...)` directly and never invokes the summon item's own `CanUseItem`/`UseItem` at all (confirmed in `Systems/BossSummonPlayer.cs`) — so the position gate is never evaluated by this pipeline. This is the identical situation Phase 4's `04-RESEARCH.md` Open Question 1 already documented for Calamity's `Teratoma.CanUseItem`'s `ZoneCorrupt` gate. Unlike Hive Mind, Infernon's actual *AI* (not just its summon-item gate) has no biome/position despawn dependency either (verified: full `PreAI`/`AI` read for both `Infernon` and `InfernoSkull`) — so no special arena subworld is needed at all; the default `BossArenaSubworld` (plain stone platform) is sufficient.
**Warning signs:** N/A if this guidance is followed at planning time; if skipped, the symptom would be Infernon simply not spawning (silent no-op) if some future code path accidentally does call the item's real `CanUseItem`.

## Code Examples

Verified against the actually-installed decompiled source (`ModReader/SpiritMod/`), not from memory or documentation:

### Infernon's own WorldGen side effect (source, for reference — do not call this method itself, replay its logic per Architecture Pattern 1)
```csharp
// Source: ModReader/SpiritMod/NPCs/Boss/Infernon/Infernon.cs, OnKill() (lines 388-414)
// (InfernoSkull.cs has a near-identical duplicate at lines 66-93, centered on NPC.Center
// instead of NPC.position + width/2 — functionally the same ring/lava effect.)
public override void OnKill()
{
    if (Main.netMode != NetmodeID.MultiplayerClient)
    {
        int centerX = (int)(NPC.position.X + (NPC.width / 2)) / 16;
        int centerY = (int)(NPC.position.Y + (NPC.height / 2)) / 16;
        int halfLength = NPC.width / 2 / 16 + 1;
        for (int x = centerX - halfLength; x <= centerX + halfLength; x++)
            for (int y = centerY - halfLength; y <= centerY + halfLength; y++)
            {
                Tile tile = Main.tile[x, y];
                if ((x == centerX - halfLength || x == centerX + halfLength || y == centerY - halfLength || y == centerY + halfLength) && !Main.tile[x, y].HasTile)
                {
                    tile.TileType = TileID.HellstoneBrick;
                    tile.HasTile = true;
                }
                tile.LiquidType = LiquidID.Lava;
                tile.LiquidAmount = 0;
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, x, y, 1);
                else
                    WorldGen.SquareTileFrame(x, y, true);
            }
    }
}
```

### The actual tracking mechanism (source, for reference)
```csharp
// Source: ModReader/SpiritMod/NPCs/BossDownedTracker.cs (lines 25-69)
internal class BossDownedTracker : GlobalNPC
{
    internal static Dictionary<string, bool> Downed = new();
    public static bool IsBossDowned<T>() where T: ModNPC => Downed.ContainsKey(GetBossKey<T>()) && Downed[GetBossKey<T>()];
    public static string GetBossKey(NPC npc) =>
        npc.type < NPCID.Count ? npc.type.ToString() : npc.ModNPC.Mod.Name + "/" + npc.ModNPC.Name;
    public override void OnKill(NPC npc)
    {
        if (npc.boss)
        {
            Downed[GetBossKey(npc)] = true;
            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendData(MessageID.WorldData);
        }
    }
}
```

## State of the Art

| Old approach (superseded) | Current approach | When changed | Impact |
|---------------------------|-------------------|---------------|--------|
| `PROJECT.md`/`REQUIREMENTS.md` "MyWorld static-field pattern" for Spirit | `BossDownedTracker`'s internal `Dictionary<string,bool>`, exposed only via `MyWorld`'s read-only computed properties | Reconciled by CONTEXT.md's D-01, confirmed and extended by this research | Downstream planning must reflect for writes; reads use the public property/`Mod.Call`, not the "plain field" model `MOD-02`'s original wording implied |

## Open Questions

1. **Should `BossDefinition.ApplyDowned`/`IsDowned` gain a failure-signaling shape now that Phase 5 introduces the first genuinely fallible registration?**
   - What we know: `Action ApplyDowned` / `Func<bool> IsDowned` have no built-in way to report "reflection lookup failed" back to `BossRegistry.Apply()`/`BossCoreItem.UseItem()`. Calamity's Phase 4 delegates could never fail this way (direct compiled calls either work or fail to JIT entirely, which is a load-time crash, not a runtime `Apply()`-time failure).
   - What's unclear: Whether to (a) swallow reflection failures inside `ApplyInfernonDowned` itself (log + no-op, `IsDowned` would then just report "not yet downed" forever, `BossCoreItem` stays usable/retryable), or (b) extend `BossDefinition`'s contract project-wide (e.g. `Func<bool> ApplyDowned` returning success) so `BossCoreItem.UseItem`'s existing `ApplyResult` switch can surface a distinct "apply failed" message instead of silently behaving like `Applied`.
   - Recommendation: For this phase specifically, swallow-and-log (option a) is sufficient and matches the "prove the pattern with one boss" scope — extending the shared `BossDefinition` contract is a cross-cutting change better deferred to whichever future phase (6/7) first hits a case where this actually matters in practice, or done as a deliberate refactor once 2-3 reflection-based mods exist to generalize from. Flag for the planner to decide explicitly rather than defaulting silently either way.

2. **Does Infernon's AI behave identically without SpiritMod's other systems fully active (e.g. inside a subworld with `NormalUpdates` effectively disabled)?**
   - What we know: Full `PreAI`/`AI` source read for both `Infernon` and `InfernoSkull` shows no explicit dependency on day/night, other NPCs, or world-update-driven systems beyond player targeting.
   - What's unclear: Not verified live yet — this is a source-reading conclusion, not an in-game observation, mirroring Phase 4's Open Question 1 for Hive Mind (which turned out to have a real, non-obvious AI-level dependency despite a similarly clean-looking `OnKill`).
   - Recommendation: Not a blocker for planning — surface as something to *observe* (not specifically test) during the D-05 live verification checkpoints; if Infernon's AI errors out or behaves unexpectedly, it will surface immediately as a failed/stuck fight attempt, same detection path Phase 4 used.

3. **Exact current SpiritMod Workshop version vs. the `1.5.0.44` recorded in `LastLaunchedMods.txt`.**
   - What we know: `LastLaunchedMods.txt` records `SpiritMod 1.5.0.44` as the last version actually launched on this machine, and `ModReader/SpiritMod/` was decompiled from that same install.
   - What's unclear: Whether this is still the current Steam Workshop version (SpiritMod is not currently present in `Mods/` to re-verify its `.tmod` header directly, unlike Calamity's exact-match check in Phase 4).
   - Recommendation: Pin `weakReferences = SpiritMod@1.5.0.44` for now (matches the decompiled source being used for all other findings in this document); if the user re-subscribes to a newer version before live testing, re-verify the pin against the new `.tmod`'s actual header before the D-05 checkpoint, same discipline as Phase 4's Calamity version check.

## Environment Availability

| Dependency | Required by | Available | Version | Fallback |
|------------|--------------|-----------|---------|----------|
| SpiritMod `.tmod` (in `Mods/` folder, enabled) | D-05 live verification checkpoints (both "enabled" and "disabled" tests) | ✗ — not currently present in `Mods/` at all (only `CalamityModMusic`, `CalamityMod`, `BossArenaSubWorld` `.tmod` files exist locally today; `enabled.json` lists neither SpiritMod nor BossChecklist as enabled) | Last known: 1.5.0.44 (`LastLaunchedMods.txt`) | User must re-subscribe via Steam Workshop/Mod Browser before any live in-game testing can occur — this blocks D-05 specifically, not compilation |
| `ModReader/SpiritMod/SpiritMod.dll` (already-decompiled assembly) | Compile-time `Libs/SpiritMod.dll` reference | ✓ | Matches `1.5.0.44` decompile | None needed — copy directly, skip `extract_tmod.py` for this mod |
| .NET 8 SDK / `dotnet msbuild` | Build | ✓ (already proven working in Phases 1-4) | — | — |
| SubworldLibrary | Runtime dependency (unchanged from prior phases) | ✓ (`modReferences`, already working) | 2.2.3.3 (`LastLaunchedMods.txt`) | — |

**Missing dependencies with no fallback:**
- SpiritMod `.tmod` not currently installed/enabled — blocks live verification (D-05) only, not implementation/compilation (the already-decompiled `SpiritMod.dll` covers that). Flag to the user before scheduling the live-verification task.

**Missing dependencies with fallback:**
- None beyond the above (the DLL-copy shortcut removes what would otherwise be a real compile-time blocker).

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None — tModLoader mod, no automated in-game test harness (matches Phase 1-4's established precedent, see `04-RESEARCH.md`) |
| Config file | none |
| Quick run command | `dotnet build BossArenaSubWorld.csproj` |
| Full suite command | N/A — "full verification" is the two live in-game checkpoints below (D-05), each requiring SpiritMod to actually be installed first (see Environment Availability) |

### Phase Requirements → Test Map
| Req ID | Behavior | Test type | Automated command | File exists? |
|--------|----------|-----------|---------------------|---------------|
| MOD-02 | Infernon registered via `BossDefinition`, compiles against `Libs/SpiritMod.dll` | build (compile-time type check) | `dotnet build BossArenaSubWorld.csproj` | ❌ Wave 0 (new file: `Integrations/SpiritIntegration.cs`) |
| MOD-02 / SC1 | Using the carrier item sets `MyWorld.DownedInfernon` (and thus BossChecklist, if installed) to true in the main world | manual-only, **dedicated test world per D-05** | live in-game: kill Infernon in the subworld, return, use `BossCoreItem`, check `MyWorld.DownedInfernon` (via a debug print or BossChecklist UI) | ❌ Wave 0 |
| MOD-02 / SC1 | Reflection failure path degrades gracefully (does not crash the item-use) | manual-only, code-review-assisted | temporarily break the field name in a debug build, confirm `Mod.Logger.Warn` fires and `UseItem` doesn't throw, then revert | ❌ Wave 0 |
| SC3 | Mod loads safely with SpiritMod disabled | manual-only, **real checkpoint per D-05** | disable SpiritMod in Mod Configuration, launch, confirm no crash/JIT exception in client log | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet build BossArenaSubWorld.csproj` (0 warnings/errors expected; requires `Libs/SpiritMod.dll` present locally — gitignored, copy from `ModReader/SpiritMod/SpiritMod.dll` per Environment Availability)
- **Per wave merge:** same build command
- **Phase gate:** both live checkpoints (Infernon-downed-applies test, SpiritMod-disabled test) green before `/gsd:verify-work`, structured as two separate `checkpoint:human-verify` tasks mirroring Phase 4's `04-02-PLAN.md` shape

### Wave 0 Gaps
- [ ] `Integrations/SpiritIntegration.cs` — new file, no automated test beyond the build gate
- [ ] `Libs/SpiritMod.dll` — copy from `ModReader/SpiritMod/SpiritMod.dll` (no `extract_tmod.py` run needed, unlike Calamity — see Environment Availability)
- [ ] `build.txt` — add `weakReferences = SpiritMod@1.5.0.44`
- [ ] `.csproj` — add the `<Reference Include="SpiritMod">` block
- [ ] SpiritMod re-subscribed/enabled in the live `Mods/` folder before the D-05 live checkpoints (blocks live verification only, not compilation)

*No automated test-framework gap beyond the build gate and the two live checkpoints — matches this project's established, previously-approved manual-verification model.*

## Sources

### Primary (HIGH confidence — directly read, locally available decompiled source)
- `ModReader/SpiritMod/NPCs/BossDownedTracker.cs` — the tracking mechanism, `internal` class declaration, `Downed` dictionary, `GetBossKey`/`IsBossDowned`, `BossDownedTrackingIO` (public `ModSystem`), `HandleBossSyncing` (confirmed to write the wrong key for Infernon)
- `ModReader/SpiritMod/MyWorld.cs` — confirms `DownedInfernon => BossDownedTracker.IsBossDowned<InfernoSkull>()` (public static property), and that plain static bools remain accurate only for non-boss events
- `ModReader/SpiritMod/NPCs/Boss/Infernon/Infernon.cs` — full source: `SetDefaults`, `PreAI` (`InfernoSkull` add-spawning logic, both-mode conditions), `PreKill` (mode-dependent death-allow logic), `HitEffect` (expert-mode fade-instead-of-die branch), `OnKill` (WorldGen tile-ring effect), `RegisterToChecklist` (confirms `MyWorld.DownedInfernon` is the checklist's own read path)
- `ModReader/SpiritMod/NPCs/Boss/Infernon/InfernoSkull.cs` — full source: confirms near-duplicate `OnKill()` tile-ring effect, no biome/position AI dependency
- `ModReader/SpiritMod/Items/Consumable/CursedCloth.cs` — the summon item; confirms `CanUseItem`'s depth gate and `UseItem`'s `NPC.SpawnOnPlayer` call
- `ModReader/SpiritMod/SpiritMod.Call.cs` — confirms the public `Mod.Call("downed", bossName)` API surface, its exact `BossDowned()` switch (includes `"Infernon" => MyWorld.DownedInfernon`), and confirms no "set downed" `Call` context exists
- `LastLaunchedMods.txt` (project-local, tModLoader-authored) — records `SpiritMod 1.5.0.44` as the last-launched version on this machine
- `Mods/enabled.json` (project-local) — confirms SpiritMod is not currently enabled/present
- This project's own `Systems/BossRegistry.cs`, `Systems/BossSummonPlayer.cs`, `Systems/BossArenaRoutingRegistry.cs`, `Integrations/CalamityIntegration.cs`, `ItemDropRules/BossCoreDropRule.cs`, `Items/BossCoreItem.cs`, `Subworlds/BossArenaSubworld.cs` — read in full to confirm exact integration points and confirm no changes needed to boss-agnostic infrastructure

### Secondary (MEDIUM-HIGH confidence)
- `.planning/research/PITFALLS.md` — Pitfalls 2/3/6/7 (JIT hazards, reflection brittleness, `Assembly.GetTypes()` gotcha, `Type.GetType` gotcha) — directly applicable now that this phase actually needs reflection, previously theoretical guidance
- `.planning/phases/04-calamity-integration-cross-mod-side-effect-reproduction/04-RESEARCH.md` and `04-CONTEXT.md` — precedent for the `weakReferences`/`[JITWhenModsEnabled]` pattern, the lambda-hoisting JIT lesson, and the `CanUseItem`-gate-is-bypassed finding this research extends to Infernon/`CursedCloth`

### Tertiary (LOW confidence)
- None — every claim in this document traces to directly-read local source or the project's own prior verified research.

## Metadata

**Confidence breakdown:**
- Standard stack (SpiritMod API shape, internal-class accessibility finding): HIGH — verified by direct source read + unambiguous C# language accessibility rules, not inference
- Architecture (integration file shape, reflection pattern): HIGH — directly extends the already-proven `CalamityIntegration.cs` template, cross-checked against `BossRegistry`/`BossCoreDropRule`/`BossSummonPlayer` source
- Pitfalls: HIGH for A/B/C (source-verified), MEDIUM for the live-behavior claims in Pitfall D and Open Question 2 (source-reading conclusion, not yet live-observed — explicitly flagged as such)
- SpiritMod version pin (`1.5.0.44`): MEDIUM-HIGH — from `LastLaunchedMods.txt`, not re-verified against a currently-installed `.tmod` header (unavailable locally, see Environment Availability)

**Research date:** 2026-08-13
**Valid until:** ~30 days, or immediately if SpiritMod is updated on Steam Workshop before implementation (re-verify the version pin and re-diff `BossDownedTracker.cs`/`Infernon.cs` against the newly-installed copy before trusting this document's code examples verbatim, per this project's own "trust the installed binary, not research notes" precedent from Phase 3)

---
*Phase: 05-spirit-integration*
*Researched: 2026-08-13*
