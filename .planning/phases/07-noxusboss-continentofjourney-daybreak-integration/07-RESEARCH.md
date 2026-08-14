# Phase 7: ContinentOfJourney/Daybreak (Homeward Journey) Integration - Research

**Researched:** 2026-08-14
**Domain:** tModLoader cross-mod boss-registration (decompiled DLL API discovery, `.tmod` binary extraction, boss-roster risk survey)
**Confidence:** HIGH (mod identity, internal name, downed-progress API, chosen boss's full `OnKill()`/summon-item chain, and biome-routing decision were all confirmed by decompiling the actual Steam Workshop-cached assembly, not inferred from wiki text or training data)

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01 (Mod identity):** "ContinentOfJourney" is **Homeward Journey** by GabeHasWon ("Pan"), Steam Workshop id `2930931197`. Confirmed this session: the mod's internal/folder/namespace name is literally `ContinentOfJourney` (display name "Homeward Journey" is cosmetic only — same class of display-name/internal-name split as Phase 5's SpiritMod, but here the *internal* name matches the phase title's original guess exactly). 15 bosses across three tiers (confirmed via terrariamods.wiki.gg and re-confirmed via decompiled `NPCs/` folder listing this session).
- **D-02 (Daybreak):** Reconfirmed as `gold-meridian/daybreak-mod`, a boss-less library dependency of Wrath of the Gods. No registration target exists under this name — out of this phase's research/registration scope entirely.
- **D-03 (NoxusBoss removed from v1 scope):** NoxusBoss is not researched or registered in this phase, or anywhere in v1. Not re-litigated here.
- **D-04 (Boss selection — Claude's Discretion, tiebreaker = lowest research risk):** Resolved this session via full-roster risk survey (see Summary). **Chosen boss: Goblin Chariot** (`ContinentOfJourney.NPCs.Boss_GoblinChariot.GoblinChariot`), a Pre-Hardmode boss.
- **D-05 (Biome/arena routing — apply wiki-thematic principle from the start):** Resolved this session. Goblin Chariot has **no wiki-stated biome** ("summoned at any time," confirmed both via terrariamods.wiki.gg and via decompiled source — no `Zone*`/`CheckActive` override anywhere in its ~1940-line decompiled source, and its summon item's `CanUseItem()` has no biome/location gate). Per D-05's own instruction ("assign the boss's wiki-stated biome... even without confirmed functional dependency" — conditioned on a biome actually being wiki-stated), no thematic override applies here; Goblin Chariot **falls back to the default `BossArenaSubworld`** with zero new subworld-build work, the same way King Slime does. This is a valid, fully-researched D-05 outcome, not a shortcut — the "Abyss" biome flagged as a possible new-subworld risk in CONTEXT.md/ROADMAP.md belongs to a different boss (Diver, confirmed via its `AbyssPortal.cs` companion file) that was surveyed and NOT selected, precisely to avoid that new-subworld-build cost this phase.

### Claude's Discretion

- Exact boss pick within Homeward Journey's 15-boss roster (D-04) — resolved above: **Goblin Chariot**.
- Whether the chosen boss's biome needs a new subworld built this phase, or reuses an existing Phase 9 one, or needs none at all (D-05) — resolved above: **none needed**, default arena.
- New integration file: `Integrations/HomewardJourneyIntegration.cs` (chosen name — mirrors the "display name" the user/roadmap uses, consistent with how `Integrations/CalamityIntegration.cs`/`SpiritIntegration.cs`/etc. are named after what the mod is *called*, even though the C#-level `ModLoader.HasMod(...)` check must use the internal name `"ContinentOfJourney"`, not `"HomewardJourney"` — see Common Pitfalls).
- Per-boss decompiled-source verification (side effects, player-scoped vs. world-scoped classification, actual downed-flag API shape) — performed this session (see Summary/Architecture Patterns).

### Deferred Ideas (OUT OF SCOPE)

- Registering the remaining 14 Homeward Journey bosses — explicitly out of this phase's scope, same "one worked example" discipline as Phase 6.
- Building a new `BossArenaAbyssSubworld` — not needed this phase, since the selected boss (Goblin Chariot) has no biome dependency. Flagged here only so a future full-roster phase (if one is ever commissioned, analogous to Phase 10) knows this work was surveyed-but-not-built, not overlooked: Diver's `AbyssPortal.cs` companion file confirms at least one other Homeward Journey boss (not registered this phase) does relate to the mod's own "Abyss" biome.
- NoxusBoss — permanently out of v1 scope (D-03), not re-litigated.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MOD-06 | ContinentOfJourney / Daybreak (identified as Homeward Journey) bosses researched (downed-progress API) and registered | Full API confirmed via decompile: `ContinentOfJourney.DownedBossSystem` is a `public class : ModSystem` with `public static bool` fields (e.g. `downedGoblinChariot`) — directly writable, zero reflection, identical shape to Redemption/CatalystMod (Phase 6). Worked-example boss selected: **Goblin Chariot** (see Summary for the 15-boss risk survey that produced this pick). Its `OnKill()` is the simplest of any boss registered in this project so far: one line, `NPC.SetEventFlagCleared(ref DownedBossSystem.downedGoblinChariot, -1)`, no chat/netcode/WorldGen side effects beyond that vanilla helper, no player-scoped bookkeeping anywhere (confirmed via a full-project grep for `downedGoblinChariot` — the only write site in the entire decompiled assembly is `GoblinChariot.OnKill()`). |
</phase_requirements>

## Summary

Homeward Journey's downed-progress API turned out to be **the simplest of all five mods integrated in this project** — `ContinentOfJourney.DownedBossSystem` is a `public class : ModSystem` with plain `public static bool downedX` fields (identical shape to Redemption/CatalystMod from Phase 6: no reflection, unlike Spirit's `internal` dictionary from Phase 5). This session additionally confirmed the mod's own internal name is literally `ContinentOfJourney` (the `Mod`-subclass is `public class ContinentOfJourney : Mod`), meaning `ModLoader.HasMod("ContinentOfJourney")` is correct — the phase title's long-standing "ContinentOfJourney" guess was, in a genuine sense, right all along at the code level, even though the Workshop display name is "Homeward Journey."

**File access:** Homeward Journey is not currently installed/enabled in the local `Mods\` folder (absent from `enabled.json`), but its Steam Workshop content cache is present at `D:\SteamLibrary\steamapps\workshop\content\1281930\2930931197\2026.3\ContinentOfJourney.tmod` (v0.8.70.88, confirmed via the `.tmod`'s own embedded `Info` blob). Extracted this session via this project's existing `scripts/extract_tmod.py` (same tool/approach as Phase 6's CatalystMod), no new extraction tooling needed. `ContinentOfJourney.dll` + `.pdb` are ordinary file entries inside the `.tmod` — no anti-datamining posture like CatalystMod's (Phase 6 D-01) was encountered for this mod.

**Boss selection (D-04) — 15-boss risk survey:** The full roster (`Goblin Chariot`, `Big Dipper`, `Puppet Opera`, `Marquis Moonsquid` — pre-hardmode; `Priestess Rod`, `Diver`, `The Motherbrain` — hardmode; `Wall of Shadow`, `Slime God`, `The Overwatcher`, `The Lifebringer`, `The Materealizer`, `Scarab Belief`, `World's End Everlasting Falling Whale`, `The Son` — post-Moon Lord) was enumerated directly from the decompiled `NPCs/Boss_*` folder structure (matches the 15-boss count from CONTEXT.md's wiki-sourced tier list). Three candidates were fully decompiled and compared:
- **Puppet Opera** — has the cleanest possible `OnKill()` (identical one-line shape to Goblin Chariot) and zero `Main.masterMode` branches anywhere in its AI, but was **disqualified**: its actual summon trigger is not a portable item at all. `Tiles/Theatre/Stair.cs` + `TemplatePlayer.cs`'s `PasswordProgress`/`theatreTimer` state machine reveal the boss is summoned by entering a fixed "Theatre" structure and inputting a directional-key password sequence there — a structure-and-puzzle-gated trigger, exactly the class of thing SUBW-01/D-04 rule out ("not altar-thrown or bulb-break style triggers... no structure/progression-gating").
- **Marquis Moonsquid** — **disqualified**: no summon item exists anywhere in the decompiled `Items/` tree (only a `TemplatePlayer.cs` reference), and its AI directly checks `target.ZoneBeach`/`!val.ZoneBeach` (a genuine biome-AI dependency, the same risk class as Phase 4's Hive Mind `ZoneCorrupt` bug) — worse on both axes (no portable summon item AND a real biome dependency) than either finalist below.
- **Goblin Chariot (chosen)** — a plain, unconditional, single-NPC-type summon item (`PurpleFlareGun`) whose `CanUseItem()` only checks `!NPC.AnyNPCs(...)` (no biome/location/structure gate at all), whose `UseItem()` calls `NPC.SpawnOnPlayer` directly (matches this project's established SUBW-04/D-09 bypass pattern exactly, same as every prior mod), and whose `OnKill()` is the minimal one-line `SetEventFlagCleared` call with **zero** `Main.masterMode` branches, `Zone*` references, or `CheckActive()` override anywhere in its ~1940-line decompiled source. A full-project grep confirms `downedGoblinChariot` is written in exactly one place (`GoblinChariot.OnKill()`) — every other reference is a read (BossChecklist integration, a Fishmen trading-post unlock condition, save/load/net-sync bookkeeping). Wiki (terrariamods.wiki.gg) independently confirms "can be summoned at any time," with no stated biome — so per D-05's own wording, no wiki-thematic arena assignment applies, and Goblin Chariot correctly falls back to the plain default `BossArenaSubworld` with **zero new subworld-build work**, the lowest-possible-risk D-05 outcome.
- **Runner-up (Big Dipper)**: also very clean (`OnKill()` is the same one-line shape; no `Zone*`/`CheckActive` in its AI), and its summon item (`MaliciousPacket`) does state a `player.ZoneSkyHeight` requirement in its own `CanUseItem()`/`UseItem()` — wiki-confirmed ("summoned... in Space, but can be fought in any environment"). Since this project's SUBW-04 pipeline bypasses `CanUseItem()`/`UseItem()` entirely (Phase 2 D-09), that item-level gate is a non-issue either way, and Big Dipper would cleanly route to Phase 9's existing `BossArenaSpaceSubworld` per D-05's wiki-thematic principle. Kept as the documented fallback if Goblin Chariot proves harder to implement than expected during execution — see Alternatives Considered.

**Primary recommendation:** Register Homeward Journey's `Goblin Chariot` (direct public-static-field write via `ContinentOfJourney.DownedBossSystem.downedGoblinChariot`, no reflection, no Zone dependency, no arena routing needed) using the exact same `BossDefinition`/`[JITWhenModsEnabled]`-per-method/named-delegate pattern established in `Integrations/RedemptionIntegration.cs` and `Integrations/CatalystIntegration.cs`. The mod must be re-subscribed/enabled in the local `Mods\` folder before any live in-game verification checkpoint can run (see Environment Availability) — this does not block code-level implementation, since `Libs/ContinentOfJourney.dll` is already extracted and available as a compile-time reference.

## Standard Stack

### Core (tooling, not runtime libraries — this phase adds no new NuGet/library dependencies)

| Tool | Version (confirmed) | Purpose | Why Standard |
|------|---------------------|---------|---------------|
| `ilspycmd` | 8.2.0.7535 (locally installed; 11.0.0.9375 latest upstream — update optional) | Decompile `ContinentOfJourney.dll` | Same tool used in Phase 4/5/6/9; already proven against this exact codebase |
| `scripts/extract_tmod.py` (existing, built in Phase 4) | N/A | Extract `ContinentOfJourney.dll`/`.pdb` from the Workshop-cached `.tmod` | Same tool reused unchanged for the third time (Phase 4 Calamity, Phase 6 CatalystMod, this phase); no new extraction logic needed — Homeward Journey has no anti-datamining posture, extraction was uneventful |
| Python 3.13 (`C:\Users\chang\AppData\Local\Programs\Python\Python313\python.exe`) | 3.13.0 | Runs `extract_tmod.py` | Same environment pitfall as Phase 6: bare `python`/`python3` resolves to a non-functional Windows Store alias stub — use the full path |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Homeward Journey: Goblin Chariot (chosen) | Homeward Journey: Big Dipper (runner-up) | Equally clean `OnKill()`/no-Zone-in-AI, but its item-level `CanUseItem()` states a Space/`ZoneSkyHeight` requirement (harmless under this project's UseItem-bypass pipeline, but would pull in a D-05 routing decision to `BossArenaSpaceSubworld` that Goblin Chariot avoids entirely). Valid fallback if Goblin Chariot proves harder to implement than expected during execution. |
| Homeward Journey: Goblin Chariot (chosen) | Homeward Journey: Puppet Opera | Disqualified, not just "riskier" — its real summon trigger is a fixed-structure ("Theatre") password-sequence puzzle, not a portable item at all; does not satisfy SUBW-01's "simple use-to-summon" constraint no matter how clean its `OnKill()` is |
| Homeward Journey: Goblin Chariot (chosen) | Homeward Journey: Marquis Moonsquid | Disqualified: no summon item exists anywhere in the mod (natural/structure-triggered only per decompiled source), and its AI has a genuine `ZoneBeach` biome dependency — worse on both axes than any other pre-hardmode candidate |
| Extracting `ContinentOfJourney.dll` from the local `Mods/` folder | Extracting from the Steam Workshop content cache (`D:\SteamLibrary\steamapps\workshop\content\1281930\2930931197\2026.3\ContinentOfJourney.tmod`) | The local `Mods/` folder copy does not currently exist (mod not subscribed/enabled locally) — the Workshop cache is the only currently-available source, same situation as Phase 6's CatalystMod |

**Installation:** None — no new NuGet packages. Only `build.txt`/`.csproj` additions (see Architecture Patterns) and reuse of the existing `scripts/extract_tmod.py`.

## Architecture Patterns

### Recommended file additions (mirrors Phase 4/5/6 exactly)

```
Integrations/
└── HomewardJourneyIntegration.cs   # new — mirrors RedemptionIntegration.cs/CatalystIntegration.cs shape
Libs/
└── ContinentOfJourney.dll          # gitignored — already extracted this session via scripts/extract_tmod.py
```

### Pattern 1: Direct public-static-field write (no reflection — same shape as Redemption/CatalystMod, Phase 6)

**What:** `ContinentOfJourney.DownedBossSystem.downedGoblinChariot` is a `public static bool` field on a `public class : ModSystem`. Writable directly, no reflection needed.

```csharp
// Source: decompiled ContinentOfJourney.dll (ilspycmd),
// ContinentOfJourney.NPCs.Boss_GoblinChariot.GoblinChariot.OnKill()
[JITWhenModsEnabled("ContinentOfJourney")]
private static void ApplyGoblinChariotDowned()
{
    // Faithful replay of GoblinChariot.OnKill() -- the entire method body in the
    // real source is exactly this one line. No chat broadcast, no netcode call
    // beyond what SetEventFlagCleared itself does, no WorldGen side effect, no
    // player-scoped bookkeeping anywhere (confirmed via full-project grep for
    // "downedGoblinChariot" -- this OnKill() is the ONLY write site in the entire
    // decompiled assembly; every other reference reads the flag: BossChecklist
    // integration, a Fishmen Free Market trading-post unlock condition, and
    // DownedBossSystem's own save/load/NetSend/NetReceive bookkeeping).
    NPC.SetEventFlagCleared(ref ContinentOfJourney.DownedBossSystem.downedGoblinChariot, -1);
}

[JITWhenModsEnabled("ContinentOfJourney")]
private static bool IsGoblinChariotDowned() => ContinentOfJourney.DownedBossSystem.downedGoblinChariot;
```

### Pattern 2: Summon-item registration (unchanged from Phase 2/4/5/6 — direct NPC type mapping)

```csharp
[JITWhenModsEnabled("ContinentOfJourney")]
private void RegisterGoblinChariot()
{
    int itemType = ModContent.ItemType<ContinentOfJourney.Items.PurpleFlareGun>();
    int npcType = ModContent.NPCType<ContinentOfJourney.NPCs.Boss_GoblinChariot.GoblinChariot>();

    // SummonItemRegistry/BossRegistry are boss-agnostic -- zero changes needed to
    // either existing file. No eligibility delegate needed -- PurpleFlareGun's real
    // CanUseItem() only checks !NPC.AnyNPCs(...) (no biome/location/structure gate,
    // confirmed via decompile), so this project's UseItem()-bypassing pipeline
    // (Phase 2 D-09) loses nothing by skipping it.
    SummonItemRegistry.Register(itemType, npcType);

    // No BossArenaRoutingRegistry.Register<T>() call -- confirmed no Zone*/CheckActive
    // override anywhere in GoblinChariot's ~1940-line decompiled source, and no
    // wiki-stated biome exists to apply D-05's thematic-assignment principle to
    // ("summoned at any time" per terrariamods.wiki.gg). Falls back to the default
    // BossArenaSubworld automatically -- same as vanilla King Slime.
    BossRegistry.Register("continentofjourney:goblin_chariot", new BossDefinition(
        NpcTypes: new[] { npcType },
        ApplyDowned: ApplyGoblinChariotDowned,
        IsDowned: IsGoblinChariotDowned));
}
```

### Anti-Patterns to Avoid

- **Checking `ModLoader.HasMod("HomewardJourney")`:** the mod's actual internal name (folder/dll/namespace/`Mod`-subclass name) is `ContinentOfJourney`, NOT `HomewardJourney` — "Homeward Journey" is only the Workshop display name. Using the display name in `ModLoader.HasMod(...)` or `weakReferences` will silently fail to detect the mod even when it's installed. See Common Pitfalls.
- **Assuming Puppet Opera or Marquis Moonsquid are viable fallbacks:** both were investigated and disqualified this session for structural reasons (structure/password-puzzle gate; no summon item + real biome AI dependency respectively) — see Summary and Alternatives Considered. Do not re-select either without new research; if Goblin Chariot proves problematic during implementation, the researched fallback is **Big Dipper**, not these two.
- **Building a new `BossArenaAbyssSubworld` for this phase:** not needed — Goblin Chariot has no biome dependency. The "Abyss" biome only became relevant during the D-04 survey via Diver's `AbyssPortal.cs` companion file; Diver was not selected, so this work is correctly out of scope this phase (see Deferred Ideas).

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Extracting a mod's `.dll` from a Workshop-cached `.tmod` when it's not in the local `Mods/` folder | A new/different extraction tool | `scripts/extract_tmod.py` (already in this repo, this is its third successful reuse) | Already correct, already proven against two prior mods this project |
| Determining whether a boss has a genuine biome/Zone AI dependency | Assuming from the boss's flavor/name (e.g. "Marquis Moonsquid" sounds ocean-themed, so it must have a beach check — true here, but don't stop at guessing) | Full-source grep for `Zone`/`CheckActive`/`SpawnModBiomes` per candidate boss, exactly as done here for all three finalists | Cheap to check exhaustively via decompiled source; this session's survey caught a real disqualifying dependency (Marquis Moonsquid/`ZoneBeach`) that the wiki summary alone did not surface |
| Determining whether a boss's "summon item" is actually a portable item | Trusting a wiki's "summoned with the X" phrasing at face value | Decompile the boss's actual summon path (search for `NPCType<Boss>()` usage across the whole assembly, not just the `Items/` folder) | Puppet Opera's real summon trigger (a structure + password-sequence puzzle in `TemplatePlayer.cs`) would have been missed by trusting wiki phrasing or only checking the `Items/` folder for an obviously-named summon item |

**Key insight:** The actual research risk this phase was **roster-wide risk triage** (surveying multiple candidates to find the one with zero disqualifying properties), not API-shape complexity — Homeward Journey's downed-flag API (`DownedBossSystem`, plain public static fields) was, once found, the simplest of all five mods integrated in this project. The D-04 tiebreaker ("lowest research risk") only works if research actually decompiles multiple candidates rather than picking the first wiki-plausible one; this session's disqualification of Puppet Opera (despite having the objectively cleanest `OnKill()`) is the clearest evidence that pattern was applied correctly.

## Common Pitfalls

### Pitfall 1: Display name ("Homeward Journey") vs. internal name (`ContinentOfJourney`) mismatch
**What goes wrong:** Using `ModLoader.HasMod("HomewardJourney")` or `weakReferences = HomewardJourney@...` in `build.txt`, based on the mod's Steam Workshop/in-game display name.
**Why it happens:** tModLoader's `ModLoader.HasMod(string)` and `build.txt`'s `weakReferences` both key on the mod's **internal name** (the folder name inside the `.tmod`, matching the `Mod`-subclass's containing namespace), not its display name. This project already hit an analogous mismatch with SpiritMod in Phase 5 (flagged in STATE.md as a precedent to re-check every phase).
**How to avoid:** Confirmed this session by decompiling `ContinentOfJourney.dll`: the `Mod`-subclass is `public class ContinentOfJourney : Mod` in `namespace ContinentOfJourney`, and the extracted `.tmod`'s file/folder naming is consistently `ContinentOfJourney` throughout. Use `"ContinentOfJourney"` for both `ModLoader.HasMod(...)` and `weakReferences = ContinentOfJourney@0.8.70.88`.
**Warning signs:** `ModLoader.HasMod("HomewardJourney")` silently returns `false` even with the mod installed and enabled — no exception, just a no-op registration skip that looks identical to "mod not installed."

### Pitfall 2: Not every wiki-described "summon item" is actually a portable item (Puppet Opera)
**What goes wrong:** Picking a boss because its `OnKill()` looks maximally simple/low-risk, without verifying its actual summon trigger is a genuine "use item to summon" mechanism.
**Why it happens:** Homeward Journey implements at least one boss (Puppet Opera) as a structure-entry + input-sequence puzzle ("Theatre" tile structure + `TemplatePlayer.PasswordProgress`/`theatreTimer` state machine), not an item-use trigger — this is invisible from `OnKill()` alone and easy to miss if research stops at "does this boss have a summon item file in `Items/`."
**How to avoid:** For every risk-survey candidate, grep the *whole* decompiled assembly for `NPCType<CandidateBoss>()` usage (not just the `Items/` folder) and read every call site's surrounding context, not just the first match. This session did that for all three finalists and it's what caught Puppet Opera's real trigger mechanism (in `Tiles/Theatre/Stair.cs` and `TemplatePlayer.cs`, both outside `Items/`).
**Warning signs:** A boss's summon-related code references tile/structure classes (`ModTile`, a dedicated biome/structure folder) or player-input state machines (`PasswordProgress`-style fields) rather than a single `ModItem.UseItem()` override.

### Pitfall 3: A clean `OnKill()` doesn't guarantee a clean AI — check `Zone*`/`CheckActive` separately (Marquis Moonsquid)
**What goes wrong:** Assuming a pre-hardmode boss with a simple-looking name/flavor has no biome dependency, without grepping its AI for `Zone*` references.
**Why it happens:** Marquis Moonsquid's `OnKill()` is exactly as simple as Goblin Chariot's/Big Dipper's (a one-line `SetEventFlagCleared` call) — the biome dependency only shows up in its `AI()` method (`target.ZoneBeach`/`!val.ZoneBeach` checks), a completely separate part of the source that a shallow "check OnKill richness" pass would never see.
**How to avoid:** Grep every finalist candidate's *entire* class file for `Zone`/`CheckActive`, not just its `OnKill()` — exactly the same discipline Phase 4/6 already established for Hive Mind/Thorn/Astrageldon, applied here across multiple candidates instead of just the one eventually chosen.
**Warning signs:** AI code references `player.Zone*`/`target.Zone*` or overrides `CheckActive()` — this is the same despawn-risk signature that caused Phase 4's live Hive Mind bug.

## Code Examples

### `.tmod` extraction (reused, already exists in `scripts/extract_tmod.py`)
```bash
# Environment pitfall: use the full python.exe path, not the bare `python`/`python3` command.
"C:/Users/chang/AppData/Local/Programs/Python/Python313/python.exe" scripts/extract_tmod.py \
  "D:/SteamLibrary/steamapps/workshop/content/1281930/2930931197/2026.3/ContinentOfJourney.tmod" \
  Libs/_hj_extract_tmp
# Then copy Libs/_hj_extract_tmp/ContinentOfJourney.dll (+ .pdb) to Libs/ContinentOfJourney.dll (+ .pdb)
```
(Already performed this session — `Libs/ContinentOfJourney.dll`/`.pdb` are present and ready for the `.csproj` Reference block below. Temp extraction/decompile scratch folders were cleaned up after research, matching the project's convention of keeping `Libs/` to just the `.dll`/`.pdb` per mod.)

### `.csproj` Reference addition (mirrors the existing SubworldLibrary/CalamityMod/SpiritMod/Redemption/CatalystMod blocks exactly)
```xml
<Reference Include="ContinentOfJourney" Condition="Exists('Libs\ContinentOfJourney.dll')">
    <HintPath>Libs\ContinentOfJourney.dll</HintPath>
    <Private>false</Private>
</Reference>
```

### `build.txt` addition
```
weakReferences = CalamityMod@2.2.4, SpiritMod@1.5.0.44, Redemption@0.8.0.4501, CatalystMod@1.1.8, ContinentOfJourney@0.8.70.88
```
(Comma-separated, confirmed working syntax per Phase 5's tooling note.)

## State of the Art

Not applicable — first-time integration for this mod, not a migration. Note for future maintenance: Homeward Journey's own `build.txt`-equivalent (`Info` blob) declares `weakReferences = CalamityMod@2.0.1.5, ThoriumMod@1.7.1.6` — i.e. Homeward Journey itself has an optional soft dependency on Calamity (already a weak reference in this project from Phase 4) and Thorium (not currently a dependency of this project at all, not installed, and not needed — Homeward Journey's own weak reference to it doesn't propagate any requirement onto us).

## Open Questions

1. **Does Goblin Chariot's `PurpleFlareGun.UseAnimation()` (a separate, cosmetic `PurplePuffer` projectile spawn) matter for this project's pipeline?**
   - What we know: `UseAnimation()` and `UseItem()` are separate `ModItem` overrides; this project's SUBW-04/D-09 pipeline calls `NPC.SpawnOnPlayer` directly and bypasses both `UseItem()` and `UseAnimation()` entirely (same as every prior mod's summon item).
   - What's unclear: Nothing functionally — this is purely a cosmetic difference (no flare-gun visual effect plays in the subworld), already precedented exactly by Astrageldon's ritual-projectile bypass in Phase 6.
   - Recommendation: No action needed; document the bypass in a code comment for source-fidelity, consistent with Phase 6's `CatalystIntegration.cs` precedent.

2. **Is Boss Checklist's recognition of `downedGoblinChariot` automatic, given Homeward Journey ships its own `CoJ_BossChecklist.cs` integration?**
   - What we know: `ContinentOfJourney/CoJ_BossChecklist.cs` exists in the decompiled source and registers a `(Func<bool>)(() => DownedBossSystem.downedGoblinChariot)` condition with BossChecklist directly — meaning once our carrier item sets the real flag via the real `SetEventFlagCleared` call, Homeward Journey's own BossChecklist integration should recognize it with no extra work from this project (BossChecklist is already installed/enabled per `Mods\enabled.json`).
   - What's unclear: Not empirically verified live yet (blocked on Homeward Journey not being enabled locally — see Environment Availability).
   - Recommendation: This is directly relevant to Phase 8's VERIFY-03 (external tracker recognition) — flag for that phase's live checkpoint; no special registration-time handling needed here beyond faithfully replaying the flag write.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| `ilspycmd` | Decompiling `ContinentOfJourney.dll` | Yes | 8.2.0.7535 (11.0.0.9375 latest upstream) | Works as-is; upgrade optional |
| Python 3 | Running `scripts/extract_tmod.py` | Yes, but not via bare `python`/`python3` command | 3.13.0 (`C:\Users\chang\AppData\Local\Programs\Python\Python313\python.exe`) | Use the full path or the `py` launcher |
| `ContinentOfJourney.dll` + `.pdb` | Decompile source for Goblin Chariot's `OnKill()`; compile-time `.csproj` reference | Yes, extracted this session | v0.8.70.88 | `D:\SteamLibrary\steamapps\workshop\content\1281930\2930931197\2026.3\ContinentOfJourney.tmod` (Workshop cache; already extracted to `Libs/ContinentOfJourney.dll`) |
| **Homeward Journey (ContinentOfJourney) mod, installed+enabled in local `Mods\` folder** | Live in-game verification (build/JIT-safety checkpoint, downed-flag-applies checkpoint, Open Question 2's BossChecklist recognition check) | **No** | — | User must re-subscribe/let tModLoader sync the Workshop cache into `Mods\`, then enable via Mod Configuration, before any live checkpoint in this phase's plan can run. Does not block code-level implementation or compilation (compile-time `Libs/ContinentOfJourney.dll` reference is already available). Not present in `Mods\enabled.json` (currently: `CalamityModMusic, SubworldLibrary, CheatSheet, SpiritMod, BossChecklist, BossArenaSubWorld, CalamityMod` — same absence pattern as Redemption/CatalystMod were in Phase 6). |

**Missing dependencies with no fallback:**
- None — the one missing item (Homeward Journey not locally installed) has a clear, low-effort fallback (re-subscribe/enable), and doesn't block compile-time work.

**Missing dependencies with fallback:**
- Homeward Journey not present in the local `Mods\` folder — blocks only the live in-game verification checkpoints, not code implementation. Flag this explicitly as a Wave 0 / pre-checkpoint action item, same as Phase 6's precedent.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | None — tModLoader mod, no automated in-game test harness (matches Phase 1-6/9's established precedent) |
| Config file | none |
| Quick run command | `dotnet build BossArenaSubWorld.csproj` |
| Full suite command | N/A — "full verification" is the live in-game checkpoint below, requiring Homeward Journey to actually be installed+enabled first (see Environment Availability) |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| MOD-06 | Goblin Chariot registered via `BossDefinition`, compiles against `Libs/ContinentOfJourney.dll` | build (compile-time type check) | `dotnet build BossArenaSubWorld.csproj` | ❌ Wave 0 (new file: `Integrations/HomewardJourneyIntegration.cs`) |
| MOD-06 / SC1 | Using the carrier item sets `DownedBossSystem.downedGoblinChariot` to true in the main world | manual-only, dedicated throwaway world | live in-game: kill Goblin Chariot in the subworld, return, use `BossCoreItem`, confirm flag + BossChecklist recognition (Open Question 2) | ❌ Wave 0 |
| SC2 | Mod loads safely with Homeward Journey (ContinentOfJourney) disabled | manual-only, real checkpoint | disable the mod in Mod Configuration, launch, confirm no `JITException` in client log | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet build BossArenaSubWorld.csproj` (requires `Libs/ContinentOfJourney.dll` present locally per Environment Availability)
- **Per wave merge:** same build command
- **Phase gate:** both live checkpoints (Goblin-Chariot-downed-applies, mod-disabled checkpoint) green before `/gsd:verify-work` — blocked until Homeward Journey is re-enabled locally (see Environment Availability)

### Wave 0 Gaps
- [ ] `Integrations/HomewardJourneyIntegration.cs` — new file, no automated test beyond the build gate
- [ ] `Libs/ContinentOfJourney.dll` (+ `.pdb`) — already extracted this session via `scripts/extract_tmod.py` against `D:\SteamLibrary\steamapps\workshop\content\1281930\2930931197\2026.3\ContinentOfJourney.tmod`, ready for the `.csproj` reference
- [ ] `build.txt` — add `weakReferences = ..., ContinentOfJourney@0.8.70.88`
- [ ] `.csproj` — add the one new `<Reference Include>` block
- [ ] Homeward Journey re-subscribed/enabled in the live `Mods\` folder before any live checkpoint (blocks live verification only, not compilation — see Environment Availability)

## Sources

### Primary (HIGH confidence — direct decompile of the actual Workshop-cached assembly)
- `ContinentOfJourney.dll` + `.pdb` (extracted this session from `D:\SteamLibrary\steamapps\workshop\content\1281930\2930931197\2026.3\ContinentOfJourney.tmod`, v0.8.70.88, via `scripts/extract_tmod.py`) decompiled via `ilspycmd` — full source read for `ContinentOfJourney.cs` (the `Mod`-subclass, confirming internal name), `DownedBossSystem.cs` (the downed-flag API), and all three risk-survey finalists' `NPCs/Boss_*` classes (`GoblinChariot.cs`, `BigDipper.cs`, `MarquisMoonsquid.cs`, `PuppetOpera.cs` + its `Playwright.cs`/`BoardOfFlesh.cs`/`BoardOfFlower.cs` companions, `Tiles/Theatre/Stair.cs`, `TemplatePlayer.cs`) and their summon items (`Items/PurpleFlareGun.cs`, `Items/MaliciousPacket.cs`)
- Homeward Journey's own embedded `Info` blob (ASCII-extracted from the `.tmod`) — confirmed `version = 0.8.70.88`, `displayName = Homeward Journey`, `weakReferences = CalamityMod@2.0.1.5, ThoriumMod@1.7.1.6`
- This project's own existing `scripts/extract_tmod.py` (built Phase 4, reused Phase 6 and this phase) — third successful reuse, no changes needed
- Existing project files `Systems/BossRegistry.cs`, `Systems/SummonItemRegistry.cs`, `Systems/BossArenaRoutingRegistry.cs`, `Integrations/RedemptionIntegration.cs`, `Integrations/CatalystIntegration.cs`, `build.txt`, `BossArenaSubWorld.csproj` — confirmed the exact pattern to extend and that zero changes are needed to the boss-agnostic pipeline files
- Full-project grep for `downedGoblinChariot` across the entire decompiled assembly — confirmed the flag has exactly one write site (`GoblinChariot.OnKill()`)

### Secondary (MEDIUM confidence)
- `terrariamods.wiki.gg/wiki/Homeward_Journey/Goblin_Chariot` and `.../Big_Dipper` (via WebSearch, page content summarized rather than directly fetched — direct `WebFetch` against `terrariamods.wiki.gg` returned HTTP 404 for both exact URLs guessed from the mod-name pattern; the summarized search-result content was cross-checked against and fully corroborated by the decompiled source, so treated as confirmed) — "can be summoned at any time" (Goblin Chariot, no biome) and "summoned... in Space, but can be fought in any environment" (Big Dipper) both independently match the decompiled `CanUseItem()`/AI findings
- Steam library location (`D:\SteamLibrary`) and the specific Workshop cache path — discovered via direct filesystem search for the known Workshop id `2930931197`, confirmed by finding the actual `.tmod` files there

### Tertiary (LOW confidence)
- None — every load-bearing finding in this document (mod identity, internal name, boss selection, downed-flag API, summon-item shape, biome-routing decision) was directly confirmed via decompiled source or direct filesystem inspection during this research pass, not inferred from training data.

## Metadata

**Confidence breakdown:**
- Standard stack (tooling): HIGH — `extract_tmod.py`/`ilspycmd` already proven twice before this phase, uneventful third use
- Architecture (mod identity, downed-flag API, chosen boss's `OnKill()`/summon-item/Zone-dependency survey across three candidates): HIGH — full source read via decompile for every finalist, not inferred from wiki alone
- Pitfalls: HIGH — all three pitfalls were discovered empirically during this research session's risk survey (the display-name mismatch, Puppet Opera's real trigger mechanism, Marquis Moonsquid's Zone dependency), not hypothesized in advance

**Research date:** 2026-08-14
**Valid until:** Until Homeward Journey publishes a version update that changes `DownedBossSystem`/`GoblinChariot`/`PurpleFlareGun`'s field/class names (no fixed expiry — re-verify field names via decompile if the pinned `weakReferences` version is ever bumped past `0.8.70.88`)
