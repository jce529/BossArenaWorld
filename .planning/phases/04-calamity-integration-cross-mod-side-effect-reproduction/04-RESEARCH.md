# Phase 4: Calamity Integration & Cross-Mod Side-Effect Reproduction - Research

**Researched:** 2026-08-13
**Domain:** tModLoader cross-mod integration — weak-reference access to CalamityMod's boss-progress API, decompiled and verified against the actually-installed `2026.6CalamityMod.tmod` binary (mod version 2.2.4, built against tModLoader 2026.6.3.4)
**Confidence:** HIGH — every class/member name, namespace, method signature, and side-effect code path cited below was read directly out of the decompiled `CalamityMod.dll` extracted from the locally installed `.tmod` (not from memory, not from stale project docs). tModLoader-side APIs (`JITWhenModsEnabledAttribute`, `ModLoader.HasMod`/`TryGetMod`) were independently verified against the locally installed `tModLoader.dll` (1.4.4.9+2026.06.3.6).

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**D-01 — Cross-mod access strategy:** Use `weakReferences` (in `build.txt`) + `[JITWhenModsEnabled]` for all Calamity-type-touching code (`CalamityMod.DownedBossSystem`, `CalamityNetcode`, `CalamityGlobalNPC`) — the tModLoader-official documented pattern, already the project's stated intended approach per `CLAUDE.md`'s Tech Stack "What NOT to Use" table. User explicitly chose this over `research/PITFALLS.md`'s suggested safer alternative (pure runtime reflection). Every method touching a Calamity type must be fully isolated in its own method/class and tagged `[JITWhenModsEnabled("CalamityMod")]` — no partial isolation, per `research/PITFALLS.md` Pitfall 2.

**D-02 — Worked-example boss selection:** The phase's single worked-example boss is the earliest Calamity boss (in progression order) that triggers a WorldGen side effect — not simply the easiest/lowest-risk boss (e.g. Desert Scourge, which has no WorldGen effect) and not a boss the user currently fights for real. This boss must prove BOTH success criterion 2 (netcode/messaging side effects) AND success criterion 3 (WorldGen side effects) at once. **This research resolves D-02 to Hive Mind — see "D-02 Resolution" below.**

**D-03 — Scope (boss count):** Register exactly **one** Calamity boss this phase (the boss identified per D-02). Do not attempt broader Calamity coverage now.

**D-04 — Live verification approach (WorldGen test):** The WorldGen-triggering test runs against a **freshly created, dedicated test world** — NOT the backed-up main save Phase 3 used, because WorldGen effects permanently alter terrain.

**D-05 — Live verification approach (disabled-mod test):** Success criterion 4 ("mod continues to load and run safely with CalamityMod disabled") is verified via a **real in-game checkpoint** — disable CalamityMod, launch, confirm no JIT crash — not just a code review of `[JITWhenModsEnabled]` boundaries. Structured similarly to Phase 3's `03-03` live-verification checkpoint plan.

### Claude's Discretion

- Exact Calamity boss name satisfying D-02 — **resolved by this research to Hive Mind** (see below).
- Exact shape/naming of the cross-mod access helper class — **this research recommends `Integrations/CalamityIntegration.cs`** as a `ModSystem`, per `research/ARCHITECTURE.md`'s "Recommended Project Structure" and consistent with the existing `Systems/`-vs-feature-folder convention.
- Exact `weakReferences` version pin syntax in `build.txt` — **this research resolves to `weakReferences = CalamityMod@2.2.4`** (see "Exact weakReferences Syntax" below).
- Whether the WorldGen-test dedicated world is single-use-and-discarded or kept around for future phase reference — implementation/process detail, not a phase-blocking decision.

### Deferred Ideas (OUT OF SCOPE)

