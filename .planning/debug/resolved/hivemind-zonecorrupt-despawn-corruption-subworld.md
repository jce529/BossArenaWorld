---
status: resolved
trigger: "Investigate issue: hivemind-zonecorrupt-despawn-corruption-subworld -- Hive Mind despawns almost immediately after spawning inside BossArenaSubworld because the arena has no Corruption biome tiles, so player.ZoneCorrupt stays false and HiveMind.AI() caps NPC.timeLeft to ~1 second. Blocks Phase 4's 04-02-PLAN.md live verification checkpoint."
created: 2026-08-13T11:00:00Z
updated: 2026-08-13T12:20:00Z
---

## Current Focus

hypothesis: RESOLVED (both the original ZoneCorrupt despawn bug AND the follow-up "double Sky Ore message" symptom found during live verification). The double-message symptom traced to completion and confirmed to be EXPECTED/architecturally-correct behavior, not a state-corruption bug -- see Evidence/Resolution below. Documentation comments added to Integrations/CalamityIntegration.cs capturing the full mechanism for future maintainers/future boss registrations. No functional code change was needed or made.
test: N/A -- investigation complete, no fix to test (see Resolution). Build re-verified after adding documentation comments: `dotnet build BossArenaSubWorld.csproj` -- PASS (0 warnings, 0 errors).
expecting: N/A.
next_action: None. Session resolved -- user confirmed the fix is correct and complete and explicitly chose to skip the two remaining optional live spot-checks (downedHiveMind persistence across save/quit/relaunch; no third message/conversion on repeat use), both of which follow deterministically from the already-confirmed mechanism (ordinary ModSystem TagCompound persistence once written to the real main .wld; D-01 idempotency guard now correctly reading true). Archived to .planning/debug/resolved/.

## Symptoms

expected: Using a Teratoma on the Test1 portal tile sends the player into a boss arena where Hive Mind auto-spawns and can be fought normally (survives, can be damaged and killed).
actual: Hive Mind auto-spawns via NPC.SpawnOnPlayer, but despawns within ~1 second because the arena has no Corruption biome tiles, so player.ZoneCorrupt is false and HiveMind.AI() caps its NPC.timeLeft to 60.
errors: none (no crash/exception) -- live-gameplay behavioral bug, confirmed by direct decompilation of the installed CalamityMod.dll (prior session), and now further confirmed by decompiling the installed tModLoader.dll's vanilla biome-detection code (this session).
reproduction: In-game -- hold Teratoma, right-click Test1 tile, enter BossArenaSubworld, Hive Mind spawns then immediately begins sinking/falling and disappears.
started: First observed 2026-08-13 during Phase 4 Plan 04-02's live verification checkpoint (Task 1).

## Eliminated

- hypothesis: "Force player.ZoneCorrupt = true every tick via a ModPlayer override (extending the existing Systems/BiomeOverridePlayer.cs infrastructure)."
  evidence: Explicitly presented to and rejected by the user twice already (per task objective, "do not re-litigate"). User-confirmed direction is a real, separate Corruption-biome subworld instead, which is a more faithful reproduction of Hive Mind's actual intended arena and avoids an ever-growing pile of per-boss Zone* overrides in BiomeOverridePlayer for every future biome-gated boss.
  timestamp: 2026-08-13T11:00:00Z (carried in from prior sessions per task objective, not re-tested)

## Evidence

- timestamp: 2026-08-13T11:05:00Z
  checked: Decompiled `Terraria.Player.UpdateBiomes()` from the installed tModLoader.dll (ilspycmd -t Terraria.Player).
  found: |
    Line ~15376: `ZoneCorrupt = Main.SceneMetrics.EnoughTilesForCorruption;` -- confirms ZoneCorrupt is recomputed every tick from `Main.SceneMetrics`, not a sticky/latched flag, matching Systems/BiomeOverridePlayer.cs's own doc comment ("vanilla recomputes Zone* flags every tick").
  implication: Any fix must ensure the player is standing in genuine corruption tiles continuously during the fight (a real biome), not just at spawn -- a real tile-based biome (rather than a one-shot flag poke) is the only approach that survives this per-tick recompute without needing an every-tick ModPlayer override (which was already rejected).

