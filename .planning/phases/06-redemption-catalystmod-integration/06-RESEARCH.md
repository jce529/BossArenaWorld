# Phase 6: Redemption & CatalystMod Integration - Research

**Researched:** 2026-08-14
**Domain:** tModLoader cross-mod boss-registration (decompiled DLL API discovery, `.tmod` binary extraction)
**Confidence:** HIGH (both bosses' entire relevant API surface was confirmed by decompiling the actual installed assemblies, not inferred from documentation or training data)

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01 (CatalystMod source-access):** CatalystMod's modder explicitly hid code, resources, and the `.dll` itself from tModReader's extraction (`extract.log`: "The modder has chosen to hide the code... has chosen to hide resources"; a `HelloDataminers.txt` file is present). User explicitly chose to proceed anyway: decompile the installed `.tmod`'s embedded DLL directly via `ilspycmd`, same tool/approach as Phase 4/5/9. This bypasses tModReader's respect for the modder's stated preference — a deliberate, informed choice for personal/individual use against the user's own installed copy, no redistribution planned.
- **D-02 (Redemption boss selection):** No boss requested by the user. Apply the same selection discipline as Phase 4 (Hive Mind) and Phase 5 (Infernon): decompile all 10 Redemption bosses' `OnKill()` methods and pick the richest-side-effects one as the worked example.
- **D-03 (CatalystMod boss selection):** User specified **Astrageldon** based on asset-density signals (dedicated loading-screen art, background, pet projectile). Research must still confirm Astrageldon has a boss-level `OnKill()` with reproducible side effects before finalizing it; if not, fall back to the richest-side-effect heuristic.
- **Scope (carried forward, not re-discussed):** Register exactly **one** boss per mod (2 total) this phase. Full-roster registration remains out of v1 scope for every mod-integration phase.

### Claude's Discretion

- Exact `weakReferences` version pin syntax in `build.txt` for Redemption and CatalystMod (confirm installed `.tmod` version strings during research).
- Exact shape/naming of the two new integration files (`Integrations/RedemptionIntegration.cs`, `Integrations/CatalystIntegration.cs`), following the established `CalamityIntegration.cs`/`SpiritIntegration.cs` naming convention.
- Whether the selected boss has any `player.Zone*`-gated AI despawn dependency requiring a `BossArenaRoutingRegistry` biome-arena entry — research must check this explicitly; Phase 9's biome coverage only covers bosses that existed in the registry at Phase 9 execution time (Calamity, Spirit).
- **CRITICAL — carried forward from Phase 4's hard-won lesson:** Any delegate passed into a `[JITWhenModsEnabled(...)]`-guarded registration call (`BossDefinition.IsDowned`, `ApplyDowned`) MUST be a named, separately-tagged method — never an inline lambda. Locked project-wide rule, not open for reconsideration.
- Whether the selected boss has any player-scoped side effect requiring exclusion logic (Phase 4's Hive Mind pattern) vs. being fully world-scoped (Phase 3/5's pattern) — research determines per boss.

### Deferred Ideas (OUT OF SCOPE)

- Registering the remaining Redemption bosses (9 of 10) and any other CatalystMod bosses beyond Astrageldon — explicitly out of this phase's scope.
- Retroactive biome-classification sweep for Phase 6/7 bosses beyond what this phase itself determines for its own two bosses — flagged for whoever plans the phase after Phase 7/8/9 to pick up.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MOD-03 | Redemption bosses researched (downed-progress API) and registered | Full API confirmed via decompile: `Redemption.Globals.RedeBossDowned` is a `public class : ModSystem` with `public static bool` fields (e.g. `downedThorn`) — directly writable, zero reflection. Worked-example boss selected: **Thorn** (see Summary). All 10 boss `OnKill()`/`BossLoot()` bodies read and compared for the D-02 richness heuristic. |
| MOD-04 | CatalystMod bosses researched (downed-progress API) and registered | Full API confirmed via decompile (after manually extracting `CatalystMod.dll` from the installed `.tmod`, since tModReader could not — see D-01). `CatalystMod.WorldDefeats` is a `public class : ModSystem` with `public static bool downedAstrageldon` — directly writable, zero reflection. Astrageldon's `OnKill()` confirmed to have reproducible side effects (WorldGen ore-vein generation via `MetanovaGenerator.Generate()`), satisfying D-03's confirmation requirement. |
</phase_requirements>

## Summary

Both mods' downed-progress APIs turned out to be the **simplest of the four mods integrated so far** — both `Redemption.Globals.RedeBossDowned.downed*` and `CatalystMod.WorldDefeats.downed*` are `public static bool` fields on `public class : ModSystem`, directly writable with zero reflection (unlike Spirit's `internal` dictionary in Phase 5, and simpler than Calamity's wrapper properties in Phase 4 — though CatalystMod's `Astrageldon.OnKill()` does use vanilla's own `NPC.SetEventFlagCleared` helper exactly like this project's own King Slime pattern from Phase 3).