- Registering additional Calamity bosses beyond the one D-02 worked example (explicitly out of this phase's scope, D-03).
- Registering a specific late-game/flagship Calamity boss the user actually fights for FPS-relief purposes — deferred, low-marginal-cost future work once the pattern is proven.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MOD-01 | Calamity bosses registered via the `DownedBossSystem` wrapper-property pattern | Confirmed `DownedBossSystem` is a `public class DownedBossSystem : ModSystem` in namespace `CalamityMod` (not `CalamityMod.World` as prior project docs assumed — see Correction below), with `public static bool downedHiveMind { get; set; }` wrapper property whose setter calls `NPC.SetEventFlagCleared(ref _downedHiveMind, -1)`. Exact registration code shape given in Code Examples. |
| APPLY-02 | `BossRegistry.Apply` reproduces netcode/messaging side effects | Confirmed `CalamityNetcode.SyncWorld()` (`CalamityMod.CalamityNetcode`, safe no-op in singleplayer, gated on `Main.dedServ`) and `CalamityUtils.BroadcastLocalizedText(...)` (`CalamityMod.CalamityUtils`, stateless chat message) are the two side effects Hive Mind's real `OnKill()` fires. **Correction:** `CalamityGlobalNPC.SetNewBossJustDowned()` — named in the phase's canonical refs as a required side effect — is NOT recommended for replay; see "Important Correction" below. |
| APPLY-03 | `BossRegistry.Apply` reproduces WorldGen side effects | Confirmed `AerialiteOreGen.Enchant()` (`CalamityMod.World.AerialiteOreGen`) converts pre-placed `AerialiteOreDisenchanted` tiles into `AerialiteOre` tiles via `WorldGen.SquareTileFrame` — a genuine world-tile-mutation WorldGen side effect, decompiled and read directly. |
</phase_requirements>

## Summary

This research resolves both of the phase's open questions by decompiling the actually-installed `2026.6CalamityMod.tmod` (extracted via a custom `.tmod`-format parser, then decompiled with `ilspycmd`) rather than relying on the project's prior, partially-stale assumptions about Calamity's API shape.

**D-02 resolution: Hive Mind** (`CalamityMod.NPCs.HiveMind.HiveMind`) is the earliest Calamity boss in progression order with a confirmed WorldGen side effect. `DownedBossSystem`'s field declaration order (which mirrors Calamity's own progression) is: Desert Scourge → Crabulon → **Hive Mind** → Perforator → Slime God → Cryogen → ... Desert Scourge's `OnKill()` only triggers a sandstorm *event* (not WorldGen). Crabulon's `OnKill()` only triggers a Goblin Army *invasion* (not WorldGen). Hive Mind's `OnKill()` is the first to call genuine WorldGen: `AerialiteOreGen.Enchant()`, which mutates world tiles directly. Hive Mind's Crimson-world twin, Perforator (`PerforatorHive`), has the byte-for-byte identical side-effect code and is an equally valid alternate choice, but Hive Mind is recommended because (a) it's declared first, (b) it is a single-NPC-type boss (no worm-segment body parts to track, unlike Perforator/Desert Scourge), matching the low-structural-risk precedent Phase 3 set with King Slime.