- timestamp: 2026-08-13T11:08:00Z
  checked: Decompiled `Terraria.SceneMetrics` (ilspycmd -t Terraria.SceneMetrics).
  found: |
    `public static int CorruptionTileThreshold = 300;` and `public bool EnoughTilesForCorruption => EvilTileCount >= CorruptionTileThreshold;`. `EvilTileCount` is populated via `ExportTileCountsToMain()` -> `TileLoader.RecountTiles(this)`.
  implication: Need >= 300 "evil" tiles within the scan area for ZoneCorrupt to flip true. Traced further into TileLoader.RecountTiles() and TileID.Sets.CorruptBiome next to find exactly which tile IDs count and their weights.

- timestamp: 2026-08-13T11:10:00Z
  checked: Decompiled `Terraria.ModLoader.TileLoader.RecountTiles(SceneMetrics)` (ilspycmd -t Terraria.ModLoader.TileLoader).
  found: |
    `metrics.EvilTileCount += num14 * TileID.Sets.CorruptBiome[i]` where `num14` is the raw placed-tile count for tile type `i` (from a per-tick scan, see next entry) and `TileID.Sets.CorruptBiome[i]` is a per-tile-type integer weight.
  implication: EvilTileCount is a weighted sum over all currently-scanned tiles of type `i`, weight given by `TileID.Sets.CorruptBiome[i]`. Needed the actual weight table next.

- timestamp: 2026-08-13T11:12:00Z
  checked: Decompiled `Terraria.ID.TileID.Sets.CorruptBiome` and cross-referenced tile ID constants in `Terraria.ID.TileID` (ilspycmd -t Terraria.ID.TileID.Sets and -t Terraria.ID.TileID).
  found: |
    `public static int[] CorruptBiome = Factory.CreateIntSet(0, 23, 1, 661, 1, 24, 1, 25, 1, 32, 1, 112, 1, 163, 1, 400, 1, 398, 1, 27, -10);`
    Resolved tile IDs: 23 = CorruptGrass, 24 = CorruptPlants, 25 = Ebonstone, 32 = CorruptThorns, 112 = Ebonsand, 163 = CorruptIce, 398 = CorruptHardenedSand, 400 = CorruptSandstone, 661 = CorruptJungleGrass -- ALL weight 1. Tile 27 = Sunflower has weight -10 (actively pushes the count down; must not be placed in the new arena).
  implication: Simplest reliable fix is a solid mass of Ebonstone (weight 1, easiest to mass-place via a GenPass, matches existing FlatStonePlatformPass style) topped with a thin CorruptGrass surface row. Every tile placed contributes weight 1 toward the 300 threshold, so any ~300+-tile placement works; building generously larger gives a comfortable safety margin.