The main effort in this phase was not API discovery but **file access**: CatalystMod's `.tmod` is not present anywhere the CONTEXT.md assumed (not in the local `Mods/` folder, not in `enabled.json`). It was located at `D:\SteamLibrary\steamapps\workshop\content\1281930\2838015851\2026.6\CatalystMod.tmod` (a Steam Workshop content cache, not the local install folder) and successfully extracted using this project's own existing `scripts/extract_tmod.py` (already built in Phase 4 for Calamity, independently re-derived and cross-validated during this research by decompiling `Terraria.ModLoader.Core.TmodFile` itself). **CatalystMod.dll and CatalystMod.pdb are ordinary, fully-present file entries inside the `.tmod` container** — the modder's "hide code/resources" flag is a courtesy tModReader (a third-party tool) chooses to respect; it is not a technical protection baked into the `.tmod` binary format, and does not prevent extraction. Neither Redemption nor CatalystMod is currently installed/enabled in the local `Mods/` folder (see Environment Availability) — this blocks live in-game verification until the user re-subscribes/enables both, though it does not block decompile-based research or code-level implementation.

**Worked-example boss selections:**
- **Redemption: Thorn** (not Patient Zero, despite PZ having the most distinct progression systems touched — see "Important Correction" below). Thorn's `OnKill()` has three side effects beyond the flag: a net-mode-aware chat broadcast (explicit `Main.netMode == 2` server-broadcast vs. `Main.netMode == 0` singleplayer `Main.NewText` branches), a `RedeWorld.Alignment += 2` change (a public wrapper *property* whose setter internally calls `SyncAlignment()` — a genuine netcode-sync side effect, structurally identical to Calamity's wrapper-property pattern from Phase 4), and a `ChaliceAlignmentUI.BroadcastDialogue(...)` UI/dialogue broadcast. Its summon item (`HeartOfThorns`) is a simple, unconditional `NPC.SpawnOnPlayer(player, NPCType<Thorn>())` — the same shape as Calamity's Teratoma and Spirit's CursedCloth. No `CheckActive()` override and no `player.Zone*` reference anywhere in its ~2900-line decompiled source — no biome-arena routing needed.
- **CatalystMod: Astrageldon** (D-03 confirmed, no fallback needed). `OnKill()` calls `MetanovaGenerator.Generate()` (a genuine WorldGen ore-vein generator, structurally identical to Calamity's `AerialiteOreGen.Enchant()` from Phase 4) gated on `!WorldDefeats.downedAstrageldon`, then `NPC.SetEventFlagCleared(ref WorldDefeats.downedAstrageldon, -((ModNPC)this).Type)` — note the `-Type` gameEventId argument, not `-1` like every other boss in this project so far; replicate it exactly. `CheckActive()` is explicitly overridden to `return false` (never auto-despawns by distance) and no `player.Zone*` reference exists in AI — no biome-arena routing needed. **CatalystMod hard-depends on CalamityMod** (`modReferences = CalamityMod` in its own `build.txt`) and Astrageldon's own code calls into `CalamityUtils`/`CalamityWorld` for difficulty scaling — this is expected and does not change our own `weakReferences`/`[JITWhenModsEnabled]` requirements (CalamityMod is already a weak reference from Phase 4).

**Important correction to the D-02 selection heuristic:** Patient Zero (PZ) has the richest *progression-system* footprint of all 10 Redemption bosses (touches `RedeQuest.adviceUnlocked`/`adviceSeen` arrays + `RedeQuest.SyncData()` + a separate `LabArea.labAccess` array + a `downedGGBossFirst` ordinal), but its summon item (`LabHologramDevice`) does not directly spawn PZ — it spawns one of five different "Holo" pre-fight NPCs depending on which of five hardcoded world-position rectangles (anchored to `RedeGen.LabVector`, a structure Redemption itself generates during main-world WorldGen) the player is standing in. This project's established SUBW-04 pattern (Phase 2 D-09: map summon-item → boss NPC type, then call `NPC.SpawnOnPlayer` directly in the subworld, bypassing the item's real `UseItem()`) still technically works around this, but PZ's *actual* summon chain is multi-stage and location-gated in a way Thorn's and Keeper's are not, making it a poorer "worked example" for future phases to pattern-match against. **Richness was re-weighted to favor bosses whose summon item performs a direct, unconditional `NPC.SpawnOnPlayer(player, <the actual boss type>)` call** — Thorn (chosen) and Keeper (runner-up, see Alternatives) both satisfy this; PZ, KS3 (spawns an intermediate `KS3_Start` cutscene NPC, not `KS3` itself), and the two "Omega" trio bosses reached via the multi-stage `OmegaTransmitter` do not.