**D-01 concrete detail:** Every Calamity member this phase needs to touch (`DownedBossSystem`, `downedHiveMind`, `CalamityNetcode.SyncWorld()`, `AerialiteOreGen.Enchant()`, `CalamityUtils.BroadcastLocalizedText()`, the `HiveMind` NPC class, the `Teratoma` summon item class) is `public`, confirming weak-reference compiled access is fully viable — no reflection fallback is needed for this boss. `weakReferences = CalamityMod@2.2.4` is the exact syntax to add to `build.txt` (verified against tModLoader's own `BuildProperties.cs` parsing logic: `ModName@Version`, version optional, parsed via `System.Version`).

**Primary recommendation:** Add one new file, `Integrations/CalamityIntegration.cs`, as a `ModSystem` whose `PostSetupContent()` is guarded by `ModLoader.HasMod("CalamityMod")` and calls one `[JITWhenModsEnabled("CalamityMod")]`-tagged private method that registers Hive Mind into both the existing, already-boss-agnostic `SummonItemRegistry` (Phase 2) and `BossRegistry` (Phase 3). **Zero changes are required to any existing file** — `SummonItemRegistry.Register(int,int)` and `BossRegistry.Register(string,BossDefinition)` already take plain primitives/delegates, so Calamity types never need to leak into shared code.

## Important Correction to Prior Project Research

Two claims in `PROJECT.md`/`CONTEXT.md`'s canonical references — both inherited from research done before this phase — do not match the actually-installed Calamity binary and should not be carried into planning as-is:

1. **Namespace is wrong.** `DownedBossSystem` is declared as `namespace CalamityMod; public class DownedBossSystem : ModSystem`, i.e. its full name is **`CalamityMod.DownedBossSystem`**, not `CalamityMod.World.DownedBossSystem` as `PROJECT.md`'s "Context" section and `04-CONTEXT.md`'s canonical refs state. (`CalamityNetcode` and `CalamityGlobalNPC`'s assumed namespaces — `CalamityMod.CalamityNetcode` and `CalamityMod.NPCs.CalamityGlobalNPC` — are both correct as previously assumed.)

2. **`CalamityGlobalNPC.SetNewBossJustDowned()` is not what it sounds like, and replaying it is likely wrong.** Its actual signature is `public static void SetNewBossJustDowned(NPC npc)` (requires a live `NPC` argument — there is no parameterless overload). Its body, read directly from the decompiled source:
   ```csharp
   public static void SetNewBossJustDowned(NPC npc)
   {
       if (GetDownedBossVariable(npc.type)) return; // no-op if this boss's flag is already true
       CalamityNPCSets.BossSpeedrunTimerID.TryGetValue(npc.type, out var value);
       for (int i = 0; i < 255; i++)
       {
           Player val = Main.player[i];
           if (val.active)
           {
               CalamityPlayer calamityPlayer = val.Calamity();
               calamityPlayer.lastSplitType = value;
               calamityPlayer.lastSplit = calamityPlayer.previousSessionTotal.Add(SpeedrunTimerSystem.Elapsed);
           }
       }
   }
   ```
   This is **per-player speedrun-split-timer bookkeeping** (`CalamityPlayer.lastSplitType`/`lastSplit`, a `ModPlayer`-scoped field), not a "boss just downed" banner/achievement broadcast. Per this phase's own Pitfall 5 discipline (player-scoped state that survives the subworld round-trip automatically via the live player object must NOT be replayed in `Apply()`, or it double-applies): when the player kills the real Hive Mind NPC inside the arena subworld, `HiveMind.OnKill()` already calls `CalamityGlobalNPC.SetNewBossJustDowned(npc)` for real, on the live NPC, updating every active `CalamityPlayer`'s split-timer fields — and because `Subworld.NoPlayerSaving = false`, those `ModPlayer` field changes already persist across the subworld exit with no carrier item involved. Calling `SetNewBossJustDowned()` again from `BossRegistry.Apply()` in the main world would double-apply this player-scoped state.
   **Recommendation:** Do NOT call `CalamityGlobalNPC.SetNewBossJustDowned()` from `Apply()`. The netcode/messaging side effects that actually need replay (APPLY-02) are `CalamityNetcode.SyncWorld()` and `CalamityUtils.BroadcastLocalizedText(...)` — both are stateless/world-scoped and safe to replay. If the planner disagrees and wants it called anyway for maximum fidelity, a synthetic `NPC` instance (`new NPC { type = ModContent.NPCType<HiveMind>() }`, never spawned into `Main.npc[]`) is sufficient since the method only reads `.type` — but this is not the recommended path.

## Standard Stack

### Core (unchanged from Phase 1-3, no new runtime dependency)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| tModLoader | 1.4.4.9+2026.06.3.6 (locally installed, confirmed via `FileVersionInfo`) | Mod loader/runtime | Existing project constraint |
| .NET SDK | 8.0.x | Compiles the mod | Existing project constraint |
| CalamityMod | 2.2.4 (confirmed via `.tmod` header: `2026.6CalamityMod.tmod`, built against tModLoader `2026.6.3.4`) | First real content-mod integration target | This phase's subject; version confirmed compatible with the locally installed tModLoader (same `2026.06` stable branch, patch build 3.4 vs 3.6) |

**New build-time-only reference (mirrors the existing `SubworldLibrary` pattern):**

```
Libs/CalamityMod.dll   (gitignored, per existing .gitignore's `Libs/` rule — no change needed)
```

Extract `CalamityMod.dll` from the installed `Mods/2026.6CalamityMod.tmod` the same way `Libs/SubworldLibrary.dll` was extracted for Phase 1 (the `.tmod` format is NOT a plain zip — it's tModLoader's custom binary container: `TMOD` magic → length-prefixed tModLoader-version string → 20-byte SHA1 hash → 256-byte signature → int32 data length → length-prefixed mod name/version strings → file table (name, uncompressed length, compressed length per entry) → deflate-compressed file blobs). A working Python extractor for this format is documented in Code Examples below in case the project needs to re-extract from a future Calamity update.

### build.txt change

```
weakReferences = SubworldLibrary is a modReferences, unaffected
weakReferences = CalamityMod@2.2.4
```

(Existing `modReferences = SubworldLibrary` line is untouched; add a new `weakReferences` line.)

### .csproj change (mirrors the existing SubworldLibrary `<Reference>` block exactly)

```xml
<Reference Include="CalamityMod" Condition="Exists('Libs\CalamityMod.dll')">
  <HintPath>Libs\CalamityMod.dll</HintPath>
  <Private>false</Private>
</Reference>
```

### Exact weakReferences Syntax (verified against tModLoader source)

tModLoader's `BuildProperties.cs` parses both `modReferences` and `weakReferences` identically via `ModReference.Parse`: splits each comma-separated entry on `@`; one part = mod name only (no version pin); two parts = `ModName@Version`, parsed as `System.Version` (accepts 2-4 dotted components, e.g. `2.2.4`). More than one `@` throws. **Confidence: HIGH on syntax** (read directly from tModLoader source), **MEDIUM on exact match-vs-minimum-version comparison semantics** (not confirmed from source in this pass) — this is a non-issue in practice because a version mismatch produces a clear tModLoader load-time error, not a silent failure, consistent with this phase's own Pitfall 3 "loudly flagged over silently wrong" principle.

## Architecture Patterns

### D-02 Resolution: Hive Mind, with evidence

**Evidence chain (all read directly from the decompiled `CalamityMod.dll`):**

1. `DownedBossSystem`'s internal field declaration order (which is the authoritative progression ordering used throughout the class, including `ResetAllFlags()` and `SaveWorldData()`):
   `_downedDesertScourge, _downedCrabulon, _downedHiveMind, _downedPerforator, _downedSlimeGod, _downedCryogen, _downedAquaticScourge, _downedBrimstoneElemental, ...`

2. **Desert Scourge** `OnKill()` — no WorldGen:
   ```csharp
   CalamityGlobalNPC.SetNewBossJustDowned(NPC);
   if (!DownedBossSystem.downedDesertScourge) {
       CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.OpenSunkenSea", Color.Aquamarine);
       CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.SandstormTrigger", Color.PaleGoldenrod);
       if (!Sandstorm.Happening) CalamityWorld.StartSandstorm(); // event trigger, not WorldGen
   }
   DownedBossSystem.downedDesertScourge = true;
   CalamityNetcode.SyncWorld();
   ```

3. **Crabulon** `OnKill()` — no WorldGen:
   ```csharp
   CalamityGlobalNPC.SetNewBossJustDowned(NPC);
   if (!NPC.downedGoblins && Main.netMode != 1 && ...) Main.StartInvasion(1); // Goblin Army invasion, not WorldGen
   DownedBossSystem.downedCrabulon = true;
   CalamityNetcode.SyncWorld();
   ```

4. **Hive Mind** `OnKill()` (`CalamityMod.NPCs.HiveMind.HiveMind.OnKill`) — first genuine WorldGen effect:
   ```csharp
   public override void OnKill()
   {
       if (!BossRushEvent.BossRushActive)
       {
           CalamityGlobalNPC.SetNewBossJustDowned(((ModNPC)this).NPC);
           if (!DownedBossSystem.downedHiveMind && !DownedBossSystem.downedPerforator)
           {
               AerialiteOreGen.Enchant();  // <-- genuine WorldGen: converts placed tiles world-wide
               CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.SkyOreText", Color.Cyan);
           }
           DownedBossSystem.downedHiveMind = true;
           CalamityNetcode.SyncWorld();
       }
   }
   ```
   `AerialiteOreGen.Enchant()` (`CalamityMod.World.AerialiteOreGen`):
   ```csharp
   public static void Enchant()
   {
       if (Main.netMode == 1) return;
       ushort disenchanted = (ushort)ModContent.TileType<AerialiteOreDisenchanted>();
       ushort real = (ushort)ModContent.TileType<AerialiteOre>();
       for (int i = 5; i < Main.maxTilesX - 5; i++)
           for (int j = 5; j < Main.worldSurface; j++)
           {
               Tile t = Main.tile[i, j];
               if (t.TileType == disenchanted)
               {
                   t.TileType = real;
                   WorldGen.SquareTileFrame(i, j, true);
                   if (Main.dedServ) NetMessage.SendTileSquare(-1, i, j, TileChangeType.None);
               }
           }
   }
   ```
   This is a direct `Main.tile[...].TileType` mutation plus `WorldGen.SquareTileFrame` — unambiguously a WorldGen side effect, not merely a flag or event.

5. **Perforator** (`CalamityMod.NPCs.Perforator.PerforatorHive.OnKill`) has the byte-identical pattern (`!downedHiveMind && !downedPerforator` guard, same `AerialiteOreGen.Enchant()` call) — confirming Hive Mind and Perforator are the Corruption/Crimson-equivalent pair for this exact WorldGen effect. Perforator is NOT recommended as the worked example only because it is structurally a multi-NPC-type worm boss (`PerforatorHeadSmall/Medium/Large`, `PerforatorBodySmall/Medium/Large`, `PerforatorTailSmall/Medium/Large`) — more `NpcTypes` entries to track for no additional research value, since the side-effect code is identical to Hive Mind's.

6. `AerialiteOreGen.Generate()` (which places the "disenchanted" ore tiles `Enchant()` later converts) runs unconditionally as a normal `GenPass` named `"Aerialite"`, inserted via `ModifyWorldGenTasks` in `CalamityMod.Systems.WorldgenManagementSystem` — confirmed NOT gated behind hardmode or world-evil-type. This means **any freshly created world with CalamityMod enabled already has the target tiles present**, satisfying D-04's "freshly created, dedicated test world" requirement with zero special world-gen settings — just "New World" with CalamityMod enabled.

### Recommended Integration Shape

```csharp
// Integrations/CalamityIntegration.cs
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using BossArenaSubWorld.Systems;

namespace BossArenaSubWorld.Integrations
{
    public class CalamityIntegration : ModSystem
    {
        public override void PostSetupContent()
        {
            if (!ModLoader.HasMod("CalamityMod")) return;
            RegisterHiveMind();
        }

        // Every member below this point may reference CalamityMod types; this method
        // itself is the one JIT-deferred boundary per D-01/Pitfall 2 -- PostSetupContent()
        // above never touches a Calamity type directly, so it JITs safely regardless of
        // whether CalamityMod is installed.
        [JITWhenModsEnabled("CalamityMod")]
        private void RegisterHiveMind()
        {
            int itemType = ModContent.ItemType<CalamityMod.Items.SummonItems.Teratoma>();
            int npcType = ModContent.NPCType<CalamityMod.NPCs.HiveMind.HiveMind>();

            // SummonItemRegistry/BossRegistry are boss-agnostic (int/string + delegates) --
            // zero changes needed to either existing file (Phase 2/Phase 3 code untouched).
            SummonItemRegistry.Register(itemType, npcType);

            BossRegistry.Register("calamity:hive_mind", new BossDefinition(
                NpcTypes: new[] { npcType },
                ApplyDowned: ApplyHiveMindDowned,
                IsDowned: () => CalamityMod.DownedBossSystem.downedHiveMind));
        }

        [JITWhenModsEnabled("CalamityMod")]
        private static void ApplyHiveMindDowned()
        {
            // Faithful replay of HiveMind.OnKill()'s exact conditional (checks BOTH
            // downedHiveMind and downedPerforator, matching Calamity's own source --
            // do not simplify to a single-flag check, per Pitfall 4 discipline).
            if (!CalamityMod.DownedBossSystem.downedHiveMind && !CalamityMod.DownedBossSystem.downedPerforator)
            {
                CalamityMod.World.AerialiteOreGen.Enchant();
                CalamityMod.CalamityUtils.BroadcastLocalizedText(
                    "Mods.CalamityMod.Status.Progression.SkyOreText", Color.Cyan);
            }
            CalamityMod.DownedBossSystem.downedHiveMind = true; // wrapper setter calls NPC.SetEventFlagCleared internally
            CalamityMod.CalamityNetcode.SyncWorld();
            // Deliberately NOT calling CalamityGlobalNPC.SetNewBossJustDowned() -- see
            // "Important Correction" above (player-scoped, already applied for real at
            // the actual subworld kill).
        }
    }
}
```

**Why this requires zero changes to `Systems/BossRegistry.cs`, `Systems/SummonItemRegistry.cs`, `Systems/BossSummonPlayer.cs`, or `GlobalNPCs/BossKillGlobalNPC.cs`:** all four are already fully boss-agnostic (they operate on `int`/`string`/delegates, never a source-mod type), confirmed by direct read of each file. `CalamityIntegration.Register()`-equivalent logic self-registers via its own `ModSystem.PostSetupContent()`, exactly the same lifecycle hook `BossRegistry` itself uses to register King Slime — load order between the two `ModSystem`s does not matter here because `SummonItemRegistry`/`BossRegistry`'s dictionaries are populated once, independently, with no cross-reads during `PostSetupContent`.

### Spawn-side note: `Teratoma.CanUseItem` is irrelevant to this project

`Teratoma` (`CalamityMod.Items.SummonItems.Teratoma`, the item that summons Hive Mind) gates its own `CanUseItem` on `player.ZoneCorrupt` (a real Corruption biome check) — which will always be `false` inside the content-free boss-arena subworld (SUBW-05). This does **not** block this project, because Phase 2 already established the pattern of never replaying a summon item's real `UseItem`/`CanUseItem` — `BossSummonPlayer.OnEnterWorld()` calls `NPC.SpawnOnPlayer(Player.whoAmI, PendingBossNpcType.Value)` directly. This is independently confirmed correct for Calamity: `Teratoma.UseItem()` itself is implemented as `CalamityUtils.SpawnBossUsingItem<HiveMind>(player, spawnSound)`, whose singleplayer path (`Main.netMode == 0`) is literally `NPC.SpawnOnPlayer(player.whoAmI, npcType)` — the exact same primitive this project already calls generically. **No new spawn-side code is needed** — only a `SummonItemRegistry.Register(ItemType<Teratoma>(), NPCType<HiveMind>())` call, shown above.

### Anti-Pattern to avoid this phase

Do not simplify Hive Mind's `ApplyDowned()` to just `downedHiveMind = true; SyncWorld();` — this would skip the WorldGen ore-enchant + broadcast (APPLY-03's whole point) and would only be caught by literally checking the world's Aerialite ore tiles in-game, not by a compile check. Always replay the exact conditional structure from the source `OnKill()`, not a shortcut.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|--------------|-----|
| Extracting a `.tmod` file's contents | A generic zip-extraction script (it will silently produce garbage — `.tmod` is NOT a zip despite superficially "renamable archive" folklore) | The custom binary-format parser documented in Code Examples (magic `TMOD` → version string → hash/signature → file table → per-file deflate blobs), or `ilspycmd`/a full `Assembly.Load` after extraction | Confirmed via hex-dump of the actual file: header starts `54 4D 4F 44` (`TMOD`), not `50 4B` (`PK`, zip magic). A naive unzip attempt fails outright. |
| Decompiling a mod's compiled types to verify actual API shape | Trusting `PROJECT.md`'s prior (pre-this-phase) research on Calamity's namespaces uncritically | `ilspycmd` (installable via `dotnet tool install -g ilspycmd --version 8.2.0.7535` when the latest version's NuGet metadata is broken/unresolvable) against the extracted `.dll` | This exact process caught a real namespace error (`CalamityMod.World.DownedBossSystem` → actually `CalamityMod.DownedBossSystem`) that would have caused a compile-time error if trusted blindly. |