- timestamp: 2026-08-13T11:14:00Z
  checked: Decompiled the tile-scan loop in `Terraria.SceneMetrics.ScanAndExportToMain()` and `Terraria.Main`'s static field initializers for `buffScanAreaWidth`/`buffScanAreaHeight`.
  found: |
    The scan rectangle is centered on `settings.BiomeScanCenterPositionInWorld` (the player, called every tick) with size `Main.buffScanAreaWidth x Main.buffScanAreaHeight`, where `buffScanAreaWidth = (maxScreenW + 800) / 16 - 1` and `buffScanAreaHeight = (maxScreenH + 800) / 16 - 1` -- roughly 200+ tiles wide by 140+ tall (sized off max supported resolution, not current window size), independent of ShouldSave/NoPlayerSaving.
  implication: A platform-wide (not just spawn-point-local) Corruption-tile fill is needed so ZoneCorrupt stays true no matter where the player/boss drifts during the fight (Hive Mind moves erratically). Filling the ENTIRE arena platform width (matching FlatStonePlatformPass's existing full-width fill pattern) with Ebonstone + a CorruptGrass surface row guarantees the scan window is always deep inside solid corruption tiles, with a wide safety margin over the 300-tile threshold (a single ~20-column x 15-row slice alone already totals 300).

- timestamp: 2026-08-13T11:20:00Z
  checked: Decompiled `SubworldLibrary.SubworldSystem` (installed Libs/SubworldLibrary.dll) public API: `IsActive<T>()`, `AnyActive()`, `AnyActive(Mod)`, `Enter(string)`, `Enter<T>()`, `Current` property.
  found: |
    `IsActive<T>()` does exact type equality (`current.GetType() == typeof(T)`), not a subclass/interface check. `Current` is a public static `Subworld` property. `AnyActive()` (no type param) returns true whenever ANY subworld (from ANY mod) is active -- too broad to use directly for "is this one of OUR boss arenas" once a second arena subworld type exists.
  implication: Boss-aware "is any of THIS MOD's arena subworlds active" needs either an explicit per-type OR-chain or a small dynamic registry of known arena `Type`s checked against `SubworldSystem.Current?.GetType()`. Chose the registry approach (`BossArenaRoutingRegistry.IsAnyArenaActive()`) since it's boss-agnostic and extends cleanly to a third arena subworld later without touching call sites in `BossSummonPlayer`/`BossCoreDropRule` again.

- timestamp: 2026-08-13T11:25:00Z
  checked: Read `Items/BossCoreItem.cs`, `GlobalNPCs/BossKillGlobalNPC.cs`, and `ItemDropRules/BossCoreDropRule.cs` in full to trace the entire carrier-item pipeline, not just the spawn/despawn path.
  found: |
    `BossCoreDropRule.CanDrop(DropAttemptInfo info) => SubworldSystem.IsActive<BossArenaSubworld>();` -- hardcoded to the PLAIN arena type only. If Hive Mind were killed inside a new, different subworld type, this check would return false and the BossCoreItem carrier item would NEVER drop, silently breaking Phase 4's checkpoint step 5 ("Confirm a BossCoreItem drops") even after the despawn bug itself is fixed.
  implication: This is a second, previously-unidentified bug on the same code path that MUST be fixed alongside the despawn issue, or the fix would be incomplete (Hive Mind would survive and be killable, but never actually reward the carrier item). Added to this session's fix scope.

---

**SESSION CONTINUATION (2026-08-13, ~12:00-12:10): follow-up "double Sky Ore message" symptom found during live checkpoint verification.**

User confirmed checklist items 1-5 (arena loads, Hive Mind survives/killable, BossCoreItem drops, King Slime unaffected) ALL PASSED. But live testing surfaced a NEW symptom: the chat log showed "The sky is glittering with cyan light." TWICE -- once immediately after "The Hive Mind has been defeated!" (inside the subworld) and once again after "Boss credential applied: calamity:hive_mind" (after using BossCoreItem in the main world). User's own diagnosis: the WorldGen effect applied once (harmlessly) in the subworld, then applied again for real in the main world. Orchestrator's working hypothesis: same category as isolation-premise-flag-persistence.md (SubworldLibrary's exit-reload wiping in-memory modded flag state), causing CalamityMod.DownedBossSystem.downedHiveMind to read false again by the time BossCoreItem was used, allowing ApplyHiveMindDowned()'s guard to pass a second time. This session independently traced and confirmed the full mechanism via decompilation (not just trusted the hypothesis).

- timestamp: 2026-08-13T12:02:00Z
  checked: Decompiled `CalamityMod.NPCs.HiveMind.HiveMind.OnKill()` from `Mods/2026.6CalamityMod.tmod` (re-extracted via scripts/extract_tmod.py to scratchpad, ilspycmd -t).
  found: |
    ```
    public override void OnKill() {
      if (!BossRushEvent.BossRushActive) {
        CalamityGlobalNPC.SetNewBossJustDowned(NPC);
        if (!DownedBossSystem.downedHiveMind && !DownedBossSystem.downedPerforator) {
          AerialiteOreGen.Enchant();
          CalamityUtils.BroadcastLocalizedText("Mods.CalamityMod.Status.Progression.SkyOreText", Color.Cyan);
        }
        DownedBossSystem.downedHiveMind = true;
        CalamityNetcode.SyncWorld();
      }
    }
    ```
    Confirms: this genuinely, unconditionally fires on every real kill (BossRushActive is always false for us) -- exactly matching what CalamityIntegration.cs's ApplyHiveMindDowned() was written to replay. tModLoader has no concept of "this kill happened inside a subworld" -- OnKill() cannot distinguish subworld from main world.
  implication: Confirms part 1 of the orchestrator's hypothesis exactly. The in-subworld message is a REAL, unavoidable side effect of a real NPC kill event, not a bug in this project's own code.

- timestamp: 2026-08-13T12:04:00Z
  checked: Decompiled `CalamityMod.DownedBossSystem` (same source). Searched for `downedHiveMind` property, `SaveWorldData`/`LoadWorldData` overrides, and any `OnWorldLoad`/`ResetAllFlags` call site.
  found: |
    `downedHiveMind` is a plain property backed by `_downedHiveMind` (set via `NPC.SetEventFlagCleared` when true). `DownedBossSystem : ModSystem` overrides ONLY `SaveWorldData(TagCompound)` (writes `"hiveMind"` into a string list if `downedHiveMind` true) and `LoadWorldData(TagCompound)` (sets `downedHiveMind = list.Contains("hiveMind")`) -- no `OnWorldLoad`/`OnWorldUnload` override, no special subworld-awareness, no cross-world marker of any kind. `ResetAllFlags()` exists but is never called within this class (used elsewhere, e.g. a debug/seed-reset command, not relevant here).
  implication: `downedHiveMind` persists via ordinary tModLoader `ModSystem` TagCompound world-data serialization only -- it is NOT part of SubworldLibrary's special vanilla-flag whitelist (confirmed separately below), so its fate on a subworld round-trip depends entirely on tModLoader's normal save/load cycle, which in turn depends on which world file is "active" at each `SaveWorldData`/`LoadWorldData` call.

- timestamp: 2026-08-13T12:06:00Z
  checked: Decompiled `SubworldLibrary.SubworldSystem` from the installed `Libs/SubworldLibrary.dll` (ilspycmd -t), specifically `ExitWorldCallBack()`, `BeginEntering()`, `LoadWorld()`, and `LoadSubworld()` in full (not just grep hits).
  found: |
    `Exit()` -> `BeginEntering(current.ReturnDestination)` (ReturnDestination=-1) -> for index<0: `current = null` is set SYNCHRONOUSLY inside `BeginEntering`, BEFORE the async `ExitWorldCallBack` task even starts. `cache` (still the subworld instance, set earlier via the `OnEnterWorld` hook) is what `ExitWorldCallBack` actually operates on.
    Inside `ExitWorldCallBack` (netMode==0 singleplayer path): `cache.CopySubworldData(); cache.OnExit();` runs first (our `BossArenaCorruptionSubworld.OnExit()` vanilla-flag restore fires here) -> `CopyMainWorldData()` -> ... -> **`if (netMode != 1) { WorldFile.SaveWorld(); }`** (unconditional, NOT gated by `cache.ShouldSave`) -> `SystemLoader.OnWorldUnload()` -> **`LoadWorld()`**.
    Critically, at the moment of that `WorldFile.SaveWorld()` call, `current == null` already (main-world target) but `Main.ActiveWorldFileData` has NOT yet been reassigned back to `main` -- that only happens later, inside `LoadWorld()`.
    `LoadSubworld(path, cloud)` (the generation path used for `ShouldSave=false` subworlds, confirmed via the earlier resolved session's excerpt) contains: `Main.ActiveWorldFileData = new WorldFileData(path, cloud) { ... }` -- i.e., when the player originally ENTERED the arena, `Main.ActiveWorldFileData` was repointed to a BRAND NEW `WorldFileData` object whose `path` is the arena's own distinct (fake, throwaway) file path, not main's. This assignment is never reverted until the exit flow's `LoadWorld()` call runs `if (!flag) { Main.ActiveWorldFileData = main; }` (where `flag = current != null` is false when returning to main).
  implication: |
    `Main.ActiveWorldFileData` (and therefore the SUBWORLD's own path) is still active at the moment of the exit-flow's early `WorldFile.SaveWorld()` call (line ~1186 of the decompiled source) -- this call fires WHILE STILL "IN" THE SUBWORLD'S CONTEXT, one step before the real main world reloads. Need one more check: confirm `Main.worldPathName` (the actual disk-write target per the prior session's WorldFile.cs.patch finding) is actually DERIVED FROM `Main.ActiveWorldFileData.Path` (not a separately-tracked field, which was the prior session's unverified assumption) -- this is the missing link needed to fully explain both (a) why this session's Hive Mind flag resets, AND (b) retroactively resolve the prior session's own open question of why the King Slime test's terrain was NOT overwritten despite `WorldFile.SaveWorld()` firing while subworld content was still loaded.

- timestamp: 2026-08-13T12:08:00Z
  checked: Decompiled `Terraria.Main` from the installed `tModLoader.dll` (ilspycmd -t Terraria.Main), searched for `worldPathName`.
  found: |
    `public static string worldPathName => ActiveWorldFileData.Path;` -- `worldPathName` is a COMPUTED PROPERTY, not an independently-tracked field. It always equals whatever `Main.ActiveWorldFileData.Path` currently is. (This corrects the prior resolved session's isolation-premise-flag-persistence.md, which treated `worldPathName` as a separate static field that "SubworldLibrary never repoints" -- that session only grepped SubworldSystem.cs for direct writes to `worldPathName` and never checked whether it was a property derived from a field SubworldLibrary DOES write, i.e. `ActiveWorldFileData`. Not filed as a new Eliminated hypothesis since the prior session's ultimate root-cause conclusion for the VANILLA-flag bug -- the `CopyDowned()`/`ReadCopiedDowned()` mechanism -- remains independently correct and unaffected by this correction; this is an addendum to that session's incomplete "why didn't SaveWorld() overwrite terrain" side-question, not a retraction of its main finding.)
  implication: |
    Full mechanism now confirmed end-to-end: while inside the Corruption arena, `Main.ActiveWorldFileData`/`worldPathName` point at the ARENA's own throwaway file path (set by `LoadSubworld()` on entry). The exit flow's early `WorldFile.SaveWorld()` call (fires before `LoadWorld()` repoints `ActiveWorldFileData` back to `main`) therefore serializes `DownedBossSystem.SaveWorldData()` -- capturing `downedHiveMind = true` from the real in-subworld kill -- into the ARENA's own discarded file, never into the real main `.wld`. This is ALSO why the prior King Slime session found the main test world's terrain untouched: that `SaveWorld()` call was never writing to main's path in the first place. Then `LoadWorld()` repoints `ActiveWorldFileData` back to `main` and `TryLoadWorldFile()` loads the REAL main `.wld` from disk, which correctly still has `downedHiveMind = false` (never written there) -- `DownedBossSystem.LoadWorldData()` sets the in-memory static back to `false`. By the time `BossCoreItem.UseItem()` -> `BossRegistry.Apply("calamity:hive_mind")` runs, `IsDowned()` (== `downedHiveMind`) correctly reads `false`, so `ApplyHiveMindDowned()` runs for real -- exactly once, against the REAL main world's tiles/flag/save file. This is the SAME general category as Pitfall 1 (world-scoped modded flags do not survive the subworld round-trip) working EXACTLY as PROJECT.md's carrier-item architecture assumes it will -- not a new bug.

- timestamp: 2026-08-13T12:09:00Z
  checked: Decompiled `CalamityMod.World.AerialiteOreGen.Enchant()` (same source as HiveMind.OnKill above).
  found: |
    `Enchant()` has no cross-world state or persistent marker -- it purely scans whatever `Main.tile` array is CURRENTLY loaded, converting any tile of type `AerialiteOreDisenchanted` to `AerialiteOre`. `BossArenaCorruptionSubworld`'s platform (`CorruptionPlatformPass`: Ebonstone + CorruptGrass only) never places any `AerialiteOreDisenchanted` tiles, so the in-subworld `Enchant()` call converts ZERO tiles -- it is a genuine no-op on tiles, not even a wasted/discarded mutation. Only the broadcast message (gated purely by the `downedHiveMind`/`downedPerforator` flag check, independent of whether any tiles were actually found) is what the player sees twice.
  implication: The "double WorldGen mutation" the user suspected does not actually happen -- only the message is duplicated. The real, single, correct Aerialite conversion happens exactly once, at BossCoreItem-use time, against the real main world's tiles. Confirms the pipeline's actual state is already fully correct.

- timestamp: 2026-08-13T12:09:30Z
  checked: Decompiled `Terraria.ModLoader.NPCLoader.OnKill(NPC)` (installed tModLoader.dll) to determine whether this mod could pre-empt/suppress Calamity's native OnKill call via a GlobalNPC hook.
  found: |
    `public static void OnKill(NPC npc) { npc.ModNPC?.OnKill(); /* then */ HookOnKill.Enumerate(...).OnKill(npc); }` -- `ModNPC.OnKill()` (Calamity's own `HiveMind.OnKill()`) ALWAYS runs before any `GlobalNPC.OnKill()` hook, unconditionally, for every kill. There is no tModLoader-exposed hook point that fires before a boss's own `ModNPC.OnKill()`.
  implication: Confirms there is NO clean way (without IL-hooking/patching CalamityMod's compiled DLL, which is out of scope and explicitly to be avoided per the checkpoint's own framing) to suppress or pre-empt the in-subworld "Sky is glittering" broadcast. This is an unavoidable, cosmetic-only artifact of a real (if throwaway) NPC kill event, not a fixable code defect in this project.

## Resolution

root_cause: |
  CONFIRMED (both by prior decompilation of CalamityMod.dll per task objective, and by this session's decompilation of tModLoader.dll's own vanilla biome-detection code):
  `CalamityMod.NPCs.HiveMind.HiveMind.AI()` re-caps `NPC.timeLeft` to ~60 ticks (~1 second) whenever its target-validity check fails, and one branch of that check is `!player.ZoneCorrupt && !BossRushEvent.BossRushActive`. `Player.UpdateBiomes()` recomputes `ZoneCorrupt` every single tick from `Main.SceneMetrics.EnoughTilesForCorruption`, which requires a weighted count of nearby "evil" tiles (Ebonstone, CorruptGrass, CorruptPlants, CorruptThorns, Ebonsand, CorruptIce, CorruptHardenedSand, CorruptSandstone, CorruptJungleGrass -- each weight 1; Sunflower is weight -10) within a ~200x140-tile scan rectangle centered on the player to reach >= 300 (`SceneMetrics.CorruptionTileThreshold`). `BossArenaSubworld`'s platform is plain Stone with zero corruption tiles, so this count is always 0, `ZoneCorrupt` is always false, `BossRushEvent.BossRushActive` is also always false, and Hive Mind's despawn timer is re-capped every tick from the moment it spawns.

  Additionally (found during this session, not in the original task objective): `ItemDropRules/BossCoreDropRule.cs`'s `CanDrop` is hardcoded to `SubworldSystem.IsActive<BossArenaSubworld>()`, so even after fixing the despawn, killing Hive Mind in any OTHER subworld type would silently fail to drop the BossCoreItem carrier item.
fix: |
  1. New `Subworlds/CorruptionPlatformPass.cs`: a GenPass filling the entire arena platform width with `TileID.Ebonstone` (surface row `TileID.CorruptGrass`), same shape/thickness (15 tiles) as the existing `FlatStonePlatformPass`. Every placed tile has CorruptBiome weight 1, so the full-width platform vastly exceeds the 300-tile ZoneCorrupt threshold no matter where the player stands, and stays true continuously (satisfies the per-tick recompute, unlike a one-shot flag poke).
  2. New `Subworlds/BossArenaCorruptionSubworld.cs`: a second `Subworld` using `CorruptionPlatformPass`, `ShouldSave = false`, `NoPlayerSaving = false`, and a VERBATIM DUPLICATE (not inherited -- see file comment) of `BossArenaSubworld.cs`'s `OnEnter`/`OnExit` vanilla-downed-flag snapshot/restore guard (per `.planning/debug/resolved/isolation-premise-flag-persistence.md`, this guard is required for EVERY `Subworld` subclass in this mod independently, not just the original one). `BossArenaSubworld.cs` itself is left completely unmodified, per this task's explicit constraint.
  3. New `Systems/BossArenaRoutingRegistry.cs`: boss-agnostic registry mapping a boss NPC type to `SubworldSystem.Enter<T>()` for its required arena subworld (`Register<T>(bossNpcType)`), defaulting to `BossArenaSubworld` for any boss with no explicit registration (so King Slime's Phase 3 path is unaffected). Also exposes `IsAnyArenaActive()`, checking `SubworldSystem.Current?.GetType()` against the set of all registered arena types (including the default), for boss-agnostic "are we in one of our own arenas" checks.
  4. `Tiles/Test1Tile.cs`: `SubworldSystem.Enter<BossArenaSubworld>()` replaced with `BossArenaRoutingRegistry.Enter(bossNpcType)`.
  5. `Systems/BossSummonPlayer.cs`: `OnEnterWorld()`'s guard changed from `SubworldSystem.IsActive<BossArenaSubworld>()` to `BossArenaRoutingRegistry.IsAnyArenaActive()`.
  6. `ItemDropRules/BossCoreDropRule.cs`: `CanDrop` changed from `SubworldSystem.IsActive<BossArenaSubworld>()` to `BossArenaRoutingRegistry.IsAnyArenaActive()` (fixes the second bug found during evidence-gathering).
  7. `Integrations/CalamityIntegration.cs`: `RegisterHiveMind()` adds `BossArenaRoutingRegistry.Register<BossArenaCorruptionSubworld>(npcType);`.
verification: |
  Build verification: `dotnet build BossArenaSubWorld.csproj` -- PASS (0 warnings, 0 errors).
  Live verification (original despawn/drop bugs): CONFIRMED by user 2026-08-13 -- checklist items 1-5 all passed (arena loads, Hive Mind survives/killable, BossCoreItem drops, King Slime unaffected).

---

**Follow-up finding (2026-08-13, session continuation) -- "double Sky Ore message" symptom:**

root_cause_followup: |
  NOT A BUG. Fully traced via decompilation (CalamityMod.dll, SubworldLibrary.dll, tModLoader.dll) to be EXPECTED, architecturally-correct behavior matching this project's own carrier-item design (PITFALLS.md Pitfall 1):
  1. `CalamityMod.NPCs.HiveMind.HiveMind.OnKill()` genuinely, unconditionally fires when Hive Mind dies for real inside the subworld (tModLoader has no "this is a subworld kill" concept) -- it calls `AerialiteOreGen.Enchant()` + broadcasts "Sky is glittering" + sets `DownedBossSystem.downedHiveMind = true` in-memory, exactly matching what `CalamityIntegration.ApplyHiveMindDowned()` was written to replay.
  2. `Enchant()` operates purely on whatever `Main.tile` array is currently loaded and has no cross-world marker -- inside the arena (Ebonstone/CorruptGrass platform, no `AerialiteOreDisenchanted` tiles ever placed) it converts ZERO tiles; it's a genuine no-op mutation, only the message fires.
  3. `DownedBossSystem.downedHiveMind` is NOT part of SubworldLibrary's hardcoded vanilla `NPC`/`DD2Event` flag whitelist (`CopyDowned()`/`ReadCopiedDowned()`, see isolation-premise-flag-persistence.md) -- it persists purely via ordinary `ModSystem.SaveWorldData`/`LoadWorldData`. Decompiling `SubworldSystem.LoadSubworld()` confirms `Main.ActiveWorldFileData` (and the computed property `Main.worldPathName`) is repointed to the ARENA's own throwaway file path for the entire subworld visit; the exit flow's `WorldFile.SaveWorld()` call fires BEFORE `Main.ActiveWorldFileData` is repointed back to `main`, so the in-subworld `downedHiveMind = true` gets serialized only into the arena's own discarded file, never into the real main `.wld`. The subsequent real main-world reload then correctly restores `downedHiveMind = false` from the untouched real save file.
  4. Because of (3), `BossRegistry.Apply()`'s D-01 idempotency check (`IsDowned()` == `downedHiveMind`) correctly reads `false` in the main world, so `ApplyHiveMindDowned()` runs for REAL, exactly once, against the real main world's tiles/flag/save file when `BossCoreItem` is used -- exactly the intended "kill in throwaway subworld, apply for real via carrier item" architecture.
  5. Decompiling `Terraria.ModLoader.NPCLoader.OnKill()` confirms `npc.ModNPC?.OnKill()` (Calamity's own `HiveMind.OnKill()`) always fires before any `GlobalNPC.OnKill()` hook this mod could use to pre-empt/suppress the in-subworld broadcast -- there is no clean, non-invasive way to prevent that first message without IL-hooking/patching CalamityMod's own compiled code, which was explicitly out of scope.
  Net effect: the persistent, real main-world state (Aerialite ore conversion, Sky Ore broadcast, `downedHiveMind` flag, `SyncWorld()`) is applied correctly exactly once. The only user-visible artifact is a premature, harmless "Sky is glittering" chat message while still inside the throwaway arena -- a known, accepted, documented cosmetic-only UX limitation, not a state-corruption bug. Also corrects/extends (not retracts) an open side-question in the prior isolation-premise-flag-persistence.md session: `Main.worldPathName` is a computed property (`=> ActiveWorldFileData.Path`), not an independent field as that session assumed -- this fully explains why that session's King Slime terrain was never overwritten (the early exit-flow `SaveWorld()` call was writing to the subworld's own path the whole time, never main's).
fix_followup: |
  No functional code change required or made -- the pipeline already produces correct final state. Added an extensive documentation comment block to `Integrations/CalamityIntegration.cs` (above `ApplyHiveMindDowned()`) capturing this full mechanism, so future per-boss registrations with similar WorldGen side effects don't re-investigate this from scratch and don't mistake the "double message" pattern for a bug.
verification_followup: |
  Build verification: `dotnet build BossArenaSubWorld.csproj` -- PASS (0 warnings, 0 errors) after adding documentation comments.
  Mechanism verified via direct decompilation of all three relevant assemblies (CalamityMod.dll, SubworldLibrary.dll, tModLoader.dll) rather than inferred from the orchestrator's hypothesis alone -- confirms each link in the causal chain independently (see Evidence entries 2026-08-13T12:02:00Z through 12:09:30Z).
  Live verification: user already confirmed the SECOND (real, main-world) broadcast correctly converted real Aerialite ore. Outstanding optional spot-checks for the checkpoint: (a) confirm `downedHiveMind` stays `true` after a normal save-and-quit/relaunch of the main world (should behave identically to any other normal boss kill from this point on, no subworld involved anymore); (b) confirm no THIRD message/conversion occurs if the BossCoreItem mechanism is exercised again or the arena is re-entered (idempotency via `IsDowned()` now correctly reading `true` from the real main save).
files_changed:
  - "Subworlds/CorruptionPlatformPass.cs"
  - "Subworlds/BossArenaCorruptionSubworld.cs"
  - "Systems/BossArenaRoutingRegistry.cs"
  - "Tiles/Test1Tile.cs"
  - "Systems/BossSummonPlayer.cs"
  - "ItemDropRules/BossCoreDropRule.cs"
  - "Integrations/CalamityIntegration.cs" (original fix + this session's documentation-only addendum)