**Primary recommendation:** Register Redemption's `Thorn` (direct field write, no reflection, no Zone dependency) and CatalystMod's `Astrageldon` (direct field write via vanilla's own `SetEventFlagCleared` helper, no reflection, no Zone dependency) using the exact same `BossDefinition`/`[JITWhenModsEnabled]`-per-method/named-delegate pattern established in `Integrations/CalamityIntegration.cs` and `Integrations/SpiritIntegration.cs`. Both mods must be re-subscribed/enabled in the local `Mods/` folder before any live in-game verification checkpoint can run (see Environment Availability) — this does not block code-level implementation.

## Standard Stack

### Core (tooling, not runtime libraries — this phase adds no new NuGet/library dependencies)

| Tool | Version (confirmed) | Purpose | Why Standard |
|------|---------------------|---------|---------------|
| `ilspycmd` | 8.2.0.7535 (locally installed; 11.0.0.9375 is latest upstream — update optional, not required) | Decompile `Redemption.dll` and the manually-extracted `CatalystMod.dll` | Same tool used in Phase 4/5/9 against CalamityMod.dll/SpiritMod.dll; already proven to produce compilable-enough C# for research purposes |
| `scripts/extract_tmod.py` (already exists in this repo, built in Phase 4) | N/A (project script) | Parses tModLoader's custom `.tmod` binary container (NOT a zip) and extracts every embedded file, including `CatalystMod.dll`/`.pdb` | The only tool needed — confirmed correct by independently decompiling `Terraria.ModLoader.Core.TmodFile` itself (the actual class tModLoader uses to read `.tmod` files) during this research and cross-validating byte-for-byte against this existing script's logic. No third-party `.tmod` extraction tool is needed or recommended. |
| Python 3.13 (`C:\Users\chang\AppData\Local\Programs\Python\Python313\python.exe`) | 3.13.0 | Runs `extract_tmod.py` | **Environment pitfall:** the bare `python`/`python3` commands on this machine resolve to the Windows Store app-execution-alias stub (prints "Python" and exits, does nothing) — must invoke the full path above, or use the `py` launcher (`C:\Users\chang\AppData\Local\Programs\Python\Launcher\py.exe`), not the bare command name. |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Hand-parsing `.tmod`'s binary format from scratch | Reusing `scripts/extract_tmod.py` | No tradeoff — it's already correct and already in the repo; this research independently re-derived and confirmed the same format by decompiling tModLoader's own `TmodFile.cs`, so there's no remaining format-uncertainty risk |
| Extracting `CatalystMod.dll` from the local `Mods/` folder | Extracting from the Steam Workshop content cache (`D:\SteamLibrary\steamapps\workshop\content\1281930\2838015851\2026.6\CatalystMod.tmod`) | The local `Mods/` folder copy does not currently exist (see Environment Availability) — the Workshop cache is the only currently-available source. If the user later subscribes/launches and a `Mods/CatalystMod.tmod` appears, either source is equally valid (same file, same hash) |
| Redemption: Patient Zero (richest progression-system footprint) | Redemption: Thorn (chosen) | PZ's summon item is location-gated and spawns a "Holo" intermediate NPC, not PZ directly — poorer fit for this project's established "direct summon-item → boss NPC type" pattern (see Summary's "Important Correction") |
| Redemption: Keeper (runner-up) | Redemption: Thorn (chosen) | Keeper is comparably rich (conditional item drop, dialogue broadcast, Alignment change, standard flag) and also has a simple direct summon item (`WeddingRing`), but its summon item conditionally spawns `Keeper` **or** `KeeperSpirit` depending on `RedeBossDowned.keeperSaved` (a "spared" route), requiring dual-NPC-type registration (same shape as Phase 5's Infernon/InfernoSkull) — slightly more implementation surface than Thorn for equivalent richness. Valid fallback if Thorn proves harder to implement than expected during execution. |

**Installation:** None — no new NuGet packages. Only `build.txt`/`.csproj` additions (see Architecture Patterns) and reuse of the existing `scripts/extract_tmod.py`.

## Architecture Patterns

### Recommended file additions (mirrors Phase 4/5 exactly)

```
Integrations/
├── RedemptionIntegration.cs   # new — mirrors CalamityIntegration.cs shape
└── CatalystIntegration.cs     # new — mirrors SpiritIntegration.cs shape (no reflection needed for either, actually simpler than both precedents)
Libs/
├── Redemption.dll              # gitignored — copy directly from ModReader\Redemption\Redemption.dll (already extracted, current: v0.8.0.4501)
└── CatalystMod.dll             # gitignored — extract via scripts/extract_tmod.py against the Workshop .tmod (see Environment Availability)
```

### Pattern 1: Direct public-static-field write (both bosses — no reflection)

**What:** Both `RedeBossDowned.downedThorn` and `WorldDefeats.downedAstrageldon` are `public static bool` fields on `public class : ModSystem`. Both can be written directly, like a normal cross-assembly field access, no reflection needed — the simplest of the four mod-integration API shapes seen in this project so far (Calamity: wrapper properties; Spirit: `internal` dictionary via reflection; Redemption/CatalystMod: plain public fields).

**Redemption (Thorn) — exact replication of `Thorn.OnKill()`:**
```csharp
// Source: decompiled Redemption.dll (ilspycmd), Redemption.NPCs.Bosses.Thorn.Thorn.OnKill()
[JITWhenModsEnabled("Redemption")]
private static void ApplyThornDowned()
{
    if (!RedeBossDowned.downedThorn)
    {
        string text = Language.GetTextValue("Mods.Redemption.StatusMessage.Progression.ThornDowned");
        if (Main.netMode == NetmodeID.Server)
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), new Color(50, 255, 130));
        else if (Main.netMode == NetmodeID.SinglePlayer)
            Main.NewText(text, new Color(50, 255, 130));

        RedeWorld.Alignment += 2; // public static property; setter calls SetAlignment() -> SyncAlignment() internally
        ChaliceAlignmentUI.BroadcastDialogue(
            NetworkText.FromKey("Mods.Redemption.UI.Chalice.HeartOfThorns2"), 300, 30, 0f, Color.DarkGoldenrod);
    }
    NPC.SetEventFlagCleared(ref RedeBossDowned.downedThorn, -1); // matches this project's established -1 convention
}

[JITWhenModsEnabled("Redemption")]
private static bool IsThornDowned() => RedeBossDowned.downedThorn;
```

**CatalystMod (Astrageldon) — exact replication of `Astrageldon.OnKill()`:**
```csharp
// Source: decompiled CatalystMod.dll (ilspycmd, extracted via scripts/extract_tmod.py),
// CatalystMod.NPCs.Boss.Astrageldon.Astrageldon.OnKill()
[JITWhenModsEnabled("CatalystMod")]
private static void ApplyAstrageldonDowned()
{
    if (!WorldDefeats.downedAstrageldon)
        MetanovaGenerator.Generate(); // WorldGen ore-vein generation side effect (APPLY-03)

    // NOTE: gameEventId is -Type here, NOT -1 like every other boss registered so far in
    // this project -- replicate exactly, do not simplify to -1 (Pitfall 4 discipline).
    NPC.SetEventFlagCleared(ref WorldDefeats.downedAstrageldon, -ModContent.NPCType<Astrageldon>());

    // Deliberately NOT replaying: Main.BestiaryTracker.Kills.RegisterKill()/SetKillCountDirectly()
    // (fired live during the actual in-subworld kill, per Hive Mind SetNewBossJustDowned()
    // precedent -- see Common Pitfalls) or the mid-fight downedAstrageldonPhase1 flag (set
    // live during AI phase-transition, not part of the final-downed reproduction).
}

[JITWhenModsEnabled("CatalystMod")]
private static bool IsAstrageldonDowned() => WorldDefeats.downedAstrageldon;
```

### Pattern 2: Summon-item registration (unchanged from Phase 2/4/5 — direct NPC type mapping)

```csharp
[JITWhenModsEnabled("Redemption")]
private void RegisterThorn()
{
    int itemType = ModContent.ItemType<Redemption.Items.Usable.Summons.HeartOfThorns>();
    int npcType = ModContent.NPCType<Redemption.NPCs.Bosses.Thorn.Thorn>();
    SummonItemRegistry.Register(itemType, npcType);
    // No BossArenaRoutingRegistry.Register<T>() call -- confirmed no Zone*/CheckActive
    // override anywhere in Thorn's ~2900-line decompiled source.
    BossRegistry.Register("redemption:thorn", new BossDefinition(
        NpcTypes: new[] { npcType }, ApplyDowned: ApplyThornDowned, IsDowned: IsThornDowned));
}

[JITWhenModsEnabled("CatalystMod")]
private void RegisterAstrageldon()
{
    int itemType = ModContent.ItemType<CatalystMod.Items.SummonItems.AstralCommunicator>();
    int npcType = ModContent.NPCType<CatalystMod.NPCs.Boss.Astrageldon.Astrageldon>();
    SummonItemRegistry.Register(itemType, npcType);
    // No BossArenaRoutingRegistry.Register<T>() call -- Astrageldon.CheckActive() explicitly
    // returns false (never auto-despawns), and no player.Zone* reference exists in its AI.
    BossRegistry.Register("catalyst:astrageldon", new BossDefinition(
        NpcTypes: new[] { npcType }, ApplyDowned: ApplyAstrageldonDowned, IsDowned: IsAstrageldonDowned));
}
```

Note: `AstralCommunicator`'s *real* `UseItem()` spawns an `AstrageldonSpawner` **projectile** (a multi-second summon-ritual animation), which itself later calls `NPC.NewNPC(..., ModContent.NPCType<Astrageldon>(), ...)` — confirmed by decompiling the projectile too. Our `SummonItemRegistry`/`SUBW-04` pipeline bypasses the item's real `UseItem()` entirely and calls `NPC.SpawnOnPlayer` directly (per Phase 2 D-09), which is a purely cosmetic difference (no ritual animation) — already-precedented, not a new risk.

### Anti-Patterns to Avoid

- **Calling `RedeQuest.SyncData()`/`LabArea.labAccess` writes for Patient Zero-style progression systems if a future phase adds PZ:** these are genuinely separate progression trackers from the boss-downed flag itself; conflating them with the standard `BossDefinition.ApplyDowned` pattern needs its own design, not attempted this phase (PZ is out of scope).
- **Simplifying `Astrageldon`'s `-Type` gameEventId to `-1`:** the source uses `-((ModNPC)this).Type`, not `-1` — replicate exactly per Pitfall 4 discipline (established in Phase 4).
- **Registering Akka/Ukko or Nebuleus/Nebuleus2 using the `OnKill()`-replication pattern:** these four Redemption bosses set their downed flags inside `BossLoot(ref string, ref int)`, not `OnKill()` — a genuine API inconsistency *within* Redemption itself. Not relevant to this phase's chosen boss (Thorn uses plain `OnKill()`), but flagged for whoever registers additional Redemption bosses in a future phase.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Extracting files from a `.tmod` container | A new/different extraction tool or a hand-guessed binary parser | `scripts/extract_tmod.py` (already in this repo) | Confirmed correct byte-for-byte against `Terraria.ModLoader.Core.TmodFile`'s actual decompiled source during this research; no reason to reinvent it |
| Detecting a boss's "biome dependency" | Assuming no Zone-check exists because the boss is not visually a biome-boss | Full-source grep for `Zone`/`CheckActive`/`SpawnModBiomes` per boss, as done here for both Thorn and Astrageldon | Cheap to check exhaustively via decompiled source; Hive Mind (Phase 4) already proved that an undetected Zone-dependency causes a real live despawn bug discovered only in-game |

**Key insight:** Both new mods' downed-flag APIs are simpler than any prior mod integrated in this project (plain public static fields, no reflection) — the actual research risk this phase was **file access** (CatalystMod's `.dll` not being trivially reachable) and **summon-item shape verification** (confirming the chosen boss's summon item does a direct `NPC.SpawnOnPlayer` call, not a multi-stage/location-gated one), not API-shape complexity.

## Common Pitfalls

### Pitfall 1: `.tmod` is not a zip — and CatalystMod.tmod is not where CONTEXT.md assumed
**What goes wrong:** Treating `.tmod` as "a zip-like container" (as CONTEXT.md's canonical_refs paraphrased it) and trying to `unzip`/7-Zip it directly fails silently or with a garbage/corrupt-archive error. Also, assuming CatalystMod is "installed" in the local `Mods/` folder (as CONTEXT.md's canonical_refs assumed) and searching only there finds nothing.
**Why it happens:** tModLoader's `.tmod` format is a custom binary (`"TMOD"` magic + length-prefixed strings + a flat file table + per-file DEFLATE-compressed blobs), confirmed by decompiling `Terraria.ModLoader.Core.TmodFile` directly from the installed `tModLoader.dll`. Separately, Steam Workshop-subscribed mods are cached under `steamapps\workshop\content\1281930\<id>\<tModLoaderVersion>\<ModName>.tmod` and are only copied/synced into the local `Mods\` folder the next time tModLoader itself launches and performs that sync — a mod can be subscribed (and thus have Workshop-cached content available for extraction) without yet appearing in `Mods\`.
**How to avoid:** Use `scripts/extract_tmod.py` (this project's own existing tool) against the Workshop cache path directly. CatalystMod's confirmed location on this machine: `D:\SteamLibrary\steamapps\workshop\content\1281930\2838015851\2026.6\CatalystMod.tmod` (v1.1.8). Redemption's confirmed location: `D:\SteamLibrary\steamapps\workshop\content\1281930\2893332653\2026.6\Redemption.tmod` (v0.8.0.4501, matches the already-extracted `ModReader\Redemption\Redemption.dll`, no re-extraction needed for Redemption).
**Warning signs:** `find`/`ls` on the local `Mods\` folder or `enabled.json` coming up empty for a mod that's supposedly installed — check the Steam library's `libraryfolders.vdf` for additional drives/libraries before concluding a mod isn't available at all.

### Pitfall 2: "Hide code/resources" in `extract.log` is not a technical barrier to the DLL itself
**What goes wrong:** Assuming CatalystMod's `HelloDataminers.txt` + "hidden" `extract.log` markers mean the raw `.dll` bytes are genuinely encrypted/absent from the `.tmod`, and treating extraction as impossible or requiring some bypass.
**Why it happens:** tModReader (third-party) chooses to honor a modder's stated preference and skip producing output for flagged mods — but the compiled `CatalystMod.dll` MUST be a plain, uncompressed-or-deflated file entry inside the `.tmod`'s file table for tModLoader to load and JIT it at all; there is no "hidden" concept inside `TmodFile.cs` itself. It is exactly as extractable as any other file entry, confirmed live (588,288 bytes extracted successfully, decompiled cleanly).
**How to avoid:** Manual `.tmod` extraction (Pitfall 1's fix) sidesteps tModReader's courtesy-flag entirely. No special handling needed once the raw file bytes are pulled out.
**Warning signs:** N/A — this pitfall is purely conceptual; there's no runtime symptom, just a wrong assumption that would have stopped someone before trying.

### Pitfall 3: Not every "rich progression" boss has a simple summon item (Patient Zero)
**What goes wrong:** Picking Patient Zero purely because it touches the most distinct progression-tracking systems (`RedeQuest`, `LabArea`, `downedGGBossFirst`), then discovering during implementation that its summon item (`LabHologramDevice`) is location-gated against a hardcoded main-world structure position (`RedeGen.LabVector`) and spawns an intermediate "Holo" NPC, not PZ itself.
**Why it happens:** Redemption implements PZ as part of a larger "Lab Area" main-world structure/questline, not a standalone altar-summoned boss like Thorn/Keeper. The richness heuristic (from Phase 4/5) implicitly assumed "richness" and "simple summon item" go together, which held for Hive Mind and Infernon but does not hold universally.
**How to avoid:** When applying the D-02 richness heuristic, explicitly verify the summon item's `UseItem()` performs a direct, unconditional `NPC.SpawnOnPlayer`/`NPC.NewNPC` call targeting the SAME NPC type whose `OnKill()` was evaluated for richness — not an intermediate/holographic/location-gated NPC. This research did that check for all 10 Redemption bosses' summon items before finalizing Thorn.
**Warning signs:** A boss's summon item class references multiple different NPC types in its `UseItem()` body, or checks `player.Hitbox.Intersects(...)` against hardcoded rectangles, or is named generically (`*Transmitter`, `*HologramDevice`) rather than boss-specifically.

### Pitfall 4: Don't replay side effects that already happened live during the in-subworld kill
**What goes wrong:** Replaying `Main.BestiaryTracker.Kills.RegisterKill()`/`SetKillCountDirectly()` or the mid-fight `WorldDefeats.downedAstrageldonPhase1` flag inside `ApplyDowned()` at carrier-item-use time, duplicating state that already updated correctly during the real, live subworld kill.
**Why it happens:** Astrageldon's decompiled `OnKill()` contains both "first-application" side effects (the ore-vein generation, gated on `!downedAstrageldon`) and unconditional per-kill bookkeeping (bestiary registration) — only the former needs replication; the latter is exactly the same class of risk as Calamity's `SetNewBossJustDowned()` from Phase 4 (Pitfall 5, player/live-state double-apply).
**How to avoid:** For each side effect found in a decompiled `OnKill()`, ask "does this only run once, gated by the same downed-flag we're reproducing?" — if yes, replicate it; if it's unconditional per-kill flavor (bestiary, sound, dust), it already fired for real during the live kill and should NOT be replayed.
**Warning signs:** A side-effect call is not inside an `if (!downedX)` guard in the source.

## Code Examples

### `.tmod` extraction (reused, already exists in `scripts/extract_tmod.py`)
```bash
# Environment pitfall: use the full python.exe path, not the bare `python`/`python3` command
# (those resolve to a non-functional Windows Store alias stub on this machine).
"C:/Users/chang/AppData/Local/Programs/Python/Python313/python.exe" scripts/extract_tmod.py \
  "D:/SteamLibrary/steamapps/workshop/content/1281930/2838015851/2026.6/CatalystMod.tmod" \
  Libs/_catalyst_extract_tmp
# Then copy just Libs/_catalyst_extract_tmp/CatalystMod.dll to Libs/CatalystMod.dll
```

### `.csproj` Reference additions (mirrors the existing SubworldLibrary/CalamityMod/SpiritMod blocks exactly)
```xml
<Reference Include="Redemption" Condition="Exists('Libs\Redemption.dll')">
    <HintPath>Libs\Redemption.dll</HintPath>
    <Private>false</Private>
</Reference>
<Reference Include="CatalystMod" Condition="Exists('Libs\CatalystMod.dll')">
    <HintPath>Libs\CatalystMod.dll</HintPath>
    <Private>false</Private>
</Reference>
```

### `build.txt` addition
```
weakReferences = CalamityMod@2.2.4, SpiritMod@1.5.0.44, Redemption@0.8.0.4501, CatalystMod@1.1.8
```
(Comma-separated, confirmed working syntax per Phase 5's tooling note — space-separated fails to parse.)

## State of the Art

Not applicable — no prior/current API shift for either mod's downed-tracking system; this is first-time integration, not a migration.

## Open Questions

1. **Should Keeper be swapped in for Thorn if Thorn's dual-branch chat-broadcast proves awkward to test in singleplayer?**
   - What we know: Thorn's `OnKill()` explicitly branches on `Main.netMode == 2` (server) vs. `== 0` (singleplayer); this project is singleplayer-only per REQUIREMENTS.md, so only the `Main.NewText` branch is reachable in practice.
   - What's unclear: Whether replicating the unreachable server branch in `ApplyDowned()` is worth the code (harmless, matches source exactly, but adds a no-op-in-practice branch).
   - Recommendation: Include both branches for source-fidelity (per Pitfall 4/CLAUDE.md's "reproduce actual side effects" discipline) — it's a few lines, no real cost, and future multiplayer work (v2, MP-01) would need it anyway.

2. **Does a freshly-`NPC.SpawnOnPlayer`-spawned Astrageldon correctly initialize `secondPhase = false` and pass through `ApplyDifficultyAndPlayerScaling` the same as a projectile-summoned one?**
   - What we know: `NPC.SpawnOnPlayer`/`NPC.NewNPC` both invoke the standard ModNPC lifecycle (`SetDefaults`, `OnSpawn`, `ApplyDifficultyAndPlayerScaling`) regardless of the spawn trigger — this is standard tModLoader behavior, not something tied to HOW the spawn was invoked.
   - What's unclear: Not empirically verified live for Astrageldon specifically (blocked on CatalystMod not being enabled locally yet — see Environment Availability).
   - Recommendation: Standard live verification checkpoint (spawn Astrageldon in the arena subworld, confirm phase transitions occur normally) should catch this if it's wrong; low risk given the mechanism is identical to every other boss already proven in Phases 3-5.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| `ilspycmd` | Decompiling Redemption.dll/CatalystMod.dll | Yes | 8.2.0.7535 (11.0.0.9375 latest upstream) | Works as-is; upgrade optional, not required |
| Python 3 | Running `scripts/extract_tmod.py` | Yes, but not via bare `python`/`python3` command | 3.13.0 (`C:\Users\chang\AppData\Local\Programs\Python\Python313\python.exe`) | Use the full path or the `py` launcher |
| `Redemption.dll` + `.pdb` | Decompile source for Thorn's `OnKill()` | Yes, already extracted | v0.8.0.4501 (confirmed matches live Workshop cache) | `C:\Users\chang\Documents\My Games\Terraria\tModLoader\ModReader\Redemption\Redemption.dll` |
| `CatalystMod.dll` | Decompile source for Astrageldon's `OnKill()` | Yes, extracted during this research | v1.1.8 | `D:\SteamLibrary\steamapps\workshop\content\1281930\2838015851\2026.6\CatalystMod.tmod` (Workshop cache; manual extraction required, see Pitfall 1) |
| **Redemption mod, installed+enabled in local `Mods\` folder** | Live in-game verification (build/JIT-safety checkpoint, downed-flag-applies checkpoint) | **No** | — | User must re-subscribe/let tModLoader sync the Workshop cache into `Mods\`, then enable via Mod Configuration, before any live checkpoint in this phase's plan can run. Does not block code-level implementation or compilation (compile-time `Libs/Redemption.dll` reference is already available). |
| **CatalystMod mod, installed+enabled in local `Mods\` folder** | Live in-game verification | **No** | — | Same as above. Neither mod appears in `Mods\enabled.json` (only `CalamityModMusic, SubworldLibrary, CheatSheet, SpiritMod, BossChecklist, BossArenaSubWorld, CalamityMod` are currently present/enabled). |

**Missing dependencies with no fallback:**
- None — both missing items (Redemption/CatalystMod not locally installed) have a clear, low-effort fallback (re-subscribe/enable), and don't block compile-time work.

**Missing dependencies with fallback:**
- Redemption and CatalystMod not present in the local `Mods\` folder — blocks only the live in-game verification checkpoints (Success Criteria 1, 2, 3's "load safely when disabled" live check), not code implementation. Flag this explicitly at the start of planning as a Wave 0 / pre-checkpoint action item so the user can resolve it in parallel with implementation waves.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None — tModLoader mod, no automated in-game test harness (matches Phase 1-5/9's established precedent) |
| Config file | none |
| Quick run command | `dotnet build BossArenaSubWorld.csproj` |
| Full suite command | N/A — "full verification" is the live in-game checkpoints below, each requiring Redemption/CatalystMod to actually be installed+enabled first (see Environment Availability) |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| MOD-03 | Thorn registered via `BossDefinition`, compiles against `Libs/Redemption.dll` | build (compile-time type check) | `dotnet build BossArenaSubWorld.csproj` | ❌ Wave 0 (new file: `Integrations/RedemptionIntegration.cs`) |
| MOD-03 / SC1 | Using the carrier item sets `RedeBossDowned.downedThorn` to true in the main world | manual-only, dedicated throwaway world | live in-game: kill Thorn in the subworld, return, use `BossCoreItem`, confirm flag + chat message + Alignment change | ❌ Wave 0 |
| MOD-04 | Astrageldon registered via `BossDefinition`, compiles against `Libs/CatalystMod.dll` | build (compile-time type check) | `dotnet build BossArenaSubWorld.csproj` | ❌ Wave 0 (new file: `Integrations/CatalystIntegration.cs`) |
| MOD-04 / SC2 | Using the carrier item sets `WorldDefeats.downedAstrageldon` to true and runs `MetanovaGenerator.Generate()` in the main world | manual-only, dedicated throwaway world | live in-game: kill Astrageldon in the subworld, return, use `BossCoreItem`, confirm flag + ore-vein generation | ❌ Wave 0 |
| SC3 | Mod loads safely with Redemption disabled, and separately with CatalystMod disabled | manual-only, real checkpoint | disable each mod individually in Mod Configuration, launch, confirm no JITException in client log | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet build BossArenaSubWorld.csproj` (requires `Libs/Redemption.dll` and `Libs/CatalystMod.dll` present locally per Environment Availability)
- **Per wave merge:** same build command
- **Phase gate:** all three live checkpoints (Thorn-downed-applies, Astrageldon-downed-applies, both mod-disabled checkpoints) green before `/gsd:verify-work` — blocked until Redemption/CatalystMod are re-enabled locally (see Environment Availability)

### Wave 0 Gaps
- [ ] `Integrations/RedemptionIntegration.cs` — new file, no automated test beyond the build gate
- [ ] `Integrations/CatalystIntegration.cs` — new file, no automated test beyond the build gate
- [ ] `Libs/Redemption.dll` — copy from `ModReader\Redemption\Redemption.dll` (already extracted, no `.tmod` extraction needed)
- [ ] `Libs/CatalystMod.dll` — extract via `scripts/extract_tmod.py` against `D:\SteamLibrary\steamapps\workshop\content\1281930\2838015851\2026.6\CatalystMod.tmod`
- [ ] `build.txt` — add `weakReferences = ..., Redemption@0.8.0.4501, CatalystMod@1.1.8`
- [ ] `.csproj` — add the two `<Reference Include>` blocks
- [ ] Redemption and CatalystMod re-subscribed/enabled in the live `Mods\` folder before any live checkpoint (blocks live verification only, not compilation — see Environment Availability)

## Sources

### Primary (HIGH confidence — direct decompile of the actual installed/extracted assemblies)
- `Terraria.ModLoader.Core.TmodFile` decompiled from the installed `tModLoader.dll` (`D:\SteamLibrary\steamapps\common\tModLoader\tModLoader.dll`) via `ilspycmd` — confirmed exact `.tmod` binary format (magic, version string, hash, signature, file table, per-file DEFLATE blobs), confirmed no "hidden file" concept exists at the container level
- `Redemption.dll` + `.pdb` (`C:\Users\chang\Documents\My Games\Terraria\tModLoader\ModReader\Redemption\Redemption.dll`, v0.8.0.4501) decompiled via `ilspycmd` — full source read for all 10 boss `OnKill()`/`BossLoot()` methods, `RedeBossDowned`/`RedeQuest`/`LabArea`/`RedeWorld` global classes, and the `HeartOfThorns`/`WeddingRing`/`LabHologramDevice`/`CyberTech`/`OmegaTransmitter` summon items
- `CatalystMod.dll` + `.pdb` (manually extracted from `D:\SteamLibrary\steamapps\workshop\content\1281930\2838015851\2026.6\CatalystMod.tmod`, v1.1.8, via `scripts/extract_tmod.py`) decompiled via `ilspycmd` — full source read for `Astrageldon.OnKill()`/`PreKill()`/`CheckActive()`, `WorldDefeats`, `MetanovaGenerator`, `AstralCommunicator`, `AstrageldonSpawner`
- CatalystMod's own build-properties blob (`Info` file inside its `.tmod`, ASCII-extracted) — confirmed `modReferences = CalamityMod`, version `1.1.8`, `displayName = Catalyst Mod`
- Existing project file `scripts/extract_tmod.py` (built in Phase 4) — cross-validated against the from-scratch decompile of `TmodFile.cs`, confirmed correct
- Existing project files `Systems/BossRegistry.cs`, `Integrations/CalamityIntegration.cs`, `Integrations/SpiritIntegration.cs`, `GlobalNPCs/BossKillGlobalNPC.cs`, `build.txt`, `BossArenaSubWorld.csproj` — confirmed the exact pattern to extend and that zero changes are needed to the boss-agnostic pipeline files

### Secondary (MEDIUM confidence)
- Steam library location (`D:\SteamLibrary`) discovered via `libraryfolders.vdf` — confirmed by finding the actual `.tmod` files there, not just inferred

### Tertiary (LOW confidence)
- None — every finding in this document was directly confirmed via decompiled source or direct filesystem inspection during this research pass, not inferred from training data or documentation

## Metadata

**Confidence breakdown:**
- Standard stack (tooling): HIGH — `extract_tmod.py` cross-validated against `TmodFile.cs`'s actual decompiled source; `ilspycmd` already proven in Phase 4/5/9
- Architecture (both bosses' downed-flag APIs, summon items, Zone-dependency check): HIGH — full source read via decompile, not inferred
- Pitfalls: HIGH — all four pitfalls were discovered empirically during this research session (not hypothesized), including the CONTEXT.md correction on CatalystMod's file location and the D-02 heuristic refinement for Patient Zero

**Research date:** 2026-08-14
**Valid until:** Until Redemption or CatalystMod publish a version update that changes `RedeBossDowned`/`WorldDefeats`/`Thorn`/`Astrageldon`'s field/method names (no fixed expiry — re-verify field names via decompile if the pinned `weakReferences` version is ever bumped)