**Key insight:** For any Calamity-version-specific claim inherited from `PROJECT.md`/prior phases, prefer re-verifying against the actually-installed `.tmod` over trusting the written claim — Calamity is a large, frequently-refactored mod and namespace/signature drift between the version `PROJECT.md`'s original research targeted and the version now installed (`2.2.4`, built for tModLoader `2026.6.3.4`) is real and already caught once in this pass.

## Common Pitfalls

### Pitfall: Replaying `SetNewBossJustDowned()` double-applies player-scoped state
**What goes wrong:** Calling `CalamityGlobalNPC.SetNewBossJustDowned(npc)` from `BossRegistry.Apply()` re-runs a per-player speedrun-timer update that already ran for real during the actual subworld kill (because `HiveMind.OnKill()` already called it on the live NPC with live players present, and `Subworld.NoPlayerSaving = false` means those `CalamityPlayer` field changes already survived the exit).
**Why it happens:** The phase's own canonical refs (inherited from `PROJECT.md`) assumed this method was a stateless "boss just downed" banner call; it is not, in the currently-installed Calamity version.
**How to avoid:** Do not call it from `ApplyDowned()`. Rely on `CalamityNetcode.SyncWorld()` + `CalamityUtils.BroadcastLocalizedText(...)` to satisfy APPLY-02.
**Warning signs:** None observable in-game (the effect is an internal timer field with no visible UI in normal play) — this is exactly the kind of "looks done but isn't" risk `research/PITFALLS.md` Pitfall 5 warns about. Treat this research finding as authoritative rather than waiting for an observable symptom.

### Pitfall: Simplifying Hive Mind's WorldGen guard to a single-flag check
**What goes wrong:** Calamity's real code checks `!downedHiveMind && !downedPerforator` before enchanting ore — a plausible-looking simplification (`!downedHiveMind` alone) would still pass this phase's own success criteria in isolation (Hive Mind is the only Calamity boss registered this phase) but would silently diverge from source-fidelity, and would double-enchant already-enchanted ore if Perforator is ever registered in a future phase without this exact guard being preserved.
**How to avoid:** Copy the exact two-flag conditional shown in Code Examples, not a simplified one-flag version, even though only one flag is exercised by this phase's own test.

### Pitfall (inherited, reconfirmed applicable): JIT crash if any Calamity-type reference leaks outside a tagged method
**Confirmed via decompiled evidence this phase:** every Calamity member this integration touches is `public`, so there's no reflection-vs-weak-reference tradeoff to make for the flag/netcode/WorldGen calls, but the isolation discipline (D-01) still fully applies — `PostSetupContent()` itself must never directly reference `CalamityMod.*` types, only call into `[JITWhenModsEnabled("CalamityMod")]`-tagged methods, guarded by `ModLoader.HasMod("CalamityMod")` first.

## Code Examples

### `.tmod` extraction (Python, verified working against the installed CalamityMod.tmod)

```python
import struct, zlib, os

def read_7bit_int(f):
    result, shift = 0, 0
    while True:
        b = f.read(1)[0]
        result |= (b & 0x7f) << shift
        if not (b & 0x80): break
        shift += 7
    return result

def read_string(f):
    return f.read(read_7bit_int(f)).decode('utf-8')

def extract_tmod(tmod_path, out_dir):
    with open(tmod_path, 'rb') as f:
        assert f.read(4) == b'TMOD'
        tml_version = read_string(f)
        f.read(20)   # SHA1 hash
        f.read(256)  # signature
        struct.unpack('<i', f.read(4))[0]  # data length
        mod_name = read_string(f)
        mod_version = read_string(f)
        file_count = struct.unpack('<i', f.read(4))[0]
        entries = [(read_string(f), *struct.unpack('<ii', f.read(8))) for _ in range(file_count)]
        offset = f.tell()
        os.makedirs(out_dir, exist_ok=True)
        for name, ulen, clen in entries:
            f.seek(offset)
            raw = f.read(clen)
            data = zlib.decompress(raw, -15) if clen != ulen else raw
            path = os.path.join(out_dir, name.replace('/', os.sep))
            os.makedirs(os.path.dirname(path) or '.', exist_ok=True)
            open(path, 'wb').write(data)
            offset += clen
```

### Decompiling the extracted DLL

```bash
dotnet tool install -g ilspycmd --version 8.2.0.7535   # latest (11.x) NuGet metadata was broken/unresolvable in this environment
ilspycmd -l c CalamityMod.dll                            # list all types
ilspycmd -t "CalamityMod.NPCs.HiveMind.HiveMind" CalamityMod.dll   # decompile one type
```

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None — tModLoader mod, no automated in-game test harness (confirmed, matches Phase 1-3's established `0X-VALIDATION.md` precedent) |
| Config file | none |
| Quick run command | `dotnet build BossArenaSubWorld.csproj` |
| Full suite command | N/A — "full verification" is the two live in-game checkpoints below (D-04, D-05), each requiring a world backup/dedicated world per the phase's locked decisions |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| MOD-01 | Hive Mind registered with `DownedBossSystem` wrapper property | build (compile-time type check against `Libs/CalamityMod.dll`) | `dotnet build BossArenaSubWorld.csproj` | ❌ Wave 0 (new file: `Integrations/CalamityIntegration.cs`) |
| APPLY-02 | Using the carrier item calls `CalamityNetcode.SyncWorld()` + broadcasts the Sky Ore chat message | manual-only | live in-game: use carrier item, observe chat message; `SyncWorld()` itself has no singleplayer-visible effect (gated on `Main.dedServ`) — its correctness is a code-review/decompile-match check, not observable live | ❌ Wave 0 |
| APPLY-03 | Using the carrier item converts `AerialiteOreDisenchanted` tiles to `AerialiteOre` on a fresh CalamityMod-enabled world | manual-only, **dedicated test world per D-04** | live in-game: create new world w/ CalamityMod enabled, locate a placed Aerialite ore tile (surface-to-underground band, `y < worldSurface`), use carrier item, confirm tile visually changes from disenchanted to enchanted variant | ❌ Wave 0 |
| (Success Criterion 4) | Mod loads safely with CalamityMod disabled | manual-only, **real checkpoint per D-05** | disable CalamityMod in Mod Configuration, launch, confirm no crash/JIT exception in client log | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet build BossArenaSubWorld.csproj` (0 warnings/errors expected; requires `Libs/CalamityMod.dll` present locally, gitignored per existing `.gitignore` rule)
- **Per wave merge:** same build command
- **Phase gate:** both live checkpoints (D-04 WorldGen test, D-05 disabled-mod test) green before `/gsd:verify-work`, structured as two separate `checkpoint:human-verify` tasks mirroring Phase 3's `03-03-PLAN.md` shape

### Wave 0 Gaps
- [ ] `Integrations/CalamityIntegration.cs` — new file, no automated test beyond the build gate
- [ ] `Libs/CalamityMod.dll` — must be extracted from `Mods/2026.6CalamityMod.tmod` into the project's `Libs/` folder before `dotnet build` will resolve the new `<Reference>` (per-worktree manual step, exactly like the existing `Libs/SubworldLibrary.dll` requirement noted in `PROJECT.md`'s Phase 2 decisions)
- [ ] `build.txt` — add `weakReferences = CalamityMod@2.2.4`
- [ ] `.csproj` — add the `<Reference Include="CalamityMod">` block

*No automated test-framework gap beyond the build gate and the two live checkpoints — matches this project's established, previously-approved manual-verification model.*

## Open Questions

1. **Does the boss-arena subworld's absence of a Corruption biome affect Hive Mind's combat AI or difficulty?**
   - What we know: `Teratoma.CanUseItem`'s `ZoneCorrupt` gate is bypassed entirely by this project's spawn mechanism (`NPC.SpawnOnPlayer`), so the boss WILL spawn in the content-free arena regardless of biome.
   - What's unclear: Whether Hive Mind's in-combat AI (movement patterns, minion summons) reads `player.ZoneCorrupt` or similar biome flags during the fight itself (not just at spawn-gate time) in a way that changes behavior when the biome is absent. Not resolvable via the OnKill excerpt alone; would require decompiling `HiveMind.AI()`.
   - Recommendation: Not a blocker — the phase's success criteria only require a successful kill and correct downed-state replay, not faithful combat difficulty. Flag as something to observe (not specifically verify) during the D-04/D-05 live checkpoints; if Hive Mind's AI errors out or soft-locks due to a missing biome check, that would surface immediately as a failed kill attempt.

2. **Exact `weakReferences` version-pin comparison semantics (minimum vs. exact match).**
   - What we know: Syntax is `ModName@Version`, parsed via `System.Version`.
   - What's unclear: Whether tModLoader requires the installed mod's version to be `>=` the pin or requires an exact match, when a `weakReferences` version is specified.
   - Recommendation: Non-blocking — pin to the exact installed version (`2.2.4`); if this proves too strict in practice (e.g. after a minor Calamity update), the failure mode is a clear tModLoader load-time message, not a silent bug, so it's safe to discover and relax later.

## Sources

### Primary (HIGH confidence — read directly from the actually-installed binaries)
- `Mods/2026.6CalamityMod.tmod` (extracted + decompiled with `ilspycmd 8.2.0.7535`) — `CalamityMod.DownedBossSystem`, `CalamityMod.CalamityNetcode`, `CalamityMod.NPCs.CalamityGlobalNPC`, `CalamityMod.NPCs.HiveMind.HiveMind`, `CalamityMod.NPCs.Perforator.PerforatorHive`, `CalamityMod.World.AerialiteOreGen`, `CalamityMod.CalamityUtils`, `CalamityMod.Items.SummonItems.Teratoma`, `CalamityMod.Systems.WorldgenManagementSystem` — all read directly, full method bodies inspected
- `D:\SteamLibrary\steamapps\common\tModLoader\tModLoader.dll` (locally installed, decompiled) — confirmed `Terraria.ModLoader.JITWhenModsEnabledAttribute`, `Terraria.ModLoader.ModLoader.HasMod(string)`/`TryGetMod(string, out Mod)` signatures
- Existing project source: `Systems/BossRegistry.cs`, `Systems/SummonItemRegistry.cs`, `Systems/BossSummonPlayer.cs`, `GlobalNPCs/BossKillGlobalNPC.cs`, `Items/BossCoreItem.cs`, `BossArenaSubWorld.csproj`, `.gitignore` — confirmed the existing pipeline is fully boss-agnostic and requires zero modification

### Secondary (MEDIUM-HIGH confidence)
- https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/patches/tModLoader/Terraria/ModLoader/Core/TmodFile.cs (via WebFetch summary) — `.tmod` binary format spec used to write the extractor
- https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/patches/tModLoader/Terraria/ModLoader/Core/BuildProperties.cs (via WebFetch summary) — `weakReferences`/`modReferences` `ModName@Version` parsing confirmed from source

### Carried over from prior phase research (still HIGH confidence, unaffected by this phase's corrections)
- `.planning/research/PITFALLS.md` Pitfall 2 (JIT crash), Pitfall 4 (under-reproduced side effects), Pitfall 5 (player-scoped double-grant) — all directly applied in this research's recommendations
- https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content — `[JITWhenModsEnabled]`/weak-reference pattern, independently reconfirmed against the live `tModLoader.dll` this pass

## Metadata

**Confidence breakdown:**
- Standard stack (weakReferences syntax, .csproj shape): HIGH — verified against tModLoader source + existing project precedent
- Architecture (integration shape, zero-change-to-existing-files claim): HIGH — every touched existing file was read directly this pass
- D-02 boss resolution: HIGH — full `OnKill()` bodies read for Desert Scourge, Crabulon, Hive Mind, Perforator; WorldGen call chain traced to `Main.tile` mutation
- Pitfalls (SetNewBossJustDowned correction): HIGH — full method body read; reasoning follows the phase's own already-locked Pitfall 5 framework
- Live-combat-AI open question: LOW/unresolved — flagged, not blocking

**Research date:** 2026-08-13
**Valid until:** Re-verify if CalamityMod updates past version 2.2.4 (large mod, refactors namespaces/signatures between versions — already caught one such drift from this project's own prior research in this pass). tModLoader/.NET stack portions are stable (30+ days).
